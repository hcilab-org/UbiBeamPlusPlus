
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// A mutable two pair tuple.
    /// </summary>
    /// <typeparam name="T1">The first item</typeparam>
    /// <typeparam name="T2">The second item</typeparam>
    public class Pair<T1, T2>
    {
        /// <summary>
        /// Get or set the first item.
        /// </summary>
        public T1 First { get; set; }

        /// <summary>
        /// Get or set the second item.
        /// </summary>
        public T2 Second { get; set; }

        /// <summary>
        /// Create a new pair.
        /// </summary>
        /// <param name="Item1">The reference to the first item.</param>
        /// <param name="Item2">The reference to the second item.</param>
        public Pair(T1 Item1, T2 Item2)
        {
            this.First = Item1;
            this.Second = Item2;
        }
    }

    /// <summary>
    /// A mutable three pair tuple.
    /// </summary>
    /// <typeparam name="T1">The first item</typeparam>
    /// <typeparam name="T2">The second item</typeparam>
    /// <typeparam name="T3">The third item</typeparam>
    public class Triple<T1, T2, T3>
    {
        /// <summary>
        /// Get or set the first item.
        /// </summary>
        public T1 First { get; set; }

        /// <summary>
        /// Get or set the second item.
        /// </summary>
        public T2 Second { get; set; }

        /// <summary>
        /// Get or set the second item.
        /// </summary>
        public T3 Third { get; set; }

        /// <summary>
        /// Create a new pair.
        /// </summary>
        /// <param name="Item1">The reference to the first item.</param>
        /// <param name="Item2">The reference to the second item.</param>
        public Triple(T1 Item1, T2 Item2, T3 Item3)
        {
            this.First = Item1;
            this.Second = Item2;
            this.Third = Item3;
        }
    }
}
