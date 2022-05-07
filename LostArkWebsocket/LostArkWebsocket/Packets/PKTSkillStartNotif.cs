
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostArkWebsocket
{
    public class PKTSkillStartNotif
    {
        public UInt32 SkillId;
        public UInt64 SourceId;

        public PKTSkillStartNotif(Byte[] Bytes)
        {
            var bitReader = new BitReader(Bytes);
            bitReader.SkipBits(28 * 8);
            SourceId = bitReader.ReadUInt64();
            bitReader.SkipBits(14 * 8);
            SkillId = bitReader.ReadUInt32();
        }
    }
}
