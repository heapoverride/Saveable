using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Saveable
{
    public class SaveableObject
    {
        /// <summary>
        /// Read <see cref="SaveableObject"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Read(BinaryReader reader)
        {
            foreach (var prop in GetType().GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(SaveableAttribute)))
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, (string)Read<SaveableString>(reader), null);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(this, reader.ReadInt32(), null);
                    }
                }
            }
        }

        /// <summary>
        /// Write <see cref="SaveableObject"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Write(BinaryWriter writer) {
            foreach (var prop in GetType().GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(SaveableAttribute)))
                {
                    object value = prop.GetValue(this);

                    if (prop.PropertyType == typeof(string))
                    {
                        Write(writer, (SaveableString)(value as string));
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        writer.Write((int)value);
                    }
                }
            }
        }

        /// <summary>
        /// Read <see cref="SaveableObject"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Read<T>(BinaryReader reader) where T : SaveableObject
        {
            var saveableObject = Activator.CreateInstance<T>();
            saveableObject.Read(reader);
            return saveableObject;
        }

        /// <summary>
        /// Read array of <see cref="SaveableObject"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T[] ReadArray<T>(BinaryReader reader) where T : SaveableObject
        {
            var array = new T[reader.ReadInt32()];

            for (int i=0; i<array.Length; i++)
            {
                array[i] = Read<T>(reader);
            }

            return array;
        }

        /// <summary>
        /// Write <see cref="SaveableObject"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="saveableObject"></param>
        public static void Write(BinaryWriter writer, SaveableObject saveableObject) => saveableObject.Write(writer);

        /// <summary>
        /// Write array of <see cref="SaveableObject"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteArray(BinaryWriter writer, SaveableObject[] array)
        {
            writer.Write((int)array.Length);

            foreach (var obj in array)
            {
                obj.Write(writer);
            }
        }
    }
}
