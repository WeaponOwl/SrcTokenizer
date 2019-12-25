using System;
namespace CppParser
{
    public class TokenizerBase
    {
        private bool previously_in_method;
        private void delimit(string s, int c)
        {
            switch (processing_type)
            {
                case ProcessingType.PT_FILE:
                    Console.Write(s);
                    Console.Write(' ');
                    break;
                case ProcessingType.PT_METHOD:
                    if (previously_in_method && !nesting.in_method())
                        Console.WriteLine(s);
                    if (nesting.in_method())
                    {
                        Console.Write(s);
                        Console.Write(' ');
                    }
                    break;
                case ProcessingType.PT_STATEMENT:
                    if (previously_in_method && !nesting.in_method())
                        Console.WriteLine(s);
                    if (nesting.in_method())
                    {
                        if (c == ';')
                            Console.WriteLine(s);
                        else
                        {
                            Console.Write(s);
                            Console.Write(' ');
                        }
                    }
                    break;
            }
            previously_in_method = nesting.in_method();
        }

        protected string string_src;   // Source for testing
        protected CharSource src;         // Character source
                                          /** True for keywords that don't end with semicolon */
        protected bool saw_comment;       // True after a comment
        protected BolState bol = new BolState();           // Beginning of line state
        protected string input_file;     // Input file name
        protected string val;        // Token value (ids, strings, nums, ...)
                                     // Report an error message
        protected void error(string msg)
        {
            Console.Error.Write(input_file);
            Console.Error.Write('(');
            Console.Error.Write(src.line_number());
            Console.Error.Write("): ");
            Console.Error.WriteLine(msg);
        }
        protected enum ProcessingType
        {
            PT_FILE,        // Output vector for whole class
            PT_METHOD,      // Output vector for each method
            PT_STATEMENT,       // Output vector for each statement
        }
        ProcessingType processing_type;

        protected void process_options(System.Collections.Generic.List<string> opt)
        {
            foreach (var o in opt)
            {
                if (o == "file")
                    processing_type = ProcessingType.PT_FILE;
                else if (o == "method")
                    processing_type = ProcessingType.PT_METHOD;
                else if (o == "statement")
                    processing_type = ProcessingType.PT_STATEMENT;
                else
                {
                    Console.Error.Write("Unsupported processing option [");
                    Console.Error.Write(o);
                    Console.Error.WriteLine("]");
                    Console.Error.WriteLine("Valid options are one of file, method, statement");
                    Environment.Exit(1);
                }
            }
        }

        protected ProcessingType get_processing_type()
        {
            return processing_type;
        }
        protected SymbolTable symbols = new SymbolTable();
        protected NestedClassState nesting = new NestedClassState();

        public virtual int get_token() { return 0; }    // Return a single token
        public virtual string keyword_to_string(int k) { return ""; }
        public virtual string token_to_string(int k) { return ""; }
        public virtual string token_to_symbol(int k) { return ""; }
        public void numeric_tokenize()    // Tokenize numbers to stdout
        {
            int c;

            previously_in_method = false;
            while ((c = get_token())!=0)
            {
                switch (processing_type)
                {
                    case ProcessingType.PT_FILE:
                        Console.Write(c);
                        Console.Write('\t');
                        break;
                    case ProcessingType.PT_METHOD:
                        if (previously_in_method && !nesting.in_method())
                            Console.WriteLine(c);
                        if (nesting.in_method())
                        {
                            Console.Write(c);
                            Console.Write('\t');
                        }
                        break;
                    case ProcessingType.PT_STATEMENT:
                        if (previously_in_method && !nesting.in_method())
                            Console.WriteLine(c);
                        if (nesting.in_method())
                        {
                            if (c == ';')
                                Console.WriteLine(c);
                            else
                            {
                                Console.Write(c);
                                Console.Write('\t');
                            }
                        }
                        break;
                }
                previously_in_method = nesting.in_method();
            }

            Console.WriteLine("");
        }
        public void symbolic_tokenize()   // Tokenize symbols to stdout
        {
            int c;

            previously_in_method = false;
            while ((c = get_token())!=0)
            {
                System.Text.StringBuilder os = new System.Text.StringBuilder();

                if (TokenId.is_character(c))
                    os.Append((char)c);
                else if (TokenId.is_keyword(c))
                    os.Append(keyword_to_string(c));
                else if (TokenId.is_other_token(c))
                    os.Append(token_to_string(c));
                else if (TokenId.is_zero(c))
                    os.Append("0");
                else if (TokenId.is_number(c))
                {
                    os.Append("~1E");
                    os.Append(c - TokenId.NUMBER_ZERO);
                }
                else if (TokenId.is_identifier(c))
                {
                    os.Append("ID:");
                    os.Append(c);
                }
                else
                    throw new Exception();

                delimit(os.ToString(), c);
            }

            Console.WriteLine("");
        }
        public void code_tokenize()       // Tokenize code to stdout
        {
            int c;

            while ((c = get_token()) != 0)
            {
                if (TokenId.is_character(c) & !char.IsWhiteSpace((char)c))
                    Console.Write((char)c);
                else if (TokenId.is_keyword(c))
                    Console.Write(keyword_to_string(c));
                else if (TokenId.is_other_token(c))
                    Console.Write(token_to_symbol(c));
                else if (TokenId.is_zero(c))
                    Console.Write("0");
                else if (TokenId.is_number(c))
                    Console.Write(get_value());
                else if (TokenId.is_identifier(c))
                    Console.Write(get_value());
                else
                    throw new Exception();

                Console.WriteLine("");
            }
        }
        public void type_tokenize()       // Tokenize token types to stdout
        {
            int c;

            previously_in_method = false;
            while ((c = get_token())!=0)
            {
                System.Text.StringBuilder os = new System.Text.StringBuilder();

                if (TokenId.is_character(c))
                    os.Append((char)c);
                else if (TokenId.is_keyword(c))
                    os.Append(keyword_to_string(c));
                else if (TokenId.is_other_token(c))
                    os.Append(token_to_string(c));
                else if (TokenId.is_zero(c) || TokenId.is_number(c))
                    os.Append("NUM");
                else if (TokenId.is_identifier(c))
                    os.Append("ID");
                else
                    throw new Exception();

                delimit(os.ToString(), c);
            }

            Console.WriteLine("");
        }

