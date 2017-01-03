using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Entities;
using LionHunting.Simulations.Full;
using LionHunting.Utility;

namespace LionHunting.Simulations.Behaviour
{
    abstract class TargetTrendSet : Chromosome
    {
        [SubChromosome]
        public Trend Distance { get; set; }
        [SubChromosome]
        public Trend RelativeAngle { get; set; }
        [SubChromosome]
        public Trend CumulativeAttention { get; set; }
        [SubChromosome]
        public Trend IndividualDistanceAttention { get; set; }
        [SubChromosome]
        public Trend IndividualAngleAttention { get; set; }

        protected static double NormalisedABMagnitude(Vector2 a, Vector2 b, double scale)
        {
            return Math.Min(1, (b - a).Magnitude()/scale);
        }

        private static double NormalisedDistance(Entity hunter, Entity target)
        {
            return NormalisedABMagnitude(hunter.Location, target.Location, LionSimulation.DistanceRange);
        }

        private static double SafeAcos(double d)
        {
            var result = Math.Acos(Math.Min(1, Math.Max(-1, d)));
            Debug.Assert(!Double.IsNaN(result));
            return result;
        }
        
        public static double NormalisedRelativeAngle(Vector2 a, Vector2 b)
        {
            var angle = SafeAcos(b.Normalise().Dot(a.Normalise()));
            return 2 * (0.5d - Math.Abs(0.5d - angle / (2d * Math.PI)));
        }

        private static double NormalisedRelativeAngle(Entity hunter, Entity target)
        {
            return NormalisedRelativeAngle(hunter.Location, target.Location);
        }

        private double IndividualAttention(Entity hunter, Entity target)
        {
            var distanceAttention = 1 - NormalisedDistance(hunter, target);
            var angleAttention = 1 - NormalisedRelativeAngle(hunter, target);
            var distanceProportion = IndividualDistanceAttention.Influence/
                                     (IndividualDistanceAttention.Influence + IndividualAngleAttention.Influence);
            return distanceAttention*distanceProportion + angleAttention*(1 - distanceProportion);
        }

        protected void InsertFor(TrendCalculator trendCalculator, Entity hunter, Entity target, IEnumerable<Entity> hunters)
        {
            Distance.InsertFor(trendCalculator, NormalisedDistance(hunter, target));
            RelativeAngle.InsertFor(trendCalculator, NormalisedRelativeAngle(hunter, target));

            var cumulativeAttention = 0d;
            foreach (var rival in hunters)
            {
                if (rival == hunter)
                    continue;
                cumulativeAttention += IndividualAttention(rival, target);
                if (cumulativeAttention < 1d) continue;
                cumulativeAttention = 1d;
                break;
            }

            CumulativeAttention.InsertFor(trendCalculator, cumulativeAttention);
        }

        public abstract double Calculate(Entity hunter, Entity target, IEnumerable<Entity> hunters);
    }
}