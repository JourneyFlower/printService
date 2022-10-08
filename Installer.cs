using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;

namespace printServer
{
    public static class Installer
    {
        internal static string Name
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        internal static void cmd(string file, string arguments, bool pause = true)
        {
            using (var p = Process.Start(new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }))
            {
                p.OutputDataReceived += (s, _e) => Console.WriteLine(_e.Data);
                p.ErrorDataReceived += (s, _e) => Console.WriteLine(_e.Data);
                p.EnableRaisingEvents = true;
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                if (pause)
                {
                    Console.WriteLine("press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        internal static void install(string Description)
        {

            Console.WriteLine("            SETUP             ");
            Console.WriteLine("==============================");
            Console.WriteLine();
            Console.WriteLine("\t0.quit");
            Console.WriteLine("\t1.install and start service");
            Console.WriteLine("\t2.install");
            Console.WriteLine("\t3.start service");
            Console.WriteLine("\t4.stop service");
            Console.WriteLine("\t5.uninstall");
            Console.WriteLine("\t6.stop and uninstall service");
            Console.WriteLine();
            Console.WriteLine("\t"+"如果第一次安装，请直接输入1回车即可安装" + Description);
            Console.WriteLine();
            Console.WriteLine("==============================");
            Console.Write(":");

            switch (Console.ReadLine())
            {
                case "0":
                    return;
                case "1":
                    cmd("sc", "create " + Name + " binPath= \"" + Process.GetCurrentProcess().MainModule.FileName + " -start\" start= auto DisplayName= " + Name, false);
                    cmd("sc", "description " + Name + " \"" + Description);
                    cmd("net", "start " + Name);
                    break;
                case "2":
                    cmd("sc", "create " + Name + " binPath= \"" + Process.GetCurrentProcess().MainModule.FileName + " -start\" start= auto DisplayName= " + Name, false);
                    cmd("sc", "description " + Name + " \"" + Description + "\"");
                    break;
                case "3":
                    cmd("net", "start " + Name);
                    break;
                case "4":
                    cmd("net", "stop " + Name);
                    break;
                case "5":
                    cmd("sc", "delete " + Name);
                    break;
                case "6":
                    cmd("net", "stop " + Name);
                    cmd("sc", "delete " + Name);
                    break;
                default:
                    Console.Clear();
                    break;
            }
            install(Description);
        }


        internal static bool isIDE
        {
            get
            {
                var asm = Assembly.GetEntryAssembly();
                if (asm != null)
                {
                    return !asm.Location.Equals(Process.GetCurrentProcess().MainModule.FileName);
                }
                return Process.GetCurrentProcess().MainModule.FileVersionInfo.OriginalFilename.Equals("vshost.exe");
            }
        }

        internal static bool IsAdministrator
        {
            get
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var windowsPrincipal = new WindowsPrincipal(identity);
                    return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }

        internal static bool RequestAdministrator()
        {
            if (IsAdministrator) return true;

            Console.WriteLine("requesting administrator");
            Console.WriteLine();

            var me = Process.GetCurrentProcess();
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = me.MainModule.FileName,
                    Verb = "runas"
                });
                me.Kill();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return false;
            }
        }

        public static void Start<T>(string Description, Action core = null) where T : ServiceBase, new()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "-start":
                        ServiceBase.Run(new T());
                        return;
                    default:
                        return;
                }
            }
   
            Console.Title = Name;

           if (isIDE)
            {
                if (core != null)
                {
                    core();
                }
                else
                {
                    Console.WriteLine("无法调试服务");
                }
                while (true) Console.Read();
            }
            else
            {

                if (RequestAdministrator())
                {
                    Installer.install(Description);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("This program needs to run under administrator");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
        }
    }
}
