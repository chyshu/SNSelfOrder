using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public static class PRPosDB
    {

        public static string dbPath = @"prposdb.db3";
        public static string cnStr = "data source=" + Path.Combine( PRPosUtils.App_root , dbPath);
        public static int MenuColumns = 10;
        public static int MenuRows = 1;
        public static void InitSQLiteDb()
        {
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from pstrn04s ";
                cmd.Parameters.Clear();
                bool f = false;
                DataTable pstrn04sDT = new DataTable();
                da.Fill(pstrn04sDT);
                foreach (DataColumn column in pstrn04sDT.Columns)
                {
                    if (column.ColumnName.Equals("inpqty"))
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    cmd.CommandText = "ALTER TABLE pstrn04s add inpqty  int";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "update pstrn04s  set inpqty=qty ";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "select * from psmodset01 ";
                cmd.Parameters.Clear();
                f = false;
                DataTable psmodset01DT = new DataTable();
                da.Fill(psmodset01DT);
                foreach (DataColumn column in psmodset01DT.Columns)
                {
                    if (column.ColumnName.Equals("bgcolor"))
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    cmd.CommandText = "drop table psmodset01 ";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "drop table psmodsetti ";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "CREATE TABLE psmodsetti ( sid INTEGER, psid varchar(20), modifier_code varchar(20), caption varchar(50), caption_fn varchar(50), mod_type varchar(1), price_type varchar(1), amount numeric(8, 2), max_selection INTEGER, min_selection INTEGER, next_modset varchar(20), img_flag varchar(1), image varchar(60), ftime DateTime, upd_date DateTime, del_flag varchar(1), disp_caption varchar(1), disp_price varchar(1), fontcolor varchar(10), bgcolor varchar(10), fontfamily varchar(60), fontsize varchar(20), fontstyle varchar(10), soldout varchar(1), str_soldout varchar(1), str_upd_date DateTime,   PRIMARY KEY(sid) )";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "CREATE TABLE psmodset01 ( sid INTEGER NOT NULL, customerid varchar(20), modset_code varchar(20), caption nvarchar(50), caption_fn nvarchar(50), mod_type nvarchar(1), amount NUMERIC(6, 2), max_selection int DEFAULT 1, min_selection int DEFAULT 0, next_modset nvarchar(12), fontcolor varchar(10), bgcolor varchar(10), fontfamily varchar(60), fontsize varchar(20), fontstyle varchar(10), del_flag nvarchar(1), upd_date DateTime, PRIMARY KEY(sid) )";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static string ReadString(string code, string code_type,string field,string defaultvalue="")
        {
            string ret = defaultvalue;

            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from pssystem where code=@code and code_type=@code_type ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                cmd.Parameters.AddWithValue("code_type", code_type);
                SQLiteDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                        ret = dr[field].ToString();
                }
                catch (Exception err)
                {
                    // Console.WriteLine(err.Message);
                    PRPosUtils.writelog("ReadString " + code + " = " + err.Message);
                }
                finally
                {
                    dr.Close();
                }
            }
            return ret;
        }
        public static string ReadString(string code,string code_type, string defaultvalue = "")
        {
            string ret = defaultvalue;

            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from pssystem where code=@code and code_type=@code_type ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                cmd.Parameters.AddWithValue("code_type", code_type);
                SQLiteDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                        ret = dr["f1"].ToString();
                }
                catch (Exception err)
                {
                    // Console.WriteLine(err.Message);
                    PRPosUtils.writelog("ReadString " + code + " = " + err.Message);
                }
                finally
                {
                    dr.Close();
                }
            }
            return ret;
        }
        public static string ReadString(string code , string defaultvalue = "")
        {
            string ret = defaultvalue;

            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from pssystem where code=@code ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                SQLiteDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                        ret = dr["f1"].ToString();
                }
                catch (Exception err)
                {
                    // Console.WriteLine(err.Message);
                    PRPosUtils.writelog("ReadString " + code + " = " + err.Message);
                }
                finally
                {
                    dr.Close();
                }
            }
            return ret;
        }
        public static int ReadInteger(string code)
        {
            int ret = 0;
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {


                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from pssystem where code=@code ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                SQLiteDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                    {
                        ret = (dr["i1"] == DBNull.Value || dr["i1"].ToString().Equals("")) ? 0 : int.Parse(dr["i1"].ToString());
                    }
                }
                catch (Exception err)
                {
                    PRPosUtils.writelog("ReadInteger " + code + " = " + err.Message);
                }
                finally
                {
                    dr.Close();
                }
            }
            return ret;
        }
        public static int ReadInteger(string code, string code_type, string field,int defaultvalue=0)
        {
            int ret = defaultvalue;
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {


                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "select * from pssystem where code=@code and code_type=@code_type ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                cmd.Parameters.AddWithValue("code_type", code_type);
                SQLiteDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                    {
                        int v = defaultvalue;
                        if (!int.TryParse(dr[field].ToString(), out v))
                        {
                            v = defaultvalue;
                        }
                        ret = v;
                    }
                }
                catch (Exception err)
                {
                    PRPosUtils.writelog("ReadInteger " + code + " = " + err.Message);
                }
                finally
                {
                    dr.Close();
                }
            }
            return ret;
        }
        public static void ReadBackImageParameter()
        {
            PRPosUtils.Img_blank = ReadString("blank_image", "PageImage");
            PRPosUtils.Img_Start = ReadString("start_page", "PageImage");
            PRPosUtils.Img_OrderType = ReadString("order_type_page", "PageImage");
            PRPosUtils.Img_Buzzer    = ReadString("table_number_page", "PageImage");
            PRPosUtils.Img_DiningPerson = ReadString("dineine_person_page", "PageImage");
            PRPosUtils.Img_MemberCard = ReadString("member_card_page" , "PageImage");
            PRPosUtils.Img_Finally = ReadString("final_page", "PageImage");
            PRPosUtils.Img_Payment = ReadString("payment_page", "PageImage");

            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Start)))
                PRPosUtils.Img_Start = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_OrderType)))
                PRPosUtils.Img_OrderType = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Buzzer)))
                PRPosUtils.Img_Buzzer = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_DiningPerson)))
                PRPosUtils.Img_DiningPerson = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_MemberCard)))
                PRPosUtils.Img_MemberCard = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Finally)))
                PRPosUtils.Img_Finally = PRPosUtils.Img_blank;
            if (!File.Exists(System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Payment)))
                PRPosUtils.Img_Payment = PRPosUtils.Img_blank;
        }

        /*    public static void ReadBannerImageParameter()
            {
                PRPosUtils.BannerImages = new List<BannerImage>();
                using (SQLiteConnection connection = new SQLiteConnection(cnStr))
                {
                    connection.Open();
                    SQLiteCommand cmd = connection.CreateCommand();
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;

                    cmd.CommandText = @"select * from pssystem where    ifnull(f3,'N')='Y' and  code_type='banner_image' order by i1 ";// where img_flag='N' ";
                    cmd.Parameters.Clear();
                    DataTable  pssystemDT = new DataTable();
                    da.Fill(pssystemDT);
                    foreach (DataRow row in pssystemDT.Rows)
                    {
                        if (!row["f1"].ToString().Equals(""))
                        {
                            BannerImage pimage = new BannerImage()
                            {
                                Imagefile = row["f1"].ToString(),
                                Disp_order = row["i1"].ToString(),
                                Disp_delay = row["i2"].ToString(),
                            };
                        }
                    }
                }
            }*/

        public static void ReadParameter()
        {
            //PRPosUtils.Output_Folder = ReadString("output_file_path").Equals("") ? @".\output" : ReadString("output_file_path");
            //PRPosUtils.Spool_Folder = ReadString("Spool_Folder").Equals("") ? @".\spool" : ReadString("Spool_Folder");
            //    PRPosUtils.PosCode = ReadString("pos_code");
            //     PRPosUtils.PosConnection = ReadString("connection_code");
            //PRPosUtils.Image_Path = ReadString("picture_file_path").Equals("") ? @".\images" : ReadString("picture_file_path");

            //PRPosUtils.App_root = System.AppDomain.CurrentDomain.BaseDirectory;
            string[] directories = PRPosUtils.App_root.Split(System.IO.Path.DirectorySeparatorChar);
            /*
            for (int i=0;i< directories.Length-1;i++)
            {
                if (PRPosUtils.App_root.Equals(""))
                    PRPosUtils.App_root += directories[i];
                else
                    PRPosUtils.App_root += Path.DirectorySeparatorChar+ directories[i];

                if (!Directory.Exists(PRPosUtils.App_root))
                {
                    Directory.CreateDirectory(PRPosUtils.App_root);
                }

            }
            directories = PRPosUtils.Output_Folder.Split(Path.DirectorySeparatorChar);
            PRPosUtils.App_root = "";
            for (int i = 0; i < directories.Length - 1; i++)
            {
                if (PRPosUtils.App_root.Equals(""))
                    PRPosUtils.App_root += directories[i];
                else
                    PRPosUtils.App_root += Path.DirectorySeparatorChar + directories[i];

                if (!Directory.Exists(PRPosUtils.App_root))
                {
                    Directory.CreateDirectory(PRPosUtils.App_root);
                }

            }
            directories = PRPosUtils.Spool_Folder.Split(Path.DirectorySeparatorChar);
            PRPosUtils.App_root = "";
            for (int i = 0; i < directories.Length - 1; i++)
            {
                if (PRPosUtils.App_root.Equals(""))
                    PRPosUtils.App_root += directories[i];
                else
                    PRPosUtils.App_root += Path.DirectorySeparatorChar + directories[i];

                if (!Directory.Exists(PRPosUtils.App_root))
                {
                    Directory.CreateDirectory(PRPosUtils.App_root);
                }

            }*/
            // Console.WriteLine(PRPosUtils.App_root);
            ReadBackImageParameter();
            PRPosUtils.Ask_OrderType = ReadString("ask_order_type");
            PRPosUtils.Ask_Table_Number = ReadString("ask_table_number");
            // PRPosUtils.Ask_Buzzer_Number = ReadString("ask_buzzer_number");
            PRPosUtils.Ask_Member_Card = ReadString("ask_member_card");
            PRPosUtils.Ask_Covers = ReadString("ask_persson_number");

            PRPosUtils.DefaultOrderType = ReadString("default_order_type");

            PRPosUtils.Default_FasktKey = ReadString("default_fastkey");
            PRPosUtils.BannerPlayTime = ReadInteger("banner_play_time");
            PRPosUtils.MenuButtonWidth = ReadInteger("menu_button_width");
            PRPosUtils.MenuHeight = ReadInteger("menu_height");
            PRPosUtils.BannerHeight = ReadInteger("banner_height");
            PRPosUtils.DefaultFontSize = ReadInteger("default_font_size");
            PRPosUtils.DefaultFontName = ReadString("default_font_name");
            PRPosUtils.DefaultFontColor = ReadString("default_font_color");
            PRPosUtils.PlusFile = ReadString("item_plus_icon");
            PRPosUtils.MinusFile = ReadString("item_minus_icon");
            PRPosUtils.MaxQty = ReadInteger("max_qty");

            PRPosUtils.WaitingTime = ReadInteger("waiting_time");

            PRPosUtils.AlterDisplayTime = ReadInteger("alert_display_time");

            PRPosUtils.BuzzerPage_Message_Dinein = ReadString("buzzerpage_messge_dinein").Equals("") ? "Please Input Table Number": ReadString("buzzerpage_messge_dinein");
            PRPosUtils.BuzzerPage_Caption_Dinein = ReadString("buzzerpage_caption_dinein");
            PRPosUtils.BuzzerPage_Caption_Takeway = ReadString("buzzerpage_caption_takeway");
            PRPosUtils.BuzzerPage_Message_Takeway = ReadString("buzzerpage_message_takeway").Equals("") ? "Please Input Buzzer Number" : ReadString("buzzerpage_message_takeway");

            PRPosUtils.CoverPage_Caption = ReadString("coverpage_caption").Equals("") ? "NUMBER OF PERSONS DINING" : ReadString("coverpage_caption");
            PRPosUtils.CoverPage_Message = ReadString("coverpage_message").Equals("") ? "Please Input How Many Persons" : ReadString("coverpage_message");

            PRPosUtils.MemberCardPage_Caption = ReadString("memberpage_caption").Equals("") ? "SCAN YOUR MEMBER ID TO EARN POINTS" : ReadString("memberpage_caption");
            PRPosUtils.MemberCardPage_Message = ReadString("memberpage_message").Equals("") ? @"Skip if you are not a member yet." : ReadString("memberpage_message");

            PRPosUtils.OrderType_Caption = ReadString("ordertype_caption").Equals("") ? "" : ReadString("ordertype_caption");
            PRPosUtils.OrderType_Message = ReadString("ordertype_message").Equals("") ? @"" : ReadString("ordertype_message");

            



            //    PRPosUtils.CMP_NO = ReadString("cmp_no");
            //    PRPosUtils.STR_NO = ReadString("str_no");
            //    PRPosUtils.POS_NO = ReadString("pos_no");
            /*
                    string localCulture = ReadString("local_culture");
                    if (localCulture == "")
                        localCulture = "en-AU";*/
            PRPosUtils.LocalCulture = CultureInfo.CurrentCulture;
            //new System.Globalization.CultureInfo(localCulture, false);
            /*
        PRPosUtils.DateSeparator = ReadString("DateSeparator").Equals("") ?"-": ReadString("DateSeparator");
        PRPosUtils.CurrencySymbol = ReadString("CurrencySymbol").Equals("") ? PRPosUtils.LocalCulture.NumberFormat.CurrencySymbol : ReadString("CurrencySymbol");
        PRPosUtils.TimeFormat = ReadString("timeFormat").Equals("") ? PRPosUtils.LocalCulture.DateTimeFormat.LongTimePattern : ReadString("timeFormat");
        PRPosUtils.DateFormat = ReadString("dateFormat").Equals("") ? PRPosUtils.LocalCulture.DateTimeFormat.ShortDatePattern : ReadString("dateFormat");


        PRPosUtils.DateFormat = PRPosUtils.DateFormat.Replace(PRPosUtils.LocalCulture.DateTimeFormat.DateSeparator, PRPosUtils.DateSeparator);

        PRPosUtils.LocalCulture.DateTimeFormat.ShortDatePattern = PRPosUtils.DateFormat;
        PRPosUtils.LocalCulture.DateTimeFormat.LongTimePattern = PRPosUtils.TimeFormat;
 */
            PRPosUtils.DateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            PRPosUtils.TimeFormat = "HH:mm:ss";// CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            PRPosUtils.CurrencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            PRPosUtils.DateSeparator = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

           

            PRPosUtils.Receipt_head_Image = ReadString("receipt_header_image");
            PRPosUtils.Order_head_Image = ReadString("order_header_image");

            PRPosUtils.ModifierButtonBorder = ReadInteger("modifier", "button_border", "i1");
            PRPosUtils.ModifierButtonPadding = ReadInteger("modifier", "button_padding", "i1");
            PRPosUtils.ModifierButtonMargin = ReadInteger("modifier", "button_margin", "i1");
            PRPosUtils.ModifierButtonHieght = ReadInteger("modifier", "button_height", "i1");
            PRPosUtils.ModifierButtonWidth = ReadInteger("modifier", "button_width", "i1");
            PRPosUtils.ModifierButtonImage = ReadString("mod_button");

            PRPosUtils.ModifierButtonPerRow = (int)1080 / PRPosUtils.ModifierButtonWidth;

            PRPosUtils.Markup_item = ReadString("Markup_item");
            PRPosUtils.Plasticbag_item = ReadString("Plasticbag_item");

            PRPosUtils.Markup_Message = ReadString("Markup_message");
            PRPosUtils.Plasticbag_Message = ReadString("Bag_message");
            PRPosUtils.PayCashTenderCode = ReadString("Paycount_tender");
            PRPosUtils.PayEFTTenderCode = ReadString("Default_tender");

            PRPosUtils.ItemHeight = ReadInteger("item_button", "height", "i1");
            PRPosUtils.ItemWidth = ReadInteger("item_button", "width", "i1");
            PRPosUtils.ItemPadding = ReadInteger("item_button", "padding", "i1");
            PRPosUtils.ItemPerRow = ReadInteger("item_button", "item_perrow", "i1");
            PRPosUtils.ItemRowPadding = ReadInteger("item_button", "rowpadding", "i1");

            PRPosUtils.CommboItemHeight = ReadInteger("comboitem_button", "height", "i1");
            PRPosUtils.CommboItemWidth = ReadInteger("comboitem_button", "width", "i1");
            PRPosUtils.CommboItemPadding = ReadInteger("comboitem_button", "padding", "i1");

            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = @"select * from station  ";
                cmd.Parameters.Clear();
                DataTable stationDT = new DataTable();
                da.Fill(stationDT);
                if (stationDT.Rows.Count > 0)
                {
                    DataRow station = stationDT.Rows[0];
                    PRPosUtils.CustomerID = station["customerid"].ToString();
                    PRPosUtils.StoreCode = station["store_code"].ToString();
                    PRPosUtils.PosCode = station["pos_code"].ToString();
                }
                if (!PRPosUtils.CustomerID.Equals(""))
                    InitFinicalDate();
                cmd.CommandText = @"select * from pssystem where code='modifier' and  code_type='default_font' ";
                cmd.Parameters.Clear();
                DataTable pssystemDT = new DataTable();
                da.Fill(pssystemDT);
                foreach (DataRow row in pssystemDT.Rows)
                {
                    PRPosUtils.ModifierFontName = (row["f1"].ToString().Equals("") ? "Arial" : row["f1"].ToString());
                    PRPosUtils.ModifierFontColor = row["f3"].ToString().Equals("") ? "#FFFFFF" : row["f3"].ToString();
                    PRPosUtils.ModifierCaptionDisplay = row["f2"].ToString().Equals("") ? "Y" : row["f2"].ToString();
                    int fontsize = 0;
                    if (!int.TryParse(row["i1"].ToString(), out fontsize))
                    {
                        fontsize = 20;
                    }
                    PRPosUtils.ModifierFontSize = fontsize;
                    int fontsytle = 0;
                    if (!int.TryParse(row["i2"].ToString(), out fontsytle))
                    {
                        fontsytle = 1;
                    }
                    PRPosUtils.ModifierFontStyle = fontsytle;
                }

                cmd.CommandText = @"select * from pssystem where code='modifier' and  code_type='price_font' ";
                cmd.Parameters.Clear();
                pssystemDT = new DataTable();
                da.Fill(pssystemDT);
                foreach (DataRow row in pssystemDT.Rows)
                {
                    PRPosUtils.ModifierPriceFontName = (row["f1"].ToString().Equals("") ? "Arial" : row["f1"].ToString());
                    PRPosUtils.ModifierPriceFontColor = row["f3"].ToString().Equals("") ? "#FFFFFF" : row["f3"].ToString();
                    PRPosUtils.ModifierPriceDisplay = row["f2"].ToString().Equals("") ? "Y" : row["f2"].ToString();
                    int fontsize = 0;
                    if (!int.TryParse(row["i1"].ToString(), out fontsize))
                    {
                        fontsize = 20;
                    }
                    PRPosUtils.ModifierPriceFontSize = fontsize;
                    int fontsytle = 0;
                    if (!int.TryParse(row["i2"].ToString(), out fontsytle))
                    {
                        fontsytle = 1;
                    }
                    PRPosUtils.ModifierPriceFontStyle = fontsytle;

                }

            }
            // CultureInfo.CurrentCulture.NumberFormat             
        }

        public static void ReadParameterPart2()
        {
            string defaultprinter = ReadString("default_printer");
            PRPosUtils.DefaultPrinterName = ReadString(defaultprinter);

            
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = @"select * from PosPrinter where  customerid=@customerid and store_code=@store_code  and del_flag='N'  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);

                DataTable PrinterDT = new DataTable();
                da.Fill(PrinterDT);
                PRPosUtils.PosPrinters = new List<Models.PosPrinter>();
                foreach (DataRow row in PrinterDT.Rows)
                {
                    var print = new Models.PosPrinter();

                    print.PrinterName = row["kitchen_printer"].ToString();
                    print.DeviceName = row["device_name"].ToString();
                    print.DeviceType = row["device_type"].ToString();
                    print.Port = row["port"].ToString();
                    PRPosUtils.PosPrinters.Add(print);

                }

                
                cmd.CommandText = @"select * from tender where  customerid=@customerid and store_code=@store_code  and del_flag='N' order by disp_order  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);

                DataTable tenderDT = new DataTable();
                da.Fill(tenderDT);
                PRPosUtils.Tenders = new System.Collections.ObjectModel.ObservableCollection<Models.Tender>(); 
                foreach (DataRow row in tenderDT.Rows)
                {
                    var t = new Models.Tender();
                    t.Customerid = row["customerid"].ToString();
                    t.Store_code = row["store_code"].ToString();
                    t.Tender_code = row["tender_code"].ToString();
                    t.Tender_name = row["tender_name"].ToString();
                    t.Eftpos_flag = row["eftpos_flag"].ToString();
                    t.Display_name = row["display_name"].ToString();
                    t.Over_flag = row["over_flag"].ToString();
                    decimal d = 0;
                    t.Over_max = 0;
                    if (decimal.TryParse(row["over_max"].ToString(), out d))
                        t.Over_max = d;
                    t.Paymachine_flag= row["paymachine_flag"].ToString();
                    t.Received_flag = row["received_flag"].ToString();
                    t.Disp_flag = row["disp_flag"].ToString();
                    t.Card_charge_item = row["card_charge_item"].ToString();
                    t.Card_charge_rate = 0;
                    if (decimal.TryParse(row["card_charge_rate"].ToString(), out d))
                        t.Card_charge_rate = d;
                    t.Card_charge_flag = row["card_charge_flag"].ToString();
                    t.Print_at_kitchen = row["print_at_kitchen"].ToString();
                    PRPosUtils.Tenders.Add(t);
                }
            }
            //if(PRPosUtils.ReceiptPrinterName =="")
            //    PRPosUtils.ReceiptPrinterName = ReadString("receipt_printer");
            foreach (var p in PRPosUtils.PosPrinters)
            {
                p.IsDefault =  p.PrinterName == ReadString("receipt_printer");
            }
            PRPosUtils.ReceiptFooterName = ReadString("recepit_foot_parameter");
            PRPosUtils.ReceiptFooterLines = ReadInteger("recepit_foot_parameter");
            PRPosUtils.ReceiptFooter = new string[PRPosUtils.ReceiptFooterLines];
            for (int i = 0; i < PRPosUtils.ReceiptFooterLines; i++)
            {
                PRPosUtils.ReceiptFooter[i] = ReadString(PRPosUtils.ReceiptFooterName + (i + 1).ToString());
            }
            PRPosUtils.KitchenOrderHeaderFontSize = ReadInteger("kitchen_order_header", "font", "i1", 20);
            PRPosUtils.KitchenOrderHeaderFontFamily = ReadString("kitchen_order_header", "font", "f2", "Consolas");

            PRPosUtils.KitchenOrderItemFontSize = ReadInteger("kitchen_order", "item", "i1", 16);
            PRPosUtils.KitchenOrderItemName = ReadString("kitchen_order","item","f1" , "kitchen_name");
            PRPosUtils.KitchenOrderItemFontFamily = ReadString("kitchen_order", "item", "f2", "Consolas");
            PRPosUtils.KitchenOrderItemModifierName = ReadString("kitchen_order", "item", "f3", "caption");



            PRPosUtils.KitchenOrderHeaderLines = ReadInteger("kitchen_order_header_lines") ;
            PRPosUtils.KitchenOrderHeaderName = ReadString("kitchen_order_header_lines");
            PRPosUtils.KitchenOrderHeader = new string[PRPosUtils.KitchenOrderHeaderLines];
            for (int i = 0; i < PRPosUtils.KitchenOrderHeaderLines; i++)
            {
                PRPosUtils.KitchenOrderHeader[i] = ReadString(PRPosUtils.KitchenOrderHeaderName + (i + 1).ToString());
            }


            PRPosUtils.ReceiptItemItemFontFamily = ReadString("receipt", "item_font", "f1", "Consolas");
            PRPosUtils.ReceiptItemFontSize = ReadInteger("receipt", "item_font", "i1", 12);

            PRPosUtils.ReceiptCharacters = ReadInteger("recipt", "characters", "i1", 44);

            PRPosUtils.ReceiptItemWidth = ReadInteger("receipt", "item", "i1", 36);

            PRPosUtils.ReceiptItemQtyCharacters = ReadInteger("receipt", "item_qty", "i1", 3);

            PRPosUtils.ReceiptPriceCharacters = ReadInteger("receipt", "item_price", "i1", 4);
            PRPosUtils.ReceiptPriceDigitals = ReadInteger("receipt", "item_price", "i2", 2);
            

            PRPosUtils.ReceiptDWCharacters = ReadInteger("recipt_dw_characters");

            PRPosUtils.PaymentFormParameter = ReadString("payment_form_message_parameter");
            PRPosUtils.PaymentFormMessageLines = ReadInteger("payment_form_message_parameter");
            PRPosUtils.PaymentFormMessage = new string[PRPosUtils.PaymentFormMessageLines];
            for (int i = 0; i < PRPosUtils.PaymentFormMessageLines; i++)
            {
                PRPosUtils.PaymentFormMessage[i] = ReadString(PRPosUtils.PaymentFormParameter + (i + 1).ToString());
            }

            PRPosUtils.FinalFormParameter = ReadString("final_form_message_parameter");
            PRPosUtils.FinalFormMessageLines = ReadInteger("final_form_message_parameter");
            PRPosUtils.FinalFormMessage = new string[PRPosUtils.FinalFormMessageLines];
            for (int i = 0; i < PRPosUtils.FinalFormMessageLines; i++)
            {
                PRPosUtils.FinalFormMessage[i] = ReadString(PRPosUtils.FinalFormParameter + (i + 1).ToString());
            }

            PRPosUtils.FinalPayCountParameter = ReadString("final_message_pay_count");
            PRPosUtils.FinalPayCountMessageLines = ReadInteger("final_message_pay_count");
            PRPosUtils.FinalPayCountMessage = new string[PRPosUtils.FinalPayCountMessageLines];
            for (int i = 0; i < PRPosUtils.FinalPayCountMessageLines; i++)
            {
                PRPosUtils.FinalPayCountMessage[i] = ReadString(PRPosUtils.FinalPayCountParameter + (i + 1).ToString());
            }


        }
        public static void InitFinicalDate()
        {
            PRPosUtils.AccDate = DateTime.Today;
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                if (!PRPosUtils.CustomerID.Equals(""))
                {
                    cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc,ban desc";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);


                    DataTable psctrl01DT = new DataTable();
                    da.Fill(psctrl01DT);
                    if (psctrl01DT.Rows.Count > 0)
                    {
                        DataRow row = psctrl01DT.Rows[0];
                        int.TryParse(row["ban"].ToString(), out PRPosUtils.BAN);

                        DateTime date;
                        if (DateTime.TryParse(row["tdate"].ToString(), out date))
                        {
                            PRPosUtils.AccDate = date;
                        }
                    }
                    else
                    {
                        PRPosUtils.AccDate = DateTime.Today;

                        cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and tdate=@tdate order by ban desc, seq desc";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                        cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                        psctrl01DT = new DataTable();
                        da.Fill(psctrl01DT);
                        int seq = 1;
                        PRPosUtils.BAN = 1;
                        if (psctrl01DT.Rows.Count > 0)
                        {
                            DataRow row = psctrl01DT.Rows[0];
                            if (int.TryParse(row["ban"].ToString(), out PRPosUtils.BAN))
                            {
                                PRPosUtils.BAN += 1;
                            }
                            int.TryParse(row["seq"].ToString(), out seq);
                        }

                        cmd.CommandText = "insert into  psctrl01( cmp_no,str_no,pos_no,tdate,ban,seq,close_yn) values ( @cmp_no,@str_no,@pos_no,@tdate,@ban,@seq,@close_yn)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                        cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                        cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                        cmd.Parameters.AddWithValue("seq", seq);
                        cmd.Parameters.AddWithValue("close_yn", "N");
                        cmd.ExecuteNonQuery();

                    }
                }
            }
        }
        public static DataTable getAllPrinter(string device_type)
        {
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = "select * from PosPrinter where customerid=@customerid  and store_code=@store_code and pos_code=@pos_code and del_flag='N' and device_type=@device_type   ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("pos_code", PRPosUtils.PosCode);
                cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("device_type", device_type);

                DataTable PosPrinterDT = new DataTable();
                da.Fill(PosPrinterDT);

                return PosPrinterDT;
            }
        }

        public static DataRow getPrinter(string device_type, string printer)
        {
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = "select * from PosPrinter where customerid=@customerid and store_code=@store_code and pos_code=@pos_code and del_flag='N' and device_type=@device_type  and kitchen_printer=@kitchen_printer ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("pos_code", PRPosUtils.PosCode);
                cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("device_type", device_type);
                cmd.Parameters.AddWithValue("kitchen_printer", printer);
                DataTable PosPrinterDT = new DataTable();
                da.Fill(PosPrinterDT);
                DataRow R = null;
                if (PosPrinterDT.Rows.Count > 0)
                    R = PosPrinterDT.Rows[0];
                return R;
            }
        }
        public static int getDealNo()
        {
            int seq = 0;
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc,ban desc";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                DataTable psctrl01DT = new DataTable();
                da.Fill(psctrl01DT);
                //
                //
                if (psctrl01DT.Rows.Count > 0)
                {
                    DataRow row = psctrl01DT.Rows[0];
                    int.TryParse(row["ban"].ToString(), out PRPosUtils.BAN);
                    int.TryParse(row["seq"].ToString(), out seq);
                    DateTime date;
                    if (DateTime.TryParse(row["tdate"].ToString(), out date))
                    {
                        PRPosUtils.AccDate = date;
                    }
                    bool F = true;
                    do
                    {
                        cmd.CommandText = "select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no  and pos_no=@pos_no and  accdate=@accdate and deal_no=@deal_no  ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                        cmd.Parameters.AddWithValue("accdate", PRPosUtils.AccDate);
                        cmd.Parameters.AddWithValue("deal_no", seq.ToString("000"));
                        DataTable pstrn01sDT = new DataTable();
                        da.Fill(pstrn01sDT);
                        if (pstrn01sDT.Rows.Count > 0)
                        {
                            seq = seq + 1;
                        }
                        else
                        {
                            F = false;
                        }
                    } while (F);
                }
                else
                {
                    PRPosUtils.AccDate = DateTime.Today;

                    cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and tdate=@tdate order by ban desc, seq desc";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                    psctrl01DT = new DataTable();
                    da.Fill(psctrl01DT);
                    seq = 1;
                    PRPosUtils.BAN = 1;
                    if (psctrl01DT.Rows.Count > 0)
                    {
                        DataRow row = psctrl01DT.Rows[0];
                        if (int.TryParse(row["ban"].ToString(), out PRPosUtils.BAN))
                        {
                            PRPosUtils.BAN += 1;
                        }
                        int.TryParse(row["seq"].ToString(), out seq);
                    }
                    cmd.CommandText = "insert into  psctrl01( cmp_no,str_no,pos_no,tdate,ban,seq,close_yn) values ( @cmp_no,@str_no,@pos_no,@tdate,@ban,@seq,@close_yn)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                    cmd.Parameters.AddWithValue("seq", seq);
                    cmd.Parameters.AddWithValue("close_yn", "N");
                    cmd.ExecuteNonQuery();
                }
            }
            return seq;
        }
        public static void addDealNo(string dealno)
        {
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc,ban desc";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                DataTable psctrl01DT = new DataTable();
                da.Fill(psctrl01DT);
                if (psctrl01DT.Rows.Count > 0)
                {
                    DataRow row = psctrl01DT.Rows[0];
                    int seq1 = 0;
                    int.TryParse(row["seq"].ToString(), out seq1);
                    seq1 += 1;

                    int seq2 = 0;
                    int.TryParse(dealno, out seq2);
                    seq2 += 1;

                    cmd.CommandText = "update psctrl01 set seq=@seq where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and tdate=@tdate and ban=@ban";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                    cmd.Parameters.AddWithValue("seq", (seq1 > seq2 ? seq1 : seq2));
                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    int seq2 = 0;
                    int.TryParse(dealno, out seq2);
                    seq2 += 1;

                    cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and tdate=@tdate order by ban desc, seq desc";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                    psctrl01DT = new DataTable();
                    da.Fill(psctrl01DT);
                    if (psctrl01DT.Rows.Count > 0)
                    {
                        DataRow row = psctrl01DT.Rows[0];
                        if (int.TryParse(row["ban"].ToString(), out PRPosUtils.BAN))
                        {
                            PRPosUtils.BAN += 1;
                        }
                    }

                    cmd.CommandText = "insert into  psctrl01( cmp_no,str_no,pos_no,tdate,ban,seq,close_yn) values ( @cmp_no,@str_no,@pos_no,@tdate,@ban,@seq,@close_yn)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", PRPosUtils.AccDate);
                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                    cmd.Parameters.AddWithValue("seq", seq2);
                    cmd.Parameters.AddWithValue("close_yn", "N");
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void Create_Database()
        {
            SQLiteConnection.CreateFile(Path.Combine(PRPosUtils.App_root, PRPosDB.dbPath));
            using (SQLiteConnection connection = new SQLiteConnection(cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = "CREATE TABLE tender ( sid int, customerid varchar(20) NOT NULL, store_code nvarchar(10) NOT NULL, tender_code nvarchar(10) NOT NULL, tender_name nvarchar(50) NOT NULL, display_name nvarchar(50) NOT NULL, over_flag nvarchar(1), over_max numeric(6, 2), eftpos_flag nvarchar(1), paymachine_flag nvarchar(1), received_flag nvarchar(1), change_flag nvarchar(1), disp_flag nvarchar(1), disp_order int, del_flag nvarchar(1), crt_no nvarchar(30), crt_date datetime, upd_no nvarchar(30), upd_date datetime, card_charge_item nvarchar(10), card_charge_rate numeric(6, 2), card_charge_flag nvarchar(1), print_at_kitchen nvarchar(1), PRIMARY KEY(sid) ) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE taxsystem ( code varchar(10), name varchar(50), tax_name varchar(20), po_taxname varchar(50), sale_taxname varchar(50), invoice_name varchar(50), return_name varchar(50), receipt_msg1 varchar(50), receipt_msg2 varchar(50), receipt_msg3 varchar(50), vart_value number(8,2), upd_date datetime )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE tax ( code varchar(10), name varchar(50), tax_rate number(6,2), upd_date datetime )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE station ( sid INTEGER, customerid INTEGER, pos_code VARCHAR(50), expiry_date datetime, last_checked datetime, mac_address VARCHAR(50), registed_date datetime, screens INTEGER, connectioncode VARCHAR(50), token VARCHAR(50), stationid INTEGER, accesscode VARCHAR(50), vcode VARCHAR(28), pos_name VARCHAR(50), set_code VARCHAR(10), config_code VARCHAR(10), pricecolumn1 varchar(10), pricecolumn2 varchar(10), pricecolumn3 varchar(10), pricecolumn4 varchar(10), store_code varchar(20), PRIMARY KEY(sid) ) ";
                cmd.ExecuteNonQuery();                
                cmd.CommandText = "CREATE TABLE setcombo ( set_code varchar ( 20 ) NOT NULL, combo_code varchar ( 20 ) NOT NULL, combo_name varchar ( 50 ), combo_name_fn varchar ( 50 ), kitchen_memo varchar ( 200 ), max_selection INTEGER, min_selection INTEGER, seq INTEGER, PRIMARY KEY(set_code,combo_code) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE servicecharge( sid int NOT NULL, customerid int NOT NULL, store_code varchar(10) NOT NULL, charge_type varchar(1) NOT NULL, charge_name varchar(50) NOT NULL, bdate datetime NULL, edate datetime NULL, charge_flag varchar(1) NOT NULL, variety_code varchar(20) NULL, charge_item varchar(20) NULL, charge_rate numeric(6, 2) NULL, del_flag varchar(1) NULL, upd_date datetime NULL, primary key (sid) ) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pstrn04s ( seq INTEGER, cmp_no varchar(16), str_no varchar(16), pos_no varchar(16), accdate DateTime, deal_no varchar(16), item_no int, variety_code varchar(20), item_code varchar(20), modifier_code varchar(20), modset_code varchar(20), caption varchar(50), caption_fn varchar(50), qty int,inpqty int, sprice numeric(6, 2), amount numeric(6, 2), PRIMARY KEY(seq) ) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pstrn03s ( cmp_no varchar ( 16 ) NOT NULL, str_no varchar ( 16 ) NOT NULL, pos_no varchar ( 16 ) NOT NULL, accdate DatetTme NOT NULL, deal_no varchar ( 16 ) NOT NULL, item_no int NOT NULL, dc_code varchar ( 1 ), ecp_type varchar ( 4 ), ecp_amt numeric ( 8 , 2 ), change_amt numeric ( 8 , 2 ), ecp_code varchar ( 20 ), memo varchar ( 80 ), ecp_name varchar ( 40 ), ref_code1 varchar ( 20 ), ref_code2 varchar ( 20 ), CONSTRAINT pstrn03s_pkey PRIMARY KEY(str_no,pos_no,deal_no,item_no,cmp_no,accdate) ) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pstrn02s ( cmp_no varchar(16) NOT NULL, str_no varchar(16) NOT NULL, pos_no varchar(16) NOT NULL, accdate Datetime NOT NULL, deal_no varchar(16) NOT NULL, item_no int NOT NULL, item_code varchar(16), size_code varchar(10), item_type varchar(1), gst number(6, 2), goo_price number(8, 2), sprice number(8, 2), qty int, tax_amt number(8, 2), mis_amt number(8, 2), dis_amt number(8, 2), dis_rate number(8, 2), amt number(8, 2), ht_price number(8, 2), ht_amt number(8, 2), mms_mis number(8, 2), mms_no varchar(10), gid varchar(20), item_name varchar(50), kitchen_memo varchar(100), kitchen_name varchar(100), printer_name varchar(50), size_caption varchar(50), variety_code varchar(20), modset_code varchar(20), combo_code varchar(20), comno_code varchar(20), variety_caption varchar(50), variety_kitchen_name varchar(50), PRIMARY KEY(cmp_no,str_no,pos_no,deal_no,item_no,accdate) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pstrn01s ( cmp_no varchar(16), str_no varchar(16), pos_no varchar(16), accdate datetime, deal_no varchar(16), tdate datetime, clerk_no varchar(12), deal_code varchar(1), dis_amt number(10, 2), min_amt number(10, 2), over_amt number(10, 2), mms_amt number(10, 2), tot_amt number(10, 2), tax_amt number(10, 2), ntax_amt number(10, 2), ztax_amt number(10, 2), ht_amt number(10, 2), card_no varchar(20), buss_no varchar(12), uld_yn varchar(1), crt_no varchar(12), crt_date datetime, close_yn varchar(1), ref_no varchar(20), cnl_no varchar(12), cnl_time datetime, service_no varchar(12), opener_no varchar(12), order_type varchar(1), person INTEGER, opentime datetime, sendtime datetime, closetime datetime, upl_yn varchar(1), org_deal_no varchar(20), del_deal_no varchar(20), PRIMARY KEY(cmp_no,str_no,pos_no,deal_no,accdate) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pssystem ( code varchar(30), code_type varchar(12), f1 varchar(200), f2 varchar(200), f3 TEXT, n1 number(12 , 2), n2 number(12 , 2), i1 integer, i2 INTEGER, upd_date datetime, dn_flag varchar(1), ftime datetime, CONSTRAINT pk_pssystem PRIMARY KEY(code,code_type) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE psreceis ( ecp_type varchar(10) NOT NULL, chi_name varchar(20), eng_name varchar(20), over_flg varchar(1), over_max numeric, taxed_yn varchar(1), amount_yn varchar(1), repeat_yn varchar(1), cardno_yn varchar(1), connect_yn varchar(1), cardnoprt_yn varchar(1), approv_yn varchar(1), approvprt_yn varchar(1), refund_yn varchar(1), clkprt_yn varchar(1), CONSTRAINT psreceis_pkey PRIMARY KEY (ecp_type) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE psmodsetti ( sid INTEGER, psid varchar(20), modifier_code varchar(20), caption varchar(50), caption_fn varchar(50), mod_type varchar(1), price_type varchar(1), amount numeric(8, 2), max_selection INTEGER, min_selection INTEGER, next_modset varchar(20), img_flag varchar(1), image varchar(60), ftime DateTime, upd_date DateTime, del_flag varchar(1), disp_caption varchar(1), disp_price varchar(1), fontcolor varchar(10), bgcolor varchar(10), fontfamily varchar(60), fontsize varchar(20), fontstyle varchar(10), soldout varchar(1), str_soldout varchar(1), str_upd_date DateTime,   PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE psmodset01 ( sid INTEGER NOT NULL, customerid varchar(20), modset_code varchar(20), caption nvarchar(50), caption_fn nvarchar(50), mod_type nvarchar(1), amount NUMERIC(6, 2), max_selection int DEFAULT 1, min_selection int DEFAULT 0, next_modset nvarchar(12), fontcolor varchar(10), bgcolor varchar(10), fontfamily varchar(60), fontsize varchar(20), fontstyle varchar(10), del_flag nvarchar(1), upd_date DateTime, PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE psitem ( customerid varchar(20) NOT NULL, item_code varchar(20) NOT NULL, item_type varchar(1), item_kind varchar(1), item_name varchar(60), item_name_fn varchar(50), description varchar(100), sprice numeric(12, 2), gst numeric(12, 2), cate_code varchar(20), mod_code varchar(20), set_code varchar(20), kitchen_name varchar(50), kitchen_name_f varchar(50), kitchen_remark varchar(100), dept varchar(20), buttonid int, sortorder int, p1 varchar(1) DEFAULT 'N', p2 varchar(1) DEFAULT 'N', p3 varchar(1) DEFAULT 'N', p4 varchar(1) DEFAULT 'N', p5 varchar(1) DEFAULT 'N', p6 varchar(1) DEFAULT 'N', m1 varchar(1) DEFAULT 'N', m2 varchar(1) DEFAULT 'N', m3 varchar(1) DEFAULT 'N', s1 varchar(1) DEFAULT 'N', s2 varchar(1) DEFAULT 'N', s3 varchar(1) DEFAULT 'N', s4 varchar(1) DEFAULT 'N', s5 varchar(1) DEFAULT 'N', s6 varchar(1) DEFAULT 'N', s7 varchar(1) DEFAULT 'N', s8 varchar(1) DEFAULT 'N', s9 varchar(1) DEFAULT 'N', spicy varchar(1) DEFAULT 'N', vegetarian varchar(1) DEFAULT 'N', beef varchar(1) DEFAULT 'N', pork varchar(1) DEFAULT 'N', basic_item varchar(1) DEFAULT 'N', size_code varchar(20), modisetid varchar(20), image varchar(60), rest_usr varchar(1) DEFAULT 'N', crt_date datetime, crt_no varchar(30), upd_date datetime, upd_no varchar(30), printer_name varchar(50), kds_name varchar(50), del_flag nvarchar(1), img_flag varchar(1), ftime datetime, soldout varchar(1), sprice2 numeric(12, 2), sprice3 numeric(12, 2), sprice4 numeric(12, 2), sprice5 numeric(12, 2), sprice6 numeric(12, 2), sprice7 numeric(12, 2), sprice8 numeric(12, 2), sprice9 numeric(12, 2), sprice10 numeric(12, 2),str_soldout varchar(1), str_upd_date DateTime , CONSTRAINT PK_psitem PRIMARY KEY(customerid,item_code) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE psextrati ( sid INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, extra_code VARCHAR ( 20 ), item_code VARCHAR ( 20 ), extra_type VARCHAR ( 1 ), price_type VARCHAR ( 1 ), amount NUMERIC ( 8 , 2 ), image VARCHAR ( 50), upd_date VARCHAR ( 20 ) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE pscategory ( sid integer, customerid INTEGER, cate_code VARCHAR(50), cate_type VARCHAR(1), cate_name VARCHAR(50), cate_fn_name VARCHAR(50), seq INTEGER, buttonid INTEGER, publish VARCHAR(1), pcate_code INTEGER, pictures varchar(50), upd_date datetime, del_flag varchar(1), CONSTRAINT pk_pscategory PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE psctrl01 ( cmp_no varchar(16) NOT NULL, str_no varchar(16) NOT NULL, pos_no varchar(16) NOT NULL, tdate datetime, ban int not null DEFAULT 1, seq int not null DEFAULT 1, close_yn varchar(1), CONSTRAINT psctrl01_pk PRIMARY KEY(cmp_no,str_no,pos_no,tdate,ban) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE promotions ( sid INTEGER, customerid varchar(20), store_code varchar(20), item_code varchar(20), size_code varchar(10), variety_code varchar(20), bdate datetime, edate datetime, caption varchar(50), sprice NUMERIC(8, 2), w1 varchar(1), w2 varchar(1), w3 varchar(1), w4 varchar(1), w5 varchar(1), w6 varchar(1), w7 varchar(1), daily varchar(1), btime varchar(4), etime varchar(4), upd_date datetime, del_flag varchar(1), sprice2 numeric(12, 2), sprice3 numeric(12, 2), sprice4 numeric(12, 2), sprice5 numeric(12, 2), sprice6 numeric(12, 2), sprice7 numeric(12, 2), sprice8 numeric(12, 2), sprice9 numeric(12, 2), sprice10 numeric(12, 2), PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE posfastkeyset ( sid INTEGER NOT NULL, customerid varchar(20), store_code varchar(20), location_code varchar(20), set_code varchar(20), set_name varchar(50), del_flag varchar(1), upd_date datetime, PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE posfastkey02 ( sid integer NOT NULL, customerid varchar(20), store_code varchar(20), set_code varchar(20), psid integer, caption varchar(60), op_code varchar(1), ref_code varchar(20), width integer, height integer, display_yn varchar(1), default_yn varchar(1), caption_yn varchar(1), fontcolor varchar(10), bgcolor varchar(10), fontfamily varchar(60), fontsize varchar(20), fontstyle varchar(10), imagefile varchar(60), fullimage_yn varchar(1), textheight INTEGER, textbgcolor varchar(10), disp_order integer, del_flag varchar(1), upd_date datetime, img_flag varchar(1), ftime datetime, caption2 varchar(60), caption3 varchar(60), priceline varchar(1), caption2_yn varchar(1), caption3_yn varchar(1), PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE modifier ( sid INTEGER NOT NULL, customerid varchar(20), modifier_code varchar(20), caption varchar(40), caption_fn varchar(40), image varchar(60), price_type varchar(1), amount numeric(8, 2), description INTEGER, upd_date varchar(20), img_flag varchar(1), del_flag varchar(1), ftime datetime, disp_caption varchar(1), disp_price varchar(1),  PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE mealset_course_item( sid int NOT NULL, psid int NULL, variety_code varchar(30) NULL, item_code varchar(30) NULL, sprice numeric(8, 2) NULL, sprice2 numeric(12, 2) NULL, sprice3 numeric(12, 2) NULL, sprice4 numeric(12, 2) NULL, sprice5 numeric(12, 2) NULL, sprice6 numeric(12, 2) NULL, sprice7 numeric(12, 2) NULL, sprice8 numeric(12, 2) NULL, sprice9 numeric(12, 2) NULL, sprice10 numeric(12, 2) NULL, is_enabled varchar(1) NULL, del_flag varchar(1) NULL, upd_date datetime null, upd_no varchar(50) NULL, disp_order int, primary key (sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE mealset_course( sid int NOT NULL, psid int NULL, course_name varchar(50) NULL, course_name_fn varchar(50) NULL, sprice numeric(8, 2) NOT NULL, sprice2 numeric(12, 2) NULL, sprice3 numeric(12, 2) NULL, sprice4 numeric(12, 2) NULL, sprice5 numeric(12, 2) NULL, sprice6 numeric(12, 2) NULL, sprice7 numeric(12, 2) NULL, sprice8 numeric(12, 2) NULL, sprice9 numeric(12, 2) NULL, sprice10 numeric(12, 2) NULL, max_selection int NULL, min_selection int NULL, upd_date datetime NULL, upd_no varchar(50) NULL, is_enabled varchar(1) NULL, del_flag varchar(1) NULL, disp_order int, primary key(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE mealset ( sid int NOT NULL, customerid varchar(20), store_code varchar(20), mealset_code varchar(30), caption varchar(50), caption_fn varchar(50), sprice numeric(8, 2), sprice2 numeric(12, 2), sprice3 numeric(12, 2), sprice4 numeric(12, 2), sprice5 numeric(12, 2), sprice6 numeric(12, 2), sprice7 numeric(12, 2), sprice8 numeric(12, 2), sprice9 numeric(12, 2), sprice10 numeric(12, 2), gst numeric(6, 2), actived varchar(1), del_flag varchar(1), imagefile varchar(100), org_filename varchar(50), description varchar(500), kitchen_name varchar(100), kitchen_name_fn varchar(100), print_on_kitchen varchar(1), upd_date datetime, upd_no varchar(50), img_flag varchar(1), ftime datetime, PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE markupitem( sid int, customerid int, store_code varchar(10), markup_type varchar(1), item_code varchar(20), variety_code varchar(20), message varchar(200), del_flag varchar(1), disp_order int, sprice numeric(6, 2), sprice2 numeric(6, 2), sprice3 numeric(6, 2), sprice4 numeric(6, 2), sprice5 numeric(6, 2), sprice6 numeric(6, 2), sprice7 numeric(6, 2), sprice8 numeric(6, 2), sprice9 numeric(6, 2), sprice10 numeric(6,2), upd_date datetime , primary key (sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE location ( code varchar(10), name varchar(50), mtype varchar(10), address varchar(100), suburb varchar(30), state varchar(10), postcode varchar(10), contact_name varchar(50), contact_phone varchar(20), contact_email varchar(50), contact_mobile varchar(20), tax_rule varchar(10), tax_no varchar(16), currency varchar(10), upd_date datetime )";
                cmd.ExecuteNonQuery();
 
                cmd.CommandText = "CREATE TABLE lastupdate ( stationid varchar(12) NOT NULL, datatable varchar(50) NOT NULL, local_update datetime, server_update datetime, PRIMARY KEY(stationid,datatable) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemvariety( sid int, customerid int, store_code varchar(20), item_code varchar(20), variety_code varchar(20), size_code varchar(5), cook_type varchar(5), caption varchar(50), caption_fn varchar(50), description varchar(500), del_flag varchar(1), disp_order int, next_modset varchar(50), sprice numeric(6, 2), sprice2 numeric(6, 2), sprice3 numeric(6, 2), sprice4 numeric(6, 2), sprice5 numeric(6, 2), sprice6 numeric(6, 2), sprice7 numeric(6, 2), sprice8 numeric(6, 2), sprice9 numeric(6, 2), sprice10 numeric(6,2), upd_date datetime , default_item varchar(1) , soldout varchar(1), kitchen_name varchar(50), kitchen_name_fn varchar(50), primary key (sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemsize ( sid INTEGER, customerid INTEGER, store_code varchar(20), item_code varchar(20), size_code varchar(10), caption varchar(50), caption_fn varchar(50), description varchar(50), image varchar(60), sprice NUMERIC(8 , 2), del_flag varchar(1), disp_order INTEGER, upd_date datetime, next_modset varchar(20), PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemmodifier ( sid INTEGER PRIMARY KEY, customerid INTEGER, store_code varchar(20), item_code varchar(20), variety_code varchar(20), modset_code nvarchar(50), disp_step INTEGER, disp_order INTEGER, upd_date datetime  )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemmealset02 ( sid INTEGER PRIMARY KEY AUTOINCREMENT, gcode VARCHAR ( 20 ), set_code VARCHAR ( 20 ), cnl_yn VARCHAR ( 1 ), seq INTEGER )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemmealset01 ( sid INTEGER PRIMARY KEY AUTOINCREMENT, item_code VARCHAR ( 20 ), gcode VARCHAR ( 20 ), caption VARCHAR ( 30 ), caption_fn VARCHAR ( 30 ), cnl_yn VARCHAR ( 1 ), width INTEGER DEFAULT 320, height INTEGER DEFAULT 300, seq INTEGER )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemextra ( sid INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, item_code VARCHAR ( 20 ) NOT NULL, extra_code VARCHAR ( 20 ) NOT NULL, seq INTEGER, cnl_yn varchar ( 1 ), upd_date varchar ( 20 ) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE itemcombo ( sid int NOT NULL, customerid varchar(20) NOT NULL, store_code varchar(20) NOT NULL, item_code varchar(20) NOT NULL, variety_code varchar(20) NOT NULL, mealset_code varchar(30) NOT NULL, sprice numeric(12, 2), sprice2 numeric(12, 2), sprice3 numeric(12, 2), sprice4 numeric(12, 2), sprice5 numeric(12, 2), sprice6 numeric(12, 2), sprice7 numeric(12, 2), sprice8 numeric(12, 2), sprice9 numeric(12, 2), sprice10 numeric(12, 2), gst numeric(6, 2), description varchar(100), disp_order int, del_flag nvarchar(1), upd_date datetime, PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                
                cmd.CommandText = "CREATE TABLE currency ( code varchar(10), name varchar(50), value number(6,2), picture varchar(50), upd_date datetime )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE PosPrinter ( sid INTEGER, customerid varchar(20), store_code varchar(10), pos_code varchar(10), device_type varchar(30), Config_type varchar(30), port varchar(30), buad varchar(10), dataformat varchar(30), handshake varchar(10), device_name varchar(120), kitchen_printer varchar(30), ipaddress varchar(50), del_flag varchar(1), PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE EFTPOSTransRef ( sid integer, lastReference VARCHAR(50), CONSTRAINT pk_EFTPOSTransRef PRIMARY KEY(sid) )";
                cmd.ExecuteNonQuery();
 


                cmd.CommandText = "INSERT INTO main.sqlite_sequence VALUES('tmpmodifier','1')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = 
                        @"INSERT INTO main.pssystem VALUES('default_fastkey','','1','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('max_qty','','','','','','','99','','','','');
                        INSERT INTO main.pssystem VALUES('max_amount','','','','','99999.99','','','','','','');
                        INSERT INTO main.pssystem VALUES('auto_logoff','','Y','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('generate_ticket_number','','N','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('reset_ticket_number','','1','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('ask_order_type','','N','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('ask_table_number','','0','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('ask_persson_number','','0','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('waiting_time','','','','','','','60','','','','');
                        INSERT INTO main.pssystem VALUES('output_file_path','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('picture_file_path','','images','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('menu_height','','','','','','','200','','','','');
                        INSERT INTO main.pssystem VALUES('banner_height','','','','','','','300','','','','');
                        INSERT INTO main.pssystem VALUES('blank_image','PageImage','bg_637490650423000882.jpg','','','','','','','2021-07-20 11:03:18','Y','2022-03-18 14:02:29');
                        INSERT INTO main.pssystem VALUES('start_page','PageImage','bg_637490650540190090.jpg','','','','','','','2021-07-20 11:03:18','Y','2022-03-18 14:02:31');
                        INSERT INTO main.pssystem VALUES('order_type_page','PageImage','bg_637490650676906990.jpg','','','','','','','2021-07-20 11:03:18','Y','2022-03-18 14:02:33');
                        INSERT INTO main.pssystem VALUES('ask_member_card','','0','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('menu_button_width','','','','','','','200','','','','');
                        INSERT INTO main.pssystem VALUES('banner_play_time','','','','','','','8','','','','');
                        INSERT INTO main.pssystem VALUES('connection_code','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('pos_code','','001','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('default_font_name','','Calibri','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('default_font_size','','','','','','','16','','','','');
                        INSERT INTO main.pssystem VALUES('default_font_color','','#FFFFFF','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('ask_dinein_number','','Y','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('banner_image_1','','Top-Banner.png','banner_image','Y','','','1','','','','');
                        INSERT INTO main.pssystem VALUES('banner_image_2','','top-banner-1.jpg','banner_image','Y','','','2','','','','');
                        INSERT INTO main.pssystem VALUES('item_plus_icon','','plus.png','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('item_minus_icon','','minus.png','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('alert_display_time','','','','','','','20','','','','');
                        INSERT INTO main.pssystem VALUES('buzzerpage_caption_dinein','','Enter Your Table Number','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('buzzerpage_caption_takeway','','Enter Your Buzzer Number','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('ask_buzzer_number','','Y','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('company_name','','Orange Tea Australia','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('buss_no','','ABN: 82 155 656 278','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('store_name','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('store_phone','','Phone: 0413 925 505','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('store_address_line1','','Shop K7/342 McCullough Street','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('store_address_line2','','Sunnybank, Brisbane, QLD 4109','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('store_address_line3','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('printer1','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('printer2','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('printer3','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('printer4','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('printer5','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('str_no','','1001','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('cmp_no','','P01','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('pos_no','','001','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('default_printer','','printer1','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_printer','','RP80 Printer','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('final_page','','Express Ordering Kiosk Interface - 2.2.png','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('recipt_title','','***TAX INVOICE***','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_foot_line1','','PLEASE RETAIN RECEIPT FOR PROOF','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_foot_line2','','OF PURCHASE.','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_foot_line3','','ALL PRODUCT ARE NOT REFUNDABLE','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_foot_line4','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('recepit_foot_parameter','','receipt_foot_line','','','','','4','','','','');
                        INSERT INTO main.pssystem VALUES('payment_form_message_parameter','','payment_message_line','','','','','2','','','','');
                        INSERT INTO main.pssystem VALUES('payment_message_line1','','YOUR ORDER WILL BE SEND TO THE KITCHEN,','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('payment_message_line2','','AFTER YOU''VE COMPLETED PAYMENT.','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('final_form_message_parameter','','final_message_line','','','','','2','','','','');
                        INSERT INTO main.pssystem VALUES('final_message_line1','','PLEASE TAKE YOUR RECEIPT,','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('final_message_line2','','AND MOVE TO THE COLLECTION POINT.','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_header_image','','POS-Republic-green-774.bmp','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('order_header_image','','POS-Republic-green-774.bmp','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('order_title','','==== Order Number ====','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('adminpassword','','20190101','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('local_culture','','en-AU','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('dateFormat','','dd/MM/yyyy','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('CurrencySymbol','','$','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('timeFormat','','HH:mm:ss','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('recipt_dw_characters','','','','','','','28','','','','');
                        INSERT INTO main.pssystem VALUES('recipt_characters','','','','','','','42','','','','');
                        INSERT INTO main.pssystem VALUES('receipt_price_width','','','','','','','8','','','','');
                        INSERT INTO main.pssystem VALUES('keep_local_file','','N','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Holiday_charge_item','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Holiday_charge','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Card_charge','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Card_charge_item','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Holiday_charge_rate','','','','','1.8','','','','','','');
                        INSERT INTO main.pssystem VALUES('Card_charge_rate','','','','','0.9','','','','','','');
                        INSERT INTO main.pssystem VALUES('Table_service_charge','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Table_service_charge_item','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Table_service_charge_rate','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Plasticbag_item','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Paycount_tender','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Default_tender','','EFTPOS','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','default_font','Arial','','#000000','','','20','1','','','');
                        INSERT INTO main.pssystem VALUES('modifier','button_margin','','','','','','3','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','button_padding','','','','','','2','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','button_border','','','','','','3','','','','');
                        INSERT INTO main.pssystem VALUES('item_button','width','','','','','','320','','','','');
                        INSERT INTO main.pssystem VALUES('item_button','height','','','','','','330','','','','');
                        INSERT INTO main.pssystem VALUES('item_button','padding','','','','','','20','','','','');
                        INSERT INTO main.pssystem VALUES('Markup_item','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Bag_message','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Markup_message','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('Spool_Folder','','','','','','','','','','','');
                        INSERT INTO main.pssystem VALUES('member_card_page','PageImage','bg_637490650759875770.jpg','','','','','','','2021-07-20 11:03:18','Y','2022-03-18 14:02:34');
                        INSERT INTO main.pssystem VALUES('table_number_page','PageImage','bg_637490650901126701.jpg','','','','','','','2021-07-20 11:03:18','Y','2022-03-18 14:02:36');
                        INSERT INTO main.pssystem VALUES('default_order_type','','','','','','','1','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','button_height','','','','','','160','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','button_width','','','','','','200','','','','');
                        INSERT INTO main.pssystem VALUES('modifier','price_font','Arial','Y','#000000','','','14','1','','','');
                        INSERT INTO main.pssystem VALUES('comboitem_button','height','','','','','','240','','','','');
                        INSERT INTO main.pssystem VALUES('comboitem_button','width','','','','','','240','','','','');
                        INSERT INTO main.pssystem VALUES('comboitem_button','padding','','','','','','10','','','','');
                        INSERT INTO main.pssystem VALUES('item_button','item_perrow','','','','','','3','','','','');
                        INSERT INTO main.pssystem VALUES('item_button','rowpadding','','','','','','10','','','','');
                        INSERT INTO main.pssystem VALUES('Specialist','banner_image','bg_637490651138157469.jpg','','N','','','1','10','2021-07-20 11:03:18','Y','2022-03-18 14:02:37');
                        INSERT INTO main.pssystem VALUES('Creamy','banner_image','bg_637490651255032422.jpg','','N','','','2','10','2021-07-20 11:03:18','Y','2022-03-18 14:02:38');
                        INSERT INTO main.pssystem VALUES('Yakult','banner_image','bg_637490651376438963.jpg','','N','','','3','10','2021-07-20 11:03:18','Y','2022-03-18 14:02:39');";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set ftime=null where ftime='' ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set upd_date=null where upd_date='' ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set i1=null where i1='' ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set i2=null where i2='' ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set n1=null where n1='' ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "update pssystem set n2=null where n2='' ";
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
