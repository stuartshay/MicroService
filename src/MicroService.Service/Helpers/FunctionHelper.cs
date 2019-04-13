using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Helpers
{
    /// <summary>
    ///  Function Helper
    ///  Source: https://stackoverflow.com/questions/8137391/percentile-calculation
    /// </summary>
    public static class FunctionHelper
    {
        public static double Percentile(IEnumerable<double> seq, double percentile)
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

