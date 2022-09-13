using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;

namespace SNSelfOrder
{
    
    public enum ADMediaTyle
    {
        IMAGE = 1,
        VIDEO = 2
    }
 
    public static class PRPosUtils
    {
        /*public static frm_LinkyPOS LinkyPOS = null;
        public delegate void SafeCallDelegateShowForm();
       
        public static void ShowLinkPos()
        {
            if (PRPosUtils.LinkyPOS.InvokeRequired)
            {
                var d = new SafeCallDelegateShowForm(ShowLinkPos);
                PRPosUtils.LinkyPOS.Invoke(d, new object[] { });
            }
            else
            {
                PRPosUtils.OpenForms.Push(PRPosUtils.LinkyPOS);
                writelog("PRPosUtils.ShowLinkPos:Show At" + PRPosUtils.LinkyPOS.EFTPosition());
                PRPosUtils.LinkyPOS.Show();
            }
        }
        public static Stack<Form> OpenForms = new Stack<Form>();*/
        public static int SCREENLEFT = 2560;
        public static string HostURL = "";
        public static int ScreenWidth = 1080;
        public static int ScreenHeight = 1920;
        public static int MenuButtonWidth = 200;
        public static int MenuHeight = 200;

        public static int ItemHeight = 350;
        public static int ItemWidth = 320;
        public static int ItemPadding = 10;
        public static int ItemPerRow = 3;
        public static int ItemRowPadding = 20;

        public static int CommboItemHeight = 350;
        public static int CommboItemWidth = 320;
        public static int CommboItemPadding = 10;
        public static int CommboItemPerRow = 3;

        public const int PR_CLOSE_FORM = -1;
        public const int PR_RESET_DEAL = 0;

        public const int RETURN_BY_OK = 0;
        public const int RETURN_BY_CANCLE = -1;
        public const int RETURN_BY_BACK = 1;

        public static int PR_POS_COUNTDOWN = -1;

        public static int PR_POS_TIME_TICK = 0;
        public static int PR_POS_RESET_TIME = 0;
        public static int PR_POS_STOP_TIMER = 0;
        public static int PR_POS_CLOSE_FORM = 0;
        public static int PR_POS_SHOWCART = 0;
        public static int PR_CART_MESSAGE = 0;
        public static int PR_PAYMENT_MESSAGE = 0;
        public static int PR_FINALE_MESSAGE = 0;

        public static int PR_ITEM_MODIFIER = 0;
        public static int PR_ITEM_MESSAGE = 0;

        public static IntPtr thisHandle = IntPtr.Zero;

        public static int BannerHeight = 300;
        public static int BannerPlayTime = 5;
        public static int DefaultFontSize = 12;
        public static int MaxQty = 99;
        public static int WaitingTime = 60;
        public static int AlterDisplayTime = 25;
        public static string DefaultFontName = "Calibri";
        public static string DefaultFontColor = "#000000";
        public static string ModifierFontName = "Arial";
        public static string ModifierFontColor = "#FFFFFF";
        public static int ModifierFontSize = 20;
        public static int ModifierFontStyle = 1;  // 0-regular 1 bold 2 Italic  4 Underline  8 Strikeout
        public static int ModifierButtonMargin = 1;
        public static int ModifierButtonPadding = 2;
        public static int ModifierButtonBorder = 3;
        public static int ModifierButtonHieght = 120;
        public static int ModifierButtonWidth = 300;
        public static int ModifierButtonPerRow = 4;

        public static string ModifierPriceFontName = "Arial";
        public static string ModifierPriceFontColor = "#FFFFFF";
        public static int ModifierPriceFontStyle = 1;  // 0-regular 1 bold 2 Italic  4 Underline  8 Strikeout
        public static int ModifierPriceFontSize = 14;
        public static string ModifierPriceDisplay = "Y";
        public static string ModifierCaptionDisplay = "Y";

        public static string ModifierButtonImage = "";

        public static string PosCode = "";
        public static string PosConnection = "";

        public static string Markup_item = "";
        public static string Plasticbag_item = "";
        public static string Markup_Message = "";
        public static string Plasticbag_Message = "";

        public static string Img_blank = "Blank Background.png";
        public static string Img_Start = "start.png";
        public static string Img_OrderType = "Express Ordering Kiosk Interface - 2.2.png";
        public static string Img_Buzzer = "Express Ordering Kiosk Interface - 2.3.png";
        public static string Img_DiningPerson = "";
        public static string Img_MemberCard = "Express Ordering Kiosk Interface - 2.3.png";
        public static string Img_Banner = "top-banner-1.jpg";
        public static string Img_Finally = "Express Ordering Kiosk Interface - 2.2.png";
        public static string Img_Payment = "Blank Background.png";

