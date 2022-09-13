using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using SNSelfOrder.Models;
using System.Windows.Input;
using System.Windows;
using PRPos.Data;
using System.Windows.Forms;
using AxCSDEFTLib;
using System.Windows.Controls;

using SNSelfOrder.Services;
using System.Net.Http;
using Newtonsoft.Json;
using SNSelfOrder.Interfaces;
using System.Net.NetworkInformation;
using System.Windows.Threading;

namespace SNSelfOrder.ViewModel
{
    enum PosPage
    {
        BackgroundImage,   //0        
        ItemsPage,        
        Startup,
        CancelOrderPage,        
        CheckEFTPOS,
        CheckConnection,
        OrderTypePage,  //6
        TableNumberPage,
        CoverPage,
        MemberPage,
        NumberPadPage,
        BottomLinePage,
        TimeOutPage,
    }

    public class MainWindowVM : ViewModelBase
    {


        AxCsdEft eftCtrl = null;
        private object _ocxContent;
        private Services.DownloadService downloadService = null;
        private Services.UploadService uploadService = null;

        private string backgroundImagePath = "";
        private string currentBannerImagePath = "";
        private string startupImagePath = "";
        private string blankImagePath = "";

        private Station theStation;
        private int admin = 0;

        private System.Windows.Threading.DispatcherTimer bannerImageTimer;
        private System.Windows.Threading.DispatcherTimer timeoutTimer;
        private System.Windows.Threading.DispatcherTimer msgTimeoutTimer;

        private int columnCount = 4;
        private int rowCount = 7;
        private bool displayNumberPadPage = false;
        private bool displayTimeoutMsg = false;
        private int msgTimeoutCount = 20;
        private int imgIndex = 0;
        private int numerPadHeight = 1200;
        private int itemCount = 0;
        private decimal totalPrice = 0m;

        private long imgStartTick = 0;
        
        private List<string> pagesZIndex = new List<string>() { "0", "10", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        private SelfOrderSettingClass selfOrderSetting;
        private ObservableCollection<BannerItem> BannerImagePathList { get; set; }
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        private string msgCancerOrder = "CANCEL ORDER";

        private bool displayStartup = false;
        private bool dispCancelOrder = false;
        private bool displayConnectionCode = false;
        private bool displayCoverPage = false;
        private bool displayCheckMessagePage = true;
        private bool displayTableNumberPage = false;
        private bool displayMemberCardPage = false;
        private bool displayOrderTypePage = false;

        private bool canPay = false;
        private string checkMessage = "";
        private string checkMessageTitle = "";
        private string connectionMessage = "";
        private string connectionMessageTitle = "";

        private string caption_BuzzerPage = "";
        private string message_BuzzerPage = "";

        private string caption_CoverPage = "";
        private string message_CoverPage = "";

        private string caption_MemberCardPage = "";
        private string message_MemberCardPage = "";

        private string timeOutMessage = "";

        private string dealOrderType = PRPosUtils.DefaultOrderType;
        private string caption_OrderTypePage = "";
        private string message_OrderTypePage = "";

        private string checkImagePath = "";
        private PRPos.Data.PSTrn01sClass currentDeal;
        private int mCheckTimeOut = 30;
        private int checkTimeCount = 0;
        private bool mCollapsed = false;
        private string eFTWorkStage = "";
        private string tableNumber = "";
        private string cover = "";
        private string memberCard = "";

        private EFTPOSLastTransStatus ConvertStatus(string status)
        {
            var result = EFTPOSLastTransStatus.Unknown;
            switch (status)
            {
                case "00":
                case "T0":
                    result = EFTPOSLastTransStatus.Approved;
                    break;
                case "TM":
                    result = EFTPOSLastTransStatus.Cancelled;
                    break;
                /*
            case EFT_TX_DECLINED:
                result = EFTPOSLastTransStatus.Declined;
                break;
            case EFT_TX_FAILED:
                result = EFTPOSLastTransStatus.Failed;
                break;
            case EFT_TX_OFFLINE_OK:
                result = EFTPOSLastTransStatus.OfflineOk;
                break;
                */
                case "XX":
                    result = EFTPOSLastTransStatus.Failed;
                    break;
                default:
                    result = EFTPOSLastTransStatus.Unknown;
                    break;
            }

            return result;
        }
        public PRPos.Data.PSTrn01sClass CurrentDeal
        {
            get => currentDeal;
            set
            {
                SetProperty(ref currentDeal, value);
                UpdateTotal();
            }
        }
        public Station TheStation { get { return theStation; } set { theStation = value; OnPropertyChanged("TheStation"); } }
        public int ItemCount { get { return itemCount; } set { itemCount = value; OnPropertyChanged("ItemCount"); } }
        public decimal TotalPrice { get { return totalPrice; } set { totalPrice = value; OnPropertyChanged("TotalPrice"); } }

        public bool DisplayTimeoutMsg { get { return displayTimeoutMsg; } set { displayTimeoutMsg = value; OnPropertyChanged("DisplayTimeoutMsg"); } }
        public int MsgTimeoutCount { get { return msgTimeoutCount; } set { msgTimeoutCount = value; OnPropertyChanged("MsgTimeoutCount"); } }
        public DelegateCommand<PRPos.Data.FastKeyClass> ClickItemCmd { get; set; }

        public DelegateCommand CancelOrderCmd { get; set; }

        public DelegateCommand StartUpCmd { get; set; }

        public DelegateCommand CancelCmd { get; set; }
        public DelegateCommand CancelOrderNoCmd { get; set; }
        public DelegateCommand CancelOrderYesCmd { get; set; }

        public ICommand GoBackCommand { get; set; }
        public ICommand TimeOutPageMoreTimeYesCmd { get; set; }

        public ICommand TimeOutPageMoreTimeNoCmd { get; set; }
        //public RelayCommand StartImgClickCmd { get; set; }

        private DelegateCommand checkoutCmd;

        public DelegateCommand CheckoutCmd => checkoutCmd ?? (checkoutCmd = new DelegateCommand(CheckoutCommandAction));

        private ObservableCollection<PRPos.Data.FastkeySet> mMainMenu;
        public ObservableCollection<PRPos.Data.FastkeySet> MainMenu { get { return this.mMainMenu; } set { this.SetProperty(ref this.mMainMenu, value); } }
        private ICommand buttonCommand;
        private ICommand orderTypeCommand;
        public object OcxContent
        {
            get => _ocxContent;
            set => SetProperty(ref _ocxContent, value);
        }

        #region MainMenuSelectChanged
        //private PRPos.Data.FastkeySet selectedMenu;        
        public ICommand MainMenuSelectChanged { get; set; }

        public ICommand ButtonCommand => buttonCommand ?? (buttonCommand = new DelegateCommand<string>(ButtonCommandAction));

        public ICommand OrderTypeCommand => orderTypeCommand ?? (orderTypeCommand = new DelegateCommand<string>(OrderTypeCommandAction));

        private void MainMenuSelectChangedAction(object param)
        {
#if _DEBUG
            if (param != null)
                Debug.WriteLine("SelectedMainMenu by DelegateCommand " + (param as PRPos.Data.FastkeySet).Caption);
#endif
            if (param != null)
            {
                Debug.WriteLine("SelectedMainMenu by DelegateCommand " + (param as PRPos.Data.FastkeySet).Caption);
                this.SetProperty(ref this.selectedMainMenu, (param as PRPos.Data.FastkeySet));
                OnPropertyChanged("");
            }
        }

        #endregion MainMenuSelectChanged

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

        //public PRPos.Data.FastkeySet SelectedMenu { get { return this.selectedMenu; } set { this.SetProperty(ref this.selectedMenu, value); } }


        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }

