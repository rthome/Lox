using System;
using System.Collections.Generic;

namespace lox
{
    class NativeFunctions
    {
        public sealed class ClockFunction : ICallable
        {
            public int Arity => 0;

            public object Call(Interpreter interpreter, List<object> arguments) => DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;
        }
    }
}
