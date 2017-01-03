using System;
using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    class Target : AnimateEntity
    {
        protected double Life;

        public Target(Vector2 location, double boundingRadius, double weight, double maxSpeed, double maxAcceleration, double agility, double life) : base(location, boundingRadius, weight, maxSpeed, maxAcceleration, agility)
        {
            Life = weight;
        }

        public void Damage(double amount)
        {
            Life = Math.Max(0, Life - amount);
        }

        public override bool IsDead()
        {
            return Life <= 0;
        }

        public double Consume(double weight)
        {
            var amount = Math.Min(Weight, weight);
            Weight -= amount;
            return amount;
        }
    }
}