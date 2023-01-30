using System;

namespace SaveableDotNet
{
    /// <summary>
    /// Automatically read/write public properties with Saveable attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveableAttribute : Attribute
    {
    }
}