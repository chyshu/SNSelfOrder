using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SNSelfOrder
{
    /// <summary>
    /// Interaction logic for WindowItem.xaml
    /// </summary>
    public partial class WindowItem : Window
    {
        private WindowItemVM vm;
        public WindowItem()
        {
            InitializeComponent();
            vm = new WindowItemVM();
            this.DataContext = vm;
            ContentRendered += MainWindow_ContentRendered;
        }
        private bool firstTime = true;

        public bool FirstTime { get => firstTime; set => firstTime = value; }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow_ContentRendered");
            if (FirstTime)
            {
                FirstTime = false;

                await vm.BootingProcedure();
            }
        }
         
    }

    public class WindowItemVM : ViewModel.ViewModelBase
    {
        List<NIC> allNICList = new List<NIC>();
        private SelfOrderSettingClass selfOrderSetting;
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = 0;// PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        private int imgIndex = 0;
        private bool displayTimeoutMsg = false;
        private int msgTimeoutCount = 20;
        private string backgroundImagePath = "";
        private string currentBannerImagePath = "";
        private string startupImagePath = "";
        private string blankImagePath = "";
        private string checkImagePath = "";
        private ObservableCollection<BannerItem> BannerImagePathList { get; set; }
        internal List<NIC> AllNICList { get => allNICList; set => allNICList = value; }
        private ObservableCollection<PRPos.Data.FastkeySet> mainMenu;
        private System.Windows.Threading.DispatcherTimer bannerImageTimer;
        private System.Windows.Threading.DispatcherTimer timeoutTimer;
        private System.Windows.Threading.DispatcherTimer msgTimeoutTimer;

        private Station theStation;

        public Station TheStation { get { return theStation; } set { theStation = value; OnPropertyChanged("TheStation"); } }
        public ObservableCollection<PRPos.Data.FastkeySet> MainMenu { get => mainMenu; set { this.SetProperty(ref this.mainMenu, value); } }       
        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }

        private PRPos.Data.FastkeySet selectedMainMenuItem;
        private PRPos.Data.FastkeySet selectedMenu;

        public bool DisplayTimeoutMsg { get { return displayTimeoutMsg; } set { displayTimeoutMsg = value; OnPropertyChanged("DisplayTimeoutMsg"); } }
        public int MsgTimeoutCount { get { return msgTimeoutCount; } set { msgTimeoutCount = value; OnPropertyChanged("MsgTimeoutCount"); } }

        public PRPos.Data.FastkeySet SelectedMenu { get { return this.selectedMenu; } set { this.SetProperty(ref this.selectedMenu, value); } }
        public PRPos.Data.FastkeySet SelectedMainMenuItem
        {
            get
            {
                return this.selectedMainMenuItem;
            }
            set
            {
                this.SetProperty(ref this.selectedMainMenuItem, value);
                //this.SelectedMenu = MainMenu.FirstOrDefault(x => x.Sid == value.Sid && x.Default_yn == "Y");
                // Debug.WriteLine((value as PRPos.Data.FastkeySet).Caption);
            }
        }
        public ICommand MainMenuSelectChanged { get; set; }
        public SelfOrderSettingClass SelfOrderSetting { get => selfOrderSetting; set => selfOrderSetting = value; }
        public string DisplayStartup { get => displayStartup; set { displayStartup = value; OnPropertyChanged("DisplayStartup"); } }

        public string CurrentBannerImagePath { get => currentBannerImagePath; set { SetProperty(ref currentBannerImagePath, value); } }
        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref backgroundImagePath, value); } }
        public string StartupImagePath { get => startupImagePath; set { SetProperty(ref startupImagePath, value); } }
        public string BlankImagePath { get => blankImagePath; set { SetProperty(ref blankImagePath, value); } }
        public string CheckImagePath { get => checkImagePath; set { SetProperty(ref checkImagePath, value); } }

        string displayStartup = "Visible";
        private void MainMenuSelectChangedAction(object param)
        {
            Debug.WriteLine("MainMenuSelectChangedAction " + (param as PRPos.Data.FastkeySet).Caption);
            OnPropertyChanged("");
        }

        public void StartImgClick(Object sender, MouseButtonEventArgs e)
        {
            //   Trace.TraceInformation( e.GetPosition(sender as IInputElement).ToString() );

            Point clickPoint = e.GetPosition(sender as IInputElement);
            Debug.WriteLine(clickPoint);
            DisplayStartup = "Hidden";
        }
        private bool windowIsVisible = true;
        public bool WindowIsVisible
        {
            get { return this.windowIsVisible; }
            set
            {
                SetProperty(ref windowIsVisible, value);
            }
        }
        private void StartBannerImageTimer()
        {
            if (bannerImageTimer == null)
            {
                int bannerTime = PRPosUtils.BannerPlayTime != null ? PRPosUtils.BannerPlayTime : 10;
                this.bannerImageTimer = new System.Windows.Threading.DispatcherTimer();
                this.bannerImageTimer.Tick += new EventHandler(imageTimer_Tick);
                this.bannerImageTimer.Interval = new TimeSpan(0, 0, bannerTime);
            }
            this.bannerImageTimer.Start();
        }
        public void StartTimeoutTimer()
        {
            Debug.WriteLine("StartTimeoutTimer... ");
            if (!DisplayTimeoutMsg)
            {
                if (timeoutTimer == null)
                {
                    timeoutTimer = new System.Windows.Threading.DispatcherTimer();
                    timeoutTimer.Tick += timeoutTimer_Tick;
                }
                else
                {
                    timeoutTimer.Stop();
                }
                var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 20;
                timeoutTimer.Interval = new TimeSpan(0, 0, 0, seconds);
                timeoutTimer.Start();
            }
        }
        private void imageTimer_Tick(object sender, EventArgs e)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Task.Run(async () => { await LoopBannerImage(); });
                });
            }
        }
        private void timeoutTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timeoutTimer_Tick... ");
            //DisplayPage(PosPage.Timeout);
            timeoutTimer.Stop();
            if (msgTimeoutTimer == null)
            {
                msgTimeoutTimer = new System.Windows.Threading.DispatcherTimer();
                msgTimeoutTimer.Tick += msgTimeoutTimer_Tick;
            }
            else
            {
                msgTimeoutTimer.Stop();
            }
            msgTimeoutTimer.Interval = new TimeSpan(0, 0, 0, 1);
            MsgTimeoutCount = 10;
            msgTimeoutTimer.Start();
        }
        private void msgTimeoutTimer_Tick(object sender, EventArgs e)
        {
            MsgTimeoutCount--;
            if (this.MsgTimeoutCount <= 0)
            {
                //DoClose();
            }
        }
        private void StoptimeoutTimer()
        {
            if (timeoutTimer != null)
            {
                timeoutTimer.Stop();
            }
        }
        private async Task LoopBannerImage()
        {
            bannerImageTimer.Stop();
            imgIndex++;
            if (imgIndex >= BannerImagePathList.Count)
            {
                imgIndex = 0;
            }
            this.CurrentBannerImagePath = BannerImagePathList[imgIndex].BannerImagePath;
            // Debug.WriteLine(" LoopBannerImage " + this.CurrentBannerImagePath );
            this.bannerImageTimer.Interval = new TimeSpan(0, 0, BannerImagePathList[imgIndex].DisplayTime);
            this.bannerImageTimer.Start();
        }
        private async Task<ObservableCollection<BannerItem>> GetBannersByCode(string code)
        {
            ObservableCollection<BannerItem> bannerList = new ObservableCollection<BannerItem>();
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = "select * from pssystem where code_type=@code and ifnull(f3,'N')='N' and ifnull(f1,'')<>'' order by i1";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("code", code);
                DataTable pssystem = new DataTable();
                da.Fill(pssystem);
                foreach (DataRow row in pssystem.Rows)
                {
                    int disptime = 0;
                    if (!int.TryParse(row["i2"].ToString(), out disptime))
                    {
                        disptime = PRPosUtils.BannerPlayTime;
                    }
                    string fn = System.IO.Path.Combine(PRPosUtils.FilePath, row["f1"].ToString());

                    BannerItem bannerfile = new BannerItem()
                    {
                        BannerImagePath = fn,
                        DisplayTime = disptime,
                    };
                    bannerList.Add(bannerfile);
                }
            }
            return bannerList;
        }


   

        public WindowItemVM()
        {
            
            SelfOrderSetting = new SelfOrderSettingClass();
            SelfOrderSetting.ConnString = PRPosDB.cnStr;
            
            SelfOrderSetting.Security = new SecurityController();

            var AllNics = from nic in NetworkInterface.GetAllNetworkInterfaces()
                       where nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                             nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                       select nic;
            foreach(var nic in AllNics)
            {
                // Debug.WriteLine(mac);
                if(nic.OperationalStatus == OperationalStatus.Up)
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
                    AllNICList.Add(N );
                }
            }
            //AllNICList = GetMacAddress();
            /*
             AllNICList = Helpers.PRWindowsHelper.GetMacAddress();
            */
             SelfOrderSetting.HostURL = PRPosUtils.HostURL;
             SelfOrderSetting.MAC =
 #if _DEBUG
                  "1C-1B-0D-EC-67-CF";
