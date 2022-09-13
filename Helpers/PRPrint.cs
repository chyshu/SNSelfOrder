using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace SNSelfOrder.Helpers
{
    
    class PRPrint
    {
        public static void PrintRecipt(string cmp_no, string str_no, string pos_no, string dealno, DateTime accdate)
        {

            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no and accdate=@accdate";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                cmd.Parameters.AddWithValue("str_no", str_no);
                cmd.Parameters.AddWithValue("pos_no", pos_no);
                cmd.Parameters.AddWithValue("deal_no", dealno);
                cmd.Parameters.AddWithValue("accdate", accdate);
                DataTable pstrn01sDT = new DataTable();
                da.Fill(pstrn01sDT);
                if (pstrn01sDT.Rows.Count > 0)
                {
                    DataRow pstrn01s = pstrn01sDT.Rows[0];
                    using (MemoryStream ms = new MemoryStream())
                    {

                        cmd.CommandText = "select * from pstrn02s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                        cmd.Parameters.AddWithValue("str_no", str_no);
                        cmd.Parameters.AddWithValue("pos_no", pos_no);
                        cmd.Parameters.AddWithValue("deal_no", dealno);
                        DataTable pstrn02sDT = new DataTable();
                        da.Fill(pstrn02sDT);
                        cmd.CommandText = "select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                        cmd.Parameters.AddWithValue("str_no", str_no);
                        cmd.Parameters.AddWithValue("pos_no", pos_no);
                        cmd.Parameters.AddWithValue("deal_no", dealno);
                        DataTable pstrn03sDT = new DataTable();
                        da.Fill(pstrn03sDT);
                        if (!PRPosUtils.Receipt_head_Image.Equals(""))
                        {
                            string path = Path.Combine(PRPosUtils.FilePath, PRPosUtils.Receipt_head_Image);
                            using (var fs = new FileStream(path, FileMode.Open))
                            {
                                Bitmap bmp = new Bitmap(fs);
                                int sizeW = bmp.Width / 8;
                                if ((bmp.Width % 8) > 0) sizeW += 1;

                                byte[] data = new byte[sizeW * bmp.Height];
                                int p = 0;
                                int bits = 0;
                                byte d = 0;

                                for (int y = 0; y < bmp.Height; y++)
                                {
                                    for (int x = 0; x < bmp.Width; x++)

                                    {
                                        Color pixel = bmp.GetPixel(x, y);
                                        if (pixel.R == 0)
                                        {
                                            byte v = 0x01;
                                            v = (byte)(v << (7 - bits));
                                            d = (byte)(d | v);
                                        }
                                        bits += 1;
                                        if (bits == 8)
                                        {
                                            data[p] = d;
                                            p += 1;
                                            bits = 0;
                                            d = 0;
                                        }
                                    }
                                }
                                if (bits != 0)
                                {
                                    data[p] = d;
                                }
                                byte[] imagebuffer = new byte[sizeW * bmp.Height + 4];
                                imagebuffer[0] = (byte)(sizeW % 256);
                                imagebuffer[1] = (byte)(sizeW / 256);
                                imagebuffer[2] = (byte)(bmp.Height % 256);
                                imagebuffer[3] = (byte)(bmp.Height / 256);
                                for (int i = 4; i < imagebuffer.Length; i++)
                                {
                                    imagebuffer[i] = data[i - 4];
                                }
                                byte[] cmdbuffer = new byte[] { 0x1d, 0x76, 0x30, 0x00 };
                                ms.Write(cmdbuffer, 0, cmdbuffer.Length);

                                ms.Write(imagebuffer, 0, imagebuffer.Length);
                            }
                        }
                        byte[] buffer;
                        //buffer= RP80Helper.SetPrintMode((byte)(RP80Helper.FontB | RP80Helper.DoubleWidth | RP80Helper.DoubleHeight));
                        //ms.Write(buffer, 0, buffer.Length);
                        //buffer = RP80Helper.SetPrintMode((byte)(RP80Helper.FontA));
                        //ms.Write(buffer, 0, buffer.Length);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetDefaultLineSpacing(ms);

                        //buffer = RP80Helper.SetDefaultLineSpacing();
                        //ms.Write(buffer, 0, buffer.Length);
                        //buffer = RP80Helper.SetJustification((byte)(RP80Helper.TextCenter));
                        // ms.Write(buffer, 0, buffer.Length);
                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextCenter));
                        //buffer = RP80Helper.SetLineSpacing(80);
                        //ms.Write(buffer, 0, buffer.Length);
                        //buffer = RP80Helper.SetFontReset();
                        //ms.Write(buffer, 0, buffer.Length);
                        if (!PRPosDB.ReadString("recipt_title").Equals(""))
                        {

                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("recipt_title"));

                        }
                        if (!PRPosDB.ReadString("company_name").Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("company_name"));
                            //buffer = RP80Helper.SendStringToPrinter(PRPosDB.ReadString("company_name"));
                            //ms.Write(buffer, 0, buffer.Length);
                        }
                        //  buffer = RP80Helper.SetPrintMode((byte)(RP80Helper.FontA));
                        //  ms.Write(buffer, 0, buffer.Length);
                        //  buffer = RP80Helper.SetPrintMode((byte)(RP80Helper.FontB));
                        //  ms.Write(buffer, 0, buffer.Length);
                        if (!PRPosDB.ReadString("buss_no").Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("buss_no"));
                            //buffer = RP80Helper.SendStringToPrinter(PRPosDB.ReadString("buss_no"));
                            //ms.Write(buffer, 0, buffer.Length);
                        }
                        if (!PRPosDB.ReadString("store_name").Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("store_name"));
                            //buffer = RP80Helper.SendStringToPrinter(PRPosDB.ReadString("store_name"));
                            //ms.Write(buffer, 0, buffer.Length);
                        }
                        if (!PRPosDB.ReadString("store_phone").Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("store_phone"));
                            //buffer = RP80Helper.SendStringToPrinter(PRPosDB.ReadString("store_phone"));
                            //ms.Write(buffer, 0, buffer.Length);
                        }

                        string addr1 = PRPosDB.ReadString("store_address_line1");
                        if (!addr1.Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, addr1);
                        }
                        addr1 = PRPosDB.ReadString("store_address_line2");
                        if (!addr1.Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, addr1);
                        }
                        addr1 = PRPosDB.ReadString("store_address_line3");
                        if (!addr1.Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, addr1);
                        }
                        RP80Helper.LineFeed(ms, 1);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetLineSpacing(ms, 100);
                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextLeft));

                        RP80Helper.SendStringToPrinter(ms, "Table:", false);

                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA | RP80Helper.DoubleWidth | RP80Helper.DoubleHeight));
                        RP80Helper.SendStringToPrinter(ms, " " + pstrn01s["ref_no"] + " " + (pstrn01s["ref_no"].ToString().Equals("1") ? "Dine In" : "Takeaway"));
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetDefaultLineSpacing(ms);
                        //RP80Helper.SetLineSpacing(ms, 80);

                        string[] lines = new string[]
                        {
                          "Date :" + DateTime.Parse(pstrn01s["tdate"].ToString()).ToString("MM/dd/yyyy HH:mm:ss"),
                          "No   :"+ PRPosUtils.PosCode +" "+pstrn01s["deal_no"].ToString()
                        };
                        foreach (string line in lines)
                        {
                            buffer = RP80Helper.SendStringToPrinter(line);
                            ms.Write(buffer, 0, buffer.Length);
                        }
                        string output = "";
                        /*
                        for(int x = 0; x < 48; x++)
                        {
                            output += "=";
                        }*/
                        RP80Helper.SendStringToPrinter(ms, output);

                        //buffer = RP80Helper.SetPrintMode((byte)(RP80Helper.FontA | RP80Helper.Emphasized));
                        //buffer = RP80Helper.SetPrintMode((byte)(RP80Helper.FontA));
                        //ms.Write(buffer, 0, buffer.Length);
                        foreach (DataRow pstrn02s in pstrn02sDT.Rows)
                        {
                            cmd.CommandText = "select * from pstrn04s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no and item_no=@item_no and item_code=@item_code";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", pstrn02s["cmp_no"].ToString());
                            cmd.Parameters.AddWithValue("str_no", pstrn02s["str_no"].ToString());
                            cmd.Parameters.AddWithValue("pos_no", pstrn02s["pos_no"].ToString());
                            cmd.Parameters.AddWithValue("deal_no", pstrn02s["deal_no"].ToString());
                            cmd.Parameters.AddWithValue("item_no", pstrn02s["item_no"].ToString());
                            cmd.Parameters.AddWithValue("item_code", pstrn02s["item_code"].ToString());
                            DataTable pstrn04sDT = new DataTable();
                            da.Fill(pstrn04sDT);
                            decimal amt = 0;
                            decimal.TryParse(pstrn02s["amt"].ToString(), out amt);
                            decimal sprice = 0;
                            decimal.TryParse(pstrn02s["sprice"].ToString(), out sprice);
                            string item_name = pstrn02s["item_name"].ToString();
                            if (pstrn02s["item_type"].ToString().Equals("C"))
                            {
                                if (item_name.Length > PRPosUtils.ReceiptItemWidth)
                                {

                                    output = item_name.Substring(0, PRPosUtils.ReceiptItemWidth);
                                    int px = output.Length - 1;
                                    for (int i = output.Length - 1; i >= 0; i--)
                                    {
                                        if (item_name.Substring(i, 1).Equals(" "))
                                        {
                                            px = i;
                                            output = item_name.Substring(0, px);
                                            break;
                                        }
                                    }

                                    output = "  " + output.PadRight(PRPosUtils.ReceiptItemWidth, ' ');
                                    RP80Helper.SendStringToPrinter(ms, output);
                                    output = "  " + item_name.Substring(px);
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                                else
                                {
                                    output = item_name.ToString();
                                    output = "  " + output.PadRight(PRPosUtils.ReceiptItemWidth, ' ');
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                                foreach (DataRow pstrn04s in pstrn04sDT.Rows)
                                {
                                    output = "   " + pstrn04s["modifier_txt"].ToString();
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                            }
                            else
                            {
                                if (item_name.Length > PRPosUtils.ReceiptCharacters)
                                {
                                    int px = PRPosUtils.ReceiptCharacters - 1;
                                    output = item_name.Substring(0, 40);
                                    for (int i = output.Length - 1; i >= 0; i--)
                                    {
                                        if (item_name.Substring(i, 1).Equals(" "))
                                        {
                                            px = i;
                                            output = item_name.Substring(0, px);
                                            break;
                                        }
                                    }

                                    output = output.PadRight(40, ' ') + string.Format("{0:C}", amt).PadLeft(8, ' ');
                                    RP80Helper.SendStringToPrinter(ms, output);

                                    output = "  " + item_name.Substring(px);
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                                else
                                {
                                    output = item_name.ToString();
                                    output = output.PadRight(40, ' ');
                                    output = output + string.Format("{0:C}", amt).PadLeft(8, ' ');
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                                //int qty = 0;
                                //int.TryParse(pstrn02s["qty"].ToString(), out qty);                           
                                // output ="   "+qty.ToString().PadRight(4, ' ') + "@"+ string.Format("{0:C}", amt);

                                foreach (DataRow pstrn04s in pstrn04sDT.Rows)
                                {
                                    output = "   " + pstrn04s["modifier_txt"].ToString();
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }
                            }
                        }
                        decimal totamt = 0;
                        decimal.TryParse(pstrn01s["tot_amt"].ToString(), out totamt);
                        output = "Subtotal".PadRight(PRPosUtils.ReceiptCharacters - PRPosUtils.ReceiptPriceCharacters, ' ') + string.Format("{0:C}", totamt).PadLeft(PRPosUtils.ReceiptPriceCharacters, ' ');

                        RP80Helper.SendStringToPrinter(ms, output);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontB | RP80Helper.DoubleWidth | RP80Helper.DoubleHeight));

                        RP80Helper.SetLineSpacing(ms, 100);
                        output = ("TOTAL for " + pstrn02sDT.Rows.Count + " ITEMS").PadRight(22, ' ') + string.Format("{0:C}", totamt).PadLeft(10, ' ');
                        RP80Helper.SendStringToPrinter(ms, output);

                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetDefaultLineSpacing(ms);
                        foreach (DataRow pstrn03s in pstrn03sDT.Rows)
                        {
                            RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontB | RP80Helper.DoubleWidth | RP80Helper.DoubleHeight));

                            RP80Helper.SetLineSpacing(ms, 100);
                            if (pstrn03s["ecp_type"].ToString().Equals("EFTPOS"))
                            {
                                decimal ecpamt = 0;
                                decimal.TryParse(pstrn03s["ecp_amt"].ToString(), out ecpamt);
                                output = ("Credit Card").PadRight(22, ' ') + string.Format("{0:C}", ecpamt).PadLeft(10, ' ');
                                RP80Helper.SendStringToPrinter(ms, output);
                            }
                            else
                            {
                                decimal ecpamt = 0;
                                decimal.TryParse(pstrn03s["ecp_amt"].ToString(), out ecpamt);
                                decimal change_amt = 0;
                                decimal.TryParse(pstrn03s["change_amt"].ToString(), out change_amt);
                                output = pstrn03s["ecp_name"].ToString().PadRight(22, ' ') + string.Format("{0:C}", ecpamt).PadLeft(10, ' ');
                                RP80Helper.SendStringToPrinter(ms, output);
                                if (change_amt > 0)
                                {
                                    output = "CHANGE".PadRight(22, ' ') + string.Format("{0:C}", change_amt).PadLeft(10, ' ');
                                    RP80Helper.SendStringToPrinter(ms, output);
                                }

                            }
                            RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));

                        }
                        RP80Helper.SetDefaultLineSpacing(ms);
                        decimal gst = 0;
                        decimal.TryParse(pstrn01s["tax_amt"].ToString(), out gst);
                        output = ("TRANSCATION INCLUDES GST:").PadRight(36, ' ') + string.Format("{0:C}", gst).PadLeft(8, ' ');
                        RP80Helper.SendStringToPrinter(ms, output);
                        RP80Helper.LineFeed(ms, 1);
                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextCenter));

                        foreach (string footer in PRPosUtils.ReceiptFooter)
                        {

                            RP80Helper.SendStringToPrinter(ms, footer);
                        }




                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextLeft));
                        RP80Helper.LineFeed(ms, 5);
                        RP80Helper.Cut(ms);

                        ms.Seek(0, 0);

                        IntPtr pUnmanagedBytes = new IntPtr(0);
                        int nLength;

                        nLength = Convert.ToInt32(ms.Length);
                        buffer = new byte[nLength];
                        ms.Seek(0, 0);
                        int bytes = ms.Read(buffer, 0, nLength);
                        // Allocate some unmanaged memory for those bytes.
                        pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                        // Copy the managed byte array into the unmanaged array.
                        Marshal.Copy(buffer, 0, pUnmanagedBytes, nLength);
                        // Send the unmanaged bytes to the printer.
                        foreach (var print in PRPosUtils.PosPrinters.Select(fld=>new { fld.DeviceType,fld.DeviceName,fld.PrinterName,fld.IsDefault}).
                            Where(f=> f.DeviceType == "Receipt Printer" &&  f.PrinterName!="" ))
                        {

                            if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => print.DeviceName.Trim() == name.Trim()))
                                RawPrinterHelper.SendBytesToPrinter(print.PrinterName, pUnmanagedBytes, nLength);
                        }
                        //RawPrinterHelper.SendBytesToPrinter(PRPosUtils.ReceiptPrinterName, pUnmanagedBytes, nLength);
                        // Free the unmanaged memory that you allocated earlier.
                        Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    }
                }
            }

        }
        static string PepeatChar(int len, string str)
        {
            string ret = "";
            for (int i = 0; i < len; i++)
                ret += str;
            return ret;
        }
        public static void PrintJobList(string cmp_no, string str_no, string pos_no, string dealno, DateTime accdate)
        {
            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            nfi.CurrencySymbol = "$";
            DateTime PrintTime = DateTime.Now;
            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no and accdate=@accdate";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                cmd.Parameters.AddWithValue("str_no", str_no);
                cmd.Parameters.AddWithValue("pos_no", pos_no);
                cmd.Parameters.AddWithValue("deal_no", dealno);
                cmd.Parameters.AddWithValue("accdate", accdate);
                DataTable pstrn01sDT = new DataTable();
                da.Fill(pstrn01sDT);
                if (pstrn01sDT.Rows.Count > 0)
                {
                    DataRow pstrn01s = pstrn01sDT.Rows[0];

                    DateTime DealTime = new DateTime();
                    DateTime.TryParse(pstrn01s["tdate"].ToString(), out DealTime);

                    cmd.CommandText = "select distinct printer_name from pstrn02s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                    cmd.Parameters.AddWithValue("str_no", str_no);
                    cmd.Parameters.AddWithValue("pos_no", pos_no);
                    cmd.Parameters.AddWithValue("deal_no", dealno);
                    DataTable printer_nameDT = new DataTable();
                    da.Fill(printer_nameDT);
                    foreach (DataRow printer_name in printer_nameDT.Rows)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(new byte[] { 0x1b, 0x21, 0x20 }, 0, 3);
                            ms.Write(new byte[] { 0x1b, 0x61, 0x1 }, 0, 3);
                            string str = "JOB LIST";
                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            ms.Write(new byte[] { 0x1b, 0x61, 0x0 }, 0, 3);
                            str = PepeatChar(PRPosUtils.ReceiptDWCharacters, "=");   // 24 characters per line

                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            str = DealTime.ToString("dd/MM/yyyy") + pstrn01s["deal_no"].ToString().PadLeft(PRPosUtils.ReceiptDWCharacters - 10);
                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            if (pstrn01s["order_type"].ToString().Equals("1"))
                            {
                                str = "Dine In: " + pstrn01s["ref_no"].ToString();
                            }
                            else
                            {
                                str = "Takeaway: " + pstrn01s["ref_no"].ToString();
                            }
                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            str = "Print Time:\r\n" + PrintTime.ToString();
                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            str = PepeatChar(PRPosUtils.ReceiptDWCharacters, "=");   // 24 characters per line
                            ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);

                            cmd.CommandText = "select * from pstrn02s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no and printer_name=@printer_name";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                            cmd.Parameters.AddWithValue("str_no", str_no);
                            cmd.Parameters.AddWithValue("pos_no", pos_no);
                            cmd.Parameters.AddWithValue("deal_no", dealno);
                            cmd.Parameters.AddWithValue("printer_name", printer_name["printer_name"].ToString());
                            DataTable pstrn02sDT = new DataTable();
                            da.Fill(pstrn02sDT);
                            foreach (DataRow pstrn02s in pstrn02sDT.Rows)
                            {
                                cmd.CommandText = "select * from pstrn04s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and deal_no=@deal_no and item_no=@item_no and item_code=@item_code";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("cmp_no", cmp_no);
                                cmd.Parameters.AddWithValue("str_no", str_no);
                                cmd.Parameters.AddWithValue("pos_no", pos_no);
                                cmd.Parameters.AddWithValue("deal_no", dealno);
                                cmd.Parameters.AddWithValue("item_no", pstrn02s["item_no"].ToString());
                                cmd.Parameters.AddWithValue("item_code", pstrn02s["item_code"].ToString());
                                DataTable pstrn04sDT = new DataTable();
                                da.Fill(pstrn04sDT);
                                int qty = int.Parse(pstrn02s["qty"].ToString());
                                decimal sprice = decimal.Parse(pstrn02s["sprice"].ToString());

                                str = pstrn02s["qty"].ToString() + " X " + string.Format(nfi, "{0:C}", sprice);
                                ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);

                                str = pstrn02s["kitchen_name"].ToString().Equals("") ? pstrn02s["item_name"].ToString() : pstrn02s["kitchen_name"].ToString();

                                ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);

                                ms.Write(new byte[] { 0x1d, 0x4c, 0x20, 0x00 }, 0, 4);// Left Margin
                                foreach (DataRow r in pstrn04sDT.Rows)
                                {
                                    str = "  " + r["caption"].ToString();
                                    ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                                }
                                ms.Write(new byte[] { 0x1d, 0x4c, 0x00, 0x00 }, 0, 4);// Left Margin
                                str = "  ";
                                ms.Write(Encoding.UTF8.GetBytes(str + "\r\n"), 0, Encoding.UTF8.GetBytes(str + "\r\n").Length);
                            }
                            RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextLeft));
                            RP80Helper.LineFeed(ms, 5);
                            RP80Helper.Cut(ms);
                            ms.Seek(0, 0);

                            IntPtr pUnmanagedBytes = new IntPtr(0);
                            int nLength;
                            byte[] buffer;
                            nLength = Convert.ToInt32(ms.Length);
                            buffer = new byte[nLength];
                            ms.Seek(0, 0);
                            int bytes = ms.Read(buffer, 0, nLength);
                            // Allocate some unmanaged memory for those bytes.
                            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                            // Copy the managed byte array into the unmanaged array.
                            Marshal.Copy(buffer, 0, pUnmanagedBytes, nLength);
                            // Send the unmanaged bytes to the printer.

                            foreach (var kitchenprinter in PRPosUtils.PosPrinters.Select(fld => new { fld.DeviceType, fld.DeviceName, fld.PrinterName, fld.IsDefault }).
                                Where(f => f.DeviceType.ToUpper() == "KITCHEN PRINTER" && f.PrinterName != ""))
                            {

                                if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => kitchenprinter.DeviceName.Trim() == name.Trim()))
                                    RawPrinterHelper.SendBytesToPrinter(kitchenprinter.PrinterName, pUnmanagedBytes, nLength);
                            }
                            /*
                            DataRow print = PRPosDB.getPrinter("Kitchen Printer", printer_name["printer_name"].ToString());
                            if (print != null)
                            {
                                string KitchenPrinterName = print["device_name"].ToString();
                                // RawPrinterHelper.SendBytesToPrinter(ReceiptPrinterName, pUnmanagedBytes, nLength);
                                RawPrinterHelper.SendBytesToPrinter(KitchenPrinterName, pUnmanagedBytes, nLength);
                            }*/
                            // Free the unmanaged memory that you allocated earlier.
                            Marshal.FreeCoTaskMem(pUnmanagedBytes);
                        }
                    }
                }
            }
        }

    }
}
