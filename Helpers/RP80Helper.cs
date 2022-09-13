using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class RP80Helper
    {
        public static byte[] LineFeed(int n)
        {
            byte[] buffer = new byte[] { 0x1b, 0x64, 0 };
            byte line = ((byte)n);

            buffer[2] = line;
            return buffer;/*
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
        RawPrinterHelper.SendBytesToPrinter(printername , unmanagedPointer, buffer.Length);
        Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void LineFeed(Stream stream, int n)
        {
            byte[] buffer = new byte[] { 0x1b, 0x64, 0 };
            byte line = ((byte)n);

            buffer[2] = line;
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte[] Cut()
        {
            byte[] buffer = new byte[] { 0x1b, 0x69 };
            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void Cut(Stream stream)
        {
            byte[] buffer = new byte[] { 0x1b, 0x69 };
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte[] SetFontReset()
        {
            byte[] buffer = new byte[] { 0x1d, 0x21, 0x00 };

            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void SetFontReset(Stream stream)
        {
            byte[] buffer = new byte[] { 0x1d, 0x21, 0x00 };
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte[] SetDoubleWidth(string printername)
        {
            byte[] buffer = new byte[] { 0x1d, 0x21, 0x20 };
            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);
            */
        }
        public static byte FontA = 0x00;
        public static byte FontB = 0x01;
        public static byte Emphasized = 0x08;
        public static byte DoubleHeight = 0x10;
        public static byte DoubleWidth = 0x20;
        public static byte UnderLIne = 0x80;

        public static byte[] SetPrintMode(byte mode)
        {
            byte[] buffer = new byte[] { 0x1b, 0x21, 0 };
            buffer[2] = mode;
            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void SetPrintMode(Stream stream, byte mode)
        {
            byte[] buffer = new byte[] { 0x1b, 0x21, 0 };
            buffer[2] = mode;
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte TextLeft = 0x00;
        public static byte TextCenter = 0x01;
        public static byte TextRight = 0x02;
        public static byte[] SetJustification(byte mode)
        {
            byte[] buffer = new byte[] { 0x1b, 0x61, 0 };
            buffer[2] = mode;
            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void SetJustification(Stream stream, byte mode)
        {
            byte[] buffer = new byte[] { 0x1b, 0x61, 0 };
            buffer[2] = mode;
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte[] SetDoubleHeight(string printername)
        {
            byte[] buffer = new byte[] { 0x1d, 0x21, 0x02 };
            return buffer;
            /*
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, unmanagedPointer, buffer.Length);
            RawPrinterHelper.SendBytesToPrinter(printername, unmanagedPointer, buffer.Length);
            Marshal.FreeHGlobal(unmanagedPointer);*/
        }
        public static void SetDoubleHeight(Stream stream)
        {
            byte[] buffer = new byte[] { 0x1d, 0x21, 0x02 };
            stream.Write(buffer, 0, buffer.Length);
        }

        public static byte[] SendStringToPrinter(string szString)
        {
            //bool r = RawPrinterHelper.SendStringToPrinter(szPrinterName, szString+"\r\n");
            string str = szString;

            byte[] buffer = UTF8Encoding.UTF8.GetBytes(str + "\n");

            return buffer;
        }
        public static void SendStringToPrinter(Stream stream, string szString, bool LineFeed = true)
        {
            string str = szString;

            byte[] buffer = UTF8Encoding.UTF8.GetBytes(str + (LineFeed ? "\n" : ""));
            stream.Write(buffer, 0, buffer.Length);
        }

        public static byte[] SetDefaultLineSpacing()
        {
            byte[] buffer = new byte[] { 0x1b, 0x32 };
            return buffer;
        }
        public static void SetDefaultLineSpacing(Stream stream)
        {
            byte[] buffer = new byte[] { 0x1b, 0x32 };
            stream.Write(buffer, 0, buffer.Length);
        }
        public static byte[] SetLineSpacing(int n)
        {
            //bool r = RawPrinterHelper.SendStringToPrinter(szPrinterName, szString+"\r\n");
            byte[] buffer = new byte[] { 0x1b, 0x33, 0 };
            byte line = ((byte)n);

            buffer[2] = line;
            return buffer;
        }
        public static void SetLineSpacing(Stream stream, int n)
        {
            byte[] buffer = new byte[] { 0x1b, 0x33, 0 };
            byte line = ((byte)n);

            buffer[2] = line;
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}


