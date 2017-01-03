using System;
using System.Diagnostics;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    sealed class Unit
    {
        public Vector2 Location { get; private set; }
        public Species Species { get; private set; }
        public double BoundingRadius { get; private set; }
        private double _sprintTime;
        private Vector2 _velocity;
        private double _velocityScalar;
        private readonly double _vMax;
        private readonly double _k;

        private Unit(Species species, Vector2 location, double boundingRadius, double vMax, double k)
        {
            Species = species;
            Location = location;
            BoundingRadius = boundingRadius;
            _vMax = vMax;
            _k = k;
        }

        public bool Overlaps(Unit other)
        {
            return (other.Location - Location).Magnitude() < (BoundingRadius + other.BoundingRadius);
        }

        private double DistanceCurveValue(double t)
        {
            return _vMax*(t+(Math.Exp(-_k*t)-1)/_k);
        }

        private double VelocityCurveValue(double t)
        {
            return _vMax*(1 - Math.Exp(-_k*t));
        }

        private double AccelerationCurveValue(double t)
        {
            return _vMax*_k*Math.Exp(-_k*t);
        }

        public void Sprint(double dt, Vector2 direction)
        {
            var distanceDifference = DistanceCurveValue(_sprintTime + dt) - DistanceCurveValue(_sprintTime);
            Location += distanceDifference*direction;
            _sprintTime += dt;
            return;

            var newVelocityScalar = VelocityCurveValue(_sprintTime + dt);
            var acceleration = (newVelocityScalar - _velocityScalar)/dt*direction;
            Location += _velocity*dt + 0.5*acceleration*Math.Pow(dt, 2);

            _sprintTime += dt;
            _velocityScalar = newVelocityScalar;
            _velocity += acceleration;
        }

        // predator - prey = distance
        public double FindEscapeDistance(Unit predator, bool useStationarySpookDistance)
        {
            if (useStationarySpookDistance)
            {
                switch (Species)
                {
                    case Species.Gazelle:
                        return 7.5;
                    case Species.Wildebeest:
                        return 11.8;
                    case Species.Zebra:
                        return 11.0;
                }
            }

            Func<double, double> velocityDifferenceCurve =
                t => VelocityCurveValue(t) - predator.VelocityCurveValue(predator._sprintTime + t);

            const double precision = 0.001;
            var searcher = 1d;
            var searchT = 0d;
            while (true)
            {
                if (velocityDifferenceCurve(searchT) > 0)
                {
                    if (searcher / 10 < precision)
                    {
                        searchT -= searcher/2;
                        break;
                    }
                    searchT -= searcher;
                    searcher /= 10;
                }
                else
                    searchT += searcher;
            }

            return predator.DistanceCurveValue(predator._sprintTime + searchT) - DistanceCurveValue(searchT)/* + BoundingRadius + predator.BoundingRadius*/;
        }

        public void Crawl(double dt, Vector2 direction)
        {
            Location += 2*direction*dt;
                _sprintTime = 0;
        }

        public static Unit Create(Species species, Vector2 location)
        {
            switch (species)
            {
                case Species.Wildebeest:
                    return new Unit(species, location, 1.115, 14.3, 0.39);
                case Species.Gazelle:
                    return new Unit(species, location, 0.452, 26.5, 0.17);
                case Species.Lion:
                    return new Unit(species, location, 0.975, 13.9, 0.68);
                case Species.Zebra:
                    return new Unit(species, location, 1.175, 16.0, 0.31);
            }
            Debug.Fail("Attempted to create a unit of an invalid Species.");
            return null;
        }
    }
}