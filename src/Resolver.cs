
namespace lox.src
{
    internal class Resolver(Interpreter _interpreter) : Stmt.IVisitor<Object>, Expr.IVisitor<Object>
    {
        private readonly Interpreter interpreter = _interpreter;
        private Stack<Dictionary<string,bool>> scopes = new ();
        private FunctionType current_function_type = FunctionType.NONE;
        private ClassType current_class_type = ClassType.NONE;
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }
        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }
        private void EndScope()
        {
            scopes.Pop();
        }

        public void Resolve(List<Stmt> stmts) 
        { 
            foreach (Stmt stmt in stmts)
            {
                Resolve(stmt);
            }
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;
            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Variable already exists in this scope with this name");
            }
            //we have declared the variable but it's not
            //ready, hence the false
            scope.Add(name.lexeme, false);

        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            var scope = scopes.Peek();
            scope[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i =scopes.Count-1; i >= 0; i--)
            {
                var scope = scopes.ElementAt(i);
                if (scope.ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        public object VisitAssignmentExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr,expr.name);
            return null!;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.right);
            Resolve(expr.left);
            return null!;
        }

        public object VisitBlock(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null!;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach (var arg in expr.arguments)
            {
                Resolve(arg);
            }

            return null!;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null!;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null!;
        }

        private void ResolveFunction(Stmt.Function stmt, FunctionType function_type) 
        {
            BeginScope();
            FunctionType enclosing_function_type = current_function_type;

            current_function_type = function_type;
            foreach (var token in stmt.parameters)
            {
                Declare(token);
                Define(token);
            }
            EndScope();
            current_function_type = enclosing_function_type;
        }

        public object VisitGroupExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null!;   
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.then_branch);
            if (stmt.else_branch is not null) Resolve(stmt.else_branch);
            return null!;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null!;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.right);
            Resolve(expr.left);
            return null!;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null!;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if(stmt.value is not null)
            {
                if(current_function_type == FunctionType.INITIALIZER)
                {
                    Lox.Error(stmt.Keyword, "_$init() can not return explicit values");
                }

                Resolve(stmt.value);
            }

            return null!;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right); 
            return null!;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            int scopes_count = scopes.Count;
            _ = scopes.Peek().TryGetValue(expr.name.lexeme, out bool is_defined);

            if (scopes_count > 0  && !is_defined) 
            {
                Lox.Error(expr.name, "Can not read a variable in it's own initializer");
            }

            ResolveLocal(expr, expr.name);
            return null!;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if(stmt.initializer is not null)
            {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);
            return null!;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null!;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosing_class_type = current_class_type;
            current_class_type = ClassType.CLASS;

            BeginScope();
            Declare(stmt.name);
            Define(stmt.name);
            
            var scope = scopes.Peek();
            scope.Add("this", true);

            foreach (var function in stmt.methods)
            {
                FunctionType function_type = FunctionType.METHOD;
                ResolveFunction(function, function_type);
            }

            //EndScope();
            current_class_type = enclosing_class_type;
            return null!;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr._object);
            return null!;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr._object);
            Resolve(expr.value);
            return null!;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if(current_class_type == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Can not use 'this' outside a class");
            }

            ResolveLocal(expr,expr.keyword);
            return null!;
        }
    }
}
