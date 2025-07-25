namespace DircCompiler.Lexing;

public enum TokenType
{
    // Single-character symbols
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    LeftBracket, RightBracket,
    Comma, Semicolon,
    Plus, Minus, Asterisk, Slash, Pipe, Ampersand, Caret,
    Equals, ExclamationPoint,

    // Multi-character operators
    // Conditions
    EqualEqual, NotEqual,
    Less, LessEqual,
    Greater, GreaterEqual,
    // Assignment shorthands
    PlusEqual, MinusEqual, AsteriskEqual,
    SlashEqual, PipeEqual, AmpersandEqual, CaretEqual,

    // Literals
    Identifier,
    Number,
    BinaryNumber,
    HexNumber,
    True,
    False,

    // Keywords
    Import,
    If, Else,
    Return,
    While,
    For,
}
