
namespace lox.src.Interfaces
{
    internal interface ILoxCallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> args);
    }
}
