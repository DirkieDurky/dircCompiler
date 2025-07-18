using DircCompiler.CodeGen;
using DircCompiler.Parsing;

namespace DircCompiler.Semantic;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, string> _variables = new(); // name -> type
    private readonly Dictionary<string, FunctionSignature> _functions = new(); // name -> signature
    private static readonly HashSet<string> ValidTypes = new() { "int", "bool" };
    private static readonly HashSet<string> ValidReturnTypes = new() { "int", "bool", "void" };

    public void Analyze(List<AstNode> nodes, CompilerOptions options, CompilerContext context)
    {
        // First pass: collect function signatures
        // Standard library
        foreach ((string name, StandardFunction funcInfo) in StandardLibrary.Functions)
        {
            _functions.Add(name, funcInfo.Signature);
        }

        // Custom functions
        foreach (AstNode node in nodes)
        {
            if (node is FunctionDeclarationNode func)
            {
                // Check return type (allow 'void' only for return type)
                if (!ValidReturnTypes.Contains(func.ReturnTypeToken.Lexeme))
                {
                    throw new SemanticException($"Unknown return type '{func.ReturnTypeToken.Lexeme}' in function '{func.Name}'", func.ReturnTypeToken, options, context);
                }
                // Check parameter types (do not allow 'void')
                foreach (var param in func.Parameters)
                {
                    if (!ValidTypes.Contains(param.TypeName))
                    {
                        throw new SemanticException($"Unknown parameter type '{param.TypeName}' in function '{func.Name}'", null, options, context);
                    }
                }
                FunctionSignature signature = new FunctionSignature(func.ReturnTypeToken.Lexeme, func.Parameters);
                if (_functions.ContainsKey(func.Name))
                {
                    throw new SemanticException($"Function '{func.Name}' already declared", func.IdentifierToken, options, context);
                }

                _functions[func.Name] = signature;

                if (CompilerContext.AssemblyKeywords.ContainsKey(func.Name))
                {
                    throw new CodeGenException($"Can't declare function with name '{func.Name}'. Reserved keyword",
                        func.IdentifierToken, options, context
                    );
                }
            }
        }
        // Second pass: analyze all nodes
        foreach (AstNode node in nodes)
        {
            AnalyzeNode(node, null, options, context);
        }
    }

    private string? AnalyzeNode(AstNode node, string? expectedType, CompilerOptions options, CompilerContext context)
    {
        switch (node)
        {
            case BooleanLiteralNode:
                return "bool";
            case NumberLiteralNode:
                return "int";
            case VariableDeclarationNode varDecl:
                string varType = varDecl.TypeName;
                if (!ValidTypes.Contains(varType))
                {
                    throw new SemanticException($"Unknown type '{varType}' for variable '{varDecl.Name}'", varDecl.IdentifierToken, options, context);
                }
                if (_variables.ContainsKey(varDecl.Name))
                {
                    throw new SemanticException($"Variable '{varDecl.Name}' already declared", varDecl.IdentifierToken, options, context);
                }
                else
                {
                    _variables[varDecl.Name] = varType;
                }
                if (varDecl.Initializer != null)
                {
                    string? initType = AnalyzeNode(varDecl.Initializer, varType, options, context);
                    if (initType != null && initType != varType)
                    {
                        throw new SemanticException($"Type mismatch in initialization of '{varDecl.Name}': expected {varType}, got {initType}", varDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case VariableAssignmentNode varAssign:
                if (!_variables.TryGetValue(varAssign.Name, out string? assignType))
                {
                    throw new SemanticException($"Assignment to undeclared variable '{varAssign.Name}'", varAssign.IdentifierToken, options, context);
                }
                if (varAssign.Value != null)
                {
                    string? valueType = AnalyzeNode(varAssign.Value, assignType, options, context);
                    if (assignType != null && valueType != null && valueType != assignType)
                    {
                        throw new SemanticException($"Type mismatch in assignment to '{varAssign.Name}': expected {assignType}, got {valueType}", varAssign.IdentifierToken, options, context);
                    }
                }
                return assignType;
            case IdentifierNode id:
                if (!_variables.TryGetValue(id.Name, out string? idType))
                {
                    throw new SemanticException($"Use of undeclared variable '{id.Name}'", id.IdentifierToken, options, context);
                }
                return idType;
            case ConditionNode cond:
                string? leftType = AnalyzeNode(cond.Left, null, options, context);
                string? rightType = AnalyzeNode(cond.Right, null, options, context);
                if ((leftType != "int" && leftType != "bool") || (rightType != "int" && rightType != "bool"))
                {
                    throw new SemanticException($"Condition operands must be int or bool, got {leftType} and {rightType}", null, options, context);
                }
                return "bool";
            case IfStatementNode ifStmt:
                string? condType = AnalyzeNode(ifStmt.Condition, "bool", options, context);
                if (condType != "bool" && condType != "int")
                {
                    throw new SemanticException($"If condition must be bool or int, got {condType}", null, options, context);
                }
                foreach (AstNode stmt in ifStmt.Body) AnalyzeNode(stmt, null, options, context);
                if (ifStmt.ElseBody != null) foreach (AstNode stmt in ifStmt.ElseBody) AnalyzeNode(stmt, null, options, context);
                return null;
            case WhileStatementNode whileStmt:
                string? whileCondType = AnalyzeNode(whileStmt.Condition, "bool", options, context);
                if (whileCondType != "bool" && whileCondType != "int")
                {
                    throw new SemanticException($"While condition must be bool or int, got {whileCondType}", null, options, context);
                }
                foreach (AstNode stmt in whileStmt.Body) AnalyzeNode(stmt, null, options, context);
                return null;
            case CallExpressionNode call:
                if (!_functions.TryGetValue(call.Callee, out FunctionSignature? sig))
                {
                    throw new SemanticException($"Call to undeclared function '{call.Callee}'", call.CalleeToken, options, context);
                }
                if (call.Arguments.Count != sig.Parameters.Count)
                {
                    throw new SemanticException($"Function '{call.Callee}' expects {sig.Parameters.Count} arguments, got {call.Arguments.Count}", call.CalleeToken, options, context);
                }
                for (int i = 0; i < Math.Min(call.Arguments.Count, sig.Parameters.Count); i++)
                {
                    string? argType = AnalyzeNode(call.Arguments[i], sig.Parameters[i].TypeName, options, context);
                    if (argType != null && argType != sig.Parameters[i].TypeName)
                    {
                        throw new SemanticException($"Type mismatch in argument {i + 1} of '{call.Callee}': expected {sig.Parameters[i].TypeName}, got {argType}", call.CalleeToken, options, context);
                    }
                }
                return sig.ReturnType;
            case FunctionDeclarationNode func:
                // New scope for parameters
                Dictionary<string, string> oldVars = new Dictionary<string, string>(_variables);
                foreach (FunctionParameter param in func.Parameters)
                {
                    _variables[param.Name] = param.TypeName;
                }
                foreach (AstNode stmt in func.Body)
                {
                    AnalyzeNode(stmt, func.ReturnTypeToken.Lexeme, options, context);
                }
                _variables.Clear();
                foreach (KeyValuePair<string, string> kv in oldVars) _variables[kv.Key] = kv.Value;
                return null;
            case ReturnStatementNode ret:
                string? retType = AnalyzeNode(ret.ReturnValue, expectedType, options, context);
                if (expectedType != null && retType != null && retType != expectedType)
                {
                    throw new SemanticException($"Return type mismatch: expected {expectedType}, got {retType}", null, options, context);
                }
                return retType;
            case ArrayDeclarationNode arrayDecl:
                string arrayType = arrayDecl.TypeName;
                if (!ValidTypes.Contains(arrayType))
                {
                    throw new SemanticException($"Unknown type '{arrayType}' for array '{arrayDecl.Name}'", arrayDecl.IdentifierToken, options, context);
                }
                if (_variables.ContainsKey(arrayDecl.Name))
                {
                    throw new SemanticException($"Variable '{arrayDecl.Name}' already declared", arrayDecl.IdentifierToken, options, context);
                }

                // Check that size is an integer
                string? sizeType = AnalyzeNode(arrayDecl.Size, "int", options, context);
                if (sizeType != "int")
                {
                    throw new SemanticException($"Array size must be an integer, got {sizeType}", null, options, context);
                }

                _variables[arrayDecl.Name] = arrayType;

                if (arrayDecl.Initializer != null)
                {
                    string? initType = AnalyzeNode(arrayDecl.Initializer, arrayType, options, context);
                    if (initType != null && initType != arrayType)
                    {
                        throw new SemanticException($"Type mismatch in array initialization of '{arrayDecl.Name}': expected {arrayType}, got {initType}", arrayDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case ArrayLiteralNode arrayLit:
                if (arrayLit.Elements.Count == 0)
                {
                    return "int"; // Default type for empty arrays
                }

                string? firstType = AnalyzeNode(arrayLit.Elements[0], null, options, context);
                foreach (AstNode element in arrayLit.Elements.Skip(1))
                {
                    string? elementType = AnalyzeNode(element, firstType, options, context);
                    if (elementType != firstType)
                    {
                        throw new SemanticException($"All array elements must have the same type, got {firstType} and {elementType}", null, options, context);
                    }
                }
                return firstType;
            case ArrayAccessNode arrayAccess:
                if (!_variables.TryGetValue(arrayAccess.ArrayName, out string? accessArrayType))
                {
                    throw new SemanticException($"Use of undeclared array '{arrayAccess.ArrayName}'", arrayAccess.ArrayToken, options, context);
                }

                // Check that index is an integer
                string? indexType = AnalyzeNode(arrayAccess.Index, "int", options, context);
                if (indexType != "int")
                {
                    throw new SemanticException($"Array index must be an integer, got {indexType}", null, options, context);
                }

                return accessArrayType;
            case ArrayAssignmentNode arrayAssign:
                if (!_variables.TryGetValue(arrayAssign.ArrayName, out string? assignArrayType))
                {
                    throw new SemanticException($"Assignment to undeclared array '{arrayAssign.ArrayName}'", arrayAssign.ArrayToken, options, context);
                }

                // Check that index is an integer
                string? assignIndexType = AnalyzeNode(arrayAssign.Index, "int", options, context);
                if (assignIndexType != "int")
                {
                    throw new SemanticException($"Array index must be an integer, got {assignIndexType}", null, options, context);
                }

                // Check that value matches array type
                string? assignValueType = AnalyzeNode(arrayAssign.Value, assignArrayType, options, context);
                if (assignValueType != null && assignValueType != assignArrayType)
                {
                    throw new SemanticException($"Type mismatch in array assignment to '{arrayAssign.ArrayName}': expected {assignArrayType}, got {assignValueType}", arrayAssign.ArrayToken, options, context);
                }

                return assignArrayType;
            default:
                // For other nodes, just recurse if they have children
                return null;
        }
    }
}
