using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saveable.NET
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Read<T>(Stream stream) where T : Saveable
        {
            // Create read context
            using (var ctx = new ReadContext(stream, true))
            {
                // Read saveable
                return Read<T>(ctx);
            }
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static T Read<T>(ReadContext ctx) where T : Saveable
        {
            var saveable = Activator.CreateInstance<T>();

            // Update position
            if (ctx.Stream.CanSeek)
                saveable.position = ctx.Stream.Position;

            // Read saveable
            saveable.Read(ctx);

            // Update length
            if (ctx.Stream.CanSeek)
                saveable.length = ctx.Stream.Position - saveable.position;

            return saveable;
        }

        /// <summary>
        /// Read a <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Read<T>(byte[] array) where T : Saveable
        {
            using (var stream = new MemoryStream(array))
            {
                return Read<T>(stream);
            }
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ReadArray<T>(Stream stream) where T : Saveable
        {
            // Create read context
            using (var ctx = new ReadContext(stream, true))
            {
                // Read saveables
                return ReadArray<T>(ctx);
            }
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static T[] ReadArray<T>(ReadContext ctx) where T : Saveable
        {
            var array = new T[ctx.Reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Read<T>(ctx);
            }

            return array;
        }

        /// <summary>
        /// Read an array of <see cref="Saveable"/> from byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] ReadArray<T>(byte[] array) where T : Saveable
        {
            using (var stream = new MemoryStream(array))
            {
                return ReadArray<T>(stream);
            }
        }

        /// <summary>
        /// Get an enumerator from <see cref="Stream"/> that can be used to iterate over an array of <see cref="Saveable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumerator<T>(Stream stream) where T : Saveable
        {
            // Create read context
            var ctx = new ReadContext(stream);

            return GetEnumerator<T>(ctx);
        }

        /// <summary>
        /// Get an enumerator from <see cref="ReadContext"/> that can be used to iterate over an array of <see cref="Saveable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnumerator<T>(ReadContext ctx) where T : Saveable
        {
            int length = ctx.Reader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                yield return Read<T>(ctx);
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
        public static bool ReadBoolean(ReadContext ctx) => ctx.Reader.ReadBoolean();

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
        public static bool[] ReadBooleanArray(ReadContext ctx)
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
        /// Read a value from <see cref="ReadContext"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static T ReadValue<T>(ReadContext ctx) => (T)ReadValue(ctx, typeof(T));

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
                        return ReadBooleanArray(ctx);
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
                        return ReadBoolean(ctx);
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
            // Update position
            if (ctx.Stream.CanSeek)
                saveable.position = ctx.Stream.Position;

            // Write saveable
            saveable.Write(ctx);

            // Update length
            if (ctx.Stream.CanSeek)
                saveable.length = ctx.Stream.Position - saveable.position;
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
            ctx.Writer.Write((int)array.Length);

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
        public static void WriteByte(WriteContext ctx, byte value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a char to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteChar(WriteContext ctx, char value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a boolean value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteBoolean(WriteContext ctx, bool value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 16-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteInt16(WriteContext ctx, short value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 16-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteUInt16(WriteContext ctx, ushort value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 32-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteInt32(WriteContext ctx, int value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 32-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteUInt32(WriteContext ctx, uint value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 64-bit signed integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteInt64(WriteContext ctx, long value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a 64-bit unsigned integer to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteUInt64(WriteContext ctx, ulong value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a double-precision floating-point value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteDouble(WriteContext ctx, double value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a single-precision floating-point value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteFloat(WriteContext ctx, float value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a decimal value to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteDecimal(WriteContext ctx, decimal value) => ctx.Writer.Write(value);

        /// <summary>
        /// Write a length-prefixed byte array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void WriteByteArray(WriteContext ctx, byte[] array)
        {
            ctx.Writer.Write((int)array.Length);
            ctx.Writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed char array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void WriteCharArray(WriteContext ctx, char[] array)
        {
            ctx.Writer.Write((int)array.Length);
            ctx.Writer.Write(array);
        }

        /// <summary>
        /// Write a length-prefixed boolean array to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void WriteBooleanArray(WriteContext ctx, bool[] array)
        {
            ctx.Writer.Write((int)array.Length);

            foreach (var value in array)
            {
                WriteBoolean(ctx, value);
            }
        }

        /// <summary>
        /// Write a length-prefixed string to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        public static void WriteString(Saveable.WriteContext ctx, string value) => WriteByteArray(ctx, Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Write a length-prefixed array of length-prefixed strings to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void WriteStringArray(WriteContext ctx, string[] array)
        {
            ctx.Writer.Write((int)array.Length);

            foreach (var value in array)
            {
                WriteString(ctx, value);
            }
        }

        /// <summary>
        /// Write a length-prefixed array of 16-bit signed integers to <see cref="WriteContext"/>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="array"></param>
        public static void WriteInt16Array(WriteContext ctx, short[] array)
        {
            ctx.Writer.Write((int)array.Length);

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
        public static void WriteUInt16Array(WriteContext ctx, ushort[] array)
        {
            ctx.Writer.Write((int)array.Length);

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
        public static void WriteInt32Array(WriteContext ctx, int[] array)
        {
            ctx.Writer.Write((int)array.Length);

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
        public static void WriteUInt32Array(WriteContext ctx, uint[] array)
        {
            ctx.Writer.Write((uint)array.Length);

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
        public static void WriteInt64Array(WriteContext ctx, long[] array)
        {
            ctx.Writer.Write((int)array.Length);

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
        public static void WriteUInt64Array(WriteContext ctx, ulong[] array)
        {
            ctx.Writer.Write((uint)array.Length);

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
        public static void WriteDoubleArray(WriteContext ctx, double[] array)
        {
            ctx.Writer.Write((uint)array.Length);

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
        public static void WriteFloatArray(WriteContext ctx, float[] array)
        {
            ctx.Writer.Write((uint)array.Length);

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
        public static void WriteDecimalArray(WriteContext ctx, decimal[] array)
        {
            ctx.Writer.Write((uint)array.Length);

            foreach (var value in array)
            {
                ctx.Writer.Write(value);
            }
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
                        WriteByteArray(ctx, (byte[])value);
                        return;
                    case TypeCode.Char:
                        WriteCharArray(ctx, (char[])value);
                        return;
                    case TypeCode.Boolean:
                        WriteBooleanArray(ctx, (bool[])value);
                        return;
                    case TypeCode.String:
                        WriteStringArray(ctx, (string[])value);
                        return;
                    case TypeCode.Int16:
                        WriteInt16Array(ctx, (short[])value);
                        return;
                    case TypeCode.UInt16:
                        WriteUInt16Array(ctx, (ushort[])value);
                        return;
                    case TypeCode.Int32:
                        WriteInt32Array(ctx, (int[])value);
                        return;
                    case TypeCode.UInt32:
                        WriteUInt32Array(ctx, (uint[])value);
                        return;
                    case TypeCode.Int64:
                        WriteInt64Array(ctx, (long[])value);
                        return;
                    case TypeCode.UInt64:
                        WriteUInt64Array(ctx, (ulong[])value);
                        return;
                    case TypeCode.Double:
                        WriteDoubleArray(ctx, (double[])value);
                        return;
                    case TypeCode.Single:
                        WriteFloatArray(ctx, (float[])value);
                        return;
                    case TypeCode.Decimal:
                        WriteDecimalArray(ctx, (decimal[])value);
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
                        WriteByte(ctx, (byte)value);
                        return;
                    case TypeCode.Char:
                        WriteChar(ctx, (char)value);
                        return;
                    case TypeCode.Boolean:
                        WriteBoolean(ctx, (bool)value);
                        return;
                    case TypeCode.String:
                        WriteString(ctx, (string)value);
                        return;
                    case TypeCode.Int16:
                        WriteInt16(ctx, (short)value);
                        return;
                    case TypeCode.UInt16:
                        WriteUInt16(ctx, (ushort)value);
                        return;
                    case TypeCode.Int32:
                        WriteInt32(ctx, (int)value);
                        return;
                    case TypeCode.UInt32:
                        WriteUInt32(ctx, (uint)value);
                        return;
                    case TypeCode.Int64:
                        WriteInt64(ctx, (long)value);
                        return;
                    case TypeCode.UInt64:
                        WriteUInt64(ctx, (ulong)value);
                        return;
                    case TypeCode.Double:
                        WriteDouble(ctx, (double)value);
                        return;
                    case TypeCode.Single:
                        WriteFloat(ctx, (float)value);
                        return;
                    case TypeCode.Decimal:
                        WriteDecimal(ctx, (decimal)value);
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
