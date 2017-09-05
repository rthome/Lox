using System;

namespace lox
{
    class ReturnStatement : Exception
    {
        public object Value { get; private set; }

        public ReturnStatement(object value)
        {
            Value = value;
        }
    }
}
