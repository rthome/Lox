using System;
using System.Collections.Generic;
using static lox.TokenType;

namespace lox
{
    class Scanner
    {
        #region Keyword Lookup

        static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            {"and",    And},
            {"class",  Class},
            {"else",   Else},
            {"false",  False},
            {"for",    For},
            {"fun",    Fun},
            {"if",     If},
            {"nil",    Nil},
            {"or",     Or},
            {"print",  Print},
            {"return", Return},
            {"super",  Super},
            {"this",   This},
            {"true",   True},
            {"var",    Var},
            {"while",  While},
        };

        #endregion

        readonly string source;
        readonly List<Token> tokens = new List<Token>();

        int start;
        int current;
        int line = 1;

        #region Helpers

        bool IsAtEnd => current >= source.Length;

        char Peek => IsAtEnd ? '\0' : source[current];

        char PeekNext
        {
            get
            {
                if (current + 1 >= source.Length)
                    return '\0';
                return source[current + 1];
            }
        }
        
        bool IsDigit(char c) => c >= '0' && c <= '9';

        bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z')
                || (c >= 'A' && c <= 'Z')
                || c == '_';
        }

        bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

        char Advance()
        {
            current++;
            return source[current - 1];
        }

        bool Match(char expected)
        {
            if (IsAtEnd)
                return false;
            if (source[current] != expected)
                return false;
            
            current++;
            return true;
        }

        void AddToken(TokenType type) => AddToken(type, null);

        void AddToken(TokenType type, object literal)
        {
            var text = LexemeValue();
            tokens.Add(new Token(type, text, literal, line));
        }

        string LexemeValue(int leftIndent = 0, int rightIndent = 0)
            => source.Substring(start + leftIndent, (start + leftIndent) + (current - rightIndent));

        #endregion

        #region Scan Methods
        
        void String()
        {
            while (Peek != '"' && !IsAtEnd)
            {
                if (Peek == '\n')
                    line++;
                Advance();
            }

            if (IsAtEnd)
            {
                Program.Error(line, "Unterminated string literal.");
                return;
            }

            // For the closing "
            Advance();

            var value = LexemeValue(1, -1);
            AddToken(TokenType.String, value);
        }

        void Number()
        {
            while (IsDigit(Peek))
                Advance();

            if (Peek == '.' && IsDigit(PeekNext))
            {
                Advance();
                while (IsDigit(Peek))
                    Advance();
            }

            var value = LexemeValue();
            AddToken(TokenType.Number, double.Parse(value));
        }

        void Identifier()
        {
            while (IsAlphaNumeric(Peek))
                Advance();
            
            var text = LexemeValue();
            if (keywords.TryGetValue(text, out var type))
                AddToken(type);
            else
                AddToken(TokenType.Identifier);
        }

        void ScanToken()
        {
            var c = Advance();
            switch(c)
            {
                case '(': AddToken(LeftParen); break;
                case ')': AddToken(RightParen); break;
                case '{': AddToken(LeftBrace); break;
                case '}': AddToken(RightBrace); break;
                case ',': AddToken(Comma); break;
                case '.': AddToken(Dot); break;
                case '-': AddToken(Minus); break;
                case '+': AddToken(Plus); break;
                case ';': AddToken(Semicolon); break;
                case '*': AddToken(Star); break;

                case '!': AddToken(Match('=') ? BangEqual : Bang); break;
                case '=': AddToken(Match('=') ? EqualEqual : Equal); break;
                case '<': AddToken(Match('=') ? LessEqual : Less); break;
                case '>': AddToken(Match('=') ? GreaterEqual : Greater); break;

                // TODO: Support C-style /* ... */ comments
                case '/':
                    if (Match('/'))
                    {
                        while (Peek != '\n' && !IsAtEnd)
                            Advance();
                    }
                    else
                        AddToken(Slash);
                    break;
                
                case '"': String(); break;
                
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;

                case '\n':
                    line++;
                    break;

                default:
                    if (IsDigit(c))
                        Number();
                    else if (IsAlpha(c))
                        Identifier();
                    else
                        Program.Error(line, "Unexpected character.");
                    break;
            }
        }

        #endregion

        public IEnumerable<Token> ScanTokens()
        {
            while (!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, string.Empty, null, line));
            return tokens;
        }

        public Scanner(string source)
        {
            this.source = source;
        }
    }
}