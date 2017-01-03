using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Genetics;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Provides a base class for simulations used with the genetic algorithm in the system.
    /// </summary>
    public abstract class EvolvableSimulation
    {
        protected Random Random;
        public int TickLimit { get; private set; }
        public int TickCount { get; private set; }
        public double TickTime { get; private set; }
        protected readonly IList<SimulationStatistic> Statistics;

        protected EvolvableSimulation(int tickLimit, double tickTime)
        {
            TickLimit = tickLimit;
            TickTime = tickTime;
            Statistics = new List<SimulationStatistic>();
        }

        /// <summary>
        /// Initialises a simulation for use with a selection of chromosomes, with the random seed provided.
        /// </summary>
        /// <param name="chromosomes">The selection of chromosomes.</param>
        /// <param name="randomSeed">The random seed.</param>
        public void Initialise(Chromosome[] chromosomes, int randomSeed)
        {
            Random = new Random(randomSeed); // Initialise the random number generator for this simulation.
            Initialise(chromosomes);
        }

        /// <summary>
        /// Performs implementation-level initialisation of the simulation for the given set of chromosomes.
        /// </summary>
        /// <param name="chromosomes">The selection of chromosomes.</param>
        protected abstract void Initialise(Chromosome[] chromosomes);

        /// <summary>
        /// Executes one tick of the simulation and returns whether to continue.
        /// Delegates implementation-level logic to the PerformTickLogic method.
        /// </summary>
        /// <returns>Whether to continue to another tick.</returns>
        public bool Tick()
        {
            if (TickCount == TickLimit)
                return false;

            var continueSimulation = PerformTickLogic();
            TickCount++;
            return continueSimulation;
        }
        
        /// <summary>
        /// Performs the implementation-level logic for each tick of the simulation.
        /// </summary>
        /// <returns>Whether to continue to another tick.</returns>
        protected abstract bool PerformTickLogic();

        /// <summary>
        /// Returns the names of all the statistics that a given simulation type records.
        /// </summary>
        /// <param name="simulationType">The Type object for the EvolvableSimulation.</param>
        /// <returns>An array of the statistics' names.</returns>
        public static string[] GetStatisticNames(Type simulationType)
        {
            var prototypeInstance = Activator.CreateInstance(simulationType) as EvolvableSimulation;
            if (prototypeInstance == null)
                throw new Exception(simulationType.Name + " is not a subclass of EvolvableSimulation.");
            return (from statistic in prototypeInstance.Statistics select statistic.Name).ToArray();
        }

        /// <summary>
        /// Measures each simulation statistic for each individual in the simulation.
        /// </summary>
        /// <returns>A multidimensional array where each element [s][i] is the value of statistic s for individual i in the simulation.</returns>
        public double[][] MeasureStatistics()
        {
            return (from statistic in Statistics select statistic.Measure()).ToArray();
        }
    }
}