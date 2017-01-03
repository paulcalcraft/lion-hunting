using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using GeneticAlgorithms.Simulation;
using LionHunting.Simulations.Entities;

namespace LionHunting.Simulations.Full
{
    class LionVisualiser : SimulationVisualiser
    {
        private LionSimulation _simulation;

        public override EvolvableSimulation Simulation
        {
            get { return _simulation; }
            set
            {
                _simulation = (LionSimulation)value;
                Update();
            }
        }

        public LionVisualiser()
        {
            Width = LionSimulation.Width;
            Height = LionSimulation.Height;
            Background = new SolidColorBrush(Colors.White);
        }

        public override void Update()
        {
            Children.Clear();

            var lionBrush = new SolidColorBrush(Colors.OrangeRed);
            var gazelleBrush = new SolidColorBrush(Colors.Orange);
            var wildebeestBrush = new SolidColorBrush(Colors.Brown);
            var zebraBrush = new SolidColorBrush(Colors.Gray);
            var deadMeatBrush = new SolidColorBrush(Colors.Black);

            var totalMeatConsumed = 0d;
            var lionCount = 0;
            foreach (var entity in _simulation.Lions.Cast<AnimateEntity>().Concat(_simulation.Targets.Cast<AnimateEntity>()))
            {
                var shape = new Ellipse();

                if (entity.IsDead())
                    shape.Fill = deadMeatBrush;
                else if (entity is Lion)
                {
                    shape.Fill = lionBrush;
                    totalMeatConsumed += (entity as Lion).AmountConsumed;
                    lionCount++;
                }
                else if (entity is Gazelle)
                    shape.Fill = gazelleBrush;
                else if (entity is Wildebeest)
                    shape.Fill = wildebeestBrush;
                else if (entity is Zebra)
                    shape.Fill = zebraBrush;

                shape.Opacity = 0.9;

                shape.Width = shape.Height = entity.BoundingRadius * 2;
                Children.Add(shape);
                SetLeft(shape, entity.Location.X - entity.BoundingRadius);
                SetBottom(shape, entity.Location.Y - entity.BoundingRadius);
            }

            RunningStatistics.Clear();
            RunningStatistics.Add("Time (ticks): " + _simulation.TickCount + "/" + _simulation.TickLimit);
            //RunningStatistics.Add("Time (seconds): " + Math.Round(_simulation.TickCount * LionSimulation.TickTime) + "/" + _simulation.TickLimit * LionSimulation.TickTime);
            RunningStatistics.Add("Average meat consumed (kg): " + Math.Round(totalMeatConsumed/lionCount, 1));
            RunningStatistics.Add("Total meat consumed (kg): " + Math.Round(totalMeatConsumed, 1));
        }
    }
}