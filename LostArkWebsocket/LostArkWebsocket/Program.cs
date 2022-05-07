using System;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace LostArkWebsocket
{
    internal class Program
    {
        private static bool AdminRelauncher()
        {
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Assembly.GetEntryAssembly().CodeBase,
                    Verb = "runas"
                };
                try { Process.Start(startInfo); }
                catch (Exception ex) {  }
                return false;
            }
            return true;
        }
        static void AttemptFirewallPrompt()
        {
            var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            var ipLocalEndPoint = new IPEndPoint(ipAddress, 12345);
            var t = new TcpListener(ipLocalEndPoint);
            t.Start();
            t.Stop();
        }
        static void Main(string[] args)
        {
       //     Console.WriteLine("hello there");
            if (!AdminRelauncher()) return;
            AttemptFirewallPrompt();
            Oodle.Init();
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string folder = Path.Combine(homePath, ".laws");
            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var ws = new LostArkWebsocket();
            int port = ws.Start();
            File.WriteAllText(Path.Combine(folder, ".lockfile"), port.ToString());
            Sniffer sniffer = new Sniffer();
            sniffer.onMessage = message =>
           {
               var o = new JObject();
               o["type"] = message.Type;
               o["data"] = message.Data;
               ws.SendData(o.ToString());

           };
            AppDomain.CurrentDomain.ProcessExit += (s, e) => File.Delete(Path.Combine(folder, ".lockfile"));
            Console.Read();
           
        }
   
    }
}