        public void type_code_tokenize()  // Tokenize token code and its type to stdoit
        {
            int c;

            while ((c = get_token()) != 0)
            {
                if (TokenId.is_character(c) & !char.IsWhiteSpace((char)c))
                {
                    Console.Write("TOK ");
                    Console.Write((char)c);
                }
                else if (TokenId.is_keyword(c))
                {
                    Console.Write("KW ");
                    Console.Write(keyword_to_string(c));
                }
                else if (TokenId.is_other_token(c))
                {
                    Console.Write("TOK ");
                    Console.Write(token_to_symbol(c));
                }
                else if (TokenId.is_zero(c))
                {
                    Console.Write("NUM 0");
                }
                else if (TokenId.is_number(c))
                {
                    Console.Write("NUM ");
                    Console.Write(get_value());
                }
                else if (TokenId.is_identifier(c))
                {
                    Console.Write("ID ");
                    Console.Write(get_value());
                }
                else
                    throw new Exception();

                Console.WriteLine("");
            }
        }

        // Construct from a character source
        public TokenizerBase(CharSource s, string file_name,
            System.Collections.Generic.List<string> opt = null)
        {
            src = s;
            saw_comment = false;
            input_file = file_name;
            processing_type = ProcessingType.PT_FILE;
            process_options(opt != null ? opt : new System.Collections.Generic.List<string>());
        }

        // Construct for a string source
        public TokenizerBase(string s,
            System.Collections.Generic.List<string> opt = null)
        {
            string_src = s;

            byte[] stringBytes = System.Text.Encoding.ASCII.GetBytes(string_src);
            var stream = new System.IO.MemoryStream(stringBytes);
            src = new CharSource(stream);
            saw_comment = false;
            input_file = "(string)";
            processing_type = ProcessingType.PT_FILE;
            process_options(opt != null ? opt : new System.Collections.Generic.List<string>());
        }
        
        public static int num_token(ref string val)
        {
            double d = 0;

            if (val.StartsWith("0x"))
            {
                d = Convert.ToInt32(val, 16);
                if (d < 0) d = -d;
            }
            else
            {
                d = double.Parse(val);
            }

            if (double.IsInfinity(d))
                return TokenId.NUMBER_INFINITE;
            if (double.IsNaN(d))
                return TokenId.NUMBER_NAN;
            if (Math.Abs(d) < double.Epsilon)
                return TokenId.NUMBER_ZERO;

            d = Math.Log(d) / Math.Log(10);
            if (d >= 0)
                d = Math.Ceiling(d) + 1;
            if (d < 0)
                d = Math.Floor(d);
            return TokenId.NUMBER_ZERO + (int)d;
        }

        public bool process_block_comment()
        {
            char c1 = (char)0;

            src.get(ref c1);
            for (; ; )
            {
                while (c1 != '*')
                {
                    if (!char.IsWhiteSpace(c1) && bol.at_bol_space())
                        bol.saw_non_space();
                    if (!src.get(ref c1))
                    {
                        error("EOF encountered while processing a block comment");
                        return false;
                    }
                }
                if (!char.IsWhiteSpace(c1) && bol.at_bol_space())
                    bol.saw_non_space();
                if (!src.get(ref c1))
                {
                    error("EOF encountered while processing a block comment");
                    return false;
                }
                if (c1 == '/')
                    break;
            }
            return true;
        }
        public bool process_line_comment()
        {
            char c1 = (char)0;

            src.get(ref c1);
            for (; ; )
            {
                if (c1 == '\n')
                    break;
                if (!src.get(ref c1))
                    return false;
            }
            src.push(ref c1);
            return true;
        }
        public bool process_char_literal()
        {
            char c0 = (char)0;

            for (; ; )
            {
                if (!src.get(ref c0))
                {
                    error("EOF encountered while processing a character literal");
                    return false;
                }
                if (c0 == '\\')
                {
                    // Consume one character after the backslash
                    // ... to deal with the '\'' problem
                    src.get(ref c0);
                    continue;
                }
                if (c0 == '\'')
                    break;
            }
            return true;
        }
        public bool process_string_literal()
        {
            char c0 = (char)0;

            bol.saw_non_space();
            for (; ; )
            {
                if (!src.get(ref c0))
                {
                    error("EOF encountered while processing a string literal");
                    return false;
                }
                if (c0 == '\\')
                {
                    // Consume one character after the backslash
                    src.get(ref c0);
                    continue;
                }
                else if (c0 == '"')
                    break;
            }
            return true;
        }

        public int process_number(string val)
        {
            char c0 = (char)0;

            for (; ; )
            {
                src.get(ref c0);
                if (c0 == 'e' || c0 == 'E')
                {
                    val += c0;
                    src.get(ref c0);
                    if (c0 == '+' || c0 == '-')
                    {
                        val += c0;
                        continue;
                    }
                }
                if (!char.IsLetterOrDigit(c0) && c0 != '.' && c0 != '_')
                    break;
                val += c0;
            }
            src.push(ref c0);
            return num_token(ref val);
        }
        public string get_value() { return val; }
    }
}
