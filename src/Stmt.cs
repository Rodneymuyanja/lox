
namespace lox.src
{
    public abstract class Stmt
    {
        public interface IVisitor<T>
        {
            public T VisitExpressionStmt(Stmt.Expression stmt);
            public T VisitPrintStmt(Stmt.Print stmt);
        }

        public abstract T Accept<T>(Stmt.IVisitor<T> visitor);
        public class Expression(Expr _expression) : Stmt
        {
            public Expr expression = _expression;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }
        public class Print(Expr _expression) : Stmt
        {
            public Expr expression = _expression;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }
    }
}
