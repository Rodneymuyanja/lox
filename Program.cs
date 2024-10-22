// See https://aka.ms/new-console-template for more information
using lox.src;
using lox;
using System.Text;

string[] args_ = Environment.GetCommandLineArgs();
Console.WriteLine(args_);
Lox.Main();

namespace lox
{
    public class Lox
    {
        private const int ERROR_BAD_ARGS = 0xa0;
        private const int SYNTAX_ERROR = 0xa1;
        private const int RUNTIME_ERROR = 0xa2;
        private static bool had_error = false;
        private static bool had_runtime_error = false;
        private static readonly Interpreter interpreter = new ();
        public static void Main()
        {
            Console.WriteLine("started lox");
            string[] args = Environment.GetCommandLineArgs();

            //Console.WriteLine("args {0}", string.Join(',',args));
            //if (args.Length > 2)
            //{
            //    Environment.ExitCode = ERROR_BAD_ARGS;
            //}

            //if (args.Length == 1)
            //{
            //    RunPrompt();
            //}

            if (args.Length == 2)
            {
                RunFile(args[1]);
            }

            //RunFile(@"C:\disk_d\Programming stuff\#lox\tools_outputs\loxtest.lox");
        }

        private static void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string file_content = Encoding.UTF8.GetString(bytes);
            Run(file_content);

            if(had_error) Environment.Exit(SYNTAX_ERROR);
            if (had_runtime_error) Environment.Exit(RUNTIME_ERROR);
        }

        private static void RunPrompt()
        {
            for(;;)
            {
                Console.Write("lox1.0>");
                string input = Console.ReadLine()!;
                if (string.IsNullOrEmpty(input)) break;
                Run(input);
                //syntax errors should not stop us
                had_error = false;
            }
        }

        private static void Run(string source_code)
        {
            Scanner scanner = new (source_code);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new (tokens);

            if (had_error)
            {
                Console.WriteLine("Error on parsing");
                return;
            }

            List<Stmt> statements = parser.Parse();

            interpreter.Interpret(statements);

            //foreach (var token in tokens)
            //{
            //    Console.WriteLine(token);
            //}
        }

        public static void Error(int line, string message)
        {
            Report(line,"", message);
        }

        private static void Report(int line, string where, string message)
        {
            had_error = true;
            string out_put = $"[line {line}] Error {where} : {message}";
            Console.Error.WriteLine(out_put);
        }

        public static void Error(Token token, string message)
        {
            if(token.token_type == TokenType.EOF)
            {
                Report(token.line, "at end of line", message);
                return;
            }

            Report(token.line, $" at '{token.lexeme}' ", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            string error_message = $"{error.Message}\n[line {error.token.line}]";
            Console.Error.WriteLine(error_message);
            had_runtime_error = true;
        }
    }
}