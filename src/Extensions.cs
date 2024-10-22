

namespace lox.src
{
    public static class Extensions
    {
        public static object get(this Dictionary<string, object> dictionary, string key)
        {
            return dictionary[key];
        }
        public static int get(this Dictionary<Expr, Int32> dictionary, Expr key)
        {
            return dictionary[key];
        }
    }
}
