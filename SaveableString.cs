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

        public SaveableString(string value)
        {
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
            Value = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((int)Value.Length);
            writer.Write(Encoding.UTF8.GetBytes(Value));
        }

        public override string ToString() => Value;
    }
}
