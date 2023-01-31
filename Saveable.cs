using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveableDotNet
{
    /// <summary>
    /// Provides the base class for <see cref="Saveable"/> objects
    /// </summary>
    public class Saveable
    {
        #region Properties
        private long position;
        private long length;

        /// <summary>
        /// Gets the position in a <see cref="Stream"/> where the <see cref="Saveable"/> was read from or written to
        /// </summary>
        public long Position { get { return position; } }

        /// <summary>
        /// Gets the length of the <see cref="Saveable"/> that was read from or written to a <see cref="Stream"/>
        /// </summary>
        public long Length { get { return length; } }
        #endregion

        #region Overridable public virtual instance methods
        /// <summary>
        /// Read a <see cref="Saveable"/> from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        public virtual void Read(BinaryReader reader)
        {
            // Update position
            position = reader.BaseStream.Position;

            foreach (var prop in GetType().GetProperties())
            {
                // Skip properties not marked Saveable
                if (!Attribute.IsDefined(prop, typeof(SaveableAttribute))) continue;

                if (prop.PropertyType.IsArray)
                {
                    if (prop.PropertyType.GetArrayRank() != 1)
                    {
                        throw new Exception("Array must be a simple one-dimensional array.");
                    }
                    // simple array type
                    var elementType = prop.PropertyType.GetElementType();

                    // arrays of primitive data types
                    if (elementType == typeof(byte))
                    {
                        prop.SetValue(this, ReadByteArray(reader));
                    }
                    else if (elementType == typeof(char))
                    {
                        prop.SetValue(this, ReadCharArray(reader));
                    }
                    else if (elementType == typeof(string))
                    {
                        prop.SetValue(this, ReadStringArray(reader));
                    }
                    else if (elementType == typeof(short))
                    {
                        prop.SetValue(this, ReadInt16Array(reader));
                    }
                    else if (elementType == typeof(ushort))
                    {
                        prop.SetValue(this, ReadUInt16Array(reader));
                    }
                    else if (elementType == typeof(int))
                    {
                        prop.SetValue(this, ReadInt32Array(reader));
                    }
                    else if (elementType == typeof(uint))
                    {
                        prop.SetValue(this, ReadUInt32Array(reader));
                    }
                    else if (elementType == typeof(double))
                    {
                        prop.SetValue(this, ReadDoubleArray(reader));
                    }
                    else if (elementType == typeof(float))
                    {
                        prop.SetValue(this, ReadFloatArray(reader));
                    }
                    else if (elementType == typeof(decimal))
                    {
                        prop.SetValue(this, ReadDecimalArray(reader));
                    }

                    // saveable type
                    else if (typeof(Saveable).IsAssignableFrom(elementType))
                    {
                        var array = Array.CreateInstance(elementType, reader.ReadInt32());

                        for (int i = 0; i < array.Length; i++)
                        {
                            var saveableObject = Activator.CreateInstance(elementType);
                            ((Saveable)saveableObject).Read(reader);

                            array.SetValue(saveableObject, i);
                        }

                        prop.SetValue(this, array);
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

                    // primitive data types
                    if (prop.PropertyType == typeof(byte))
                    {
                        prop.SetValue(this, reader.ReadByte());
                    }
                    else if (prop.PropertyType == typeof(char))
                    {
                        prop.SetValue(this, reader.ReadChar());
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, ReadString(reader));
                    }
                    else if (prop.PropertyType == typeof(short))
                    {
                        prop.SetValue(this, reader.ReadInt16());
                    }
                    else if (prop.PropertyType == typeof(ushort))
                    {
                        prop.SetValue(this, reader.ReadUInt16());
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(this, reader.ReadInt32());
                    }
                    else if (prop.PropertyType == typeof(uint))
                    {
                        prop.SetValue(this, reader.ReadUInt32());
                    }
                    else if (prop.PropertyType == typeof(long))
                    {
                        prop.SetValue(this, reader.ReadInt64());
                    }
                    else if (prop.PropertyType == typeof(ulong))
                    {
                        prop.SetValue(this, reader.ReadUInt64());
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        prop.SetValue(this, reader.ReadDouble());
                    }
                    else if (prop.PropertyType == typeof(float))
                    {
                        prop.SetValue(this, reader.ReadSingle());
                    }
                    else if (prop.PropertyType == typeof(decimal))
                    {
                        prop.SetValue(this, reader.ReadDecimal());
                    }

                    // saveable type
                    else if (typeof(Saveable).IsAssignableFrom(prop.PropertyType))
                    {
                        var saveableObject = Activator.CreateInstance(prop.PropertyType);
                        ((Saveable)saveableObject).Read(reader);

                        prop.SetValue(this, saveableObject);
                    }

                    // throw error if type is not supported
                    else
                    {
                        throw new Exception($"Unsupported property type: {prop.PropertyType}");
                    }
                }
            }

            // Update length
            length = reader.BaseStream.Position - position;
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Write(BinaryWriter writer)
        {
            // Update position
            position = writer.BaseStream.Position;

            foreach (var prop in GetType().GetProperties())
            {
                // Skip properties not marked Saveable
                if (!Attribute.IsDefined(prop, typeof(SaveableAttribute))) continue;

                object value = prop.GetValue(this);

                if (prop.PropertyType.IsArray)
                {
                    if (prop.PropertyType.GetArrayRank() != 1)
                    {
                        throw new Exception("Array must be a simple one-dimensional array.");
                    }
                    // simple array type
                    var elementType = prop.PropertyType.GetElementType();

                    // arrays of primitive data types
                    if (elementType == typeof(byte))
                    {
                        WriteByteArray(writer, (byte[])value);
                    }
                    else if (elementType == typeof(char))
                    {
                        WriteCharArray(writer, (char[])value);
                    }
                    else if (elementType == typeof(string))
                    {
                        WriteStringArray(writer, (string[])value);
                    }
                    else if (elementType == typeof(short))
                    {
                        WriteInt16Array(writer, (short[])value);
                    }
                    else if (elementType == typeof(ushort))
                    {
                        WriteUInt16Array(writer, (ushort[])value);
                    }
                    else if (elementType == typeof(int))
                    {
                        WriteInt32Array(writer, (int[])value);
                    }
                    else if (elementType == typeof(uint))
                    {
                        WriteUInt32Array(writer, (uint[])value);
                    }
                    else if (elementType == typeof(long))
                    {
                        WriteInt64Array(writer, (long[])value);
                    }
                    else if (elementType == typeof(ulong))
                    {
                        WriteUInt64Array(writer, (ulong[])value);
                    }
                    else if (elementType == typeof(double))
                    {
                        WriteDoubleArray(writer, (double[])value);
                    }
                    else if (elementType == typeof(float))
                    {
                        WriteFloatArray(writer, (float[])value);
                    }
                    else if (elementType == typeof(decimal))
                    {
                        WriteDecimalArray(writer, (decimal[])value);
                    }

                    // saveable type
                    else if (typeof(Saveable).IsAssignableFrom(elementType))
                    {
                        Write(writer, (Saveable[])value);
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

                    // primitive data types
                    if (prop.PropertyType == typeof(byte))
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

                    // saveable type
                    else if (typeof(Saveable).IsAssignableFrom(prop.PropertyType))
                    {
                        Write(writer, (Saveable)value);
                    }

                    // throw error if type is not supported
                    else
                    {
                        throw new Exception($"Unsupported property type: {prop.PropertyType}");
                    }
                }
            }

            // Update length
            length = writer.BaseStream.Position - position;
        }
        #endregion

        #region Public instance methods
        /// <summary>
        /// Dump a <see cref="Saveable"/> to a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes() => GetBytes(this);
        #endregion

        #region Static read methods
        /// <summary>
        /// Read a <see cref="Saveable"/> from a <see cref="BinaryReader"/>
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
        /// Read a <see cref="Saveable"/> from a byte array
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
        /// Read an array of <see cref="Saveable"/> from a <see cref="BinaryReader"/>
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
        /// Read an array of <see cref="Saveable"/> from a byte array
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
        /// Get an enumerator that can be used to iterate over an array of <see cref="Saveable"/>
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

        /// <summary>
        /// Read a byte from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static byte ReadByte(BinaryReader reader) => reader.ReadByte();

        /// <summary>
        /// Read a char from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static char ReadChar(BinaryReader reader) => reader.ReadChar();

        /// <summary>
        /// Read a 16-bit signed integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static short ReadInt16(BinaryReader reader) => reader.ReadInt16();

        /// <summary>
        /// Read a 16-bit unsigned integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ushort ReadUInt16(BinaryReader reader) => reader.ReadUInt16();

        /// <summary>
        /// Read a 32-bit signed integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int ReadInt32(BinaryReader reader) => reader.ReadInt32();

        /// <summary>
        /// Read a 32-bit unsigned integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static uint ReadUInt32(BinaryReader reader) => reader.ReadUInt32();

        /// <summary>
        /// Read a 64-bit signed integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long ReadInt64(BinaryReader reader) => reader.ReadInt64();

        /// <summary>
        /// Read a 64-bit unsigned integer from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ulong ReadUInt64(BinaryReader reader) => reader.ReadUInt64();

        /// <summary>
        /// Read a double-precision floating-point value from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static double ReadDouble(BinaryReader reader) => reader.ReadDouble();

        /// <summary>
        /// Read a single-precision floating-point value from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static float ReadFloat(BinaryReader reader) => reader.ReadSingle();

        /// <summary>
        /// Read a decimal value from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static decimal ReadDecimal(BinaryReader reader) => reader.ReadDecimal();

        /// <summary>
        /// Read a length-prefixed byte array from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static byte[] ReadByteArray(BinaryReader reader) => reader.ReadBytes(reader.ReadInt32());

        /// <summary>
        /// Read a length-prefixed char array from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static char[] ReadCharArray(BinaryReader reader)
        {
            var bytes = ReadByteArray(reader);
            var array = new char[bytes.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (char)bytes[i];
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed string from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadString(BinaryReader reader) => Encoding.UTF8.GetString(ReadByteArray(reader));

        /// <summary>
        /// Read a length-prefixed array of length-prefixed strings from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string[] ReadStringArray(BinaryReader reader)
        {
            var array = new string[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadString(reader);
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 16-bit signed integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static short[] ReadInt16Array(BinaryReader reader)
        {
            var array = new short[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt16();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 16-bit unsigned integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ushort[] ReadUInt16Array(BinaryReader reader)
        {
            var array = new ushort[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt16();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 32-bit signed integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int[] ReadInt32Array(BinaryReader reader)
        {
            var array = new int[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt32();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 32-bit unsigned integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static uint[] ReadUInt32Array(BinaryReader reader)
        {
            var array = new uint[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt32();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 64-bit signed integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long[] ReadInt64Array(BinaryReader reader)
        {
            var array = new long[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadInt64();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 64-bit unsigned integers from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ulong[] ReadUInt64Array(BinaryReader reader)
        {
            var array = new ulong[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadUInt64();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of double-precision floating-point values from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static double[] ReadDoubleArray(BinaryReader reader)
        {
            var array = new double[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDouble();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of single-precision floating-point values from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static float[] ReadFloatArray(BinaryReader reader)
        {
            var array = new float[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadSingle();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of decimal values from a <see cref="BinaryReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static decimal[] ReadDecimalArray(BinaryReader reader)
        {
            var array = new decimal[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadDecimal();
            }

            return array;
        }
        #endregion

        #region Static write methods
        /// <summary>
        /// Write a <see cref="Saveable"/> to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="saveable"></param>
        public static void Write(BinaryWriter writer, Saveable saveable) => saveable.Write(writer);

        /// <summary>
        /// Write an array of <see cref="Saveable"/> to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void Write(BinaryWriter writer, Saveable[] array)
        {
            writer.Write((int)array.Length);

            foreach (var saveable in array)
            {
                saveable.Write(writer);
            }
        }

        /// <summary>
        /// Dump a <see cref="Saveable"/> to a byte array
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
        /// Dump an array of <see cref="Saveable"/> to a byte array
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

        /// <summary>
        /// Write a byte to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteByte(BinaryWriter writer, byte value) => writer.Write(value);

        /// <summary>
        /// Write a char to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteChar(BinaryWriter writer, char value) => writer.Write(value);

        /// <summary>
        /// Write a 16-bit signed integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteInt16(BinaryWriter writer, short value) => writer.Write(value);

        /// <summary>
        /// Write a 16-bit unsigned integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteUInt16(BinaryWriter writer, ushort value) => writer.Write(value);

        /// <summary>
        /// Write a 32-bit signed integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteInt32(BinaryWriter writer, int value) => writer.Write(value);

        /// <summary>
        /// Write a 32-bit unsigned integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteUInt32(BinaryWriter writer, uint value) => writer.Write(value);

        /// <summary>
        /// Write a 64-bit signed integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteInt64(BinaryWriter writer, long value) => writer.Write(value);

        /// <summary>
        /// Write a 64-bit unsigned integer to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteUInt64(BinaryWriter writer, ulong value) => writer.Write(value);

        /// <summary>
        /// Write a double-precision floating-point value to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteDouble(BinaryWriter writer, double value) => writer.Write(value);

        /// <summary>
        /// Write a single-precision floating-point value to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteFloat(BinaryWriter writer, float value) => writer.Write(value);

        /// <summary>
        /// Write a decimal value to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteDecimal(BinaryWriter writer, decimal value) => writer.Write(value);

        /// <summary>
        /// Write a length-prefixed byte array to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteByteArray(BinaryWriter writer, byte[] array)
        {
            writer.Write((int)array.Length);
            writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed char array to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteCharArray(BinaryWriter writer, char[] array)
        {
            writer.Write((int)array.Length);
            writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed string to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public static void WriteString(BinaryWriter writer, string value) => WriteByteArray(writer, Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Write a length-prefixed array of length-prefixed strings to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteStringArray(BinaryWriter writer, string[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                WriteString(writer, value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 16-bit signed integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteInt16Array(BinaryWriter writer, short[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 16-bit unsigned integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteUInt16Array(BinaryWriter writer, ushort[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 32-bit signed integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteInt32Array(BinaryWriter writer, int[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 32-bit unsigned integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteUInt32Array(BinaryWriter writer, uint[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 64-bit signed integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteInt64Array(BinaryWriter writer, long[] array)
        {
            writer.Write((int)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 64-bit unsigned integers to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteUInt64Array(BinaryWriter writer, ulong[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of double-precision floating-point values to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteDoubleArray(BinaryWriter writer, double[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of single-precision floating-point values to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteFloatArray(BinaryWriter writer, float[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of decimal values to a <see cref="BinaryWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="array"></param>
        public static void WriteDecimalArray(BinaryWriter writer, decimal[] array)
        {
            writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                writer.Write(value);
            }
        }
        #endregion
    }
}
