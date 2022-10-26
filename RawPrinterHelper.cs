using printServer.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace printServer
{
    /// <summary>
    /// 打印机 工具类
    /// </summary>
    public static class RawPrinterHelper
    {
        /// <summary>
        /// 结构和API声明
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pDateType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        /// <summary>
        /// 发送字节数组到打印机
        /// </summary>
        /// <param name="printerName">打印机名称</param>
        /// <param name="pBytes">字节数组</param>
        /// <param name="dwCount">数量</param>
        /// <returns>true / false</returns>
        private static bool SendBytesToPrinter(string printerName, IntPtr pBytes, int dwCount)
        {
            Int32 dwError = 0;
            Int32 dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);

            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false;

            di.pDocName = "My C# .NET RAW Document";
            di.pDateType = "RAW";

            // 打开打印机
            if (OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            // 发生错误时获取最新的错误代码
            if (!bSuccess)
                dwError = Marshal.GetLastWin32Error();

            return bSuccess;
        }

        /// <summary>
        /// 打印标签带z只有英文字符的ZPL指令
        /// </summary>
        /// <param name="printerName">打印机名称</param>
        /// <param name="zplString">ZPL文件中的内容</param>
        /// <returns>true / false</returns>
        /*public static bool SendStringToPrinter(string printerName, string zplString)
        {
            IntPtr pBytes;
            Int32 dwCount;

            dwCount = zplString.Length;

            // 假设打印机需要ANSI文本，将字符串转换成ANSI文本。
            pBytes = Marshal.StringToCoTaskMemAnsi(zplString);
            //pBytes = Marshal.StringToBSTR(zplString);
            //pBytes = Marshal.StringToCoTaskMemAuto(zplString);
            //pBytes = Marshal.StringToCoTaskMemUni(zplString);
            //pBytes = Marshal.StringToHGlobalAnsi(zplString);
            //pBytes = Marshal.StringToHGlobalAuto(zplString);
            //pBytes = Marshal.StringToHGlobalUni(zplString);
            // 将转换后的ANSI字符串发送到打印机
            if (SendBytesToPrinter(printerName, pBytes, dwCount))
            {
                Marshal.FreeCoTaskMem(pBytes);
                return true;
            }
            else
            {
                return false;
            }
        }*/



        /// <summary>
        /// 打印标签带有中文字符的ZPL指令
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="szString"></param>
        /// <returns></returns>
        public static bool SendStringToPrinter(string printerName, string szString)
        {
            //转换格式
            byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(szString);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 2);
            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                SendBytesToPrinter(printerName, ptr, bytes.Length);
            }
            catch
            {
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            return true;
        }



        [DllImport("fnthex32.dll")]
        public static extern int GETFONTHEX(
                            string BarcodeText,
                            string FontName,

                            int Orient,
                            int Height,
                            int Width,
                            int IsBold,
                            int IsItalic,
                            StringBuilder ReturnBarcodeCMD);

        [DllImport("fnthex32.dll")]
        public static extern int GETFONTHEX(string BarcodeText, string FontName, string FileName, int Orient, int Height, int Width, int IsBold, int IsItalic, StringBuilder ReturnBarcodeCMD);
        /// <summary>
        /// 转换中文
        /// </summary>
        /// <param name="chStr">转换的字符</param>
        /// <param name="tempName">存储的变量名称</param>
        /// <param name="font">使用的字体</param>
        /// <returns></returns>
        public static string ConvertChineseToHex(string chStr, string tempName, string font = "Microsoft YaHei")
        {
            StringBuilder cBuf = new StringBuilder(chStr.Length * 1024);
            int nCount = GETFONTHEX(chStr, font, 0, 25, 15, 1, 0, cBuf);
            string temp = " " + cBuf.ToString();
            temp = temp.Substring(0, nCount);
            return temp;
        }


        /// <summary> 
        /// 获取打印ZPL 
        /// </summary> 
        /// <param name="printText">打印文本 </param> 
        /// <param name="printFont">字体名称 </param> 
        /// <param name="Orientation">旋转方向 </param> 
        /// <param name="height">高度 </param> 
        /// <param name="width">宽度 </param> 
        /// <param name="IsBold">是否粗体 </param> 
        /// <param name="IsItalic">是否斜体 </param> 
        /// <returns>失败返回 null</returns> 
        /// <returns> </returns> 
        public static ConverFontToImageResult getFontText(string printText, string printFont, Orientation Orientation, int height, int width, bool IsBold, bool IsItalic)
        {
            ConverFontToImageResult result = null;
            try
            {
                StringBuilder buder = new StringBuilder(100 * 1024);
                string temp = string.Empty;
                int bold = IsBold ? 1 : 0;
                int italic = IsItalic ? 1 : 0;
                int count = GETFONTHEX(printText, printFont, (int)Orientation, height, width, bold, italic, buder);
                if (count > 0)
                {
                    result = new ConverFontToImageResult();
                    temp = buder.ToString();
                    string[] data = temp.Split(',');
                    result.ImageName = data[0].Replace("~DG", "");
                    result.TotalSize = data[1];
                    result.RowSize = data[2];
                    result.ImageData = data[3];
                }
            }
            catch (Exception ex)
            {
                //MyLogLib.MyLog.WriteLog(ex);
            }

            return result;
        }

    }
    public enum Orientation : int
    {
        Zero = 0,
        O_90 = 90,
        O_180 = 180,
        O_270 = 270
    }
    public class ConverFontToImageResult
    {
        private string imageName;
        /// <summary> 
        /// 文件名称 
        /// </summary> 
        public string ImageName
        {
            get { return imageName; }
            set { imageName = value; }
        }

        private string imageData;
        /// <summary> 
        /// 图片数据 
        /// </summary> 
        public string ImageData
        {
            get { return imageData; }
            set { imageData = value; }
        }

        private string totalSize;
        /// <summary> 
        /// 总共字节数 
        /// </summary> 
        public string TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; }
        }
        private string rowSize;
        /// <summary> 
        /// 每行字节数 
        /// </summary> 
        public string RowSize
        {
            get { return rowSize; }
            set { rowSize = value; }
        }


        /// <summary>
        /// 获取包装过的数据字符串
        /// </summary>
        /// <param name="imgName">图像名称</param>
        /// <param name="x">x，dot</param>
        /// <param name="y">y，dot</param>
        /// <param name="scal_x">x缩放</param>
        /// <param name="scal_y">y缩放</param>
        /// <returns></returns>
        public string GetDateString(string imgName, int x = 0, int y = 0, double scal_x = 1, double scal_y = 1)
        {
            return GetDateString(imgName, null, x, y, scal_x, scal_y);
        }
        /// <summary>
        /// 获取包装过的数据字符串
        /// </summary>
        /// <param name="imgName">图像名称</param>
        /// <param name="x">x，dot</param>
        /// <param name="y">y，dot</param>
        /// <param name="scal_x">x缩放</param>
        /// <param name="scal_y">y缩放</param>
        /// <returns></returns>
        public string GetDateString(string imgName, string[] contextFun, int x = 0, int y = 0, double scal_x = 1, double scal_y = 1) {
            List<String> funs = FunUtils.getFuns(contextFun,"^fun");
            return $"~DG{imgName},{TotalSize},{RowSize},{ImageData}^FO{x},{y}^XG{imgName},{scal_x},{scal_y}{String.Join("",funs)}^FS\r\n";
        }
    }
}