#else
                 AllNICList[0].MAC;
#endif
             Debug.WriteLine(AllNICList[0].MAC);
            BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
            StartupImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Start);
            CheckImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
            BannerImagePathList = new ObservableCollection<BannerItem>();

            this.SelectedMenu = new PRPos.Data.FastkeySet();
            mainMenu = new ObservableCollection<PRPos.Data.FastkeySet>();

            using (SQLiteConnection cn = new SQLiteConnection(@"data source=C:\Users\Roger\Desktop\data\db\prposdb.db3"))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                List<PRPos.Data.FastKeyClass> keyList = new List<PRPos.Data.FastKeyClass>();
                cmd.CommandText = "select * from posfastkeyset where set_code=@set_code and customerid=@customerid and store_code=@store_code and del_flag='N'  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("set_code", "OT TW");
                cmd.Parameters.AddWithValue("customerid", "3");
                cmd.Parameters.AddWithValue("store_code", "TW");
                DataTable posfastkeysetDT = new DataTable();
                da.Fill(posfastkeysetDT);
                System.Diagnostics.Debug.WriteLine(posfastkeysetDT.Rows.Count);
                if (posfastkeysetDT.Rows.Count > 0)
                {
                    DataRow posfastkeyset = posfastkeysetDT.Rows[0];
                    cmd.CommandText = @"select * from posfastkey02 where psid=@psid and  display_yn='Y' and op_code='1'  
                                           and del_flag='N'  and customerid=@customerid order by disp_order";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("psid", posfastkeyset["sid"].ToString());
                    cmd.Parameters.AddWithValue("customerid", "3");
                    DataTable fastkey02 = new DataTable();
                    da.Fill(fastkey02);

                    foreach (DataRow row in fastkey02.Rows)
                    {


                        PRPos.Data.FastkeySet fk = new PRPos.Data.FastkeySet()
                        {
                            Sid = int.Parse(row["sid"].ToString()),
                            PSid = int.Parse(row["psid"].ToString()),
                            Caption = row["caption"].ToString(),
                            PCode = row["op_code"].ToString(),
                            Width = row["width"] == DBNull.Value ? 200 : int.Parse(row["width"].ToString()),
                            Height = row["height"] == DBNull.Value ? 200 : int.Parse(row["height"].ToString()),
                            Selected = row["ref_code"].ToString(),
                            Seq = row["disp_order"] == DBNull.Value ? 9 : int.Parse(row["disp_order"].ToString()),
                            Default_yn = row["default_yn"] == DBNull.Value ? "N" : row["default_yn"].ToString(),
                            Display_yn = row["display_yn"] == DBNull.Value ? "Y" : row["display_yn"].ToString(),
                            Full_Image_yn = row["fullimage_yn"] == DBNull.Value ? "N" : row["fullimage_yn"].ToString(),
                            FontColor = row["fontcolor"] == DBNull.Value ? "#FFFFFF" : (row["fontcolor"].ToString().StartsWith("#") ? row["fontcolor"].ToString() : "#" + row["fontcolor"].ToString()),
                            FontFamily = row["fontfamily"].ToString().Equals("") ? "sans serif" : row["fontfamily"].ToString(),
                            FontStyle = row["fontstyle"].ToString(),
                            Picture = System.IO.Path.Combine(@"C:\Users\Roger\Desktop\data\images", row["imagefile"].ToString()),
                            FontSize = row["fontsize"] == DBNull.Value ? 16 : int.Parse(row["fontsize"].ToString()),
                            BackColor = row["bgcolor"] == DBNull.Value ? "#061213" : (row["bgcolor"].ToString().StartsWith("#") ? row["bgcolor"].ToString() : "#" + row["bgcolor"].ToString()),
                            TextBgColor = row["textbgcolor"] == DBNull.Value ? "Transparent" : (row["textbgcolor"].ToString().StartsWith("#") ? row["textbgcolor"].ToString() : "#" + row["textbgcolor"].ToString()),
                            TextOffset = row["textheight"] == DBNull.Value ? 65 : int.Parse(row["textheight"].ToString()),
                            Text_display_yn = row["caption_yn"] == DBNull.Value ? "Y" : row["caption_yn"].ToString(),
                        };
                        MainMenu.Add(fk);

                        if (fk.Default_yn == "Y")
                        {
                            this.SelectedMainMenuItem = mainMenu.Last();
                        }
                    }
                }
            }

            this.MainMenuSelectChanged = new DelegateCommand<PRPos.Data.FastkeySet>(MainMenuSelectChangedAction);
        }

        public async Task<int> BootingProcedure()
        {
            int ret = 0;
           // DAL.StationBL stationBL = new DAL.StationBL();
           // TheStation = stationBL.GetStation(SelfOrderSetting, new DAL.StationDAL());

            BannerImagePathList = GetBannersByCode("banner_image").Result;
            if (BannerImagePathList.Count > 0)
            {
                this.CurrentBannerImagePath = BannerImagePathList[0].BannerImagePath;
            }
           StartBannerImageTimer();


            return ret;
        }
    }

}
