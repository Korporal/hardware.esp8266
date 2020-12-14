namespace Steadsoft.ESP8266
{
    using static Constants.Chars;

    public static class Constants
    {
        public static class Chars
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

        public static readonly byte[] CRLF = { CR, LF };
        public static readonly byte[] IPD =  { PLUS, I, P, D, COMMA };
    }
}
