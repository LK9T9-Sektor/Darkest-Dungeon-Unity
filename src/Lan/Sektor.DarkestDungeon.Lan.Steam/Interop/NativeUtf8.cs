namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Encodes .NET strings to NUL-terminated UTF-8 buffers and back, because the
    /// Steamworks flat API exchanges lobby metadata as UTF-8 strings.
    /// </summary>
    internal static class NativeUtf8
    {
        /// <summary>Encodes the string as a pinned NUL-terminated UTF-8 buffer.</summary>
        internal static PinnedBuffer ToNative(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value + "\0");
            return new PinnedBuffer(bytes);
        }

        /// <summary>Reads a NUL-terminated UTF-8 string from the given pointer.</summary>
        internal static string FromNative(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return string.Empty;
            }

            int length = 0;
            while (Marshal.ReadByte(pointer, length) != 0)
            {
                length++;
            }

            byte[] buffer = new byte[length];
            Marshal.Copy(pointer, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>A pinned byte buffer with a disposal boundary.</summary>
        internal sealed class PinnedBuffer : IDisposable
        {
            private readonly byte[] _bytes;
            private GCHandle _handle;

            internal PinnedBuffer(byte[] bytes)
            {
                _bytes = bytes;
                _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            }

            /// <summary>Gets the address of the pinned buffer.</summary>
            internal IntPtr Pointer
            {
                get { return _handle.AddrOfPinnedObject(); }
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                {
                    _handle.Free();
                }
            }
        }
    }
}
