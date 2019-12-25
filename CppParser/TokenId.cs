using System;
namespace CppParser
{
    public class TokenId
    {
        // 0-255 ASCII characters
        public const int CHARACTER = 0;

        // Language's keywords
        public const int KEYWORD = 300;

        // Multi-character operators and other non-keyword tokens (e.g. elipsis)
        public const int OTHER_TOKEN = 600;

        // Numbers on a log_10 scale are centered around this value
        // and extend +-400 around it
        public const int NUMBER_START = 1100;
        public const int NUMBER_ZERO = 1500;
        public const int NUMBER_INFINITE = 1900;
        public const int NUMBER_NAN = 1901;
        public const int NUMBER_END = 1902;

        // Identifiers are dynamically allocated from this number upward
        public const int IDENTIFIER = 2000;

        public static bool is_character(int t) { return t < KEYWORD; }
        public static bool is_keyword(int t) { return t >= KEYWORD && t < OTHER_TOKEN; }
        public static bool is_other_token(int t) { return t >= OTHER_TOKEN && t < NUMBER_START; }
        public static bool is_zero(int t) { return t == NUMBER_ZERO; }
        public static bool is_number(int t) { return t >= NUMBER_START && t < NUMBER_END; }
        public static bool is_identifier(int t) { return t >= IDENTIFIER; }
    }
}
