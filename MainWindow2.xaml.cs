using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PRPos.Data;
using SNSelfOrder.Models;
using SNSelfOrder.ViewModel;

namespace SNSelfOrder
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
         private MainWindow2VM mainWindowVM;
        public MainWindow2()
        {
            InitializeComponent();
            mainWindowVM = new MainWindow2VM();

            this.DataContext = mainWindowVM;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            Closing += MainWindow_Closing;
            ContentRendered += MainWindow_ContentRendered;
            Debug.WriteLine("MainWindow Created");
        }

        private bool firstTime = true;

        public bool FirstTime { get => firstTime; set => firstTime = value; }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow_ContentRendered");
            if (FirstTime)
            {
                FirstTime = false;

               // await mainWindowVM.BootingProcedure();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // throw new NotImplementedException();
            Debug.WriteLine("MainWindow_Closing");
           // mainWindowVM.DisposeAll();
        }
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("MainWindow_IsVisibleChanged");
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainWindow_Loaded");
        }
        private void MainMenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("MainMenuListBox_SelectionChanged: " + (e.Source as ListBox).SelectedItem + "," +
                ItemsListBox.Items.Count);
            (e.Source as ListBox).ScrollIntoView((e.Source as ListBox).SelectedItem);
            if (ItemsListBox.Items.Count > 0)
            {
                ItemsListBox.ScrollIntoView(ItemsListBox.Items[0]);
            }
        }
    }
    public class MainWindow2VM: ViewModelBase
    {
        private ObservableCollection<BannerItem> BannerImagePathList { get; set; }
        private ObservableCollection<PRPos.Data.FastkeySet> mMainMenu;
        public ObservableCollection<PRPos.Data.FastkeySet> MainMenu { get { return this.mMainMenu; } set { this.SetProperty(ref this.mMainMenu, value); } }

        public Station TheStation { get => theStation; set => theStation = value; }
        public SelfOrderSettingClass SelfOrderSetting { get => selfOrderSetting; set => selfOrderSetting = value; }
        public List<NIC> AllNICList { get => allNICList; set => allNICList = value; }

        List<NIC> allNICList;
        string CurrentBannerImagePath;
        int imgIndex = 0;
        private Station theStation;
        private SelfOrderSettingClass selfOrderSetting;
        public MainWindow2VM() {

            AllNICList = new List<NIC>();
            var AllNics = from nic in NetworkInterface.GetAllNetworkInterfaces()
                          where nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                          select nic;
            foreach (var nic in AllNics)
            {
                // Debug.WriteLine(mac);
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    string macAddress = nic.GetPhysicalAddress().ToString();

                    macAddress = string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                                  macAddress.Substring(0, 2), macAddress.Substring(2, 2), macAddress.Substring(4, 2),
                                  macAddress.Substring(6, 2), macAddress.Substring(8, 2), macAddress.Substring(10, 2));
                    IPInterfaceProperties adapterProperties = nic.GetIPProperties();
                    UnicastIPAddressInformationCollection allAddress = adapterProperties.UnicastAddresses;
                    string ipv4 = "";
                    string ipv6 = "";
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
                    NIC N = new NIC();
                    N.AdapterName = nic.Name;
                    N.Description = nic.Description;
                    N.MAC = macAddress;
                    N.IPV4 = ipv4;
                    N.IPV6 = ipv6;
                    AllNICList.Add(N);
                }
            }
            SelfOrderSetting = new SelfOrderSettingClass();
            SelfOrderSetting.ConnString = PRPosDB.cnStr;
            SelfOrderSetting.Security = new SecurityController();
            
            SelfOrderSetting.HostURL = PRPosUtils.HostURL;
            SelfOrderSetting.MAC =
#if _DEBUG
                 "1C-1B-0D-EC-67-CF";
#else
                AllNICList[0].MAC;
