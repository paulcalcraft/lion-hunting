using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    abstract class Entity
    {
        public Vector2 Location { get; set; }
        public double BoundingRadius { get; protected set; }
        public double Weight { get; protected set; }

        protected Entity(Vector2 location, double boundingRadius, double weight)
        {
            Location = location;
            BoundingRadius = boundingRadius;
            Weight = weight;
        }

        public virtual bool IsDead()
        {
            return false;
        }

        public bool Overlaps(Entity other)
        {
            return (other.Location - Location).Magnitude() < (BoundingRadius + other.BoundingRadius);
        }
    }
}