        public static string SalePriceColumn = "";
        public static string TakeawayPriceColumn = "";

        public static string PayCashTenderCode = "";
        public static string PayEFTTenderCode = "";

        public static string DefaultOrderType = ((int)OrderType.DINING).ToString();
        
        public static string Ask_OrderType = "Y";

        public static string OrderType_Caption = "";
        public static string OrderType_Message = "";

        public static string Ask_Table_Number = "1";
        public static string BuzzerPage_Caption_Dinein = "Enter Your Table Number";
        public static string BuzzerPage_Message_Dinein = "Please Input Table Number";
        public static string BuzzerPage_Caption_Takeway = "Enter Your Buzzer Number";
        public static string BuzzerPage_Message_Takeway = "Please Input Buzzer Number";

        public static string Ask_Covers = "1";
        public static string CoverPage_Caption = "Number of Persons Dining";
        public static string CoverPage_Message = "Please Input How Many Persons";
 
        // public static string Ask_Buzzer_Number = "1";
        public static string Ask_Member_Card = "1";
        public static string MemberCardPage_Caption= "SCAN YOUR MEMBER ID TO EARN POINTS";
        public static string MemberCardPage_Message = @"Please Input Member Card,\nskip if you are not a member yet.";

        

        public static string Default_FasktKey = "";
        public static string Image_Path = @"images";
        public static string Receipt_head_Image = @"";
        public static string Order_head_Image = @"";
        public static string MinusFile = "";
        public static string PlusFile = "";

        //  public static string CMP_NO = "";
        //  public static string STR_NO = "";
        //  public static string POS_NO = "";
        public static CultureInfo LocalCulture = CultureInfo.CurrentCulture;
        public static string DateSeparator = "/";
        public static string CurrencySymbol = "$";
        public static string App_root = "";
        public static string Output_Folder = @"output";
        public static string Spool_Folder = @"spool";

        public static string DateFormat = "dd/MM/yyyy";
        public static string TimeFormat = "HH:mm:ss";

        public static string DefaultPrinterName = "";
        //public static string ReceiptPrinterName = "";

        public static int ReceiptFooterLines = 4;
        public static int ReceiptDWCharacters = 24;
        public static int ReceiptCharacters = 44;
        public static int ReceiptPriceCharacters = 4;

        public static int ReceiptPriceDigitals = 2;

        public static int ReceiptItemQtyCharacters =3;
        public static int ReceiptItemWidth = 36;
        public static int ReceiptItemFontSize  = 12;
        public static string ReceiptItemItemFontFamily = "Consolas";

        public static string ReceiptFooterName = "receipt_foot_lines";
        public static string[] ReceiptFooter = new string[ReceiptFooterLines];

        public static int KitchenOrderHeaderLines = 4;
        public static string KitchenOrderHeaderName = "kitchen_header_line";
        public static string[] KitchenOrderHeader = new string[KitchenOrderHeaderLines];
        public static string KitchenOrderItemName = "kitchen_name";
        public static string KitchenOrderItemModifierName = "caption";

        public static int  KitchenOrderHeaderFontSize= 20;
        public static int KitchenOrderItemFontSize = 20;
        public static string KitchenOrderHeaderFontFamily = "Consolas";
        public static string KitchenOrderItemFontFamily = "Consolas";

        public static ObservableCollection<Models.Tender> Tenders = new ObservableCollection<Models.Tender>();

        // public static string[] PrinterName = new string[] { "", "", "", "", "", "", "", "", "", "" };
        public static List<Models.PosPrinter> PosPrinters = new List<Models.PosPrinter>();

        public static string PaymentFormParameter = "payment_message_line";
        public static int PaymentFormMessageLines = 2;
        public static string[] PaymentFormMessage = new string[PaymentFormMessageLines];
        
        public static string FinalFormParameter = "final_message_line";
        public static int FinalFormMessageLines = 2;        
        public static string[] FinalFormMessage = new string[PaymentFormMessageLines];

