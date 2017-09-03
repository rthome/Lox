using System;
using System.Collections.Generic;
using System.Text;

namespace lox
{
    class Environment
    {
        readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public void Define(string name, object value) => values[name] = value;

        public object Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return value;
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
