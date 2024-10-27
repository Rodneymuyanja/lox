
using lox.src.Interfaces;

namespace lox.src
{
    internal class LoxClass(string _name, Dictionary<string, LoxFunction> _methods) : ILoxCallable
    {
        public string name = _name;
        public Dictionary<string, LoxFunction> methods = _methods;
        public override string ToString()
        {
            return name;
        }

        public int Arity()
        {
            LoxFunction initializer = FindMethod("_$init");
            if (initializer is not null)
            {
                return initializer.Arity();
            }

            return 0;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            LoxInstance instance = new (this);
            LoxFunction initializer = FindMethod("_$init");

            initializer?.Bind(instance).Call(interpreter,args);

            return instance;
        }

        public LoxFunction FindMethod(string name)
        {
            if(methods.TryGetValue(name, out LoxFunction? func))
            {
                return func;
            }

            return null!;
        }
    }
}
