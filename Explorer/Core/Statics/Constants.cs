namespace Steadsoft.ESP8266
{
    using static Constants.Bytes;

    public static class Constants
    {
        public static class Bytes
        {
            public const byte CR = 0x0D;
            public const byte LF = 0x0A;
            public const byte PLUS = 0x2B;
            public const byte I = 0x49;
            public const byte P = 0x50;
            public const byte D = 0x44;
            public const byte COMMA = 0x2C;
            public const byte COLON = 0x3A;
        }

        public static class Chars
        {
            public const char COMMA = ',';
            public const char COLON = ':';
            public const char CR = '\r';
            public const char LF = '\n';
            public const char LPAR = '(';
            public const char RPAR = ')';
            public const char QUOTE = '"';
        }

        public static readonly byte[] CRLF = { CR, LF };
        public static readonly byte[] IPD =  { PLUS, I, P, D, COMMA };
    }
}
