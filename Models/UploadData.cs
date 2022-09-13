using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Models
{
    public class UploadPSTrn01sClass
    {
        [JsonProperty("cmp_no")]
        public string Cmp_no = "";
        [JsonProperty("str_no")]
        public string Str_no = "";
        [JsonProperty("pos_no")]
        public string Pos_no = "";
        [JsonProperty("accdate")]
        public string Accdate = "";
        [JsonProperty("deal_no")]
        public string Deal_no = "";
        [JsonProperty("tdate")]
        public string Tdate = "";
        [JsonProperty("clerk_no")]
        public string Clerk_no = "";
        [JsonProperty("deal_code")]
        public string Deal_code = "";
        [JsonProperty("dis_amt")]
        public decimal Dis_amt;
        [JsonProperty("min_amt")]
        public decimal Min_amt;
        [JsonProperty("over_amt")]
        public decimal Over_amt;
        [JsonProperty("mms_amt")]
        public decimal Mms_amt;
        [JsonProperty("tot_amt")]
        public decimal Tot_amt;
        [JsonProperty("tax_amt")]
        public decimal Tax_amt;
        [JsonProperty("ntax_amt")]
        public decimal Ntax_amt;
        [JsonProperty("ztax_amt")]
        public decimal Ztax_amt;
        [JsonProperty("ht_amt")]
        public decimal Ht_amt;
        [JsonProperty("card_no")]
        public string Card_no = "";
        [JsonProperty("buss_no")]
        public string Buss_no = "";
        [JsonProperty("uld_yn")]
        public string Uld_yn = "";
        [JsonProperty("crt_no")]
        public string Crt_no = "";
        [JsonProperty("crt_date")]
        public string Crt_date = "";
        [JsonProperty("close_yn")]
        public string Close_yn = "";
        [JsonProperty("ref_no")]
        public string Ref_no = "";
        [JsonProperty("cnl_no")]
        public string Cnl_no = "";
        [JsonProperty("cnl_time")]
        public string Cnl_time = "";
        [JsonProperty("service_no")]
        public string Service_no = "";
        [JsonProperty("opener_no")]
        public string Opener_no = "";
        [JsonProperty("order_type")]
        public string Order_type = "";
        [JsonProperty("person")]
        public int Person;
        [JsonProperty("opentime")]
        public string Opentime = "";
        [JsonProperty("sendtime")]
        public string Sendtime = "";
        [JsonProperty("closetime")]
        public string Closetime = "";
        [JsonProperty("org_deal_no")]
        public string Org_deal_no = "";
        [JsonProperty("del_deal_no")]
        public string Del_deal_no = "";

        [JsonProperty("items")]
        public List<UploadPSTrn02sClass> PSTrn02s = new List<UploadPSTrn02sClass>();
        [JsonProperty("pays")]
        public List<UploadPSTrn03sClass> PSTrn03s = new List<UploadPSTrn03sClass>();

    }

    public class UploadPSTrn02sClass
    {
        [JsonProperty("cmp_no")]
        public string Cmp_no = "";
        [JsonProperty("str_no")]
        public string Str_no = "";
        [JsonProperty("pos_no")]
        public string Pos_no = "";
        [JsonProperty("accdate")]
        public string Accdate = "";
        [JsonProperty("deal_no")]
        public string Deal_no = "";
        [JsonProperty("item_no")]
        public int Item_no;
        [JsonProperty("item_code")]
        public string Item_code = "";
        [JsonProperty("size_code")]
        public string Size_code = "";
        [JsonProperty("item_type")]
        public string Item_type = "";
        [JsonProperty("gst")]
        public decimal Gst;
        [JsonProperty("goo_price")]
        public decimal Goo_price;
        [JsonProperty("sprice")]
        public decimal Sprice;
        [JsonProperty("qty")]
        public int Qty;
        [JsonProperty("tax_amt")]
        public decimal Tax_amt;
        [JsonProperty("mis_amt")]
        public decimal Mis_amt;
        [JsonProperty("dis_amt")]
        public decimal Dis_amt;
        [JsonProperty("dis_rate")]
        public decimal Dis_rate;
        [JsonProperty("amt")]
        public decimal Amt;
        [JsonProperty("ht_price")]
        public decimal Ht_price;
        [JsonProperty("ht_amt")]
        public decimal Ht_amt;
        [JsonProperty("mms_mis")]
        public decimal Mms_mis;
        [JsonProperty("mms_no")]
        public string Mms_no = "";
        [JsonProperty("gid")]
        public string Gid = "";
        [JsonProperty("item_name")]
        public string Item_name = "";
        [JsonProperty("kitchen_memo")]
        public string Kitchen_memo = "";
        [JsonProperty("kitchen_name")]
        public string Kitchen_name = "";
        [JsonProperty("printer_name")]
        public string Printer_name = "";
        [JsonProperty("size_caption")]
        public string Size_caption = "";

        [JsonProperty("modifier")]
        public List<UploadPSTrn04sClass> PSTrn04s = new List<UploadPSTrn04sClass>();
        [JsonProperty("variety_code")]
        public string Variety_code = "";
        [JsonProperty("combo_code")]
        public string Combo_code = "";
        [JsonProperty("modset_code")]
        public string Modset_code = "";
        [JsonProperty("variety_caption")]
        public string Variety_Caption = "";
        [JsonProperty("variety_kitchen_name")]
        public string Variety_kitchen_name = "";
    }

    public class UploadPSTrn03sClass
    {
        [JsonProperty("cmp_no")]
        public string Cmp_no = "";
        [JsonProperty("str_no")]
        public string Str_no = "";
        [JsonProperty("pos_no")]
        public string Pos_no = "";
        [JsonProperty("accdate")]
        public string Accdate = "";
        [JsonProperty("deal_no")]
        public string Deal_no = "";
        [JsonProperty("item_no")]
        public int Item_no;
        [JsonProperty("dc_code")]
        public string DC_code = "";
        [JsonProperty("ecp_type")]
        public string Ecp_type = "";
        [JsonProperty("ecp_amt")]
        public decimal Ecp_amt;
        [JsonProperty("change_amt")]
        public decimal Change_amt;
        [JsonProperty("ecp_code")]
        public string Ecp_code = "";
        [JsonProperty("memo")]
        public string Memo = "";
        [JsonProperty("ecp_name")]
        public string Ecp_name = "";
        [JsonProperty("ref_code1")]
        public string Ref_code1 = "";
        [JsonProperty("ref_code2")]
        public string Ref_code2 = "";
    }

    public class UploadPSTrn04sClass
    {
        [JsonProperty("cmp_no")]
        public string Cmp_no = "";
        [JsonProperty("str_no")]
        public string Str_no = "";
        [JsonProperty("pos_no")]
        public string Pos_no = "";
        [JsonProperty("accdate")]
        public string Accdate = "";
        [JsonProperty("deal_no")]
        public string Deal_no = "";
        [JsonProperty("item_no")]
        public int Item_no;
        [JsonProperty("variety_code")]
        public string Variety_code = "";

        [JsonProperty("item_code")]
        public string Item_code = "";

        [JsonProperty("modset_code")]
        public string Modset_code = "";

        [JsonProperty("modifier_code")]
        public string Modifier_code = "";

        [JsonProperty("caption")]
        public string Caption = "";

        [JsonProperty("caption_fn")]
        public string Caption_fn = "";

        [JsonProperty("qty")]
        public int Qty = 0;

        [JsonProperty("sprice")]
        public decimal Sprice = 0;

        [JsonProperty("amount")]
        public decimal Amount = 0;
    }
}
