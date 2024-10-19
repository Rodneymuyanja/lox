// See https://aka.ms/new-console-template for more information
using System;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static lox.src.Stmt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lox.src
{
    internal class Scanner
    {
        private readonly string source_code ;
        private List<Token> tokens = [];

        //current lexeme being considered
        private int current = 0;
        //start of the current lexeme being considered
        private int start = 0;
        //current line in the source code
        private int line = 1;

        private Dictionary<string, TokenType> keywords = [];
        
        public Scanner(string _source_code)
        {
            source_code = _source_code;
            LoadKeywords();
        }

        private void LoadKeywords()
        {
            keywords.Add("and", TokenType.AND);
            keywords.Add("class", TokenType.CLASS);
            keywords.Add("else", TokenType.ELSE);
            keywords.Add("false", TokenType.FALSE);
            keywords.Add("for", TokenType.FOR);
            keywords.Add("fun", TokenType.FUN);
            keywords.Add("if", TokenType.IF);
            keywords.Add("nil", TokenType.NIL);
            keywords.Add("or", TokenType.OR);
            keywords.Add("print", TokenType.PRINT);
            keywords.Add("return", TokenType.RETURN);
            keywords.Add("super", TokenType.SUPER);
            keywords.Add("this", TokenType.THIS);
            keywords.Add("true", TokenType.TRUE);
            keywords.Add("var", TokenType.VAR);
            keywords.Add("while", TokenType.WHILE);
        }
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
                        break;
                    }

                    if (IsAlpha(c))
                    {
                        Identifier();
                        break;
                    }
                    
                    Lox.Error(line, "Unexpected character");
                    break;
            }
        }

        private void AddToken(TokenType token_type)
        {
            AddToken(token_type, null);
        }
        private void AddToken(TokenType token_type, object? literal)
        {
            //string text = source_code.Substring(start, current);
            string text = ExtractString(start, current);
            tokens.Add(new Token(token_type,text, literal,line));
        }

        private string ExtractString(int start, int end)
        {
            StringBuilder stringbuilder = new();
            int offset = start;
            char c;

            while (offset != end)
            {
                c = source_code[offset];
                stringbuilder.Append(c);
                offset++;
            }

            string str = stringbuilder.ToString();
            return str.Trim('\"');
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
            //string value_in_quotes = source_code.Substring(start + 1, current - 1);
            

            //StringBuilder stringbuilder = new();
            ////get the first character

            //int offset = start + 1;
            //char c = source_code[offset];

            //while (c != '\"')
            //{
            //    c = source_code[offset];
            //    if (c == '\"') break;
            //    stringbuilder.Append(c);
            //    offset++;
            //}

            string value_in_quotes = ExtractString(start+1, current-1);

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
            string value = ExtractString(start,current);
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

        //character
        private static bool IsAlpha(char c)
        {
            return (c >= 'A' && c <= 'Z')||
                (c >= 'a' && c <= 'z') ||
                c == '_';
        }
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = ExtractString(start, current);
            if (keywords.TryGetValue(text,out TokenType token_type))
            {
                AddToken(token_type);
                return;
            }

            AddToken(TokenType.IDENTIFIER);
        }
    }
}