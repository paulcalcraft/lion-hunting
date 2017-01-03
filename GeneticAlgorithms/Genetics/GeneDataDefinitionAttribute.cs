using System;
using System.IO;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides an attribute to label a property as a gene variable and define its data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public abstract class GeneDataDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Performs a mutation on the given object.
        /// </summary>
        /// <param name="gene">The instance of the gene variable.</param>
        public abstract void Mutate(ref object gene);

        /// <summary>
        /// Returns a random allele of the gene.
        /// </summary>
        /// <returns>The random allele gene object.</returns>
        public abstract object RandomAllele();

        /// <summary>
        /// Reads the value of a gene from its binary representation.
        /// </summary>
        /// <param name="reader">The input stream reader.</param>
        /// <returns>The value read from the input.</returns>
        public abstract object FromBinary(BinaryReader reader);

        /// <summary>
        /// Writes the value of a gene to its binary representation.
        /// </summary>
        /// <param name="value">The instance of the gene variable.</param>
        /// <param name="writer">The output stream writer.</param>
        public abstract void ToBinary(object value, BinaryWriter writer);
    }
}