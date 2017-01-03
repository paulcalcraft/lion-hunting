using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Encapsulates a structural definition of a Chromosome, the various genetic
    /// operations that may be applied, and the mechanism to generate definitions using
    /// Reflection.
    /// </summary>
    public class ChromosomeDefinition
    {
        // Static dictionary to map Chromosome type objects to their definitions.
        // This means Types only need to be reflected to generate their definitions once.
        private static readonly Dictionary<Type, ChromosomeDefinition> ChromosomeDefinitions =
            new Dictionary<Type, ChromosomeDefinition>();
        private readonly Type _chromosomeType;
        private readonly GeneDefinition[] _genes;
        private readonly SubChromosomeDefinition[] _subChromosomes;
        public long BinarySize { get; private set; }

        /// <summary>
        /// Constructs a new ChromosomeDefinition by Reflecting over a given type's
        /// properties.
        /// </summary>
        /// <param name="type">The Type object for the Chromosome.</param>
        private ChromosomeDefinition(Type type)
        {
            _chromosomeType = type;

            var geneDefinitions = new List<GeneDefinition>();
            var subChromosomes = new List<SubChromosomeDefinition>();

            // Iterate over all the properties of the type...
            foreach (var property in _chromosomeType.GetProperties())
            {
                // Iterate over all the attributes for each property...
                foreach (var attribute in property.GetCustomAttributes(false))
                {
                    // Add to gene definitions or sub chromosome definitions according to
                    // the attribute.
                    GeneDataDefinitionAttribute geneDefinitionAttribute;
                    if ((geneDefinitionAttribute = attribute as GeneDataDefinitionAttribute) != null)
                    {
                        geneDefinitions.Add(new GeneDefinition(property, geneDefinitionAttribute));
                    }
                    else if (attribute is SubChromosomeAttribute)
                    {
                        subChromosomes.Add(new SubChromosomeDefinition(property));
                    }
                }
            }

            _genes = geneDefinitions.ToArray();
            _subChromosomes = subChromosomes.ToArray();

            BinarySize = MeasureSize();
        }

        /// <summary>
        /// Constructs a ChromosomeDefinition manually, with each part passed in.
        /// </summary>
        /// <param name="chromosomeDataType">The Type object that the definition is for.</param>
        /// <param name="genes">The array of gene definitions.</param>
        /// <param name="subChromosomes">The array of sub chromosome definitions.</param>
        /// <param name="size">The binary size of the chromosome.</param>
        private ChromosomeDefinition(Type chromosomeDataType, GeneDefinition[] genes, SubChromosomeDefinition[] subChromosomes, long size)
        {
            _chromosomeType = chromosomeDataType;
            _genes = genes;
            _subChromosomes = subChromosomes;
            BinarySize = size;

            Debug.Assert(size == MeasureSize());
        }

        /// <summary>
        /// Gets the Type of the Chromosome that this definition defines.
        /// </summary>
        public Type ChromosomeType
        {
            get { return _chromosomeType; }
        }

        /// <summary>
        /// Measures the size of the binary representation of the Chromosome.
        /// </summary>
        /// <returns>The size in bytes.</returns>
        private long MeasureSize()
        {
            // Measure the size by using the ToBinary method on a temporary MemoryStream
            // and recording the length.
            var writer = new BinaryWriter(new MemoryStream());
            ToBinary(GenerateRandom(), writer);
            var length = writer.BaseStream.Length;
            writer.Close();
            return length;
        }

        /// <summary>
        /// Write out a binary type definition, which defines the ordering of all the
        /// genes in this ChromosomeDefinition, identified by property names.
        /// </summary>
        /// <param name="writer">The output stream writer.</param>
        public void WriteTypeDescriptor(BinaryWriter writer)
        {
            writer.Write(_genes.Length);
            foreach (var geneDefinition in _genes)
                geneDefinition.WriteTypeDescriptor(writer);

            writer.Write(_subChromosomes.Length);
            foreach (var subChromosome in _subChromosomes)
                subChromosome.WriteTypeDescriptor(writer);
        }

        /// <summary>
        /// Creates an adapted ChromosomeDefinition according to a descriptor in a file.
        /// </summary>
        /// <param name="reader">The input stream reader.</param>
        /// <returns>The adapated definition with reordered fields.</returns>
        public ChromosomeDefinition AdaptToDescriptor(BinaryReader reader)
        {
            // Read in the gene property names.
            var geneCount = reader.ReadInt32();
            var geneNames = new string[geneCount];
            for (var i = 0; i < geneCount; i++)
                geneNames[i] = reader.ReadString();

            // Reorder gene definitions by the order their name appears in the input.
            var adaptedGenes = from geneName in geneNames
                               join gene in _genes on geneName equals gene.Name
                               select gene;

            // Read in list of sub chromosome property names.
            var subChromosomeCount = reader.ReadInt32();
            var adaptedSubChromosomes = new SubChromosomeDefinition[subChromosomeCount];
            for (var i = 0; i < subChromosomeCount; i++)
            {
                var name = reader.ReadString();
                // Find the first sub chromosome definition with the matching name.
                var subChromosome = _subChromosomes.First(c => c.Name == name);
                // Recursively adapt the sub chromosome definition.
                adaptedSubChromosomes[i] = subChromosome.AdaptToDescriptor(reader);
            }

            return new ChromosomeDefinition(_chromosomeType, adaptedGenes.ToArray(), adaptedSubChromosomes.ToArray(), BinarySize);
        }

        /// <summary>
        /// Retrieves the ChromosomeDefinition for the given Type object, or generates it
        /// if it hasn't been already.
        /// </summary>
        /// <param name="type">The Type to get the ChromosomeDefinition for.</param>
        /// <returns>The corresponding ChromosomeDefinition.</returns>
        public static ChromosomeDefinition Retrieve(Type type)
        {
            Debug.Assert(type.IsSubclassOf(typeof(Chromosome)));

            ChromosomeDefinition definition;
            if (!ChromosomeDefinitions.TryGetValue(type, out definition))
            {
                definition = new ChromosomeDefinition(type);
                ChromosomeDefinitions.Add(type, definition);
            }

            return definition;
        }

        /// <summary>
        /// Writes a binary representation of a given instance of this Chromosome to the
        /// provided writer.
        /// </summary>
        /// <param name="chromosome">The Chromosome instance.</param>
        /// <param name="writer">The output writer.</param>
        public void ToBinary(Chromosome chromosome, BinaryWriter writer)
        {
            foreach (var geneDefinition in _genes)
                geneDefinition.ToBinary(chromosome, writer);

            foreach (var subChromosome in _subChromosomes)
                subChromosome.ToBinary(chromosome, writer);
        }

        /// <summary>
        /// Reads a Chromosome of this ChromosomeDefinition from its binary
        /// representation.
        /// </summary>
        /// <param name="reader">The input reader.</param>
        /// <returns>The Chromsome instance that's been read.</returns>
        public Chromosome FromBinary(BinaryReader reader)
        {
            var chromosome = NewChromosome();
            foreach (var geneDefinition in _genes)
                geneDefinition.FromBinary(chromosome, reader);

            foreach (var subChromosome in _subChromosomes)
                subChromosome.FromBinary(chromosome, reader);
            return chromosome;
        }

        /// <summary>
        /// Creates a new Chromosome of this ChromosomeDefinition with default values.
        /// </summary>
        /// <returns>The new Chromosome instance.</returns>
        private Chromosome NewChromosome()
        {
            return Activator.CreateInstance(_chromosomeType, true) as Chromosome;
        }

        /// <summary>
        /// Creates a new Chromosome of this ChromosomeDefinition with random values.
        /// </summary>
        /// <returns>The new Chromosome instance.</returns>
        public Chromosome GenerateRandom()
        {
            var chromosome = NewChromosome();

            foreach (var geneDefinition in _genes)
                geneDefinition.SetRandomAllele(chromosome);

            foreach (var subChromosome in _subChromosomes)
                subChromosome.GenerateRandom(chromosome);

            return chromosome;
        }

        /// <summary>
        /// Combines two parent Chromosome instances to produce two child Chromosomes.
        /// The resulting Chromosomes are crossed over and mutated according to the
        /// provided probabilities.
        /// </summary>
        /// <param name="parent1">The first parent Chromosome.</param>
        /// <param name="parent2">The second parent Chromosome.</param>
        /// <param name="child1">The first child Chromosome.</param>
        /// <param name="child2">The second child Chromosome.</param>
        /// <param name="probabilityProvider">The delegate to use for crossover and mutation probabilities.</param>
        public void Combine(Chromosome parent1, Chromosome parent2, out Chromosome child1, out Chromosome child2, IGeneticProbabilityProvider probabilityProvider)
        {
            Combine(probabilityProvider.ShouldCrossover(), parent1, parent2, out child1, out child2, probabilityProvider);
        }

        /// <summary>
        /// Combines two parent Chromosome instances to produce two child Chromosomes.
        /// The resulting Chromosomes are crossed over according to the first argument,
        /// and are mutated according to the provided probability.
        /// </summary>
        /// <param name="crossover">Whether a crossover should occur.</param>
        /// <param name="parent1">The first parent Chromosome.</param>
        /// <param name="parent2">The second parent Chromosome.</param>
        /// <param name="child1">The first child Chromosome.</param>
        /// <param name="child2">The second child Chromosome.</param>
        /// <param name="probabilityProvider">The delegate to use for mutation probability.</param>
        private void Combine(bool crossover, Chromosome parent1, Chromosome parent2, out Chromosome child1, out Chromosome child2, IGeneticProbabilityProvider probabilityProvider)
        {
            child1 = NewChromosome();
            child2 = NewChromosome();

            foreach (var gene in _genes)
                gene.Combine(crossover, parent1, parent2, child1, child2, probabilityProvider);

            foreach (var subChromosome in _subChromosomes)
                subChromosome.Combine(crossover, parent1, parent2, child1, child2, probabilityProvider);
        }

        /// <summary>
        /// Provides a definition of an atomic gene property.
        /// </summary>
        class GeneDefinition
        {
            private readonly string _name;
            private readonly MethodInfo _getMethod;
            private readonly MethodInfo _setMethod;
            private readonly GeneDataDefinitionAttribute _dataDefinition;

            /// <summary>
            /// Constructs a new GeneDefinition for a property with a
            /// GeneDataDefinitionAttribute applied.
            /// </summary>
            /// <param name="property">The property field to construct the definition for.</param>
            /// <param name="dataDefinition">The data definition applied to the property field.</param>
            public GeneDefinition(PropertyInfo property, GeneDataDefinitionAttribute dataDefinition)
            {
                Debug.Assert(property.PropertyType.IsValueType);
                _name = property.Name;
                _getMethod = property.GetGetMethod();
                _setMethod = property.GetSetMethod();
                _dataDefinition = dataDefinition;
            }

            /// <summary>
            /// Returns the name of the gene variable; the name of the property in the
            /// Chromosome.
            /// </summary>
            public string Name
            {
                get { return _name; }
            }

            /// <summary>
            /// Sets a random allele of this gene variable on the given Chromosome
            /// instance.
            /// </summary>
            /// <param name="chromosome">The Chromosome instance.</param>
            public void SetRandomAllele(Chromosome chromosome)
            {
                _setMethod.Invoke(chromosome, new[] { _dataDefinition.RandomAllele() });
            }

            /// <summary>
            /// Performs a combination of this gene variable between two parent
            /// Chromosomes, projecting the result onto the given child Chromosome
            /// instances.
            /// </summary>
            /// <param name="crossover">Whether a crossover should occur.</param>
            /// <param name="parent1">The first parent Chromosome.</param>
            /// <param name="parent2">The second parent Chromosome.</param>
            /// <param name="child1">The first child Chromosome.</param>
            /// <param name="child2">The second child Chromosome.</param>
            /// <param name="probabilityProvider">The delegate to use for mutation probability.</param>
            public void Combine(bool crossover, Chromosome parent1, Chromosome parent2, Chromosome child1, Chromosome child2, IGeneticProbabilityProvider probabilityProvider)
            {
                var parent1Value = _getMethod.Invoke(parent1, null);
                var parent2Value = _getMethod.Invoke(parent2, null);

                object child1Value, child2Value;
                if (crossover && RandomUtility.Generator.CoinToss())
                {
                    child1Value = parent2Value;
                    child2Value = parent1Value;
                }
                else
                {
                    child1Value = parent1Value;
                    child2Value = parent2Value;
                }

                if (probabilityProvider.ShouldMutate())
                    _dataDefinition.Mutate(ref child1Value);
                if (probabilityProvider.ShouldMutate())
                    _dataDefinition.Mutate(ref child2Value);

                _setMethod.Invoke(child1, new[] {child1Value});
                _setMethod.Invoke(child2, new[] {child2Value});
            }

            /// <summary>
            /// Writes out the binary representation of this gene variable for the given
            /// Chromosome to the provided output.
            /// </summary>
            /// <param name="chromosome">The Chromosome instance.</param>
            /// <param name="writer">The output writer.</param>
            public void ToBinary(Chromosome chromosome, BinaryWriter writer)
            {
                _dataDefinition.ToBinary(_getMethod.Invoke(chromosome, null), writer);
            }

            /// <summary>
            /// Reads this gene variable into the given Chromosome from its binary
            /// representation.
            /// </summary>
            /// <param name="chromosome">The Chromosome instance to read into.</param>
            /// <param name="reader">The input reader.</param>
            public void FromBinary(Chromosome chromosome, BinaryReader reader)
            {
                _setMethod.Invoke(chromosome, new[] { _dataDefinition.FromBinary(reader) });
            }

            /// <summary>
            /// Writes out the type descriptor for this gene variable.
            /// For gene definitions, consists only of writing their names as identifiers.
            /// </summary>
            /// <param name="writer">The output writer.</param>
            public void WriteTypeDescriptor(BinaryWriter writer)
            {
                writer.Write(_name);
            }
        }

        /// <summary>
        /// Provides a definition of a gene property that is a reference to another
        /// Chromosome structure.
        /// </summary>
        class SubChromosomeDefinition
        {
            private readonly string _name;
            private readonly MethodInfo _getMethod;
            private readonly MethodInfo _setMethod;
            private readonly ChromosomeDefinition _chromosomeDefinition;

            /// <summary>
            /// Constructs a new SubChromosomeDefinition from the given property.
            /// </summary>
            /// <param name="property">The property to build the definition from.</param>
            public SubChromosomeDefinition(PropertyInfo property)
            {
                _name = property.Name;
                _getMethod = property.GetGetMethod();
                _setMethod = property.GetSetMethod();
                _chromosomeDefinition = Retrieve(property.PropertyType);
            }

            /// <summary>
            /// Constructs a new SubChromosomeDefinition manually with all the parts being
            /// passed in.
            /// </summary>
            /// <param name="name">The name of the gene variable.</param>
            /// <param name="getMethod">The MethodInfo for the getter.</param>
            /// <param name="setMethod">The MethodInfo for the setter.</param>
            /// <param name="chromosomeDefinition">The ChromosomeDefinition for the subchromosome.</param>
            private SubChromosomeDefinition(string name, MethodInfo getMethod, MethodInfo setMethod, ChromosomeDefinition chromosomeDefinition)
            {
                _name = name;
                _getMethod = getMethod;
                _setMethod = setMethod;
                _chromosomeDefinition = chromosomeDefinition;
            }

            /// <summary>
            /// Returns the name of the subchromosome gene variable; the name of the
            /// property in the Chromosome.
            /// </summary>
            public string Name
            {
                get { return _name; }
            }

            /// <summary>
            /// Generates an instance of the subchromosome with random values, and assigns
            /// it to the given superchromosome.
            /// </summary>
            /// <param name="superChromosome"></param>
            public void GenerateRandom(Chromosome superChromosome)
            {
                _setMethod.Invoke(superChromosome, new[] {_chromosomeDefinition.GenerateRandom()});
            }

            /// <summary>
            /// Performs a combination of this subchromosome between two parent
            /// superchromosomes, projecting the result onto the given child
            /// superchromosome instances.
            /// </summary>
            /// <param name="crossover">Whether a crossover should occur.</param>
            /// <param name="parent1">The first parent (super)Chromosome.</param>
            /// <param name="parent2">The second parent (super)Chromosome.</param>
            /// <param name="child1">The first child (super)Chromosome.</param>
            /// <param name="child2">The second child (super)Chromosome.</param>
            /// <param name="probabilityProvider">The delegate to use for mutation probability.</param>
            public void Combine(bool crossover, Chromosome parent1, Chromosome parent2, Chromosome child1, Chromosome child2, IGeneticProbabilityProvider probabilityProvider)
            {
                var parent1SubChromosome = _getMethod.Invoke(parent1, null) as Chromosome;
                var parent2SubChromosome = _getMethod.Invoke(parent2, null) as Chromosome;

                Chromosome child1SubChromosome, child2SubChromosome;
                _chromosomeDefinition.Combine(crossover, parent1SubChromosome, parent2SubChromosome,
                                              out child1SubChromosome, out child2SubChromosome, probabilityProvider);

                _setMethod.Invoke(child1, new[] {child1SubChromosome});
                _setMethod.Invoke(child2, new[] {child2SubChromosome});
            }

            /// <summary>
            /// Writes out the binary representation of this subchromosome for the given
            /// superchromosome to the provided output.
            /// </summary>
            /// <param name="chromosome">The Chromosome instance.</param>
            /// <param name="writer">The output writer.</param>
            public void ToBinary(Chromosome chromosome, BinaryWriter writer)
            {
                var subChromosome = _getMethod.Invoke(chromosome, null) as Chromosome;
                _chromosomeDefinition.ToBinary(subChromosome, writer);
            }

            /// <summary>
            /// Reads this subchromosome into the given superchromosome from its binary
            /// representation.
            /// </summary>
            /// <param name="chromosome">The Chromosome instance to read into.</param>
            /// <param name="reader">The input reader.</param>
            public void FromBinary(Chromosome chromosome, BinaryReader reader)
            {
                _setMethod.Invoke(chromosome, new[] { _chromosomeDefinition.FromBinary(reader) });
            }

            /// <summary>
            /// Writes out the type descriptor for this subchromosome.
            /// </summary>
            /// <param name="writer">The output writer.</param>
            public void WriteTypeDescriptor(BinaryWriter writer)
            {
                writer.Write(_name);
                _chromosomeDefinition.WriteTypeDescriptor(writer);
            }

            /// <summary>
            /// Creates an adapted SubChromosomeDefinition according to a descriptor in a
            /// file.
            /// </summary>
            /// <param name="reader">The input stream reader.</param>
            /// <returns>The adapated definition with reordered fields.</returns>
            public SubChromosomeDefinition AdaptToDescriptor(BinaryReader reader)
            {
                return new SubChromosomeDefinition(_name, _getMethod, _setMethod, _chromosomeDefinition.AdaptToDescriptor(reader));
            }
        }
    }
}