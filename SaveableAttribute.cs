using System;

namespace SaveableDotNet
{
    /// <summary>
    /// Saveable attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SaveableAttribute : Attribute { }
}