using System;
using System.Linq;

namespace GeneticAlgorithms.Utility
{
    /// <summary>
    /// Provides utility methods for retrieving attributes from Type objects.
    /// </summary>
    public static class AttributeUtility
    {
        /// <summary>
        /// Gets the first attribute of type T from the given type, without inheriting.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        /// <param name="source">The source Type object.</param>
        /// <returns>The first attribute, or null if there isn't one of the given type.</returns>
        public static T GetAttribute<T>(this Type source) where T : Attribute
        {
            var attribute = source.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Gets all the attributes of type T from the given type, including inherited attributes.
        /// </summary>
        /// <typeparam name="T">The type of attribute to retrieve.</typeparam>
        /// <param name="source">The source Type object.</param>
        /// <returns>The array of attributes obtained.</returns>
        public static T[] GetAttributes<T>(this Type source) where T : Attribute
        {
            return source.GetCustomAttributes(typeof(T), true) as T[];
        }
    }
}