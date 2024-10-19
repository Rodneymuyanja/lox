
namespace lox.src
{
    /// <summary>
    /// 
    /// the parser
    /// first of all the grammar
    /// 
    /// program             -> declaration* EOF;
    /// declaration         -> vardecl 
    ///                         | statement
    ///                         | funDecl;
    /// funDecl             -> "fun" IDENTIFIER "(" parameters? ")" block;
    /// parameters          -> IDENTIFIER ("," IDENTIFIER)*;
    /// statement           -> printStatement 
    ///                         | ifStatement 
    ///                         | expressionStatement 
    ///                         | whileStatement
    ///                         | forStatement
    ///                         | block
    ///                         | returnStatement;
    /// returnStatement     -> "return" expression? ";"
    /// block               -> "{" declaration* "}";
    /// printStatement      -> "print" expression ";"
    /// expressionStatement -> expression;
    /// ifStatement         -> "if" "(" expression ")" statement
    ///                         ( "else" statement )? ;
    /// whileStatement      -> "while" "(" expression ")" statement;
    /// forStatement        -> "for" "(" ( varDecl | expressionStatement ";" )
    ///                         expression? ";"
    ///                         expression? ")"
    ///                         statement;
    /// expression          -> assignment;
    /// assignment          -> IDENTIFIER "=" assignment 
    ///                        | logical_or;
    /// logical_or          -> logical_and ("or" logical_and)*;
    /// logical_and         -> equality ("and" equality)*;
    /// equality            -> comparison (('==' | '!=') comparison)*;
    /// comparison          -> term(('<' | '<=' | '>' | '>=') term)*;
    /// term                -> factor (( '+' | '-' ) factor)*;
    /// factor              -> unary (('/' | '*') unary)*;
    /// unary               -> ('!' | '-') unary | call;
    /// call                -> primary("(" arguments? ")")*;
    /// arguments           -> expression ("," expression)*;
    /// primary             -> NUMBER 
    ///                        | STRING 
    ///                        | 'false' 
    ///                        | 'true' 
    ///                        | 'nil' 
    ///                        | '(' expression ')';
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
        private const int MAX_ARGS_LENGTH = 255;

        public List<Stmt> Parse()
        {
            List<Stmt> statements = [];
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }


