using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// A static class to provide helper methods for construction of simulations using slices and repeats.
    /// </summary>
    public static class SimulationPopulationHelper
    {
        /// <summary>
        /// Constructs and initialises an EvolvableSimulation instance for a subset of a population as
        /// used with the given simulation type.
        /// </summary>
        /// <param name="population">The population from which to use individuals.</param>
        /// <param name="simulationType">The Type object for the EvolvableSimulation implementation.</param>
        /// <param name="sliceIndex">The slice index for which to create the simulation.</param>
        /// <param name="repeatIndex">The repeat index for which to create the simulation.</param>
        /// <param name="randomSeed">The base random seed value for the given population.</param>
        /// <returns>An initialised simulation.</returns>
        public static EvolvableSimulation ConstructSimulation(this Population population, Type simulationType, int sliceIndex, int repeatIndex, int randomSeed)
        {
            // Obtain evolution settings.
            var settings = simulationType.GetAttribute<EvolutionSettingsAttribute>();
            // Use Reflection to create an instance of the given simulation type.
            var simulation = Activator.CreateInstance(simulationType) as EvolvableSimulation;

            // Slice the population according to the evolution settings and the given slice index.
            // Offset the base random seed by the appropriate amount for the slice/repeat required.
            // This offsetting is required to ensure repeats aren't just reruns of identical simulations.
            IEnumerable<Chromosome> slice;
            if (settings.SliceSize == 0)
            {
                Debug.Assert(sliceIndex == 0);
                // Slice size of zero indicates no slicing, so pass through the whole population.
                slice = population;
                // When no slicing is involved, the random seed is only offset by the repeat index.
                randomSeed += repeatIndex;
            }
            else
            {
                // Take a subset of the population's chromosomes as the slice.
                slice = population.Skip(sliceIndex * settings.SliceSize).Take(settings.SliceSize);
                // Offset the random seed both by the slice index and the repeat index.
                randomSeed += sliceIndex * settings.RepeatCount + repeatIndex;
            }

            // Initialise the simulation with the given slice and offset random seed, and then return it.
            simulation.Initialise(slice.ToArray(), randomSeed);
            return simulation;
        }

        /// <summary>
        /// Constructs and initialises an EvolvableSimulation instance for a subset of a certain generation
        /// of the given evolution line.
        /// </summary>
        /// <param name="evolutionLine">The evolution line to construct a simulation for.</param>
        /// <param name="generationIndex">The index of the generation in the line for which to create the simulation.</param>
        /// <param name="sliceIndex">The slice index for which to create the simulation.</param>
        /// <param name="repeatIndex">The repeat index for which to create the simulation.</param>
        /// <returns>An initialised simulation.</returns>
        public static EvolvableSimulation ConstructSimulation(this EvolutionLine evolutionLine, int generationIndex, int sliceIndex, int repeatIndex)
        {
            // Obtain the specified population and the simulation type from the evolution line and pass through to the above method.
            return evolutionLine.GetPopulation(generationIndex).ConstructSimulation(evolutionLine.SimulationType,
                                                                                    sliceIndex,
                                                                                    repeatIndex,
                                                                                    evolutionLine[generationIndex].
                                                                                        RandomSeed);
        }
    }
}