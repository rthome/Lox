namespace lox.Util
{
    sealed class Void
    {
        public static Void Value => null;

        private Void() { }
    }
}
