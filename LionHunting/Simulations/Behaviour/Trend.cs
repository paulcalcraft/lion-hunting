using System;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;

namespace LionHunting.Simulations.Behaviour
{
    class Trend : Chromosome
    {
        private const int IntervalCount = 3; //7;
        private const double IntervalWidth = 1d/IntervalCount;
        private readonly double[] _yValues = new double[IntervalCount+1];

        [DoubleGene]
        public double Influence { get; set; }

        [DoubleGene]
        public double Y0
        {
            get { return _yValues[0]; }
            set { _yValues[0] = value; }
        }
        [DoubleGene]
        public double Y1
        {
            get { return _yValues[1]; }
            set { _yValues[1] = value; }
        }
        [DoubleGene]
        public double Y2
        {
            get { return _yValues[2]; }
            set { _yValues[2] = value; }
        }
        [DoubleGene]
        public double Y3
        {
            get { return _yValues[3]; }
            set { _yValues[3] = value; }
        }
        /*[DoubleGene]
        public double Y4
        {
            get { return _yValues[4]; }
            set { _yValues[4] = value; }
        }
        [DoubleGene]
        public double Y5
        {
            get { return _yValues[5]; }
            set { _yValues[5] = value; }
        }
        [DoubleGene]
        public double Y6
        {
            get { return _yValues[6]; }
            set { _yValues[6] = value; }
        }
        [DoubleGene]
        public double Y7
        {
            get { return _yValues[7]; }
            set { _yValues[7] = value; }
        }*/

        public void InsertFor(TrendCalculator trendCalculator, double input)
        {
            trendCalculator.AddTrend(Influence, Calculate(input));
        }

        private double Calculate(double x)
        {
            Debug.Assert(x >= 0 && x <= 1);
            var rangeIndex = (int)Math.Truncate(x*IntervalCount);
            var lean = (x%IntervalWidth)/IntervalWidth;
            if (rangeIndex == IntervalCount)
                return _yValues[rangeIndex];
            return (_yValues[rangeIndex]*(1 - lean) + _yValues[rangeIndex + 1]*lean);
        }
    }
}