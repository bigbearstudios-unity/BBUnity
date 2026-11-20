using System;
using System.Text;

namespace BBUnity.Support.Conversion.Internal {

    internal static class NumberToRetroScore {
        private static readonly StringBuilder Builder = new StringBuilder(10);

        internal static string Convert(int number, int places) {
            int remainingPlaces = 10 - NumberOfDigits(number);
            places = places < remainingPlaces ? places : remainingPlaces;

            Builder.Clear();

            for(int i = 0; i < places; ++i) {
                Builder.Append("0");
            }

            Builder.Append(number);

            return Builder.ToString();
        }

        private static int NumberOfDigits(int number) {
            if(number < 10)        return 1;
            if(number < 100)       return 2;
            if(number < 1000)      return 3;
            if(number < 10000)     return 4;
            if(number < 100000)    return 5;
            if(number < 1000000)   return 6;
            if(number < 10000000)  return 7;
            if(number < 100000000) return 8;
            if(number < 1000000000) return 9;

            return 10;
        }
    }
}
