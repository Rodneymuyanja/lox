
namespace lox.src.Interfaces
{
    public interface IVisitor<T>
    {
       public T VisitLiteralExpr(Expr.Literal expr);
       public T VisitGroupExpr(Expr.Grouping expr);
       public T VisitUnaryExpr(Expr.Unary expr);
       public T VisitBinaryExpr(Expr.Binary expr);
    }
}
