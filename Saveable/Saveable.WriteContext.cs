using System;
using System.IO;

namespace Saveable.NET
{
    public abstract partial class Saveable
    {
        /// <summary>
        /// Write context
        /// </summary>
        public class WriteContext : Context, IDisposable
        {
            /// <summary>
            /// Gets the <see cref="BinaryWriter"/> associated with this <see cref="WriteContext"/>
            /// </summary>
            public BinaryWriter Writer { get; }

            /// <summary>
            /// Initializes a new <see cref="WriteContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryWriter"/>
            /// </summary>
            /// <param name="stream"></param>
            public WriteContext(Stream stream) : this(stream, false) { }

            /// <summary>
            /// Initializes a new <see cref="WriteContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryWriter"/>
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="leaveOpen"></param>
            public WriteContext(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
            {
                Writer = new BinaryWriter(stream);
            }

            /// <summary>
            /// Initializes a new <see cref="WriteContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryWriter"/>
            /// </summary>
            /// <param name="writer"></param>
            public WriteContext(BinaryWriter writer) : this(writer, false) { }

            /// <summary>
            /// Initializes a new <see cref="WriteContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryWriter"/>
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="leaveOpen"></param>
            public WriteContext(BinaryWriter writer, bool leaveOpen) : this(writer.BaseStream, leaveOpen) { }
        }
    }
}