        private bool windowIsVisible = true;
        private System.Threading.SynchronizationContext _syncContext = null;
        public bool WindowIsVisible { get => this.windowIsVisible; set { SetProperty(ref windowIsVisible, value); } }
        public string CurrentBannerImagePath { get => currentBannerImagePath; set { SetProperty(ref currentBannerImagePath, value); } }
        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref backgroundImagePath, value); } }

        public string StartupImagePath { get => startupImagePath; set { SetProperty(ref startupImagePath, value); } }

        public string BlankImagePath { get => blankImagePath; set { SetProperty(ref blankImagePath, value); } }

        public bool DisplayStartup { get { return displayStartup; } set { SetProperty(ref displayStartup, value); } }

        public bool DisplayNumberPadPage { get => displayNumberPadPage; set { SetProperty(ref displayNumberPadPage, value); } }

        public bool DispCancelOrder { get { return dispCancelOrder; } set { SetProperty(ref dispCancelOrder, value); } }

        public string MsgCancelOrder { get { return msgCancerOrder; } set { SetProperty(ref msgCancerOrder, value); } }

        public bool CanPay { get { return canPay; } set { SetProperty(ref canPay, value); } }

        public string CheckMessage { get => checkMessage; set { SetProperty(ref checkMessage, value); } }

        public bool DisplayCheckMessagePage { get => displayCheckMessagePage; set { SetProperty(ref displayCheckMessagePage, value); } }
        public string CheckMessageTitle { get => checkMessageTitle; set { SetProperty(ref checkMessageTitle, value); } }
        public string CheckImagePath { get => checkImagePath; set { SetProperty(ref checkImagePath, value); } }

        public string EFTWorkStage { get => eFTWorkStage; set { SetProperty(ref eFTWorkStage, value); } }
        public bool Collapsed { get => mCollapsed; set { SetProperty(ref mCollapsed, value); } }

        public DownloadService DownloadService { get => downloadService; set => downloadService = value; }
        

        public bool DisplayConnectionCode { get => displayConnectionCode; set { SetProperty(ref displayConnectionCode, value); } }
        public string ConnectionMessage { get => connectionMessage; set { SetProperty(ref connectionMessage, value); } }
        public string ConnectionMessageTitle { get => connectionMessageTitle; set { SetProperty(ref connectionMessageTitle, value); } }

        public SelfOrderSettingClass SelfOrderSetting { get => selfOrderSetting; set => selfOrderSetting = value; }
        public UploadService UploadService { get => uploadService; set => uploadService = value; }
        public int Admin { get => admin; set => admin = value; }
        public List<string> PagesZIndex { get => pagesZIndex; set => pagesZIndex = value; }
        public string TableNumber { get => tableNumber; set { SetProperty(ref tableNumber, value); } }

        public bool DisplayTableNumberPage { get => displayTableNumberPage; set { SetProperty(ref displayTableNumberPage, value); } }
        public string Caption_BuzzerPage { get => caption_BuzzerPage; set { SetProperty(ref caption_BuzzerPage, value); } }

        public bool DisplayCoverPage { get => displayCoverPage; set { SetProperty(ref displayCoverPage, value); } }
        public string Message_BuzzerPage { get => message_BuzzerPage; set { SetProperty(ref message_BuzzerPage, value); } }

        public int NumerPadHeight { get => numerPadHeight; set => numerPadHeight = value; }
        public int RowCount { get => rowCount; set => rowCount = value; }
        public string Cover { get => cover; set { SetProperty(ref cover, value); } }

        public bool DisplayMemberCardPage { get => displayMemberCardPage; set { SetProperty(ref displayMemberCardPage, value); } }
        public string Caption_MemberCardPage { get => caption_MemberCardPage; set { SetProperty(ref caption_MemberCardPage, value); } }
        public string Message_MemberCardPage { get => message_MemberCardPage; set { SetProperty(ref message_MemberCardPage, value); } }

        public string Caption_CoverPage { get => caption_CoverPage; set { SetProperty(ref caption_CoverPage, value); } }
        public string Message_CoverPage { get => message_CoverPage; set { SetProperty(ref message_CoverPage, value); } }
        public string MemberCard { get => memberCard; set { SetProperty(ref memberCard, value); } }

        public bool DisplayOrderTypePage { get => displayOrderTypePage; set { SetProperty(ref displayOrderTypePage, value); } }
        public string Caption_OrderTypePage { get => caption_OrderTypePage; set { SetProperty(ref caption_OrderTypePage, value); } }
        public string Message_OrderTypePage { get => message_OrderTypePage; set { SetProperty(ref message_OrderTypePage, value); } }
        public string DealOrderType { get => dealOrderType; set { SetProperty(ref dealOrderType, value); } }

        public string TimeOutMessage { get => timeOutMessage; set { SetProperty(ref timeOutMessage, value); } }

        public int ColumnCount { get => columnCount; set => columnCount = value; }

        private PosPage CurrentPage = PosPage.Startup;

        private void OrderTypeCommandAction(string str)
        {
            StartTimeoutTimer();
            if (str == "1")
                DealOrderType = ((int)OrderType.DINING).ToString();
            else if (str == "2")
                DealOrderType = ((int)OrderType.TAKEWAY).ToString();
            else if (str == "3")
                DealOrderType = ((int)OrderType.UBEREAT).ToString();

            NextPage();
        }
        private void ButtonCommandAction(string str)
        {
            // Debug.WriteLine("ButtonCommandAction "+str);
            StartTimeoutTimer();
            if (str == "C")
                if (CurrentPage == PosPage.TableNumberPage) TableNumber = "";
                else if (CurrentPage == PosPage.MemberPage) MemberCard += "";
                else Cover = "";
            else if (str == "B")
            {
                if (CurrentPage == PosPage.TableNumberPage)
                {
                    if (TableNumber.Length > 0)
                        TableNumber = TableNumber.Substring(0, TableNumber.Length - 1);
                }
                else if (CurrentPage == PosPage.MemberPage)
                {
                    if (MemberCard.Length > 0)
                        MemberCard = MemberCard.Substring(0, MemberCard.Length - 1);
                }
                else if (CurrentPage == PosPage.CoverPage)
                    if (Cover.Length > 0)
                        Cover = Cover.Substring(0, Cover.Length - 1);

            }
            else if (str == "E")
            {
                if (CurrentPage == PosPage.TableNumberPage)
                {
                    if (TableNumber != "")
                        NextPage();
                }
                else if (CurrentPage == PosPage.MemberPage)
                {
                    NextPage();
                }
                else if (CurrentPage == PosPage.CoverPage)
                    NextPage();
            }
            else
            {
                if (CurrentPage == PosPage.TableNumberPage)
                    TableNumber += str;
                else if (CurrentPage == PosPage.MemberPage)
                    MemberCard += str;
                else
                    Cover += str;

            }
        }
        public void StartImgClick(Object sender, MouseButtonEventArgs e)
        {
            //   Trace.TraceInformation( e.GetPosition(sender as IInputElement).ToString() );

            Point clickPoint = e.GetPosition(sender as IInputElement);
            Debug.WriteLine("StartImgClick:" + clickPoint);
            if ((clickPoint.X >= 0) && (clickPoint.X <= 150) && (clickPoint.Y <= 150))
            {
                if (Admin == 0)
                    Admin = 1;

                StartTimeoutTimer();
            }
            else if ((clickPoint.X >= (WindowWidth - 150)) && (clickPoint.Y <= 150))
            {
                if (Admin == 1)
                    Admin = 2;
                StartTimeoutTimer();
            }
            else if ((clickPoint.X >= (WindowWidth / 2 - 120)) && (clickPoint.X <= ((WindowWidth / 2) + 120)) && (clickPoint.Y >= 280) && (clickPoint.Y <= 420))
            {
                if (Admin == 2)
                {
                    StoptimeoutTimer();
                    FormAdmin formadmin = new FormAdmin();
                    FormAdminVM vm = new FormAdminVM();
                    vm.TheStation = TheStation;
                    vm.SelfOrderSetting = SelfOrderSetting;
                    vm.EftCtrl = this.eftCtrl;

                    formadmin.Closed += (s, e) =>
                    {
                        Debug.WriteLine("formadmin closed ");
                        Admin = 0;
                    };

                    formadmin.DataContext = vm;
                    formadmin.Show();
                    vm.VM_Start();
                }
            }
            else
            {
                Admin = 0;
                // DisplayPage(PosPage.TableNumberPage);
                Cover = "";
                TableNumber = "";
                dealOrderType = PRPosUtils.DefaultOrderType;
                NextPage();
            }
        }
        private void PreviousPage()
        {
            if (CurrentPage == PosPage.OrderTypePage)
            {
                DoCancelOrderYes();
            }
            else if (CurrentPage == PosPage.TableNumberPage)
            {
                if (PRPosUtils.Ask_OrderType == "Y")
                    DisplayPage(PosPage.OrderTypePage);
                else
                    DoCancelOrderYes();
            }
            else if (CurrentPage == PosPage.CoverPage)
            {
                if ((DealOrderType == ((int)OrderType.DINING).ToString()))
                {
                    if ((PRPosUtils.Ask_Table_Number == "1") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (PRPosUtils.Ask_OrderType == "Y")
                        DisplayPage(PosPage.OrderTypePage);
                    else
                        DoCancelOrderYes();
                }
                else
                {
                    if ((PRPosUtils.Ask_Table_Number == "2") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (PRPosUtils.Ask_OrderType == "Y")
                        DisplayPage(PosPage.OrderTypePage);
                    else
                        DoCancelOrderYes();
                }
            }
            else if (CurrentPage == PosPage.MemberPage)
            {
                if ((DealOrderType == ((int)OrderType.DINING).ToString()))
                {
                    if ((PRPosUtils.Ask_Covers == "1") || (PRPosUtils.Ask_Covers == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if ((PRPosUtils.Ask_Table_Number == "1") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (PRPosUtils.Ask_OrderType == "Y")
                        DisplayPage(PosPage.OrderTypePage);
                    else
                        DoCancelOrderYes();
                }
                else
                {
                    if ((PRPosUtils.Ask_Covers == "2") || (PRPosUtils.Ask_Covers == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if ((PRPosUtils.Ask_Table_Number == "2") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (PRPosUtils.Ask_OrderType == "Y")
                        DisplayPage(PosPage.OrderTypePage);
                    else
                        DoCancelOrderYes();
                }
            }
        }
        private void NextPage()
        {
            if (CurrentPage == PosPage.Startup)
            {
                StartTimeoutTimer();
                if ((PRPosUtils.Ask_OrderType == "Y"))
                {
                    DisplayPage(PosPage.OrderTypePage);
                }
                else
                {
                    DealOrderType = PRPosUtils.DefaultOrderType;

                    if ((PRPosUtils.Ask_Table_Number == "1") && (DealOrderType == "1"))                                               
                        DisplayPage(PosPage.TableNumberPage);
                    else if ((PRPosUtils.Ask_Table_Number == "2") && (DealOrderType == "2"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (  (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if ((PRPosUtils.Ask_Covers == "1") && (DealOrderType == "1"))
                        DisplayPage(PosPage.CoverPage);
                    else if ((PRPosUtils.Ask_Covers == "2") && (DealOrderType == "2") )
                        DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Covers == "3")
                         DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Member_Card == "1")
                        DisplayPage(PosPage.MemberPage);
                    else
                        PrepareOrder();
                }
            }
            else if (CurrentPage == PosPage.OrderTypePage)
            {
                if ((DealOrderType == ((int)OrderType.DINING).ToString()))
                {

                    if ((PRPosUtils.Ask_Table_Number == "1") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if ((PRPosUtils.Ask_Covers == "1") || (PRPosUtils.Ask_Covers == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Member_Card == "1")
                        DisplayPage(PosPage.MemberPage);
                    else
                        PrepareOrder();
                }
                else
                {

                    if ((PRPosUtils.Ask_Table_Number == "2") || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.TableNumberPage);
                    else if (PRPosUtils.Ask_Covers == "2" || (PRPosUtils.Ask_Table_Number == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Member_Card == "1")
                        DisplayPage(PosPage.MemberPage);
                    else
                        PrepareOrder();
                }
            }
            else if (CurrentPage == PosPage.TableNumberPage)
            {
                if ((DealOrderType == ((int)OrderType.DINING).ToString()))
                {
                    if ((PRPosUtils.Ask_Covers == "1") || (PRPosUtils.Ask_Covers == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Member_Card == "1")
                        DisplayPage(PosPage.MemberPage);
                    else
                        PrepareOrder();
                }
                else
                {
                    if ((PRPosUtils.Ask_Covers == "2") || (PRPosUtils.Ask_Covers == "3"))
                        DisplayPage(PosPage.CoverPage);
                    else if (PRPosUtils.Ask_Member_Card == "1")
                        DisplayPage(PosPage.MemberPage);
                    else
                        PrepareOrder();
                }
            }
            else if (CurrentPage == PosPage.CoverPage)
            {
                if ((PRPosUtils.Ask_Member_Card == "1"))
                    DisplayPage(PosPage.MemberPage);
                else PrepareOrder();

            }
            else
            {
                PrepareOrder();
            }
        }
        private List<string> ResetPage()
        {
            return new List<string>() { "1", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        }
        private void DisplayPage(PosPage page)
        {
            List<string> _pagesZIndex;

            switch (page)
            {
                case PosPage.Startup:

                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.Startup] = "10";
                    break;
                case PosPage.CancelOrderPage:
                    // _pagesZIndex = ResetPage();
                    _pagesZIndex = pagesZIndex;
                    _pagesZIndex[(int)PosPage.CancelOrderPage] = "10";

                    break;
                case PosPage.CheckEFTPOS:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.CheckEFTPOS] = "10";
                    break;
                case PosPage.CheckConnection:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.CheckConnection] = "10";
                    break;
                case PosPage.TableNumberPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.TableNumberPage] = "10";
                    _pagesZIndex[(int)PosPage.NumberPadPage] = "10";
                    _pagesZIndex[(int)PosPage.BottomLinePage] = "10";
                    Message_BuzzerPage = PRPosUtils.BuzzerPage_Message_Dinein;
                    Caption_BuzzerPage = PRPosUtils.BuzzerPage_Caption_Dinein;
                    break;
                case PosPage.CoverPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.CoverPage] = "10";
                    _pagesZIndex[(int)PosPage.NumberPadPage] = "10";
                    _pagesZIndex[(int)PosPage.BottomLinePage] = "10";
                    Message_CoverPage = PRPosUtils.CoverPage_Message;
                    Caption_CoverPage = PRPosUtils.CoverPage_Caption;
                    break;
                case PosPage.MemberPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.MemberPage] = "10";
                    _pagesZIndex[(int)PosPage.NumberPadPage] = "10";
                    _pagesZIndex[(int)PosPage.BottomLinePage] = "10";
                    Message_MemberCardPage = PRPosUtils.MemberCardPage_Message;
                    Caption_MemberCardPage = PRPosUtils.MemberCardPage_Caption;
                    break;
                case PosPage.TimeOutPage:
                    if (pagesZIndex[(int)PosPage.TableNumberPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)PosPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)PosPage.TimeOutPage] = "0";
                                GoToPage(PosPage.TableNumberPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)PosPage.CoverPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)PosPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)PosPage.TimeOutPage] = "0";
                                GoToPage(PosPage.CoverPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)PosPage.MemberPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)PosPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)PosPage.TimeOutPage] = "0";
                                GoToPage(PosPage.MemberPage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    else if (pagesZIndex[(int)PosPage.OrderTypePage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)PosPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)PosPage.TimeOutPage] = "0";
                                GoToPage(PosPage.OrderTypePage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    else
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)PosPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)PosPage.TimeOutPage] = "0";
                                GoToPage(PosPage.ItemsPage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    break;

                case PosPage.OrderTypePage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.OrderTypePage] = "10";
                    _pagesZIndex[(int)PosPage.BottomLinePage] = "10";
                    break;
                case PosPage.ItemsPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.ItemsPage] = "10";
                    break;
                default:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)PosPage.Startup] = "10";
                    break;
            }
            pagesZIndex = _pagesZIndex;
            CurrentPage = page;
            Debug.WriteLine(CurrentPage);
            OnPropertyChanged("");
            DisplayStartup = true;
            DisplayCheckMessagePage = true;
            DispCancelOrder = true;
            DisplayConnectionCode = true;
            DisplayTimeoutMsg = true;
            DisplayTableNumberPage = true;
            DisplayNumberPadPage = true;
            DisplayCoverPage = true;
            DisplayMemberCardPage = true;
            DisplayOrderTypePage = true;

            /*
        DisplayStartup = (page == PosPage.Startup);
        DisplayCheckMessage = (page == PosPage.CheckEFTPOS);
        DispCancelOrder = (page == PosPage.CancelOrder);
        DisplayConnectionCode = (page == PosPage.CheckConnection);
        DisplayTimeoutMsg = (page == PosPage.Timeout);
        DisplayTableNumber = (page == PosPage.TableNumberPage);*/
        }
        private void GoToPage(PosPage page)
        {
            CurrentPage = page;
            StoptimeoutTimer();
            StopmsgTimeoutTimer();
            OnPropertyChanged("");
            if (timeoutTimer != null)
            {
                var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
                timeoutTimer.Interval = new TimeSpan(0, 0, 0, seconds);
                timeoutTimer.Start();
            }
        }
        private void PrepareOrder()
        {
            this.CurrentDeal = new PSTrn01sClass();
            this.currentDeal.Order_type = DealOrderType;
            this.CurrentDeal.Ref_no = TableNumber;
            this.CurrentDeal.Person = Cover.Equals("") ? 0 : int.Parse(Cover);
            this.CurrentDeal.Card_no = MemberCard;            
            this.CurrentDeal.Opentime = DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
            this.CurrentDeal.Deal_code = "S";
            this.CurrentDeal.CustomerId = PRPosUtils.CustomerID;
            this.CurrentDeal.Store_Code = PRPosUtils.StoreCode;
            this.CurrentDeal.Pos_No = PRPosUtils.PosCode;
            this.CurrentDeal.AccDate = PRPosUtils.AccDate.ToString(PRPosUtils.DateFormat);
            LoadFastKey();
            this.bannerImageTimer.Start();

            DisplayPage(PosPage.ItemsPage);
        }
        #region LOADFASTKEY
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
                string salePrice = this.TheStation.Pricecolumn1;// PRPosUtils.SalePriceColumn.Equals("") ? "sprice" : PRPosUtils.SalePriceColumn;
                string takeawayPrice = this.TheStation.Pricecolumn2; //  PRPosUtils.TakeawayPriceColumn.Equals("") ? "sprice2" : PRPosUtils.SalePriceColumn;

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
                                mSelectedMenu  = menuItem;
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
        #endregion
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

        #region TimeOutTimer
        public void StartTimeoutTimer()
        {
            //if (!DisplayTimeoutMsg)
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
                var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
                timeoutTimer.Interval = new TimeSpan(0, 0, 0, seconds);
                timeoutTimer.Start();

                Debug.WriteLine("StartTimeoutTimer... " + seconds);
            }
        }
        private void timeoutTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timeoutTimer_Tick... ");
            DisplayPage(PosPage.TimeOutPage);
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
            MsgTimeoutCount = PRPosUtils.AlterDisplayTime;
            msgTimeoutTimer.Start();
        }
        private void StoptimeoutTimer()
        {
            if (timeoutTimer != null)
            {
                timeoutTimer.Stop();
            }
        }
        #endregion

        #region BANNER_TIMER
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
        private void StopBannerimageTimer()
        {
            if (bannerImageTimer != null)
            {
                bannerImageTimer.Stop();
            }
        }
        #endregion

        #region MSGTIMEOUT_TIMER
        private void msgTimeoutTimer_Tick(object sender, EventArgs e)
        {
            MsgTimeoutCount--;
            Debug.WriteLine("CountDowm_Tick... " + MsgTimeoutCount);
            if (this.MsgTimeoutCount <= 0)
            {
                DoClose();
            }
        }
        private void StopmsgTimeoutTimer()
        {
            if (msgTimeoutTimer != null)
            {
                msgTimeoutTimer.Stop();
            }
        }
        #endregion
        private void StopAllTimers()
        {
            StopBannerimageTimer();
            StoptimeoutTimer();
            StopmsgTimeoutTimer();
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
        public async Task ClickItem(PRPos.Data.FastKeyClass fkSelected)
        {
            //TimeOutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //waittimeout = false;
            StoptimeoutTimer();
            StopmsgTimeoutTimer();
            Debug.WriteLine("ClickItem " + fkSelected.Caption + " " + fkSelected.SoldOut);
            FormItem2022 formItem2022 = new FormItem2022();
            var vmitem2022 = new FormItem2022VM();
            vmitem2022.AddItem += (s, v, q, p, i) =>
            {
                Debug.WriteLine("vmitem2022 AddItem ");
                //
                int itemno = CurrentDeal.OrderItems.Count + 1;
                PRPos.Data.PSTrn02sClass txn2 = new PRPos.Data.PSTrn02sClass()
                {
                    CustomerId = CurrentDeal.CustomerId,
                    Store_Code = CurrentDeal.Store_Code,
                    Pos_No = CurrentDeal.Pos_No,
                    AccDate = CurrentDeal.AccDate,
                    Item_Code = s.PsItem["item_code"].ToString(),
                    Item_Name = s.ItemName,
                    Kitchen_Name = s.PsItem[PRPosUtils.KitchenOrderItemName].ToString(),
                    Kitchen_Memo = s.PsItem["kitchen_remark"].ToString(),
                    Printer_Name = s.PsItem["printer_name"].ToString(),
                    Item_No = itemno,
                    Variety_Code = v.Variety_code,
                    Size_Code = v.Size_code,
                    Variety_Caption = v.Caption,
                    Variety_Kitchen_name = v.Kitchen_name,                    
                    Qty = q,
                    CalQty=q,
                    Sprice = p,    
                    Goo_Price= p,
                    GST = s.GST,
                    Dis_Amt = 0,
                    Mis_Amt = 0,
                    Dis_Rate = 0,
                    Ht_price = 0,
                    Ht_Amt = 0,
                    Item_Type = "I",
                    ItemPicture = s.ButtonImgURI,
                    Amount = i * q,
                    FastKey = s
                };
                foreach (var m in v.ModifierSets)
                {
                    foreach (var detail in m.Modifiers)
                    {
                        if (detail.SelectedQty > 0)
                        {
                            int mNO = txn2.Modifiers.Count + 1;
                            PSTrn04sClass modifer = new PSTrn04sClass()
                            {
                                CustomerId = CurrentDeal.CustomerId,
                                Store_Code = CurrentDeal.Store_Code,
                                Pos_No = CurrentDeal.Pos_No,
                                AccDate = CurrentDeal.AccDate,
                                Item_Code = s.PsItem["item_code"].ToString(),
                                Item_No = mNO,
                                Variety_Code = m.Variety_code,
                                Modset_Code = m.ModSet_code,
                                Modifier_Code = detail.Modifier_code,
                                Sprice = detail.Sprice,                                
                                Qty = detail.InpQty*q,
                                InpQty = detail.InpQty,
                                CalQty = detail.InpQty * q,
                                CalSprice = detail.InpQty * q * detail.Sprice,
                                Amount = detail.SelectedQty * detail.Sprice,
                                Caption = detail.Caption,
                                Caption_fn = detail.Caption_fn,
                                Kitchen_Name= detail.KitchenName,
                            };
                            txn2.Modifiers.Add(modifer);
                        }
                    }
                }

                CurrentDeal.OrderItems.Add(txn2);
                UpdateTotal();
                formItem2022.Close();
                StartTimeoutTimer();
            };
            vmitem2022.VMClose += (e) =>
            {
                if (e == Interfaces.TimeOutStatus.None)
                    StartTimeoutTimer();
            };
            formItem2022.TimeOut += (s, e) =>
            {
                StopAllTimers();
                DisplayPage(PosPage.Startup);
                Debug.WriteLine("formItem2022 Timeout ");
            };
            formItem2022.Closed += (s, e) =>
             {
                 Debug.WriteLine("formItem2022 closed ");
                 // StartTimeoutTimer();
             };
            vmitem2022.SelectedItem = fkSelected;
            formItem2022.DataContext = vmitem2022;

            formItem2022.Show();
            // Debug.WriteLine("formItem2022 Show ");
            /*
           try
           {
               if (fkSelected != null)
               {

                   form_item2020v2 itemForm = new form_item2020v2();
                   //itemForm.ReturnValueCallback += ItemForm_ReturnValueCallback;
                   itemForm.ItemNo = fkSelected.Selected;
                   PRPosUtils.OpenForms.Push(itemForm);
                   itemForm.ShowDialog();
                   if (PRPosUtils.OpenForms.Count > 0)
                       if (PRPosUtils.OpenForms.Peek() == itemForm)
                           PRPosUtils.OpenForms.Pop();
                   itemForm = null;
                   //updateTotal();
                   await UpdateTotal();
                   StartTimeoutTimer();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ClickItem : " + ex.Message);
            }
            */
        }

        void CheckoutCommandAction()
        {
            Debug.WriteLine("CheckoutCommandAction ");

            StoptimeoutTimer();
            StopmsgTimeoutTimer();

            FormCart2022 form = new FormCart2022();
            FormCart2022VM vm = new FormCart2022VM();

            vm.CurrentDeal = this.CurrentDeal;
            vm.EftCtrl = this.eftCtrl;
            form.FormCart2022ViewModel = vm;
            vm.CloseTranscation += async (d) =>
            {
                StopAllTimers();

                DisplayPage(PosPage.Startup);
                Debug.WriteLine("Transcation Finished");
                App.log.Info("Transcation Finished ");

                await UploadService.CheckUpload().ContinueWith(async task =>
                {
                    if (task.Result == "0")
                    {
                        Debug.WriteLine("UploadService continue ");
                        App.log.Info("UploadService continue ");
                        await DownloadService.CheckDownload().ContinueWith(task =>
                        {
                            Debug.WriteLine("CheckDownload continue ");
                            App.log.Info("CheckDownload continue ");
                        });
                    }
                });
                form.Close();
            };
            vm.VMClose += (e) =>
            {
                if (e == Interfaces.TimeOutStatus.None)
                    StartTimeoutTimer();
            };
            form.TimeOut += (s, e) =>
            {
                StopAllTimers();
                DisplayPage(PosPage.Startup);
                Debug.WriteLine("FormCart2022 Timeout ");
            };
            
            form.Closed += async (s, e) =>
            {
                //StartTimeoutTimer();
                Debug.WriteLine("FormCart2022 closed ");
                await UpdateTotal();
            };


            form.Show();
            Debug.WriteLine("FormCart2022 Show ");

        }

        private async Task UpdateTotal()
        { 

            decimal amt = 0;
            foreach (var d in CurrentDeal.OrderItems)
            {
                decimal sprice = d.Sprice;
                decimal modifierprice = 0;
                foreach ( var m in d.Modifiers)
                {
                    //int qty = m.InpQty*d.Qty;
                    modifierprice += m.InpQty * m.Sprice;
                }
                amt += (modifierprice + sprice) * d.Qty;
            }
            TotalPrice = amt;
            this.ItemCount = CurrentDeal.OrderItems.Count;
            CanPay = ItemCount > 0;
          //  Debug.WriteLine(this.ItemCount + "," + CanPay);
        }

        public void DoCheckout()
        {

            //TimeOutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //waittimeout = false;
            string MarkupItem = "";
            string MarkupMessage = "";

            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                /*
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from markupitem where markup_type='A' and item_code not in ( select item_code from tmpti where code_type in ('I' ) )   order by disp_order ";
                cmd.Parameters.Clear();
                DataTable Markup_itemDT = new DataTable();
                da.Fill(Markup_itemDT);
                foreach (DataRow Markup_item in Markup_itemDT.Rows)
                {
                    MarkupItem = Markup_item["item_code"].ToString();
                    MarkupMessage = Markup_item["message"].ToString();
                    break;
                }
                Markup_itemDT.Dispose();
                Markup_itemDT = null;
            }
            if (MarkupItem.Equals(""))
            {
                ShowCart();
            }
            else
            {
                Form_markup formmarkupitem = new Form_markup();
                formmarkupitem.ItemNo = MarkupItem;
                formmarkupitem.Message = MarkupMessage;
                //formmarkupitem.ReturnValueCallback += Formkarkup_ReturnValueCallback;
                PRPosUtils.OpenForms.Push(formmarkupitem);
                var retcode = formmarkupitem.ShowDialog();
                if (PRPosUtils.OpenForms.Count > 0)
                    if (PRPosUtils.OpenForms.Peek() == formmarkupitem)
                        PRPosUtils.OpenForms.Pop();
                formmarkupitem.Dispose();
                formmarkupitem = null;
                if (retcode == DialogResult.Yes)
                {
                    UpdateTotal();
                }

                ShowCart();
                */
            }

        }
        private async Task DoMessageOK()
        {
            // DisplayTimeoutMsg = false;
            DisplayPage(PosPage.ItemsPage);
            StoptimeoutTimer();
            StopmsgTimeoutTimer();
            if (timeoutTimer != null)
            {
                var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
                timeoutTimer.Interval = new TimeSpan(0, 0, 0, seconds);
                timeoutTimer.Start();
            }
        }


        private async Task DoCancelOrderYes()
        {
            StopAllTimers();

            DisplayPage(PosPage.Startup);
        }
        private async Task DoCancelOrderNo()
        {
            DisplayPage(PosPage.ItemsPage);
            StartBannerImageTimer();
        }
        private async Task DoClose()
        {
            DisplayPage(PosPage.Startup);
            StopAllTimers();
        }
        private async Task DoCancelOrder()
        {
            //DisplayTimeoutMsg = false;
            //StopAllTimers();
            // MVVMHelpers.PRWindowsHelper.HideView("FormMainItemsView");
            StopBannerimageTimer();
            // DispCancelOrder = true;
            DisplayPage(PosPage.CancelOrderPage);
        }

        public MainWindowVM()
        {
            _syncContext = System.Threading.SynchronizationContext.Current;
            PagesZIndex = ResetPage();
            PagesZIndex[4] = "10";
            DisplayStartup = true;
            DisplayCheckMessagePage = true;
            DispCancelOrder = true;
            DisplayConnectionCode = true;
            DisplayTimeoutMsg = true;
            DisplayTableNumberPage = true;
            DisplayNumberPadPage = true;
            DisplayCoverPage = true;
            DisplayMemberCardPage = true;
            DisplayOrderTypePage = true;

            TimeOutMessage = "Do You Need " + PRPosUtils.WaitingTime + " More Seconds to Finish Order?";
            
             
            SelfOrderSetting = new SelfOrderSettingClass();
            SelfOrderSetting.ConnString = PRPosDB.cnStr;
            SelfOrderSetting.Security = new SecurityController();
            
            SelfOrderSetting.HostURL = PRPosUtils.HostURL;
 

            BannerImagePathList = new ObservableCollection<BannerItem>();
            this.ClickItemCmd = new DelegateCommand<PRPos.Data.FastKeyClass>(async (itm) => { await this.ClickItem(itm); }, ItemCommand_CanExecute);
            // this.CheckoutCmd = new RelayCommand(DoCheckout, CheckoutCmd_CanExecure );

            this.CancelOrderCmd = new DelegateCommand(() => DoCancelOrder());

            this.CancelOrderYesCmd = new DelegateCommand(() => DoCancelOrderYes());
            this.CancelOrderNoCmd = new DelegateCommand(() => DoCancelOrderNo());
            this.TimeOutPageMoreTimeNoCmd = new DelegateCommand(() =>
            {

                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                {
                    DoCancelOrderYes();
                };
                _syncContext.Post(methodDelegate, null);
            });
            this.GoBackCommand = new DelegateCommand(() =>
            {
                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                {
                    PreviousPage();
                };
                _syncContext.Post(methodDelegate, null);
            });
            /*
           this.TimeOutMessageOKCmd = new RelayCommand(() => DoMessageOK());
           this.TimeOutMessageNoCmd = new RelayCommand(() => DoCancelOrderYes());
           */
            this.MainMenuSelectChanged =
                new DelegateCommand<PRPos.Data.FastkeySet>(MainMenuSelectChangedAction);

            eftCtrl = new AxCsdEft();
            System.Windows.Forms.Integration.WindowsFormsHost host = 
                new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Child = eftCtrl;
            ((System.ComponentModel.ISupportInitialize)eftCtrl).BeginInit();
            OcxContent = new StackPanel
            {
                Children = { host }
            };
            ((System.ComponentModel.ISupportInitialize)eftCtrl).EndInit();

            Debug.WriteLine("MainWindowVM Created");
        }

        private bool CheckoutCmd_CanExecure()
        {
            return true;
        }
        public void DisposeAll()
        {
            StackPanel panel = (this.OcxContent as StackPanel);
            var host = (panel.Children[0] as System.Windows.Forms.Integration.WindowsFormsHost);
            eftCtrl.Dispose();
            eftCtrl = null;
            host.Dispose();
            host = null;
            panel = null;
            Debug.WriteLine("DisposeAll called");
        }
        private bool ItemCommand_CanExecute(PRPos.Data.FastKeyClass arg)
        {
            return true;
        }

        public async Task<int> CheckCode()
        {
            int ret = 0;
            //CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(SelfOrderSetting);
            //var task = checkCodeService.CheckCode(SelfOrderSetting.ConnectionCode);
            /*
            await checkCodeService.CheckCode(SelfOrderSetting.ConnectionCode).ContinueWith(task =>
            {
                Station st = task.Result;
                ret = st.StatusCode;
                if (st.StatusCode == 0)
                {
           
                    TheStation.Token = st.Token;
                    TheStation.Connection = st.Connection;
                    TheStation.Config_code = st.Config_code;
                    TheStation.Set_code = st.Set_code;
                    TheStation.IsAuth = st.IsAuth;
                    TheStation.Last_checked = st.Last_checked;
                    TheStation.SalePriceColumn = st.SalePriceColumn;
                    TheStation.TakeawayPriceColumn = st.TakeawayPriceColumn;
                    TheStation.CustomerID = st.CustomerID;
                    TheStation.Store_code = st.Store_code;
                    TheStation.Pos_code = st.Pos_code;
                    TheStation.Pos_name = st.Pos_name;
                    TheStation.Pricecolumn1 = st.Pricecolumn1;
                    TheStation.Pricecolumn2 = st.Pricecolumn2;
                    TheStation.Pricecolumn3 = st.Pricecolumn3;
                    TheStation.Pricecolumn4 = st.Pricecolumn4;
                    SelfOrderSetting.ConnectionCode = TheStation.Connection;
                    SelfOrderSetting.Token = TheStation.Token;
                    SelfOrderSetting.StoreCode = TheStation.Store_code;
                    SelfOrderSetting.PosCode = TheStation.Pos_code;
                }
                else
                {
                    // server error
                }
            });*/
            return ret;
        }
        public async Task<int> Procedure_DoCheckEFT()
        {
            /* check EFTPOS */
            DAL.EFTPOSTransRefDAL DAL = new DAL.EFTPOSTransRefDAL(PRPosDB.cnStr);
            DAL.EFTPOSTransRef lastRef = DAL.GetLastReference();

                        CheckMessageTitle = "EFTPOS Check";
            CheckMessage = "Checking EFTPOS status";
            DisplayPage(PosPage.CheckEFTPOS);

            TaskCompletionSource<EFTPOSResponse> tcs = new TaskCompletionSource<EFTPOSResponse>();
            EventHandler handler = null;
            handler = new EventHandler(delegate (Object o, EventArgs a)
            {
                EFTPOSResponse ret = new EFTPOSResponse();

                ret.ResponseCode = this.eftCtrl.ResponseCode;
                ret.ResponseText = this.eftCtrl.ResponseText;
                ret.ResponseType = this.eftCtrl.ResponseType;
                ret.Status = this.ConvertStatus(this.eftCtrl.ResponseCode);
                ret.LastTransTxnRef = this.eftCtrl.TxnRef;
                ret.AuthCode = this.eftCtrl.AuthCode;
                ret.TransType = (this.eftCtrl.TxnType.Equals("P") ? EFTPOSTransType.Purchase : this.eftCtrl.TxnType.Equals("B") ? EFTPOSTransType.Balance : this.eftCtrl.TxnType.Equals("R") ? EFTPOSTransType.Refund : EFTPOSTransType.Settlement);
                ret.TxnSucess = this.eftCtrl.LastTxnSuccess;
                ret.Sucess = this.eftCtrl.Success;
                ret.AccountType = this.eftCtrl.AccountType;
                ret.CardType = this.eftCtrl.CardType;
                ret.DateOfTxn = this.eftCtrl.Date;
                ret.CardName = this.eftCtrl.CardName;
                ret.Stan = this.eftCtrl.Stan;
                ret.TimeOfTxn = this.eftCtrl.Time;
                ret.DateExpiry = this.eftCtrl.DateExpiry;
                ret.Rrn = this.eftCtrl.Rrn;
                ret.PurchaseAnalysisData = this.eftCtrl.PurchaseAnalysisData;
                ret.DataField = this.eftCtrl.DataField;
                ret.Pan = this.eftCtrl.Pan;
                ret.AmtCash = this.eftCtrl.AmtCash;
                ret.AmtPurchase = this.eftCtrl.AmtPurchase;

                DateTime ttime = DateTime.Now;
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseCode = " + eftCtrl.ResponseCode);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseText = " + eftCtrl.ResponseText);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseType = " + eftCtrl.ResponseType);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "Status = " + eftCtrl.ResponseCode);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "TxnRef = " + eftCtrl.TxnRef);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "LastTxnSuccess = " + eftCtrl.LastTxnSuccess);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "Sucess = " + eftCtrl.Success);
                Debug.WriteLine("============================================================");

                eftCtrl.GetLastTransactionEvent -= handler;

                tcs.SetResult(ret);
            });
            eftCtrl.EnableErrorDialog = false;
            eftCtrl.GetLastTransactionEvent += handler;
            int ret = 1;
            eftCtrl.DoGetLastTransaction();


            await tcs.Task.ContinueWith(async task =>
            {
                var obj = task.Result;
                if (obj.Sucess)
                {
                    if ((lastRef.LastReference == "") || (lastRef.LastReference == obj.LastTransTxnRef))
                    {
                        CheckMessage = "System was restarted, EFTPOS connected.";
                    }
                    else if (obj.Status == Models.EFTPOSLastTransStatus.Approved)
                    {
                        CheckMessage = "System was restarted while doing an EFTPOS transaction. We checked the last transaction and it was accepted by the bank. Card was charged. ";
                    }
                    else if (obj.Status == Models.EFTPOSLastTransStatus.Unknown)
                        CheckMessage = "System was restarted while doing an EFTPOS transaction. Cannot determine if the last transaction was successful or not. ";
                    else if (obj.Status == Models.EFTPOSLastTransStatus.Cancelled)
                        CheckMessage = "System was restarted while doing an EFTPOS transaction.  We checked the last transaction and it was cancelled. ";
                    else if (obj.Status == Models.EFTPOSLastTransStatus.Declined)
                        CheckMessage = "System was restarted while doing an EFTPOS transaction.  We checked the last transaction and it was Declined. ";
                    else
                        CheckMessage = "System was restarted while doing an EFTPOS transaction. Cannot determine if the last transaction was successful or not. ";
                    DAL.SetLastReference(obj.LastTransTxnRef);
                    //await Task.Delay(3000);
                    ret = 0;
                }
                else
                {
                    CheckMessage = "System was restarted, got a non-success message while doing an EFTPOS Check.\n"+ eftCtrl.ResponseText;
                    //await Task.Delay(15000);
                }
            });
            return ret;
        }
        private async void Procedure_DoDownloadCheck(){
            CheckMessageTitle = "CHECK DOWNNLOAD";
            CheckMessage = "Download from Server";


            await DownloadService.CheckDownload().ContinueWith(task =>
            {
                Debug.WriteLine("DownloadService=>" + task.IsCompleted);
                //string downloaddata = task.Result;
                //Debug.WriteLine(downloaddata);
                CheckMessageTitle = "LOADING";
                CheckMessage = "Loading Banner Image";

                BannerImagePathList = GetBannersByCode("banner_image").Result;
                if (BannerImagePathList.Count > 0)
                {
                    this.CurrentBannerImagePath = BannerImagePathList[0].BannerImagePath;
                }
                if (bannerImageTimer == null)
                {
                    int bannerTime = PRPosUtils.BannerPlayTime != null ? PRPosUtils.BannerPlayTime : 10;
                    this.bannerImageTimer = new System.Windows.Threading.DispatcherTimer();
                    this.bannerImageTimer.Tick += new EventHandler(imageTimer_Tick);
                    this.bannerImageTimer.Interval = new TimeSpan(0, 0, bannerTime);
                }
                PRPosUtils.SalePriceColumn = TheStation.Pricecolumn1.ToString().Equals("") ? "sprice" : TheStation.Pricecolumn1.ToString();
                PRPosUtils.TakeawayPriceColumn = TheStation.Pricecolumn1.ToString().Equals("") ? "sprice2" : TheStation.Pricecolumn2.ToString();
                //Task.Delay(100);

                BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
                StartupImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_Start);
                CheckImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);

                CheckMessageTitle = "";
                CheckMessage = "";

                DisplayPage(PosPage.Startup);
            });
        }
        EventHandler handler = null;
        DispatcherTimer EFT_Check_DispatcherTimer = null;
        private async void EFT_Check_dispatcherTimer_Tick(object sender, EventArgs e)
        {
            EFT_Check_DispatcherTimer.Stop();
            var result =await Procedure_DoCheckEFT();
            if(result==1)
            {
                EFT_Check_DispatcherTimer.Interval = new TimeSpan(0, 0, 10);
                EFT_Check_DispatcherTimer.Start();
            }
            else
            {
                Procedure_DoDownloadCheck();
            }
        }
        // call from MainWindow.xaml.cs
        public async Task<int> BootingProcedure()
        {
            Debug.WriteLine("=======================Booting Procedure======================");
            PRPosDB.ReadParameterPart2();

            this.currentDeal = new PSTrn01sClass();
            // Check ConnectionCode
            DisplayPage(PosPage.CheckConnection);
            ConnectionMessageTitle = "Startup CHECK";
            ConnectionMessage = "Check Connection Code";

            DAL.StationBL stationBL = new DAL.StationBL();
            TheStation = stationBL.GetStation(SelfOrderSetting, new DAL.StationDAL());

            StoptimeoutTimer();
            StopmsgTimeoutTimer();
            /*
            while ( (TheStation.Connection == "") || (TheStation.Expiry_date< DateTime.Now))
            {
                ConnectionCode frmConnection = new ConnectionCode();
                var vm = new ConnectionCodeVM();
                vm.ConnectionCode = TheStation.Connection;
                vm.InputClose += async (e) =>
                {
                    string strconnection = (e as string);
                    Debug.WriteLine(strconnection);
                    frmConnection.Close();
                    Task.Delay(100);
                    SelfOrderSetting.ConnectionCode = strconnection;
                    int res = await CheckCode();                    
                };
                frmConnection.DataContext = vm;
                frmConnection.Show();
            }
            */
            SelfOrderSetting.Token = TheStation.Token;
            SelfOrderSetting.ConnectionCode = TheStation.Connection;
            SelfOrderSetting.CustomerID = TheStation.CustomerID;
            SelfOrderSetting.StoreCode = TheStation.Store_code;
            SelfOrderSetting.PosCode = TheStation.Pos_code;
            SelfOrderSetting.FilePath = PRPosUtils.FilePath;
            UploadService = new Services.UploadService(SelfOrderSetting);
            DownloadService = new Services.DownloadService(SelfOrderSetting);
                     
           
            EFT_Check_DispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            EFT_Check_DispatcherTimer.Tick += new EventHandler(EFT_Check_dispatcherTimer_Tick);
            EFT_Check_DispatcherTimer.Start();
           
            return 0;
        }        
    }
}
