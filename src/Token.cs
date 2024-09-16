
namespace lox.src
{
    internal class Token(TokenType _type, string _lexeme, object? _literal, int _line)
    {
        TokenType token_type = _type;
        string lexeme = _lexeme;
        object literal = _literal!;   
        int line = _line;

        public override string ToString()
        {
            return $"{token_type} {lexeme} {lexeme}";
        }
    }
}
