using PRPos.Data;
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
    /// Interaction logic for WindowItemDetail.xaml
    /// </summary>
    public partial class WindowItemDetail : Window
    {
        private WindowItemDetailVM vm;
        public WindowItemDetail()
        {
            InitializeComponent();
            vm = new WindowItemDetailVM();
            this.DataContext = vm;
        }
    }

    public class WindowItemDetailVM : ViewModel.ViewModelBase
    {
        private int mHeight = 1920;
        private int mWidth = 1080;
        private int mLeft = PRPosUtils.SCREENLEFT;
        private int mTop = 0;

        private int mItemImageHeight = 300;

        private int mItemImageWidth = 300;

        private int varietyHeight = 140;
        private int varietyWidth = 70;

        public int VarietyWidth { get { return varietyWidth; } set { SetProperty(ref varietyWidth, value); } }

        public int VrietyHeight { get { return varietyHeight; } set { varietyHeight = value; OnPropertyChanged("VrietyHeight"); } }

        private string backgroundImagePath = "";

        public string BackgroundImagePath { get => backgroundImagePath; set { SetProperty(ref  backgroundImagePath , value); } }

        public int ItemDescHeight { get { return mItemImageHeight + (int)ImageMargin.Top + (int)ImageMargin.Bottom; } }

        private Thickness mImageMargin = new Thickness(0);
        public Thickness ImageMargin { get => mImageMargin; set { SetProperty(ref mImageMargin, value); } }

        #region ItemImagePath

        public string ItemImagePath { get { return SelectedItem == null?"": System.IO.Path.Combine(PRPosUtils.FilePath, SelectedItem.ButtonImgURI); } }
        #endregion ItemImagePath

        private ObservableCollection<PRPos.Data.ItemVarietyClass> itemVarietys = new ObservableCollection<PRPos.Data.ItemVarietyClass>();

        public ObservableCollection<PRPos.Data.ItemVarietyClass> ItemVarietys
        {
            get { return itemVarietys; }
            set { itemVarietys = value; OnPropertyChanged("ItemVarietys"); }
        }

        private int mQty = 1;
        public int Qty { get => mQty; set { mQty = value; mItemAmt = mQty * ItemPrice; OnPropertyChanged(""); } }

        private decimal mItemPrice;
        public decimal ItemPrice { get => mItemPrice; set { mItemPrice = value; mItemAmt = mQty * ItemPrice; OnPropertyChanged(""); } }

        public string ItemDescription { get { return SelectedItem.Description; } }

        public string ItemName { get { return SelectedItem.ItemName; } }

        public string ItemCaption { get { return SelectedItem.Caption; } }

        private decimal mItemAmt;
        public decimal ItemAmount { get => mItemAmt; set { mItemAmt = value; } }
        private PRPos.Data.FastKeyClass mSelectedItem;
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
                    cmd.Parameters.AddWithValue("customerid", "3");
                    cmd.Parameters.AddWithValue("store_code", "TW");
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
                        cmd.Parameters.AddWithValue("variety_code", VarietyRow["variety_code"].ToString());
                        cmd.Parameters.AddWithValue("item_code", VarietyRow["item_code"].ToString());
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
                                          psmodsetti.caption_fn,psmodset01.modset_code,psmodsetti.disp_caption,psmodsetti.disp_price,localmodifiersetting.soldout ,
                                          psmodsetti.soldout as modifier_soldout
                                from psmodsetti  
                                left outer join modifier on modifier.modifier_code=psmodsetti.modifier_code
                                left outer join psmodset01 on psmodset01.sid=psmodsetti.psid
                                left outer join localmodifiersetting on psmodsetti.sid = localmodifiersetting.psmodsetti_id and psmodsetti.modifier_code = localmodifiersetting.modifier_code 
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

                                        LocalModSoldOut = modifier["soldout"].ToString(),
                                        Disp_caption = modifier["disp_caption"].ToString(),
                                        Disp_price = modifier["disp_price"].ToString(),
                                        ModSoldOut = modifier["modifier_soldout"].ToString(),
                                        Next_modset = modifier["next_modset"].ToString(),
                                        Modifier_code = modifier["modifier_code"].ToString(),
                                        Picture = System.IO.Path.Combine(PRPosUtils.FilePath, modifier["image"].ToString().Equals("") ? modifier["mod_image"].ToString() : modifier["image"].ToString()),
                                    };
                                    set.Modifiers.Add(modifierDT);
                                    mModifier.Add(modifierDT);
                                }

                                mModifierSet.Add(set);
                            }

                        }

                        varity.ModifierSets = mModifierSet;
                        ModifierSets = mModifierSet;

                        objItemVarietys.Add(varity);
                    }
                    ItemVarietys = objItemVarietys;

                    foreach (PRPos.Data.ItemVarietyClass c in objItemVarietys)
                    {
                        VarietySelectChangedAction(c);
                        break;
                    }
                }
            }
        }
        
        private PRPos.Data.ItemVarietyClass mSelectedVariety;
        public PRPos.Data.ItemVarietyClass SelectedVariety
        {
            get { return this.mSelectedVariety; }
            set
            {
                // Debug.WriteLine("SelectedVariety ==> " + value);
                SetProperty(ref mSelectedVariety, value);
                //  varietyUpdate();

                decimal refPrice = 0;
                //if(PRPosUtils.SalePriceColumn=="sprice")
                decimal.TryParse(value.VarietyRow["sprice"].ToString(), out refPrice);
                this.ItemPrice = refPrice;

                this.SetProperty(ref this.mSelectedVariety, value);
                // varietyUpdate();
                OnPropertyChanged("SelectedVariety");

            }
        }
        public DelegateCommand<PRPos.Data.ItemVarietyClass> ClickItemVarietyCmd { get; set; }

        public ICommand VarietySelectChanged { get; set; }

        private ObservableCollection<PRPos.Data.PsModset01> modifierSets = new ObservableCollection<PsModset01>();
        public ObservableCollection<PsModset01> ModifierSets { get => modifierSets;
            set { SetProperty(ref modifierSets, value);
                OnPropertyChanged("ModifierSets");
            } 
        }

        public ObservableCollection<PsModsetDT> Modifiers { get => mModifier; set
            {
                
                SetProperty(ref mModifier, value);
                OnPropertyChanged("Modifiers");
            }
        }

        private ObservableCollection<PsModsetDT> mModifier = new ObservableCollection<PsModsetDT>();

        private void VarietySelectChangedAction(object param)
        {
            // Debug.WriteLine("VarietySelectChangedAction by DelegateCommand ");
          
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
        public ICommand ModifierClick { get; set; }
        #region Modifier_CLICK_Process
        private void ModifierClickAction(Object param)
        {
            Debug.WriteLine("ModifierClickAction ");

        }
        private void CalculatePrice(string variety_code)
        {
            // reset the other variety
            foreach (PRPos.Data.ItemVarietyClass imv in this.ItemVarietys)
            {
                if (imv.Variety_code != variety_code)
                {
                    foreach (PRPos.Data.PsModset01 set in imv.ModifierSets)
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
                    decimal.TryParse(imv.VarietyRow[PRPosUtils.SalePriceColumn].ToString(), out refPrice);


                    foreach (PRPos.Data.PsModset01 set in imv.ModifierSets)
                    {
                        foreach (PRPos.Data.PsModsetDT modifier in set.Modifiers)
                        {
                            if (modifier.SelectedQty > 0)
                            {
                                refPrice += modifier.SelectedQty * modifier.Sprice;
                            }
                        }
                    }
                    this.ItemPrice = refPrice;
                    break;
                }
            }
        }
        #endregion
        public WindowItemDetailVM()
        {
            BackgroundImagePath = System.IO.Path.Combine(PRPosUtils.FilePath, PRPosUtils.Img_blank);
            using (SQLiteConnection cn = new SQLiteConnection(@"data source=C:\Users\Roger\Desktop\PRSelfOrder_GIOV2\PRSelfOrder\PRSelfOrder\db\prposdb.db3"))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;

                List<PRPos.Data.FastKeyClass> keyList = new List<PRPos.Data.FastKeyClass>();

                cmd.CommandText =
                    @"select * from posfastkey02 where sid=@psid and  display_yn='Y'
                      and op_code='1' and del_flag='N' 
                      and customerid=@customerid order by disp_order";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("psid", 393);
                cmd.Parameters.AddWithValue("customerid","3" );
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
                        Picture = System.IO.Path.Combine(PRPosUtils.FilePath, row["imagefile"].ToString()),
                        FontSize = row["fontsize"] == DBNull.Value ? 16 : int.Parse(row["fontsize"].ToString()),
                        BackColor = row["bgcolor"] == DBNull.Value ? "#061213" : (row["bgcolor"].ToString().StartsWith("#") ? row["bgcolor"].ToString() : "#" + row["bgcolor"].ToString()),
                        TextBgColor = row["textbgcolor"] == DBNull.Value ? "Transparent" : (row["textbgcolor"].ToString().StartsWith("#") ? row["textbgcolor"].ToString() : "#" + row["textbgcolor"].ToString()),
                        TextOffset = row["textheight"] == DBNull.Value ? 65 : int.Parse(row["textheight"].ToString()),
                        Text_display_yn = row["caption_yn"] == DBNull.Value ? "Y" : row["caption_yn"].ToString(),
                    };

                    cmd.CommandText = @"select * from posfastkey02 where display_yn='Y' and op_code = 2 and store_code=@store_code and del_flag='N' 
                                            and customerid=@customerid and psid=@psid order by disp_order";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", "3");
                    cmd.Parameters.AddWithValue("store_code", "TW");
                    cmd.Parameters.AddWithValue("psid", menuItem.Sid);

                    DataTable fastkeyItemDT = new DataTable();
                    da.Fill(fastkeyItemDT);
                    ObservableCollection<FastKeyClass> keyItems = new ObservableCollection<FastKeyClass>();
                    foreach (DataRow itemrow in fastkeyItemDT.Rows)
                    {
                        cmd.CommandText = "select psitem.* " +
                              ",itemvariety. sprice as ivsprice,itemvariety.sprice2 as ivsprice2 " +
                              ",promotions.sprice  as psprice,promotions.sprice2 as psprice2 " +
                              ",localitemsetting.soldout as localsoldout,localitemsetting.upd_date as local_update " +
                          "from psitem " +
                          "left join localitemsetting on psitem.item_code = localitemsetting.item_code " +
                          "left join itemvariety on psitem.item_code = itemvariety.item_code " +
                          "left join promotions on psitem.item_code = promotions.item_code and promotions.customerid = psitem.customerid  and bdate >= date('now') and edate<= date('now') and promotions.del_flag='N' " +
                              "where psitem.customerid=@customerid and psitem.item_code=@item_code and itemvariety.store_code = @store_code ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("item_code", itemrow["ref_code"].ToString());
                        cmd.Parameters.AddWithValue("customerid", "3");
                        cmd.Parameters.AddWithValue("store_code", "TW");
                        DataTable psItemDT = new DataTable();
                        da.Fill(psItemDT);
                        decimal price1 = 0;
                        decimal price2 = 0;
                        string soldOut = "", upddate = "";
                        decimal gst = 0;
                        if (psItemDT.Rows.Count > 0)
                        {
                            var psitem = psItemDT.Rows[0];

                            decimal.TryParse(psitem["sprice"].ToString(), out price1);
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
                            decimal.TryParse(psitem["sprice2"].ToString(), out price2);
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
                            if (DateTime.TryParse(psitem["local_update"].ToString(), out localUpdDate))
                            {
                                if (DateTime.TryParse(psitem["upd_date"].ToString(), out cloudUpdDate))
                                {
                                    if (localUpdDate > cloudUpdDate)
                                    {
                                        soldOut = string.IsNullOrEmpty(psitem["localsoldout"].ToString()) ? psitem["soldout"].ToString() : psitem["localsoldout"].ToString();
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
                                    soldOut = string.IsNullOrEmpty(psitem["localsoldout"].ToString()) ? psitem["soldout"].ToString() : psitem["localsoldout"].ToString();
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
                                SoldOut = soldOut,
                                Sprice = price1,
                                GST = gst,
                                Takeawayprice = price2,
                                PCode = itemrow["op_code"].ToString(),
                                Width = PRPosUtils.ItemWidth,
                                Height = PRPosUtils.ItemHeight,
                                Selected = itemrow["ref_code"].ToString(),
                                Seq = itemrow["disp_order"] == DBNull.Value ? 9 : int.Parse(itemrow["disp_order"].ToString()),
                                Default_yn = itemrow["default_yn"] == DBNull.Value ? "N" : itemrow["default_yn"].ToString(),
                                Display_yn = itemrow["display_yn"] == DBNull.Value ? "Y" : itemrow["display_yn"].ToString(),
                                Full_Image_yn = itemrow["fullimage_yn"] == DBNull.Value ? "N" : itemrow["fullimage_yn"].ToString(),
                                FontColor = itemrow["fontcolor"] == DBNull.Value ? "FFFFFF" : itemrow["fontcolor"].ToString().Equals("") ? "Transparent" : itemrow["fontcolor"].ToString(),
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
                            //Debug.WriteLine(" add " + keybutton.Caption);
                        }
                    }
                    menuItem.FastkeyItems = keyItems;
                    SelectedItem = keyItems[0];



                }

            }

            this.ModifierClick = new DelegateCommand<PRPos.Data.PsModsetDT>(ModifierClickAction);
        }
    }
}
