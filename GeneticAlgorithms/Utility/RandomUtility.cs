using System;

namespace GeneticAlgorithms.Utility
{
    /// <summary>
    /// Provides utility methods for simple random operations.
    /// </summary>
    public static class RandomUtility
    {
        /// <summary>
        /// A static random number generator used when seeding doesn't matter/determinism isn't required.
        /// </summary>
        public static Random Generator { get; private set; }

        /// <summary>
        /// Statically initialises the random number generator, using the current time as the seed by default.
        /// </summary>
        static RandomUtility()
        {
            Generator = new Random();
        }

        /// <summary>
        /// Returns either true or false, each with equal probability.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <returns>Either true or false.</returns>
        public static bool CoinToss(this Random random)
        {
            return random.Next(2) == 1;
        }

        /// <summary>
        /// Performs a roulette wheel selection - probabilistically choosing an index from a list of weights.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="weights">The list of weights representing fractions of 1 that are covered by the given indices. Must sum to 1.</param>
        /// <returns>The chosen index.</returns>
        public static int ChooseIndexFromWeighted(this Random random, double[] weights)
        {
            // Obtain a choice value between 0 and 1.
            var choice = random.NextDouble();

            double cumulative = 0;
            var i = -1;
            // Keep adding the next weight to the accumulation until it exceeds the choice value.
            while (cumulative < choice)
                cumulative += weights[++i];

            // If choice is exactly 0, no iteration will be performed and the index will stay at -1; it should be corrected to 0.
            return i != -1 ? i : 0;
        }

        /// <summary>
        /// Returns a random value between the specified minimum and maximum.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The exclusive maximum value.</param>
        /// <returns>A value greater than or equal to min, and less than max.</returns>
        public static double DoubleInRange(this Random random, double min, double max)
        {
            return min + random.NextDouble()*(max - min);
        }
    }
}