using lox.src.Interfaces;
namespace lox.src
{
    public class Interpreter : IVisitor<object>
    {
        public void Interpret(Expr expr)
        {
            try
            {
                Object value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError r)
            {
                Lox.RuntimeError(r);
            }
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
    }
}
