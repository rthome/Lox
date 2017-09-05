using System.Collections.Generic;

namespace lox
{
    interface ICallable
    {
        int Arity { get; }

        object Call(Interpreter interpreter, List<object> arguments);
    }
}