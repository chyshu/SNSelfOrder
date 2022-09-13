using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Services
{
    public class UploadService
    {        
        private SelfOrderSettingClass posSetting = null;
        public UploadService(SelfOrderSettingClass setting)
        {
            posSetting = setting;
        }
        public async Task<string> CheckUpload()
        {
            string psec = "1.0";
            string ret = "";
            try
            {
                List<Models.UploadPSTrn01sClass> ListPSTrn01s = new List<Models.UploadPSTrn01sClass>();
                using (SQLiteConnection cn = new SQLiteConnection(posSetting.ConnString))
                {
                    Debug.WriteLine("CheckUpload start");
                    App.log.Info("CheckUpload start");
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;
                    cmd.CommandText = @"select * from pstrn01s  where cmp_no=@cmp_no and str_no=@str_no  and  pos_no=@pos_no  and ifnull(uld_yn,'N')='N'";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", posSetting.CustomerID);
                    cmd.Parameters.AddWithValue("str_no", posSetting.StoreCode);
                    cmd.Parameters.AddWithValue("pos_no", posSetting.PosCode);
                    DataTable pstrn01sDT = new DataTable();
                    da.Fill(pstrn01sDT);
                    foreach (DataRow rowpstrn01s in pstrn01sDT.Rows)
                    {
                        Models.UploadPSTrn01sClass pstrn01s = new Models.UploadPSTrn01sClass();
                        pstrn01s.Cmp_no = rowpstrn01s["cmp_no"].ToString();
                        pstrn01s.Str_no = rowpstrn01s["str_no"].ToString();
                        pstrn01s.Pos_no = rowpstrn01s["pos_no"].ToString();
                        pstrn01s.Accdate = rowpstrn01s["accdate"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["accdate"].ToString()).ToString("MM/dd/yyyy");
                        pstrn01s.Deal_no = rowpstrn01s["deal_no"].ToString();

                        pstrn01s.Tdate = rowpstrn01s["tdate"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["tdate"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Clerk_no = rowpstrn01s["clerk_no"].ToString();
                        pstrn01s.Deal_code = rowpstrn01s["deal_code"].ToString();
                        pstrn01s.Dis_amt = rowpstrn01s["dis_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["dis_amt"].ToString());
                        pstrn01s.Min_amt = rowpstrn01s["min_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["min_amt"].ToString());
                        pstrn01s.Over_amt = rowpstrn01s["over_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["over_amt"].ToString());
                        pstrn01s.Mms_amt = rowpstrn01s["mms_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["mms_amt"].ToString());
                        pstrn01s.Tot_amt = rowpstrn01s["tot_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["tot_amt"].ToString());
                        pstrn01s.Tax_amt = rowpstrn01s["tax_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["tax_amt"].ToString());
                        pstrn01s.Ntax_amt = rowpstrn01s["ntax_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["ntax_amt"].ToString());
                        pstrn01s.Ztax_amt = rowpstrn01s["ztax_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["ztax_amt"].ToString());
                        pstrn01s.Ht_amt = rowpstrn01s["ht_amt"] == DBNull.Value ? 0 : decimal.Parse(rowpstrn01s["ht_amt"].ToString());
                        pstrn01s.Card_no = rowpstrn01s["card_no"].ToString();
                        pstrn01s.Buss_no = rowpstrn01s["buss_no"].ToString();
                        pstrn01s.Uld_yn = rowpstrn01s["uld_yn"].ToString();
                        pstrn01s.Crt_no = rowpstrn01s["crt_no"].ToString();
                        pstrn01s.Crt_date = rowpstrn01s["crt_date"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["crt_date"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Close_yn = rowpstrn01s["close_yn"].ToString();
                        pstrn01s.Ref_no = rowpstrn01s["ref_no"].ToString();
                        pstrn01s.Cnl_no = rowpstrn01s["cnl_no"].ToString();
                        pstrn01s.Cnl_time = rowpstrn01s["cnl_time"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["cnl_time"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Service_no = rowpstrn01s["service_no"].ToString();
                        pstrn01s.Opener_no = rowpstrn01s["opener_no"].ToString();
                        pstrn01s.Order_type = rowpstrn01s["order_type"].ToString();
                        pstrn01s.Person = rowpstrn01s["person"] == DBNull.Value ? 0 : int.Parse(rowpstrn01s["person"].ToString());
                        pstrn01s.Opentime = rowpstrn01s["opentime"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["opentime"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Sendtime = rowpstrn01s["sendtime"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["sendtime"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Closetime = rowpstrn01s["closetime"] == DBNull.Value ? "" : DateTime.Parse(rowpstrn01s["closetime"].ToString()).ToString("MM/dd/yyyy HH:mm:ss");
                        pstrn01s.Org_deal_no = rowpstrn01s["org_deal_no"].ToString();
                        pstrn01s.Del_deal_no = rowpstrn01s["del_deal_no"].ToString();
                        cmd.CommandText =
                            @"select * from pstrn03s  where cmp_no=@cmp_no and str_no=@str_no  and  pos_no=@pos_no 
                               and accdate=@accdate  and  deal_no=@deal_no  order by item_no";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", rowpstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", rowpstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", rowpstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(rowpstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", rowpstrn01s["deal_no"].ToString());
                        DataTable pstrn03sdt = new DataTable();
                        da.Fill(pstrn03sdt);
                        pstrn01s.PSTrn03s.Clear();
                        foreach (DataRow row in pstrn03sdt.Rows)
                        {
                            Models.UploadPSTrn03sClass trn03s = new Models.UploadPSTrn03sClass()
                            {
                                Cmp_no = row["cmp_no"].ToString(),
                                Str_no = row["str_no"].ToString(),
                                Pos_no = row["pos_no"].ToString(),
                                Accdate = row["accdate"] == DBNull.Value ? DateTime.Parse(rowpstrn01s["accdate"].ToString()).ToString("MM/dd/yyyy") : DateTime.Parse(row["accdate"].ToString()).ToString("MM/dd/yyyy"),
                                Deal_no = row["deal_no"].ToString(),
                                Item_no = int.Parse(row["item_no"].ToString()),
                                DC_code = row["dc_code"].ToString(),
                                Ecp_type = row["ecp_type"].ToString(),
                                Ecp_amt = row["ecp_amt"] == DBNull.Value ? 0 : decimal.Parse(row["ecp_amt"].ToString()),
                                Change_amt = row["change_amt"] == DBNull.Value ? 0 : decimal.Parse(row["change_amt"].ToString()),
                                Ecp_code = row["ecp_code"].ToString(),
                                Ecp_name = row["ecp_name"].ToString(),
                                Memo = row["memo"].ToString(),
                                Ref_code1 = row["ref_code1"].ToString(),
                                Ref_code2 = row["ref_code2"].ToString(),
                            };
                            pstrn01s.PSTrn03s.Add(trn03s);
                        }

                        cmd.CommandText = @"select * from pstrn02s  where cmp_no=@cmp_no and str_no=@str_no  and  pos_no=@pos_no  and accdate=@accdate  and  deal_no=@deal_no order by item_no";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", rowpstrn01s["cmp_no"].ToString());
                        cmd.Parameters.AddWithValue("str_no", rowpstrn01s["str_no"].ToString());
                        cmd.Parameters.AddWithValue("pos_no", rowpstrn01s["pos_no"].ToString());
                        cmd.Parameters.AddWithValue("accdate", DateTime.Parse(rowpstrn01s["accdate"].ToString()));
                        cmd.Parameters.AddWithValue("deal_no", rowpstrn01s["deal_no"].ToString());
                        DataTable pstrn02sdt = new DataTable();
                        da.Fill(pstrn02sdt);
                        foreach (DataRow row in pstrn02sdt.Rows)
                        {
                            Models.UploadPSTrn02sClass trn02s = new Models.UploadPSTrn02sClass()
                            {
                                Cmp_no = row["cmp_no"].ToString(),
                                Str_no = row["str_no"].ToString(),
                                Pos_no = row["pos_no"].ToString(),
                                Accdate = row["accdate"] == DBNull.Value ? DateTime.Parse(rowpstrn01s["accdate"].ToString()).ToString("MM/dd/yyyy") : DateTime.Parse(row["accdate"].ToString()).ToString("MM/dd/yyyy"),
                                Deal_no = row["deal_no"].ToString(),
                                Item_no = int.Parse(row["item_no"].ToString()),
                                Item_code = row["item_code"].ToString(),
                                Size_code = row["size_code"].ToString(),
                                Item_type = row["item_type"].ToString(),
                                Gst = row["gst"] == DBNull.Value ? 0 : decimal.Parse(row["gst"].ToString()),
                                Goo_price = row["goo_price"] == DBNull.Value ? 0 : decimal.Parse(row["goo_price"].ToString()),
                                Sprice = row["sprice"] == DBNull.Value ? 0 : decimal.Parse(row["sprice"].ToString()),
                                Qty = row["qty"] == DBNull.Value ? 0 : int.Parse(row["qty"].ToString()),
                                Tax_amt = row["tax_amt"] == DBNull.Value ? 0 : decimal.Parse(row["tax_amt"].ToString()),
                                Mis_amt = row["mis_amt"] == DBNull.Value ? 0 : decimal.Parse(row["mis_amt"].ToString()),
                                Dis_amt = row["dis_amt"] == DBNull.Value ? 0 : decimal.Parse(row["dis_amt"].ToString()),
                                Dis_rate = row["dis_rate"] == DBNull.Value ? 0 : decimal.Parse(row["dis_rate"].ToString()),
                                Amt = row["amt"] == DBNull.Value ? 0 : decimal.Parse(row["amt"].ToString()),
                                Ht_price = row["ht_price"] == DBNull.Value ? 0 : decimal.Parse(row["ht_price"].ToString()),
                                Ht_amt = row["ht_amt"] == DBNull.Value ? 0 : decimal.Parse(row["ht_amt"].ToString()),
                                Mms_mis = row["mms_mis"] == DBNull.Value ? 0 : decimal.Parse(row["mms_mis"].ToString()),
                                Mms_no = row["mms_no"].ToString(),
                                Item_name = row["item_name"].ToString(),
                                Kitchen_memo = row["kitchen_memo"].ToString(),
                                Kitchen_name = row["kitchen_name"].ToString(),
                                Printer_name = row["printer_name"].ToString(),
                                Size_caption = row["size_caption"].ToString(),
                                Variety_code = row["variety_code"].ToString(),
                                Combo_code = row["combo_code"].ToString(),
                                Modset_code = row["modset_code"].ToString(),
                                Variety_Caption = row["variety_caption"].ToString(),
                                Variety_kitchen_name = row["variety_kitchen_name"].ToString(),
                            };
                            cmd.CommandText = @"select * from pstrn04s  
                                                where cmp_no=@cmp_no and str_no=@str_no  and  pos_no=@pos_no  
                                                and accdate=@accdate  and  deal_no=@deal_no  and  item_no=@item_no and variety_code=@variety_code 
                                                and  item_code=@item_code  order by seq ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", row["cmp_no"].ToString());
                            cmd.Parameters.AddWithValue("str_no", row["str_no"].ToString());
                            cmd.Parameters.AddWithValue("pos_no", row["pos_no"].ToString());
                            cmd.Parameters.AddWithValue("accdate", DateTime.Parse(row["accdate"].ToString()));
                            cmd.Parameters.AddWithValue("deal_no", row["deal_no"].ToString());
                            cmd.Parameters.AddWithValue("item_no", row["item_no"].ToString());
                            cmd.Parameters.AddWithValue("item_code", row["item_code"].ToString());
                            cmd.Parameters.AddWithValue("variety_code", row["variety_code"].ToString());
                            DataTable pstrn04sDT = new DataTable();
                            da.Fill(pstrn04sDT);
                            foreach (DataRow modifier in pstrn04sDT.Rows)
                            {
                                int qty = 0;
                                int.TryParse(modifier["qty"].ToString(), out qty);
                                decimal amount = 0;
                                decimal.TryParse(modifier["amount"].ToString(), out amount);
                                decimal sprice = 0;
                                decimal.TryParse(modifier["sprice"].ToString(), out sprice);
                                Models.UploadPSTrn04sClass trn04s = new Models.UploadPSTrn04sClass()
                                {
                                    Cmp_no = modifier["cmp_no"].ToString(),
                                    Str_no = modifier["str_no"].ToString(),
                                    Pos_no = modifier["pos_no"].ToString(),
                                    Accdate = modifier["accdate"] == DBNull.Value ? DateTime.Parse(rowpstrn01s["accdate"].ToString()).ToString("MM/dd/yyyy") : DateTime.Parse(modifier["accdate"].ToString()).ToString("MM/dd/yyyy"),
                                    Deal_no = modifier["deal_no"].ToString(),
                                    Item_no = int.Parse(modifier["item_no"].ToString()),
                                    Item_code = modifier["item_code"].ToString(),
                                    Caption = modifier["caption"].ToString(),
                                    Caption_fn = modifier["caption_fn"].ToString(),
                                    Variety_code = modifier["variety_code"].ToString(),
                                    Modset_code = modifier["modset_code"].ToString(),
                                    Modifier_code = modifier["modifier_code"].ToString(),
                                    Amount = amount,
                                    Qty = qty,
                                    Sprice = sprice,
                                };

                                trn02s.PSTrn04s.Add(trn04s);
                            }
                            pstrn01s.PSTrn02s.Add(trn02s);
                        }
                        ListPSTrn01s.Add(pstrn01s);
                    }


                    UploadDataClass postdata = new UploadDataClass()
                    {
                        MAC = posSetting.MAC,
                        Code = posSetting.ConnectionCode,
                        Token = posSetting.Token,
                    };
                    postdata.UplDataValue = JsonConvert.SerializeObject(ListPSTrn01s);
                    Console.WriteLine(postdata.UplDataValue);
                    string postBody = JsonConvert.SerializeObject(postdata);
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.PostAsync(posSetting.HostURL + "v3/" + "postPosData", new StringContent(postBody, Encoding.UTF8, "application/json"));
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // ret = responseBody;
                    // Console.WriteLine(responseBody);
                    UploadDataClass resultPosData = JsonConvert.DeserializeObject<UploadDataClass>(responseBody);
                    if (resultPosData.Retcode.Equals("0"))
                    {
                        foreach (Models.UploadPSTrn01sClass pstrn01s in ListPSTrn01s)
                        {
                            cmd.CommandText = @"update pstrn01s  set uld_yn='Y'  where cmp_no=@cmp_no and str_no=@str_no  and  pos_no=@pos_no and accdate=@accdate and deal_no=@deal_no ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("cmp_no", pstrn01s.Cmp_no);
                            cmd.Parameters.AddWithValue("str_no", pstrn01s.Str_no);
                            cmd.Parameters.AddWithValue("pos_no", pstrn01s.Pos_no);
                            cmd.Parameters.AddWithValue("accdate", DateTime.Parse(pstrn01s.Accdate));
                            cmd.Parameters.AddWithValue("deal_no", pstrn01s.Deal_no);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                ret = "0";
            }
            catch (Exception err)
            {
                Debug.WriteLine("CheckUpload:" + psec + " " + err.Message);
                App.log.Info("CheckUpload:" + psec + " " + err.Message);
                ret = "";
            }
            return ret;
            }

    }
}
