using System.Collections.Generic;

namespace lox
{
    class Function : ICallable
    {
        readonly Stmt.Function declaration;
        readonly Environment closure;

        public int Arity => declaration.Parameters.Count;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var env = new Environment(closure);
            for (int i = 0; i < declaration.Parameters.Count; i++)
                env.Define(declaration.Parameters[i].Lexeme, arguments[i]);

            try
            {
                interpreter.ExecuteBlock(declaration.Body, env);
            }
            catch (ReturnStatement returnValue)
            {
                return returnValue.Value;
            }
            return null;
        }

        public override string ToString() => $"<fun {declaration.Name.Lexeme}>";

        public Function(Stmt.Function declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }
    }
}
