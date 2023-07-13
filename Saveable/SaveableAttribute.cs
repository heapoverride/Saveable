using System;

namespace Saveable.NET
{
    /// <summary>
    /// Saveable attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveableAttribute : Attribute {
        /// <summary>
        /// Saveable offset
        /// </summary>
        public int Offset { get; set; }
    }
}