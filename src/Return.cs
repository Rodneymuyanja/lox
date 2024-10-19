

namespace lox.src
{
    internal class Return(object _value) : Exception
    {
        public object value = _value;
    }
}
