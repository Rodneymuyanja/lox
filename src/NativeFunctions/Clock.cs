using lox.src.Interfaces;

namespace lox.src.NativeFunctions
{
    internal class Clock : ILoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> args)
        {
            return DateTime.Now.Second;
        }

        public override string ToString()
        {
            return "<native fn clock>";
        }
    }
}
