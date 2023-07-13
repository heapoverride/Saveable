using System;
using System.IO;

namespace SaveableDotNet
{
    public abstract partial class Saveable
    {
        /// <summary>
        /// Read context
        /// </summary>
        public class ReadContext : Context, IDisposable
        {
            /// <summary>
            /// Gets the <see cref="BinaryReader"/> associated with this <see cref="ReadContext"/>
            /// </summary>
            public BinaryReader Reader { get; }

            /// <summary>
            /// Initializes a new <see cref="ReadContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryReader"/>
            /// </summary>
            /// <param name="stream"></param>
            public ReadContext(Stream stream) : this(stream, false) { }

            /// <summary>
            /// Initializes a new <see cref="ReadContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryReader"/>
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="leaveOpen"></param>
            public ReadContext(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
            {
                Reader = new BinaryReader(stream);
            }

            /// <summary>
            /// Initializes a new <see cref="ReadContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryReader"/>
            /// </summary>
            /// <param name="reader"></param>
            public ReadContext(BinaryReader reader) : this(reader, false) { }

            /// <summary>
            /// Initializes a new <see cref="ReadContext"/> that acts as a wrapper for <see cref="Stream"/> and <see cref="BinaryReader"/>
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="leaveOpen"></param>
            public ReadContext(BinaryReader reader, bool leaveOpen) : this(reader.BaseStream, leaveOpen) { }
        }
    }
}