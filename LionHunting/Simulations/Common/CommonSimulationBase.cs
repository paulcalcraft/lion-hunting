using System;
using System.Collections.Generic;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    /// <summary>
    /// Provides a common base for simulations consisting of lion and prey units on a 2D plane.
    /// This common base is used by the CommonVisualiser to allow reuse of the visualisation code.
    /// </summary>
    abstract class CommonSimulationBase : EvolvableSimulation
    {
        public double Width { get; private set; }
        public double Height { get; private set; }

        private readonly IList<Unit> _lionUnits;
        private readonly IList<Unit> _preyUnits;

        public IList<Unit> LionUnits
        {
            get { return _lionUnits; }
        }

        public IList<Unit> PreyUnits
        {
            get { return _preyUnits; }
        }

        protected CommonSimulationBase(double timeLimit, double tickTime, double width, double height)
            : base((int)Math.Ceiling(timeLimit/tickTime), tickTime)
        {
            Width = width;
            Height = height;
            _lionUnits = new List<Unit>();
            _preyUnits = new List<Unit>();
        }

        protected Vector2 RandomLocation()
        {
            var x = Random.DoubleInRange(0, Width);
            var y = Random.DoubleInRange(0, Height);
            return new Vector2(x, y);
        }

        protected Vector2 RandomLocation(Vector2 centre, double dispersal)
        {
            return RandomLocation(centre, dispersal, dispersal);
        }

        protected Vector2 RandomLocation(Vector2 centre, double xDispersal, double yDispersal)
        {
            var x = Random.DoubleInRange(-xDispersal, xDispersal);
            var y = Random.DoubleInRange(-yDispersal, yDispersal);
            return new Vector2(x, y) + centre;
        }

        public bool IsOutOfBounds(Unit unit)
        {
            return unit.Location.X < 0 || unit.Location.X > Width || unit.Location.Y < 0 || unit.Location.Y > Height;
        }
    }
}