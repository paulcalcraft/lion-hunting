using System.Collections.Generic;

namespace LionHunting.Simulations.Behaviour
{
    class TrendCalculator
    {
        private readonly IList<KeyValuePair<double, double>> _trends;
        private double _totalInfluence;

        public TrendCalculator()
        {
            _trends = new List<KeyValuePair<double, double>>();
        }

        public void AddTrend(double influence, double value)
        {
            if (value < 0 || influence < 0)
                influence *= 1;
            _trends.Add(new KeyValuePair<double, double>(influence, value));
            _totalInfluence += influence;
        }

        public double Calculate()
        {
            var total = 0d;
            foreach (var trend in _trends)
                total += (trend.Key/_totalInfluence)*trend.Value;
            return total;
        }
    }
}