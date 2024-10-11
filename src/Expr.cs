
using lox.src.Interfaces;

namespace lox.src
{
    public abstract class Expr
    {
        public interface IVisitor<T>
        {
            public T VisitLiteralExpr(Expr.Literal expr);
            public T VisitGroupExpr(Expr.Grouping expr);
            public T VisitUnaryExpr(Expr.Unary expr);
            public T VisitBinaryExpr(Expr.Binary expr);
        }

        public abstract T Accept <T>(IVisitor<T> visitor);
        public class Binary(Expr _left, Token _operator, Expr _right) : Expr
        {
            public Expr left = _left;
            public Token __operator = _operator;
            public Expr right = _right;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }
        public class Grouping(Expr _expression) : Expr
        {
            public Expr expression = _expression;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupExpr(this);
            }
        }
        public class Literal(object _value) : Expr
        {
            public object value = _value;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }
        public class Unary(Token _operator, Expr _right) : Expr
        {
            public Token __operator = _operator;
            public Expr right = _right;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }
    }

}
