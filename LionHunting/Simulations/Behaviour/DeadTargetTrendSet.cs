using System.Collections.Generic;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Entities;
using LionHunting.Simulations.Full;

namespace LionHunting.Simulations.Behaviour
{
    class DeadTargetTrendSet : TargetTrendSet
    {
        [SubChromosome]
        public Trend Weight { get; set; }

        public override double Calculate(Entity hunter, Entity target, IEnumerable<Entity> hunters)
        {
            var trendCalculator = new TrendCalculator();

            InsertFor(trendCalculator, hunter, target, hunters);

            Weight.InsertFor(trendCalculator, target.Weight/LionSimulation.WeightRange);
            
            return trendCalculator.Calculate();
        }
    }
}