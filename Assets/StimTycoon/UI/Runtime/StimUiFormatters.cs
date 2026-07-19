using System;
using System.Globalization;

namespace StimTycoon.UI
{
    public static class StimUiFormatters
    {
        public static string Money(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C0", CultureInfo.CurrentCulture);
        }

        public static string SignedPercent(decimal value)
        {
            var prefix = value >= 0 ? "+" : "−";
            return $"{prefix}{Math.Abs(value):0.#}%";
        }

        public static string ClampedStat(int value)
        {
            var clamped = value < 0 ? 0 : value > 100 ? 100 : value;
            return clamped.ToString(CultureInfo.InvariantCulture);
        }
    }
}
