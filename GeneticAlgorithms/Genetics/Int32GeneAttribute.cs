using System;
using System.IO;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides an attribute to label a property as a gene variable of type int.
    /// </summary>
    class Int32GeneAttribute : GeneDataDefinitionAttribute
    {
        private readonly int _exclusiveMaxValue;
        private readonly int _minValue;

        public Int32GeneAttribute(int minValue, int maxValue)
        {
            _minValue = minValue;
            _exclusiveMaxValue = maxValue + 1;
        }

        public Int32GeneAttribute(int maxValue)
            : this(0, maxValue)
        {
        }

        public override void Mutate(ref object gene)
        {
            var allele = RandomUtility.Generator.Next(_minValue, _exclusiveMaxValue - 1);
            if (allele >= (Int32) gene)
                allele++;
            gene = allele;
        }

        public override object RandomAllele()
        {
            return RandomUtility.Generator.Next(_minValue, _exclusiveMaxValue);
        }

        public override object FromBinary(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        public override void ToBinary(object value, BinaryWriter writer)
        {
            writer.Write((int)value);
        }
    }
}