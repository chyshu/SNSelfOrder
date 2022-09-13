using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Threading;
using log4net;
using System.Xml;
using System.Xml.XPath;
using log4net.Repository;
using System.Reflection;
using SNSelfOrder.Models;
using System.Net.NetworkInformation;

namespace SNSelfOrder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {/*
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
*/
        public static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static System.Threading.SynchronizationContext _syncContext = null;
        protected async override void OnStartup(StartupEventArgs e)
        {
            //do some preparation here
            //also access the command line arguments on e.Args

            // log4net.Config.XmlConfigurator.Configure();

            log4net.Repository.ILoggerRepository RootRep;
            RootRep = LogManager.GetRepository(Assembly.GetCallingAssembly());

            XmlElement section = ConfigurationManager.GetSection("log4net") as XmlElement;

            XPathNavigator navigator = section.CreateNavigator();
            XPathNodeIterator nodes = navigator.Select("appender/param");
            foreach (XPathNavigator param in nodes)
            {
                if (param.GetAttribute("name", String.Empty) == "File")
                {
                    param.MoveToAttribute("value", string.Empty);
                    param.SetValue(string.Format(param.Value, DateTime.Today.ToString("yyyyMMdd")));
                }
            }

            IXmlRepositoryConfigurator xmlCon = RootRep as IXmlRepositoryConfigurator;
            xmlCon.Configure(section);

            log.Info("==Startup=====================>>>");

            // log.InfoLine(System.Windows.Application.Current.Resources.MergedDictionaries);    
            try
            {
                string datapath = ConfigurationManager.AppSettings["datapath"];
                log.Info("Config:" + datapath);
                if (datapath == "")
                {
                    datapath = AppDomain.CurrentDomain.BaseDirectory;
                }

                if (!System.IO.Directory.Exists(datapath))
                {
                    try
                    { System.IO.Directory.CreateDirectory(datapath); }
                    catch (Exception err)
                    { Debug.WriteLine(err.Message); }
                }

                PRPosUtils.App_root = datapath;// @"C:\Users\user\Desktop\PRSelfOrder_GIOV2\PRSelfOrder\PRSelfOrder";

                log.Info("App_root path=:" + datapath);
                log.Info("PRPosUtils App Root Path =" + PRPosUtils.App_root);

                PRPosUtils.FilePath = Path.Combine(datapath, "images");

                //   PRPosUtils.App_root = System.AppDomain.CurrentDomain.BaseDirectory;

                string downloadpath = ConfigurationManager.AppSettings["downloadpath"];

                PRPosUtils.FilePath = Path.Combine(PRPosUtils.App_root, downloadpath);

                if (!System.IO.Directory.Exists(PRPosUtils.FilePath))
                {
                    try
                    { System.IO.Directory.CreateDirectory(PRPosUtils.FilePath); }
                    catch (Exception err)
                    { Debug.WriteLine(err.Message); }
                }

                string logpath = ConfigurationManager.AppSettings["logpath"];

                if (!System.IO.Directory.Exists(Path.Combine(PRPosUtils.App_root, logpath)))
                {
                    try
                    { System.IO.Directory.CreateDirectory(Path.Combine(PRPosUtils.App_root, logpath)); }
                    catch (Exception err)
                    { Debug.WriteLine(err.Message); }
                }

                string dbpath = ConfigurationManager.AppSettings["dbpath"];

                if (!System.IO.Directory.Exists(Path.Combine(PRPosUtils.App_root, dbpath)))
                {
                    try
                    { System.IO.Directory.CreateDirectory(Path.Combine(PRPosUtils.App_root, dbpath)); }
                    catch (Exception err)
                    { Debug.WriteLine(err.Message); }
                }

                PRPosUtils.Spool_Folder = ConfigurationManager.AppSettings["spool"];

                if (!System.IO.Directory.Exists(Path.Combine(PRPosUtils.App_root, PRPosUtils.Spool_Folder)))
                {
                    try

                    { System.IO.Directory.CreateDirectory(Path.Combine(PRPosUtils.App_root, PRPosUtils.Spool_Folder)); }
                    catch (Exception err)
                    { Debug.WriteLine(Path.Combine(PRPosUtils.App_root, PRPosUtils.Spool_Folder) + "," + err.Message); }
                }

                PRPosUtils.CheckDownload = 3;
                // PRPosUtils.FilePath = "images";

                PRPosUtils.HostURL = "http://35.201.9.186/";

                log.Info("Application_Startup at " + System.AppDomain.CurrentDomain.BaseDirectory);

                log.Info("PRPosUtils App Root Path =" + PRPosUtils.App_root);

                PRPosDB.dbPath = Path.Combine(dbpath, "prposdb.db3");
                PRPosDB.cnStr = "data source=" + Path.Combine(PRPosUtils.App_root, PRPosDB.dbPath);

                log.Info(PRPosDB.cnStr);

                if (!System.IO.File.Exists(Path.Combine(PRPosUtils.App_root, PRPosDB.dbPath)))
                {
                    /*
                        MsgWindow wnd = new MsgWindow();
                        Application.Current.MainWindow = wnd;
                        wnd.SetCaption = "System Error ";
                        wnd.SetMessage = "DB File do not exists!!\n File path:" + Path.Combine(PRPosUtils.App_root, PRPosDB.dbPath);
                        // Show the window
                        wnd.InitializeComponent();
                        wnd.Show();
                    */
                    PRPosDB.Create_Database();
                }
                PRPosDB.InitSQLiteDb();
                PRPosDB.ReadParameter();

                if (System.IO.File.Exists(@"./App.config"))
                {
                    PRPosUtils.InputCode = ConfigurationManager.AppSettings["InputCode"];
                    PRPosUtils.HostURL = ConfigurationManager.AppSettings["backoffice"];
                    log.Info(PRPosUtils.HostURL);

                    // PRPosUtils.FilePath = Path.Combine(PRPosUtils.App_root, strpath);

                    string zstr = ConfigurationManager.AppSettings["CheckDownload_period"];
                    if (zstr != "")
                    {
                        int r = 0;
                        int.TryParse(zstr, out r);
                        if (r == 0) r = 3;
                    }
                    else
                    {
                        PRPosUtils.CheckDownload = 3;
                    }
                }
                log.Info(CultureInfo.CurrentCulture.Name);



                // NumberFormatInfo nfi = (NumberFormatInfo)Thread.CurrentThread.CurrentCulture.NumberFormat.Clone();

               
                /*
                CultureInfo.CurrentCulture = PRPosUtils.LocalCulture;
                CultureInfo.CurrentUICulture = PRPosUtils.LocalCulture;
                
                CultureInfo.DefaultThreadCurrentCulture = PRPosUtils.LocalCulture;
                CultureInfo.DefaultThreadCurrentUICulture = PRPosUtils.LocalCulture;

                Thread.CurrentThread.CurrentCulture = PRPosUtils.LocalCulture;
                Thread.CurrentThread.CurrentUICulture = PRPosUtils.LocalCulture;
                */
                
                log.Info(CultureInfo.CurrentCulture.Name);
                log.Info(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
                log.Info(CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern);
                log.Info(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                log.Info(CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
                 
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(
                      System.Windows.Application.LoadComponent(new Uri("SNSelfOrder;component/Style/SelfOrder.xaml", UriKind.Relative)) as System.Windows.ResourceDictionary
                );


                if (System.IO.File.Exists(@".\Kiosk.xaml"))
                {
                    base.OnStartup(e);
                    var appResources = System.Windows.Application.Current.Resources.MergedDictionaries;
                    var currentSkin = appResources[0];

                    // load the Kiosk.xaml file which contains the user definitions
                    var userDefinedSkin = new System.Windows.ResourceDictionary();

                    var skinUri = new Uri(@".\Kiosk.xaml", UriKind.RelativeOrAbsolute);
                    userDefinedSkin.Source = skinUri;

                    // if we have a user defined skin then merge its keys in to the new skin
                    if (userDefinedSkin != null)
                    {
                        // now merge in the user defined details
                        foreach (var key in userDefinedSkin.Keys)
                        {
                            var value = userDefinedSkin[key];
                            if (currentSkin.Contains(key))
                            {
                                currentSkin[key] = value;
                            }
                            else
                            {
                                currentSkin.Add(key, value);
                            }
                        }
                    }
                }

                PRPosUtils.SelfOrderSetting.ConnString = PRPosDB.cnStr;
                PRPosUtils.SelfOrderSetting.Security = new SecurityController();
                PRPosUtils.SelfOrderSetting.HostURL = PRPosUtils.HostURL;
                List<NIC> AllNICList = new List<NIC>();

                #region Get_NETWORK_ADAPTER
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

                #endregion

                PRPosUtils.SelfOrderSetting.MAC =
#if _DEBUG
                 "1C-1B-0D-EC-67-CF";
#else
                AllNICList[0].MAC;
#endif
                _syncContext = System.Threading.SynchronizationContext.Current;
                DAL.StationBL stationBL = new DAL.StationBL();
                PRPosUtils.ThisStation = stationBL.GetStation(PRPosUtils.SelfOrderSetting, new DAL.StationDAL());
                /*
                WindowItem wnd = new WindowItem();
                Application.Current.MainWindow = wnd;
                wnd.InitializeComponent();
                wnd.Show();

                */
                CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(PRPosUtils.SelfOrderSetting);
                if ((PRPosUtils.ThisStation.Connection == "") || (PRPosUtils.InputCode == "1"))
                {
                    App.log.Info(" CheckCCode    ");

                    ConnectionCode frmConnection = new ConnectionCode();
                    var vm = new ViewModel.ConnectionCodeVM();
                    vm.ConnectionCode = PRPosUtils.ThisStation.Connection;
                    vm.DisplayMessage = "Input Connection Code";
                    App.log.Info(" CheckConnectionCodeService Start   ");
                    vm.InputClose += async (e) =>
                    {
                        string strconnection = (e as string);
                        App.log.Info(" CheckConnectionCodeService  :" + strconnection);
                        await checkCodeService.LaunchCodeCheck(strconnection).ContinueWith(task =>
                        {
                            App.log.Info(" CheckCode 1 :" + task.Result);
                            Station _station = task.Result;
                            //if (_station.StatusCode == 0)
                            {
                                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                                {
                                    MainWindow wnd = new MainWindow();
                                        // Show the window
                                    Application.Current.MainWindow = wnd;
                                    wnd.InitializeComponent();
                                    wnd.Show();
                                    frmConnection.Close();
                                };
                                _syncContext.Post(methodDelegate, null);

                            }
                            //else
                            //{
                            //    vm.DisplayMessage = checkCodeService.StatusToMessage(_station.StatusCode);
                           // }
                        });
                    };
                    frmConnection.DataContext = vm;
                    frmConnection.Show();


                }
                else
                {

                    await checkCodeService.LaunchCodeCheck(PRPosUtils.ThisStation.Connection).ContinueWith(task =>
                    {
                       Station _station = task.Result;
                       App.log.Error(" CheckCode 2 :" + _station.StatusCode);

                        // if can't connect to server show connection code dialog

                        if (_station.StatusCode == 0)
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                MainWindow wnd = new MainWindow();
                               // Show the window
                               Application.Current.MainWindow = wnd;
                                wnd.InitializeComponent();
                                wnd.Show();
                            };
                            _syncContext.Post(methodDelegate, null);
                        }
                        else
                        {
                            if ((_station.StatusCode != -3) && (_station.StatusCode != -4) && (_station.StatusCode != -5))
                            {

                                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                                {


                                    ConnectionCode frmConnection = new ConnectionCode();
                                    var vm = new ViewModel.ConnectionCodeVM();
                                    vm.ConnectionCode = PRPosUtils.ThisStation.Connection;
                                    vm.DisplayMessage = checkCodeService.StatusToMessage(_station.StatusCode);

                                    vm.InputClose += async (e) =>
                                    {
                                        string strconnection = (e as string);
                                        var task1 = await checkCodeService.LaunchCodeCheck(strconnection);
                                        {
                                            _station = task1;
                                            if (_station.StatusCode == 0)
                                            {
                                               // connection code is active 

                                                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                                                {
                                                    MainWindow wnd = new MainWindow();
                                                   // Show the window
                                                   Application.Current.MainWindow = wnd;
                                                    wnd.InitializeComponent();
                                                    wnd.Show();
                                                    frmConnection.Close();
                                                };
                                                _syncContext.Post(methodDelegate, null);

                                            }
                                            else
                                            {
                                                vm.DisplayMessage = checkCodeService.StatusToMessage(_station.StatusCode);
                                            }
                                        }
                                    };
                                    frmConnection.DataContext = vm;
                                    frmConnection.Show();
                                };
                                _syncContext.Post(methodDelegate, null);
                            }
                        }
                   });
                }  
                 
                /*await checkCodeService.CheckCode(PRPosUtils.ThisStation.Connection).ContinueWith(task=>                    
                {
                    log.Info("CheckCode return ");
                    if (task.Result.StatusCode == 0)
                    {
                        MainWindow wnd = new MainWindow();
                        // Show the window
                        Application.Current.MainWindow = wnd;
                        wnd.InitializeComponent();
                        wnd.Show();
                    }
                });

                CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(PRPosUtils.SelfOrderSetting);

                checkCodeService.CheckCode(PRPosUtils.ThisStation.Connection).ContinueWith(
                      task =>
                      {
                          Station st = task.Result;
                          Debug.WriteLine(st.StatusCode);
                          if (st.StatusCode == 0)
                          {
                              MainWindow wnd = new MainWindow();
                              // Show the window
                              Application.Current.MainWindow = wnd;
                              wnd.InitializeComponent();
                              wnd.Show();
                          }
                      });
                */
            }
            catch (Exception ert)
            {
                App.log.Error(ert.Message);
            }
            
        }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
          
        }
    }
}
