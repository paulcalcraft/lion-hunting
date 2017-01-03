using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    // Connochaetes gnu
    class Wildebeest : Target
    {
        public Wildebeest(Vector2 location)
            : base(location, 1.115, 300, 24.2, 2, 1, 4)
        {
        }
    }
}