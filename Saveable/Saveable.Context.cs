using System;
using System.IO;

namespace Saveable.NET
{
    public abstract partial class Saveable
    {
        /// <summary>
        /// Context base class
        /// </summary>
        public class Context : IDisposable
        {
            /// <summary>
            /// Gets the base <see cref="System.IO.Stream"/> associated with this <see cref="Context"/>
            /// </summary>
            public Stream Stream { get; }

            protected readonly bool leaveOpen;

            /// <summary>
            /// Initializes a new <see cref="Context"/> that acts as a wrapper for <see cref="System.IO.Stream"/>
            /// </summary>
            /// <param name="stream"></param>
            public Context(Stream stream)
            {
                Stream = stream;
            }

            /// <summary>
            /// Initializes a new <see cref="Context"/> that acts as a wrapper for <see cref="System.IO.Stream"/>
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="leaveOpen"></param>
            public Context(Stream stream, bool leaveOpen) : this(stream)
            {
                this.leaveOpen = leaveOpen;
            }

            public void Dispose()
            {
                if (!leaveOpen)
                {
                    Stream.Dispose();
                }
            }

            /// <summary>
            /// Close a <see cref="System.IO.Stream"/> associated with this <see cref="Context"/>
            /// </summary>
            public void Close() => Stream.Close();
        }
    }
}