

namespace lox.src
{
    public static class Extensions
    {
        public static object get(this Dictionary<string, object> dictionary, string key)
        {
            if(dictionary.TryGetValue(key, out object? value))
            {
                return value;
            }
            return null!;
        }
        public static Int32? get(this Dictionary<Expr, Int32> dictionary, Expr key)
        {
            if(dictionary.TryGetValue(key, out int value))
            {
                return value;
            }

            return null;
        }
    }
}
