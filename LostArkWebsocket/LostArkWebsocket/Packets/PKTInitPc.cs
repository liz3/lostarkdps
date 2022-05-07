using System;

namespace LostArkWebsocket
{
    public class PKTInitPc
    {
        public UInt64 PlayerId;
        public UInt16 ClassId;

        public string PlayerName;

        public PKTInitPc(Byte[] Bytes)
        {
            var bitReader = new BitReader(Bytes);
            bitReader.SkipBits(8 * 122);
            PlayerId = bitReader.ReadUInt64();
            bitReader.SkipBits(8 * 3);
            PlayerName = PKTNewPC.ReadString(bitReader, true);
            int toOffset = 12 * 8;
            bitReader.SkipBits(toOffset);
            ClassId = bitReader.ReadUInt16();
        }
    }
}