        private List<Stmt> Block()
        {
            List<Stmt> statements = [];
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());   
            }

            Consume(TokenType.RIGHT_BRACE, "Expected '}'");
            return statements;
        }

        //declaration -> vardecl | statement
        private Stmt Declaration()
        {
            try
            {
                if (Match([TokenType.VAR])) return VarDeclaration();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null!;
            }
        }

        private Stmt Statement()
        {
            if (Match([TokenType.PRINT])) return PrintStatement();
            if (Match([TokenType.LEFT_BRACE])) return new Stmt.Block(Block());
            if (Match([TokenType.IF])) return IfStatement();
            if (Match([TokenType.WHILE])) return WhileStatement();
            if (Match([TokenType.FUN])) return Function();
            if (Match([TokenType.RETURN])) return ReturnStatement();

            return ExpressionStatement();
        }

        // returnStatement     -> "return" expression? ";"

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null!;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after return statement");
            return new Stmt.Return(keyword, value);
        }

        // funDecl -> IDENTIFIER "(" parameters? ")" block;
        private Stmt.Function Function()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected function name");
            Consume(TokenType.LEFT_PAREN, "Expected '(' after function declaration");
            List<Token> parameters = [];

            //this is how we cater for zero params
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(parameters.Count >= MAX_ARGS_LENGTH)
                    {
                        Error(Peek(), "Too many parameters");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER,"Expected parameter name"));

                } while (Match([TokenType.COMMA]));
            }

            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameter list");
            Consume(TokenType.LEFT_BRACE, "Expected '{' after function declaration");
            List<Stmt> stmts = Block();
            return new Stmt.Function(name,parameters, stmts);
        }

        // call -> primary("(" arguments? ")")*;
        private Expr Call()
        {
            Expr expr = Primary();
            while (true)
            {
                if (Match([TokenType.LEFT_PAREN]))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> args = [];
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(args.Count >= MAX_ARGS_LENGTH)
                    {
                        Error(Peek(), "Too many arguments");
                    }
                    args.Add(Expression());
                } while (Match([TokenType.COMMA]));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments");

            return new Expr.Call(callee,paren,args);
        }

        // whileStatement -> "while" "(" expression ")" statement;
        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_BRACE, "Expect '(' after while");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_BRACE, "Expect ')' after expression in while loop");

            Stmt body = Statement();

            return new Stmt.While(condition, body); 
        }

        // ifStatement -> "if" "(" expression ")" statement
        //                 ( "else" statement )? ;
        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after if");
            Expr condition = Expression();
            //this is the only reason we need braces actually
            //to know when an expression ends
            //the LEFT_BRACE is just for consistency
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression in if");

            Stmt then_branch = Statement();

            Stmt else_branch = null!;

            if (Match([TokenType.ELSE]))
            {
                else_branch = Statement();
            }

            return new Stmt.If(condition, then_branch, else_branch);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' at the end of a print statement");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' at the end of a expression statement");
            return new Stmt.Expression(expr);
        }

        //assignment -> IDENTIFIER "=" assignment | equality;
        private Expr Assignment()
        {
            Expr expr = Equality();

            if (Match([TokenType.EQUAL]))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if(expr is Expr.Variable v)
                {
                    Token name = v.name;
                    return new Expr.Assign(name, value);    
                }

                Error(equals, "Invalid assignment target");
            }

            return expr;
        }

        // logical_or -> logical_and ("or" logical_and)*;
        private Expr Or()
        {
            Expr expr = And();

            while (Match([TokenType.OR]))
            {
                Token op = Previous();
                Expr right = Or();
                expr = new Expr.Logical(expr,op,right);
            }

            return expr;
        }

        // logical_and -> equality ("and" equality)*;
        private Expr And()
        {
            Expr expr = Equality();

            while (Match([TokenType.AND]))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        //varDecl -> "var" IDENTIFIER ("=" expression)? ";";
        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected an identifier");
            Expr identifier = null!;
            if (Match([TokenType.EQUAL])) { 
                identifier = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
            return new Stmt.Var(name, identifier);
        }

        internal Expr Expression()
        {
            return Assignment();
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
            return Call();
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

            if (Match([TokenType.IDENTIFIER]))
            {
                return new Expr.Variable(Previous());
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

        /// forStatement -> "for" "(" ( varDecl | expressionStatement ";" )
        ///                  expression? ";"
        ///                  expression? ")"
        ///                  statement;
        ///                  

        ///we desugar for loops
        ///
        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after for");

            Stmt initializer;

            //for(; ...
            //not initializer
            if (Match([TokenType.SEMICOLON]))
            {
                initializer = null!;
            }
            //for(var i = 0...
            //variable declaration
            else if( Match([TokenType.VAR]))
            {
                initializer = VarDeclaration();
            }
            //for(i = 0 ...
            //expression statement
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null!;
            //for(var i = 0;;
            if (!Check(TokenType.SEMICOLON))
            {
                //for(var i = 0; i < 10 ;
                condition = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after condition");

            Expr increment = null!;
            //for(var i = 0; i < 10 ) ..no increment
            if (!Check(TokenType.RIGHT_PAREN))
            {
                //for(var i = 0; i < 10 ; i = i + 1)
                increment = Expression();
            }

            Consume(TokenType.RIGHT_PAREN, "Expected ')' to close for");

            Stmt body = Statement();
            if(increment is not null)
            {
                List<Stmt> body_bits = [];
                body_bits.Add(body);
                //the increment is attached to the body
                //and executed at the end of each block
                body_bits.Add(new Stmt.Expression(increment));

                body = new Stmt.Block(body_bits);
            }

            //if the condition is not specified
            //we go to infinity
            condition ??= new Expr.Literal(true);

            //this is where we desugar the for loop to
            //a while loop
            body = new Stmt.While(condition, body);

            if(initializer is not null)
            {
                List<Stmt> body_bits2 = [];
                //make the initializer part of the body
                //it should be executed once
                body_bits2.Add(initializer);
                body_bits2.Add(body);
                body = new Stmt.Block(body_bits2);
            }

            //interpreter receives a Block tree
            return body;
        }
    }
}
