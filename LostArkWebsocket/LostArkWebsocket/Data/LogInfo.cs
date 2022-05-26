using System;
namespace LostArkLogger
{
    public class LogInfo
    {
        public DateTime Time { get; set; }
        public UInt64 SourceEntity { get; set; }
        public UInt64 DestinationEntity { get; set; }
        public UInt32 SkillId { get; set; }
        public UInt32 SkillSubId { get; set; }
        public String SkillName { get; set; }
        public UInt64 Damage { get; set; }
        public UInt64 Heal { get; set; }
        public UInt64 Shield { get; set; }
        public UInt64 Stagger { get; set; }
        public Boolean Crit { get; set; }
        public Boolean BackAttack { get; set; }
        public Boolean FrontAttack { get; set; }
        public Boolean Counter { get; set; }
        public override string ToString()
        {
            return Time.ToString("yy:MM:dd:HH:mm:ss.f") + "," +
                   SkillName + "," +
                   Damage + "," +
                   (Crit ? "1" : "0") + "," +
                   (BackAttack ? "1" : "0") + "," +
                   (FrontAttack ? "1" : "0") + "," +
                   (Counter ? "1" : "0");
        }
    }
}
