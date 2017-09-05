using System;

namespace lox
{
    class ReturnValue : Exception
    {
        public object Value { get; private set; }

        public ReturnValue(object value)
        {
            Value = value;
        }
    }
}
