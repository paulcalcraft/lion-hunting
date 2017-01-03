using System;
using System.Diagnostics;
using LionHunting.Utility;

namespace LionHunting.Simulations.Entities
{
    class AnimateEntity : Entity
    {
        private Vector2 _velocity;
        public Vector2 Velocity
        {
            get { return _velocity; }
            protected set { _velocity = value; }
        }

        private Vector2 _intendedVelocity;
        public Vector2 IntendedVelocity
        {
            get { return _intendedVelocity; }
            protected set { _intendedVelocity = value; }
        }

        protected double MaxSpeed;
        protected double MaxAcceleration;
        protected double Agility;
        

        public AnimateEntity(Vector2 location, double boundingRadius, double weight, double maxSpeed, double maxAcceleration, double agility) : base(location, boundingRadius, weight)
        {
            MaxSpeed = maxSpeed;
            MaxAcceleration = maxAcceleration;
        }

        public void SetIntendedVelocity(double speedFactor, Vector2 direction)
        {
            var speed = Math.Min(MaxSpeed, speedFactor*MaxSpeed);
            IntendedVelocity = direction*speed;
        }

        private Vector2 CreateAccelerationVector()
        {
            var idealAccelerationVector = IntendedVelocity - Velocity;
            var normalisedRelativeAngle = Vector2.NormalisedRelativeAngle(IntendedVelocity, Velocity);
            var agilityBoost = 1+normalisedRelativeAngle*Agility;
            Debug.Assert(!Double.IsNaN(agilityBoost));
            var idealAcceleration = idealAccelerationVector.Magnitude();
            if (idealAcceleration <= MaxAcceleration)
                return idealAccelerationVector*agilityBoost;

            return idealAccelerationVector*(MaxAcceleration/idealAcceleration)*agilityBoost;
        }

        public void Advance(double dt)
        {
            var a = CreateAccelerationVector();
            Location += (Velocity*dt) + (a/2*Math.Pow(dt, 2)); // s = ut + 1/2at^2
            Velocity += a*dt;
        }
    }
}