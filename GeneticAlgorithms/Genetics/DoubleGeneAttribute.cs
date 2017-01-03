using System;
using System.Diagnostics;
using System.IO;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides an attribute to label a property as a gene variable of type double.
    /// </summary>
    public class DoubleGeneAttribute : GeneDataDefinitionAttribute
    {
        private readonly double _maxValue;
        private readonly double _minValue;

        public DoubleGeneAttribute(double minValue, double maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public DoubleGeneAttribute()
            : this(0, 1)
        {
        }

        public override void Mutate(ref object gene)
        {
            gene = RandomAllele();
        }

        private bool IsValid(double value)
        {
            return !Double.IsNaN(value) && value <= _maxValue && value >= _minValue;
        }

        public override object RandomAllele()
        {
            return RandomUtility.Generator.NextDouble() * (_maxValue - _minValue) + _minValue;
        }

        public override object FromBinary(BinaryReader reader)
        {
            var value = reader.ReadDouble();
            Debug.Assert(IsValid(value));
            return value;
        }

        public override void ToBinary(object value, BinaryWriter writer)
        {
            var doubleValue = (double) value;
            Debug.Assert(IsValid(doubleValue));
            writer.Write(doubleValue);
        }
    }
}