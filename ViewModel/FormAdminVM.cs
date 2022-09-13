using AxCSDEFTLib;
using Newtonsoft.Json;
using PRPos.Data;
using SNSelfOrder.Interfaces;
using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SNSelfOrder.ViewModel
{
    enum AdminPage{   // order same xmal
        BackgroundPage,  //0
        PasswordPage,   
        AdminMenuPage,
        EftposPage,
        TranscationPage,
        XReportReadPage,
        ConnectionPage,
        ItemsPage,
        TotalAmountsPage,              
        NumberPage,        
        BottomPage,
        NUMBERPADKeyPage,
        ALPHAPADKeyPage,
        ConfirmDialogPage,
        TimeOutPage,              
    }
    enum KeyBoardPage
    {
        None,
        NumberKeyboardPage,
        AlphanumericKeyboardPage,
 
    }
    class FormAdminVM : ViewModelBase, ICloseWindows, IInputClose, ITimeOut
    {
        private System.Threading.SynchronizationContext _syncContext = null;
        private AdminPage CurrentPage = AdminPage.PasswordPage;
        private List<string> pagesZIndex = new List<string>() { "1", "10", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        private AxCSDEFTLib.AxCsdEft eftCtrl;
        private string backgroundImagePath;
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        private bool windowIsVisible = true;
        private int displayKeyboard = 0;
        private DelegateCommand goBackCommand;
        private int rowCount = 22;
        private string starRows = "18,19";
        private int columnCount = 3;
        private string txnListCaption = "Receipt Reprint";
        private  DateTime beginDate = DateTime.Today;
        private DateTime endDate = DateTime.Today;
        private string password = "";
        private string funcCaption = "Reprint Receipt";
        private string soldOutTitle = "Sold Out Items";
        private string timeOutMessage = "";
        private int msgTimeoutCount = 20;
        private int countDown = PRPosUtils.AlterDisplayTime;
        private int timeRemain = PRPosUtils.WaitingTime;
        private System.Windows.Threading.DispatcherTimer idleTimer;
        private System.Windows.Threading.DispatcherTimer msgTimeoutTimer;
        private string confirmMessageTitle = "";
        private string confirmMessage = "";
        //private int displayPage = 0;
        private string responseMessage = "";
        private string amountMessage = "";
        private string receiptNo = "";
        private string readMessageTitle = "";
        private string readContentMessage = "";
        private bool showJobPage = false;
        private string paymentMsg = "";
        private string mEFTPOSMessage = "";
        private string connectionCode = "";
        private string checkCodeResult = "";
        private ObservableCollection<PRPos.Data.PSTrn01sClass> orderList = null;

        private bool isModifierMode = false;
        private bool isYesShow = true;
        private bool isOKShow = false;
        private bool isNoShow = true;

        private ObservableCollection<PRPos.Data.FastkeySet> mMainMenu;
        public ObservableCollection<PRPos.Data.FastkeySet> MainMenu { get { return this.mMainMenu; } set { this.SetProperty(ref this.mMainMenu, value); } }
        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref backgroundImagePath, value); } }
        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }
        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }
        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }
        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }
        public bool WindowIsVisible { get => windowIsVisible; set { SetProperty(ref windowIsVisible, value); } }
        public int CountDown { get => countDown; set { SetProperty(ref countDown, value); } }
        public DelegateCommand GoBackCommand => goBackCommand ?? (goBackCommand = new DelegateCommand(GoBackCommandAction));

        public Action<TimeOutStatus> VMClose { get; set; }
        public int ColumnCount { get => columnCount; set { SetProperty(ref columnCount, value); } }
        public int RowCount { get => rowCount; set { SetProperty(ref rowCount, value); OnPropertyChanged("LastRow"); } }
        public int LastRow { get => rowCount - 1; }
        public string StarRows { get => starRows; set { SetProperty(ref starRows, value); } }
       
        

        private ICommand buttonCommand;
        public ICommand ButtonCommand => buttonCommand ?? (buttonCommand = new DelegateCommand<string>(ButtonCommandAction));

        private ICommand showTxnCommand;
        private ICommand keyboardCommand;
        private ICommand confirmYesCmd;
        private ICommand confirmNoCmd;
        private ICommand alphaNumericCommand;
        

        private ICommand shutdownCommand;
        private ICommand closeSystemCommand;
        private ICommand refundCommand;
        private ICommand connectionCommand;
        private ICommand rePrintCommand;
        private ICommand xReadCommand;
        private ICommand readXReportCommand;
        private ICommand zReadCommand;        
        private ICommand auditCommand;
        private ICommand getLastTxnCommand;
        private ICommand clearCommand;

        private ICommand doRePrintCommand;
        private ICommand doJobReprintCommand;
        private ICommand transcationListBox_ItemClicked;
        
        private ICommand itemSoldOutCommand;
        private ICommand clickItemCmd;
        private ICommand resetCmd;
        private ICommand saveCloseCmd;
        private ICommand switchModeCmd;
        private ICommand clickModifierCmd;

        private ICommand totalAmountCommand;

        private PRPos.Data.FastkeySet selectedMainMenu;
        private ObservableCollection<ModSet> modifierSetList;
        
        private ModSet selectedModifierItem;
        public PRPos.Data.FastkeySet SelectedMainMenu
        {
            get
            {
                return this.selectedMainMenu;
            }
            set
            {

                this.SetProperty(ref this.selectedMainMenu, value);
                StartIdleTimer();

#if _DEBUG
                if (value != null)
                    Debug.WriteLine("SelectedMainMenu " + value.Caption);
#endif
                //  this.SelectedMenu = MainMenu.FirstOrDefault(x => x.Sid == value.Sid && x.Default_yn == "Y");
                //   OnPropertyChanged("");
            }
        }

        public ICommand CalendarCommand { get; set; }
        public ICommand ItemsSelectChanged { get; set; }

        public ICommand ReadXReportCommand => readXReportCommand ?? (readXReportCommand = new DelegateCommand(ReadXZReportCommandAction));        

        public string Password { get => password; set { SetProperty(ref password, value); } }
        public Action<object> InputClose { get; set; }

        
        public string ConfirmMessageTitle { get => confirmMessageTitle; set { SetProperty(ref confirmMessageTitle, value); } }
        public string ConfirmMessage { get => confirmMessage; set { SetProperty(ref confirmMessage, value); } }
        public ICommand ConfirmYesCmd => confirmYesCmd ?? (confirmYesCmd = new DelegateCommand(ConfirmYesCmdAction));
        public ICommand ConfirmNoCmd => confirmNoCmd ?? (confirmNoCmd = new DelegateCommand(ConfirmNoCmdAction));

        public ICommand ConfirmOKCmd { get; set; }
        public ICommand CloseSystemCommand => closeSystemCommand ?? (closeSystemCommand = new DelegateCommand(CloseSystemCommandAction));

        public ICommand ShutdownCommand => shutdownCommand ?? (shutdownCommand = new DelegateCommand(ShutdownCommandAction));

        public ICommand GetLastTxnCommand => getLastTxnCommand ?? (getLastTxnCommand = new DelegateCommand(GetLastTxnCommandAction));
        public ICommand RePrintCommand => rePrintCommand ?? (rePrintCommand = new DelegateCommand(RePrintCommandAction));

        public ICommand KeyboardCommand=> keyboardCommand?? (keyboardCommand = new DelegateCommand(KeyboardCommandAction));
        public ICommand AlphaNumericCommand => alphaNumericCommand ?? (alphaNumericCommand = new DelegateCommand<string>(AlphaNumericCommandAction));
        public ICommand ShowTxnCommand => showTxnCommand ?? (showTxnCommand = new DelegateCommand(ShowTxnCommandAction));
        public ICommand ClearCommand => clearCommand ?? (clearCommand = new DelegateCommand(ClearCommandAction));
        public ICommand DoReprintCommand => doRePrintCommand ?? (doRePrintCommand = new DelegateCommand(DoReprintCommandAction));
        public ICommand DoJobReprintCommand => doJobReprintCommand ?? (doJobReprintCommand = new DelegateCommand(DoJobReprintCommandAction));

        public ICommand ConnectionCommand => connectionCommand ?? (connectionCommand = new DelegateCommand(ConnectionCommandAction));
        public ICommand RefundCommand  => refundCommand ?? (refundCommand = new DelegateCommand(RefundCommandAction));

        public ICommand ItemSoldOutCommand => itemSoldOutCommand ?? (itemSoldOutCommand = new DelegateCommand(ItemSoldOutCommandAction));
        
        public ICommand XReadCommand => xReadCommand ?? (xReadCommand = new DelegateCommand(XReadCommandAction));

        public ICommand ZReadCommand => zReadCommand ?? (zReadCommand = new DelegateCommand(ZReadCommandAction));

        public ICommand AuditCommand => auditCommand ?? (auditCommand = new DelegateCommand(AuditCommandAction));
        
        public ICommand TotalAmountCommand => totalAmountCommand ?? (totalAmountCommand = new DelegateCommand(TotalAmountCommandAction));
        public ICommand TranscationListBox_ItemClicked => transcationListBox_ItemClicked ?? (transcationListBox_ItemClicked = new DelegateCommand<object>(TranscationListBox_ItemClickedAction));

        //public int DisplayPage { get => displayPage; set { SetProperty(ref displayPage, value); } }
        public ICommand ClickItemCmd => clickItemCmd ?? (clickItemCmd = new DelegateCommand<PRPos.Data.FastKeyClass>(ClickItemCmdAction));

        public ICommand ResetCmd => resetCmd ?? (resetCmd = new DelegateCommand(resetCmdAction));
        public ICommand SaveCloseCmd => saveCloseCmd ?? (saveCloseCmd = new DelegateCommand(saveCloseCmdAction));

        public ICommand ClickModifierCmd => clickModifierCmd ?? (clickModifierCmd = new DelegateCommand<ModSetTi>( ClickModifierCmdAction ));

        public AxCsdEft EftCtrl { get => eftCtrl; set => eftCtrl = value; }
        public string ResponseMessage { get => responseMessage; set { SetProperty(ref responseMessage, value); } }

        public string AmountMessage { get => amountMessage; set { SetProperty(ref amountMessage, value); } }

        public string TxnListCaption { get => txnListCaption; set { SetProperty(ref txnListCaption, value); } }

        public DateTime  BeginDate { get => beginDate; set { SetProperty(ref beginDate, value); } }
        public DateTime EndDate { get => endDate; set { SetProperty(ref endDate, value); } }

        public int DisplayKeyboard { get => displayKeyboard; set { SetProperty(ref displayKeyboard, value); } }

        public string ReceiptNo { get => receiptNo; set { SetProperty(ref receiptNo, value); } }

        public ObservableCollection<PSTrn01sClass> OrderList { get => orderList; set { SetProperty(ref orderList, value); } }

        public string ReadMessageTitle { get => readMessageTitle; set { SetProperty(ref readMessageTitle, value); } }

        public string ReadContentMessage { get => readContentMessage; set { SetProperty(ref readContentMessage, value); } }

        public bool ShowJobPage { get => showJobPage; set { SetProperty(ref showJobPage, value); } }

        public string FuncCaption { get => funcCaption; set { SetProperty(ref funcCaption, value); } }

        public string PaymentMsg { get => paymentMsg; set { SetProperty(ref paymentMsg, value); } }

        private Station theStation;
        private SelfOrderSettingClass selfOrderSetting;

        public ICommand TimeOutPageMoreTimeYesCmd { get; set; }
        public ICommand TimeOutPageMoreTimeNoCmd { get; set; }
        public ICommand SwitchModeCmd => switchModeCmd ?? (switchModeCmd = new DelegateCommand(SwitchMode));

        public Station TheStation { get => theStation; set => theStation = value; }
        public SelfOrderSettingClass SelfOrderSetting { get => selfOrderSetting; set => selfOrderSetting = value; }
        public int MsgTimeoutCount { get => msgTimeoutCount; set { SetProperty(ref msgTimeoutCount, value); } }
        public List<string> PagesZIndex { get => pagesZIndex; set { SetProperty(ref pagesZIndex, value); } }

        public string TimeOutMessage { get => timeOutMessage; set { SetProperty(ref timeOutMessage, value); } }

        public string ConnectionCode { get => connectionCode; set { SetProperty(ref connectionCode, value); } }

        public string CheckCodeResult { get => checkCodeResult; set { SetProperty(ref checkCodeResult, value); } }

        public bool IsYesShow { get => isYesShow; set { SetProperty(ref isYesShow, value); } }
        public bool IsOKShow { get => isOKShow; set { SetProperty(ref isOKShow, value); } }
        public bool IsNoShow { get => isNoShow; set { SetProperty(ref isNoShow, value); } }

        public string EFTPOSMessage { get => mEFTPOSMessage; set { SetProperty(ref mEFTPOSMessage, value); } }

        public string SoldOutTitle { get => soldOutTitle; set { SetProperty(ref soldOutTitle, value); }  }

        public bool IsModifierMode { get => isModifierMode; set { SetProperty(ref isModifierMode, value); }  }

        
        public ModSet SelectedModifierItem { get => selectedModifierItem; set { SetProperty(ref selectedModifierItem, value); StartIdleTimer(); } }

        public ObservableCollection<ModSet> ModifierSetList { get => modifierSetList; set { SetProperty(ref modifierSetList, value); } }

        

        private void SwitchMode()
        {
            this.IsModifierMode = !this.IsModifierMode;
            StartIdleTimer(); 
        }


        #region TimeOutTimer

        public void ResetIdleTimer()
        {
            idleTimer.Stop();
            timeRemain = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 60;

            var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 60;
            idleTimer.Interval = new TimeSpan(0, 0, 0, seconds);
            idleTimer.Start();

            Debug.WriteLine("Start Timeout Timer... " + seconds);
        }

        public void StartIdleTimer()
        {
            //if (!DisplayTimeoutMsg)
            {
                if (idleTimer == null)
                {
                    idleTimer = new System.Windows.Threading.DispatcherTimer();
                    idleTimer.Tick += IdleTimeoutTimer_Tick;
                }
                else
                {
                    idleTimer.Stop();
                }
                ResetIdleTimer();
            }
        }
        private void IdleTimeoutTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timeoutTimer_Tick... ");
            PageDisplay(AdminPage.TimeOutPage);

            idleTimer.Stop();
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
        public void StopIdleTimer()
        {
            if (idleTimer != null)
            {
                idleTimer.Stop();
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
                StopAllTimers();
                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                {
                    VMClose?.Invoke(TimeOutStatus.None);
                };
                _syncContext.Post(methodDelegate, null);
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

        private void GoToPage(AdminPage page)
        {
            CurrentPage = page;
            StopIdleTimer();
            StopmsgTimeoutTimer();
            OnPropertyChanged("");
            StartIdleTimer();
        }
        private List<string> ResetPage()
        {
            return  new List<string>() { "1", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
        }
        
        private void PageDisplay(AdminPage page)
        {
            List<string> _pagesZIndex;

            switch (page)
            {
                case AdminPage.PasswordPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.PasswordPage] = "10";
                    _pagesZIndex[(int)AdminPage.NumberPage] = "10";
                    _pagesZIndex[(int) AdminPage.BottomPage] = "10";                    
                    break;
                case AdminPage.AdminMenuPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.AdminMenuPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";
                    break;
                case AdminPage.ItemsPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.ItemsPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";                    
                    break;
                case AdminPage.TotalAmountsPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.TotalAmountsPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";
                    break;
                case AdminPage.ConnectionPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.ConnectionPage] = "10";
                    _pagesZIndex[(int)AdminPage.NumberPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";
                    break;
                case AdminPage.ConfirmDialogPage:                    
                    _pagesZIndex = pagesZIndex;
                    _pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "10";
                    break;
                case AdminPage.TranscationPage:                    
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.TranscationPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";
                    break;
                case AdminPage.XReportReadPage:                    
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)AdminPage.XReportReadPage] = "10";
                    _pagesZIndex[(int)AdminPage.BottomPage] = "10";
                    break;
                case AdminPage.TimeOutPage:
                    if (pagesZIndex[(int)AdminPage.PasswordPage] =="10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.PasswordPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)AdminPage.AdminMenuPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.AdminMenuPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)AdminPage.TranscationPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {                                
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.TranscationPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)AdminPage.XReportReadPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {                                
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.XReportReadPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)AdminPage.ConnectionPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.ConnectionPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else if (pagesZIndex[(int)AdminPage.TotalAmountsPage] == "10")
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.TotalAmountsPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    else
                    {
                        _pagesZIndex = pagesZIndex;
                        _pagesZIndex[(int)AdminPage.TimeOutPage] = "10";
                        TimeOutPageMoreTimeYesCmd = new DelegateCommand(() =>
                        {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)AdminPage.TimeOutPage] = "0";
                                GoToPage(AdminPage.ConnectionPage);
                            };
                            _syncContext.Post(methodDelegate, null);

                        });
                    }
                    break;

                default:
                    _pagesZIndex = ResetPage();
                    break;
            }
            PagesZIndex = _pagesZIndex;
            CurrentPage = page;
            Debug.WriteLine(CurrentPage);
            OnPropertyChanged("");
            StartIdleTimer();
        }
        private void GoBackCommandAction()
        {
            
            if ((CurrentPage == AdminPage.PasswordPage) || (CurrentPage == AdminPage.AdminMenuPage))
            {
                StopAllTimers();
                VMClose?.Invoke(TimeOutStatus.None); 
            
            }
            else if (CurrentPage == AdminPage.TranscationPage)
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
            else if (CurrentPage == AdminPage.ConnectionPage)
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
            else if (CurrentPage == AdminPage.XReportReadPage)
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
            else if (CurrentPage == AdminPage.ItemsPage)
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
            else if (CurrentPage == AdminPage.TotalAmountsPage)
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
            else
            {
                PageDisplay(AdminPage.AdminMenuPage);
            }
        }
        private void ConfirmYesCmdAction() {
              Debug.WriteLine("ConfirmYesCmdAction ");
        }
 
        private void ConfirmNoCmdAction()
        {
            Debug.WriteLine("ConfirmNoCmdAction ");
        }

        public bool CanClose()
        {
            return true;
        }
        private void ShutdownCommandAction()
        {
            StartIdleTimer();
            //  DisplayPage = (int)AdminPage.ConfirmDialogPage;
            ConfirmMessageTitle = "Confirm";
            ConfirmMessage = "Do You Want To Shutdown System?";
            IsYesShow = true;
            IsNoShow = true;
            isOKShow = false;
            confirmYesCmd = new DelegateCommand(() => {
                StopAllTimers();
                VMClose?.Invoke(TimeOutStatus.SHUTDOWN);
            });
            confirmNoCmd = new DelegateCommand(() => {
                // DisplayPage = (int)AdminPage.AdminMenuPage;
                pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "0";
                OnPropertyChanged("");
            });
            pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "10";
            OnPropertyChanged("");
        }
        #region ADMIN_PAGE_COMMAND
        private void CloseSystemCommandAction()
        {
            StartIdleTimer();
            ConfirmMessageTitle = "Confirm";
            ConfirmMessage = "Do you want to Close System?";
            IsYesShow = true;
            IsNoShow = true;
            isOKShow = false;
            confirmYesCmd = new DelegateCommand(()=> {
                StopAllTimers();
                VMClose?.Invoke(TimeOutStatus.CLOSE_APP);
            });

            confirmNoCmd = new DelegateCommand(()=> {
                //DisplayPage = (int)AdminPage.AdminMenuPage;
                //PageDisplay(AdminPage.AdminMenuPage);
                pagesZIndex[(int) AdminPage.ConfirmDialogPage ] = "0";
                OnPropertyChanged("");
            });
            pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "10";
            OnPropertyChanged("");
        }
        private void GetLastTxnCommandAction()
        {
            StopAllTimers();
            ResponseMessage = "";
            EventHandler handler = null;
            handler = new EventHandler(delegate (Object o, EventArgs a)
            {               
                DateTime ttime = DateTime.Now;
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseCode = " + eftCtrl.ResponseCode);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseText = " + eftCtrl.ResponseText);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "ResponseType = " + eftCtrl.ResponseType);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "Status = " + eftCtrl.ResponseCode);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "TxnRef = " + eftCtrl.TxnRef);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "LastTxnSuccess = " + eftCtrl.LastTxnSuccess);
                Debug.WriteLine(ttime.ToString("yyyyMMdd HHmmss ") + "Sucess = " + eftCtrl.Success);
                Debug.WriteLine("============================================================");
                 responseMessage = "";
                responseMessage +=  "TIME="+ eftCtrl.Date+" "+ eftCtrl.Time+ "\n";
                responseMessage += "STAN = " + eftCtrl.Stan + "\n";
                responseMessage += "TXN REF = " + eftCtrl.TxnRef + "\n";
                responseMessage += "CARD Type = " + eftCtrl.CardType + "\n";
                responseMessage += "CARD NAME = " + eftCtrl.CardName + "\n";
                responseMessage += "ACCOUNT = " + eftCtrl.AccountType + "\n";
                responseMessage += "PURCHASE = " + eftCtrl.AmtPurchase + "\n";
                responseMessage += "ResponseCode = " + eftCtrl.ResponseCode + "\n";
                responseMessage += "ResponseText = " + eftCtrl.ResponseText + "\n";
                responseMessage += "ResponseType = " + eftCtrl.ResponseType + "\n";
                responseMessage += "Status = " + eftCtrl.ResponseCode + "\n";
                responseMessage += "LastTxnSuccess = " + eftCtrl.LastTxnSuccess + "\n";
                                
                eftCtrl.GetLastTransactionEvent -= handler;                
                OnPropertyChanged("");
                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                {
                    StartIdleTimer();
                };
                _syncContext.Post(methodDelegate, null);
            });
            eftCtrl.GetLastTransactionEvent += handler;
            int ret = 1;
            eftCtrl.DoGetLastTransaction();
           
        }

        private void RePrintCommandAction()
        {
            
            TxnListCaption = "Receipt Reprint";
            FuncCaption = "Reprint Receipt";
            PaymentMsg = "";
            OrderList = new ObservableCollection<PSTrn01sClass>();
            doRePrintCommand = new DelegateCommand(()=>{
                StopAllTimers();
                foreach (var txn in OrderList)
                {
                    if (txn.ItemSelected)
                    {
                        if (txn.Del_deal_no == "")
                        {
                            Helpers.ReceiptPrinter receiptPrinter = new Helpers.ReceiptPrinter();

                            receiptPrinter.SetPrinter(PRPosUtils.PosPrinters);
                            receiptPrinter.PrintingReceipt(txn, true);
                        }
                        else
                        {
                            IsYesShow = false;
                            IsNoShow = false;
                            isOKShow = true;
                            ConfirmMessageTitle = "Can Not Reprint!!";
                            ConfirmMessage = "This transcation is refunded";


                            ConfirmOKCmd = new DelegateCommand(() =>
                            {
                                // DisplayPage = (int)AdminPage.AdminMenuPage;
                                pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "0";
                                OnPropertyChanged("");
                                StartIdleTimer();
                            });
                            pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "10";
                            OnPropertyChanged("");
                            
                        }
                        StartIdleTimer();
                        break;
                    }
                }
                
            });
            // DisplayPage = (int)AdminPage.TranscationPage;
            PageDisplay(AdminPage.TranscationPage);
            OnPropertyChanged("");
            StartIdleTimer();
        }
        #endregion

        #region REPRINT_PAGE_COMMAND
        public void ClearCommandAction()
        {
            ReceiptNo = "";
            OrderList = new ObservableCollection<PSTrn01sClass>();
            StartIdleTimer();
        }
        private void DoReprintCommandAction()
        {
          
        }
        #endregion
        /* Password dailog */
        #region Password_button
        private async void ButtonCommandAction(string str)
        {
            // Debug.WriteLine("ButtonCommandAction "+str);
            if (str == "C")
            {
                if (CurrentPage == AdminPage.PasswordPage)
                    Password = "";
                else
                    ConnectionCode = "";
            }
            else if (str == "B")
            {
                if (Password.Length > 0)
                {
                    if (CurrentPage == AdminPage.PasswordPage)
                        Password = Password.Substring(0, Password.Length - 1);
                    else
                        ConnectionCode = ConnectionCode.Substring(0, ConnectionCode.Length - 1);
                }
            }
            else if (str == "E")
            {
                if (CurrentPage == AdminPage.PasswordPage)
                {
                    if (PRPosDB.ReadString("adminpassword").Equals(Password))
                        PageDisplay(AdminPage.AdminMenuPage);
                }
                else
                {
                    CheckCodeResult = "Check Code....";
                    string strconnection = ConnectionCode;
                    CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(SelfOrderSetting);

                    var task = await checkCodeService.LaunchCodeCheck(strconnection);

                    Station st = task;
                    Debug.WriteLine(st.StatusCode);
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

                        CheckCodeResult = "This Code Assigned to this KIOSK";
                    }
                    else
                    {
                        CheckCodeResult = "Error Code:"+ st.StatusCode;

                    }
                }
            }
            else
            {
                if (CurrentPage == AdminPage.PasswordPage)
                    Password += str;
                else
                    ConnectionCode += str;
            }
        }
        #endregion
        private void AlphaNumericCommandAction(string str)
        {
            if (str == "BS")
            {
                if ((CurrentPage == AdminPage.TranscationPage))
                {
                    if (ReceiptNo.Length > 0)
                        ReceiptNo = ReceiptNo.Substring(0, ReceiptNo.Length - 1);
                }
                else {
                    if (ConnectionCode.Length > 0)
                        ConnectionCode = ConnectionCode.Substring(0, ConnectionCode.Length - 1);
                }
            }
            else if (str == "&amp;")
            {
                if ((CurrentPage == AdminPage.TranscationPage))
                    ReceiptNo += "&";
            }
            else if (str == "&quot;")
            {
                if ((CurrentPage == AdminPage.TranscationPage))
                    ReceiptNo += "\"";
            }
            else if (str == "CHG")
            {
                /*
                if(DisplayKeyboard== (int) KeyBoardPage.NumberKeyboardPage)
                    DisplayKeyboard = (int) KeyBoardPage.AlphanumericKeyboardPage;
                else if (DisplayKeyboard == (int) KeyBoardPage.AlphanumericKeyboardPage)
                    DisplayKeyboard = (int)KeyBoardPage.NumberKeyboardPage;*/
                if (pagesZIndex[8] == "10")
                {
                    pagesZIndex[9] = "10";
                    pagesZIndex[8] = "0";
                }
                else
                {
                    pagesZIndex[8] = "10";
                    pagesZIndex[9] = "0";
                }
                OnPropertyChanged("");
            }
            else if (str != "")
            {
                if ((CurrentPage == AdminPage.TranscationPage))
                    ReceiptNo += str;
                else
                    ConnectionCode += str;
            }
            StopmsgTimeoutTimer();
        }       
        private void KeyboardCommandAction()
        {
            DisplayKeyboard = (int)KeyBoardPage.NumberKeyboardPage;
        }
        private void ShowTxnCommandAction()
        {
            orderList = new ObservableCollection<PSTrn01sClass>();
            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText =
                    @"select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and tdate between @accdate1 and @accdate2 
                       and deal_code='S'  " +
                      (ReceiptNo.Equals("") ? @"" : @"and deal_no=@deal_no ") +
                      " order by tdate,deal_no  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                if (!ReceiptNo.Equals(""))
                    cmd.Parameters.AddWithValue("deal_no", ReceiptNo);
                cmd.Parameters.AddWithValue("accdate1", DateTime.ParseExact( BeginDate.ToString(PRPosUtils.DateFormat+" " + PRPosUtils.TimeFormat ) , PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, null ) );
                cmd.Parameters.AddWithValue("accdate2", DateTime.ParseExact( EndDate.AddDays(1).AddMinutes(-1).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat), PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, null));
                DataTable psrtrn01sDT = new DataTable();
                da.Fill(psrtrn01sDT);
                foreach(DataRow pstrn01s in psrtrn01sDT.Rows)
                {
                    PSTrn01sClass trn01 = new PSTrn01sClass()
                    {
                        CustomerId = pstrn01s["cmp_no"].ToString(),
                        Store_Code = pstrn01s["str_no"].ToString(),
                        Pos_No = pstrn01s["pos_no"].ToString(),
                        AccDate = DateTime.Parse(pstrn01s["accdate"].ToString()).ToString(PRPosUtils.DateFormat),
                        Deal_No = pstrn01s["deal_no"].ToString(),

                        Tdate = DateTime.Parse(pstrn01s["tdate"].ToString()).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat),
                        Clerk_no = pstrn01s["clerk_no"].ToString(),
                        Deal_code = pstrn01s["deal_code"].ToString(),
                        Dis_amt = pstrn01s["dis_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["dis_amt"].ToString()),
                        Min_amt = pstrn01s["min_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["min_amt"].ToString()),
                        Over_amt = pstrn01s["over_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["over_amt"].ToString()),

                        Mms_amt = pstrn01s["mms_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["mms_amt"].ToString()),
                        Tot_amt = Decimal.Parse(pstrn01s["tot_amt"].ToString()),
                        Tax_amt = pstrn01s["tax_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["tax_amt"].ToString()),
                        Ntax_amt = pstrn01s["ntax_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["ntax_amt"].ToString()),
                        Ztax_amt = pstrn01s["ztax_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["ztax_amt"].ToString()),
                        Ht_amt = pstrn01s["ht_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn01s["ht_amt"].ToString()),

                        Card_no = pstrn01s["card_no"].ToString(),
                        Buss_no = pstrn01s["buss_no"].ToString(),

                        Close_yn = pstrn01s["close_yn"].ToString(),

                        Ref_no = pstrn01s["ref_no"].ToString(),
                        Service_no = pstrn01s["service_no"].ToString(),
                        Opener_no = pstrn01s["opener_no"].ToString(),
                        Order_type = pstrn01s["order_type"].ToString(),

                        Person = pstrn01s["person"].ToString().Equals("") ? 0 : int.Parse(pstrn01s["person"].ToString()),
                        Opentime = pstrn01s["sendtime"].ToString() == "" ? "" : DateTime.Parse(pstrn01s["opentime"].ToString()).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat),
                        Sendtime = pstrn01s["sendtime"].ToString()==""?"": DateTime.Parse(pstrn01s["sendtime"].ToString()).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat),
                        Closetime = pstrn01s["sendtime"].ToString() == "" ? "" : DateTime.Parse(pstrn01s["closetime"].ToString()).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat),

                        Uld_yn = pstrn01s["uld_yn"].ToString(),
                        Org_deal_no = pstrn01s["org_deal_no"].ToString(),
                        Del_deal_no = pstrn01s["del_deal_no"].ToString(),
                        Cnl_no = pstrn01s["cnl_no"].ToString(),
                        Cnl_time = pstrn01s["cnl_time"].ToString() == "" ? "" : DateTime.Parse(pstrn01s["cnl_time"].ToString()).ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat),
                    };
                    
                    cmd.CommandText =@"select * from pstrn02s where 1=1 and cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and pos_no=@pos_no 
                                        and accdate=@accdate and deal_no=@deal_no";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse( pstrn01s["accdate"].ToString()));
                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());                    
                    DataTable psrtrn02sDT = new DataTable();
                    da.Fill(psrtrn02sDT);
                    foreach (DataRow pstrn02s in psrtrn02sDT.Rows)
                    {
                        PSTrn02sClass trn02s = new PSTrn02sClass()
                        {
                            CustomerId = pstrn02s["cmp_no"].ToString(),
                            Store_Code = pstrn02s["str_no"].ToString(),
                            Pos_No = pstrn02s["pos_no"].ToString(),
                            AccDate = DateTime.Parse(pstrn02s["accdate"].ToString()).ToString(PRPosUtils.DateFormat ),
                            Deal_No = pstrn02s["deal_no"].ToString(),                            
                            Item_No = int.Parse(pstrn02s["item_no"].ToString()),
                            Item_Code = pstrn02s["item_code"].ToString(),
                            Size_Code = pstrn02s["size_code"].ToString(),
                            Item_Type = pstrn02s["item_type"].ToString(),
                            GST = pstrn02s["GST"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["GST"].ToString()),

                            Goo_Price = pstrn02s["goo_price"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["goo_price"].ToString()),
                            Sprice = pstrn02s["sprice"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["sprice"].ToString()), 
                            Qty = pstrn02s["qty"].ToString() == "" ? 0 : int.Parse(pstrn02s["qty"].ToString()),
                            Tax_Amt = pstrn02s["tax_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn02s["tax_amt"].ToString()),
                            Mis_Amt = pstrn02s["mis_amt"].ToString().Equals("") ? 0 : Decimal.Parse(pstrn02s["mis_amt"].ToString()),
                            Dis_Amt = pstrn02s["dis_amt"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["dis_amt"].ToString()),
                            Dis_Rate = pstrn02s["dis_rate"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["dis_rate"].ToString()),
                            Amount = pstrn02s["amt"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["amt"].ToString()),
                            Ht_price = pstrn02s["ht_price"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["ht_price"].ToString()),
                            Ht_Amt = pstrn02s["ht_amt"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["ht_amt"].ToString()),
                            Mms_mis =  pstrn02s["mms_mis"].ToString().Equals("") ? 0 : decimal.Parse(pstrn02s["mms_mis"].ToString()),
                            Mms_no = pstrn02s["mms_no"].ToString(),

                            Item_Name = pstrn02s["item_name"].ToString(),
                            Kitchen_Memo = pstrn02s["kitchen_memo"].ToString(),
                            Kitchen_Name = pstrn02s["kitchen_name"].ToString(),
                            Printer_Name = pstrn02s["printer_name"].ToString(),
                            Size_Caption = pstrn02s["size_caption"].ToString(),
                            Variety_Code = pstrn02s["variety_code"].ToString(),
                            Modset_Code = pstrn02s["modset_code"].ToString(),
                            Combo_Code = pstrn02s["combo_code"].ToString(),
                            Variety_Caption = pstrn02s["variety_caption"].ToString(),
                            Variety_Kitchen_name = pstrn02s["variety_kitchen_name"].ToString(),
                        };

                        cmd.CommandText =
                      @" select  pstrn04s.*        from pstrn04s
                           where pstrn04s.cmp_no=@cmp_no and pstrn04s.str_no=@str_no 
                           and pstrn04s.pos_no=@pos_no and pstrn04s.deal_no=@deal_no and accdate=@accdate and variety_code=@variety_code and item_code=@item_code
                           order by item_no ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn02s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn02s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn02s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("deal_no", pstrn02s["deal_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("variety_code", pstrn02s["variety_code"]);
                        cmd.Parameters.AddWithValue("item_code", pstrn02s["item_code"]);
                        DataTable pstrn04sDT = new DataTable();
                        da.Fill(pstrn04sDT);
                        foreach (DataRow pstrn04s in pstrn04sDT.Rows)
                        {
                            PSTrn04sClass trn04 = new PSTrn04sClass()
                            {
                                CustomerId = pstrn04s["cmp_no"].ToString(),
                                Store_Code = pstrn04s["str_no"].ToString(),
                                Pos_No = pstrn04s["pos_no"].ToString(),
                                AccDate = DateTime.Parse(pstrn04s["accdate"].ToString()).ToString(PRPosUtils.DateFormat ),
                                Deal_No = pstrn04s["deal_no"].ToString(),
                                Item_No = int.Parse(pstrn04s["item_no"].ToString()),
                                Item_Code = pstrn04s["item_code"].ToString(),
                                Variety_Code = pstrn04s["variety_code"].ToString(),
                                Modifier_Code = pstrn04s["modifier_code"].ToString(),
                                Modset_Code = pstrn04s["modset_code"].ToString(),
                                Caption = pstrn04s["caption"].ToString(),
                                Caption_fn = pstrn04s["caption_fn"].ToString(),
                                
                                Qty = pstrn04s["qty"].ToString().Equals("") ? 0 :  int.Parse(pstrn04s["qty"].ToString()),
                                Sprice = pstrn04s["sprice"].ToString().Equals("") ? 0 :  decimal.Parse(pstrn04s["sprice"].ToString()),
                                Amount = pstrn04s["amount"].ToString().Equals("") ? 0 : decimal.Parse(pstrn04s["amount"].ToString()), 
                            };
                            trn02s.Modifiers.Add(trn04);
                        }
                        trn01.OrderItems.Add(trn02s);
                    }
                    orderList.Add(trn01);
                }
            }

            OnPropertyChanged("OrderList");
            StartIdleTimer();
        }
        public void ItemsSelectChangedAction(object param)
        {
            StartIdleTimer();
            if (param != null)
                Debug.WriteLine("ItemsSelectChanged:" + ( param as PRPos.Data.PSTrn01sClass).Deal_No );
            else
                Debug.WriteLine("ItemsSelectChanged with null");
        }

        
        private  void ItemSoldOutCommandAction()
        {
            LoadFastKey();
            PageDisplay(AdminPage.ItemsPage);
            OnPropertyChanged("");
        }
        public void TranscationListBox_ItemClickedAction(object sender)
        {
            
            // click at deal info
            if (sender is PRPos.Data.PSTrn01sClass)
            {                
                var obj= (sender as PSTrn01sClass);


                PSTrn01sClass target;
                for (int i = 0; i < orderList.Count; i++)
                {
                    var txn = orderList[i];
                    if ((txn.AccDate == obj.AccDate) && (txn.Deal_No == obj.Deal_No) && (txn.Tdate == obj.Tdate))
                    {
                        target = orderList[i];
                        target.ItemSelected = true;
                        orderList.RemoveAt(i);
                        orderList.Insert(i, target);
                        
                    }
                    else
                    {
                        target = orderList[i];
                        if (target.ItemSelected)
                        {
                            target.ItemSelected = false;
                            orderList.RemoveAt(i);
                            orderList.Insert(i, target);
                        }
                    }
                }

                // var NewLists =  new ObservableCollection<PSTrn01sClass>();

                /*
               // Debug.WriteLine(obj.Deal_No + " " + obj.ItemSelected );
                for (int i = 0; i < OrderList.Count; i++)
                {
                    OrderList[i].ItemSelected = false;
                    NewLists.Add(OrderList[i]);
                    
                }

                 for (int i=0;i< NewLists.Count;i++)
                {
                    var txn = NewLists[i];                    
                    if ((txn.AccDate == obj.AccDate) && (txn.Deal_No == obj.Deal_No) && (txn.Tdate == obj.Tdate))
                    {                        
                        txn.ItemSelected = true;
                        
                        break;
                    }                    
                }
                OrderList = NewLists;
                */
                OnPropertyChanged("OrderList");
                StartIdleTimer();
            }
        }
        #region XREAD

        private void ReadXZReportToPrint(DateTime accDate, string typecode, string seq)
        {
            //CultureInfo ci = new CultureInfo(PRPosDB.ReadString("local_culture"),false);
            
            NumberFormatInfo nfi = (NumberFormatInfo)PRPosUtils.LocalCulture.NumberFormat.Clone();
            nfi.CurrencySymbol = PRPosUtils.CurrencySymbol;
            string output = "";
            using (MemoryStream ms = new MemoryStream())
            {
                using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
                {
                    connection.Open();
                    SQLiteCommand cmd = connection.CreateCommand();
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;


                    cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no   and tdate=@tdate and  ban=@ban";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                    cmd.Parameters.AddWithValue("tdate", accDate.Date);
                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);

                    da.SelectCommand = cmd;
                    DataTable psctrl01DT = new DataTable();
                    da.Fill(psctrl01DT);
                    if (psctrl01DT.Rows.Count > 0)
                    {
                        DataRow psctrl01 = psctrl01DT.Rows[0];

                        cmd.CommandText = "select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no  and accdate=@accdate and deal_code='S' order by deal_no";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                        cmd.Parameters.AddWithValue("accdate", accDate.Date);
                        DataTable pstrn01sDT = new DataTable();
                        da.Fill(pstrn01sDT);
                        int cnt = 0;
                        string minseq = "";
                        string maxseq = "";
                        decimal TotAmt = 0;
                        decimal TotTax = 0;

                        foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                        {

                            cnt += 1;
                            if (minseq.Equals(""))
                            {
                                minseq = pstrn01s["deal_no"].ToString();
                                maxseq = pstrn01s["deal_no"].ToString();
                            }
                            if (pstrn01s["deal_no"].ToString().CompareTo(maxseq) > 0)
                            {
                                maxseq = pstrn01s["deal_no"].ToString();
                            }
                            cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                            cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                            cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                            cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                            cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                            DataTable pstrn03sDT = new DataTable();
                            da.Fill(pstrn03sDT);
                            if (pstrn03sDT.Rows.Count > 0)
                            {
                                decimal amt = 0;
                                decimal tax = 0;
                                decimal.TryParse(pstrn01s["tot_amt"].ToString(), out amt);
                                decimal.TryParse(pstrn01s["tax_amt"].ToString(), out tax);
                                TotAmt += amt;
                                TotTax += tax;

                            }
                        }

                        StreamWriter ReadWriter = new StreamWriter(ms);

                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA | RP80Helper.DoubleWidth | RP80Helper.DoubleHeight));
                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextCenter));
                        RP80Helper.SetLineSpacing(ms, 100);
                        if (typecode.Equals("X"))
                            RP80Helper.SendStringToPrinter(ms, "X Read Report");
                        else if (typecode.Equals("Z"))
                            RP80Helper.SendStringToPrinter(ms, "Z Read & Reset Report");


                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetDefaultLineSpacing(ms);

                        if (!PRPosDB.ReadString("company_name").Equals(""))
                        {
                            RP80Helper.SendStringToPrinter(ms, PRPosDB.ReadString("company_name"));
                            //buffer = RP80Helper.SendStringToPrinter(PRPosDB.ReadString("company_name"));
                            //ms.Write(buffer, 0, buffer.Length);
                        }
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
                        RP80Helper.SendStringToPrinter(ms, "Station: " + PRPosUtils.PosCode + " No.: " + minseq + " to " + maxseq);

                        RP80Helper.SendStringToPrinter(ms, "Print at " + DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat));
                        RP80Helper.SendStringToPrinter(ms, "Account Date:" + accDate.ToString(PRPosUtils.DateFormat));
                        RP80Helper.LineFeed(ms, 1);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextLeft));
                        output = "";
                        for (int x = 0; x < 48; x++)
                        {
                            output += "=";
                        }

                        Dictionary<string, decimal> paymentlist = new Dictionary<string, decimal>();

                        RP80Helper.SendStringToPrinter(ms, output);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA | RP80Helper.Emphasized));
                        RP80Helper.SendStringToPrinter(ms, "Payment Summary".PadRight(38, ' ') + "Amount".PadLeft(10, ' '));
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                        {
                            cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                            cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                            cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                            cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                            cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                            DataTable pstrn03sDT = new DataTable();
                            da.Fill(pstrn03sDT);
                            foreach (DataRow p03 in pstrn03sDT.Rows)
                            {
                                decimal ecp_amt = 0;
                                decimal.TryParse(p03["ecp_amt"].ToString(), out ecp_amt);

                                if (!paymentlist.ContainsKey(p03["ecp_type"].ToString()))
                                {
                                    paymentlist.Add(p03["ecp_type"].ToString(), ecp_amt);
                                }
                                else
                                {
                                    paymentlist[p03["ecp_type"].ToString()] += ecp_amt;
                                }
                            }
                        }
                        decimal payamt = 0;
                        foreach (KeyValuePair<string, decimal> pay in paymentlist)
                        {
                            RP80Helper.SendStringToPrinter(ms, "  " + pay.Key.PadRight(36, ' ') + string.Format(nfi, "{0:C}", pay.Value).PadLeft(10, ' '));
                            payamt += pay.Value;
                        }

                        /*
                        cmd.CommandText = @"select ecp_type,sum(ecp_amt) as amt from pstrn03s 
                                           where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate and  deal_no between @deal_no1 and @deal_no2 
                                           group by  ecp_type";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", Common.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", Common.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", Common.PosCode);
                        cmd.Parameters.AddWithValue("accdate", accDate.Date);
                        cmd.Parameters.AddWithValue("deal_no1", minseq) ;
                        cmd.Parameters.AddWithValue("deal_no2", maxseq);
                        DataTable DTpstrn03s = new DataTable();
                        da.Fill(DTpstrn03s);
                        decimal payamt = 0;
                        foreach(DataRow pstrn03s in DTpstrn03s.Rows)
                        {
                            decimal amt = 0;
                            decimal.TryParse(pstrn03s["amt"].ToString(), out amt);
                            RP80Helper.SendStringToPrinter(ms, pstrn03s["ecp_type"].ToString().PadRight(38, ' ') + string.Format(nfi, "{0:C}", amt).PadLeft(10, ' '));
                            payamt += amt;
                        }*/

                        RP80Helper.SendStringToPrinter(ms, "Payment Total:".PadRight(38, ' ') + string.Format(nfi, "{0:C}", payamt).PadLeft(10, ' '));
                        output = "";
                        for (int x = 0; x < 48; x++)
                        {
                            output += "=";
                        }

                        RP80Helper.SendStringToPrinter(ms, output);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA | RP80Helper.Emphasized));
                        RP80Helper.SendStringToPrinter(ms, "Sale Taxes".PadRight(38, ' ') + "Amount".PadLeft(10, ' '));

                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        RP80Helper.SendStringToPrinter(ms, "  GST".PadRight(38, ' ') + string.Format(nfi, "{0:C}", TotTax).PadLeft(10, ' '));
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        output = "";
                        for (int x = 0; x < 48; x++)
                        {
                            output += "=";
                        }

                        RP80Helper.SendStringToPrinter(ms, output);
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA | RP80Helper.Emphasized));
                        // RP80Helper.SendStringToPrinter(ms, "   Items Types".PadRight(30, ' ') + "Quantity".PadRight(8, ' ') + "Amount".PadLeft(10, ' '));
                        RP80Helper.SendStringToPrinter(ms, "Items Types".PadRight(30, ' ') + "".PadRight(8, ' ') + "Amount".PadLeft(10, ' '));
                        RP80Helper.SetPrintMode(ms, (byte)(RP80Helper.FontA));
                        /*
                        cmd.CommandText = @" select case when pstrn02s.item_type='S' then pscategory1.cate_name else pscategory.cate_name  end as cate_name,sum(amt) as amt
                                           from pstrn02s
                                           left outer join psitem on psitem.item_code=pstrn02s.item_code and pstrn02s.item_type in ('C','I','E') 
                                           left outer join mealset on mealset.set_code=pstrn02s.item_code and pstrn02s.item_type in ('S') 
                                           left outer join pscategory  on psitem.cate_code= pscategory.cate_code
                                           left outer join pscategory as pscategory1 on mealset.cate_code= pscategory1.cate_code
                                           where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no 
                                           and pstrn02s.pos_no=@pos_no and pstrn02s.deal_no between @deal_no1 and @deal_no2 
                                           group by  case when pstrn02s.item_type='S'  then pscategory1.cate_name else pscategory.cate_name  end
                                          order by pscategory.seq ";
                                          */

                        Dictionary<string, string> categoryList = new Dictionary<string, string>();

                        Dictionary<string, decimal> categoryAmt = new Dictionary<string, decimal>();
                        cmd.CommandText = @" select customerid,cate_code,cate_name   from pscategory  where customerid=@customerid and pcate_code<>'' and del_flag='N' order by seq ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                        DataTable pscategoryDT = new DataTable();
                        da.Fill(pscategoryDT);

                        foreach (DataRow cate in pscategoryDT.Rows)
                        {
                            // Console.WriteLine(cate["cate_code"].ToString() +","+ cate["cate_name"].ToString() + ","+                            categoryList.ContainsKey(cate["cate_code"].ToString())+","+      categoryAmt.ContainsKey(cate["cate_code"].ToString()));
                            if (!categoryList.ContainsKey(cate["cate_code"].ToString()))
                            {
                                categoryList.Add(cate["cate_code"].ToString(), cate["cate_name"].ToString());
                            }
                            if (!categoryAmt.ContainsKey(cate["cate_code"].ToString()))
                            {
                                categoryAmt.Add(cate["cate_code"].ToString(), 0);
                            }
                        }

                        decimal saleamt = 0;

                        foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                        {
                            cmd.CommandText = @" select  pstrn02s.*,psitem.cate_code  from pstrn02s                                       
                                      left outer join psitem on psitem.item_code=pstrn02s.item_code and psitem.customerid= pstrn02s.cmp_no
                                       where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no  
                                       and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no=@deal_no  and pstrn02s.item_type in ('C','I','E')  ";


                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                            cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                            cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                            cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                            cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                            DataTable DTpstrn02s = new DataTable();
                            da.Fill(DTpstrn02s);

                            foreach (DataRow pstrn02s in DTpstrn02s.Rows)
                            {
                                decimal item_amt = 0;
                                decimal.TryParse(pstrn02s["amt"].ToString(), out item_amt);
                                // Console.WriteLine(pstrn02s["cate_code"].ToString()+" = " + categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString())) ;
                                if (categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString()))
                                {
                                    categoryAmt[pstrn02s["cate_code"].ToString()] += item_amt;
                                    saleamt += item_amt;
                                }
                            }
                        }
                        /*
                            cmd.CommandText = @" select  pscategory.cate_name ,sum(amt) as amt
                                           from pstrn02s
                                           left outer join psitem on psitem.item_code=pstrn02s.item_code and pstrn02s.item_type in ('C','I','E') 
                                           left outer join pscategory  on psitem.cate_code= pscategory.cate_code		                               
                                           where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no 
                                           and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no between @deal_no1 and @deal_no2 
                                           group by    pscategory.cate_name 
                                          order by pscategory.seq ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", Common.CustomerID);
                        cmd.Parameters.AddWithValue("str_no", Common.StoreCode);
                        cmd.Parameters.AddWithValue("pos_no", Common.PosCode);
                        cmd.Parameters.AddWithValue("accdate", accDate.Date);
                        cmd.Parameters.AddWithValue("deal_no1", minseq);
                        cmd.Parameters.AddWithValue("deal_no2", maxseq);
                        DataTable DTpstrn02s = new DataTable();
                        da.Fill(DTpstrn02s);
                        decimal saleamt = 0;
                        foreach(DataRow pstrn02s in DTpstrn02s.Rows)
                        {
                            decimal amt = 0;
                            decimal.TryParse(pstrn02s["amt"].ToString(), out amt);
                            RP80Helper.SendStringToPrinter(ms, pstrn02s["cate_name"].ToString().PadRight(38, ' ') + string.Format(nfi, "{0:C}", amt ).PadLeft(10, ' '));
                            saleamt += amt;
                        }*/
                        foreach (KeyValuePair<string, decimal> cateAmt in categoryAmt)
                        {
                            if (cateAmt.Value != 0)
                            {
                                RP80Helper.SendStringToPrinter(ms, "  " + categoryList[cateAmt.Key].ToString().PadRight(36, ' ') + string.Format(nfi, "{0:C}", cateAmt.Value).PadLeft(10, ' '));
                            }
                        }

                        RP80Helper.SendStringToPrinter(ms, "Item Type Totals:".PadRight(38, ' ') + string.Format(nfi, "{0:C}", saleamt).PadLeft(10, ' '));

                        RP80Helper.SetJustification(ms, (byte)(RP80Helper.TextLeft));
                        RP80Helper.LineFeed(ms, 5);
                        RP80Helper.Cut(ms);

                        ms.Seek(0, 0);

                        IntPtr pUnmanagedBytes = new IntPtr(0);
                        int nLength;

                        nLength = Convert.ToInt32(ms.Length);
                        byte[] buffer = new byte[nLength];
                        ms.Seek(0, 0);
                        int bytes = ms.Read(buffer, 0, nLength);
                        // Allocate some unmanaged memory for those bytes.
                        pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                        // Copy the managed byte array into the unmanaged array.
                        Marshal.Copy(buffer, 0, pUnmanagedBytes, nLength);
                        // Send the unmanaged bytes to the printer.

                        foreach (var posprinter in PRPosUtils.PosPrinters.Select(fld => new { fld.DeviceName, fld.DeviceType, fld.PrinterName, fld.Port }).Where(fld => fld.DeviceType == "Receipt Printer"))
                        {
                            RawPrinterHelper.SendBytesToPrinter(posprinter.DeviceName, pUnmanagedBytes, nLength);
                        }
                        // Free the unmanaged memory that you allocated earlier.
                        Marshal.FreeCoTaskMem(pUnmanagedBytes);

                        if (typecode.Equals("Z"))
                        {
                            foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                            {
                                cmd.CommandText = @"update pstrn01s set close_yn='Y'
                                            where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N'  
                                            and accdate=@accdate and deal_no=@deal_no  ";



                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                                cmd.ExecuteNonQuery();
                            }
                            // if(DateTime.Parse(psctrl01["tdate"].ToString()) < DateTime.Today)

                            cmd.CommandText = @"update psctrl01 set close_yn='Y' 
                                                where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' 
                                                and tdate=@tdate and ban=@ban ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                            cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                            cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                            cmd.Parameters.AddWithValue("tdate", accDate.Date);
                            cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                            cmd.ExecuteNonQuery();
                            PRPosDB.getDealNo();

                        }
                    }
 
                }
            }
        }

        private void ReadXZReportToPrintDocument(DateTime accDate, string typecode, string seq)
        {
           
            foreach (var printer in PRPosUtils.PosPrinters.Select(fld => new { fld.DeviceType, fld.DeviceName, fld.PrinterName, fld.IsDefault }).
                           Where(f => f.DeviceType == "Receipt Printer" && f.PrinterName != ""))
            {
                if (System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().Any(name => printer.DeviceName.Trim() == name.Trim()))
                {
                    System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                    pd.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
                    pd.PrinterSettings.PrinterName = printer.DeviceName;
                    pd.DocumentName = "Report of Kiosk";

                    SizeF layoutSize = new SizeF(pd.DefaultPageSettings.PaperSize.Width, pd.DefaultPageSettings.PaperSize.Height);
                    pd.PrintPage += (s, e) =>
                    {
                        NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                        nfi.CurrencySymbol = "$";

                        Font stringFont = new  Font(PRPosUtils.ReceiptItemItemFontFamily, PRPosUtils.ReceiptItemFontSize+1);

                         Font stringFont2 = new  Font(PRPosUtils.ReceiptItemItemFontFamily,   PRPosUtils.ReceiptItemFontSize+4 );

                        SizeF PriceSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("9", 5) + "." + PRPosUtils.repeatStr("9", PRPosUtils.ReceiptPriceDigitals), stringFont);
                        SizeF QtySize = e.Graphics.MeasureString(PRPosUtils.repeatStr("9", PRPosUtils.ReceiptItemQtyCharacters), stringFont);

                        int qtyWidth = QtySize.ToSize().Width;
                        int priceWidth = PriceSize.ToSize().Width;
                        int itemWidth = (int)layoutSize.Width - qtyWidth - priceWidth;

                        SizeF stringSize = new SizeF();
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;


                        Rectangle rect = e.PageBounds;
                        float linePadding = 0.0f;
                        float y = 0f;
                        float x = 0f;

                        using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
                        {
                            connection.Open();
                            SQLiteCommand cmd = connection.CreateCommand();
                            SQLiteDataAdapter da = new SQLiteDataAdapter();
                            da.SelectCommand = cmd;
                            cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no   and tdate=@tdate and  ban=@ban";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                            cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                            cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                            cmd.Parameters.AddWithValue("tdate", accDate.Date);
                            cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);

                            da.SelectCommand = cmd;
                            DataTable psctrl01DT = new DataTable();
                            da.Fill(psctrl01DT);
                            if (psctrl01DT.Rows.Count > 0)
                            {
                                DataRow psctrl01 = psctrl01DT.Rows[0];

                                cmd.CommandText =@"select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no  and accdate=@accdate 
                                                  and deal_code='S' order by deal_no";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                                cmd.Parameters.AddWithValue("accdate", accDate.Date);
                                DataTable pstrn01sDT = new DataTable();
                                da.Fill(pstrn01sDT);
                                int cnt = 0;
                                string minseq = "";
                                string maxseq = "";
                                decimal TotAmt = 0;
                                decimal TotTax = 0;

                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {

                                    cnt += 1;
                                    if (minseq.Equals(""))
                                    {
                                        minseq = pstrn01s["deal_no"].ToString();
                                        maxseq = pstrn01s["deal_no"].ToString();
                                    }
                                    if (pstrn01s["deal_no"].ToString().CompareTo(maxseq) > 0)
                                    {
                                        maxseq = pstrn01s["deal_no"].ToString();
                                    }
                                    cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                                    DataTable pstrn03sDT = new DataTable();
                                    da.Fill(pstrn03sDT);
                                    if (pstrn03sDT.Rows.Count > 0)
                                    {
                                        decimal amt = 0;
                                        decimal tax = 0;
                                        decimal.TryParse(pstrn01s["tot_amt"].ToString(), out amt);
                                        decimal.TryParse(pstrn01s["tax_amt"].ToString(), out tax);
                                        TotAmt += amt;
                                        TotTax += tax;

                                    }
                                }
                                string output = "";
                                if (typecode.Equals("X"))
                                    output = "X Read Report";
                                else if (typecode.Equals("Z"))
                                    output = "Z Read & Reset Report";

                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Center;                                
                                
                                stringSize = e.Graphics.MeasureString(output  , stringFont2, (int)layoutSize.Width, stringFormat);
                                e.Graphics.DrawString(output, stringFont2, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), 
                                    new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;
                                
                                if (!PRPosDB.ReadString("company_name").Equals(""))
                                {
                                    output = PRPosDB.ReadString("company_name");
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }
                                if (!PRPosDB.ReadString("buss_no").Equals(""))
                                {
                                    output = PRPosDB.ReadString("buss_no");
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }
                                if (!PRPosDB.ReadString("store_name").Equals(""))
                                {                                    
                                    output = PRPosDB.ReadString("store_name");
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }
                                if (!PRPosDB.ReadString("store_phone").Equals(""))
                                {
                                    output = PRPosDB.ReadString("store_phone");
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }

                                string addr1 = PRPosDB.ReadString("store_address_line1");
                                if (!addr1.Equals(""))
                                {
                                    output = addr1;
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }
                                addr1 = PRPosDB.ReadString("store_address_line2");
                                if (!addr1.Equals(""))
                                {
                                    output = addr1;
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }
                                addr1 = PRPosDB.ReadString("store_address_line3");
                                if (!addr1.Equals(""))
                                {
                                    output = addr1;
                                    stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                        new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                    y += stringSize.Height + linePadding;
                                }

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Station: " + PRPosUtils.PosCode + " No.: " + minseq + " to " + maxseq;
                                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                    new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;                                

                                output = "Print at " + DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                    new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;
                                
                                output = "Account Date:" + accDate.ToString(PRPosUtils.DateFormat);
                                stringSize = e.Graphics.MeasureString(output, stringFont, (int)layoutSize.Width, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y),
                                    new SizeF(rect.Width, stringSize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;
                                y += stringSize.Height + linePadding;

                                output = "";
                                stringFormat.Alignment = StringAlignment.Near;
                                stringFormat.LineAlignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont);
                                e.Graphics.DrawString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters ), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;
                              
                                output = "Payment Summary";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                output = "Amount";
                                SizeF _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);
                                
                                y += stringSize.Height + linePadding;
                                Dictionary<string, decimal> paymentlist = new Dictionary<string, decimal>();
                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {
                                    cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                                    DataTable pstrn03sDT = new DataTable();
                                    da.Fill(pstrn03sDT);
                                    foreach (DataRow p03 in pstrn03sDT.Rows)
                                    {
                                        decimal ecp_amt = 0;
                                        decimal.TryParse(p03["ecp_amt"].ToString(), out ecp_amt);

                                        if (!paymentlist.ContainsKey(p03["ecp_name"].ToString()))
                                        {
                                            paymentlist.Add(p03["ecp_name"].ToString(), ecp_amt);
                                        }
                                        else
                                        {
                                            paymentlist[p03["ecp_name"].ToString()] += ecp_amt;
                                        }
                                    }
                                }
                                decimal payamt = 0;
                                foreach (KeyValuePair<string, decimal> pay in paymentlist)
                                {
                                    stringFormat.Alignment = StringAlignment.Near;
                                    output = pay.Key;
                                    stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                    stringFormat.Alignment = StringAlignment.Far;
                                    output = string.Format(nfi, "{0:C}", pay.Value);
                                    _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                        new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                    y += stringSize.Height + linePadding;

                                    payamt += pay.Value;
                                }

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Payment Total:";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                output = string.Format(nfi, "{0:C}", string.Format(nfi, "{0:C}", payamt));
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;

                                stringFormat.Alignment = StringAlignment.Near;
                                stringFormat.LineAlignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont);
                                e.Graphics.DrawString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;


                                output = "Sale Taxes";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                output = "Amount";
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "GST:";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);
                                
                                stringFormat.Alignment = StringAlignment.Far;
                                output = string.Format(nfi, "{0:C}", string.Format(nfi, "{0:C}", TotTax));
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;

                                stringFormat.Alignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont);
                                e.Graphics.DrawString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;


                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Items Types";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                output = "Amount";
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;
                             

                                Dictionary<string, string> categoryList = new Dictionary<string, string>();

                                Dictionary<string, decimal> categoryAmt = new Dictionary<string, decimal>();
                                cmd.CommandText = @" select customerid,cate_code,cate_name   from pscategory  where customerid=@customerid and pcate_code<>'' and del_flag='N' order by seq ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                                DataTable pscategoryDT = new DataTable();
                                da.Fill(pscategoryDT);

                                foreach (DataRow cate in pscategoryDT.Rows)
                                {
                                    // Console.WriteLine(cate["cate_code"].ToString() +","+ cate["cate_name"].ToString() + ","+                            categoryList.ContainsKey(cate["cate_code"].ToString())+","+      categoryAmt.ContainsKey(cate["cate_code"].ToString()));
                                    if (!categoryList.ContainsKey(cate["cate_code"].ToString()))
                                    {
                                        categoryList.Add(cate["cate_code"].ToString(), cate["cate_name"].ToString());
                                    }
                                    if (!categoryAmt.ContainsKey(cate["cate_code"].ToString()))
                                    {
                                        categoryAmt.Add(cate["cate_code"].ToString(), 0);
                                    }
                                }

                                decimal saleamt = 0;

                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {
                                    cmd.CommandText = @" select  pstrn02s.*,psitem.cate_code  from pstrn02s                                       
                                      left outer join psitem on psitem.item_code=pstrn02s.item_code and psitem.customerid= pstrn02s.cmp_no
                                       where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no  
                                       and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no=@deal_no  and pstrn02s.item_type in ('C','I','E')  ";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                                    DataTable DTpstrn02s = new DataTable();
                                    da.Fill(DTpstrn02s);

                                    foreach (DataRow pstrn02s in DTpstrn02s.Rows)
                                    {
                                        decimal item_amt = 0;
                                        decimal.TryParse(pstrn02s["amt"].ToString(), out item_amt);
                                        // Console.WriteLine(pstrn02s["cate_code"].ToString()+" = " + categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString())) ;
                                        if (categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString()))
                                        {
                                            categoryAmt[pstrn02s["cate_code"].ToString()] += item_amt;
                                            saleamt += item_amt;
                                        }
                                    }
                                }
                                
                                foreach (KeyValuePair<string, decimal> cateAmt in categoryAmt)
                                {
                                    if (cateAmt.Value != 0)
                                    {

                                        

                                        stringFormat.Alignment = StringAlignment.Near;
                                        output = categoryList[cateAmt.Key].ToString();
                                        stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                        stringFormat.Alignment = StringAlignment.Far;
                                        output = string.Format(nfi, "{0:C}", cateAmt.Value);
                                        _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                            new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                        y += stringSize.Height + linePadding;
                                        
                                    }
                                }

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Item Type Totals:";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                output = string.Format(nfi, "{0:C}", saleamt);
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;

                                // refund deal
                                //
                                //
                                cmd.CommandText = @"select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no  and accdate=@accdate 
                                                  and deal_code='R' order by deal_no";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                                cmd.Parameters.AddWithValue("accdate", accDate.Date);
                                pstrn01sDT = new DataTable();
                                da.Fill(pstrn01sDT);

                                int refundcnt = 0;
                                decimal RefundTotAmt = 0;
                                decimal RefundTotTax = 0;

                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {

                                    refundcnt += 1;
                                    
                                    cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                                    DataTable pstrn03sDT = new DataTable();
                                    da.Fill(pstrn03sDT);
                                    if (pstrn03sDT.Rows.Count > 0)
                                    {
                                        decimal amt = 0;
                                        decimal tax = 0;
                                        decimal.TryParse(pstrn01s["tot_amt"].ToString(), out amt);
                                        decimal.TryParse(pstrn01s["tax_amt"].ToString(), out tax);
                                        RefundTotAmt += amt;
                                        RefundTotTax += tax;

                                    }
                                }

                                Dictionary<string, decimal> refundpaymentlist = new Dictionary<string, decimal>();
                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {
                                    cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                                    DataTable pstrn03sDT = new DataTable();
                                    da.Fill(pstrn03sDT);
                                    foreach (DataRow p03 in pstrn03sDT.Rows)
                                    {
                                        decimal ecp_amt = 0;
                                        decimal.TryParse(p03["ecp_amt"].ToString(), out ecp_amt);

                                        if (!refundpaymentlist.ContainsKey(p03["ecp_name"].ToString()))
                                        {
                                            refundpaymentlist.Add(p03["ecp_name"].ToString(), ecp_amt);
                                        }
                                        else
                                        {
                                            refundpaymentlist[p03["ecp_name"].ToString()] += ecp_amt;
                                        }
                                    }
                                }
                                decimal refundpayamt = 0;
                                foreach (KeyValuePair<string, decimal> pay in refundpaymentlist)
                                {
                                    stringFormat.Alignment = StringAlignment.Near;
                                    output = pay.Key;
                                    stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                    stringFormat.Alignment = StringAlignment.Far;
                                    output = string.Format(nfi, "{0:C}", pay.Value);
                                    _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                    e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                        new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                    y += stringSize.Height + linePadding;

                                    refundpayamt += pay.Value;
                                }
                                stringFormat.Alignment = StringAlignment.Near;
                                stringFormat.LineAlignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont);
                                e.Graphics.DrawString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Refund Payment Total:";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                output = string.Format(nfi, "{0:C}", string.Format(nfi, "{0:C}", refundpayamt));
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);
                                y += stringSize.Height + linePadding;

                                stringFormat.Alignment = StringAlignment.Near;
                                stringFormat.LineAlignment = StringAlignment.Near;
                                stringSize = e.Graphics.MeasureString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont);
                                e.Graphics.DrawString(PRPosUtils.repeatStr("=", PRPosUtils.ReceiptCharacters), stringFont, System.Drawing.Brushes.Black, new PointF(x, y));
                                y += stringSize.Height + linePadding;



                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Refund Items Types";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                output = "Amount";
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;


                                categoryList = new Dictionary<string, string>();

                                categoryAmt = new Dictionary<string, decimal>();
                                cmd.CommandText = @" select customerid,cate_code,cate_name   from pscategory  where customerid=@customerid and pcate_code<>'' and del_flag='N' order by seq ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                                 pscategoryDT = new DataTable();
                                da.Fill(pscategoryDT);

                                foreach (DataRow cate in pscategoryDT.Rows)
                                {
                                    // Console.WriteLine(cate["cate_code"].ToString() +","+ cate["cate_name"].ToString() + ","+                            categoryList.ContainsKey(cate["cate_code"].ToString())+","+      categoryAmt.ContainsKey(cate["cate_code"].ToString()));
                                    if (!categoryList.ContainsKey(cate["cate_code"].ToString()))
                                    {
                                        categoryList.Add(cate["cate_code"].ToString(), cate["cate_name"].ToString());
                                    }
                                    if (!categoryAmt.ContainsKey(cate["cate_code"].ToString()))
                                    {
                                        categoryAmt.Add(cate["cate_code"].ToString(), 0);
                                    }
                                }

                                decimal refundsaleamt = 0;

                                foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                {
                                    cmd.CommandText = @" select  pstrn02s.*,psitem.cate_code  from pstrn02s                                       
                                      left outer join psitem on psitem.item_code=pstrn02s.item_code and psitem.customerid= pstrn02s.cmp_no
                                       where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no  
                                       and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no=@deal_no  and pstrn02s.item_type in ('C','I','E')  ";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                    cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                    cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                    cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                    cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                                    DataTable DTpstrn02s = new DataTable();
                                    da.Fill(DTpstrn02s);

                                    foreach (DataRow pstrn02s in DTpstrn02s.Rows)
                                    {
                                        decimal item_amt = 0;
                                        decimal.TryParse(pstrn02s["amt"].ToString(), out item_amt);
                                        // Console.WriteLine(pstrn02s["cate_code"].ToString()+" = " + categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString())) ;
                                        if (categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString()))
                                        {
                                            categoryAmt[pstrn02s["cate_code"].ToString()] += item_amt;
                                            refundsaleamt += item_amt;
                                        }
                                    }
                                }

                                foreach (KeyValuePair<string, decimal> cateAmt in categoryAmt)
                                {
                                    if (cateAmt.Value != 0)
                                    {
 
                                        stringFormat.Alignment = StringAlignment.Near;
                                        output = categoryList[cateAmt.Key].ToString();
                                        stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                        stringFormat.Alignment = StringAlignment.Far;
                                        output = string.Format(nfi, "{0:C}", cateAmt.Value);
                                        _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                        e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                            new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                        y += stringSize.Height + linePadding;

                                    }
                                }

                                stringFormat.Alignment = StringAlignment.Near;
                                output = "Refund Item Types Totals:";
                                stringSize = e.Graphics.MeasureString(output, stringFont, itemWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black, new RectangleF(new PointF(x, y), new SizeF(itemWidth, stringSize.Height)), stringFormat);

                                stringFormat.Alignment = StringAlignment.Far;
                                output = string.Format(nfi, "{0:C}", refundsaleamt);
                                _pricesize = e.Graphics.MeasureString(output, stringFont, priceWidth, stringFormat);
                                e.Graphics.DrawString(output, stringFont, System.Drawing.Brushes.Black,
                                    new RectangleF(new PointF(itemWidth + qtyWidth, y), new SizeF(priceWidth, _pricesize.Height)), stringFormat);

                                y += stringSize.Height + linePadding;


                                if (typecode.Equals("Z"))
                                {
                                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                                    {
                                        cmd.CommandText = @"update pstrn01s set close_yn='Y'
                                            where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N'  
                                            and accdate=@accdate and deal_no=@deal_no  ";


                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                                        cmd.ExecuteNonQuery();
                                    }
                                    // if(DateTime.Parse(psctrl01["tdate"].ToString()) < DateTime.Today)

                                    cmd.CommandText = @"update psctrl01 set close_yn='Y' 
                                                where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' 
                                                and tdate=@tdate and ban=@ban ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);
                                    cmd.Parameters.AddWithValue("tdate", accDate.Date);
                                    cmd.Parameters.AddWithValue("ban", PRPosUtils.BAN);
                                    cmd.ExecuteNonQuery();
                                    PRPosDB.getDealNo();

                                }
                            }
                        }
                    };
                    pd.PrintController = new System.Drawing.Printing.StandardPrintController();
                    pd.Print();
                }
            }
        }
        private void ReadXZReportCommandAction() {
            Debug.WriteLine("ReadXCommandAction");
        }
        private void XReadCommandAction()
        {

            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;


                cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                da.SelectCommand = cmd;
                DataTable psctrl01DT = new DataTable();
                da.Fill(psctrl01DT);
                DataRow psctrl01 = psctrl01DT.Rows[0];
                ReadMessageTitle = " X Report Read";
                DateTime aDate;
                if (DateTime.TryParse(psctrl01["tdate"].ToString(), out aDate))
                    ReadContentMessage = "Accounting Date:" + aDate.ToString(PRPosUtils.DateFormat);
                this.readXReportCommand = new DelegateCommand(() =>
                {
                    ReadXZReportToPrintDocument(aDate, "X", psctrl01["seq"].ToString());
                    //ReadXZReportToPrint(aDate, "X", psctrl01["seq"].ToString());
                });
                //Debug.WriteLine("readXCommand");
                // DisplayPage = (int)AdminPage.XReportReadPage;
                PageDisplay(AdminPage.XReportReadPage);
                OnPropertyChanged("");
            }
        }
        private void ZReadCommandAction()
        {
            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;


                cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                da.SelectCommand = cmd;
                DataTable psctrl01DT = new DataTable();
                da.Fill(psctrl01DT);
                DataRow psctrl01 = psctrl01DT.Rows[0];
                ReadMessageTitle = " Z Report Read";
                DateTime aDate;
                if (DateTime.TryParse(psctrl01["tdate"].ToString(), out aDate))

                    ReadContentMessage = "Accounting Date:" + aDate.ToString(PRPosUtils.DateFormat);
                this.readXReportCommand = new DelegateCommand(() =>
                {
                    //Debug.WriteLine("readXCommand");
                    //ReadXZReportToPrint(aDate, "Z", psctrl01["seq"].ToString());

                    ReadXZReportToPrintDocument(aDate, "Z", psctrl01["seq"].ToString());

                    //DisplayPage = (int)AdminPage.AdminMenuPage;
                    PageDisplay(AdminPage.AdminMenuPage);
                });

               //   DisplayPage = (int)AdminPage.XReportReadPage;
                PageDisplay(AdminPage.XReportReadPage);
                OnPropertyChanged("");
            }
        }
        #endregion
        private void DoJobReprintCommandAction()
        {
            foreach (var txn in OrderList)
            {
                if (txn.ItemSelected)
                {
                    Helpers.PRPrint.PrintJobList(txn.CustomerId, txn.Store_Code, txn.Pos_No, txn.Deal_No, DateTime.Parse(txn.AccDate));
                }
            }
        }
        private void AuditCommandAction()
        {
            TxnListCaption = "KIOSK Audit";
            FuncCaption = "Reprint Job List";
            PaymentMsg = "";
            OrderList = new ObservableCollection<PSTrn01sClass>();
            doRePrintCommand = new DelegateCommand(() => {

                Debug.WriteLine("DoJobReprintCommandAction");
                foreach (var txn in OrderList)
                {
                    if (txn.ItemSelected)
                    {
                        Helpers.PRPrint.PrintJobList(txn.CustomerId, txn.Store_Code, txn.Pos_No, txn.Deal_No, DateTime.Parse(txn.AccDate));
                    }
                }
            });
            // DisplayPage = (int)AdminPage.TranscationPage;
            PageDisplay(AdminPage.TranscationPage);
            OnPropertyChanged("");
        }
        private void doRefundAction()
        {
            string ReceiptText = "";

            StopAllTimers();
            EventHandler handler = null;
            EventHandler displayHanlder = null;
            AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEventHandler PrintReceiptHandler = null;

            foreach (var txn in OrderList)
            {
                if (txn.ItemSelected)
                {
                    if (txn.Del_deal_no == "")
                    {
                        using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
                        {
                            cn.Open();
                            SQLiteCommand cmd = cn.CreateCommand();
                            SQLiteDataAdapter da = new SQLiteDataAdapter();
                            da.SelectCommand = cmd;
                            PRPosDB.InitFinicalDate();

                            var objstr = JsonConvert.SerializeObject(txn);
                            PSTrn01sClass RefundDeal = JsonConvert.DeserializeObject<PSTrn01sClass>(objstr);
                            RefundDeal.Deal_code = "R";
                            RefundDeal.Closetime = DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                            RefundDeal.Sendtime = DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                            RefundDeal.Opentime = DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                            RefundDeal.AccDate = PRPosUtils.AccDate.ToString(PRPosUtils.DateFormat);
                            RefundDeal.Tot_amt = txn.Tot_amt;
                            RefundDeal.Tax_amt = txn.Tax_amt;
                            RefundDeal.Deal_No = PRPosDB.getDealNo().ToString("000");
                            RefundDeal.Org_deal_no = txn.Deal_No;

                            foreach (var d in RefundDeal.OrderItems)
                            {
                                d.Deal_No = RefundDeal.Deal_No;
                                foreach (var m in d.Modifiers)
                                {
                                    m.Deal_No = RefundDeal.Deal_No;
                                }
                            }

                            cmd.CommandText = @" select  pstrn03s.*
                                       from pstrn03s                                     
                                       where pstrn03s.cmp_no=@cmp_no and pstrn03s.str_no=@str_no 
                                       and pstrn03s.pos_no=@pos_no and pstrn03s.deal_no=@deal_no and accdate=@accdate                                         
                                        order by item_no ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", txn.CustomerId);
                            cmd.Parameters.AddWithValue("str_no", txn.Store_Code);
                            cmd.Parameters.AddWithValue("pos_no", txn.Pos_No);
                            cmd.Parameters.AddWithValue("deal_no", txn.Deal_No);
                            cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(txn.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));

                            DataTable pstrn03sDT = new DataTable();
                            da.Fill(pstrn03sDT);

                            DataRow pstrn03s = pstrn03sDT.Rows[0];
                            if (pstrn03s["ecp_type"].ToString().Equals("EFTPOS"))
                            {
                                Debug.WriteLine("------Refund Transaction------");
                                pagesZIndex[(int)AdminPage.EftposPage] = "10";
                                OnPropertyChanged("");

                                decimal ecp_amt = 0;
                                decimal.TryParse(pstrn03s["ecp_amt"].ToString(), out ecp_amt);

                                Debug.WriteLine("Refund Transaction ");
                                App.log.Info("Refund Transaction ");

                                // transcation prepare.....
                                ReceiptText = "";
                                PaymentMsg = "";

                                DateTime ctime = DateTime.Now;
                                string Tdate = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);

                                PRPosDB.addDealNo(RefundDeal.Deal_No);
                                eftCtrl.EnableErrorDialog = false;
                                EftCtrl.DialogType = "30";
                                int AMT = (int)(RefundDeal.Tot_amt * 100);
                                string strAMT = string.Format("{0}", AMT);

                                string PAD = "OPR" + string.Format("{0:D3}", "99|KIOSK".Length) + "99|KIOSK" +
                                                                "AMT" + string.Format("{0:D3}", strAMT.Length) + strAMT +
                                                                "PCM0010";
                                EftCtrl.TxnType = "R";
                                EftCtrl.TxnRef = pstrn03s["ref_code1"].ToString().Substring(1);
                                eftCtrl.DialogTitle = "PLEASE SWIPE YOUR CARD";
                                EFTPOSMessage = "PLEASE SWIPE YOUR CARD";
                                PaymentMsg = "PLEASE SWIPE YOUR CARD";
                                EftCtrl.PosProductId = "TXN_TAG_DATA";
                                EftCtrl.AmtPurchase = RefundDeal.Tot_amt;
                                EftCtrl.AmtCash = 0;
                                EftCtrl.CutReceipt = false;
                                EftCtrl.ReceiptAutoPrint = false;
                                EftCtrl.PurchaseAnalysisData = PAD;

                                handler = new EventHandler(async delegate (Object o, EventArgs a)
                                {
                                    EFTPOSMessage = "Transaction Done";
                                    App.log.Info("OnTransactionEvent " + EftCtrl.Success);
                                    Directory.CreateDirectory(PRPosUtils.Spool_Folder);                                    
                                    using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + txn.Pos_No + "_" + DateTime.ParseExact(RefundDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + RefundDeal.Deal_No + ".txn", FileMode.CreateNew))
                                    {
                                        StreamWriter swWriter = new StreamWriter(fs);

                                        swWriter.WriteLine("TxnRef:" + EftCtrl.TxnRef);
                                        swWriter.WriteLine("ResponseCode:" + EftCtrl.ResponseCode);
                                        swWriter.WriteLine("ResponseText:" + EftCtrl.ResponseText);
                                        swWriter.WriteLine("Rrn:" + EftCtrl.Rrn);
                                        swWriter.WriteLine("Stan:" + EftCtrl.Stan);
                                        swWriter.WriteLine("PAD:" + EftCtrl.PurchaseAnalysisData);
                                        swWriter.WriteLine("AuthCode:" + EftCtrl.AuthCode);
                                        swWriter.WriteLine("Date:" + EftCtrl.Date);
                                        swWriter.WriteLine("Time:" + EftCtrl.Time);
                                        swWriter.WriteLine("AmtPurchase:" + EftCtrl.AmtPurchase);
                                        swWriter.Close();
                                        fs.Close();
                                    }
                                    if (EftCtrl.Success)
                                    {
                                        PaymentMsg = "Transcaton Success!! ";
                                        EFTPOSMessage = "Transcaton Success!! ";
                                        /********* Success ************/
                                        if (EftCtrl.ResponseCode == "00")
                                        {
                                            App.log.Info("Transcaton ResponseCode " + EftCtrl.ResponseCode); ;

                                            DateTime ctime = DateTime.Now;

                                            PRPos.Data.PSTrn03sClass pstrn03 = new PRPos.Data.PSTrn03sClass()
                                            {
                                                CustomerId = PRPosUtils.CustomerID,
                                                Store_Code = PRPosUtils.StoreCode,
                                                Pos_No = PRPosUtils.PosCode,
                                                AccDate = RefundDeal.AccDate,
                                                Deal_No = RefundDeal.Deal_No,
                                                Dc_code = "C",
                                                Item_No = 1,
                                                Ecp_type = PRPosUtils.PayEFTTenderCode,
                                                Ecp_amt = EftCtrl.AmtPurchase,
                                                Ecp_name = PRPosUtils.PayEFTTenderCode,
                                                Change_amt = 0,
                                                Memo = EftCtrl.AuthCode + "/" + EftCtrl.Date + "," + EftCtrl.Time,
                                                Epc_code = EftCtrl.Pan,
                                                Ref_code1 = "R" + EftCtrl.TxnRef,
                                                Ref_code2 = EftCtrl.Stan.ToString("000000")
                                            };
                                            RefundDeal.Payments = new ObservableCollection<PSTrn03sClass>();
                                            RefundDeal.Payments.Add(pstrn03);
                                            /* save transcation */
                                            SNSelfOrder.DAL.TranscationDAL transcationDAL = new DAL.TranscationDAL(PRPosDB.cnStr);
                                            transcationDAL.SetRefund(RefundDeal);
                                            transcationDAL.SetEFTPOSTransRef(EftCtrl.TxnRef);
                                            string JsonData = JsonConvert.SerializeObject(RefundDeal);

                                            using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + RefundDeal.Pos_No + "_" + DateTime.ParseExact(RefundDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + RefundDeal.Deal_No + ".txt", FileMode.CreateNew))
                                            {
                                                StreamWriter swWriter = new StreamWriter(fs);

                                                swWriter.WriteLine(JsonData);
                                                swWriter.Close();
                                                fs.Close();
                                            }
                                            using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + RefundDeal.Pos_No + "_" + DateTime.ParseExact(RefundDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + RefundDeal.Deal_No + ".rcp", FileMode.CreateNew))
                                            {
                                                StreamWriter swWriter = new StreamWriter(fs);
                                                swWriter.WriteLine(ReceiptText);
                                                swWriter.Close();
                                                fs.Close();
                                            }
                                            Helpers.ReceiptPrinter receiptPrinter = new Helpers.ReceiptPrinter();

                                            receiptPrinter.SetPrinter(PRPosUtils.PosPrinters);
                                            receiptPrinter.PrintingReceipt(RefundDeal);
                                            receiptPrinter.PrintingCardReceipt(ReceiptText);
                                        
                                            // using because that is lambada and old connection is disposed 
                                            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
                                            {
                                                cn.Open();
                                                SQLiteCommand cmd = cn.CreateCommand();
                                                SQLiteDataAdapter da = new SQLiteDataAdapter();
                                                da.SelectCommand = cmd;
                                                cmd.CommandText =
                                                  @"update  pstrn01s set del_deal_no=@del_deal_no
                                                  where cmp_no=@cmp_no and str_no=@str_no
                                                  and pos_no=@pos_no and accdate=@accdate   and deal_code='S'  
                                                  and deal_no=@deal_no ";

                                                cmd.Parameters.Clear();
                                                cmd.Parameters.AddWithValue("cmp_no", txn.CustomerId);
                                                cmd.Parameters.AddWithValue("str_no", txn.Store_Code);
                                                cmd.Parameters.AddWithValue("pos_no", txn.Pos_No);
                                                cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(txn.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));
                                                cmd.Parameters.AddWithValue("deal_no", txn.Deal_No);
                                                cmd.Parameters.AddWithValue("del_deal_no", RefundDeal.Deal_No);
                                                cmd.ExecuteNonQuery();
                                                ShowTxnCommand.Execute(null);
                                            }
                                        }
                                        /// Debug.WriteLine("DoGetLastTransaction Return  " + EftCtrl.TxnRef);                    
                                    }
                                    else
                                    {
                                        // Debug.WriteLine("DoGetLastTransaction Return  " + EftCtrl.ResponseCode);
                                        PaymentMsg = "Transcaton Failure: " + EftCtrl.ResponseText;
                                        EFTPOSMessage = "Transcaton Failure: " + EftCtrl.ResponseText;
                                    }

                                    EftCtrl.TransactionEvent -= handler;
                                    EftCtrl.PrintReceiptEvent -= PrintReceiptHandler;
                                    EftCtrl.DisplayEvent -= displayHanlder;

                                    System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                                    {
                                        StartIdleTimer();
                                        pagesZIndex[(int)AdminPage.EftposPage] = "0";
                                        OnPropertyChanged("");
                                    };
                                    _syncContext.Post(methodDelegate, null);
                                });

                                PrintReceiptHandler = new AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEventHandler(delegate (Object o, AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEvent a)
                                {
                                    Debug.WriteLine("receiptType :" + a.receiptType);
                                    if (a.receiptType == "R")
                                    {

                                        Debug.WriteLine("PrintReceiptHandler===========");
                                        App.log.Info("========PrintReceiptHandler===========");
                                        Debug.WriteLine(EftCtrl.Receipt);
                                        ReceiptText = EftCtrl.Receipt;
                                        App.log.Info(EftCtrl.Receipt); ;
                                    }

                                });
                                displayHanlder = new EventHandler(delegate (Object o, EventArgs a)
                                {

                                    Debug.WriteLine("[" + EftCtrl.DataField + "]");
                                    Debug.WriteLine("[" + EftCtrl.CsdReservedString1 + "]");
                                    PaymentMsg = EftCtrl.DataField.Substring(0, 20).Trim() + "\n" + EftCtrl.DataField.Substring(20, 20).Trim();
                                    EFTPOSMessage = EftCtrl.DataField.Substring(0, 20).Trim() + " " + EftCtrl.DataField.Substring(20, 20).Trim();
                                });
                                // Debug.WriteLine(EftCtrl.CsdReservedMethod3);

                                EftCtrl.DisplayEvent += displayHanlder;
                                EftCtrl.PrintReceiptEvent += PrintReceiptHandler;
                                EftCtrl.TransactionEvent += handler;
                                EftCtrl.DoTransaction();
                            }
                            else
                            {
                                foreach (var p in RefundDeal.Payments)
                                {
                                    p.Deal_No = RefundDeal.Deal_No;
                                }
                                /* save transcation */
                                SNSelfOrder.DAL.TranscationDAL transcationDAL = new DAL.TranscationDAL(PRPosDB.cnStr);
                                transcationDAL.SetRefund(RefundDeal);
                                transcationDAL.SetEFTPOSTransRef("");
                                string JsonData = JsonConvert.SerializeObject(RefundDeal);

                                using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + RefundDeal.Pos_No + "_" + DateTime.ParseExact(RefundDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + RefundDeal.Deal_No + ".txt", FileMode.CreateNew))
                                {
                                    StreamWriter swWriter = new StreamWriter(fs);

                                    swWriter.WriteLine(JsonData);
                                    swWriter.Close();
                                    fs.Close();
                                }
                                using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + RefundDeal.Pos_No + "_" + DateTime.ParseExact(RefundDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + RefundDeal.Deal_No + ".rcp", FileMode.CreateNew))
                                {
                                    StreamWriter swWriter = new StreamWriter(fs);
                                    swWriter.WriteLine(ReceiptText);
                                    swWriter.Close();
                                    fs.Close();
                                }
                                Helpers.ReceiptPrinter receiptPrinter = new Helpers.ReceiptPrinter();
                                
                                
                                receiptPrinter.SetPrinter(PRPosUtils.PosPrinters);
                                receiptPrinter.PrintingReceipt(RefundDeal);

                                receiptPrinter.PrintingCardReceipt(ReceiptText);
                                
                                cmd.CommandText =
                                        @"update  pstrn01s set del_deal_no=@del_deal_no
                                      where cmp_no=@cmp_no and str_no=@str_no
                                      and pos_no=@pos_no and accdate=@accdate   and deal_code='S'  
                                      and deal_no=@deal_no ";

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("cmp_no", txn.CustomerId);
                                cmd.Parameters.AddWithValue("str_no", txn.Store_Code);
                                cmd.Parameters.AddWithValue("pos_no", txn.Pos_No);
                                cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(txn.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));
                                cmd.Parameters.AddWithValue("deal_no", txn.Deal_No);
                                cmd.Parameters.AddWithValue("del_deal_no", RefundDeal.Deal_No);
                                cmd.ExecuteNonQuery();
                                ShowTxnCommand.Execute(null);
                                StartIdleTimer();
                            }                            
                        }
                    }
                    else
                    {
                        IsYesShow = false;
                        IsNoShow = false;
                        isOKShow = true;
                        ConfirmMessageTitle = "Can Not Refund!!";
                        ConfirmMessage = "This transcation is refunded";


                        ConfirmOKCmd = new DelegateCommand(() =>
                        {
                            // DisplayPage = (int)AdminPage.AdminMenuPage;
                            pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "0";
                            OnPropertyChanged("");
                            StartIdleTimer();
                        });
                        pagesZIndex[(int)AdminPage.ConfirmDialogPage] = "10";                        
                        OnPropertyChanged("");
                        StartIdleTimer();
                    }
                }
            }
        }

        #region LOADFASTKEY
        private void LoadFastKey()
        {
            PRPos.Data.FastkeySet mSelectedMenu = null;
            ModifierSetList = new ObservableCollection<ModSet>();
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
                /* Add  categories & items */
                #region MainMenu
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

                        
                        menuItem.FastkeyItems = LoadFastKeyClassById(row["sid"].ToString() );

                        menulists.Add(menuItem);
                        //  Debug.WriteLine(" add " + menuItem.Caption + " ," + menuItem.FontStyle + " ," + menuItem.FontColor+","+ menuItem.Width+","+ menuItem.Height);
#if _DEBUG
                        //        Debug.WriteLine("FastkeySet add "+ menuItem.Caption+" items="+ menuItem.FastkeyItems.Count);
#endif
                        if (menuItem.Default_yn == "Y")
                        {
                            if (mSelectedMenu == null)
                                this.SelectedMainMenu = menuItem;
                        }
                    }

                    this.mMainMenu = menulists;
                    //  Debug.WriteLine("MainMenu Set");
                    if (mSelectedMenu == null)
                        mSelectedMenu = menulists[0];
                    this.selectedMainMenu = mSelectedMenu;

                    //OnPropertyChanged("");               
                }
                #endregion
                #region Modifier Set

                cmd.CommandText = @"select * from psmodset01 where customerid=@customerid and modset_code in (select distinct modset_code from itemmodifier )";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                DataTable psmodset01DT = new DataTable();
                da.Fill(psmodset01DT);
                foreach (DataRow modset in psmodset01DT.Rows)
                {

                    ModSet ms = new ModSet()
                    {
                        Sid = modset["sid"].ToString(),
                        Customerid = modset["customerid"].ToString(),
                        Modset_code = modset["modset_code"].ToString(),
                        Caption = modset["caption"].ToString(),
                        Caption_fn = modset["caption_fn"].ToString(),
                        Del_flag = modset["del_flag"].ToString(),
                        Upd_date = modset["upd_date"].ToString(),
                        Mod_type = modset["mod_type"].ToString(),
                        Amount = modset["amount"].ToString(),
                        Max = modset["max_selection"].ToString(),
                        Min = modset["min_selection"].ToString(),
                        Next_modset = modset["next_modset"].ToString(),
                    };
                    
                    ms.ModSetlist = LoadModiferiesById(modset["sid"].ToString());
                    ModifierSetList.Add( ms );
                }
                if (selectedModifierItem == null)
                    selectedModifierItem = ModifierSetList[0];
                this.SelectedModifierItem = selectedModifierItem;
                #endregion
            }
        }
        private List<ModSetTi> LoadModiferiesById(string psid)
        {
            List<ModSetTi> retvals = new List<ModSetTi>();
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText =
                    @"select *,   datetime(upd_date) cloudUpdDate, datetime(str_upd_date)  localUpdDate      
                          from psmodsetti  where psid=@sid and ifnull(del_flag,'N')='N' ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("sid", psid );

                DataTable psmodsettiDT = new DataTable();
                da.Fill(psmodsettiDT);
                foreach (DataRow modsetti in psmodsettiDT.Rows)
                {
                    ModSetTi msti = new ModSetTi()
                    {
                        Sid = modsetti["sid"].ToString(),
                        Psid = modsetti["psid"].ToString(),
                        Modifier_code = modsetti["modifier_code"].ToString(),
                        Caption = modsetti["caption"].ToString(),
                        Caption_fn = modsetti["caption_fn"].ToString(),
                        Mod_type = modsetti["mod_type"].ToString(),
                        Price_type = modsetti["price_type"].ToString(),
                        Amount = modsetti["amount"].ToString(),
                        Max = modsetti["max_selection"].ToString(),
                        Min = modsetti["min_selection"].ToString(),
                        Next_modset = modsetti["next_modset"].ToString(),
                        Image = modsetti["image"].ToString(),
                        Del_flag = modsetti["del_flag"].ToString(),
                        Disp_price = modsetti["disp_price"].ToString(),
                        Disp_caption = modsetti["disp_caption"].ToString(),
                        SoldOut = modsetti["soldout"].ToString(),
                        Upd_date = modsetti["cloudUpdDate"].ToString(),
                        Str_upd_date = modsetti["localUpdDate"].ToString(),
                        StrSoldOut = modsetti["str_soldout"].ToString(),
                    };
                    retvals.Add(msti);
                }
            }
            return retvals;
        }

        private ObservableCollection<FastKeyClass> LoadFastKeyClassById(string psid)
        {
            ObservableCollection<FastKeyClass> retvals = new ObservableCollection<FastKeyClass>();
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                string salePrice = this.TheStation.Pricecolumn1;// PRPosUtils.SalePriceColumn.Equals("") ? "sprice" : PRPosUtils.SalePriceColumn;
                string takeawayPrice = this.TheStation.Pricecolumn2; //  PRPosUtils.TakeawayPriceColumn.Equals("") ? "sprice2" : PRPosUtils.SalePriceColumn;

                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                cmd.CommandText = @"select * from posfastkey02 where display_yn='Y' and op_code = 2 and store_code=@store_code and del_flag='N' 
                                            and customerid=@customerid and psid=@psid order by disp_order";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", TheStation.CustomerID);
                cmd.Parameters.AddWithValue("store_code", TheStation.Store_code);
                cmd.Parameters.AddWithValue("psid", psid);

                DataTable fastkeyItemDT = new DataTable();
                da.Fill(fastkeyItemDT);
                
                foreach (DataRow itemrow in fastkeyItemDT.Rows)
                {
                    cmd.CommandText =
                        @"select psitem.* ,itemvariety." + salePrice + @" as ivsprice,itemvariety." + takeawayPrice + @" as ivsprice2 
                                     ,promotions." + salePrice + @" as psprice,promotions." + takeawayPrice + @" as psprice2,
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
                    string soldOut = "N", cloudUpdDate = "";
                    string localUpdDate = "";
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
                            SoldOut = psitem["soldout"].ToString(),
                            StrSoldOut = psitem["str_soldout"].ToString(),
                            Sprice = price1,
                            GST = gst,
                            Takeawayprice = price2,
                            PCode = itemrow["op_code"].ToString(),
                            Width = itemrow["width"] == DBNull.Value ? PRPosUtils.ItemWidth : int.Parse(itemrow["width"].ToString()) * PRPosUtils.ItemWidth,
                            Height = itemrow["height"] == DBNull.Value ? PRPosUtils.ItemHeight : int.Parse(itemrow["height"].ToString()) * PRPosUtils.ItemHeight,
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
                            Upd_date = psitem["cloudUpdDate"].ToString(),
                            PsItem = psitem,
                            Str_Upd_date = psitem["localUpdDate"].ToString(),
                        };
                        retvals.Add(keybutton);
                        
                    }
                    else
                    {
                        Debug.WriteLine(" NOT FOUND " + itemrow["ref_code"].ToString());
                    }
                }
            }
            return retvals;
        }
        #endregion


        private void saveCloseCmdAction()
        {
            StopAllTimers();
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                if (IsModifierMode)
                {

                    cmd.CommandText = "update psmodsetti set str_soldout=@str_soldout,str_upd_date=@str_upd_date where sid=@sid ";
                    foreach (var modset in ModifierSetList)
                    {
                        //foreach (var modifier in SelectedModifierItem.ModSetlist)
                        foreach (var modifier in modset.ModSetlist)
                        {
                            if (modifier.StrSoldOut == "Y")
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", modifier.Sid);
                                cmd.Parameters.AddWithValue("str_soldout", "Y");
                                cmd.Parameters.AddWithValue("str_upd_date", DateTime.Now);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", modifier.Sid);
                                cmd.Parameters.AddWithValue("str_soldout", DBNull.Value);
                                cmd.Parameters.AddWithValue("str_upd_date", DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        //SelectedModifierItem.ModSetlist = LoadModiferiesById(SelectedModifierItem.Sid);
                        modset.ModSetlist = LoadModiferiesById(modset.Sid);
                    }
                }
                else
                {
                    cmd.CommandText = "update psitem set str_soldout=@str_soldout,str_upd_date=@str_upd_date where customerid=@customerid and item_code=@item_code ";
                    foreach (var catelog in mMainMenu)
                    {
                        
                        foreach (var item in catelog.FastkeyItems)
                        {
                            if (item.StrSoldOut == "Y")
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                                cmd.Parameters.AddWithValue("item_code", item.ItemCode);
                                cmd.Parameters.AddWithValue("str_soldout", "Y");
                                cmd.Parameters.AddWithValue("str_upd_date", DateTime.Now);
                                cmd.ExecuteNonQuery();
                                Debug.WriteLine(" Set " + item.ItemCode+" "+ item.ItemName);
                            }
                            else
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                                cmd.Parameters.AddWithValue("str_soldout", DBNull.Value);
                                cmd.Parameters.AddWithValue("str_upd_date", DBNull.Value);
                                cmd.Parameters.AddWithValue("item_code", item.ItemCode);
                                cmd.ExecuteNonQuery();
                                Debug.WriteLine(" RESet " + item.ItemCode + " " + item.ItemName);
                            }
                        }
                        catelog.FastkeyItems = LoadFastKeyClassById(catelog.Sid.ToString());
                    }
                }               
            }
            GoBackCommandAction();
        }
        private void resetCmdAction()
        {
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                if (IsModifierMode)
                {
                    cmd.CommandText = "update psmodsetti set str_soldout=@str_soldout,str_upd_date=@str_upd_date where sid=@sid ";
                    foreach (var modset in ModifierSetList)
                    {
                        //foreach (var modifier in SelectedModifierItem.ModSetlist)
                        foreach (var modifier in modset.ModSetlist)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", modifier.Sid);
                            cmd.Parameters.AddWithValue("str_soldout", DBNull.Value);
                            cmd.Parameters.AddWithValue("str_upd_date", DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        // SelectedModifierItem.ModSetlist = LoadModiferiesById(SelectedModifierItem.Sid);
                        modset.ModSetlist = LoadModiferiesById(modset.Sid);
                    }
                }
                else
                {
                    cmd.CommandText = "update psitem set str_soldout=@str_soldout,str_upd_date=@str_upd_date where customerid=@customerid and item_code=@item_code ";

                    //foreach (var item in SelectedMainMenu.FastkeyItems)
                    foreach (var catelog in mMainMenu)
                    {
                        foreach (var item in catelog.FastkeyItems)
                        {
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                                cmd.Parameters.AddWithValue("str_soldout", DBNull.Value);
                                cmd.Parameters.AddWithValue("str_upd_date", DBNull.Value);
                                cmd.Parameters.AddWithValue("item_code", item.ItemCode);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        // SelectedMainMenu.FastkeyItems = LoadFastKeyClassById(SelectedMainMenu.Sid.ToString() );
                        catelog.FastkeyItems = LoadFastKeyClassById(catelog.Sid.ToString());
                    }
                }
            }
            OnPropertyChanged("");
            StartIdleTimer();
        }
        private void ClickItemCmdAction(PRPos.Data.FastKeyClass item)
        {
            // Debug.WriteLine(itm.Sid);
            string SoldOut = item.StrSoldOut == "Y" ? "" : "Y";
            item.StrSoldOut = SoldOut;
            StartIdleTimer();
            OnPropertyChanged("");
            
        }
        private void ClickModifierCmdAction(ModSetTi Modfier)
        {
            string SoldOut = Modfier.StrSoldOut == "Y" ? "" : "Y";
            Modfier.StrSoldOut = SoldOut;
            StartIdleTimer();
          //  OnPropertyChanged("");
        }
        private void ConnectionCommandAction()
        {
            // DAL.StationBL stationBL = new DAL.StationBL();
            // CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(SelfOrderSetting);
            //checkCodeService.CheckCode(TheStation.Connection);
            ConnectionCode = TheStation.Connection;
            PageDisplay(AdminPage.ConnectionPage);
            /*
            ConnectionCode frmConnection = new ConnectionCode();
            var vm = new ConnectionCodeVM();
            vm.ConnectionCode = TheStation.Connection;
            vm.InputClose += async (e) =>
            {
                string strconnection = (e as string);
                Debug.WriteLine(strconnection);
                
                SelfOrderSetting.ConnectionCode = strconnection;
                CheckConnectionCodeService checkCodeService = new CheckConnectionCodeService(SelfOrderSetting);
                
                var task = await checkCodeService.LaunchCodeCheck(strconnection);                
                {
                    Station st = task;
                    Debug.WriteLine(st.StatusCode);
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
                        frmConnection.Close();
                        Task.Delay(100);
                    }
                    else
                    {
                        Debug.WriteLine(st.StatusCode);

                    }
                };
            };
            frmConnection.DataContext = vm;
            frmConnection.Show();
           */
        }
        private void RefundCommandAction()
        {
            TxnListCaption = "KIOSK Refund";
            FuncCaption = "Refund Selected";
            PaymentMsg = "";
            OrderList = new ObservableCollection<PSTrn01sClass>();

            doRePrintCommand = new DelegateCommand(doRefundAction);
            // DisplayPage = (int)AdminPage.TranscationPage;
            PageDisplay(AdminPage.TranscationPage);
            OnPropertyChanged("");
        }

        private void TotalAmountCommandAction()
        {
           
            NumberFormatInfo nfi = (NumberFormatInfo)PRPosUtils.LocalCulture.NumberFormat.Clone();
            nfi.CurrencySymbol = PRPosUtils.CurrencySymbol;
            using (SQLiteConnection connection = new SQLiteConnection(PRPosDB.cnStr))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                AmountMessage = "";

                cmd.CommandText = "select * from psctrl01 where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and  ifnull(close_yn,'N')='N' order by tdate desc";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                da.SelectCommand = cmd;
                DataTable psctrl01DT = new DataTable();
                da.Fill(psctrl01DT);

                if (psctrl01DT.Rows.Count > 0)
                {
                    DataRow psctrl01 = psctrl01DT.Rows[0];

                    DateTime aDate;
                    if (DateTime.TryParse(psctrl01["tdate"].ToString(), out aDate))
                    {
                        amountMessage = "Accounting Date:" + aDate.ToString(PRPosUtils.DateFormat) + "\n";
                    }

                    cmd.CommandText = "select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no  and accdate=@accdate and deal_code='S' order by deal_no";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                    cmd.Parameters.AddWithValue("accdate", aDate.Date);
                    DataTable pstrn01sDT = new DataTable();
                    da.Fill(pstrn01sDT);
                    int cnt = 0;
                    string minseq = "";
                    string maxseq = "";
                    decimal TotAmt = 0;
                    decimal TotTax = 0;

                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {

                        cnt += 1;
                        if (minseq.Equals(""))
                        {
                            minseq = pstrn01s["deal_no"].ToString();
                            maxseq = pstrn01s["deal_no"].ToString();
                        }
                        if (pstrn01s["deal_no"].ToString().CompareTo(maxseq) > 0)
                        {
                            maxseq = pstrn01s["deal_no"].ToString();
                        }
                        cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                        DataTable pstrn03sDT = new DataTable();
                        da.Fill(pstrn03sDT);
                        if (pstrn03sDT.Rows.Count > 0)
                        {
                            decimal amt = 0;
                            decimal tax = 0;
                            decimal.TryParse(pstrn01s["tot_amt"].ToString(), out amt);
                            decimal.TryParse(pstrn01s["tax_amt"].ToString(), out tax);
                            TotAmt += amt;
                            TotTax += tax;

                        }
                    }

                    amountMessage += "X Read Report" + "\n";
                    if (!PRPosDB.ReadString("company_name").Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("company_name") + "\n";

                    }
                    if (!PRPosDB.ReadString("buss_no").Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("buss_no") + "\n";
                    }
                    if (!PRPosDB.ReadString("store_name").Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("store_name") + "\n";
                    }
                    if (!PRPosDB.ReadString("store_phone").Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("store_phone") + "\n";
                    }

                    string addr1 = PRPosDB.ReadString("store_address_line1");
                    if (!addr1.Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("addr1") + "\n";
                    }
                    addr1 = PRPosDB.ReadString("store_address_line2");
                    if (!addr1.Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("addr1") + "\n";
                    }
                    addr1 = PRPosDB.ReadString("store_address_line3");
                    if (!addr1.Equals(""))
                    {
                        amountMessage += PRPosDB.ReadString("addr1") + "\n";
                    }
                    amountMessage += "Station: " + PRPosUtils.PosCode + " No.: " + minseq + " to " + maxseq + "\n";

                    amountMessage += "Print at " + DateTime.Now.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat) + "\n";
                    amountMessage += "Account Date:" + aDate.ToString(PRPosUtils.DateFormat) + "\n";


                    string output = PRPosUtils.repeatStr("=", 44);
                    
                    Dictionary<string, decimal> paymentlist = new Dictionary<string, decimal>();

                    amountMessage += output + "\n";

                    amountMessage += "Payment Summary".PadRight(34, ' ') + "Amount".PadLeft(10, ' ') + "\n";

                    amountMessage += output + "\n";

                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {
                        cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                        DataTable pstrn03sDT = new DataTable();
                        da.Fill(pstrn03sDT);
                        foreach (DataRow p03 in pstrn03sDT.Rows)
                        {
                            decimal ecp_amt = 0;
                            decimal.TryParse(p03["ecp_amt"].ToString(), out ecp_amt);

                            if (!paymentlist.ContainsKey(p03["ecp_type"].ToString()))
                            {
                                paymentlist.Add(p03["ecp_type"].ToString(), ecp_amt);
                            }
                            else
                            {
                                paymentlist[p03["ecp_type"].ToString()] += ecp_amt;
                            }
                        }
                    }
                    decimal payamt = 0;
                    foreach (KeyValuePair<string, decimal> pay in paymentlist)
                    {
                        amountMessage += "  " + pay.Key.PadRight(32, ' ') + string.Format(nfi, "{0:C}", pay.Value).PadLeft(10, ' ') + "\n";
                        payamt += pay.Value;
                    }
                    amountMessage += output + "\n";
                    amountMessage += "Payment Total:".PadRight(34, ' ') + string.Format(nfi, "{0:C}", payamt).PadLeft(10, ' ') + "\n";
                    amountMessage += output + "\n";

                    amountMessage += "Sale Taxes".PadRight(34, ' ') + "Amount".PadLeft(10, ' ') + "\n";
                    amountMessage += "  GST".PadRight(32, ' ') + string.Format(nfi, "{0:C}", TotTax).PadLeft(10, ' ') + "\n" + "\n";

                    amountMessage += output + "\n";
                    amountMessage += "Items Types".PadRight(28, ' ') + "".PadRight(6, ' ') + "Amount".PadLeft(10, ' ') + "\n";
                    amountMessage += output + "\n";

                    Dictionary<string, string> categoryList = new Dictionary<string, string>();

                    Dictionary<string, decimal> categoryAmt = new Dictionary<string, decimal>();
                    cmd.CommandText = @" select customerid,cate_code,cate_name   from pscategory  where customerid=@customerid and pcate_code<>'' and del_flag='N' order by seq ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                    DataTable pscategoryDT = new DataTable();
                    da.Fill(pscategoryDT);

                    foreach (DataRow cate in pscategoryDT.Rows)
                    {
                        // Console.WriteLine(cate["cate_code"].ToString() +","+ cate["cate_name"].ToString() + ","+                            categoryList.ContainsKey(cate["cate_code"].ToString())+","+      categoryAmt.ContainsKey(cate["cate_code"].ToString()));
                        if (!categoryList.ContainsKey(cate["cate_code"].ToString()))
                        {
                            categoryList.Add(cate["cate_code"].ToString(), cate["cate_name"].ToString());
                        }
                        if (!categoryAmt.ContainsKey(cate["cate_code"].ToString()))
                        {
                            categoryAmt.Add(cate["cate_code"].ToString(), 0);
                        }
                    }

                    decimal saleamt = 0;

                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {
                        cmd.CommandText = @" select  pstrn02s.*,psitem.cate_code  from pstrn02s                                       
                                      left outer join psitem on psitem.item_code=pstrn02s.item_code and psitem.customerid= pstrn02s.cmp_no
                                       where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no  
                                       and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no=@deal_no  and pstrn02s.item_type in ('C','I','E')  ";


                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                        DataTable DTpstrn02s = new DataTable();
                        da.Fill(DTpstrn02s);

                        foreach (DataRow pstrn02s in DTpstrn02s.Rows)
                        {
                            decimal item_amt = 0;
                            decimal.TryParse(pstrn02s["amt"].ToString(), out item_amt);
                            // Console.WriteLine(pstrn02s["cate_code"].ToString()+" = " + categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString())) ;
                            if (categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString()))
                            {
                                categoryAmt[pstrn02s["cate_code"].ToString()] += item_amt;
                                saleamt += item_amt;
                            }
                        }
                    }

                    foreach (KeyValuePair<string, decimal> cateAmt in categoryAmt)
                    {
                        if (cateAmt.Value != 0)
                        {
                            amountMessage += "  " + categoryList[cateAmt.Key].ToString().PadRight(32, ' ') + string.Format(nfi, "{0:C}", cateAmt.Value).PadLeft(10, ' ') + "\n";
                        }
                    }
                    amountMessage += output + "\n";
                    amountMessage += "Item Type Totals:".PadRight(34, ' ') + string.Format(nfi, "{0:C}", saleamt).PadLeft(10, ' ') + "\n" + "\n";



                    // refund deal
                    //
                    //
                    cmd.CommandText = @"select * from pstrn01s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no  and accdate=@accdate 
                                                  and deal_code='R' order by deal_no";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", PRPosUtils.PosCode);

                    cmd.Parameters.AddWithValue("accdate", aDate.Date);
                    pstrn01sDT = new DataTable();
                    da.Fill(pstrn01sDT);

                    int refundcnt = 0;
                    decimal RefundTotAmt = 0;
                    decimal RefundTotTax = 0;

                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {

                        refundcnt += 1;

                        cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());

                        DataTable pstrn03sDT = new DataTable();
                        da.Fill(pstrn03sDT);
                        if (pstrn03sDT.Rows.Count > 0)
                        {
                            decimal amt = 0;
                            decimal tax = 0;
                            decimal.TryParse(pstrn01s["tot_amt"].ToString(), out amt);
                            decimal.TryParse(pstrn01s["tax_amt"].ToString(), out tax);
                            RefundTotAmt += amt;
                            RefundTotTax += tax;

                        }
                    }

                    Dictionary<string, decimal> refundpaymentlist = new Dictionary<string, decimal>();
                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {
                        cmd.CommandText = @"select * from pstrn03s where cmp_no=@cmp_no and str_no=@str_no and pos_no=@pos_no and accdate=@accdate
                                            and  deal_no=@deal_no order by deal_no,item_no ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                        DataTable pstrn03sDT = new DataTable();
                        da.Fill(pstrn03sDT);
                        foreach (DataRow p03 in pstrn03sDT.Rows)
                        {
                            decimal ecp_amt = 0;
                            decimal.TryParse(p03["ecp_amt"].ToString(), out ecp_amt);

                            if (!refundpaymentlist.ContainsKey(p03["ecp_name"].ToString()))
                            {
                                refundpaymentlist.Add(p03["ecp_name"].ToString(), ecp_amt);
                            }
                            else
                            {
                                refundpaymentlist[p03["ecp_name"].ToString()] += ecp_amt;
                            }
                        }
                    }

                    amountMessage += "\n\n";
                    amountMessage += "Refund Message" + "\n";
                    amountMessage += output + "\n";
                    decimal refundpayamt = 0;
                    foreach (KeyValuePair<string, decimal> pay in refundpaymentlist)
                    {
                        amountMessage += "  " + pay.Key.PadRight(32, ' ') + string.Format(nfi, "{0:C}", pay.Value).PadLeft(10, ' ') + "\n";
                        refundpayamt += pay.Value;                        
                    }
     
                                                        
                    amountMessage += "Refund Payment Total:".PadRight(34, ' ') + string.Format(nfi, "{0:C}", refundpayamt).PadLeft(10, ' ') + "\n";
                    amountMessage += output + "\n";

                                                          
                    amountMessage += "Refund Items Types".PadRight(34, ' ') +  "Amount".PadLeft(10, ' ') + "\n";
                    amountMessage += output + "\n";

                    categoryList = new Dictionary<string, string>();

                    categoryAmt = new Dictionary<string, decimal>();
                    cmd.CommandText = @" select customerid,cate_code,cate_name   from pscategory  where customerid=@customerid and pcate_code<>'' and del_flag='N' order by seq ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                    pscategoryDT = new DataTable();
                    da.Fill(pscategoryDT);

                    foreach (DataRow cate in pscategoryDT.Rows)
                    {
                        // Console.WriteLine(cate["cate_code"].ToString() +","+ cate["cate_name"].ToString() + ","+                            categoryList.ContainsKey(cate["cate_code"].ToString())+","+      categoryAmt.ContainsKey(cate["cate_code"].ToString()));
                        if (!categoryList.ContainsKey(cate["cate_code"].ToString()))
                        {
                            categoryList.Add(cate["cate_code"].ToString(), cate["cate_name"].ToString());
                        }
                        if (!categoryAmt.ContainsKey(cate["cate_code"].ToString()))
                        {
                            categoryAmt.Add(cate["cate_code"].ToString(), 0);
                        }
                    }

                    decimal refundsaleamt = 0;

                    foreach (DataRow pstrn01s in pstrn01sDT.Rows)
                    {
                        cmd.CommandText = @" select  pstrn02s.*,psitem.cate_code  from pstrn02s                                       
                                      left outer join psitem on psitem.item_code=pstrn02s.item_code and psitem.customerid= pstrn02s.cmp_no
                                       where pstrn02s.cmp_no=@cmp_no and pstrn02s.str_no=@str_no  
                                       and pstrn02s.pos_no=@pos_no  and pstrn02s.accdate=@accdate and pstrn02s.deal_no=@deal_no  and pstrn02s.item_type in ('C','I','E')  ";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", pstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", pstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", pstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", pstrn01s["deal_no"].ToString());
                        DataTable DTpstrn02s = new DataTable();
                        da.Fill(DTpstrn02s);

                        foreach (DataRow pstrn02s in DTpstrn02s.Rows)
                        {
                            decimal item_amt = 0;
                            decimal.TryParse(pstrn02s["amt"].ToString(), out item_amt);
                            // Console.WriteLine(pstrn02s["cate_code"].ToString()+" = " + categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString())) ;
                            if (categoryAmt.ContainsKey(pstrn02s["cate_code"].ToString()))
                            {
                                categoryAmt[pstrn02s["cate_code"].ToString()] += item_amt;
                                refundsaleamt += item_amt;
                            }
                        }
                    }


                    foreach (KeyValuePair<string, decimal> cateAmt in categoryAmt)
                    {
                        if (cateAmt.Value != 0)
                        {
                            amountMessage += "  " + categoryList[cateAmt.Key].ToString().PadRight(32, ' ') + string.Format(nfi, "{0:C}", cateAmt.Value).PadLeft(10, ' ') + "\n";
                        }
                    }
                    amountMessage += output + "\n";
                    amountMessage += "Refund Type Totals:".PadRight(34, ' ') + string.Format(nfi, "{0:C}", refundsaleamt).PadLeft(10, ' ') + "\n";
                }                                               
            }

            PageDisplay(AdminPage.TotalAmountsPage);
            OnPropertyChanged("");
        }

        private void StopAllTimers()
        {            
            StopIdleTimer();
            StopmsgTimeoutTimer();
        }
        public void VM_Start()
        {            
            ConnectionCode = TheStation.Connection;
            StartIdleTimer();
            // DisplayPage = (int)AdminPage.PasswordPage;
            
        }
        public FormAdminVM()
        {
            ResponseMessage = "";
            TimeOutMessage =  "Do You Need "+PRPosUtils.WaitingTime+" More Seconds to Work on Admin Page?";
            _syncContext = System.Threading.SynchronizationContext.Current;
            BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
            PageDisplay(AdminPage.PasswordPage);
            BeginDate = DateTime.Today;
            EndDate = DateTime.Today;
            OrderList = new ObservableCollection<PSTrn01sClass>();
            ItemsSelectChanged = new DelegateCommand<object>(ItemsSelectChangedAction);
            this.TimeOutPageMoreTimeNoCmd = new DelegateCommand(() => {
                StopAllTimers();
                System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                {
                    VMClose?.Invoke(TimeOutStatus.None);
                };
                _syncContext.Post(methodDelegate, null);
            });
            CalendarCommand = new DelegateCommand(() => {
                StartIdleTimer();
            });
            
        }
    }
}
