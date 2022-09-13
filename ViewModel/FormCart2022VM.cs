using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using SNSelfOrder.Interfaces;
using System.Windows.Threading;
using System.Data;
using System.Collections.ObjectModel;
using SNSelfOrder.Models;
using AxCSDEFTLib;
using log4net;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;

namespace SNSelfOrder.ViewModel
{
    enum CartPage
    {   // order same xmal
        BackgroundPage,  //0
        CartPage,  //1 
        PaymentPage, //2        
        FinalPage, //3
       TimeOutPage, //4
    }
    public class FormCart2022VM : ViewModelBase, ICloseWindows, ITimeOut, IEndTranscation
    {
        private List<string> pagesZIndex = new List<string>() { "1", "10", "0", "0" ,"0" };
        private CartPage CurrentPage = CartPage.CartPage;
        public Action<TimeOutStatus> VMClose { get; set; }
        private AxCSDEFTLib.AxCsdEft eftCtrl;
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        
        private int countDown = PRPosUtils.AlterDisplayTime;
        private int timeRemain = PRPosUtils.WaitingTime;
        private DispatcherTimer idleTimer;
        private DispatcherTimer showFinalTimer;
        private System.Windows.Threading.DispatcherTimer bannerImageTimer;



        private bool displayPayCash = true;
        private bool displayPayEFTPOS = true;


        private System.Threading.SynchronizationContext _syncContext = null;
        private bool canPay = false;
        private int itemCount = 0;
        private decimal totalPrice = 0;
        private bool windowIsVisible = true;
        private int imgIndex = 0;

        private string orderType = "Dine In";

        
        private bool closeButtonEnable = false;
        private bool payButtonEnable = false;
        private bool cancelButtonEnable = false;
        private string tableNumber = "1";
        private string paymentMsg = "YOUR ORDER WILL BE SENT TO THE KITCHEN, AFTER YOU'VE COMPLETED PAYMENT.";
        private string orderTypeMsg = "CHANGE TO TAKEWAY";
        private string receiptText = "";
        private string messageFinalPage = "PLEASE TAKE YOUR RECEIPT, AND MOVE TO THE COLLECTION POINT.";
        public DelegateCommand TimeOutMessageOKCmd { get; set; }
        public DelegateCommand TimeOutMessageNoCmd { get; set; }

        private DelegateCommand cartCancelCommand;

        private DelegateCommand confirmOrderCmd;

        private DelegateCommand changeOrderTypeCommand;

        private DelegateCommand backToCartCommand;

        private DelegateCommand payByCASHCommand;

        private DelegateCommand payByEFTPOSCommand;

        private DelegateCommand eFTPOSCancelCommand;

        private DelegateCommand finalCloseCommand;

        private ObservableCollection<BannerItem> BannerImagePathList { get; set; }
        private string currentBannerImagePath;

        private string backgroundImagePath;

        private string finalBackgroundImagePath;
        private string paymanetBackgroundImagePath;
        public ICommand ModifyItemClick { get; set; }
        public ICommand RemoveItemClick { get; set; }

