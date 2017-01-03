using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Entities;
using LionHunting.Simulations.Full;

namespace LionHunting.Simulations.Behaviour
{
    class LiveTargetTrendSet : TargetTrendSet
    {
        [SubChromosome]
        public Trend RelativeVelocityAngle { get; set; }
        [SubChromosome]
        public Trend Speed { get; set; }

        public override double Calculate(Entity hunter, Entity target, IEnumerable<Entity> hunters)
        {
            var aHunter = hunter as AnimateEntity;
            var aTarget = target as AnimateEntity;
            Debug.Assert(aHunter != null && aTarget != null);

            var trendCalculator = new TrendCalculator();

            InsertFor(trendCalculator, hunter, target, hunters);

            if (aHunter.Velocity.Magnitude() != 0 || aTarget.Velocity.Magnitude() != 0)
                RelativeVelocityAngle.InsertFor(trendCalculator, NormalisedRelativeAngle(aHunter.Velocity, aTarget.Velocity));
            // TODO: remove reliance on lion simulation
            Speed.InsertFor(trendCalculator, aTarget.Velocity.Magnitude()/LionSimulation.SpeedRange);

            return trendCalculator.Calculate();
        }
    }
}