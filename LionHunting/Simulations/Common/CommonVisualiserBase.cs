using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using GeneticAlgorithms.Simulation;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    abstract class CommonVisualiserBase : SimulationVisualiser
    {
        private IList<Vector2> _previousLocations;

        protected CommonVisualiserBase()
        {
            Background = new SolidColorBrush(Colors.White);
        }

        public override EvolvableSimulation Simulation
        {
            get { return CommonSimulation; }
            set
            {
                CommonSimulation = (CommonSimulationBase)value;
                Width = CommonSimulation.Width;
                Height = CommonSimulation.Height;
                _previousLocations = null;
                Update();
            }
        }

        protected abstract CommonSimulationBase CommonSimulation { get; set; }

        public override void Update()
        {
            var units = CommonSimulation.LionUnits.Concat(CommonSimulation.PreyUnits);

            IList<Vector2> firstLocations = null;

            if (!PathView)
                Children.Clear();
            else if (_previousLocations == null)
            {
                Children.Clear();
                firstLocations = new List<Vector2>();
            }

            for (var i = 0; i < Children.Count; i++)
                if (Children[i] is Ellipse)
                    Children.RemoveAt(i--);

            var lionBrush = new SolidColorBrush(Colors.OrangeRed);
            var gazelleBrush = new SolidColorBrush(Colors.Orange);
            var wildebeestBrush = new SolidColorBrush(Colors.Brown);
            var zebraBrush = new SolidColorBrush(Colors.Gray);

            var unitIndex = 0;
            foreach (var unit in units)
            {
                var outOfBounds = CommonSimulation.IsOutOfBounds(unit);

                Brush brush = null;
                switch (unit.Species)
                {
                    case Species.Gazelle:
                        brush = gazelleBrush;
                        break;
                    case Species.Wildebeest:
                        brush = wildebeestBrush;
                        break;
                    case Species.Zebra:
                        brush = zebraBrush;
                        break;
                    case Species.Lion:
                        brush = lionBrush;
                        break;
                }

                if (firstLocations != null)
                    firstLocations.Add(unit.Location);
                else if (PathView)
                {
                    var line = new Line
                                {
                                    Stroke = brush,
                                    StrokeThickness = 0.2,
                                    X1 = unit.Location.X,
                                    X2 = _previousLocations[unitIndex].X,
                                    Y1 =  CommonSimulation.Height - unit.Location.Y,
                                    Y2 = CommonSimulation.Height - _previousLocations[unitIndex].Y
                                };
                    _previousLocations[unitIndex] = unit.Location;
                    if (!outOfBounds)
                        Children.Add(line);
                }

                if (!outOfBounds)
                {
                    var diameter = unit.BoundingRadius*2;
                    var shape = new Ellipse {Opacity = 0.9, Width = diameter, Height = diameter, Fill = brush};
                    SetLeft(shape, unit.Location.X - unit.BoundingRadius);
                    SetBottom(shape, unit.Location.Y - unit.BoundingRadius);
                    Children.Add(shape);
                }

                unitIndex++;
            }

            if (firstLocations != null)
                _previousLocations = firstLocations;

            RunningStatistics.Clear();
            RunningStatistics.Add("Time (ticks): " + CommonSimulation.TickCount + "/" + CommonSimulation.TickLimit);
            RunningStatistics.Add("Time (seconds): " + Math.Round(CommonSimulation.TickCount * CommonSimulation.TickTime) + "/" + CommonSimulation.TickLimit * CommonSimulation.TickTime);
            UpdateStatistics();
        }

        protected abstract void UpdateStatistics();
    }
}