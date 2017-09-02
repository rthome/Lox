using System;
using System.IO;
using System.Linq;

namespace lox
{
    class Program
    {
        static bool hadError;
        static bool hadRuntimeError;

        static readonly Interpreter interpreter = new Interpreter();

        #region Error Reporting

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Report(token.Line, " at end", message);
            else
                Report(token.Line, " at '" + token.Lexeme + "'", message);
        }

        public static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        public static void RuntimeError(RuntimeException error)
        {
            Console.WriteLine($"{error.Message} [line {error.Token.Line}]");
            hadRuntimeError = true;
        }

        static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        #endregion

        static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var expression = parser.Parse();

            if (hadError)
                return;

            interpreter.Interpret(expression);
        }

        static void RunFile(string path)
        {
            var sourceString = File.ReadAllText(path);
            Run(sourceString);
            if (hadError)
                Environment.Exit(65);
            if (hadRuntimeError)
                Environment.Exit(70);
        }

        static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 1)
                Console.WriteLine("Usage: lox [script]");
            else if (args.Length == 1)
                RunFile(args.Single());
            else
                RunPrompt();
        }
    }
}
