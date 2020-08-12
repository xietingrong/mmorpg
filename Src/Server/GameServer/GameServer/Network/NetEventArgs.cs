using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Network
{
    /// <summary>
    /// EventArgs class holding a Byte[].
    /// </summary>
    public class DataEventArgs : EventArgs
    {
        public IPEndPoint RemoteEndPoint { get; set; }
        public Byte[] Data { get; set; }
        public Int32 Offset { get; set; }
        public Int32 Length { get; set; }
    }
}
