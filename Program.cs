using printServer.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace printServer
{
    class Program
    {
        static void Main(string[] args)
        {
            bool console = false;
            if (console)
            {
                Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
                WinService.CoreStart();

                string cmd;
                while ((cmd = Console.ReadLine()) != "exit")
                {
                    switch (cmd)
                    {
                        case "list":
                            foreach (var it in WinService.HttpServer.Contexts)
                            {
                                Console.WriteLine(UAP.JSON.Parse(new
                                {
                                    Url = it.Url,
                                    EndPoint = it.client.Client.RemoteEndPoint.ToString(),
                                    Headers = it.Headers
                                }).Beautify());
                            }
                            break;
                        case "tcp":
                            foreach (var it in WinService.HttpServer.Clients)
                            {
                                Console.WriteLine(UAP.JSON.Parse(new
                                {
                                    Available = it.Available,
                                    Connected = it.Connected,
                                    EndPoint = it.Client.RemoteEndPoint.ToString()
                                }).Beautify());
                            }
                            break;
                    }
                }
            }
            else
            {
                Installer.Start<WinService>("本地-打印服务", WinService.CoreStart);
            }
        }

        public static string Config(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
        public static int SendCMD(List<PrintParam> pList)
        {
            Print.OpenPort(Config("Machine"));
            foreach (PrintParam param in pList)
            {
                Print.Send(param.cmd);
            }

            Print.OpenPort(Config("Machine"));
            return 1;
        }

        public static int SendZPL(List<PrintParam> pList) {
            string strCmd1 = @"^XA
                                ^CW1,E:SIMSUN.TTF
                                ^SEE:GB18030.DAT^CI26
                                ^FO50,60^A1N,20,20^FD简体中文abcd1234^FS
                                ^FO50,160^A1N,30,30^FD简体中文abcd1234^FS
                                ^FO50,260^A1N,50,50^FD简体中文abcd1234^FS
                                ^XZ";
            RawPrinterHelper.SendStringToPrinter(Config("Machine"), contextTo(strCmd1));
            foreach (PrintParam print in pList)
            {
                RawPrinterHelper.SendStringToPrinter(Config("Machine"), contextTo(print.cmd));
            }
            return 1;
        }

        private static String contextTo(String cmd) {
            string regstr = @"\!<<(.*?)(!\})*\>>"; //提取页面所有元素内容
            Regex reg = new Regex(regstr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection mc = reg.Matches(cmd);
            cmd = cmd.Replace("^XA\n", "").Replace("\n^XZ", "");
            foreach (Match m in mc)
            {
                
                String[] fun = m.Groups[0].ToString().Substring(3, m.Groups[0].Length - 5).Split('|');
                String[] contextFun = fun[0].Split(' ');
                String contextInfo = fun[1];
                //cmd = cmd.Replace(m.Groups[0].ToString(), guid);
                //cmd += "\n" + RawPrinterHelper.ConvertChineseToHex(contextInfo, guid);
                cmd = cmd.Replace(m.Groups[0].ToString(), "");
                //int fontSize = Convert.ToInt32(contextInfo.Length);
                //double size_mm = GetMilimeter(fontSize, 96);
                //int size_printer = GetDot(size_mm, 300);
                int rowLength = Convert.ToInt32(FunUtils.getFun(contextFun, "^rz", "0"));
                int height = Convert.ToInt32(FunUtils.getFun(contextFun, "^fz", "0")) * 2;
                int width = Convert.ToInt32(FunUtils.getFun(contextFun, "^fz", "0"));
                int x = Convert.ToInt32(FunUtils.getFun(contextFun, "^x", "0"));
                int y = Convert.ToInt32(FunUtils.getFun(contextFun, "^y", "0"));
                if (rowLength>0 && contextInfo.Length > rowLength)
                {
                    while (String.IsNullOrEmpty(contextInfo) == false)
                    {
                        if (contextInfo.Length > rowLength)
                        {
                            cmd += getZPL(contextInfo.Substring(0, rowLength),x,y,height, width, contextFun);
                            contextInfo = contextInfo.Substring(rowLength);
                            y += height;
                        }
                        else
                        {
                            cmd += getZPL(contextInfo, x, y, height, width, contextFun);
                            contextInfo = null;
                            y += height;
                        }
                    }
                }
                else {
                    cmd += getZPL(contextInfo,x,y, height, width, contextFun);
                }

            }
            cmd = "^XA\n" + cmd + "\n^XZ";
            return cmd;
        }

        private static String getZPL(String contextInfo,int x,int y, int height,int width,String[] contextFun) {
            return RawPrinterHelper.getFontText(contextInfo, "黑体", Orientation.Zero, height, width, false, false).GetDateString(Guid.NewGuid().ToString(), contextFun, x,y);
        }
       


        public static int GetDot(double milimeter, int dpi)
        {
            //1英寸=25.4mm
            //300dpi = 300dot per inch 300点每英寸
            return (int)Math.Round(milimeter * dpi / 25.4, 0);
        }

        public static double GetMilimeter(int pixel, int dpi)
        {
            return Math.Round(pixel * 25.4d / dpi, 2);
        }

        public static PrintParam GetParam(string code)
        {
            return new PrintParam
            {
                cmd = $@"SIZE 40 mm,30 mm
                        GAP 4 mm,0 mm
                        DIRECTION 1
                        CLS
                        QRCODE 5,10,H,4,A,0,""{code}""
                        TEXT 5,100,""3"",0,1,1,""{code}""
                        PRINT 1"
            } ;
        }
    }
}
