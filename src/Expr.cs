

namespace lox.src
{
    internal abstract class Expr
    {
        public class Binary(Expr _left, Token _operator, Expr _right) : Expr
        {
            Expr left = _left;
            Token __operator = _operator;
            Expr right = _right;

        }
        public class Grouping(Expr _expression) : Expr
        {
            Expr expression = _expression;
        }
        public class Literal(object _value) : Expr
        {
            object value = _value;

        }
        public class Unary(Token _operator, Expr _right) : Expr
        {
            Token __operator = _operator;
            Expr right = _right;

        }
    }

}
