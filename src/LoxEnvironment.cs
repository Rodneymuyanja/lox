
namespace lox.src
{
    public class LoxEnvironment
    {
        private Dictionary<string, object> values = [];
        LoxEnvironment? _enclosing;

        public LoxEnvironment(LoxEnvironment enclosing_env)
        {
            _enclosing = enclosing_env;
        }
        public LoxEnvironment() { }

        public void Define(string key, object value)
        {
            //this is a semantic choice ensuring that variables can be redefined
            if(values.ContainsKey(key))
            {
                values[key] = value;
            }

            values.Add(key, value);
        }

        public object Get(Token name)
        {
            if(values.TryGetValue(name.lexeme, out object? value)) return value;

            //check the enclosing environment next
            if (_enclosing is not null) return _enclosing.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'");
        }

        public void Assign(Token name, object value)
        { 
            if(!values.ContainsKey(name.lexeme))
            {
                throw new RuntimeError(name, $"Undefined variable {name.lexeme}");
            }

            values[name.lexeme] = value;
        }
    }
}
