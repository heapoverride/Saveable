using System;

namespace SaveableDotNet
{
    /// <summary>
    /// Saveable attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveableAttribute : Attribute
    {
        /// <summary>
        /// Saveable position in stream
        /// </summary>
        /// <value></value>
        public int Position { get; set; } = -1;
    }
}