
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

namespace SNSelfOrder.ViewModel
{
    enum ItemPage
    {   // order same xmal
        BackgroundPage,  //0
        ItemDetailPage,  //1 
        QtyPage, //2        
        RequiredPage, //3
        TimeOutPage, //4
    }
    public class FormItem2022VM : ViewModelBase, ICloseWindows, ITimeOut, IOrderTranscation
    {        
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;
        private int mItemImageHeight = 300;
        private int mItemImageWidth = 300;
        private int varietyHeight = 140;
        private int varietyWidth = 70;

        private bool displayTimeoutMsg = false;

        private bool displayToppingQtyDialog = false;

        private bool displayRequiredMsg = false;

        private string requiredMessage = "";

        private bool applyIsVisible = false;
        private Thickness mImageMargin = new Thickness(0);

        private int countDown = PRPosUtils.AlterDisplayTime;
        private int timeRemain = PRPosUtils.WaitingTime;

        private List<string> pagesZIndex = new List<string>() { "1", "10", "0", "0", "0" };
        private ItemPage CurrentPage = ItemPage.ItemDetailPage;

        private string backgroundImagePath;

        private bool[] isToppingQty = new bool[10] { false, false, false, false, false, false, false, false, false, false };
        private bool windowIsVisible = true;

        private int toppingQyy = 0;

        private PRPos.Data.ItemVarietyClass mSelectedVariety;

        private PRPos.Data.FastKeyClass mSelectedItem;

        private PRPos.Data.PsModsetDT selected_modifier = null;

        private int mQty = 1;
        private decimal mItemPrice;
        private decimal mModifierPrice;
        private decimal mItemAmt;
        private ObservableCollection<PRPos.Data.PsModset01> varietyModSets = new ObservableCollection<PRPos.Data.PsModset01>();
        private ObservableCollection<PRPos.Data.ItemVarietyClass> itemVarietys = new ObservableCollection<PRPos.Data.ItemVarietyClass>();
        

        public int ItemImageHeight { get => mItemImageHeight; set { SetProperty(ref mItemImageHeight, value); } }

        public int ItemImageWidth { get => mItemImageWidth; set { SetProperty(ref mItemImageWidth, value); } }

        public int VrietyHeight { get { return varietyHeight; } set { varietyHeight = value; OnPropertyChanged("VrietyHeight"); } }
        public int VarietyWidth { get { return varietyWidth; } set { SetProperty(ref varietyWidth, value); } }

