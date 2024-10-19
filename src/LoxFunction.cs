using lox.src.Interfaces;

namespace lox.src
{
    internal class LoxFunction(Stmt.Function _declaration, LoxEnvironment _closure) : ILoxCallable
    {
        private readonly Stmt.Function declaration = _declaration;
        private readonly LoxEnvironment closure = _closure;
        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            LoxEnvironment env = new(closure);
            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                env.Define(declaration.parameters[i].lexeme, args[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, env);
            }
            catch (Return r)
            {
                return r.value;
            }

            return null!;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}
