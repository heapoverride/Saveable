using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveableNET
{
    /// <summary>
    /// Represents the base class for <see cref="Saveable"/> objects
    /// </summary>
    public abstract partial class Saveable
    {
        private long position;
        private long length;

        /// <summary>
        /// Gets the position in a <see cref="Stream"/> where the <see cref="Saveable"/> was read from or written to
        /// </summary>
        public long Position { get { return position; } }

        /// <summary>
        /// Gets the length of the <see cref="Saveable"/> that was read from or written to <see cref="Stream"/>
        /// </summary>
        public long Length { get { return length; } }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void Read(ReadContext ctx)
        {
            foreach (var prop in GetType().GetProperties())
            {
                // Try to get Saveable attribute
                var attribute = (SaveableAttribute)Attribute.GetCustomAttribute(prop, typeof(SaveableAttribute));

                // Skip properties with no Saveable attribute
                if (attribute == null) continue;

                // Add offset to current stream position if greater than zero
                AddOffset(ctx, attribute.Offset);

                // Read and set value
                prop.SetValue(this, ReadValue(ctx, prop.PropertyType));
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        public void ReadFrom(Stream stream)
        {
            // Create read context
            using (var ctx = new ReadContext(stream, true))
            {
                ReadFrom(ctx);
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from byte array
        /// </summary>
        /// <param name="array"></param>
        public void ReadFrom(byte[] array)
        {
            using (var stream = new MemoryStream(array))
            {
                ReadFrom(stream);
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        public void ReadFrom(ReadContext ctx)
        {
            // Update position
            if (ctx.Stream.CanSeek)
                position = ctx.Stream.Position;

            // Read saveable
            Read(ctx);

            // Update length
            if (ctx.Stream.CanSeek)
                length = ctx.Stream.Position - position;
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        protected virtual void Write(WriteContext ctx)
        {
            foreach (var prop in GetType().GetProperties())
            {
                // Try to get Saveable attribute
                var attribute = (SaveableAttribute)Attribute.GetCustomAttribute(prop, typeof(SaveableAttribute));

                // Skip properties with no Saveable attribute
                if (attribute == null) continue;

                // Add offset to current stream position if greater than zero
                AddOffset(ctx, attribute.Offset);

                // Get and write value
                WriteValue(ctx, prop.GetValue(this));
            }
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        public void WriteTo(Stream stream)
        {
            using (var ctx = new WriteContext(stream, true))
            {
                WriteTo(ctx);
            }
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        public void WriteTo(WriteContext ctx)
        {
            // Update position
            if (ctx.Stream.CanSeek)
                position = ctx.Stream.Position;

            // Write saveable
            Write(ctx);

            // Update length
            if (ctx.Stream.CanSeek)
                length = ctx.Stream.Position - position;
        }

        /// <summary>
        /// Dump a <see cref="Saveable"/> to byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes() => GetBytes(this);

        /// <summary>
        /// Add an offset to the <see cref="Context"/>'s <see cref="Stream"/> position
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="offset"></param>
        /// <remarks>
        /// Offset must be greater than zero
        /// </remarks>
        public static void AddOffset(Context ctx, int offset) {
            if (offset > 0 && ctx.Stream.CanSeek)
            {
                ctx.Stream.Position += offset;
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="TSaveable"></typeparam>
        /// <returns></returns>
        public static TSaveable Read<TSaveable>(Stream stream) where TSaveable : Saveable, new()
        {
            // Create read context
            using (var ctx = new ReadContext(stream, true))
            {
                // Read saveable
                return Read<TSaveable>(ctx);
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static TSaveable Read<TSaveable>(ReadContext ctx) where TSaveable : Saveable, new()
        {
            var saveable = new TSaveable();
            saveable.ReadFrom(ctx);
            return saveable;
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static TSaveable Read<TSaveable>(byte[] array) where TSaveable : Saveable, new()
        {
            using (var stream = new MemoryStream(array))
            {
                return Read<TSaveable>(stream);
            }
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="TSaveable"></typeparam>
        /// <returns></returns>
        public static TSaveable[] ReadArray<TSaveable>(Stream stream) where TSaveable : Saveable, new()
        {
            // Create read context
            using (var ctx = new ReadContext(stream, true))
            {
                // Read saveables
                return ReadArray<TSaveable>(ctx);
            }
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static TSaveable[] ReadArray<TSaveable>(ReadContext ctx) where TSaveable : Saveable, new()
        {
            var array = new TSaveable[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Read<TSaveable>(ctx);
            }

            return array;
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static TSaveable[] ReadArray<TSaveable>(byte[] array) where TSaveable : Saveable, new()
        {
            using (var stream = new MemoryStream(array))
            {
                return ReadArray<TSaveable>(stream);
            }
        }

        /// <summary>
        /// Get an enumerator from <see cref="Stream"/> that can be used to iterate over an array of <see cref="Saveable"/>
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<TSaveable> GetEnumerator<TSaveable>(Stream stream) where TSaveable : Saveable, new()
        {
            // Create read context
            var ctx = new ReadContext(stream);

            return GetEnumerator<TSaveable>(ctx);
        }

        /// <summary>
        /// Get an enumerator from <see cref="ReadContext"/> that can be used to iterate over an array of <see cref="Saveable"/>
        /// </summary>
        /// <typeparam name="TSaveable"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static IEnumerable<TSaveable> GetEnumerator<TSaveable>(ReadContext ctx) where TSaveable : Saveable, new()
        {
            int length = ctx.Reader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                yield return Read<TSaveable>(ctx);
            }
        }

        /// <summary>
        /// Read a byte from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static byte ReadByte(ReadContext ctx) => ctx.Reader.ReadByte();

        /// <summary>
        /// Read a char from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static char ReadChar(ReadContext ctx) => ctx.Reader.ReadChar();

        /// <summary>
        /// Read a boolean value from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool ReadBool(ReadContext ctx) => ctx.Reader.ReadBoolean();

        /// <summary>
        /// Read a 16-bit signed integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static short ReadInt16(ReadContext ctx) => ctx.Reader.ReadInt16();

        /// <summary>
        /// Read a 16-bit unsigned integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ushort ReadUInt16(ReadContext ctx) => ctx.Reader.ReadUInt16();

        /// <summary>
        /// Read a 32-bit signed integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static int ReadInt32(ReadContext ctx) => ctx.Reader.ReadInt32();

        /// <summary>
        /// Read a 32-bit unsigned integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static uint ReadUInt32(ReadContext ctx) => ctx.Reader.ReadUInt32();

        /// <summary>
        /// Read a 64-bit signed integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static long ReadInt64(ReadContext ctx) => ctx.Reader.ReadInt64();

        /// <summary>
        /// Read a 64-bit unsigned integer from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ulong ReadUInt64(ReadContext ctx) => ctx.Reader.ReadUInt64();

        /// <summary>
        /// Read a double-precision floating-point value from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static double ReadDouble(ReadContext ctx) => ctx.Reader.ReadDouble();

        /// <summary>
        /// Read a single-precision floating-point value from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static float ReadFloat(ReadContext ctx) => ctx.Reader.ReadSingle();

        /// <summary>
        /// Read a decimal value from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static decimal ReadDecimal(ReadContext ctx) => ctx.Reader.ReadDecimal();

        /// <summary>
        /// Read a length-prefixed byte array from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static byte[] ReadByteArray(ReadContext ctx) => ctx.Reader.ReadBytes(ctx.Reader.ReadInt32());

        /// <summary>
        /// Read a length-prefixed char array from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static char[] ReadCharArray(ReadContext ctx)
        {
            var array = new char[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadChar();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed boolean array from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static bool[] ReadBoolArray(ReadContext ctx)
        {
            var array = new bool[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadBoolean();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed string from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string ReadString(ReadContext ctx) => Encoding.UTF8.GetString(ReadByteArray(ctx));

        /// <summary>
        /// Read a length-prefixed array of length-prefixed strings from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string[] ReadStringArray(ReadContext ctx)
        {
            var array = new string[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadString(ctx);
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 16-bit signed integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static short[] ReadInt16Array(ReadContext ctx)
        {
            var array = new short[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadInt16();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 16-bit unsigned integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ushort[] ReadUInt16Array(ReadContext ctx)
        {
            var array = new ushort[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadUInt16();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 32-bit signed integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static int[] ReadInt32Array(ReadContext ctx)
        {
            var array = new int[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadInt32();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 32-bit unsigned integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static uint[] ReadUInt32Array(ReadContext ctx)
        {
            var array = new uint[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadUInt32();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 64-bit signed integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static long[] ReadInt64Array(ReadContext ctx)
        {
            var array = new long[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadInt64();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of 64-bit unsigned integers from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static ulong[] ReadUInt64Array(ReadContext ctx)
        {
            var array = new ulong[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadUInt64();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of double-precision floating-point values from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static double[] ReadDoubleArray(ReadContext ctx)
        {
            var array = new double[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadDouble();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of single-precision floating-point values from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static float[] ReadFloatArray(ReadContext ctx)
        {
            var array = new float[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadSingle();
            }

            return array;
        }

        /// <summary>
        /// Read a length-prefixed array of decimal values from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static decimal[] ReadDecimalArray(ReadContext ctx)
        {
            var array = new decimal[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ctx.Reader.ReadDecimal();
            }

            return array;
        }

        /// <summary>
        /// Read a <see cref="Dictionary{TKey, TValue}"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(ReadContext ctx)
        {
            var dict = new Dictionary<TKey, TValue>();

            var length = ReadInt32(ctx);

            for (int i = 0; i < length; i++)
            {
                dict.Add(ReadValue<TKey>(ctx), ReadValue<TValue>(ctx));
            }

            return dict;
        }

        /// <summary>
        /// Read a <see cref="Tuple{T1, T2}"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Tuple<T1, T2> ReadTuple<T1, T2>(ReadContext ctx) => new Tuple<T1, T2>(ReadValue<T1>(ctx), ReadValue<T2>(ctx));

        /// <summary>
        /// Read a <see cref="Tuple{T1, T2, T3}"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Tuple<T1, T2, T3> ReadTuple<T1, T2, T3>(ReadContext ctx) => new Tuple<T1, T2, T3>(ReadValue<T1>(ctx), ReadValue<T2>(ctx), ReadValue<T3>(ctx));

        /// <summary>
        /// Read a <see cref="Tuple{T1, T2, T3}"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Tuple<T1, T2, T3, T4> ReadTuple<T1, T2, T3, T4>(ReadContext ctx) => new Tuple<T1, T2, T3, T4>(ReadValue<T1>(ctx), ReadValue<T2>(ctx), ReadValue<T3>(ctx), ReadValue<T4>(ctx));

        /// <summary>
        /// Read a value from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static TValue ReadValue<TValue>(ReadContext ctx) => (TValue)ReadValue(ctx, typeof(TValue));

        /// <summary>
        /// Read a value from <see cref="ReadContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static object ReadValue(ReadContext ctx, Type type)
        {
            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                {
                    throw new NotSupportedException("Array must not have more than 1 dimensions.");
                }

                // Simple array type
                var elementType = type.GetElementType();

                // Arrays of primitive data types
                switch (Type.GetTypeCode(elementType))
                {
                    case TypeCode.Byte:
                        return ReadByteArray(ctx);
                    case TypeCode.Char:
                        return ReadCharArray(ctx);
                    case TypeCode.Boolean:
                        return ReadBoolArray(ctx);
                    case TypeCode.String:
                        return ReadStringArray(ctx);
                    case TypeCode.Int16:
                        return ReadInt16Array(ctx);
                    case TypeCode.UInt16:
                        return ReadUInt16Array(ctx);
                    case TypeCode.Int32:
                        return ReadInt32Array(ctx);
                    case TypeCode.UInt32:
                        return ReadUInt32Array(ctx);
                    case TypeCode.Int64:
                        return ReadInt64Array(ctx);
                    case TypeCode.UInt64:
                        return ReadUInt64Array(ctx);
                    case TypeCode.Double:
                        return ReadDoubleArray(ctx);
                    case TypeCode.Single:
                        return ReadFloatArray(ctx);
                    case TypeCode.Decimal:
                        return ReadDecimalArray(ctx);
                }

                // Saveable type
                if (typeof(Saveable).IsAssignableFrom(elementType))
                {
                    var array = Array.CreateInstance(elementType, ReadInt32(ctx));

                    for (int i = 0; i < array.Length; i++)
                    {
                        var saveableObject = Activator.CreateInstance(elementType);
                        ((Saveable)saveableObject).Read(ctx);

                        array.SetValue(saveableObject, i);
                    }

                    return array;
                }
            }
            else
            {
                // Non-array type

                // Primitive data types
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                        return ReadByte(ctx);
                    case TypeCode.Char:
                        return ReadChar(ctx);
                    case TypeCode.Boolean:
                        return ReadBool(ctx);
                    case TypeCode.String:
                        return ReadString(ctx);
                    case TypeCode.Int16:
                        return ReadInt16(ctx);
                    case TypeCode.UInt16:
                        return ReadUInt16(ctx);
                    case TypeCode.Int32:
                        return ReadInt32(ctx);
                    case TypeCode.UInt32:
                        return ReadUInt32(ctx);
                    case TypeCode.Int64:
                        return ReadInt64(ctx);
                    case TypeCode.UInt64:
                        return ReadUInt64(ctx);
                    case TypeCode.Double:
                        return ReadDouble(ctx);
                    case TypeCode.Single:
                        return ReadFloat(ctx);
                    case TypeCode.Decimal:
                        return ReadDecimal(ctx);
                }

                // Saveable type
                if (typeof(Saveable).IsAssignableFrom(type))
                {
                    var saveableObject = Activator.CreateInstance(type);
                    ((Saveable)saveableObject).Read(ctx);

                    return saveableObject;
                }
            }

            throw new NotSupportedException($"Type {type} is not supported.");
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="saveable"></param>
        public static void Write(Stream stream, Saveable saveable)
        {
            // Create write context
            using (var ctx = new WriteContext(stream, true))
            {
                // Write saveable
                Write(ctx, saveable);
            }
        }

        /// <summary>
        /// Write a <see cref="Saveable"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="saveable"></param>
        public static void Write(WriteContext ctx, Saveable saveable)
        {
            saveable.WriteTo(ctx);
        }

        /// <summary>
        /// Write an array of <see cref="Saveable"/> to <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="array"></param>
        public static void Write(Stream stream, Saveable[] array)
        {
            // Create write context
            using (var ctx = new WriteContext(stream, true))
            {
                // Write saveables
                Write(ctx, array);
            }
        }

        /// <summary>
        /// Write an array of <see cref="Saveable"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, Saveable[] array)
        {
            // Write number of saveables
            ctx.Writer.Write(array.Length);

            // Write saveables
            foreach (var saveable in array)
            {
                Write(ctx, saveable);
            }
        }

        /// <summary>
        /// Dump a <see cref="Saveable"/> to byte array
        /// </summary>
        /// <param name="saveable"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Saveable saveable)
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, saveable);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Dump an array of <see cref="Saveable"/> to byte array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Saveable[] array)
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, array);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Write a byte to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, byte value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a char to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, char value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a boolean value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, bool value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 16-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, short value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 16-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, ushort value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 32-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, int value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 32-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, uint value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 64-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, long value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 64-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, ulong value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a double-precision floating-point value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, double value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a single-precision floating-point value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, float value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a decimal value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, decimal value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a length-prefixed byte array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, byte[] array)
        {
            ctx.Writer.Write(array.Length);
            ctx.Writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed char array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, char[] array)
        {
            ctx.Writer.Write(array.Length);
            ctx.Writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed boolean array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, bool[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                Write(ctx, value);
            }
        }

        /// <summary>
        /// Write a length-prefixed string to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void Write(WriteContext ctx, string value) => Write(ctx, Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Write a length-prefixed array of length-prefixed strings to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, string[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                Write(ctx, value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 16-bit signed integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, short[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 16-bit unsigned integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, ushort[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 32-bit signed integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, int[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 32-bit unsigned integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, uint[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 64-bit signed integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, long[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 64-bit unsigned integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, ulong[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of double-precision floating-point values to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, double[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of single-precision floating-point values to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, float[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of decimal values to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void Write(WriteContext ctx, decimal[] array)
        {
            ctx.Writer.Write(array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
        }

        /// <summary>
        /// Write a <see cref="Dictionary{TKey, TValue}"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="dict"></param>
        public static void Write<TKey, TValue>(WriteContext ctx, Dictionary<TKey, TValue> dict)
        {
            Write(ctx, dict.Count);

            foreach (var pair in dict)
            {
                WriteValue(ctx, pair.Key);
                WriteValue(ctx, pair.Value);
            }
        }

        /// <summary>
        /// Write a <see cref="Tuple{T1, T2}"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="tuple"></param>
        public static void Write<T1, T2>(WriteContext ctx, Tuple<T1, T2> tuple)
        {
            WriteValue(ctx, tuple.Item1);
            WriteValue(ctx, tuple.Item2);
        }

        /// <summary>
        /// Write a <see cref="Tuple{T1, T2, T3}"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="tuple"></param>
        public static void Write<T1, T2, T3>(WriteContext ctx, Tuple<T1, T2, T3> tuple)
        {
            WriteValue(ctx, tuple.Item1);
            WriteValue(ctx, tuple.Item2);
            WriteValue(ctx, tuple.Item3);
        }

        /// <summary>
        /// Write a <see cref="Tuple{T1, T2, T3, T4}"/> to <see cref="WriteContext"/>
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="tuple"></param>
        public static void Write<T1, T2, T3, T4>(WriteContext ctx, Tuple<T1, T2, T3, T4> tuple)
        {
            WriteValue(ctx, tuple.Item1);
            WriteValue(ctx, tuple.Item2);
            WriteValue(ctx, tuple.Item3);
            WriteValue(ctx, tuple.Item4);
        }

        /// <summary>
        /// Write a value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException"></exception>
        public static void WriteValue(WriteContext ctx, object value)
        {
            var type = value.GetType();

            if (type.IsArray)
            {
                if (type.GetArrayRank() != 1)
                {
                    throw new NotSupportedException("Array must not have more than 1 dimensions.");
                }

                // Simple array type
                var elementType = type.GetElementType();

                // Arrays of primitive data types
                switch (Type.GetTypeCode(elementType))
                {
                    case TypeCode.Byte:
                        Write(ctx, (byte[])value);
                        return;
                    case TypeCode.Char:
                        Write(ctx, (char[])value);
                        return;
                    case TypeCode.Boolean:
                        Write(ctx, (bool[])value);
                        return;
                    case TypeCode.String:
                        Write(ctx, (string[])value);
                        return;
                    case TypeCode.Int16:
                        Write(ctx, (short[])value);
                        return;
                    case TypeCode.UInt16:
                        Write(ctx, (ushort[])value);
                        return;
                    case TypeCode.Int32:
                        Write(ctx, (int[])value);
                        return;
                    case TypeCode.UInt32:
                        Write(ctx, (uint[])value);
                        return;
                    case TypeCode.Int64:
                        Write(ctx, (long[])value);
                        return;
                    case TypeCode.UInt64:
                        Write(ctx, (ulong[])value);
                        return;
                    case TypeCode.Double:
                        Write(ctx, (double[])value);
                        return;
                    case TypeCode.Single:
                        Write(ctx, (float[])value);
                        return;
                    case TypeCode.Decimal:
                        Write(ctx, (decimal[])value);
                        return;
                }

                // Saveable type
                if (typeof(Saveable).IsAssignableFrom(elementType))
                {
                    Write(ctx, (Saveable[])value);
                    return;
                }
            }
            else
            {
                // Non-array type

                // Primitive data types
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                        Write(ctx, (byte)value);
                        return;
                    case TypeCode.Char:
                        Write(ctx, (char)value);
                        return;
                    case TypeCode.Boolean:
                        Write(ctx, (bool)value);
                        return;
                    case TypeCode.String:
                        Write(ctx, (string)value);
                        return;
                    case TypeCode.Int16:
                        Write(ctx, (short)value);
                        return;
                    case TypeCode.UInt16:
                        Write(ctx, (ushort)value);
                        return;
                    case TypeCode.Int32:
                        Write(ctx, (int)value);
                        return;
                    case TypeCode.UInt32:
                        Write(ctx, (uint)value);
                        return;
                    case TypeCode.Int64:
                        Write(ctx, (long)value);
                        return;
                    case TypeCode.UInt64:
                        Write(ctx, (ulong)value);
                        return;
                    case TypeCode.Double:
                        Write(ctx, (double)value);
                        return;
                    case TypeCode.Single:
                        Write(ctx, (float)value);
                        return;
                    case TypeCode.Decimal:
                        Write(ctx, (decimal)value);
                        return;
                }

                // Saveable type
                if (typeof(Saveable).IsAssignableFrom(type))
                {
                    Write(ctx, (Saveable)value); 
                    return;
                }
            }

            throw new NotSupportedException($"Type {type} is not supported.");
        }
    }
}
