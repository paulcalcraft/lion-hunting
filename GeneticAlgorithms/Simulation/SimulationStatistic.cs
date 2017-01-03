using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Represents a recorded statistic for an EvolvableSimulation.
    /// </summary>
    public class SimulationStatistic
    {
        public string Name { get; private set; }
        // A measurer function with return type double[], used to return the value for each individual.
        private readonly Func<double[]> _measurer;

        /// <summary>
        /// Defines a statistic with the given name and measuring function.
        /// </summary>
        /// <param name="name">The name of the statistic.</param>
        /// <param name="measurer">The measuring function that returns the statistic value for each individual in the simulation as an array.</param>
        private SimulationStatistic(string name, Func<double[]> measurer)
        {
            Name = name;
            _measurer = measurer;
        }

        /// <summary>
        /// Defines a statistic with the given name and measuring function.
        /// Used for programmatic brevity at the call site, so the ToArray call is centralised instead of repeated at every instantiation.
        /// </summary>
        /// <param name="name">The name of the statistic.</param>
        /// <param name="measurer">The measuring function that returns the statistic value for each individual in the simulation as an enumerable list of doubles.</param>
        public SimulationStatistic(string name, Func<IEnumerable<double>> measurer)
            : this(name, () => measurer().ToArray())
        {
        }

        /// <summary>
        /// Performs the measurement using the stored function.
        /// </summary>
        /// <returns>The value of the statistic for each individual in the simulation.</returns>
        public double[] Measure()
        {
            return _measurer();
        }
    }
}