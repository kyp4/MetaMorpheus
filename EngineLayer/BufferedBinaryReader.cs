using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EngineLayer
{
    /// <summary>
    /// See https://www.jacksondunstan.com/articles/3568
    /// </summary>
    public class BufferedBinaryReader : IDisposable
    {
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int bufferSize;
        private int bufferOffset;
        private int numBufferedBytes;

        public BufferedBinaryReader(Stream stream, int bufferSize)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];
            bufferOffset = bufferSize;
        }

        public int NumBytesAvailable { get { return Math.Max(0, numBufferedBytes - bufferOffset); } }

        public bool FillBuffer()
        {
            var numBytesUnread = bufferSize - bufferOffset;
            var numBytesToRead = bufferSize - numBytesUnread;
            bufferOffset = 0;
            numBufferedBytes = numBytesUnread;
            if (numBytesUnread > 0)
            {
                Buffer.BlockCopy(buffer, numBytesToRead, buffer, 0, numBytesUnread);
            }
            while (numBytesToRead > 0)
            {
                var numBytesRead = stream.Read(buffer, numBytesUnread, numBytesToRead);
                if (numBytesRead == 0)
                {
                    return false;
                }
                numBufferedBytes += numBytesRead;
                numBytesToRead -= numBytesRead;
                numBytesUnread += numBytesRead;
            }
            return true;
        }

        public ushort ReadUInt16()
        {
            var val = (ushort)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
            bufferOffset += 2;
            return val;
        }

        public int ReadInt32()
        {
            int val = ((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8 | (int)buffer[bufferOffset + 2] << 16 | (int)buffer[bufferOffset + 3] << 24);
            bufferOffset += 4;
            return val;
        }

        public void Dispose()
        {
            stream.Close();
        }
    }
}
