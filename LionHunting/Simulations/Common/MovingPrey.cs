using System;
using System.Collections.Generic;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    class MovingPrey
    {
        private readonly Unit _unit;
        private Vector2 _previousDirection;
        //private readonly double _fleeDistance;
        private readonly double _fleeProbability;
        public bool Fleeing { get; private set; }
        private bool _forceSpook;

        public Unit Unit
        {
            get { return _unit; }
        }

        public MovingPrey(Species species, Vector2 location, Random randomForFleeing)
        {
            _unit = Unit.Create(species, location);
            /*var idealFleeDistance = 0d;
            switch (species)
            {
                case Species.Gazelle:
                    idealFleeDistance = 7.5;
                    break;
                case Species.Wildebeest:
                    idealFleeDistance = 11.8;
                    break;
                case Species.Zebra:
                    idealFleeDistance = 11.0;
                    break;
            }*/

            //const double minFleeDistance = 5;
            //const double desiredFactor = 0.15;
            //var maxFleeDistance = (idealFleeDistance /*+ _unit.BoundingRadius + 0.975*/ - minFleeDistance) / desiredFactor + minFleeDistance;
            //_fleeDistance = randomForFleeing.DoubleInRange(minFleeDistance, maxFleeDistance);

            _fleeProbability = randomForFleeing.NextDouble();

            //_fleeDistance += 10;
            //_fleeDistance = randomForFleeing.DoubleInRange(8, 10);
        }

        public void ForceSpook()
        {
            if (Fleeing) return;
            _forceSpook = true;
        }

        public void Update(double dt, IList<Unit> predators, bool useStationarySpookDistances)
        {
            //var perceptualLimit = _fleeDistance;//60;

            var cumulativeVector = new Vector2();

            var canSeePredators = false;
            if (_forceSpook)
            {
                canSeePredators = true;
                Fleeing = true;
            }
            for (var i = 0; i < predators.Count; i++)
            {
                double magnitude;
                var direction =
                    (predators[i].Location.To(_unit.Location)).Normalise(out magnitude);

                var escapeDistance = _unit.FindEscapeDistance(predators[i], useStationarySpookDistances);
                const double desiredFactor = 0.15;// 0.23;//0.15;
                var maxFleeDistance = escapeDistance/desiredFactor;
                var fleeDistance = _fleeProbability*maxFleeDistance;

                if (_forceSpook)
                    cumulativeVector += direction;
                else if (magnitude <= fleeDistance)
                {
                    _forceSpook = true;

                    Update(dt, predators, useStationarySpookDistances);
                    return;

                    Fleeing = true;
                    cumulativeVector += (1 - magnitude / fleeDistance) * direction;
                    canSeePredators = true;
                }
            }

            if (!Fleeing)
                return;

            if (canSeePredators)
                _unit.Sprint(dt, _previousDirection = cumulativeVector.Normalise());
            else
                _unit.Sprint(dt, _previousDirection);
        }
    }
}