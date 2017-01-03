using System;
using System.Diagnostics;

namespace LionHunting.Utility
{
    /// <summary>
    /// Represents a 2D vector with double floating point precision.
    /// </summary>
    class Vector2
    {
        //private const double Epsilon = 0.001;
        private double _x;
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>
        /// Constructs a new Vector2 with the given X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vector2(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Constructs a new Vector2 with zeros.
        /// </summary>
        public Vector2()
        {
            
        }

        /// <summary>
        /// Dots this vector with the given vector.
        /// </summary>
        /// <param name="b">The vector to dot with.</param>
        /// <returns>The scalar dot product result.</returns>
        public double Dot(Vector2 b)
        {
            return _x*b._x + _y*b._y;
        }

        /*public double PerpDot(Vector2 b)
        {
            return _x*b._y - _y*b._x;
        }

        // http://www.flaviusalecu.com/?p=29
        public double SignedAngleTo(Vector2 b)
        {
            return Math.Atan2(PerpDot(b), Dot(b));
        }

        public double CounterClockFaceAngle()
        {
            // Assert unit vector.
            var magnitude = Magnitude();
            Debug.Assert(magnitude > 1 - Epsilon && magnitude < 1 + Epsilon);
            // Shortcut for: new Vector2(0, 1).SignedAngleTo(this)
            var signedAngle = Math.Atan2(-_x, _y);
            return signedAngle >= 0 ? signedAngle : signedAngle + 2*Math.PI;
        }

        public Vector2 Rotate(double theta)
        {
            var cosTheta = Math.Cos(theta);
            var sinTheta = Math.Sin(theta);
            return new Vector2(_x*cosTheta - _y*sinTheta, _x*sinTheta + _y*cosTheta);
        }

        public static Vector2 CreateCounterClockFaceUnitVector(double theta)
        {
            return new Vector2(-Math.Sin(theta), Math.Cos(theta));
        }*/

        /// <summary>
        /// Calculates the magnitude of this vector.
        /// </summary>
        /// <returns>The magnitude of this vector.</returns>
        public double Magnitude()
        {
            return Math.Sqrt(SquaredMagnitude());
        }

        /// <summary>
        /// Calculates the squared magnitude of this vector.
        /// </summary>
        /// <returns>The square of the magnitude of this vector.</returns>
        public double SquaredMagnitude()
        {
            return _x * _x + _y * _y;
        }

        /// <summary>
        /// Calculates the normalised form of this vector.
        /// </summary>
        /// <returns>The normalised unit vector.</returns>
        public Vector2 Normalise()
        {
            var magnitude = Magnitude();
            Debug.Assert(magnitude > 0);
            return new Vector2(_x/magnitude, _y/magnitude);
        }

        /// <summary>
        /// Calculates the normalised form of this vector and returns the original vector's magnitude in the out variable.
        /// </summary>
        /// <returns>The normalised unit vector.</returns>
        public Vector2 Normalise(out double magnitude)
        {
            magnitude = Magnitude();
            Debug.Assert(magnitude > 0);
            return new Vector2(_x / magnitude, _y / magnitude);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a._x + b._x, a._y + b._y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a._x - b._x, a._y - b._y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a._x, -a._y);
        }

        public static Vector2 operator *(Vector2 a, double scalar)
        {
            return new Vector2(a._x * scalar, a._y * scalar);
        }

        public static Vector2 operator *(double scalar, Vector2 a)
        {
            return new Vector2(a._x * scalar, a._y * scalar);
        }

        public static Vector2 operator /(Vector2 a, double scalar)
        {
            return new Vector2(a._x / scalar, a._y / scalar);
        }

        public static Vector2 operator /(double scalar, Vector2 a)
        {
            return new Vector2(scalar / a._x, scalar / a._y);
        }

        /// <summary>
        /// Calculates the vector that points from this vector to a target vector.
        /// </summary>
        /// <param name="target">The target vector.</param>
        /// <returns>The vector from this to the target.</returns>
        public Vector2 To(Vector2 target)
        {
            return target - this;
        }

        /// <summary>
        /// Calculates the vector that points to this vector from a source vector.
        /// </summary>
        /// <param name="source">The source vector.</param>
        /// <returns>The vector to this from the source.</returns>
        public Vector2 From(Vector2 source)
        {
            return this - source;
        }

        public static double NormalisedRelativeAngle(Vector2 a, Vector2 b)
        {
            var angle = SafeAcos(b.Normalise().Dot(a.Normalise()));
            return 2 * (0.5d - Math.Abs(0.5d - angle / (2d * Math.PI)));
        }

        private static double SafeAcos(double d)
        {
            var result = Math.Acos(Math.Min(1, Math.Max(-1, d)));
            Debug.Assert(!Double.IsNaN(result));
            return result;
        }
    }
}