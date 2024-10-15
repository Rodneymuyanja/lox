
namespace lox.src
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private LoxEnvironment env = new ();
        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }catch(RuntimeError r)
            {
                Lox.RuntimeError(r);
            }
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null!;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null!;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);
            switch (expr.__operator.token_type)
            {
                case TokenType.PLUS:
                    if(left is Double && right is Double)
                    {
                        return (double)left + (double)right;
                    }

                    if(CheckAnyString(left,right))
                    {
                        return $"{left}{right}";
                    }

                    throw new RuntimeError(expr.__operator,"Expected numbers or strings");

                case TokenType.MINUS:
                    CheckNumberOperands(expr.__operator,left, right);
                    return (double)left - (double)right;

                case TokenType.STAR:
                    CheckNumberOperands(expr.__operator, left, right);
                    return (double)left * (double)right;

                case TokenType.SLASH:
                    CheckNumberOperands(expr.__operator, left, right);
                    //catch Zero division
                    if ((double)right == 0) throw new RuntimeError(expr.__operator, "Zero Division");

                    return (double)left / (double)right;

                case TokenType.GREATER:
                    CheckNumberOperands(expr.__operator, left, right);
                    return (double)left > (double)right;

                case TokenType.LESS:
                    CheckNumberOperands(expr.__operator, left, right);
                    return (double)left < (double)right;

                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.__operator, left, right);
                    return (double)left >= (double)right;

                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.__operator, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);

                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);

                default:
                    return null!;
            }
        }

        public object VisitGroupExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);
            switch (expr.__operator.token_type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.__operator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !Truthy(right);
                default:
                    return null!;
            }
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return env.Get(expr.name);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            //lox variables are implicitly NULL
            object value = null!;
            if (stmt.initializer is not null)
            {
                value = Evaluate(stmt.initializer);
            }

            env.Define(stmt.name.lexeme, value);
            return null!;
        }

        private bool CheckAnyString(Object left, Object right)
        {
            if(left is String || right is String)
            {
                return true;
            }

            return false;
        }

        //this method helps us recurse the tree
        private Object Evaluate(Expr expr)
        {
            //accept this very visitor
            return expr.Accept(this);
        }

        //lox's notion if truth is that 
        //everything else is true 
        //aside from null and false itself
        private Boolean Truthy(Object obj)
        {
            if(obj is null) return false;
            if(obj is Boolean) return (bool)obj;    

            return true;
        }

        private Boolean IsEqual(Object a, Object b)
        {
            if (a is null && b is null) return true;
            if (a is null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token op, Object operand)
        {
            if (operand is Double) return;
            throw new RuntimeError(op, "Operand must be a number");
        }

        private void CheckNumberOperands(Token op, Object right, Object left)
        {
            if (left is Double && right is Double) return;
            throw new RuntimeError(op, "Operands must be numbers");
        }

        private String Stringify(Object obj)
        {
            if (obj is null) return "nil";

            if(obj is Double number)
            {
                String text = number.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return obj.ToString()!;
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitAssignmentExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            env.Assign(expr.name, value);
            return value;
        }

        public object VisitBlock(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new LoxEnvironment(env));
            return null!;
        }

        private void ExecuteBlock(List<Stmt> statements, LoxEnvironment enclosing)
        {
            LoxEnvironment previous_env = this.env;
            try
            {
                env = enclosing;
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.env = previous_env;
            }
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (Truthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.then_branch);

            }
            else if (stmt.else_branch is not null)
            {
                Execute(stmt.else_branch);
            }
            return null!;

        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if(expr.op.token_type == TokenType.OR)
            {
                //or short circuits if 
                //cuz it looks for any truth on the leaves
                //so if the left node is true
                //short-circuit and return it
                if (Truthy(left)) return left;
            }else if(expr.op.token_type == TokenType.AND)
            {
                //and only cares about trues and both need to be true
                //so if any is false short-circuit
                if (!Truthy(left)) return left;
            }

            //other wise evaluate the right node to
            //see if its true 
            return Evaluate(expr.right);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            if (Truthy(stmt.condition))
            {
                Execute(stmt.body);
            }

            return null!;
        }
    }
}