        public static string FinalPayCountParameter = "final_pay_count";
        public static int FinalPayCountMessageLines = 2;
        public static string[] FinalPayCountMessage = new string[FinalPayCountMessageLines];
        public static DateTime AccDate = DateTime.Today;
        //public static List<BannerImage> BannerImages = new List<BannerImage>();
        // public static PRPosTrn01 Trn01 = new PRPosTrn01();
        // public static List<PRPosTrnti> Trnti = new List<PRPosTrnti>();
        public static string InputCode = "";
        public static ADMediaTyle MediaType = ADMediaTyle.IMAGE;
        public static Models.Station ThisStation = new Models.Station();
        public static SelfOrderSettingClass SelfOrderSetting = new SelfOrderSettingClass();
        //public static Screen MainScreen = null;
        public enum ScreenOrientation
        {
            Portrait = 1,
            Landscape = 2,
        }
        /*
        public static void FormInit(Form frm)
        {
            frm.StartPosition = FormStartPosition.Manual;
            frm.FormBorderStyle = FormBorderStyle.None;
            Screen[] screens = Screen.AllScreens;
            int X = 0;
            int Y = 0;

            foreach (Screen s in screens)
            {
 
                if ((s.WorkingArea.Size.Width == PRPosUtils.ScreenWidth) && (s.WorkingArea.Size.Height > s.WorkingArea.Size.Width))
                {
                    //frm.Location = new Point(s.WorkingArea.Left, s.WorkingArea.Top);
                    X = s.WorkingArea.Left;
                    Y = s.WorkingArea.Top;
                    MainScreen = s;
                    break;
                }
            }
            if (MainScreen == null)
            {
                int HH = 0;
                foreach (Screen s in screens)
                {
                    if (s.WorkingArea.Height > HH)
                    {
                        HH = s.WorkingArea.Height;
                        MainScreen = s;
                    }
                }
            }
            X = MainScreen.WorkingArea.Left;
            Y = MainScreen.WorkingArea.Top;
            frm.Location = new Point(X, Y);
            frm.Size = new Size(PRPosUtils.ScreenWidth, MainScreen.Bounds.Size.Height);
        }
   */
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
                graphics.Dispose();

            }

