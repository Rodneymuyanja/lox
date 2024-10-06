// See https://aka.ms/new-console-template for more information
namespace lox.src
{
    internal class Scanner(string _source_code)
    {
        private readonly string source_code = _source_code;
        private List<Token> tokens = [];

        //current lexeme being considered
        private int current = 0;
        //start of the current lexeme being considered
        private int start = 0;
        //current line in the source code
        private int line = 1;
        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));

            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN); break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN); break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE); break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE); break;
                case ',':
                    AddToken(TokenType.COMMA); break;
                case '.':
                    AddToken(TokenType.DOT); break;
                case '-':
                    AddToken(TokenType.MINUS) ; break;
                case '+':
                    AddToken(TokenType.PLUS) ; break;
                case ';':
                    AddToken(TokenType.SEMICOLON); break;
                case '*':
                    AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        //this is a comment
                        //read till the end
                        while(Peek() != '\0' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    //whitespace
                    break;
                case '\n':
                    line++;
                    break;
                case '"':
                    String();
                    break;
                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else 
                    {
                        Lox.Error(line, "Unexpected character");
                    }
                    
                    break;
            }
        }

        private void AddToken(TokenType token_type)
        {
            AddToken(token_type, null);
        }
        private void AddToken(TokenType token_type, object? literal)
        {
            string text = source_code.Substring(start, current);
            tokens.Add(new Token(token_type,text, literal,line));
        }

        private void String()
        {
            while(Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(line, "Unterminated string");
            }

            //at this point we have the closing "
            Advance();

            string value_in_quotes = source_code.Substring(start + 1, current - 1);
            AddToken(TokenType.STRING,value_in_quotes);
        }

        private void Number()
        {
            //peek the current value and check if it's a digit
            //if it is, advance 
            while (IsDigit(Peek())) Advance();

            //if it's not a digit
            //but it is a '.' , lookahead and check the what follows the '.'
            //if its a digit as advance to the next
            //then keep going until we are out of strings
            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            //then extrapolate from the start to where we are 
            string value = source_code.Substring(start, current);
            AddToken(TokenType.NUMBER,Double.Parse(value));
        }
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        //lookahead one
        private char Peek()
        {
            if(IsAtEnd()) return '\0';
            return source_code[current];
        }

        //lookahead 2
        private char PeekNext()
        {
            if(current + 1 >= source_code.Length) 
            {
                return '\0';
            }

            return source_code[current + 1];
        }
        private bool Match(char expected)
        {
            if (IsAtEnd())
                return false;

            if (source_code[current] != expected)
                return false;

            current++;
            return true;
        }

        //move to the next character
        private char Advance()
        {
            return source_code[current++];
        }

        //check if we're at the end of the source code
        private bool IsAtEnd()
        {
            return current >= source_code.Length;
        }
    }
}