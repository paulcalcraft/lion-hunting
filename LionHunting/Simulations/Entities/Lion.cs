using LionHunting.Simulations.Full;
using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    // Panthera leo
    // Principals of Animal Locomotion
    // http://books.google.co.uk/books?id=gWU4ubSay8EC&dq=Principles+of+Animal+Locomotion&printsec=frontcover&source=bn&hl=en&ei=HglKS8fjHIy60gSpu7mQDw&sa=X&oi=book_result&ct=result&resnum=5&ved=0CCMQ6AEwBA#v=onepage&q=acceleration&f=false
    // Page 3 gives acceleration of lion and gazelle.
    // For lion it's 1480.1 ms^2.kg. For gazelle it's 92.25. What a difference! :S
    // Page 4 and beyond discusses endurance etc. which could be very useful.
    class Lion : AnimateEntity
    {
        public LionChromosome Chromosome { get; set; }
        public double AmountConsumed { get; private set; }

        public Lion(LionChromosome chromosome, Vector2 location) : base(location, 0.975, 155.8, 15.9, 9.5, 0)
        {
            Chromosome = chromosome;
            AmountConsumed = 0;
        }

        public void Consume(double weight)
        {
            AmountConsumed += weight;
        }
    }
}