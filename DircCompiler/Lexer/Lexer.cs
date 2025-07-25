namespace DircCompiler.Lexing;

class Lexer
{
    private string _source = "";
    private List<Token> _tokens = new();
    private int __start = 0;
    private int _current = 0;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> _keywords = new()
    {
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "import", TokenType.Import },
        { "return", TokenType.Return },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "while", TokenType.While },
        { "for", TokenType.For },
    };

    private CompilerContext _compilerContext;
    private CompilerOptions _compilerOptions;

    public Lexer(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        _compilerContext = compilerContext;
        _compilerOptions = compilerOptions;
    }

    public List<Token> Tokenize(string source)
    {
        _source = source;
        _tokens = new List<Token>();
        __start = 0;
        _current = 0;
        _line = 1;

        while (!IsAtEnd())
        {
            __start = _current;
            ScanToken();
        }

        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            // Single-character tokens
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case '[': AddToken(TokenType.LeftBracket); break;
            case ']': AddToken(TokenType.RightBracket); break;
            case ',': AddToken(TokenType.Comma); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '+': AddToken(TokenType.Plus); break;
            case '-': AddToken(TokenType.Minus); break;
            case '*': AddToken(TokenType.Asterisk); break;
            case '|': AddToken(TokenType.Pipe); break;
            case '&': AddToken(TokenType.Ampersand); break;
            case '^': AddToken(TokenType.Caret); break;
            case '/':
                if (NextIs('/'))
                {
                    // Comment, skip to end of line
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else if (NextIs('*'))
                {
                    // Multiline comment, skip to first "*/"
                    while (!(_source[_current - 1] == '*' && Peek() == '/') && !IsAtEnd()) Advance();
                    Advance();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;
            case '=':
                AddToken(NextIs('=') ? TokenType.EqualEqual : TokenType.Equals);
                break;
            case '!':
                AddToken(NextIs('=') ? TokenType.NotEqual : TokenType.ExclamationPoint);
                break;
            case '<':
                AddToken(NextIs('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(NextIs('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;

            // Whitespace
            case ' ':
            case '\t':
            case '\r':
                break;
            case '\n':
                _line++;
                break;

            // Literals
            default:
                if (IsDigit(c))
                {
                    if (c == '0' && NextIs('b'))
                    {
                        BinaryNumber();
                    }
                    else if (c == '0' && NextIs('x'))
                    {
                        HexNumber();
                    }
                    else
                    {
                        Number();
                    }
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    throw new LexicalException($"Unknown character", c, _line, _compilerOptions, _compilerContext);
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = _source[__start.._current];
        if (_keywords.TryGetValue(text, out TokenType type))
        {
            AddToken(type);
        }
        else
        {
            AddToken(TokenType.Identifier);
        }
    }

    private void BinaryNumber()
    {
        Advance();
        Advance();

        while (IsDigit(Peek())) Advance();

        string text = _source[__start.._current];
        AddToken(TokenType.BinaryNumber, text);
    }

    private void HexNumber()
    {
        Advance();
        Advance();

        while (IsAlphaNumeric(Peek())) Advance();

        string text = _source[__start.._current];
        AddToken(TokenType.HexNumber, text);
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        string text = _source[__start.._current];
        AddToken(TokenType.Number, text);
    }

    private bool NextIs(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;
        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        string text = _source[__start.._current];
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';
    private static bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
}
