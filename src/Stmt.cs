
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
            public T VisitIfStmt(Stmt.If stmt);
            public T VisitWhileStmt(Stmt.While stmt);
            public T VisitFunctionStmt(Stmt.Function stmt);
            public T VisitReturnStmt(Stmt.Return stmt);
            public T VisitClassStmt(Stmt.Class stmt);
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

        public class If(Expr _condition, Stmt _then_branch, Stmt _else_branch) : Stmt
        {
            public Expr condition = _condition;
            public Stmt then_branch = _then_branch;
            public Stmt else_branch = _else_branch;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class While(Expr _condition, Stmt _body) : Stmt
        {
            public Expr condition = _condition;
            public Stmt body = _body;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

        public class Function(Token _name, List<Token> _parameters, List<Stmt> _body) : Stmt
        {
            public Token name = _name;
            public List<Token> parameters = _parameters;
            public List<Stmt> body = _body;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }

        public class Return(Token _Keyword, Expr _value) : Stmt
        {
            public Token Keyword = _Keyword;
            public Expr value = _value;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }
        }

        public class Class(Token _name, List<Stmt.Function> _methods) : Stmt
        {
            public Token name = _name;
            public List<Stmt.Function> methods = _methods;

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitClassStmt(this);
            }
        }
    }
}
