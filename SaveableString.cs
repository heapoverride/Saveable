using System.IO;
using System.Text;

namespace Saveable
{
    public class SaveableString : SaveableObject
    {
        /// <summary>
        /// Gets or sets the underlying <see cref="string"/> value
        /// </summary>
        public string Value { get; set; }

        public SaveableString() { }

        public SaveableString(string value) { 
            Value = value; 
        }

        /// <summary>
        /// Implicit conversion from string to SaveableString
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SaveableString(string value) => new SaveableString(value);

        /// <summary>
        /// Implicit conversion from SaveableString to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(SaveableString value) => value.Value;

        public override void Read(BinaryReader reader)
        {
            Value = ReadString(reader);
        }

        public override void Write(BinaryWriter writer) => WriteString(writer, Value);

        public override string ToString() => Value;
    }
}
