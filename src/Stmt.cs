
namespace lox.src
{
    public abstract class Stmt
    {
        public interface IVisitor<T>
        {
            public T VisitExpressionStmt(Stmt.Expression stmt);
            public T VisitPrintStmt(Stmt.Print stmt);
            public T VisitVarStmt(Stmt.Var stmt);
            public T VisitBlock(Stmt.Block stmt);
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

        public class Var(Token _name, Expr _initializer) : Stmt
        {
            public Token name = _name;
            public Expr initializer = _initializer;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }
        public class Block(List<Stmt> _statements) : Stmt
        {
            public List<Stmt> statements = _statements;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlock(this);
            }
        }
    }
}
