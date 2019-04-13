using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Helpers
{
    /// <summary>
    ///  Function Helper
    ///  Source: https://github.com/fsprojects/ExcelFinancialFunctions/issues/15
    /// https://stackoverflow.com/questions/8137391/percentile-calculation
    /// </summary>
    public static class FunctionHelper
    {
        public static double Percentile1(double[] sequence, double excelPercentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * excelPercentile + 1;

            return n;
        }

        public static double Percentile2(double[] sequence, double excelPercentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;

            double n = (N + 1) * excelPercentile;
            if (n == 1d)
                return sequence[0];
            else if (n == N)
                return sequence[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }




        public static double Percentile3(IEnumerable<double> seq, double percentile)
        {
            var elements = seq.ToArray();
            Array.Sort(elements);
            double realIndex = percentile * (elements.Length - 1);
            int index = (int)realIndex;

            double frac = realIndex - index;
            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            else
                return elements[index];
        }

    }
}

