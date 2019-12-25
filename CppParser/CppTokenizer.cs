using System;
namespace CppParser
{
    public class CppTokenizer : TokenizerBase
    {
        private bool scan_cpp_directive;    // Keyword after a C preprocessor #
        private Keyword cpp_keyword = new Keyword();
        private Token cpp_token = new Token();

        public override int get_token()        // Return a single token
        {
            char c0 = (char)0, c1 = (char)0, c2 = (char)0;
            Keyword.IdentifierType key;

            for (; ; )
            {
                if (!src.get(ref c0))
                    return 0;

                switch (c0)
                {
                    /*
                     * Single character C operators and punctuators
                     * ANSI 3.1.5 p. 32 and 3.1.6 p. 33
                     */
                    case '\n':
                        bol.saw_newline();
                        break;
                    case ' ':
                    case '\t':
                    case '\v':
                    case '\f':
                    case '\r':
                        break;
                    case '{':
                        bol.saw_non_space();
                        symbols.enter_scope();
                        nesting.saw_open_brace();
                        return (int)c0;
                    case '}':
                        bol.saw_non_space();
                        symbols.exit_scope();
                        nesting.saw_close_brace();
                        return (int)c0;
                    case ';':
                        bol.saw_non_space();
                        /*
                         * class might have been used as a forwar declaration
                         * or an elaborated type.
                         */
                        nesting.unsaw_class();
                        return (int)c0;
                    /*
                     * Double character C tokens with more than 2 different outcomes
                     * (e.g. +, +=, ++)
                     */
                    case '+':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '+':
                                return (int)Token.TokenNum.PLUS_PLUS; // ++
                            case '=':
                                return (int)Token.TokenNum.PLUS_EQUAL; // +=
                            default:
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    case '-':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '-':
                                return (int)Token.TokenNum.MINUS_MINUS; // --
                            case '=':
                                return (int)Token.TokenNum.MINUS_EQUAL; // -=
                            case '>':
                                src.get(ref c2);
                                if (c2 == '*')
                                    return (int)Token.TokenNum.MEMBER_PTR_FROM_OBJECT_PTR; // ->*
                                else
                                {
                                    src.push(ref c2);
                                    return (int)Token.TokenNum.MEMBER_FROM_OBJECT_PTR; // ->
                                }
                            default:
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    case '&':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '&':
                                return (int)Token.TokenNum.BOOLEAN_AND; // &&
                            case '=':
                                return (int)Token.TokenNum.AND_EQUAL; // &=
                            default:
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    case '|':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '|':
                                return (int)Token.TokenNum.BOOLEAN_OR; // ||
                            case '=':
                                return (int)Token.TokenNum.OR_EQUAL; // |=
                            default:
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    /* Simple single/double character tokens (e.g. !, !=) */
                    case '!':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == '=')
                            return (int)Token.TokenNum.NOT_EQUAL; // !=
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case '%':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == '=')
                            return (int)Token.TokenNum.MOD_EQUAL; // %=
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case '*':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == '=')
                            return (int)Token.TokenNum.TIMES_EQUAL; // *=
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case '=':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == '=')
                            return (int)Token.TokenNum.EQUAL; // ==
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case ':':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == ':')
                            return (int)Token.TokenNum.SCOPE; // ::
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case '^':
                        bol.saw_non_space();
                        src.get(ref c1);
                        if (c1 == '=')
                            return (int)Token.TokenNum.XOR_EQUAL; // ^=
                        else
                        {
                            src.push(ref c1);
                            return (int)c0;
                        }
                        break;
                    case '#':
                        src.get(ref c1);
                        if (c1 == '#')
                            return (int)Token.TokenNum.TOKEN_PASTE; // ##
                        else
                            src.push(ref c1);
                        if (bol.at_bol_space())
                            scan_cpp_directive = true;
                        bol.saw_non_space();
                        return (int)c0;
                    /* Operators starting with < or > */
                    case '>':
                        bol.saw_non_space();
                        // class might have been used as a template argument
                        nesting.unsaw_class();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '=':               /* >= */
                                return (int)Token.TokenNum.GREATER_EQUAL; // >=
                            case '>':
                                src.get(ref c1);
                                if (c1 == '=')          /* >>= */
                                    return (int)Token.TokenNum.RSHIFT_EQUAL; // >>=
                                else
                                {          /* << */
                                    src.push(ref c1);
                                    return (int)Token.TokenNum.RSHIFT; // >>
                                }
                                break;
                            default:                /* > */
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    case '<':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '=':               /* <= */
                                return (int)Token.TokenNum.LESS_EQUAL; // <=
                            case '<':
                                src.get(ref c1);
                                if (c1 == '=')          /* <<= */
                                    return (int)Token.TokenNum.LSHIFT_EQUAL; // <<=
                                else
                                {          /* << */
                                    src.push(ref c1);
                                    return (int)Token.TokenNum.LSHIFT; // <<
                                }
                                break;
                            default:                /* < */
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    /* Comments and / operators */
                    case '/':
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '=':               /* /= */
                                return (int)Token.TokenNum.DIV_EQUAL; // /=
                            case '*':               /* Block comment */
                                c2 = src.char_after();
                                if (process_block_comment())
                                {
                                    if (c2 == '*')
                                        return (int)Token.TokenNum.JAVADOC_COMMENT; // /**...*/
                                    else
                                        return (int)Token.TokenNum.BLOCK_COMMENT; // /*...*/

                                }
                                else
                                    return 0;
                                break;
                            case '/':               /* Line comment */
                                if (process_line_comment())
                                    return (int)Token.TokenNum.LINE_COMMENT; // //...
                                else
                                    return 0;
                                break;
                            default:                /* / */
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    case '.':   /* . .* ... */
                        bol.saw_non_space();
                        src.get(ref c1);
                        switch (c1)
                        {
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                val = "." + (char)(c1);
                                return process_number(val);
                            case '.':
                                src.get(ref c2);
                                if (c2 == '.')
                                    return (int)Token.TokenNum.ELIPSIS; // ...
                                else
                                {
                                    src.push(ref c2);
                                    src.push(ref c1);
                                    return (int)c0;
                                }
                                break;
                            case '*':
                                return (int)Token.TokenNum.MEMBER_PTR_FROM_OBJECT; // .*
                            default:
                                src.push(ref c1);
                                return (int)c0;
                        }
                        break;
                    /* XXX Can also be non-ASCII */
                    case '_':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                        bol.saw_non_space();
                        val = c0.ToString();
                        for (; ; )
                        {
                            src.get(ref c0);
                            if (!char.IsLetterOrDigit(c0) && c0 != '_')
                                break;
                            val += c0;
                        }
                        src.push(ref c0);
                        key = cpp_keyword.identifier_type(val);
                        switch (key)
                        {
                            case Keyword.IdentifierType.K_define:
                            case Keyword.IdentifierType.K_elif:
                            case Keyword.IdentifierType.K_endif:
                            case Keyword.IdentifierType.K_error:
                            case Keyword.IdentifierType.K_ifdef:
                            case Keyword.IdentifierType.K_ifndef:
                            case Keyword.IdentifierType.K_include:
                            case Keyword.IdentifierType.K_line:
                            case Keyword.IdentifierType.K_pragma:
                            case Keyword.IdentifierType.K_undef:
                                if (scan_cpp_directive)
                                {
                                    scan_cpp_directive = false;
                                    return (int)key;
                                }
                                else
                                    return symbols.value(val);
                                break;
                            case Keyword.IdentifierType.IDENTIFIER:
                                return symbols.value(val);
                            case Keyword.IdentifierType.K_class:
                            case Keyword.IdentifierType.K_struct:
                                nesting.saw_class();
                                return (int)key;
                            // Alternative representations of standard tokens
                            case Keyword.IdentifierType.K_and:
                                return (int)Token.TokenNum.BOOLEAN_AND; // &&
                            case Keyword.IdentifierType.K_bitor:
                                return '|';
                            case Keyword.IdentifierType.K_or:
                                return (int)Token.TokenNum.BOOLEAN_OR; // ||
                            case Keyword.IdentifierType.K_xor:
                                return '^';
                            case Keyword.IdentifierType.K_compl:
                                return '~';
                            case Keyword.IdentifierType.K_bitand:
                                return '&';
                            case Keyword.IdentifierType.K_and_eq:
                                return (int)Token.TokenNum.AND_EQUAL; // &=
                            case Keyword.IdentifierType.K_or_eq:
                                return (int)Token.TokenNum.OR_EQUAL; // |=
                            case Keyword.IdentifierType.K_xor_eq:
                                return (int)Token.TokenNum.XOR_EQUAL; // ^=
                            case Keyword.IdentifierType.K_not:
                                return '!';
                            case Keyword.IdentifierType.K_not_eq:
                                return (int)Token.TokenNum.NOT_EQUAL; // !=
                            default:
                                return (int)key;
                        }
                        break;
                    case '\'':
                        bol.saw_non_space();
                        if (process_char_literal())
                            return (int)Token.TokenNum.CHAR_LITERAL; // '.'
                        else
                            return 0;
                    case '"':
                        bol.saw_non_space();
                        if (process_string_literal())
                            return (int)Token.TokenNum.STRING_LITERAL; // \"...\"
                        else
                            return 0;
                    /* Various numbers */
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        bol.saw_non_space();
                        val = c0.ToString();
                        return process_number(val);
                    default:
                        bol.saw_non_space();
                        return (int)(c0);
                }
            }
        }

        public override string keyword_to_string(int k)
        {
            return cpp_keyword.to_string(k);
        }

        public override string token_to_string(int k)
        {
            return cpp_token.to_string(k);
        }

        public override string token_to_symbol(int k)
        {
            return cpp_token.to_symbol(k);
        }

        // Construct from a character source
        public CppTokenizer(CharSource s, string file_name,
                System.Collections.Generic.List<string> opt = null) :
        base(s, file_name, opt)
        {
            scan_cpp_directive = false;
        }

        // Construct for a string source
        public CppTokenizer(string s, System.Collections.Generic.List<string> opt = null) :
            base(s, opt)
        {
            scan_cpp_directive = false;
        }
    }
}
