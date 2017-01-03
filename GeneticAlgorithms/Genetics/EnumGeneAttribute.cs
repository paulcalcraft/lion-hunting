using System;
using System.IO;
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides an attribute to label a property as a gene variable of type enum.
    /// </summary>
    public class EnumGeneAttribute : GeneDataDefinitionAttribute
    {
        private readonly int _enumLength;
        private readonly Array _enumValues;

        public EnumGeneAttribute(Type enumType)
        {
            _enumValues = Enum.GetValues(enumType);
            _enumLength = _enumValues.GetLength(0);
        }

        public override void Mutate(ref object gene)
        {
            var newIndex = RandomUtility.Generator.Next(_enumLength - 1);

            if (newIndex >= Array.IndexOf(_enumValues, gene))
                newIndex++;

            gene = _enumValues.GetValue(newIndex);
        }

        public override object RandomAllele()
        {
            return _enumValues.GetValue(RandomUtility.Generator.Next(_enumLength));
        }

        public override object FromBinary(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public override void ToBinary(object value, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}