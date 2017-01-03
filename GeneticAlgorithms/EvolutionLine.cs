using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Represents an evolutionary run, from the first generation up to the current population.
    /// </summary>
    public class EvolutionLine
    {
        public Population CurrentPopulation { get; protected set; }
        /// <summary>
        /// The Type object for the EvolvableSimulation implementation used in the evolutionary run.
        /// </summary>
        public Type SimulationType { get; private set; }
        protected EvolutionSettingsAttribute Settings;
        public List<GenerationInfo> Generations { get; private set; }
        protected readonly List<Population> Populations;
        protected ChromosomeDefinition ChromosomeDefinition;
        protected int NextChromosomeIndex;
        public string[] StatisticNames { get; private set; }

        /// <summary>
        /// Gets the GenerationInfo for a given generation index.
        /// </summary>
        /// <param name="index">The generation index.</param>
        /// <returns>The GenerationInfo object for the specified generation.</returns>
        public GenerationInfo this[int index]
        {
            get { return Generations[index]; }
        }

        /// <summary>
        /// Gets the number of generations in the EvolutionLine.
        /// </summary>
        public int Count
        {
            get { return Generations.Count; }
        }

        /// <summary>
        /// Constructs an EvolutionLine with the given initial population size and simulation type reference.
        /// </summary>
        /// <param name="initialPopulationSize">The initial population size.</param>
        /// <param name="simulationType">The Type object for the EvolvableSimulation implementation.</param>
        public EvolutionLine(int initialPopulationSize, Type simulationType)
            : this() // Defer list construction to parameterless constructor.
        {
            Initialise(simulationType);
            CurrentPopulation = new Population(initialPopulationSize, ChromosomeDefinition);
        }

        // Construct the relevant lists.
        protected EvolutionLine()
        {
            Generations = new List<GenerationInfo>();
            Populations = new List<Population>();
        }

        /// <summary>
        /// Initialises aspects of the EvolutionLine related to the EvolvableSimulation implementation type.
        /// </summary>
        /// <param name="simulationType">The Type object to initialise the line for.</param>
        protected void Initialise(Type simulationType)
        {
            SimulationType = simulationType;
            StatisticNames = EvolvableSimulation.GetStatisticNames(SimulationType);
            Settings = SimulationType.GetAttribute<EvolutionSettingsAttribute>();
            ChromosomeDefinition = ChromosomeDefinition.Retrieve(Settings.ChromosomeType);
        }

        /// <summary>
        /// Gets the Population for a given generation index.
        /// </summary>
        /// <param name="generationIndex">The generation index.</param>
        /// <returns>The specified generation's Population.</returns>
        public virtual Population GetPopulation(int generationIndex)
        {
            return Populations[generationIndex];
        }

        /// <summary>
        /// Adds a population to the EvolutionLine as a new generation, with the given random seed and recorded statistics.
        /// </summary>
        /// <param name="randomSeed">The random seed for the new generation.</param>
        /// <param name="statistics">The statistics recorded for the new generation.</param>
        /// <param name="nextPopulation">The Population to add.</param>
        public void AddGeneration(int randomSeed, double[][][] statistics, Population nextPopulation)
        {
            Generations.Add(new GenerationInfo(randomSeed, CurrentPopulation.Size, statistics, NextChromosomeIndex));
            NextChromosomeIndex += CurrentPopulation.Size;
            Populations.Add(CurrentPopulation);
            CurrentPopulation = nextPopulation;
        }

        /// <summary>
        /// Gets the index of the slice in which the specified individual is involved.
        /// </summary>
        /// <param name="individualIndex">The index of the individual.</param>
        /// <returns>The slice index for that individual.</returns>
        public int GetSliceForIndividual(int individualIndex)
        {
            return individualIndex/Settings.SliceSize;
        }

        /// <summary>
        /// Encapsulates information about a generation to associate a Population with its place in an EvolutionLine.
        /// </summary>
        public class GenerationInfo
        {
            /// <summary>
            /// The random seed used for simulations for this generation.
            /// </summary>
            public int RandomSeed { get; private set; }
            /// <summary>
            /// The size of this generation's population.
            /// </summary>
            public int PopulationSize { get; private set; }
            /// <summary>
            /// The statistics recorded during the simulations of this generation.
            /// </summary>
            public double[][][] Statistics { get; private set; }
            /// <summary>
            /// The index into the EvolutionLines chromosomes that this generation's population starts.
            /// </summary>
            public int FirstChromosomeIndex { get; private set; }

            public GenerationInfo(int randomSeed, int populationSize, double[][][] statistics, int firstChromosomeIndex)
            {
                RandomSeed = randomSeed;
                PopulationSize = populationSize;
                Statistics = statistics;
                FirstChromosomeIndex = firstChromosomeIndex;
            }

            /// <summary>
            /// Returns individual based statistics for this generation, averaged over all the repeats.
            /// </summary>
            /// <returns>An enumerable of an enumerable. The jth element of the ith enumerable is the average of statistic i for individual j.</returns>
            public IEnumerable<IEnumerable<double>> GetStatisticsForIndividuals()
            {
                return from statistic in Statistics select (from individual in statistic select individual.Average());
            }
        }
    }
}