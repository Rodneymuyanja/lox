﻿
namespace lox.src
{
    /// <summary>
    /// 
    /// the parser
    /// first of all the grammar
    /// 
    /// expression -> equality;
    /// equality -> comparison (('==' | '!=') comparison)*;
    /// comparison -> term(('<' | '<=' | '>' | '>=') term)*;
    /// term -> factor (( '+' | '-' ) factor)*;
    /// factor -> unary (('/' | '*') unary)*;
    /// unary -> ('!' | '-') unary | primary;
    /// primary -> NUMBER | STRING | 'false' | 'true' | 'nil' | '(' expression ')';
    /// 
    /// 
    /// 
    /// the asterisk means repeat whatever is before 
    /// 0 to as many times....basically represented as a loop
    /// 
    /// </summary>
    internal class Parser(List<Token> _tokens)
    {
        private class ParseError : Exception { }
        //this is the list of tokens from the scanner
        private List<Token> tokens = _tokens;
        //points to the NEXT token waiting to be read
        private int current = 0;

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError error)
            {
                return null!;
            }
        }

        internal Expr Expression()
        {
            return Equality();
        }

        //equality -> comparison (('==' | '!=') comparison)*;
        internal Expr Equality()
        {
            Expr expr = Comparison();
            while (Match([TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL]))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op,right);
            }

            return expr;
        }

        //comparison -> term(('<' | '<=' | '>' | '>=') term)*;
        internal Expr Comparison()
        {
            Expr expr = Term();
            while (Match([TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL]))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        //term -> factor (( '+' | '-' ) factor)*;
        internal Expr Term()
        {
            Expr expr = Factor();

            while (Match([TokenType.PLUS, TokenType.MINUS]))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        //factor -> unary (('/' | '*') unary)*;
        internal Expr Factor()
        {
            Expr expr = Unary();

            while (Match([TokenType.SLASH, TokenType.STAR]))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr ,op, right);
            }

            return expr;
        }
        // unary -> ('!' | '-') unary | primary;
        internal Expr Unary()
        {
            if (Match([TokenType.BANG, TokenType.MINUS]))
            {
                Token op = Previous();
                //you were wondering how this would break
                //well if on some recursive call we don't enter this
                //block, we'd return a Primary and it has high precedence
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        //primary -> NUMBER | STRING | 'false' | 'true' | 'nil' | '(' expression ')';

        internal Expr Primary()
        {
            if (Match([TokenType.FALSE])) return new Expr.Literal(false);
            if (Match([TokenType.TRUE])) return new Expr.Literal(true);
            if (Match([TokenType.NIL])) return new Expr.Literal(null!);
            if (Match([TokenType.NUMBER, TokenType.STRING]))
            {
                return new Expr.Literal(Previous().literal);
            }

            if (Match([TokenType.LEFT_PAREN]))
            {
                Expr expr = Expression();
                //check if we have a closing paren
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");
                return new Expr.Grouping(expr);
            }

            //at this point i don't know what was passed ngl
            throw Error(Peek(), "Expected expression [primary]");
        }

        private Token Consume(TokenType token_type, string message)
        {
            if (Check(token_type)) return Advance();

            throw Error(Peek(), message);
        }
        
       
        //gets the previous token
        //the one behind current
        private Token Previous()
        {
            return tokens[current-1];
        }

        //this is a lookahead, but no advance
        private Token Peek()
        {
            return tokens[current];
        }

        //checks if the next token is the end
        private bool IsAtEnd()
        {
            return Peek().token_type == TokenType.EOF;
        }

        //moves our pointer to the next token
        //in the stream
        private Token Advance()
        {
            if(!IsAtEnd())
            {
                current++;
            }

            return Previous();
        }

        //just checks if the next token matches a 
        //type we want
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;

            return Peek().token_type == type;
        }
        
        //checks if the next token is in a certain list
        //note it advances into the stream
        private bool Match(List<TokenType> token_types)
        {
            foreach (var token_type in token_types)
            {
                if(Check(token_type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().token_type == TokenType.SEMICOLON) return;

                switch (Peek().token_type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.FOR:
                    default:
                        return;
                }
            }
        }
    }
}
