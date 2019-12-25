using System;

// Origin: https://github.com/dspinellis/tokenizer

namespace CppParser
{
    class MainClass
    {
        protected static void process_file(string lang, System.Collections.Generic.List<string> opt, string filename, string processing_type)
        {
            CharSource cs = new CharSource();
            TokenizerBase t = null;

            if (lang == "" || lang == "Java")
                throw new NotImplementedException(); //t = new JavaTokenizer(cs, filename, opt);
            else if (lang == "C")
                throw new NotImplementedException(); //t = new CTokenizer(cs, filename, opt);
            else if (lang == "CSharp" || lang == "C#")
                throw new NotImplementedException(); //t = new CSharpTokenizer(cs, filename, opt);
            else if (lang == "C++")
                t = new CppTokenizer(cs, filename, opt);
            else if (lang == "PHP")
                throw new NotImplementedException(); //t = new PHPTokenizer(cs, filename, opt);
            else if (lang == "Python")
                throw new NotImplementedException(); //t = new PythonTokenizer(cs, filename, opt);
            else
            {
                Console.Error.WriteLine("Unknown language specified.");
                Console.Error.WriteLine("The following languages are supported:");
                Console.Error.WriteLine("\tC");
                Console.Error.WriteLine("\tCSharp (or C#)");
                Console.Error.WriteLine("\tC++");
                Console.Error.WriteLine("\tJava");
                Console.Error.WriteLine("\tPHP");
                Console.Error.WriteLine("\tPython");
                Environment.Exit(1);
            }

            switch (processing_type)
            {
                case "c":
                    t.code_tokenize();
                    break;
                case "n":
                    t.numeric_tokenize();
                    break;
                case "s":
                    t.symbolic_tokenize();
                    break;
                case "t":
                    t.type_tokenize();
                    break;
                case "T":
                    t.type_code_tokenize();
                    break;
                default:
                    Console.Error.WriteLine("Unknown processing type specified.");
                    Console.Error.WriteLine("The following processing types are supported:");
                    Console.Error.WriteLine("\tc: output code; one token per line");
                    Console.Error.WriteLine("\tn: output numeric values");
                    Console.Error.WriteLine("\ts: output token symbols");
                    Console.Error.WriteLine("\tt: output token types");
                    Console.Error.WriteLine("\tT: output token types and code; one token per line");
                    Environment.Exit(1);
                    break;
            }
        }

        public static void Main(string[] args)
        {
            int optind = 1;
            string optarg = "";
            string opt = "";

            string lang = "";
            System.Collections.Generic.List<string> processing_opt = new System.Collections.Generic.List<string>();
            string processing_type = "n";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    opt = args[i].Substring(1);
                    optarg = args[i + 1];
                    optind = i + 1;
                }
                else
                {
                    continue;
                }

                switch (opt)
                {
                    case "l":
                        lang = optarg;
                        break;
                    case "o":
                        processing_opt.Add(optarg);
                        break;
                    case "t":
                        processing_type = optarg;
                        break;
                    default: /* ? */
                        Console.Error.WriteLine("Usage: {0} [-l lang] [-o opt] [-t type] [file ...]");
                        Environment.Exit(1);
                        break;
                }
            }

            if (string.IsNullOrEmpty(args[optind]))
            {
                process_file(lang, processing_opt, "-", processing_type);
                Environment.Exit(0);
            }

            // Read from file, if specified
            while (!string.IsNullOrEmpty(args[optind]))
            {
                try
                {
                    var text = System.IO.File.ReadAllText(args[optind]);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Unable to open {0}: {1}", args[optind], e);
                    Environment.Exit(1);
                }

                process_file(lang, processing_opt, args[optind], processing_type);
                optind++;
            }
        }
    }
}
