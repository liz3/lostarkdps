using System;

namespace LostArkWebsocket
{
    public class PKTDeathNotif
    {
        public UInt64 TargetId;
        public UInt64 KillerId;
        public PKTDeathNotif(Byte[] Bytes)
        {
            var bitReader = new BitReader(Bytes);
            TargetId = bitReader.ReadUInt64();
            bitReader.SkipBits(17 * 8);
            KillerId = bitReader.ReadUInt64();
        }
    }

}