            return destImage;
        }
        public static Color StringToColor(string colorstring)
        {
            Color c = Color.Black;
            string colorcode = colorstring.TrimStart('#');
            if (colorcode.Length == 6)
                c = Color.FromArgb(255, // hardcoded opaque
                            int.Parse(colorcode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colorcode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colorcode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
            else if (colorcode.Length == 5)
            {
                c = Color.FromArgb(255, // hardcoded opaque
                         int.Parse(colorcode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(4), System.Globalization.NumberStyles.HexNumber));
            }
            else if (colorcode.Length == 4)
            {
                c = Color.FromArgb(255, // hardcoded opaque
                         int.Parse(colorcode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(2, 1), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(3), System.Globalization.NumberStyles.HexNumber));
            }
            else if (colorcode.Length == 3)
            {
                c = Color.FromArgb(255, // hardcoded opaque
                         int.Parse(colorcode.Substring(0, 1), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(1, 1), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(2), System.Globalization.NumberStyles.HexNumber));
            }
            else if (colorcode.Length == 7)
            {
                c = Color.FromArgb(
                         int.Parse(colorcode.Substring(0, 1), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                         int.Parse(colorcode.Substring(5), System.Globalization.NumberStyles.HexNumber));
            }
            else // assuming length of 8
                c = Color.FromArgb(
                            int.Parse(colorcode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colorcode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colorcode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                            int.Parse(colorcode.Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
            return c;
        }

       
        public static string repeatStr(string str,int len)
        {
            string ret ="";
            for(int i = 0; i < len; i++)
            {
                ret += str;
            }
            return ret;
        }
        public static byte[] HexToByte(this string hexString)
        {
            // A R G B
            byte[] byteOUT = new byte[4] { 0xff, 0xff, 0xff, 0xff };
            hexString = hexString.Replace("#", "").Trim();
            int offset = 0;
            int Len = hexString.Length;
            if (Len > 8) Len = 8;
            if (Len < 8) offset = 1;

            int i = 0;
            while ((i < Len) && (offset < 4))
            {
                byteOUT[offset] = Convert.ToByte(hexString.Substring(i, 2), 16);
                offset += 1;
                i += 2;

            }
            return byteOUT;
        }
        public static void writelog(string message)
        {
            string logpath = Path.Combine(PRPosUtils.App_root, "log");

            if (!Directory.Exists(logpath))
            {
                Directory.CreateDirectory(logpath);
            }
            using (StreamWriter sw = new StreamWriter(  Path.Combine(logpath,"log" + DateTime.Today.ToString("yyyyMMdd") + ".log"), true))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + message);
            }
        }


        public static string key1 = "s99321901";
        public static string path = "";
        // public static string PosCode = "";
        public static string ConnectionCode = "";
        public static string PosName = "";
        public static string CustomerID = "";
        public static string PosID = "";
        public static string StoreCode = "";
        public static int BAN = 1;
        public static string Fastkey = "";
 
         public static string FilePath = @"images";
 
        public static bool IsAuth = false;
        public static int AuthorizType = 0;
        public static bool IsExpiry = true;
        public static string key2 = "chyshu";
        public static int CountDown = 10;

        public static bool AutoStart = false;
        public static string Token = "";
        public static int CheckDownload = 5; // 5 minutes
        public static DateTime ExpiryDate;
        public static long DaysSeconds = (24 * 60 * 60);//

        public const int AUTH_OK = 0b00000000;
        public const int INVALID = 0b00000001;
        public const int DECODE_ERROR = 0b00000011;
        public const int EXPRIRED = 0b00000100;
        public const int CHECKSUM_ERROR = 0b00000101;
        public const int TDATE_ISSUE = 0b00000110;
        public const int SERVER_ERROR = 0b00000111;
        public const int NOTFOUND = 0b00001000;
        public const int BEFORE_REGDATE = 0b00001001;
        public const int ST_NOTFOUND = 0b00001010;
        public const int FMT_ERROR = 0b00001011;
        public const int CODE_INUSED = 0b00001100;
        public static string CodeToMessage(int code)
        {
            string ret = "";
            switch (code)
            {
                case AUTH_OK:
                    ret = "Authorized";
                    break;
                case INVALID:
                    ret = "Not Authorized";
                    break;
                case DECODE_ERROR:
                    ret = "Connection code error!";
                    break;
                case EXPRIRED:
                    ret = "License is expired";
                    break;
                case CHECKSUM_ERROR:
                    ret = "Invalid data format";
                    break;
                case TDATE_ISSUE:
                    ret = "Register date failure!";
                    break;
                case SERVER_ERROR:
                    ret = "Can't connect to server";
                    break;
                case NOTFOUND:
                    ret = "Connection Code Not Found!";
                    break;
                case BEFORE_REGDATE:
                    ret = "No authorized at this date";
                    break;
                case ST_NOTFOUND:
                    ret = "Station Not Found";
                    break;

                case CODE_INUSED:
                    ret = "Connection Code used by other ST";
                    break;
                case FMT_ERROR:
                    ret = "Format error";
                    break;
            }

            return ret;
        }

        #region newwork_NIC
        /*
        public static List<NIC> GetMacAddress()
        {
            List<NIC> ret = new List<NIC>();

            string macAddress = string.Empty;
            string ipv4 = string.Empty;
            string ipv6 = string.Empty;


            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ConnectorPresent(nic))
                {
                    IPInterfaceProperties adapterProperties = nic.GetIPProperties();
                    UnicastIPAddressInformationCollection allAddress = adapterProperties.UnicastAddresses;
                    //  richTextBox1.AppendText(
                    //     "Found MAC Address: " + nic.GetPhysicalAddress() +
                    //     " Type: " + nic.NetworkInterfaceType + System.Environment.NewLine);

                    if (allAddress.Count > 0)
                    {
                        ///  PRPosUtils.writelog(nic.NetworkInterfaceType);
                        //  PRPosUtils.writelog(nic.Description);
                        //  PRPosUtils.writelog(nic.Name);
                        if ((nic.OperationalStatus == OperationalStatus.Up) &&
                           ((nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet) || (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)))
                        {

                            macAddress = nic.GetPhysicalAddress().ToString();
                            foreach (UnicastIPAddressInformation addr in allAddress)
                            {
                                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    ipv4 = addr.Address.ToString();
                                }
                                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                                {
                                    ipv6 = addr.Address.ToString();
                                }
                            }

                            // PRPosUtils.writelog("MAC=" + macAddress + ",ip4=" + ipv4 + ", ip6=" + ipv6);



                            if (string.IsNullOrWhiteSpace(macAddress) || (string.IsNullOrWhiteSpace(ipv4) && string.IsNullOrWhiteSpace(ipv6)))
                            {
                                macAddress = "";
                                ipv4 = "";
                                ipv6 = "";
                                continue;
                            }
                            else
                            {
                                if (macAddress.Length == 12)
                                {
                                    macAddress = string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                                    macAddress.Substring(0, 2), macAddress.Substring(2, 2), macAddress.Substring(4, 2),
                                    macAddress.Substring(6, 2), macAddress.Substring(8, 2), macAddress.Substring(10, 2));

                                    ret.Add(new NIC() { AdapterName = nic.Name, Description = nic.Description, MAC = macAddress, IPV4 = ipv4, IPV6 = ipv6 });
                                }
                                //  break;
                            }
                        }
                    }
                }
            }

            return ret;
        }
        public static bool ConnectorPresent(NetworkInterface ni)
        {
            ManagementScope scope = new ManagementScope(@"\\localhost\root\StandardCimv2");
            ObjectQuery query = new ObjectQuery(String.Format(
                @"SELECT * FROM MSFT_NetAdapter WHERE ConnectorPresent = True AND DeviceID = '{0}'", ni.Id));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection result = searcher.Get();
            return result.Count > 0;
        }*/
        #endregion
    }
    public static class StringExtensions
    {
        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
    }
}

