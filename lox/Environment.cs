using System.Collections.Generic;

namespace lox
{
    class Environment
    {
        readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public void Define(string name, object value) => values[name] = value;

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
                values[name.Lexeme] = value;
            else
                throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return value;
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