        public bool DisplayTimeoutMsg { get { return displayTimeoutMsg; } set { displayTimeoutMsg = value; OnPropertyChanged("DisplayTimeoutMsg"); } }
        public bool DisplayToppingQtyDialog { get { return displayToppingQtyDialog; } set { displayToppingQtyDialog = value; OnPropertyChanged("DisplayToppingQtyDialog"); } }
        public bool DisplayRequiredMsg { get { return displayRequiredMsg; } set { displayRequiredMsg = value; OnPropertyChanged("DisplayRequiredMsg"); } }
        public string RequiredMessage { get { return requiredMessage; } set { SetProperty(ref requiredMessage, value); } }
        public Thickness ImageMargin { get => mImageMargin; set { SetProperty(ref mImageMargin, value); } }
        public int CountDown { get => countDown; set { SetProperty(ref countDown, value); } }            
        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref backgroundImagePath, value); } }
        
        public int ItemDescHeight { get { return mItemImageHeight + (int)ImageMargin.Top + (int)ImageMargin.Bottom; } }                
        public string ItemImagePath { get { return System.IO.Path.Combine(PRPosUtils.FilePath, SelectedItem.ButtonImgURI); } }        
       
        public bool[] IsToppingQty { get { return isToppingQty; } }

        public string ItemDescription { get { return SelectedItem.Description; } }

        public string ItemName { get { return SelectedItem.ItemName; } }
        
        public string ItemCaption { get { return SelectedItem.Caption; } }
        public int WindowHeight { get => mHeight; set { SetProperty(ref mHeight, value); } }

        public int WindowWidth { get => mWidth; set { SetProperty(ref mWidth, value); } }

        public int WindowLeft { get => mLeft; set { SetProperty(ref mLeft, value); } }

        public int WindowTop { get => mTop; set { SetProperty(ref mTop, value); } }

        
        public bool WindowIsVisible { get { return this.windowIsVisible; } set { SetProperty(ref windowIsVisible, value); } }
        public ICommand VisibleTrigger { get; set; }

        private DispatcherTimer idleTimer;
        #region TIMEOUT
        private void IdleTimeoutTimer_Tick(object sender, EventArgs e)
        {
            timeRemain--;
            //Debug.WriteLine("ForItem2022VM IdleTimeoutTimer_Tick : "+ timeRemain+","+ CountDown);
            if (this.timeRemain == 0)
            {
                //DoClose();
                // Close?.Invoke();
                CountDown = PRPosUtils.AlterDisplayTime;


                PageDisplay(ItemPage.TimeOutPage);
            }
            else if (this.timeRemain <= 0)
            {
                CountDown--;
                if (CountDown <= 0)
                {
                    StopIdleTimer();
                    idleTimer.Tick -= IdleTimeoutTimer_Tick;
                    idleTimer = null;
                    VMClose?.Invoke(TimeOutStatus.TimeOut);
                }
            }
        }
        public void StartIdleTimer()
        {

            idleTimer.Stop();

            var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
            timeRemain = seconds;
            idleTimer.Start();
            // Debug.WriteLine("ResetIdleTimer " + timeRemain);
        }

        public void StopIdleTimer()
        {
            if (idleTimer != null)
            {
                idleTimer.Stop();
            }
        }

        public void ResetIdleTimer()
        {

            idleTimer.Stop();
            var seconds = PRPosUtils.WaitingTime > 0 ? PRPosUtils.WaitingTime : 40;
            timeRemain = seconds;
            idleTimer.Start();
            Debug.WriteLine("ResetIdleTimer " + timeRemain);
        }

        #endregion
        private void IsVisibleChangedAction()
        {
            Debug.WriteLine("IsVisibleChangedAction by DelegateCommand ");
#if _DEBUG

#endif
            OnPropertyChanged("");
        }
        private DelegateCommand itemAddCommand;

        private DelegateCommand itemUpdateCommand;

        private DelegateCommand itemCancelCommand;

        private DelegateCommand itemIncrementCommand;

        private DelegateCommand itemDecrementCommand;       
        public DelegateCommand ItemAddCommand => itemAddCommand ?? (itemAddCommand = new DelegateCommand(ItemAddCommandAction));

        public DelegateCommand ItemUpdateCommand => itemUpdateCommand ?? (itemUpdateCommand = new DelegateCommand(ItemUpdateCommandAction));

        public DelegateCommand ItemIncrementCommand => itemIncrementCommand ?? (itemIncrementCommand = new DelegateCommand(ItemIncrementCommandAction));

        public DelegateCommand ItemDecrementCommand => itemDecrementCommand ?? (itemDecrementCommand = new DelegateCommand(ItemDecrementCommandAction));
        public DelegateCommand ItemCancelCommand => itemCancelCommand ?? (itemCancelCommand = new DelegateCommand(ItemCancelCommandAction));

        public List<string> PagesZIndex { get => pagesZIndex; set { SetProperty(ref pagesZIndex, value); } }

        private void GoToPage(ItemPage page)
        {
            CurrentPage = page;
            StopIdleTimer();            
            OnPropertyChanged("");
            StartIdleTimer();
        }
        private List<string> ResetPage()
        {
            return new List<string>() { "1", "0", "0", "0", "0" };
        }
        private void PageDisplay(ItemPage page)
        {
            List<string> _pagesZIndex;

            switch (page)
            {
                case ItemPage.ItemDetailPage:
                    _pagesZIndex = ResetPage();
                    _pagesZIndex[(int)ItemPage.ItemDetailPage] = "10";
                    break;
                case ItemPage.QtyPage:
                    _pagesZIndex = pagesZIndex;
                    _pagesZIndex[(int)ItemPage.QtyPage] = "10";
                    break;
                case ItemPage.RequiredPage:
                    _pagesZIndex = pagesZIndex;
                    _pagesZIndex[(int)ItemPage.RequiredPage] = "10";
                    break;
                case ItemPage.TimeOutPage:

                    _pagesZIndex = pagesZIndex;
                    if (pagesZIndex[(int)ItemPage.ItemDetailPage] == "10")
                    {

                        _pagesZIndex[(int)ItemPage.TimeOutPage] = "10";
                        TimeOutMessageOKCmd = new DelegateCommand(() => {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)ItemPage.TimeOutPage] = "0";
                                GoToPage(ItemPage.ItemDetailPage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    else if (pagesZIndex[(int)ItemPage.QtyPage] == "10")
                    {

                        _pagesZIndex[(int)ItemPage.TimeOutPage] = "10";
                        TimeOutMessageOKCmd = new DelegateCommand(() => {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)ItemPage.TimeOutPage] = "0";
                                GoToPage(ItemPage.QtyPage);
                            };
                            _syncContext.Post(methodDelegate, null);
                        });
                    }
                    else if (pagesZIndex[(int)ItemPage.RequiredPage] == "10")
                    {

                        _pagesZIndex[(int)ItemPage.TimeOutPage] = "10";
                        TimeOutMessageOKCmd = new DelegateCommand(() => {
                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {
                                PagesZIndex[(int)ItemPage.TimeOutPage] = "0";
                                GoToPage(ItemPage.RequiredPage);
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
            //Debug.WriteLine(CurrentPage);
            OnPropertyChanged("");


        }

        void ItemIncrementCommandAction()
        {
            if (Qty < PRPosUtils.MaxQty)
                Qty++;
            // Debug.WriteLine("ItemIncrementCommandAction by DelegateCommand ");
            foreach (PRPos.Data.PsModset01 set in SelectedVariety.ModifierSets)
            {
                var mod = from m in set.Modifiers where m.InpQty > 0 select m ;
                foreach (var m in mod)
                    m.SelectedQty = Qty * m.InpQty;
                   // Debug.WriteLine("selected mod " + m);
            }
        }
        void ItemDecrementCommandAction()
        {
            //Debug.WriteLine("ItemDeccrementCommandAction by DelegateCommand ");
            if (Qty > 1)
                Qty--;
            foreach (PRPos.Data.PsModset01 set in SelectedVariety.ModifierSets)
            {
                var mod = from m in set.Modifiers where m.InpQty > 0 select m;
                foreach (var m in mod)
                    m.SelectedQty = Qty * m.InpQty;
                // Debug.WriteLine("selected mod " + m);

            }
        }
        void ItemAddCommandAction()
        {
            // check all must selected  modifier set is selected
            bool canAdd = true;
            ResetIdleTimer();
            string msgRequired = "";
            foreach (PRPos.Data.PsModset01 set in SelectedVariety.ModifierSets)
            {
                foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                {
                    int mod_maxQ = modifier.Max_selection;
                    int mod_minQ = modifier.Min_selection;
                    if (mod_minQ > 0)
                    {
                        if (modifier.InpQty < mod_minQ)
                        {
                            canAdd = false;
                            if (mod_minQ == 1)
                                msgRequired = modifier.Caption + " required one piece at minimum.";
                            else
                                msgRequired =  modifier.Caption + " required " + mod_minQ+ " pieces at minimum.";

                            break;
                        }
                    }
                    if (mod_maxQ > 0)
                    {
                        if (modifier.InpQty > mod_maxQ )
                        {
                            canAdd = false;
                            if (mod_maxQ == 1)
                                msgRequired = modifier.Caption + " required one piece at maximum.";
                            else
                                msgRequired = modifier.Caption + " required " + mod_maxQ + " pieces at maximum.";

                            break;
                        }
                    }
                }
                if (canAdd)
                {
                    int maxQ = set.Max_selection;
                    int minQ = set.Min_selection;
                    int selQty = 0;
                    foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                    {
                        if (modifier.InpQty > 0)
                        {
                            selQty += 1;
                        }
                    }
                    if (minQ > 0)
                    {
                        if (selQty < minQ)
                        {
                            canAdd = false;
                            if (minQ == 1)
                                msgRequired = set.Caption + " Required. ";
                            else
                                msgRequired = minQ + " " + set.Caption + " Required. ";

                            break;
                        }
                    }
                }
            }
            if(canAdd)
            {
                StopIdleTimer();
                idleTimer.Tick -= IdleTimeoutTimer_Tick;
                idleTimer = null;
                AddItem?.Invoke( this.SelectedItem, this.SelectedVariety, Qty, this.SelectedItem.Sprice, ItemPrice);
            }
            else
            {
                RequiredMessage = msgRequired;
                PageDisplay(ItemPage.RequiredPage);
            }
        }
        void ItemUpdateCommandAction()
        {
            bool canGoBack = true;
            ResetIdleTimer();
            string msgRequired = "";
            foreach (PRPos.Data.PsModset01 set in SelectedVariety.ModifierSets)
            {
                int maxQ = set.Max_selection;
                int minQ = set.Min_selection;
                int selQty = 0;
                foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                {
                    if (modifier.SelectedQty > 0)
                    {
                        selQty += 1;
                    }
                }
                if (minQ > 0)
                {
                    if (selQty < minQ)
                    {
                        canGoBack = false;
                        if (minQ == 1)
                            msgRequired = set.Caption + " Required. ";
                        else
                            msgRequired = minQ + " " + set.Caption + " Required. ";

                        break;
                    }
                }
            }
            if (canGoBack)
            {
                StopIdleTimer();
                idleTimer.Tick -= IdleTimeoutTimer_Tick;
                idleTimer = null;
                UpdateItem?.Invoke(this.SelectedItem, this.SelectedVariety, Qty, this.SelectedItem.Sprice, ItemPrice);
            }
            else
            {
                RequiredMessage = msgRequired;
                PageDisplay(ItemPage.RequiredPage);
            }
        }
        void ItemCancelCommandAction()
        {
            // Debug.WriteLine("ItemCancelCommandAction by DelegateCommand ");
            //OnPropertyChanged("");
            StopIdleTimer();
            idleTimer.Tick -= IdleTimeoutTimer_Tick;
            idleTimer = null;
            VMClose?.Invoke(TimeOutStatus.None);
        }
        public Action<TimeOutStatus> VMClose { get; set; }
        public bool CanClose()     {          return true;      }
        #region IOrderTranscation
        public Action<PRPos.Data.FastKeyClass, PRPos.Data.ItemVarietyClass, int,decimal, decimal> AddItem { get; set; }
        public Action<PRPos.Data.FastKeyClass, PRPos.Data.ItemVarietyClass> RemoveItem { get; set; }
        public Action<PRPos.Data.FastKeyClass, PRPos.Data.ItemVarietyClass, int, decimal, decimal> UpdateItem { get; set; }
        #endregion
                        
        public int Qty { get => mQty; set { mQty = value; OnPropertyChanged("Qty"); ItemAmount = mQty * (mItemPrice + mModifierPrice); } }
        
        public decimal ItemPrice { get => mItemPrice; set { mItemPrice = value; OnPropertyChanged("ItemPrice"); ItemAmount = mQty * (mItemPrice + mModifierPrice);  } }
        
        public decimal ItemAmount { get => mItemAmt; set { mItemAmt = value; ; OnPropertyChanged("ItemAmount"); } }        
        public decimal ModifierPrice { get => mModifierPrice; set { mModifierPrice = value; OnPropertyChanged("ModifierPrice"); ItemAmount = mQty * (mItemPrice + mModifierPrice);  } }

        public ObservableCollection<PRPos.Data.PsModset01> VarietyModSets
        {
            get { return varietyModSets; }
            set { varietyModSets = value; OnPropertyChanged("VarietyModSets"); }
        }       
        public ObservableCollection<PRPos.Data.ItemVarietyClass> ItemVarietys { get { return itemVarietys; } set { itemVarietys = value; OnPropertyChanged("ItemVarietys"); } }
        
        public PRPos.Data.FastKeyClass SelectedItem
        {
            get { return this.mSelectedItem; }
            set
            {
                SetProperty(ref mSelectedItem, value);
                using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;
                    cmd.CommandText = "select * from itemvariety where customerid=@customerid and store_code=@store_code and item_code=@item_code and del_flag='N' order by disp_order";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("item_code", value.Selected);

                    DataTable itemvarietyDT = new DataTable();
                    da.Fill(itemvarietyDT);
                    ObservableCollection<PRPos.Data.ItemVarietyClass> objItemVarietys = new ObservableCollection<PRPos.Data.ItemVarietyClass>();
                    //for (int i = 0; i < itemvarietyDT.Rows.Count; i++)
                    foreach (DataRow VarietyRow in itemvarietyDT.Rows)
                    {

                        PRPos.Data.ItemVarietyClass varity =
                           new PRPos.Data.ItemVarietyClass()
                           {
                               Customerid = VarietyRow["customerid"].ToString(),
                                Store_code = VarietyRow["store_code"].ToString(),
                               Item_code = VarietyRow["item_code"].ToString(),
                               Variety_code = VarietyRow["variety_code"].ToString(),
                                Size_code = VarietyRow["size_code"].ToString(),
                               Caption = VarietyRow["caption"].ToString(),
                               Caption_fn = VarietyRow["caption_fn"].ToString(),
                               Description = VarietyRow["description"].ToString(),
                               Next_modset = VarietyRow["next_modset"].ToString(),                               
                               BackColor = "#FFC300",
                               FontSize = 18,
                               FontFamily = "Arial",
                               FontStyle = "Bold",
                               FontColor = "#FDFEFE",
                               TextBgColor = "#FFC300",
                               Offset = 10,
                               Default_item = VarietyRow["default_item"].ToString(),
                               Kitchen_name = VarietyRow["kitchen_name"].ToString(),
                               Kitchen_name_fn = VarietyRow["kitchen_name_fn"].ToString(),
                               Sprice1 = VarietyRow["sprice"].ToString(),
                               Sprice2 = VarietyRow["sprice2"].ToString(),
                               Sprice3 = VarietyRow["sprice3"].ToString(),
                               Sprice4 = VarietyRow["sprice4"].ToString(),
                               Sprice5 = VarietyRow["sprice5"].ToString(),
                               Sprice6 = VarietyRow["sprice6"].ToString(),
                               Sprice7 = VarietyRow["sprice7"].ToString(),
                               Sprice8 = VarietyRow["sprice8"].ToString(),
                               Sprice9 = VarietyRow["sprice9"].ToString(),
                               Sprice10 = VarietyRow["sprice10"].ToString(),                               
                               VarietyRow = VarietyRow,
                           };
                        
                        /*
                        decimal refPrice = 0;
                        decimal.TryParse(VarietyRow[PRPosUtils.SalePriceColumn].ToString(), out refPrice);
                        this.ItemPrice = refPrice;
                        */
                        cmd.CommandText = @" select itemmodifier.* from itemmodifier 
                                           where itemmodifier.customerid=@customerid and store_code=@store_code 
                                             and item_code=@item_code and variety_code=@variety_code order by disp_order";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", VarietyRow["customerid"].ToString());
                        cmd.Parameters.AddWithValue("store_code", VarietyRow["store_code"].ToString());
                        cmd.Parameters.AddWithValue("variety_code", VarietyRow["variety_code"].ToString() );
                        cmd.Parameters.AddWithValue("item_code", VarietyRow["item_code"].ToString() );
                        DataTable itemmodifierDT = new DataTable();
                        da.Fill(itemmodifierDT);

                        //Debug.WriteLine("itemmodifierDT " + mSelectedVariety.Variety_code + "," + mSelectedVariety.Item_code + "," + PRPosUtils.CustomerID + "," + PRPosUtils.StoreCode);
                        ObservableCollection<PRPos.Data.PsModset01> mModifierSet = new ObservableCollection<PRPos.Data.PsModset01>();
                        foreach (DataRow itemmodifier in itemmodifierDT.Rows)
                        {
                            cmd.CommandText = @" select psmodset01.* from psmodset01 where  psmodset01.customerid=@customerid and psmodset01.modset_code=@modset_code ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("customerid", itemmodifier["customerid"].ToString());
                            cmd.Parameters.AddWithValue("modset_code", itemmodifier["modset_code"].ToString());
                            DataTable psmodset01DT = new DataTable();
                            da.Fill(psmodset01DT);
                            foreach (DataRow psmodset01 in psmodset01DT.Rows)
                            {
                                int max_sel = 0;
                                int min_sel = 0;
                                decimal amount = 0;
                                if (!int.TryParse(psmodset01["max_selection"].ToString(), out max_sel))
                                    max_sel = 1;
                                if (!int.TryParse(psmodset01["min_selection"].ToString(), out min_sel))
                                    min_sel = 1;

                                if (!decimal.TryParse(psmodset01["amount"].ToString(), out amount))
                                    amount = 0;

                                PRPos.Data.PsModset01 set = new PRPos.Data.PsModset01()
                                {
                                    Caption = psmodset01["caption"].ToString(),
                                    Caption_fn = psmodset01["caption_fn"].ToString(),
                                    Item_code = itemmodifier["item_code"].ToString(),
                                    ModSet_code = psmodset01["modset_code"].ToString(),
                                    Mod_type = psmodset01["mod_type"].ToString(),
                                    Amount = amount,
                                    Max_selection = max_sel,
                                    Min_selection = min_sel,
                                    Sid = psmodset01["sid"].ToString(),
                                    Customerid = psmodset01["customerid"].ToString(),
                                    Store_code = itemmodifier["store_code"].ToString(),
                                    Variety_code = itemmodifier["variety_code"].ToString(),
                                    Next_modset = psmodset01["next_modset"].ToString(),

                                };

                                cmd.CommandText =
                                        @"select psmodsetti.modifier_code,psmodsetti.mod_type,psmodsetti.price_type ,psmodsetti.amount as sprice,psmodsetti.next_modset,
                                          psmodsetti.image,psmodsetti.max_selection,psmodsetti.min_selection,modifier.image as mod_image,psmodsetti.caption,
                                          psmodsetti.caption_fn,psmodset01.modset_code,psmodsetti.disp_caption,psmodsetti.disp_price,
                                          psmodsetti.soldout as modifier_soldout,psmodsetti.str_soldout
                                from psmodsetti  
                                left outer join modifier on modifier.modifier_code=psmodsetti.modifier_code
                                left outer join psmodset01 on psmodset01.sid=psmodsetti.psid                                
                                where psmodsetti.psid=@psid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("psid", psmodset01["sid"].ToString());
                                DataTable psmodsettiDT = new DataTable();
                                da.Fill(psmodsettiDT);
                                foreach (DataRow modifier in psmodsettiDT.Rows)
                                {
                                    int ModMax_sel = 0;
                                    int ModMin_sel = 0;
                                    decimal sprice = 0;
                                    if (!int.TryParse(modifier["max_selection"].ToString(), out ModMax_sel))
                                        ModMax_sel = 1;
                                    if (!int.TryParse(modifier["min_selection"].ToString(), out ModMin_sel))
                                        ModMin_sel = 1;

                                    if (!decimal.TryParse(modifier["sprice"].ToString(), out sprice))
                                        sprice = 0;
                                    PRPos.Data.PsModsetDT modifierDT = new PRPos.Data.PsModsetDT(set)
                                    {
                                       
                                        Caption = modifier["caption"].ToString(),
                                        Caption_fn = modifier["caption_fn"].ToString(),
                                        Mod_type = modifier["mod_type"].ToString(),
                                        Price_type = modifier["mod_type"].ToString(),
                                        Sprice = sprice,
                                        Max_selection = ModMax_sel,
                                        Min_selection = ModMin_sel,

                                        LocalModSoldOut = modifier["str_soldout"].ToString(),
                                        Disp_caption = modifier["disp_caption"].ToString(),
                                        Disp_price = modifier["disp_price"].ToString(),
                                        ModSoldOut = modifier["modifier_soldout"].ToString(),
                                        Next_modset = modifier["next_modset"].ToString(),
                                        Modifier_code = modifier["modifier_code"].ToString(),
                                        Picture = System.IO.Path.Combine(PRPosUtils.FilePath, modifier["image"].ToString().Equals("") ? modifier["mod_image"].ToString() : modifier["image"].ToString()),
                                        FontColor = PRPosUtils.ModifierFontColor,
                                         FontSize=PRPosUtils.ModifierFontSize.ToString(),
                                        FontFamily = PRPosUtils.ModifierFontName,
                                          FontStyle=PRPosUtils.ModifierFontStyle==2? "Bold": "Regular",
                                           KitchenName= modifier[PRPosUtils.KitchenOrderItemModifierName].ToString(),
                                    };
                                    set.Modifiers.Add(modifierDT);
                                }

                                mModifierSet.Add(set);
                            }

                        }

                        varity.ModifierSets = mModifierSet;

                        objItemVarietys.Add(varity);
                    }
                    ItemVarietys = objItemVarietys;
 
                    foreach (PRPos.Data.ItemVarietyClass c in  objItemVarietys)
                    {                        
                        VarietySelectChangedAction(c);
                        break;
                    } 
                }
            }
        }

        public ICommand VarietySelectChanged { get; set; }
        private void VarietySelectChangedAction(object param)
        {
            // Debug.WriteLine("VarietySelectChangedAction by DelegateCommand ");
            ResetIdleTimer();
#if _DEBUG
            if (param != null)
                Debug.WriteLine("VarietySelectChangedAction ==> " + (param as PRPos.Data.ItemVarietyClass).Caption);
#endif
            if (param != null)
            {
                PRPos.Data.ItemVarietyClass variety = (param as PRPos.Data.ItemVarietyClass);
                this.SelectedVariety = variety;
            }
        }
        #region _varietyUpdate_CODE

        private void varietyUpdate()
        {
            using (SQLiteConnection cn = new SQLiteConnection(PRPosDB.cnStr))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText =
                  @"select * from itemvariety where customerid=@customerid and store_code=@store_code  and item_code=@item_code and variety_code=@variety_code ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);
                cmd.Parameters.AddWithValue("variety_code", mSelectedVariety.Variety_code);
                cmd.Parameters.AddWithValue("item_code", mSelectedVariety.Item_code);
                DataTable itemvarietyDT = new DataTable();

                da.Fill(itemvarietyDT);
                if (itemvarietyDT.Rows.Count > 0)
                {
                    DataRow VarietyRow = itemvarietyDT.Rows[0];
                    decimal refPrice = 0;
                    decimal.TryParse(VarietyRow[PRPosUtils.SalePriceColumn].ToString(), out refPrice);
                    this.ItemPrice = refPrice;

                    cmd.CommandText = @" select itemmodifier.* from itemmodifier 
                                           where itemmodifier.customerid=@customerid and store_code=@store_code 
                                             and item_code=@item_code and variety_code=@variety_code order by disp_order";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", PRPosUtils.CustomerID);
                    cmd.Parameters.AddWithValue("store_code", PRPosUtils.StoreCode);
                    cmd.Parameters.AddWithValue("variety_code", mSelectedVariety.Variety_code);
                    cmd.Parameters.AddWithValue("item_code", mSelectedVariety.Item_code);
                    DataTable itemmodifierDT = new DataTable();
                    da.Fill(itemmodifierDT);

                    Debug.WriteLine("itemmodifierDT " + mSelectedVariety.Variety_code + "," + mSelectedVariety.Item_code + "," + PRPosUtils.CustomerID + "," + PRPosUtils.StoreCode);
                    ObservableCollection<PRPos.Data.PsModset01> mModifierSet = new ObservableCollection<PRPos.Data.PsModset01>();
                    foreach (DataRow itemmodifier in itemmodifierDT.Rows)
                    {
                        cmd.CommandText = @" select psmodset01.* from psmodset01 where psmodset01.modset_code=@modset_code ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("modset_code", itemmodifier["modset_code"].ToString());
                        DataTable psmodset01DT = new DataTable();
                        da.Fill(psmodset01DT);


                        foreach (DataRow psmodset01 in psmodset01DT.Rows)
                        {
                            int max_sel = 0;
                            int min_sel = 0;
                            decimal amount = 0;
                            if (!int.TryParse(psmodset01["max_selection"].ToString(), out max_sel))
                                max_sel = 1;
                            if (!int.TryParse(psmodset01["min_selection"].ToString(), out min_sel))
                                min_sel = 1;

                            if (!decimal.TryParse(psmodset01["amount"].ToString(), out amount))
                                amount = 0;

                            PRPos.Data.PsModset01 set = new PRPos.Data.PsModset01()
                            {
                                Caption = psmodset01["caption"].ToString(),
                                Caption_fn = psmodset01["caption_fn"].ToString(),
                                Item_code = mSelectedVariety.Variety_code,
                                ModSet_code = psmodset01["modset_code"].ToString(),
                                Mod_type = psmodset01["mod_type"].ToString(),
                                Amount = amount,
                                Max_selection = max_sel,
                                Min_selection = min_sel,
                                Sid = psmodset01["sid"].ToString(),
                                Customerid = psmodset01["customerid"].ToString(),
                                Store_code = PRPosUtils.StoreCode,
                                Variety_code = mSelectedVariety.Variety_code,
                                Next_modset = psmodset01["next_modset"].ToString(),

                            };


                            cmd.CommandText =
                                    @"select psmodsetti.modifier_code,psmodsetti.mod_type,psmodsetti.price_type ,psmodsetti.amount as sprice,psmodsetti.next_modset,
                                          psmodsetti.image,psmodsetti.max_selection,psmodsetti.min_selection,modifier.image as mod_image,psmodsetti.caption,
                                          psmodsetti.caption_fn,psmodset01.modset_code,psmodsetti.disp_caption,psmodsetti.disp_price,
                                          psmodsetti.soldout as modifier_soldout,psmodsetti.str_soldout
                                from psmodsetti  
                                left outer join modifier on modifier.modifier_code=psmodsetti.modifier_code
                                left outer join psmodset01 on psmodset01.sid=psmodsetti.psid                                
                                where psmodsetti.psid=@psid ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("psid", psmodset01["sid"].ToString());
                            DataTable psmodsettiDT = new DataTable();
                            da.Fill(psmodsettiDT);
                            foreach (DataRow modifier in psmodsettiDT.Rows)
                            {
                                int ModMax_sel = 0;
                                int ModMin_sel = 0;
                                decimal sprice = 0;
                                if (!int.TryParse(modifier["max_selection"].ToString(), out ModMax_sel))
                                    ModMax_sel = 1;
                                if (!int.TryParse(modifier["min_selection"].ToString(), out ModMin_sel))
                                    ModMin_sel = 1;

                                if (!decimal.TryParse(modifier["sprice"].ToString(), out sprice))
                                    sprice = 0;
                                PRPos.Data.PsModsetDT modifierDT = new PRPos.Data.PsModsetDT(set)
                                {
                                    Caption = modifier["caption"].ToString(),
                                    Caption_fn = modifier["caption_fn"].ToString(),
                                    Mod_type = modifier["mod_type"].ToString(),
                                    Price_type = modifier["mod_type"].ToString(),
                                    Sprice = sprice,
                                    Max_selection = ModMax_sel,
                                    Min_selection = ModMin_sel,

                                    LocalModSoldOut = modifier["str_soldout"].ToString(),
                                    Disp_caption = modifier["disp_caption"].ToString(),
                                    Disp_price = modifier["disp_price"].ToString(),
                                    ModSoldOut = modifier["modifier_soldout"].ToString(),
                                    Next_modset = modifier["next_modset"].ToString(),
                                    Modifier_code = modifier["modifier_code"].ToString(),
                                    Picture = System.IO.Path.Combine(PRPosUtils.FilePath, modifier["image"].ToString().Equals("") ? modifier["mod_image"].ToString() : modifier["image"].ToString()),
                                };
                                set.Modifiers.Add(modifierDT);
                            }

                            mModifierSet.Add(set);
                        }

                    }
                    mSelectedVariety.ModifierSets = mModifierSet;
                    //Debug.WriteLine("Count--->" + mSelectedVariety.ModifierSets.Count);
                }
            }
        }
        #endregion
        
        public PRPos.Data.ItemVarietyClass SelectedVariety
        {
            get { return this.mSelectedVariety; }
            set {
               
                SetProperty(ref mSelectedVariety, value);
                //  varietyUpdate();

                decimal refPrice = 0;
                //if(PRPosUtils.SalePriceColumn=="sprice")
                decimal.TryParse(value.VarietyRow[PRPosUtils.SalePriceColumn].ToString(), out refPrice);
                this.ItemPrice = refPrice;

                decimal modifierPrice = 0;

                foreach (PRPos.Data.PsModset01 set in mSelectedVariety.ModifierSets)
                {
                    foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                    {
                        if (modifier.InpQty > 0)
                        {
                            modifierPrice += modifier.InpQty * modifier.Sprice;
                        }
                    }
                }
                this.ModifierPrice = modifierPrice;
                //Debug.WriteLine("modifierPrice=> " + modifierPrice);

                //this.SetProperty(ref this.mSelectedVariety, value);
                // varietyUpdate();
                OnPropertyChanged("SelectedVariety");
 
            }
        }
        public DelegateCommand<PRPos.Data.ItemVarietyClass> ClickItemVarietyCmd { get; set; }

        public DelegateCommand RequiredMessageOkCmd { get; set; }

        public DelegateCommand TimeOutMessageOKCmd { get; set; }
        public DelegateCommand TimeOutMessageNoCmd { get; set; }
 
        private async Task DoMessageOK()
        {                       
            ResetIdleTimer();
            // reset timer
        }
        private async Task VarietyChanged(PRPos.Data.ItemVarietyClass param)
        {
             Debug.WriteLine("VarietyChanged : "+ param.IsSelected);       
        }
     
        private async Task DoCloseRequiredMessage()
        {
            //DisplayRequiredMsg = false;
            PageDisplay(ItemPage.ItemDetailPage);
            ResetIdleTimer();
        }

        private async Task DoCancelOrderYes()
        {
            StopIdleTimer();
            idleTimer.Tick -= IdleTimeoutTimer_Tick;
            idleTimer = null;
            VMClose?.Invoke(TimeOutStatus.TimeOut);
        }


        private bool ItemCommand_CanExecute(PRPos.Data.ItemVarietyClass arg)
        {
            return true;
        }

        public ICommand ModifierClick { get; set; }

        #region Modifier_CLICK_Process
        private void ModifierClickAction(Object param)
        {
            ResetIdleTimer();
            if (param != null)
            {
                selected_modifier = (param as PRPos.Data.PsModsetDT);
                // foreach(var m in ItemVarietys)
                //     Debug.WriteLine("ItemVarietys==>" + m.Variety_code);
                //  Debug.WriteLine("item_variety_code==>"+selected_modifier.ModifierSet.Max_selection+","+ selected_modifier.ModifierSet.Min_selection+","+ selected_modifier.SelectedQty);
                if (selected_modifier.ModSoldOut != "Y")
                {
                    int selectedTopping = 0;
                   
                    if (selected_modifier.ModifierSet.Max_selection == 1)
                    {
                        foreach (var m in selected_modifier.ModifierSet.Modifiers)
                        {
                            if (selected_modifier != m)
                            {
                                if (m.SelectedQty > 0)
                                {
                                    m.SelectedQty = 0;
                                    m.InpQty = 0;
                                }
                            }
                        }
                        if (selected_modifier.Max_selection > 1)
                        {
                            if (selected_modifier.SelectedQty < selected_modifier.Max_selection)
                            {
                                ToppingQty = selected_modifier.SelectedQty;
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i <= selected_modifier.Max_selection)
                                        isToppingQty[i] = true;
                                    else
                                        isToppingQty[i] = false;
                                }

                                OnPropertyChanged("IsToppingQty");
                                //DisplayToppingQtyDialog = true;
                                PageDisplay(ItemPage.QtyPage);
                            }
                        }
                        else if (selected_modifier.SelectedQty == 1)
                        {
                            selected_modifier.SelectedQty = 0;
                            selected_modifier.InpQty = 0;
                            CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                        }
                        else
                        {
                            selected_modifier.SelectedQty = 1 * Qty;
                            selected_modifier.InpQty = 1 ;
                            CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                        }
                    }
                    else if (selected_modifier.ModifierSet.Max_selection == 0)
                    {
                        if (selected_modifier.SelectedQty > 0)
                        {
                            selected_modifier.SelectedQty = 0;
                            selected_modifier.InpQty = 0;
                            CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                        }
                        else
                        {
                            if (selected_modifier.Max_selection > 1)
                            {
                                ToppingQty = selected_modifier.SelectedQty;
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i <= selected_modifier.Max_selection)
                                        isToppingQty[i] = true;
                                    else
                                        isToppingQty[i] = false;
                                }

                                OnPropertyChanged("IsToppingQty");                                
                                //DisplayToppingQtyDialog = true;
                                PageDisplay(ItemPage.QtyPage);
                            }
                            else
                            {
                                selected_modifier.SelectedQty = 1*Qty;
                                selected_modifier.InpQty = 1;
                                CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                            }
                        }
                    }
                    else
                    {
                        if (selected_modifier.SelectedQty > 0)
                        {
                            selected_modifier.SelectedQty = 0;
                            selected_modifier.InpQty = 0;
                            CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                        }
                        else
                        {
                            foreach (var m in selected_modifier.ModifierSet.Modifiers)
                            {
                                if (m.SelectedQty > 0)
                                {
                                    selectedTopping += 1;
                                }
                            }
                            if ((selectedTopping < selected_modifier.ModifierSet.Max_selection))
                            {
                                ToppingQty = selected_modifier.SelectedQty;
                                for (int i = 0; i < 10; i++)
                                {
                                    if (i <= selected_modifier.Max_selection)
                                        isToppingQty[i] = true;
                                    else
                                        isToppingQty[i] = false;
                                }

                                OnPropertyChanged("IsToppingQty");
                                //_modifier.SelectedQty = 1;
                                //DisplayToppingQtyDialog = true;
                                PageDisplay(ItemPage.QtyPage);
                            }
                        }
                    }
                }

                // Debug.WriteLine("ModifierSet   " + selected_modifier.ModifierSet.Max_selection + "," + selected_modifier.ModifierSet.Min_selection + "," + selected_modifier.Max_selection);
            }
        }
        private void CalculatePrice(string variety_code)
        {
            // reset the other variety
            foreach( PRPos.Data.ItemVarietyClass imv in this.ItemVarietys)
            {
                if (imv.Variety_code != variety_code)
                {
                    foreach( PRPos.Data.PsModset01 set in imv.ModifierSets)
                    {
                        foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                        {
                            modifier.SelectedQty = 0;                            
                        }
                    }
                }
            }
            foreach (PRPos.Data.ItemVarietyClass imv in this.ItemVarietys)
            {
                if (imv.Variety_code == variety_code)
                {
                    decimal refPrice = 0;
                    //if(PRPosUtils.SalePriceColumn=="sprice")
                    decimal.TryParse( imv.VarietyRow[PRPosUtils.SalePriceColumn].ToString(), out refPrice);
                    this.ItemPrice = refPrice;

                    decimal mPrice = 0;
                    
                    foreach (PRPos.Data.PsModset01 set in imv.ModifierSets)
                    {
                        foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                        {
                           if( modifier.SelectedQty > 0)
                            {
                                mPrice += modifier.SelectedQty * modifier.Sprice;
                            }
                        }
                    }
                    this.ModifierPrice = mPrice;
                    //Debug.WriteLine("mPrice=> " + mPrice);
                    break;
                }
            }
        }
        #endregion
        
        public int ToppingQty   {    get => toppingQyy; set { toppingQyy = value; OnPropertyChanged("ToppingQty"); }    }

        public ICommand ToppingQtyDialogButtonCmd { get; set; }
        public ICommand ToppingQtyDialogCancelCmd { get; set; }
        public bool ApplyIsVisible { get => applyIsVisible; set { applyIsVisible = value; OnPropertyChanged("ApplyIsVisible"); } }

        private void ToppingQtyDialogButtonCmdAction(object sender)
        {
            ResetIdleTimer();
            if (sender is System.Windows.Controls.Button)
            {
                if ( (string)(sender as System.Windows.Controls.Button).Content == "Enter") {
                    //DisplayToppingQtyDialog = false;
                    PageDisplay(ItemPage.ItemDetailPage);
                    selected_modifier.SelectedQty = ToppingQty*Qty;
                    selected_modifier.InpQty = ToppingQty;
                    CalculatePrice(selected_modifier.ModifierSet.Variety_code);
                }
                else
                {
                    ToppingQty = int.Parse( (string) (sender as System.Windows.Controls.Button).Content);
                }
            }
        }

        private void ToppingQtyDialogCacelCmdAction()
        {
            ResetIdleTimer();
            // DisplayToppingQtyDialog = false;
            PageDisplay(ItemPage.ItemDetailPage);
        }

        private System.Threading.SynchronizationContext _syncContext = null;
        public FormItem2022VM()
        {
            PageDisplay(ItemPage.ItemDetailPage);
            _syncContext = System.Threading.SynchronizationContext.Current;
            if (idleTimer == null)
            {
                idleTimer = new DispatcherTimer();
                idleTimer.Interval = TimeSpan.FromMilliseconds(1000);
                idleTimer.Tick += IdleTimeoutTimer_Tick;
            }
            idleTimer.Stop();

            this.BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
            
            this.VisibleTrigger = new DelegateCommand(IsVisibleChangedAction);
            // this.ItemCancelCommand = new DelegateCommand<Window>(ItemCancelCommandAction);

            this.TimeOutMessageOKCmd = new DelegateCommand(() => DoMessageOK());
            this.TimeOutMessageNoCmd = new DelegateCommand(() => DoCancelOrderYes());
            this.RequiredMessageOkCmd = new DelegateCommand(() => DoCloseRequiredMessage());

            // this.ClickItemVarietyCmd = new RelayCommand<PRPos.Data.ItemVarietyClass>(async (v) => { await this.VarietyChanged(v); }, ItemCommand_CanExecute);

            this.VarietySelectChanged = new DelegateCommand<PRPos.Data.ItemVarietyClass>(VarietySelectChangedAction);
            this.ModifierClick = new DelegateCommand<PRPos.Data.PsModsetDT>(ModifierClickAction);
            // this.ClickModifierCmd = new RelayCommand<PRPos.Data.PsModsetDT>(async (v) => { await this.ClickModifierAction(v); }, Modifier_CanExecute);
            this.ToppingQtyDialogCancelCmd = new DelegateCommand(ToppingQtyDialogCacelCmdAction);
            this.ToppingQtyDialogButtonCmd = new DelegateCommand<object >(ToppingQtyDialogButtonCmdAction);
        }
       
    }

}
