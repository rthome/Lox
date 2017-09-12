using System.Collections.Generic;

namespace lox
{
    class Environment
    {
        readonly Dictionary<string, object> values = new Dictionary<string, object>();
        readonly Environment parent;

        public void Define(string name, object value) => values[name] = value;

        public void AssignAt(int distance, Token name, object value)
        {
            var environment = this;
            for (int i = 0; i < distance; i++)
                environment = environment.parent;
            environment.values[name.Lexeme] = value;
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
                values[name.Lexeme] = value;
            else if (parent != null)
                parent.Assign(name, value);
            else
                throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object LookupAt(int distance, string name)
        {
            var environment = this;
            for (int i = 0; i < distance; i++)
                environment = environment.parent;
            return environment.values[name];
        }

        public object Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return value;
            if (parent != null)
                return parent.Lookup(name);
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public Environment()
            : this(null)
        {
        }

        public Environment(Environment parent)
        {
            this.parent = parent;
        }
    }
}
