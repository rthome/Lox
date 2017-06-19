using System;
using System.IO;
using System.Linq;

namespace lox
{
    class Program
    {
        static bool hadError;

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

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        static void RunFile(string path)
        {
            var sourceString = File.ReadAllText(path);
            Run(sourceString);
            if (hadError)
                Environment.Exit(65);
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
