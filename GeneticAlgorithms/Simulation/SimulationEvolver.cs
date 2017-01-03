using System;
using System.Linq;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Performs a genetic algorithm for a given implementation of EvolvableSimulation.
    /// </summary>
    public class SimulationEvolver
    {
        private readonly IGeneticProbabilityProvider _probabilityProvider;
        private readonly Type _simulationType;
        private readonly EvolutionSettingsAttribute _settings;

        /// <summary>
        /// Instantiates a SimulationEvolver for the given simulation type, with the provided genetic probabilities.
        /// </summary>
        /// <param name="simulationType">The Type object for the EvolvableSimulation implementation.</param>
        /// <param name="probabilityProvider">The provider for the genetic probabilities.</param>
        public SimulationEvolver(Type simulationType, IGeneticProbabilityProvider probabilityProvider)
        {
            _probabilityProvider = probabilityProvider;
            _simulationType = simulationType;
            // Retrieve the settings attribute for the simulation type.
            _settings = _simulationType.GetAttribute<EvolutionSettingsAttribute>();
        }

        /// <summary>
        /// Evolves a new generation for the EvolutionLine of the given size.
        /// </summary>
        /// <param name="evolutionLine">The evolution line to evolve.</param>
        /// <param name="populationSize">The size of the new population.</param>
        public void Evolve(EvolutionLine evolutionLine, int populationSize)
        {
            // Create new random seed using the current time.
            var randomSeed = (int)DateTime.Now.Ticks;

            var population = evolutionLine.CurrentPopulation;

            // Determine the number of slices.
            // A slice size of zero indicates no slicing, so the whole population is essentially 1 slice itself.
            int sliceCount;
            if (_settings.SliceSize == 0)
            {
                sliceCount = 1;
            }
            else
            {
                if (population.Size % _settings.SliceSize != 0)
                    throw new Exception("Population size cannot be sliced.");
                sliceCount = population.Size / _settings.SliceSize;
            }

            // Initialise all the dimensions of the statistics array.
            // statistics[s][i][r] is the value of statistic s measured in repeat r of the simulations for individual i.
            var statistics = new double[evolutionLine.StatisticNames.Length][][];
            for (var stat = 0; stat < evolutionLine.StatisticNames.Length; stat++)
            {
                statistics[stat] = new double[population.Size][];
                for (var individual = 0; individual < population.Size; individual++)
                    statistics[stat][individual] = new double[_settings.RepeatCount];
            }
            
            // For each population slice...
            for (var slice = 0; slice < sliceCount; slice++)
            {
                // For each repeat to be performed...
                for (var repeat = 0; repeat < _settings.RepeatCount; repeat++)
                {
                    // Construct a simulation.
                    var simulation = population.ConstructSimulation(_simulationType, slice, repeat, randomSeed);
                    // Run it fully.
                    while (simulation.Tick())
                    {
                    }
                    // Obtain the statistics.
                    var simulationStatistics = simulation.MeasureStatistics();

                    // Copy the statistics into the statistics array for all the individuals involved in this slice.
                    for (var stat = 0; stat < simulationStatistics.Length; stat++)
                        for (var i = 0; i < simulationStatistics[stat].Length; i++)
                            statistics[stat][slice * _settings.SliceSize + i][repeat] = simulationStatistics[stat][i];
                }
            }
            // Average repeats of first statistic to form fitness values.
            var fitnessValues = from individual in statistics[0] select individual.Average();
            // Evolve a new population using the above calculated fitness values.
            var newPopulation = evolutionLine.CurrentPopulation.EvolveWithFitnessValues(populationSize,
                                                                                        fitnessValues.ToArray(),
                                                                                        _probabilityProvider);
            // Add the new population to the volution line.
            evolutionLine.AddGeneration(randomSeed, statistics, newPopulation);
        }
    }
}