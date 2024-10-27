using lox.src.Interfaces;

namespace lox.src
{
    internal class LoxFunction(Stmt.Function _declaration, LoxEnvironment _closure,bool _is_initializer) : ILoxCallable
    {
        public readonly Stmt.Function declaration = _declaration;
        public readonly LoxEnvironment closure = _closure;
        public readonly bool is_initializer = _is_initializer;
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
                if (is_initializer) return closure.GetAt(0, "this");
                return r.value;
            }

            return null!;
        }
        
        public LoxFunction Bind(LoxInstance instance)
        {
            LoxEnvironment env = new (closure);
            env.Define("this", instance);
            return new LoxFunction(declaration, env, is_initializer);
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}
