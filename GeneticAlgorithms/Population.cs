using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Represents a Population of Chromosomes.
    /// Implements the Chromosome IEnumerable interface to provide a standard method for iteration.
    /// </summary>
    public class Population : IEnumerable<Chromosome>
    {
        private readonly Chromosome[] _populace;
        private readonly ChromosomeDefinition _chromosomeDefinition;

        /// <summary>
        /// Gets the size of the population; how many individuals (chromosomes) it comprises.
        /// </summary>
        public int Size
        {
            get { return _populace.Length; }
        }

        /// <summary>
        /// Constructs a population (of a given size) of random individuals for the ChromosomeDefinition provided.
        /// </summary>
        /// <param name="size">The size of the population to construct.</param>
        /// <param name="chromosomeDefinition">The ChromosomeDefinition.</param>
        public Population(int size, ChromosomeDefinition chromosomeDefinition)
        {
            _chromosomeDefinition = chromosomeDefinition;
            _populace = new Chromosome[size];
            for (var i = 0; i < Size; i++)
                _populace[i] = _chromosomeDefinition.GenerateRandom();
        }

        /// <summary>
        /// Constructs a population with the given populace and ChromosomeDefinition.
        /// </summary>
        /// <param name="populace">The members of the population; the chromosomes.</param>
        /// <param name="chromosomeDefinition">The ChromosomeDefinition.</param>
        private Population(Chromosome[] populace, ChromosomeDefinition chromosomeDefinition)
        {
            _populace = populace;
            _chromosomeDefinition = chromosomeDefinition;
        }

        /// <summary>
        /// Loads a population from a binary reader given its size and the reader ChromosomeDefinition.
        /// </summary>
        /// <param name="size">The number of chromosomes to read.</param>
        /// <param name="reader">The input reader.</param>
        /// <param name="readerDefinition">The ChromosomeDefinition to be used for interpreting the chromosomes.</param>
        /// <returns></returns>
        public static Population FromBinary(int size, BinaryReader reader, ChromosomeDefinition readerDefinition)
        {
            var populace = new Chromosome[size];
            for (var i = 0; i < size; i++)
                populace[i] = readerDefinition.FromBinary(reader);
            return new Population(populace, readerDefinition);
        }

        /// <summary>
        /// Saves a population to a binary using its ChromosomeDefinition.
        /// </summary>
        /// <param name="writer">The output writer.</param>
        public void ToBinary(BinaryWriter writer)
        {
            foreach (var chromosome in _populace)
                _chromosomeDefinition.ToBinary(chromosome, writer);
        }

        /// <summary>
        /// Evolves a new Population from this one, using roulette wheel selection, provided with the selection weights and the genetic probability provider.
        /// </summary>
        /// <param name="numberOfChildren">The number of children in the new generation.</param>
        /// <param name="selectionWeights">The selection weights for the members of the current population. These must sum to 1.</param>
        /// <param name="probabilityProvider">The genetic probability provider.</param>
        /// <returns>A newly evolved population.</returns>
        private Population EvolveWithSelectionWeights(int numberOfChildren, double[] selectionWeights, IGeneticProbabilityProvider probabilityProvider)
        {
            Debug.Assert(selectionWeights.Length == Size && Size % 2 == 0);

            var newPopulation = new Chromosome[numberOfChildren];
            for (var i = 0; i < numberOfChildren; i += 2)
            {
                var parent1 = RandomUtility.Generator.ChooseIndexFromWeighted(selectionWeights);
                var parent2 = RandomUtility.Generator.ChooseIndexFromWeighted(selectionWeights);

                _chromosomeDefinition.Combine(_populace[parent1], _populace[parent2], out newPopulation[i], out newPopulation[i + 1], probabilityProvider);
            }

            return new Population(newPopulation, _chromosomeDefinition);
        }

        /// <summary>
        /// Evolves a new Population from this one by taking the provided fitness values, normalising them to total 1, and then passing through to the above method.
        /// </summary>
        /// <param name="numberOfChildren">The number of children in the new generation.</param>
        /// <param name="fitnessValues">The fitness values for the members of the current population.</param>
        /// <param name="probabilityProvider">The genetic probability provider.</param>
        /// <returns>A newly evolved population.</returns>
        public Population EvolveWithFitnessValues(int numberOfChildren, double[] fitnessValues, IGeneticProbabilityProvider probabilityProvider)
        {
            var totalFitness = fitnessValues.Sum();
            var selectionWeights = new double[fitnessValues.Length];

            for (var i = 0; i < fitnessValues.Length; i++)
                selectionWeights[i] = fitnessValues[i] / totalFitness;

            return EvolveWithSelectionWeights(numberOfChildren, selectionWeights, probabilityProvider);
        }

        /// <summary>
        /// Gets an enumerator to walk over the chromosomes in this population.
        /// </summary>
        /// <returns>A chromosome IEnumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a strongly typed Chromosome IEnumerator to walk over the chromosomes in this population.
        /// </summary>
        /// <returns>A chromosome IEnumerator.</returns>
        public IEnumerator<Chromosome> GetEnumerator()
        {
            return (_populace as IEnumerable<Chromosome>).GetEnumerator();
        }
    }
}