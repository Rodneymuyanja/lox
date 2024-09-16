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
        private static bool had_error = false;
        public static void Main()
        {
            Console.WriteLine("started lox");
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine("args {0}", string.Join(',',args));
            if (args.Length > 2)
            {
                Environment.ExitCode = ERROR_BAD_ARGS;
            }

            if (args.Length > 1)
            {
                RunPrompt();
            }

            if(args.Length == 1)
            {
                RunFile(args[0]);
            }
        }

        private static void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string file_content = Encoding.UTF8.GetString(bytes);
            Run(file_content);

            if(had_error) Environment.Exit(SYNTAX_ERROR);
        }

        private static void RunPrompt()
        {
            for(;;)
            {
                Console.WriteLine("lox1.0>");
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
            List<string> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
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
    }
}