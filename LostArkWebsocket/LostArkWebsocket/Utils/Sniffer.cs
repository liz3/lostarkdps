using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
namespace LostArkWebsocket
{
    internal class Sniffer : IDisposable
    {
        Socket socket;
        Byte[] packetBuffer = new Byte[0x10000];
        public Action newZone;
        public Action<Message> onMessage;
        public Sniffer()
        {
            var tcp = new Machina.TCPNetworkMonitor();
            tcp.Config.WindowName = "LOST ARK (64-bit, DX11) v.2.2.3.1";
            tcp.Config.MonitorType = Machina.Infrastructure.NetworkMonitorType.RawSocket;
            tcp.DataReceivedEventHandler += (Machina.Infrastructure.TCPConnection connection, byte[] data) => Device_OnPacketArrival(connection, data);
            tcp.Start();

        }
        public Dictionary<UInt64, UInt64> ProjectileOwner = new Dictionary<UInt64, UInt64>();
        public Dictionary<UInt64, String> IdToName = new Dictionary<UInt64, String>();
        public Dictionary<UInt64, bool> NPcMap = new Dictionary<UInt64, bool>();
        public Dictionary<String, String> NameToClass = new Dictionary<String, String>();
        Byte[] fragmentedPacket = new Byte[0];
        void ProcessPacket(List<Byte> data)
        {
       //     if (!Directory.Exists("logs2")) Directory.CreateDirectory("logs2");
            var packets = data.ToArray();
            var packetWithTimestamp = BitConverter.GetBytes(DateTime.UtcNow.ToBinary()).ToArray().Concat(data);
        
            while (packets.Length > 0)
            {
                if (fragmentedPacket.Length > 0)
                {
                    packets = fragmentedPacket.Concat(packets).ToArray();
                    fragmentedPacket = new Byte[0];
                }
                if (6 > packets.Length)
                {
                    fragmentedPacket = packets.ToArray();
                    return;
                }

                var opcode = (OpCodes)BitConverter.ToUInt16(packets, 2);
               /* if (opcode != OpCodes.PKTNewTargetNotify && opcode != OpCodes.PKTMoveNotify && opcode != OpCodes.PKTMoveStopNotify && opcode != OpCodes.PKTMoveNotifyList)
                    Console.WriteLine(opcode.ToString());
             */   var packetSize = BitConverter.ToUInt16(packets.ToArray(), 0);
                if (packets[5] != 1 || 6 > packets.Length || packetSize < 7)
                {
                    // not sure when this happens
                    fragmentedPacket = new Byte[0];
                    return;
                }
                if (packetSize > packets.Length)
                {
                    fragmentedPacket = packets.ToArray();
                    return;
                }
                var payload = packets.Skip(6).Take(packetSize - 6).ToArray();
                Xor.Cipher(payload, (UInt16)opcode);
                var message = new Message();
                message.Type = opcode.ToString();
                if (packets[4] == 3) payload = Oodle.Decompress(payload).Skip(16).ToArray();
                if (opcode == OpCodes.PKTNewProjectile)
                    ProjectileOwner[BitConverter.ToUInt64(payload, 4)] = BitConverter.ToUInt64(payload, 4 + 8);
                else if (opcode == OpCodes.PKTNewNpc)
                {
                    var id = BitConverter.ToUInt64(payload, 7);
                    var npcName = Npc.GetNpcName(BitConverter.ToUInt32(payload, 15));
                    IdToName[id] = npcName;
                    NPcMap[id] = true;

                } else if (opcode == OpCodes.PKTMusicUpdateNotify)
                {
                    var id = BitConverter.ToUInt32(payload, 6);
                    message.Data = new JObject();
                    message.Data["id"] = id;
                    onMessage(message);
                    
                }
                else if (opcode == OpCodes.PKTNewPC)
                {
                    var pc = new PKTNewPC(payload);
                    var pcClass = Npc.GetPcClass(pc.ClassId);
                    if (!NameToClass.ContainsKey(pc.Name)) NameToClass[pc.Name] = pcClass + (NameToClass.ContainsValue(pcClass) ? (" - " + Guid.NewGuid().ToString().Substring(0, 4)) : "");
                    IdToName[pc.PlayerId] = pc.Name + " (" + pcClass + ")";
                    var json = new JObject();
                    json["formatted"] = IdToName[pc.PlayerId];
                    json["class_id"] = pc.ClassId.ToString();
                    json["class_name"] = pcClass;
                    json["id"] = pc.PlayerId.ToString();
                    json["player_name"] = pc.Name;
                    message.Data = json;
                    onMessage(message);

                }
                else if (opcode == OpCodes.PKTInitPC)
                {
                    var pc = new PKTInitPc(payload);
                    var pcClass = Npc.GetPcClass(pc.ClassId);
                    var json = new JObject();
                    json["formatted"] = IdToName[pc.PlayerId];
                    json["class_id"] = pc.ClassId.ToString();
                    json["class_name"] = pcClass;
                    json["id"] = pc.PlayerId.ToString();
                    json["player_name"] = pc.PlayerName;
                    message.Data = json;
                    onMessage(message);

                }
                else if (opcode == OpCodes.PKTInitEnv)
                {
                    var pc = new PKTInitEnv(payload);
                    IdToName[pc.PlayerId] = "You";
                    newZoneMsg();
                    message.Type = "ThisPlayer";
                    var json = new JObject();
                    json["id"] = pc.PlayerId;
                    message.Data = json;
                    onMessage(message);


                } else if (opcode == OpCodes.PKTRaidStatusUpdateNotify)
                {
                    if(payload.Length == 38)
                    {
                        message.Data = new JObject();
                        onMessage(message);
                        
                    }
                }
                /*if ((OpCodes)BitConverter.ToUInt16(converted.ToArray(), 2) == OpCodes.PKTRemoveObject)
                {
                    var projectile = new PKTRemoveObject { Bytes = converted };
                    ProjectileOwner.Remove(projectile.ProjectileId, projectile.OwnerId);
                }*/
                else if (opcode == OpCodes.PKTSkillDamageNotify)
                {
                    var damage = new PKTSkillDamageNotify(payload);
                    {
                        foreach (var dmgEvent in damage.Events)
                        {
                            var skillName = Skill.GetSkillName(damage.SkillId, damage.SkillIdWithState);
                            var ownerId = ProjectileOwner.ContainsKey(damage.PlayerId) ? ProjectileOwner[damage.PlayerId] : damage.PlayerId;
                            if (NPcMap.ContainsKey(ownerId))
                                continue;
                            var nameKnown = IdToName.ContainsKey(ownerId);
                            var sourceName = nameKnown ? IdToName[ownerId] : ownerId.ToString("X");
                            var destinationName = IdToName.ContainsKey(dmgEvent.TargetId) ? IdToName[dmgEvent.TargetId] : dmgEvent.TargetId.ToString("X");
                            if (sourceName == "You" && Skill.GetClassFromSkill(damage.SkillId) != "UnknownClass")
                            {

                                var myClass = Skill.GetClassFromSkill(damage.SkillId);
                                if (myClass != "UnknownClass") sourceName = IdToName[ownerId] = "You (" + myClass + ")";
                            }
                            //var log = new LogInfo { Time = DateTime.Now, Source = sourceName, PC = sourceName.Contains("("), Destination = destinationName, SkillName = skillName, Crit = (dmgEvent.FlagsMaybe & 0x81) > 0, BackAttack = (dmgEvent.FlagsMaybe & 0x10) > 0, FrontAttack = (dmgEvent.FlagsMaybe & 0x20) > 0 };
                            var log = new LogInfo { Time = DateTime.Now, Source = sourceName, PC = true, Destination = destinationName, SkillName = skillName, Damage = dmgEvent.Damage, Crit = (dmgEvent.FlagsMaybe & 0x81) > 0, BackAttack = (dmgEvent.FlagsMaybe & 0x10) > 0, FrontAttack = (dmgEvent.FlagsMaybe & 0x20) > 0 };
                            var json = new JObject();
                            json["source_id"] = ownerId.ToString();
                            json["source_name"] = sourceName;
                            json["skill_id"] = damage.SkillId.ToString();
                            json["skill_id_with_state"] = damage.SkillIdWithState.ToString();
                            json["damage"] = log.Damage;
                            json["crit"] = log.Crit;
                            json["name_known"] = nameKnown;
                            if (log.BackAttack)
                            {
                                json["type"] = "back_attack";
                            } else if(log.FrontAttack)
                            {
                                json["type"] = "front_attack";
                            } else
                            {
                                json["type"] = "normal";
                            }
                            json["skill_name"] = skillName;
                            json["target_id"] = dmgEvent.TargetId.ToString();
                            json["target_name"] = destinationName;

                            message.Data = json;
                            onMessage(message);
                        }
                    }
                }
                else if (opcode == OpCodes.PKTSkillDamageAbnormalMoveNotify)
                {
                    var damage = new PKTSkillDamageAbnormalMoveNotify(payload);
                    //for (var i = 0; i < payload.Length - 4; i++)
                    //    Console.WriteLine(i + " : " + BitConverter.ToUInt32(payload, i) + " : " + BitConverter.ToUInt32(payload, i).ToString("X"));
                    // normal mobs when skills make them move. not needed for boss tracking, since guardians don't get moved by abilities. this will show more damage taken by players
                } else if(opcode == OpCodes.PKTDeathNotify)
                {
                    var entry = new PKTDeathNotif(payload);
                    var destinationName = IdToName.ContainsKey(entry.TargetId) ? IdToName[entry.TargetId] : entry.TargetId.ToString("X");
                    var sourceName = IdToName.ContainsKey(entry.KillerId) ? IdToName[entry.KillerId] : entry.KillerId.ToString("X");
                    var json = new JObject();
                    json["target_id"] = entry.TargetId.ToString();
                    json["source_id"] = entry.KillerId.ToString();
                    json["target_name"] = destinationName;
                    json["source_name"] = sourceName;
                    message.Data = json;
                    onMessage?.Invoke(message);

                }
                else if (opcode == OpCodes.PKTSkillStartNotify)
                {
                    if(payload.Length == 55)
                    {
                        var entry = new PKTSkillStartNotif(payload);
                        var skillName = Skill.GetSkillName(entry.SkillId, 0);
                        var ownerId = ProjectileOwner.ContainsKey(entry.SourceId) ? ProjectileOwner[entry.SourceId] : entry.SourceId;
                        var nameKnown = IdToName.ContainsKey(ownerId);
                        var sourceName = nameKnown ? IdToName[ownerId] : ownerId.ToString("X");
                        var json = new JObject();
                        if (sourceName == "You" && Skill.GetClassFromSkill(entry.SkillId) != "UnknownClass")
                        {

                            var myClass = Skill.GetClassFromSkill(entry.SkillId);
                            if (myClass != "UnknownClass") sourceName = IdToName[ownerId] = "You (" + myClass + ")";
                        }
                        json["skill_id"] = entry.SkillId.ToString();
                        json["source_id"] = ownerId.ToString();
                        json["skill_name"] = skillName;
                        json["source_name"] = sourceName;
                        json["name_known"] = nameKnown;
                        message.Data = json;
                        onMessage?.Invoke(message);
                    }
                } else if (opcode == OpCodes.PKTRaidBegin || opcode == OpCodes.PKTRaidResult || opcode == OpCodes.PKTEnterDungeonInfo
                    || opcode == OpCodes.PKTChaosDungeonRewardNotify || opcode == OpCodes.PKTInitChaosDungeonRewardCount || opcode == OpCodes.PKTReverseRuinRewardNotify)
                {
          
                    message.Data = new JObject();
                    onMessage?.Invoke(message);
                } 
                if (packets.Length < packetSize) throw new Exception("bad packet maybe");
                packets = packets.Skip(packetSize).ToArray();
            }
        }
        public static IPAddress GetLocalIPAddress()
        {
            try
            {
                var tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var ipEndpoint = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 6040).Serialize();
                var optionIn = new Byte[ipEndpoint.Size];
                for (int i = 0; i < ipEndpoint.Size; i++) optionIn[i] = ipEndpoint[i];
                var optionOut = new Byte[optionIn.Length];
                tempSocket.IOControl(IOControlCode.RoutingInterfaceQuery, optionIn, optionOut);
                tempSocket.Close();
                return new IPAddress(optionOut.Skip(4).Take(4).ToArray());
            }
            catch
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                //var activeDevice = NetworkInterface.GetAllNetworkInterfaces().First(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                //var activeDeviceIpProp = activeDevice.GetIPProperties().UnicastAddresses.Select(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                return ipAddress;
            }
        }
        TcpReconstruction tcpReconstruction;
        BlockingCollection<Byte[]> packetQueue = new BlockingCollection<Byte[]>();
        Byte[] fragmentedRead = new Byte[0];
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                var bytesRead = socket?.EndReceive(ar);
                if (bytesRead > 0)
                {

                    var packets = new Byte[(int)bytesRead];
                    Array.Copy(packetBuffer, packets, (int)bytesRead);
                    packetQueue.Add(packets);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            packetBuffer = new Byte[packetBuffer.Length];
            socket?.BeginReceive(packetBuffer, 0, packetBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), ar);
        }
        UInt32 currentIpAddr = 0xdeadbeef;
        void AppendLog(String s)
        {
        }
        void newZoneMsg()
        {
            JObject o = new JObject();
            var msg = new Message();
            msg.Type = "NewZone";
            msg.Data = o;
            onMessage(msg);
        }
        void Device_OnPacketArrival(Machina.Infrastructure.TCPConnection connection, byte[] bytes)
        {
            if (connection.RemotePort != 6040) return;
            var srcAddr = connection.RemoteIP;
            if (srcAddr != currentIpAddr)
            {
                if (currentIpAddr == 0xdeadbeef || (bytes.Length > 4 && (OpCodes)BitConverter.ToUInt16(bytes, 2) == OpCodes.PKTAuthTokenResult && bytes[0] == 0x1e))
                {
                    newZoneMsg();
                    currentIpAddr = srcAddr;
                }
                else return;
            }
            ProcessPacket(bytes.ToList());
        }

        public void Dispose()
        {
            socket.Close();
            socket = null;
        }
    }
}
