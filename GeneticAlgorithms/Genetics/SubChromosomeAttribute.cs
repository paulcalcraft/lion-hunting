using System;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides an attribute to label a property as a subchromosome gene.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SubChromosomeAttribute : Attribute
    {
        
    }
}