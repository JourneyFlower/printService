using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace printServer
{
    public class Print
    {
        [DllImport("TSCLIB.dll")]
        private static extern int sendcommand(string cmd);

        [DllImport("TSCLIB.dll")]
        private static extern int openport(string cmd);

        [DllImport("TSCLIB.dll")]
        private static extern int closeport(string cmd);

        public static int Send(string cmd)
        {
            return sendcommand(cmd);
        }

        public static int OpenPort(string cmd)
        {
            return openport(cmd);
        }

        public static int ClosePort(string cmd)
        {
            return closeport(cmd);
        }
    }
}
