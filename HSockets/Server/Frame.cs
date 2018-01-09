using System;
using System.Collections.Generic;
using System.Text;

namespace HSocket.Server
{
    class Frame
    {
        public bool FIN { get; set; }
        public long Length { get; set; }
        public byte[] Mask { get; set; }
        public byte[] Payload { get; set; }
        public OpCode OpCode { get; set; }
        private byte[] _unmaskedData;
        public byte[] UnmaskedData
        {
            get
            {
                if (_unmaskedData == null)
                {
                    var d = new byte[Payload.Length];
                    for (int i = 0; i < Payload.Length; i++)
                        d[i] = (byte)(Payload[i] ^ Mask[i % 4]);
                    _unmaskedData = d;
                }
                return _unmaskedData;
            }
        }

    }
}
