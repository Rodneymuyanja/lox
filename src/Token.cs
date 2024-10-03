
namespace lox.src
{
    public class Token(TokenType _type, string _lexeme, object? _literal, int _line)
    {
        internal TokenType token_type = _type;
        internal string lexeme = _lexeme;
        internal object literal = _literal!;   
        internal int line = _line;

        public override string ToString()
        {
            return $"{token_type} {lexeme} {lexeme}";
        }
    }
}
