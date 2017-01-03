using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.Utility
{
    /// <summary>
    /// Provides utility methods to perform a standard deviation on an enumerable source of doubles.
    /// </summary>
    public static class MathUtility
    {
        /// <summary>
        /// Calculates the standard deviation from the specified mean, over the enumerable list of doubles provided.
        /// </summary>
        /// <param name="source">The double values to calculate the standard deviation for.</param>
        /// <param name="mean">The mean value of the source, from which to calculate the deviations.</param>
        /// <returns>The standard deviation.</returns>
        public static double StandardDeviation(this IEnumerable<double> source, double mean)
        {
            var count = 0;
            var totalSquaredDeviation = 0d;
            foreach (var value in source)
            {
                totalSquaredDeviation += Math.Pow(value - mean, 2);
                count++;
            }
            return Math.Sqrt(totalSquaredDeviation / count);
        }

        /// <summary>
        /// Calculates the standard deviation over the enumerable list of doubles provided and returns the mean in an out parameter.
        /// </summary>
        /// <param name="source">The double values to calculate the standard deviation for.</param>
        /// <param name="mean">The returned mean value of the source.</param>
        /// <returns>The standard deviation.</returns>
        public static double StandardDeviation(this IEnumerable<double> source, out double mean)
        {
            mean = source.Average();
            var count = 0;
            var totalSquaredDeviation = 0d;
            foreach (var value in source)
            {
                totalSquaredDeviation += Math.Pow(value - mean, 2);
                count++;
            }
            return Math.Sqrt(totalSquaredDeviation / count);
        }

        /// <summary>
        /// Calculates the standard deviation over the enumerable list of doubles provided.
        /// </summary>
        /// <param name="source">The double values to calculate the standard deviation for.</param>
        /// <returns>The standard deviation.</returns>
        public static double StandardDeviation(this IEnumerable<double> source)
        {
            return source.StandardDeviation(source.Average());
        }
    }
}