        public ICommand MinusClick { get; set; }
        public ICommand PlusClick { get; set; }
        public List<string> PagesZIndex { get => pagesZIndex; set { SetProperty(ref pagesZIndex, value); } }
        private async void ModifyItemClickAction(Object param)
        {
            if (param != null)
            {
                var theitem = (param as PRPos.Data.PSTrn02sClass);
                StopIdleTimer();
                FormItem2022 formItem2022 = new FormItem2022();

                var vmitem2022 = new FormItem2022VM();
                vmitem2022.ApplyIsVisible = true;
                vmitem2022.UpdateItem += async (s, v, q, p, i) =>
                {
                    Debug.WriteLine("cart call  vmitem2022 Apply " + theitem);

                    theitem.Variety_Code = v.Variety_code;
                    theitem.Size_Code = v.Size_code;
                    theitem.Variety_Caption = v.Caption;
                    theitem.Variety_Kitchen_name = v.Kitchen_name;
                    theitem.Qty = q;
                    theitem.CalQty = q;                   
                    theitem.Sprice = p;
                    theitem.Amount = i * q;
                    theitem.Modifiers = new ObservableCollection<PRPos.Data.PSTrn04sClass>();
                    foreach (var m in v.ModifierSets)
                    {
                        foreach (var detail in m.Modifiers)
                        {
                            if (detail.SelectedQty > 0)
                            {
                                PRPos.Data.PSTrn04sClass modifer = new PRPos.Data.PSTrn04sClass()
                                {
                                    Variety_Code = m.Variety_code,
                                    Modset_Code = m.ModSet_code,
                                    Modifier_Code = detail.Modifier_code,
                                    Sprice = detail.Sprice,
                                    Qty = detail.InpQty * theitem.Qty,
                                    InpQty = detail.InpQty,
                                    CalQty = detail.InpQty * theitem.Qty,
                                    Amount = detail.InpQty * theitem.Qty * detail.Sprice,
                                    CalSprice= detail.InpQty * theitem.Qty * detail.Sprice,
                                    Caption = detail.Caption,
                                    Caption_fn = detail.Caption_fn,
                                };
                                theitem.Modifiers.Add(modifer);
                            }
                        }
                    }

                    await UpdateTotal();
                    formItem2022.Close();
                    ResetIdleTimer();
                };
                formItem2022.TimeOut += async (s, e) =>
                {
                    Debug.WriteLine("cart call formItem2022 Timeout ");
                    if (bannerImageTimer != null)
                    {
                        bannerImageTimer.Stop();
                        this.bannerImageTimer.Tick -= imageTimer_Tick;
                        bannerImageTimer = null;
                    }
                    if (idleTimer != null)
                    {
                        StopIdleTimer();
                        idleTimer.Tick -= IdleTimeoutTimer_Tick;
                        idleTimer = null;
                    }
                    VMClose?.Invoke(TimeOutStatus.TimeOut);
                };
                formItem2022.Closed += (s, e) =>
                {
                    Debug.WriteLine("cart call formItem2022 closed ");
                };
                vmitem2022.SelectedItem = theitem.FastKey;
                vmitem2022.Qty = theitem.Qty;
                                                                                
               foreach (var v in vmitem2022.ItemVarietys)
                {
                    if (v.Variety_code == theitem.Variety_Code)
                    {
                        foreach (var modifier in theitem.Modifiers)
                        {
                            foreach (var mset in v.ModifierSets)
                            {
                                if (mset.ModSet_code == modifier.Modset_Code)
                                {
                                    foreach (var m in mset.Modifiers)
                                    {
                                        if (m.Modifier_code == modifier.Modifier_Code)
                                        {
                                            m.SelectedQty = modifier.InpQty * theitem.Qty;
                                            m.InpQty = modifier.InpQty;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        vmitem2022.SelectedVariety = v;

                        break;
                    }
                }
                
                formItem2022.DataContext = vmitem2022;
                
                formItem2022.Show();
            }
        }
        private async void RemoveItemClickAction(Object param)
        {
            if (param != null)
            {
                Console.WriteLine(param);
                CurrentDeal.OrderItems.Remove(param as PRPos.Data.PSTrn02sClass);

                await UpdateTotal();
            }
            ResetIdleTimer();
        }
        private async void MinusClickAction(Object param)
        {
            if (param != null)
            {
                PRPos.Data.PSTrn02sClass item = (param as PRPos.Data.PSTrn02sClass);
                if (item.Qty > 1)
                {
                    item.Qty -= 1;
                    decimal sprice = 0;
                    foreach (var m in item.Modifiers)
                    {
                        m.Qty = m.InpQty;
                        m.CalQty = item.Qty * m.InpQty;
                        sprice += item.Qty * m.InpQty * m.Sprice;
                        m.CalSprice = item.Qty * m.InpQty * m.Sprice;
                    }
                    sprice += item.Sprice;
                    item.Amount = item.Qty * sprice;
                }
                await UpdateTotal();
            }            
        }
        private async void PlusClickAction(Object param)
        {
            if (param != null)
            {
                PRPos.Data.PSTrn02sClass item = (param as PRPos.Data.PSTrn02sClass);
                if (item.Qty < PRPosUtils.MaxQty)
                {
                    item.Qty += 1;
                    decimal sprice = 0;
                    foreach (var m in item.Modifiers)
                    {
                        m.Qty = m.InpQty;
                        m.CalQty = item.Qty * m.InpQty;
                        sprice += item.Qty * m.InpQty * m.Sprice;
                        m.CalSprice = item.Qty * m.InpQty * m.Sprice;
                    }
                    sprice += item.Sprice;
                    item.Amount = item.Qty * sprice;
                    await UpdateTotal();
                }
            }
            
        }
        private async Task UpdateTotal()
        {
            decimal amt = 0;
            decimal taxamt = 0;
            decimal ntxamt = 0;
            foreach (var orderItem in CurrentDeal.OrderItems)
            {
                if ((orderItem.Item_Type == "I") || (orderItem.Item_Type == "S") || (orderItem.Item_Type == "H"))
                {
                    decimal modamt = 0;

                    decimal itmtaxamt = 0;
                    foreach (var m in orderItem.Modifiers)
                    {
                        m.Qty = m.InpQty * orderItem.Qty;
                        decimal mamt =m.InpQty * m.Sprice;
                        modamt += mamt;
                    }
                    modamt += orderItem.Sprice;
                    
                    orderItem.Goo_Price = orderItem.Sprice * orderItem.Qty;
                    orderItem.Amount = modamt * orderItem.Qty;

                    if (orderItem.GST == 0m)
                    {
                        itmtaxamt = 0;
                    }
                    else
                    {
                        itmtaxamt = Math.Round(orderItem.Amount * (orderItem.GST / 100), 2);
                    }
                    orderItem.Tax_Amt = itmtaxamt;

                }
                else
                {
                    orderItem.Amount = 0;
                    orderItem.Tax_Amt = 0;
                }
                taxamt += orderItem.Tax_Amt;
                amt += orderItem.Amount;
            }
            TotalPrice = amt;
            this.ItemCount = CurrentDeal.OrderItems.Count;
            this.CurrentDeal.Tot_amt = amt;
            this.CurrentDeal.Tax_amt = taxamt;
            CanPay = ItemCount > 0;
            OnPropertyChanged("");
            ResetIdleTimer();
        }
        public DelegateCommand CartCancelCommand => cartCancelCommand ?? (cartCancelCommand = new DelegateCommand(CartCancelCommandAction));
        public DelegateCommand BackToCartCommand => backToCartCommand ?? (backToCartCommand = new DelegateCommand(BackToCartCommandAction));
        public DelegateCommand ConfirmOrderCmd => confirmOrderCmd ?? (confirmOrderCmd = new DelegateCommand(ConfirmOrderCmdAction));
        public DelegateCommand ChangeOrderTypeCommand => changeOrderTypeCommand ?? (changeOrderTypeCommand = new DelegateCommand(ChangeOrderTypeCommandAction));
        public DelegateCommand PayByCASHCommand => payByCASHCommand ?? (payByCASHCommand = new DelegateCommand(PayByCASHCommandAction));
        public DelegateCommand PayByEFTPOSCommand => payByEFTPOSCommand ?? (payByEFTPOSCommand = new DelegateCommand(PayByEFTPOSCommandAction));

        public DelegateCommand EFTPOSCancelCommand => eFTPOSCancelCommand ?? (eFTPOSCancelCommand = new DelegateCommand(EFTPOSCancelCommandAction));

        public DelegateCommand FinalCloseCommand => finalCloseCommand ?? (finalCloseCommand = new DelegateCommand(DoCloseAction));

        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref backgroundImagePath, value); } }
      

        public bool WindowIsVisible { get => windowIsVisible; set { SetProperty(ref windowIsVisible, value); } }

        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }

        public int ItemCount { get { return itemCount; } set { itemCount = value; OnPropertyChanged("ItemCount"); } }
        public decimal TotalPrice { get { return totalPrice; } set { totalPrice = value; OnPropertyChanged("TotalPrice"); } }

        public string CurrentBannerImagePath { get => currentBannerImagePath; set { currentBannerImagePath = value; OnPropertyChanged("CurrentBannerImagePath"); } }

        public string OrderType { get => orderType; set { orderType = value; OnPropertyChanged("OrderType"); } }

        public string TableNumber { get => tableNumber; set { tableNumber = value; OnPropertyChanged("TableNumber"); } }

        public string OrderTypeMsg { get => orderTypeMsg; set { orderTypeMsg = value; OnPropertyChanged("OrderTypeMsg"); } }

        public bool CanPay { get { return canPay; } set { SetProperty(ref canPay, value); } }


        public string PaymanetBackgroundImagePath { get => paymanetBackgroundImagePath; set { SetProperty(ref paymanetBackgroundImagePath, value); } }

        public string FinalBackgroundImagePath { get => finalBackgroundImagePath; set { SetProperty(ref finalBackgroundImagePath, value); } }
        public bool PayButtonEnable { get => payButtonEnable; set { SetProperty(ref payButtonEnable, value); } }

        public bool CloseButtonEnable { get => closeButtonEnable; set { SetProperty(ref closeButtonEnable, value); } }
        public bool CancelButtonEnable { get => cancelButtonEnable; set { SetProperty(ref cancelButtonEnable, value); } }


        public bool DisplayPayCash { get => displayPayCash; set { SetProperty(ref displayPayCash, value); } }
        public bool DisplayPayEFTPOS { get => displayPayEFTPOS; set { SetProperty(ref displayPayEFTPOS, value); } }

        public AxCsdEft EftCtrl { get => eftCtrl; set => eftCtrl = value; }
        public string PaymentMsg { get => paymentMsg; set { SetProperty(ref paymentMsg, value); } }

        private PRPos.Data.PSTrn01sClass currentDeal;
        public PRPos.Data.PSTrn01sClass CurrentDeal
        {
            get => currentDeal;
            set
            {
                SetProperty(ref currentDeal, value);
                // Debug.WriteLine(currentDeal.Order_type + " " + PRPos.Data.OrderType.DINING.ToString());
                if (currentDeal.Order_type ==  ((int)PRPos.Data.OrderType.DINING).ToString())
                {
                    currentDeal.Order_type = ((int)PRPos.Data.OrderType.DINING).ToString();
                    this.OrderType = "DINE IN";
                    OrderTypeMsg = "CHANGE TO TAKEWAY";
                }
                else
                {
                    currentDeal.Order_type = ((int)PRPos.Data.OrderType.TAKEWAY).ToString();
                    this.OrderType = "TAKEWAY";
                    OrderTypeMsg = "CHANGE TO DINE IN";
                  
                }
                UpdateTotal();
            }

        }

        public Action<PRPos.Data.PSTrn01sClass> CloseTranscation { get; set; }

        public string ReceiptText { get => receiptText; set => receiptText = value; }
        public string MessageFinalPage { get => messageFinalPage; set { SetProperty(ref messageFinalPage, value); } }

        public int MsgTimeoutCount { get => msgTimeoutCount; set { SetProperty(ref msgTimeoutCount, value); } }

        public int CountDown { get => countDown; set { SetProperty(ref countDown, value); } }
         
        private void GoToPage(CartPage page)
        {
            CurrentPage = page;
            StopIdleTimer();
            StopmsgTimeoutTimer();
            OnPropertyChanged("");
            StartIdleTimer();
        }
        private List<string> ResetPage()
        {
            return new List<string>() { "1", "0", "0", "0", "0" };
        }
        private void PageDisplay(CartPage page)
        {
            List<string> _pagesZIndex;

            switch (page)
            {
                case CartPage.CartPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)CartPage.CartPage] = "10";
                    break;
                case CartPage.FinalPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)CartPage.FinalPage] = "10";
                    break;
                case CartPage.PaymentPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)CartPage.PaymentPage] = "10";
                    break;
                case CartPage.TimeOutPage:

                    _pagesZIndex = pagesZIndex;
                    if (pagesZIndex[(int)CartPage.PaymentPage] == "10")
                    {
                        
                        _pagesZIndex[(int)CartPage.TimeOutPage] = "10";
                        TimeOutMessageOKCmd = new DelegateCommand(() => {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)CartPage.TimeOutPage] = "0";
                                GoToPage(CartPage.PaymentPage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    else if (pagesZIndex[(int)CartPage.CartPage] == "10")
                    {
                        
                        _pagesZIndex[(int)CartPage.TimeOutPage] = "10";
                        TimeOutMessageOKCmd = new DelegateCommand(() => {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)CartPage.TimeOutPage] = "0";
                                GoToPage(CartPage.CartPage);
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
        
        }


        #region MSGTIMEOUT_TIMER

        private System.Windows.Threading.DispatcherTimer msgTimeoutTimer;
        private int msgTimeoutCount = 20;
        private void msgTimeoutTimer_Tick(object sender, EventArgs e)
        {
            MsgTimeoutCount--;
            Debug.WriteLine("CountDowm_Tick... " + MsgTimeoutCount);
            if (this.MsgTimeoutCount <= 0)
            {

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
        public FormCart2022VM()
        {
            _syncContext = System.Threading.SynchronizationContext.Current;
            CloseButtonEnable = false;
            string Msg = "";
            foreach (string str in PRPosUtils.PaymentFormMessage)
            {
                Msg += (Msg == "" ? "" : "\n") + str;
            }
            PaymentMsg = Msg;
            Msg = "";
            foreach (string str in PRPosUtils.FinalFormMessage)
            {
                Msg += (Msg == "" ? "" : "\n") + str;
            }
            MessageFinalPage = Msg;

            BannerImagePathList = GetBannersByCode("banner_image").Result;
            if (BannerImagePathList.Count > 0)
            {
                this.CurrentBannerImagePath = BannerImagePathList[0].BannerImagePath;
            }
            //var tenders = PRPosUtils.Tenders.Where(t => t.Tender_code ==PRPosUtils.PayEFTTenderCode || t.Tender_code== PRPosUtils.PayCashTenderCode ).ToList();
            this.displayPayEFTPOS = false;
            this.displayPayCash = false;
            var tender = PRPosUtils.Tenders.Where(t => t.Tender_code == PRPosUtils.PayEFTTenderCode && t.Eftpos_flag=="Y").FirstOrDefault();
            if (tender != null)
                if (tender.Disp_flag == "Y") this.displayPayEFTPOS = true;
            tender = PRPosUtils.Tenders.Where(t => t.Tender_code == PRPosUtils.PayCashTenderCode && t.Eftpos_flag == "N").FirstOrDefault();
            if (tender != null)
                if (tender.Disp_flag == "Y") this.displayPayCash = true;
            
            if (idleTimer == null)
            {
                idleTimer = new DispatcherTimer();
                idleTimer.Interval = TimeSpan.FromSeconds(1);
                idleTimer.Tick += IdleTimeoutTimer_Tick;
            }
            idleTimer.Stop();
            imgIndex = -1;
            StartBannerImageTimer();

            if (showFinalTimer == null)
            {
                showFinalTimer = new DispatcherTimer();
                showFinalTimer.Interval = TimeSpan.FromSeconds(PRPosUtils.AlterDisplayTime);
                showFinalTimer.Tick += ShowFinalTimer_Tick; ;
            }
            showFinalTimer.Stop();

            this.BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);

            this.PaymanetBackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, (PRPosUtils.Img_Payment.Equals("") ? PRPosUtils.Img_blank : PRPosUtils.Img_Payment));


            this.FinalBackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, (PRPosUtils.Img_Finally.Equals("") ? PRPosUtils.Img_blank : PRPosUtils.Img_Finally));
            //this.TimeOutMessageOKCmd = new DelegateCommand(() => DoTimeOutOK());
            this.TimeOutMessageNoCmd = new DelegateCommand(() => DoTimeOutNo());

            this.MinusClick = new DelegateCommand<PRPos.Data.PSTrn02sClass>(MinusClickAction);
            this.PlusClick = new DelegateCommand<PRPos.Data.PSTrn02sClass>(PlusClickAction);

            this.RemoveItemClick = new DelegateCommand<PRPos.Data.PSTrn02sClass>(RemoveItemClickAction);
            this.ModifyItemClick = new DelegateCommand<PRPos.Data.PSTrn02sClass>(ModifyItemClickAction);

            this.PayButtonEnable = true;
            this.CancelButtonEnable = false;
            PageDisplay(CartPage.CartPage);
        }

        private void ShowFinalTimer_Tick(object sender, EventArgs e)
        {
            showFinalTimer.Stop();
            DoCloseAction();
        }
        #region BannerImage
        private async Task LoopBannerImage()
        {
            if (bannerImageTimer != null)
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
        private void StartBannerImageTimer()
        {
            if (bannerImageTimer == null)
            {
                int bannerTime = PRPosUtils.BannerPlayTime != null ? PRPosUtils.BannerPlayTime : 10;
                this.bannerImageTimer = new System.Windows.Threading.DispatcherTimer();
                this.bannerImageTimer.Tick += imageTimer_Tick;
                this.bannerImageTimer.Interval = new TimeSpan(0, 0, bannerTime);
            }
            this.bannerImageTimer.Start();
        }
        private void StopBannerimageTimer()
        {
            if (bannerImageTimer != null)
            {
                bannerImageTimer.Stop();
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
        #endregion
        public bool CanClose()
        {
            return true;
        }
        void DoCloseAction()
        {
            CloseButtonEnable = false;
            if (bannerImageTimer != null)
            {
                bannerImageTimer.Stop();
                this.bannerImageTimer.Tick -= imageTimer_Tick;
                bannerImageTimer = null;
            }
            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Tick -= IdleTimeoutTimer_Tick;
                idleTimer = null;
            }
            if (showFinalTimer != null)
            {
                showFinalTimer.Stop();
                showFinalTimer.Tick -= ShowFinalTimer_Tick;
                showFinalTimer = null;
            }
            
            CloseTranscation?.Invoke(CurrentDeal);
        }
        private async Task DoTimeOutOK()
        {
           // PageDisplay(CartPage.CartPage);            
           // ResetIdleTimer();
            // reset timer
        }
        private async Task DoTimeOutNo()
        {
            //PageDisplay(CartPage.CartPage);
            if (bannerImageTimer != null)
            {
                bannerImageTimer.Stop();
                this.bannerImageTimer.Tick -= imageTimer_Tick;
                bannerImageTimer = null;
            }
            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Tick -= IdleTimeoutTimer_Tick;
                idleTimer = null;
            }
            VMClose?.Invoke(TimeOutStatus.TimeOut);
        }

        void CartCancelCommandAction()
        {
            if (bannerImageTimer != null)
            {
                bannerImageTimer.Stop();
                this.bannerImageTimer.Tick -= imageTimer_Tick;
                bannerImageTimer = null;
            }
            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Tick -= IdleTimeoutTimer_Tick;
                idleTimer = null;
            }
            VMClose?.Invoke(TimeOutStatus.None);
        }
        void BackToCartCommandAction()
        {
            Debug.WriteLine("BackToCartCommandAction ");
            ResetIdleTimer();
            PageDisplay(CartPage.CartPage);

        }
        void ConfirmOrderCmdAction()
        {
            if (PayButtonEnable)
            {
                Debug.WriteLine("ConfirmOrderCmdAction ");
                ResetIdleTimer();
                PageDisplay(CartPage.PaymentPage);
            }
        }
        void ChangeOrderTypeCommandAction()
        {
            if (CurrentDeal.Order_type == PRPos.Data.OrderType.DINING.ToString())
            {
                CurrentDeal.Order_type = PRPos.Data.OrderType.TAKEWAY.ToString();
                this.OrderType = "TAKEWAY";
                OrderTypeMsg = "CHANGE TO DINE IN";
            }
            else
            {
                CurrentDeal.Order_type = PRPos.Data.OrderType.DINING.ToString();
                this.OrderType = "DINE IN";
                OrderTypeMsg = "CHANGE TO TAKEWAY";
            }
            ResetIdleTimer();
        }
        public void ResetIdleTimer()
        {
            idleTimer.Stop();
            var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
            timeRemain = seconds;
            idleTimer.Start();
            Debug.WriteLine("ResetIdleTimer " + timeRemain);
        }

        public void StartIdleTimer()
        {
            Debug.WriteLine("StartIdleTimer "+ timeRemain);
            idleTimer.Stop();            
            var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
            timeRemain = seconds;
            idleTimer.Start();
        }
        public  void StopIdleTimer()
        {
            if (idleTimer != null)
            {
                idleTimer.Stop();
            }
        }
        private void IdleTimeoutTimer_Tick(object sender, EventArgs e)
        {
            timeRemain--;
            //Debug.WriteLine("IdleTimeoutTimer_Tick :" + timeRemain+","+ CountDown);
            if (this.timeRemain == 0)
            {
             //   Debug.WriteLine("DisplayTimeoutMsg :" + CountDown);
                //DoClose();
                // Close?.Invoke();
                CountDown = PRPosUtils.AlterDisplayTime;
                PageDisplay(CartPage.TimeOutPage);

                App.log.Info("DisplayTimeoutMsg :" + CountDown);
                
            }
            else if (this.timeRemain < 0)
            {
                CountDown--;
                if (CountDown <= 0)
                {
                    bannerImageTimer.Stop();
                    this.bannerImageTimer.Tick -= imageTimer_Tick;
                    bannerImageTimer = null;
                    idleTimer.Stop();
                    idleTimer.Tick -= IdleTimeoutTimer_Tick;
                    idleTimer = null;
                    App.log.Info("Payment CountDown 0");
                    
                    VMClose?.Invoke(TimeOutStatus.TimeOut);
                }
                
            }

        }
        async void EFTPOSCancelCommandAction()
        {
            Debug.WriteLine("EFTPOSCancelCommandAction ");
            App.log.Info("EFTPOSCancelCommandAction ");
            try
            {
                EftCtrl.DataField = "100";
                EftCtrl.CsdReservedMethod1();
            }
            catch (Exception err)
            {
                App.log.Info("EFTPOSCancelCommandAction " + err.Message);
            }
        }
        async void PayByEFTPOSCommandAction()
        {
            StopIdleTimer();
            PayButtonEnable = false;

            Debug.WriteLine("PayByEFTPOSCommandAction ");
            App.log.Info("PayByEFTPOSCommandAction");
            App.log.Info("StopIdleTimer");
            try
            {
                // transcation prepare.....
                int deal_no = PRPosDB.getDealNo();
                ReceiptText = "";
                DateTime ctime = DateTime.Now;
                this.currentDeal.Tdate = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                this.CurrentDeal.Deal_No = deal_no.ToString("000"); // PRPosUtils.Trn01.AccDate.Value.ToString("yyMMdd") + deal_no.ToString("0000");
                PRPosDB.addDealNo(this.CurrentDeal.Deal_No);

                foreach (var d in CurrentDeal.OrderItems)
                {
                    d.Deal_No = this.CurrentDeal.Deal_No;
                    foreach (var m in d.Modifiers)
                    {
                        m.Deal_No = this.CurrentDeal.Deal_No;
                    }
                }

                eftCtrl.EnableErrorDialog = false;
                EftCtrl.DialogType = "30";

                int AMT = (int)(this.CurrentDeal.Tot_amt * 100);
                string strAMT = string.Format("{0}", AMT);

                string PAD =
                    "OPR" + string.Format("{0:D3}", "99|KIOSK".Length) + "99|KIOSK" +
                                                "AMT" + string.Format("{0:D3}", strAMT.Length) + strAMT +
                                                "PCM0010";
                EftCtrl.TxnType = "P";
                EftCtrl.PosProductId = "TXN_TAG_DATA";
                EftCtrl.TxnRef = DateTime.ParseExact(this.CurrentDeal.Opentime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture).ToString("yyMMddHHmmss") + CurrentDeal.Deal_No;
                eftCtrl.DialogTitle = "PLEASE SWIPE YOUR CARD";
                PaymentMsg = "PLEASE SWIPE YOUR CARD";
                EftCtrl.AmtPurchase = this.CurrentDeal.Tot_amt;
                EftCtrl.AmtCash = 0;
                EftCtrl.CutReceipt = false;
                EftCtrl.ReceiptAutoPrint = false;
                EftCtrl.PurchaseAnalysisData = PAD;
                /*
                EventHandler handler = null;
                EventHandler displayHanlder = null;

                AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEventHandler PrintReceiptHandler = null;

                handler = new EventHandler(async delegate (Object o, EventArgs a)
              {


              });

                PrintReceiptHandler = new AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEventHandler(delegate (Object o, AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEvent a)
               {
                  

               });
                displayHanlder = new EventHandler(delegate (Object o, EventArgs a)
                {

                
                });
                */
                // Debug.WriteLine(EftCtrl.CsdReservedMethod3);

                EftCtrl.DisplayEvent += OnDisplayHanlder;
                EftCtrl.PrintReceiptEvent += OnPrintReceiptHandler;
                EftCtrl.TransactionEvent += OnTransactionEvent;
                App.log.Info("EftCtrl DoTransaction");
                EftCtrl.DoTransaction();
            }
            catch (Exception err)
            {
                App.log.Info("PayByEFTPOSCommandAction " + err.Message);
                MessageFinalPage = err.Message;
            }
        }
        private void OnPrintReceiptHandler(Object o, AxCSDEFTLib._DCsdEftEvents_PrintReceiptEventEvent a)
        {
            Debug.WriteLine("receiptType :" + a.receiptType);
            App.log.Info("receiptType :" + a.receiptType);
            if (a.receiptType == "R")
            {

                Debug.WriteLine("PrintReceiptHandler===========");
                App.log.Info("========PrintReceiptHandler===========");
                Debug.WriteLine(EftCtrl.Receipt);
                ReceiptText = EftCtrl.Receipt;
                App.log.Info(EftCtrl.Receipt); ;
            }
        }
        private void OnTransactionEvent(Object o, EventArgs a)
        {
            App.log.Info("OnTransactionEvent " + EftCtrl.Success);

            Directory.CreateDirectory(PRPosUtils.Spool_Folder);
            App.log.Info(" Write Eft LOG");
            using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + CurrentDeal.Pos_No + "_" + DateTime.ParseExact(this.CurrentDeal.Opentime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + CurrentDeal.Deal_No + ".txn", FileMode.CreateNew))
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
                App.log.Info(" Eft Success");
                PaymentMsg = "Transcaton Success!! Send order to Kitchen now. ";
                /********* Success ************/
                if (EftCtrl.ResponseCode == "00")
                {
                    App.log.Info("Transcaton ResponseCode " + EftCtrl.ResponseCode); ;

                    DateTime ctime = DateTime.Now;
                    CurrentDeal.Closetime = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                    CurrentDeal.Sendtime = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                    var tenders = PRPosUtils.Tenders.Where(t => t.Tender_code == PRPosUtils.PayEFTTenderCode ).FirstOrDefault();
                    PRPos.Data.PSTrn03sClass pstrn03 = new PRPos.Data.PSTrn03sClass()
                    {
                        CustomerId = CurrentDeal.CustomerId,
                        Store_Code = CurrentDeal.Store_Code,
                        Pos_No = CurrentDeal.Pos_No,
                        AccDate = CurrentDeal.AccDate,
                        Deal_No = CurrentDeal.Deal_No,
                        Dc_code = "D",
                        Item_No = 1,
                        Ecp_type = PRPosUtils.PayEFTTenderCode,
                        Ecp_amt = EftCtrl.AmtPurchase,
                        Ecp_name = tenders==null ? PRPosUtils.PayEFTTenderCode : tenders.Tender_name,
                        Change_amt = 0,
                        Memo = EftCtrl.AuthCode + "/" + EftCtrl.Date + "," + EftCtrl.Time,
                        Epc_code = EftCtrl.Pan,
                        Ref_code1 = "P" + EftCtrl.TxnRef,
                        Ref_code2 = EftCtrl.Stan.ToString("000000")
                    };
                    CurrentDeal.Payments = new ObservableCollection<PRPos.Data.PSTrn03sClass>();
                    CurrentDeal.Payments.Add(pstrn03);
                    /* save transcation */
                    SNSelfOrder.DAL.TranscationDAL transcationDAL = new DAL.TranscationDAL(PRPosDB.cnStr);
                    transcationDAL.SetTranscation(CurrentDeal);
                    transcationDAL.SetEFTPOSTransRef(EftCtrl.TxnRef);
                    string JsonData = JsonConvert.SerializeObject(CurrentDeal);

                    using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + CurrentDeal.Pos_No + "_" + DateTime.ParseExact(CurrentDeal.Opentime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + CurrentDeal.Deal_No + ".txt", FileMode.CreateNew))
                    {
                        StreamWriter swWriter = new StreamWriter(fs);
                        swWriter.WriteLine(JsonData);
                        swWriter.Close();
                        fs.Close();
                    }
                    using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + CurrentDeal.Pos_No + "_" + DateTime.ParseExact(this.CurrentDeal.Opentime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + CurrentDeal.Deal_No + ".rcp", FileMode.CreateNew))
                    {
                        StreamWriter swWriter = new StreamWriter(fs);
                        swWriter.WriteLine(ReceiptText);
                        swWriter.Close();
                        fs.Close();
                    }
                   // Debug.WriteLine("ReceiptPrinterName:" + PRPosUtils.ReceiptPrinterName);
                    Helpers.ReceiptPrinter receiptPrinter = new Helpers.ReceiptPrinter();
                    
                    
                        receiptPrinter.SetPrinter(PRPosUtils.PosPrinters); ;
                        receiptPrinter.PrintingReceipt(currentDeal);
                        receiptPrinter.PrintingCardReceipt(ReceiptText);


                    Helpers.LabelPrinter labelPrinter = new Helpers.LabelPrinter();
                    labelPrinter.SetPrinter(PRPosUtils.PosPrinters);
                    labelPrinter.PrintingLabel(currentDeal,60);

                    Helpers.KitchenPrinter kitchenPrinter = new Helpers.KitchenPrinter();
                    kitchenPrinter.SetPrinter(PRPosUtils.PosPrinters);
                    kitchenPrinter.PrintingKitchenOrder(currentDeal);

                    /*   STOP TIMER AND CLOSE     **/


                    PageDisplay(CartPage.FinalPage);

                    string Msg = "";
                    foreach (string str in PRPosUtils.FinalFormMessage)
                    {
                        Msg += (Msg == "" ? "" : "\n") + str;
                    }
                    MessageFinalPage = Msg;

                    // MessageFinalPage = "Please Get Your Recipt And Wait At The Counter";
                    CloseButtonEnable = true;
                    showFinalTimer.Start();
                }
                /// Debug.WriteLine("DoGetLastTransaction Return  " + EftCtrl.TxnRef);                    
            }
            else
            {
                // Debug.WriteLine("DoGetLastTransaction Return  " + EftCtrl.ResponseCode);

                PaymentMsg = "Transcaton Failure: " + EftCtrl.ResponseText;

                ResetIdleTimer();
                PayButtonEnable = true;
                CancelButtonEnable = false;
            }

            EftCtrl.TransactionEvent -= OnTransactionEvent;
            EftCtrl.PrintReceiptEvent -= OnPrintReceiptHandler;
            EftCtrl.DisplayEvent -= OnDisplayHanlder;
        }
        private void OnDisplayHanlder(Object o, EventArgs a)
        {
            App.log.Info("[" + EftCtrl.DataField + "]");
            App.log.Info("[" + EftCtrl.CsdReservedString1 + "]");
            PaymentMsg = EftCtrl.DataField.Substring(0, 20).Trim() + "\n" + EftCtrl.DataField.Substring(20, 20).Trim();
            if (EftCtrl.CsdReservedString1.Substring(0, 1) == "1")
            {
                CancelButtonEnable = true;
            }
            else
            {
                CancelButtonEnable = false;
            }
        }

        async void PayByCASHCommandAction()
        {
            // Debug.WriteLine("PayByCASHCommandAction ");
            PayButtonEnable = false;
            CancelButtonEnable = false;
            StopIdleTimer();

            Debug.WriteLine("PayByCASHCommandAction ");
            App.log.Info("PayByCASHCommandAction");
            try
            {
                // transcation prepare.....
                int deal_no = PRPosDB.getDealNo();
                ReceiptText = "";
                DateTime ctime = DateTime.Now;
                this.currentDeal.Tdate = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                this.CurrentDeal.Deal_No = deal_no.ToString("000"); // PRPosUtils.Trn01.AccDate.Value.ToString("yyMMdd") + deal_no.ToString("0000");
                PRPosDB.addDealNo(this.CurrentDeal.Deal_No);

                foreach (var d in CurrentDeal.OrderItems)
                {
                    d.Deal_No = this.CurrentDeal.Deal_No;
                    foreach (var m in d.Modifiers)
                    {
                        m.Deal_No = this.CurrentDeal.Deal_No;
                    }
                }

                CurrentDeal.Closetime = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                CurrentDeal.Sendtime = ctime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat);
                var tenders = PRPosUtils.Tenders.Where(t => t.Tender_code == PRPosUtils.PayCashTenderCode).FirstOrDefault();
                PRPos.Data.PSTrn03sClass pstrn03 = new PRPos.Data.PSTrn03sClass()
                {
                    CustomerId = CurrentDeal.CustomerId,
                    Store_Code = CurrentDeal.Store_Code,
                    Pos_No = CurrentDeal.Pos_No,
                    AccDate = CurrentDeal.AccDate,
                    Deal_No = CurrentDeal.Deal_No,
                    Dc_code = "D",
                    Item_No = 1,
                    Ecp_type = PRPosUtils.PayCashTenderCode,
                    Ecp_amt = this.CurrentDeal.Tot_amt,
                    Ecp_name = tenders==null? PRPosUtils.PayCashTenderCode : tenders.Tender_name,
                    Change_amt = 0,
                    Memo = "",
                    Epc_code = "",
                    Ref_code1 = "",
                    Ref_code2 = "",
                };
                CurrentDeal.Payments = new ObservableCollection<PRPos.Data.PSTrn03sClass>();
                CurrentDeal.Payments.Add(pstrn03);
                /* save transcation */
                SNSelfOrder.DAL.TranscationDAL transcationDAL = new DAL.TranscationDAL(PRPosDB.cnStr);
                transcationDAL.SetTranscation(CurrentDeal);

                string JsonData = JsonConvert.SerializeObject(CurrentDeal);

                using (FileStream fs = new FileStream(PRPosUtils.Spool_Folder + "/" + CurrentDeal.Pos_No + "_" + DateTime.ParseExact(CurrentDeal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture).ToString("yyMMdd") + "_" + CurrentDeal.Deal_No + ".txt", FileMode.CreateNew))
                {
                    StreamWriter swWriter = new StreamWriter(fs);
                    swWriter.WriteLine(JsonData);
                    swWriter.Close();
                    fs.Close();
                }
                
                Helpers.ReceiptPrinter receiptPrinter = new Helpers.ReceiptPrinter();
                
                receiptPrinter.SetPrinter(PRPosUtils.PosPrinters);
                receiptPrinter.PrintingReceipt(currentDeal);
/*
                Helpers.LabelPrinter labelPrinter = new Helpers.LabelPrinter();
                labelPrinter.SetPrinter(PRPosUtils.PosPrinters);
                labelPrinter.PrintingLabel(currentDeal, 60);
*/
                Helpers.KitchenPrinter kitchenPrinter= new Helpers.KitchenPrinter();
                kitchenPrinter.SetPrinter(PRPosUtils.PosPrinters);
                kitchenPrinter.PrintingKitchenOrder(currentDeal);

                /*   STOP TIMER AND CLOSE     **/

                PageDisplay(CartPage.FinalPage);
                App.log.Info("DisplayFinalPage " );

                string Msg = "";
                foreach (string str in PRPosUtils.FinalPayCountMessage)
                {
                    Msg += (Msg == "" ? "" : "\n") + str;
                }
                
                MessageFinalPage = Msg;
                CloseButtonEnable = true;
                showFinalTimer.Start();
            }
            catch (Exception err)
            {
                App.log.Info("PayByCASHCommandAction " + err.Message);
                MessageFinalPage = err.Message;
                ResetIdleTimer();
            }
        }
    }
}
