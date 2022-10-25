using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using Tune;
using System.Text.Json;
using System.Net.Http.Json;

namespace TuneHijacker // Note: actual namespace depends on the project name.
{
    internal class Program
    {


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            var bind = args[0];

            //var bind = "94EA32ADF778@Bedroom";

            /*//TuneBlade.client.PutAsync("StreamingMode", new StringContent("{\"StreamingMode\":\"RealTime\",\"BufferSize\":-1}"));
            var a = TuneBlade.GetStreaming()!;
            a.StreamingMode = "RealTime";
            Console.WriteLine(JsonSerializer.Serialize(a));
            //_ = TuneBlade.client.PutAsJsonAsync("StreamingMode", a).Result;
            TuneBlade.client.PutAsync("StreamingMode", new StringContent(JsonSerializer.Serialize(a)));*/


            //Console.ReadKey();

            while (true)
            {
                try
                {
                    KeepTuneRunning();
                    var stats = GetMusicStats();
                    var device = TuneBlade.GetDevice(bind);
                    if (stats == MusicStats.RUNNING)
                    {
                        var streaming = TuneBlade.GetStreaming();
                        if (streaming.StreamingMode != "RealTime")
                        {
                            streaming.StreamingMode = "RealTime";
                            TuneBlade.SetStreaming(streaming);
                        }

                        if (device.Status != "Connected")
                        {
                            device.connect();
                        }

                    }
                    else
                    {
                        if (device.Status != "Disonnect")
                        {
                            device.disconnect();
                        }
                    }

                    Console.WriteLine("current state: " + stats);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(2000);
            }

        }

        static void KeepTuneRunning()
        {
            if (!IsProccessRunning("TuneBlade") || !TuneBlade.IsWebActive())
            {
                var p = new Process(); //实例一个Process类，启动一个独立进程
                p.StartInfo.FileName = "C:\\Program Files (x86)\\TuneBlade\\TuneBlade\\TuneBlade.exe"; //设定程序名
                p.StartInfo.Arguments = "Silent Isolate StartHttpControl Port=54412";
                p.StartInfo.CreateNoWindow = true; // 设置不显示窗口
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                Console.WriteLine("Restart TuneBlade on 54412.");
            }


        }




        static bool IsProccessRunning(string name)
        {
            if (Process.GetProcessesByName(name).Length == 0)
            {
                return false;
            }
            return true;
        }

        static MusicStats GetMusicStats()
        {
            if (!IsProccessRunning("WsaService"))
            {
                return MusicStats.STOPPED;
            }
            {
                var connection = RunAdbCmd("connect localhost:58526");

                if (!connection.Contains("already"))
                {
                    Console.WriteLine(connection);
                }
                
            }


            MusicStats stats;
            var result = RunAdbCmd("-s localhost:58526 shell dumpsys activity services com.apple.android.music");
            if (result.Contains("(nothing)"))
            {
                stats = MusicStats.STOPPED;
            }
            else
            {
                stats = MusicStats.RUNNING;
            }
            return stats;
        }

        static string RunAdbCmd(string arg)
        {
            var p = new Process(); //实例一个Process类，启动一个独立进程
            p.StartInfo.FileName = "adb.exe"; //设定程序名
            p.StartInfo.Arguments = arg;
            p.StartInfo.UseShellExecute = false; //关闭Shell的使用
            p.StartInfo.RedirectStandardInput = true; //重定向标准输入
            p.StartInfo.RedirectStandardOutput = true; //重定向标准输出
            p.StartInfo.RedirectStandardError = true; //重定向错误输出
            p.StartInfo.CreateNoWindow = true; // 设置不显示窗口
            p.StartInfo.ErrorDialog = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }

        enum MusicStats
        {
            STOPPED,
            RUNNING,

        }
    }

}