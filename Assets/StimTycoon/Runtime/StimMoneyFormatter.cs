using System.Globalization;

namespace StimTycoon.Runtime
{
    public static class StimMoneyFormatter
    {
        private static readonly CultureInfo UsDollarCulture = CultureInfo.GetCultureInfo("en-US");

        public static string Format(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C0", UsDollarCulture);
        }

        public static string FormatPrecise(long minorUnits)
        {
            return (minorUnits / 100m).ToString("C2", UsDollarCulture);
        }
    }
}
