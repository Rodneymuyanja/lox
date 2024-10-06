
namespace lox.src
{
    public class RuntimeError(Token _token, String message) : Exception(message)
    {
        internal Token token = _token;
    }
}
