using System;
using System.Text;

namespace BBUnity.Support.Conversion.Internal {

    internal static class NumberToRomanNumerals {
        private static readonly int[]    Values  = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        private static readonly string[] Symbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        internal static string Convert(int number) {
            var result = new StringBuilder();

            for(int i = 0; i < Values.Length; i++) {
                while(number >= Values[i]) {
                    result.Append(Symbols[i]);
                    number -= Values[i];
                }
            }

            return result.ToString();
        }
    }
}
