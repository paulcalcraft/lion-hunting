using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    // Equus burchelli
    class Zebra : Target
    {
        public Zebra(Vector2 location)
            : base(location, 1.175, 235, 18.9, 2, 0, 0)
        {
        }
    }
}