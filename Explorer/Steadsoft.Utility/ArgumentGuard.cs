using System;

namespace Steadsoft.Utility
{
    public static class ArgumentGuard
    {
        public static void ThrowIfNull (this object Arg, string Name)
        {
            if (Arg == null)
                throw new ArgumentNullException(Name);
        }

        public static void ThrowIfGreaterThan(this int Arg, int Max, string Name)
        {
            if (Arg > Max)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be greater than {Max}.");
        }

        public static void ThrowIfZero(this int Arg, string Name)
        {
            if (Arg == 0)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be zero.");
        }

        public static void ThrowIfGreaterThanZero(this int Arg, string Name)
        {
            if (Arg > 0)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be greater than zero.");
        }

        public static void ThrowIfGreaterThanOrEqualToZero(this int Arg, string Name)
        {
            if (Arg >= 0)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be greater than or equal to zero.");
        }

        public static void ThrowIfLessThanZero(this int Arg, string Name)
        {
            if (Arg < 0)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be less than zero.");
        }

        public static void ThrowIfLessThanOrEqualToZero(this int Arg, string Name)
        {
            if (Arg <= 0)
                throw new ArgumentOutOfRangeException(Name, $"The value must not be less than or equal to zero.");
        }



    }
}