#endif

            DAL.StationBL stationBL = new DAL.StationBL();
            TheStation = stationBL.GetStation(SelfOrderSetting, new DAL.StationDAL());

            LoadFastKey();
        }
        private bool windowIsVisible = true;
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }
        private async Task LoopBannerImage()
        {

            this.CurrentBannerImagePath = BannerImagePathList[imgIndex].BannerImagePath;
            // Debug.WriteLine(" LoopBannerImage " + this.CurrentBannerImagePath );
            
        }
        private PRPos.Data.FastkeySet selectedMainMenu;
        public PRPos.Data.FastkeySet SelectedMainMenu
        {
            get
            {
                return this.selectedMainMenu;
            }
            set
            {
                Debug.WriteLine("SelectedMainMenu set " + value);
                this.SetProperty(ref this.selectedMainMenu, value);
                //StartTimeoutTimer();
#if _DEBUG
                // if (value != null)
                //     Debug.WriteLine("SelectedMainMenu " + value.Caption);
#endif
                //  this.SelectedMenu = MainMenu.FirstOrDefault(x => x.Sid == value.Sid && x.Default_yn == "Y");
                //   OnPropertyChanged("");

            }
        }

        public bool WindowIsVisible { get => windowIsVisible; set => windowIsVisible = value; }

        private void LoadFastKey()
        {
            PRPos.Data.FastkeySet mSelectedMenu = null;

            //this.SelectedMenu = new PRPos.Data.FastkeySet();
            MainMenu = new ObservableCollection<PRPos.Data.FastkeySet>();
            //ModifierSetList = new ObservableCollection<ModSet>();

            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                string salePrice =  "sprice";// PRPosUtils.SalePriceColumn.Equals("") ? "sprice" : PRPosUtils.SalePriceColumn;
                string takeawayPrice = "sprice2"; //  PRPosUtils.TakeawayPriceColumn.Equals("") ? "sprice2" : PRPosUtils.SalePriceColumn;

                ObservableCollection<PRPos.Data.FastkeySet> menulists = new ObservableCollection<PRPos.Data.FastkeySet>();
                cmd.CommandText = "select * from posfastkeyset where set_code=@set_code and customerid=@customerid and store_code=@store_code and del_flag='N'  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("set_code", TheStation.Set_code);
                cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                cmd.Parameters.AddWithValue("store_code", TheStation.Store_code);
                DataTable posfastkeysetDT = new DataTable();
                da.Fill(posfastkeysetDT);

                if (posfastkeysetDT.Rows.Count > 0)
                {
                    DataRow posfastkeyset = posfastkeysetDT.Rows[0];
                    cmd.CommandText = @"select * from posfastkey02 where psid=@psid and  display_yn='Y' and op_code='1'  
                                           and del_flag='N'  and customerid=@customerid order by disp_order";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("psid", posfastkeyset["sid"].ToString());
                    cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                    DataTable fastkey02 = new DataTable();
                    da.Fill(fastkey02);

                    foreach (DataRow row in fastkey02.Rows)
                    {
                        PRPos.Data.FastkeySet menuItem = new PRPos.Data.FastkeySet()
                        {
                            Sid = int.Parse(row["sid"].ToString()),
                            PSid = int.Parse(row["psid"].ToString()),
                            Caption = row["caption"].ToString(),
                            PCode = row["op_code"].ToString(),
                            Width = row["width"] == DBNull.Value ? PRPosUtils.MenuButtonWidth : int.Parse(row["width"].ToString()) * PRPosUtils.MenuButtonWidth,
                            Height = row["height"] == DBNull.Value ? PRPosUtils.MenuHeight : int.Parse(row["height"].ToString()) * PRPosUtils.MenuHeight,
                            Selected = row["ref_code"].ToString(),
                            Seq = row["disp_order"] == DBNull.Value ? 9 : int.Parse(row["disp_order"].ToString()),
                            Default_yn = row["default_yn"] == DBNull.Value ? "N" : row["default_yn"].ToString(),
                            Display_yn = row["display_yn"] == DBNull.Value ? "Y" : row["display_yn"].ToString(),
                            Full_Image_yn = row["fullimage_yn"] == DBNull.Value ? "N" : row["fullimage_yn"].ToString(),
                            FontColor = row["fontcolor"] == DBNull.Value ? "FFFFFF" : row["fontcolor"].ToString(),
                            FontFamily = row["fontfamily"].ToString().Equals("") ? "sans serif" : row["fontfamily"].ToString(),
                            FontStyle = row["fontstyle"].ToString(),
                            Picture = System.IO.Path.Combine(PRPosUtils.FilePath, row["imagefile"].ToString()),
                            FontSize = row["fontsize"] == DBNull.Value ? 16 : int.Parse(row["fontsize"].ToString()),
                            BackColor = row["bgcolor"] == DBNull.Value ? "061213" : row["bgcolor"].ToString(),
                            TextBgColor = row["textbgcolor"] == DBNull.Value ? "Transparent" : row["textbgcolor"].ToString(),
                            TextOffset = row["textheight"] == DBNull.Value ? 65 : int.Parse(row["textheight"].ToString()),
                            Text_display_yn = row["caption_yn"] == DBNull.Value ? "Y" : row["caption_yn"].ToString(),
                        };

                        cmd.CommandText = @"select * from posfastkey02 where display_yn='Y' and op_code = 2 and store_code=@store_code and del_flag='N' 
                                            and customerid=@customerid and psid=@psid order by disp_order";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                        cmd.Parameters.AddWithValue("store_code", TheStation.Store_code);
                        cmd.Parameters.AddWithValue("psid", menuItem.Sid);

                        DataTable fastkeyItemDT = new DataTable();
                        da.Fill(fastkeyItemDT);
                        ObservableCollection<FastKeyClass> keyItems = new ObservableCollection<FastKeyClass>();
                        foreach (DataRow itemrow in fastkeyItemDT.Rows)
                        {
                            cmd.CommandText =
                                @"select psitem.* ,itemvariety." + salePrice + @" as ivsprice,itemvariety." + takeawayPrice + @" as ivsprice2 
                                     ,promotions." + salePrice + @" as psprice,promotions." + takeawayPrice + @" as psprice2 ,
                                     datetime(psitem.upd_date) cloudUpdDate,  datetime(psitem.str_upd_date) localUpdDate  
                                  from psitem                                   
                                  left join itemvariety on psitem.item_code = itemvariety.item_code 
                                  left join promotions on psitem.item_code = promotions.item_code and promotions.customerid = psitem.customerid
                                               and bdate >= date('now') and edate<= date('now') and promotions.del_flag='N' 
                                  where psitem.customerid=@customerid and psitem.item_code=@item_code and itemvariety.store_code = @store_code ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("item_code", itemrow["ref_code"].ToString());
                            cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                            cmd.Parameters.AddWithValue("store_code", TheStation.Store_code);
                            DataTable psItemDT = new DataTable();
                            da.Fill(psItemDT);
                            decimal price1 = 0;
                            decimal price2 = 0;
                            string soldOut = "", upddate = "";
                            decimal gst = 0;
                            if (psItemDT.Rows.Count > 0)
                            {
                                var psitem = psItemDT.Rows[0];

                                decimal.TryParse(psitem[salePrice].ToString(), out price1);
                                if (!string.IsNullOrEmpty(psitem["ivsprice"].ToString()))
                                {
                                    decimal.TryParse(psitem["ivsprice"].ToString(), out price1);
                                }
                                if (!string.IsNullOrEmpty(psitem["psprice"].ToString()))
                                {
                                    decimal.TryParse(psitem["psprice"].ToString(), out price1);
                                }
                                if (!string.IsNullOrEmpty(psitem["gst"].ToString()))
                                {
                                    decimal.TryParse(psitem["gst"].ToString(), out gst);
                                }
                                decimal.TryParse(psitem[takeawayPrice].ToString(), out price2);
                                if (!string.IsNullOrEmpty(psitem["ivsprice2"].ToString()))
                                {
                                    decimal.TryParse(psitem["ivsprice2"].ToString(), out price2);
                                }
                                if (!string.IsNullOrEmpty(psitem["psprice2"].ToString()))
                                {
                                    decimal.TryParse(psitem["psprice2"].ToString(), out price2);
                                }
                                DateTime cloudUpdDate;
                                DateTime localUpdDate;
                                if (DateTime.TryParse(psitem["localUpdDate"].ToString(), out localUpdDate))
                                {
                                    if (DateTime.TryParse(psitem["cloudUpdDate"].ToString(), out cloudUpdDate))
                                    {
                                        if (localUpdDate > cloudUpdDate)
                                        {
                                            soldOut = string.IsNullOrEmpty(psitem["str_soldout"].ToString()) ? psitem["soldout"].ToString() : psitem["str_soldout"].ToString();
                                            upddate = localUpdDate.ToString();
                                        }
                                        else
                                        {
                                            soldOut = psitem["soldout"].ToString();
                                            upddate = cloudUpdDate.ToString();
                                        }
                                    }
                                    else
                                    {
                                        soldOut = string.IsNullOrEmpty(psitem["str_soldout"].ToString()) ? psitem["soldout"].ToString() : psitem["str_soldout"].ToString();
                                        upddate = localUpdDate.ToString();
                                    }
                                }
                                else
                                {
                                    soldOut = psitem["soldout"].ToString();
                                }

                                PRPos.Data.FastKeyClass keybutton = new PRPos.Data.FastKeyClass()
                                {
                                    Sid = int.Parse(itemrow["sid"].ToString()),
                                    PSid = int.Parse(itemrow["psid"].ToString()),
                                    Caption = itemrow["caption"].ToString(),
                                    Caption2 = itemrow["caption2"].ToString(),
                                    Caption3 = itemrow["caption3"].ToString(),
                                    Description = psitem["description"].ToString(),
                                    Spicy = psitem["spicy"].ToString(),
                                    SizeCode = psitem["size_code"].ToString(),
                                    ItemName = psitem["item_name"].ToString(),
                                    ItemCode = psitem["item_code"].ToString(),
                                    PriceLine = itemrow["priceline"].ToString(),
                                    SoldOut = soldOut.ToUpper(),
                                    Sprice = price1,
                                    GST = gst,
                                    Takeawayprice = price2,
                                    PCode = itemrow["op_code"].ToString(),
                                    Width = row["width"] == DBNull.Value ? PRPosUtils.ItemWidth : int.Parse(row["width"].ToString()) * PRPosUtils.ItemWidth,
                                    Height = row["height"] == DBNull.Value ? PRPosUtils.ItemHeight : int.Parse(row["height"].ToString()) * PRPosUtils.ItemHeight,
                                    Selected = itemrow["ref_code"].ToString(),
                                    Seq = itemrow["disp_order"] == DBNull.Value ? 9 : int.Parse(itemrow["disp_order"].ToString()),
                                    Default_yn = itemrow["default_yn"] == DBNull.Value ? "N" : itemrow["default_yn"].ToString(),
                                    Display_yn = itemrow["display_yn"] == DBNull.Value ? "Y" : itemrow["display_yn"].ToString(),
                                    Full_Image_yn = itemrow["fullimage_yn"] == DBNull.Value ? "N" : itemrow["fullimage_yn"].ToString(),
                                    FontColor = itemrow["fontcolor"] == DBNull.Value ? "FFFFFFFF" : itemrow["fontcolor"].ToString().Equals("") ? "Transparent" : itemrow["fontcolor"].ToString(),
                                    FontFamily = itemrow["fontfamily"].ToString().Equals("") ? "sans serif" : itemrow["fontfamily"].ToString(),
                                    FontStyle = itemrow["fontstyle"].ToString(),
                                    ButtonImgURI = System.IO.Path.Combine(PRPosUtils.FilePath, itemrow["imagefile"].ToString()),
                                    FontSize = itemrow["fontsize"] == DBNull.Value ? 16 : int.Parse(itemrow["fontsize"].ToString()),
                                    BackColor = itemrow["bgcolor"] == DBNull.Value ? "FF061213" : itemrow["bgcolor"].ToString().Equals("") ? "Transparent" : itemrow["bgcolor"].ToString(),
                                    TextBgColor = itemrow["textbgcolor"] == DBNull.Value ? "FFFFFF" : itemrow["textbgcolor"].ToString().Equals("") ? "Transparent" : itemrow["textbgcolor"].ToString(),
                                    TextHeight = itemrow["textheight"] == DBNull.Value ? 65 : int.Parse(itemrow["textheight"].ToString()),
                                    Text_display_yn = itemrow["caption_yn"] == DBNull.Value ? "Y" : itemrow["caption_yn"].ToString(),
                                    Text2_display_yn = itemrow["caption2_yn"] == DBNull.Value ? "N" : itemrow["caption2_yn"].ToString(),
                                    Text3_display_yn = itemrow["caption3_yn"] == DBNull.Value ? "N" : itemrow["caption3_yn"].ToString(),
                                    Upd_date = upddate,
                                    PsItem = psitem,
                                };
                                keyItems.Add(keybutton);
                                //Debug.WriteLine(" add " + keybutton.Caption + " ," + keybutton.FontStyle + " ," + keybutton.FontColor );
                            }
                        }
                        menuItem.FastkeyItems = keyItems;

                        menulists.Add(menuItem);
                        //  Debug.WriteLine(" add " + menuItem.Caption + " ," + menuItem.FontStyle + " ," + menuItem.FontColor+","+ menuItem.Width+","+ menuItem.Height);
#if _DEBUG
                        //        Debug.WriteLine("FastkeySet add "+ menuItem.Caption+" items="+ menuItem.FastkeyItems.Count);
#endif
                        if (menuItem.Default_yn == "Y")
                        {
                            if (mSelectedMenu == null)
                                mSelectedMenu = menuItem;
                        }
                    }

                    this.mMainMenu = menulists;
                    //  Debug.WriteLine("MainMenu Set");
                    if (mSelectedMenu == null)
                        mSelectedMenu = menulists[0];
                    this.selectedMainMenu = mSelectedMenu;

                    //OnPropertyChanged("");               
                }

            }
        }
    }


}
