using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayAccessNode : AstNode
{
    public Token ArrayToken { get; }
    public AstNode Index { get; }
    public string ArrayName => ArrayToken.Lexeme;
    // The semantic analyzer will fill this in
    public bool ArrayIsPointer { get; set; }

    public ArrayAccessNode(Token arrayToken, AstNode index, bool arrayIsPointer = false)
    {
        ArrayToken = arrayToken;
        Index = index;
        ArrayIsPointer = arrayIsPointer;
    }

    public override string ToString() => $"ArrayAccess({ArrayName}[{Index}])";
}
