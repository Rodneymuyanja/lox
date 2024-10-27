
namespace lox.src
{
    internal class LoxInstance(LoxClass _lox_class)
    {
        LoxClass lox_class = _lox_class;
        private Dictionary<string, object> fields = [];
        public override string ToString()
        {
            return $"{lox_class.name} instance";
        }

        public object Get(Token name)
        {
            //look for a field first
            if(fields.TryGetValue(name.lexeme, out object? obj))
            {
                return obj;
            }


            //if you do not find it
            LoxFunction method = lox_class.FindMethod(name.lexeme);
            //return a method instead

            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property {name.lexeme}");
            
        }
        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }
    }
}
