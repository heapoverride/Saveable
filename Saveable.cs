using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveableDotNet
{
    public class Saveable
    {
        #region Properties
        private long position;

        /// <summary>
        /// Gets the <see cref="Stream"/> position where the <see cref="Saveable"/> was read from
        /// </summary>
        public long Position { get { return position; } }
        #endregion

        #region Overridable public virtual instance methods
        /// <summary>
        /// Read <see cref="Saveable"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Read(BinaryReader reader)
        {
            position = reader.BaseStream.Position;

            foreach (var prop in GetType().GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(SaveableAttribute)))
                {
                    if (prop.PropertyType.IsArray)
                    {
                        if (prop.PropertyType.GetArrayRank() != 1)
                        {
                            throw new Exception("Array must be a simple one-dimensional array.");
                        }
                        // simple array type

                        // saveable type
                        if (typeof(Saveable).IsAssignableFrom(prop.PropertyType.GetElementType()))
                        {
                            var array = Array.CreateInstance(prop.PropertyType.GetElementType(), reader.ReadInt32());

                            for (int i = 0; i < array.Length; i++)
                            {
                                var saveableObject = Activator.CreateInstance(prop.PropertyType.GetElementType());
                                ((Saveable)saveableObject).Read(reader);

                                array.SetValue(saveableObject, i);
                            }

                            prop.SetValue(this, array, null);
                        }

                        // arrays of primitive data types
                        else if (prop.PropertyType == typeof(byte[]))
                        {
                            prop.SetValue(this, ReadByteArray(reader), null);
                        }
                        else if (prop.PropertyType == typeof(char[]))
                        {
                            prop.SetValue(this, ReadCharArray(reader), null);
                        }
                        else if (prop.PropertyType == typeof(string[]))
                        {
                            prop.SetValue(this, ReadStringArray(reader), null);
                        }
                        else if (prop.PropertyType == typeof(short[]))
                        {
                            prop.SetValue(this, ReadInt16Array(reader), null);
                        }
                        else if (prop.PropertyType == typeof(ushort[]))
                        {
                            prop.SetValue(this, ReadUInt16Array(reader), null);
                        }
                        else if (prop.PropertyType == typeof(int[]))
                        {
                            prop.SetValue(this, ReadInt32Array(reader), null);
                        }
                        else if (prop.PropertyType == typeof(uint[]))
                        {
                            prop.SetValue(this, ReadUInt32Array(reader), null);
                        }
                        else if (prop.PropertyType == typeof(double[]))
                        {
                            prop.SetValue(this, ReadDoubleArray(reader), null);
                        }
                        else if (prop.PropertyType == typeof(float[]))
                        {
                            prop.SetValue(this, ReadFloatArray(reader), null);
                        }
                        else if (prop.PropertyType == typeof(decimal[]))
                        {
                            prop.SetValue(this, ReadDecimalArray(reader), null);
                        }

                        // throw error if type is not supported
                        else
                        {
                            throw new Exception($"Unsupported property type: {prop.PropertyType}");
                        }
                    }
                    else
                    {
                        // non-array type

                        // saveable type
                        if (typeof(Saveable).IsAssignableFrom(prop.PropertyType))
                        {
                            var saveableObject = Activator.CreateInstance(prop.PropertyType);
                            ((Saveable)saveableObject).Read(reader);

                            prop.SetValue(this, saveableObject, null);
                        }

                        // primitive data types
                        else if (prop.PropertyType == typeof(byte))
                        {
                            prop.SetValue(this, reader.ReadByte(), null);
                        }
                        else if (prop.PropertyType == typeof(char))
                        {
                            prop.SetValue(this, reader.ReadChar(), null);
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(this, ReadString(reader), null);
                        }
                        else if (prop.PropertyType == typeof(short))
                        {
                            prop.SetValue(this, reader.ReadInt16(), null);
                        }
                        else if (prop.PropertyType == typeof(ushort))
                        {
                            prop.SetValue(this, reader.ReadUInt16(), null);
                        }
                        else if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(this, reader.ReadInt32(), null);
                        }
                        else if (prop.PropertyType == typeof(uint))
                        {
                            prop.SetValue(this, reader.ReadUInt32(), null);
                        }
                        else if (prop.PropertyType == typeof(long))
                        {
                            prop.SetValue(this, reader.ReadInt64(), null);
                        }
                        else if (prop.PropertyType == typeof(ulong))
                        {
                            prop.SetValue(this, reader.ReadUInt64(), null);
                        }
                        else if (prop.PropertyType == typeof(double))
                        {
                            prop.SetValue(this, reader.ReadDouble(), null);
                        }
                        else if (prop.PropertyType == typeof(float))
                        {
                            prop.SetValue(this, reader.ReadSingle(), null);
                        }
                        else if (prop.PropertyType == typeof(decimal))
                        {
                            prop.SetValue(this, reader.ReadDecimal(), null);
                        }

                        // throw error if type is not supported
                        else
                        {
                            throw new Exception($"Unsupported property type: {prop.PropertyType}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write <see cref="Saveable"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Write(BinaryWriter writer)
        {
            foreach (var prop in GetType().GetProperties())
            {
                if (Attribute.IsDefined(prop, typeof(SaveableAttribute)))
                {
                    object value = prop.GetValue(this);

                    if (prop.PropertyType.IsArray)
                    {
                        if (prop.PropertyType.GetArrayRank() != 1)
                        {
                            throw new Exception("Array must be a simple one-dimensional array.");
                        }
                        // simple array type

                        // saveable type
                        if (typeof(Saveable).IsAssignableFrom(prop.PropertyType.GetElementType()))
                        {
                            Write(writer, (Saveable[])value);
                        }

                        // arrays of primitive data types
                        else if (prop.PropertyType == typeof(byte[]))
                        {
                            WriteByteArray(writer, (byte[])value);
                        }
                        else if (prop.PropertyType == typeof(char[]))
                        {
                            WriteCharArray(writer, (char[])value);
                        }
                        else if (prop.PropertyType == typeof(string[]))
                        {
                            WriteStringArray(writer, (string[])value);
                        }
                        else if (prop.PropertyType == typeof(short[]))
                        {
                            WriteInt16Array(writer, (short[])value);
                        }
                        else if (prop.PropertyType == typeof(ushort[]))
                        {
                            WriteUInt16Array(writer, (ushort[])value);
                        }
                        else if (prop.PropertyType == typeof(int[]))
                        {
                            WriteInt32Array(writer, (int[])value);
                        }
                        else if (prop.PropertyType == typeof(uint[]))
                        {
                            WriteUInt32Array(writer, (uint[])value);
                        }
                        else if (prop.PropertyType == typeof(long[]))
                        {
                            WriteInt64Array(writer, (long[])value);
                        }
                        else if (prop.PropertyType == typeof(ulong[]))
                        {
                            WriteUInt64Array(writer, (ulong[])value);
                        }
                        else if (prop.PropertyType == typeof(double[]))
                        {
                            WriteDoubleArray(writer, (double[])value);
                        }
                        else if (prop.PropertyType == typeof(float[]))
                        {
                            WriteFloatArray(writer, (float[])value);
                        }
                        else if (prop.PropertyType == typeof(decimal[]))
                        {
                            WriteDecimalArray(writer, (decimal[])value);
                        }

                        // throw error if type is not supported
                        else
                        {
                            throw new Exception($"Unsupported property type: {prop.PropertyType}");
                        }
                    }
                    else
                    {
                        // non-array type

                        // saveable type
                        if (typeof(Saveable).IsAssignableFrom(prop.PropertyType))
                        {
                            Write(writer, (Saveable)value);
                        }

                        // primitive data types
                        else if (prop.PropertyType == typeof(byte))
                        {
                            writer.Write((byte)value);
                        }
                        else if (prop.PropertyType == typeof(char))
                        {
                            writer.Write((char)value);
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            WriteString(writer, (string)value);
                        }
                        else if (prop.PropertyType == typeof(short))
                        {
                            writer.Write((short)value);
                        }
                        else if (prop.PropertyType == typeof(ushort))
                        {
                            writer.Write((ushort)value);
                        }
                        else if (prop.PropertyType == typeof(int))
                        {
                            writer.Write((int)value);
                        }
                        else if (prop.PropertyType == typeof(uint))
                        {
                            writer.Write((uint)value);
                        }
                        else if (prop.PropertyType == typeof(long))
                        {
                            writer.Write((long)value);
                        }
                        else if (prop.PropertyType == typeof(ulong))
                        {
                            writer.Write((ulong)value);
                        }
                        else if (prop.PropertyType == typeof(double))
                        {
                            writer.Write((double)value);
                        }
                        else if (prop.PropertyType == typeof(float))
                        {
                            writer.Write((float)value);
                        }
                        else if (prop.PropertyType == typeof(decimal))
                        {
                            writer.Write((decimal)value);
                        }

                        // throw error if type is not supported
                        else
                        {
                            throw new Exception($"Unsupported property type: {prop.PropertyType}");
                        }
                    }
                }
            }
        }
        #endregion

        #region Public instance methods
        /// <summary>
        /// Dump <see cref="Saveable"/> to a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes() => GetBytes(this);
        #endregion

        #region Protected read methods
        protected byte[] ReadByteArray(BinaryReader reader) => reader.ReadBytes(reader.ReadInt32());

        protected char[] ReadCharArray(BinaryReader reader) {
            var bytes = ReadByteArray(reader);
            var array = new char[bytes.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (char)bytes[i];
            }

            return array;
        }

        protected string ReadString(BinaryReader reader) => Encoding.UTF8.GetString(ReadByteArray(reader));

        protected string[] ReadStringArray(BinaryReader reader)
        {
            var array = new string[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadString(reader);
            }

            return array;
        }

        protected short[] ReadInt16Array(BinaryReader reader)
        {
            var array = new short[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt16();
            }

            return array;
        }

        protected ushort[] ReadUInt16Array(BinaryReader reader)
        {
            var array = new ushort[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt16();
            }

            return array;
        }

        protected int[] ReadInt32Array(BinaryReader reader)
        {
            var array = new int[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt32();
            }

            return array;
        }

        protected uint[] ReadUInt32Array(BinaryReader reader)
        {
            var array = new uint[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt32();
            }

            return array;
        }

        protected long[] ReadInt64Array(BinaryReader reader)
        {
            var array = new long[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt64();
            }

            return array;
        }

        protected ulong[] ReadUInt64Array(BinaryReader reader)
        {
            var array = new ulong[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt64();
            }

            return array;
        }

        protected double[] ReadDoubleArray(BinaryReader reader)
        {
            var array = new double[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDouble();
            }

            return array;
        }

        protected float[] ReadFloatArray(BinaryReader reader)
        {
            var array = new float[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadSingle();
            }

            return array;
        }

        protected decimal[] ReadDecimalArray(BinaryReader reader)
        {
            var array = new decimal[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDecimal();
            }

            return array;
        }
        #endregion

        #region Protected write methods
        protected void WriteByteArray(BinaryWriter writer, byte[] array)
        {
            writer.Write((int)array.Length);
            writer.Write(array);
        }

        protected void WriteCharArray(BinaryWriter writer, char[] array)
        {
            writer.Write((int)array.Length);
            writer.Write(array);
        }

        protected void WriteString(BinaryWriter writer, string value) => WriteByteArray(writer, Encoding.UTF8.GetBytes(value));

        protected void WriteStringArray(BinaryWriter writer, string[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                WriteString(writer, value);
            }
        }

        protected void WriteInt16Array(BinaryWriter writer, short[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteUInt16Array(BinaryWriter writer, ushort[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteInt32Array(BinaryWriter writer, int[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteUInt32Array(BinaryWriter writer, uint[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteInt64Array(BinaryWriter writer, long[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteUInt64Array(BinaryWriter writer, ulong[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteDoubleArray(BinaryWriter writer, double[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteFloatArray(BinaryWriter writer, float[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        protected void WriteDecimalArray(BinaryWriter writer, decimal[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }
        #endregion

        #region Static read methods
        /// <summary>
        /// Read <see cref="Saveable"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Read<T>(BinaryReader reader) where T : Saveable
        {
            var saveable = Activator.CreateInstance<T>();
            saveable.Read(reader);
            return saveable;
        }

        /// <summary>
        /// Read <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Read<T>(byte[] array) where T : Saveable
        {
            using (var stream = new MemoryStream(array))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return Read<T>(reader);
                }
            }
        }

        /// <summary>
        /// Read array of <see cref="Saveable"/> from <see cref="BinaryReader"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T[] ReadArray<T>(BinaryReader reader) where T : Saveable
        {
            var array = new T[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Read<T>(reader);
            }

            return array;
        }

        /// <summary>
        /// Read array of <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] ReadArray<T>(byte[] array) where T : Saveable
        {
            using (var stream = new MemoryStream(array))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var _array = new T[reader.ReadInt32()];

                    for (int i = 0; i < array.Length; i++)
                    {
                        _array[i] = Read<T>(reader);
                    }

                    return _array;
                }
            }
        }

        /// <summary>
        /// Get an enumerator that can be used to iterate over array of <see cref="Saveable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumerator<T>(BinaryReader reader) where T : Saveable
        {
            int length = reader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                yield return Read<T>(reader);
            }
        }
        #endregion

        #region Static write methods
        /// <summary>
        /// Write <see cref="Saveable"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="saveable"></param>
        public static void Write(BinaryWriter writer, Saveable saveable) => saveable.Write(writer);

        /// <summary>
        /// Write array of <see cref="Saveable"/> to <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void Write(BinaryWriter writer, Saveable[] array)
        {
            writer.Write((int)array.Length);

            foreach (var obj in array)
            {
                obj.Write(writer);
            }
        }

        /// <summary>
        /// Dump <see cref="Saveable"/> to a byte array
        /// </summary>
        /// <param name="saveable"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Saveable saveable)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    Write(writer, saveable);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Dump array of <see cref="Saveable"/> to a byte array
        /// </summary>
        /// <param name="saveable"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Saveable[] saveable)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    Write(writer, saveable);
                }

                return stream.ToArray();
            }
        }
        #endregion
    }
}
