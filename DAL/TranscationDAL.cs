using PRPos.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.DAL
{
    public interface ITranscationDAL
    {
        PRPos.Data.PSTrn01sClass GetTranscation();
        int SetTranscation(PRPos.Data.PSTrn01sClass deal);
    }
    public class TranscationDAL : ITranscationDAL
    {
        string connstring;
        public TranscationDAL(string _conn)
        {
            connstring = _conn;
        }
        public PSTrn01sClass GetTranscation()
        {
            throw new NotImplementedException();
        }
        public int SetEFTPOSTransRef(string TransRef)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connstring))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                App.log.Info("SAVE EFTPOSTransRef");
                cmd.CommandText = "update EFTPOSTransRef set lastReference=@lastReference  where sid=1  ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("lastReference", TransRef);

                return cmd.ExecuteNonQuery();
            }
        }
        public int SetTranscation(PSTrn01sClass deal)
        {
            return SaveTranscation(deal, "S"); ;
        }
        public int SetRefund(PSTrn01sClass deal)
        {
            int ret = SaveTranscation(deal, "R"); 

            return ret;
        }

        private int SaveTranscation(PSTrn01sClass deal, string deal_code)
        {
            int ret = 0;
            using (SQLiteConnection connection = new SQLiteConnection(connstring))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                App.log.Info("SAVE pstrn02s");

                foreach (var item in deal.OrderItems)
                {
                    cmd.CommandText =
                        @" insert into pstrn02s 
                                (cmp_no,str_no,pos_no,accdate,deal_no,item_no,item_code,size_code,item_type,gst,sprice
                                ,qty,tax_amt,amt,gid,item_name,kitchen_memo,kitchen_name,printer_name,size_caption,variety_code,modset_code,combo_code
                                ,variety_caption,variety_kitchen_name)
                                values 
                                (@cmp_no,@str_no,@pos_no,@accdate,@deal_no,@item_no,@item_code,@size_code,@item_type,@gst,@sprice
                               ,@qty,@tax_amt,@amt,@gid,@item_name,@kitchen_memo,@kitchen_name,@printer_name,@size_caption,@variety_code,@modset_code,@combo_code
                               ,@variety_caption,@variety_kitchen_name)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", deal.CustomerId);
                    cmd.Parameters.AddWithValue("str_no", deal.Store_Code);
                    cmd.Parameters.AddWithValue("pos_no", deal.Pos_No);
                    cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(deal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("deal_no", deal.Deal_No);
                    cmd.Parameters.AddWithValue("item_no", item.Item_No);
                    cmd.Parameters.AddWithValue("item_code", item.Item_Code);
                    cmd.Parameters.AddWithValue("size_code", item.Size_Code);
                    cmd.Parameters.AddWithValue("item_type", item.Item_Type);
                    cmd.Parameters.AddWithValue("gst", item.GST);
                    cmd.Parameters.AddWithValue("sprice", item.Sprice);
                    cmd.Parameters.AddWithValue("qty", item.Qty);
                    cmd.Parameters.AddWithValue("tax_amt", item.Tax_Amt);
                    cmd.Parameters.AddWithValue("amt", item.Amount);
                    cmd.Parameters.AddWithValue("gid", "");
                    cmd.Parameters.AddWithValue("item_name", item.Item_Name);
                    cmd.Parameters.AddWithValue("kitchen_memo", item.Kitchen_Memo);
                    cmd.Parameters.AddWithValue("kitchen_name", item.Kitchen_Name);
                    cmd.Parameters.AddWithValue("printer_name", item.Printer_Name == "" ? PRPosUtils.DefaultPrinterName : item.Printer_Name);
                    cmd.Parameters.AddWithValue("size_caption", item.Size_Caption);
                    cmd.Parameters.AddWithValue("variety_code", item.Variety_Code);
                    cmd.Parameters.AddWithValue("modset_code", item.Modset_Code);
                    cmd.Parameters.AddWithValue("combo_code", item.Combo_Code);
                    cmd.Parameters.AddWithValue("variety_caption", item.Variety_Caption);
                    cmd.Parameters.AddWithValue("variety_kitchen_name", item.Variety_Kitchen_name);
                    cmd.ExecuteNonQuery();
                    int itemno = 0;
                    foreach (var modifier in item.Modifiers)
                    {
                        itemno += 1;
                        cmd.CommandText =
                                       @"insert into pstrn04s 
                                    (cmp_no,str_no,pos_no,accdate,deal_no,item_no,variety_code,item_code,modset_code, modifier_code,modset_code,caption_fn,caption,qty,sprice,amount,inpqty )
                                     values 
                                    (@cmp_no,@str_no,@pos_no,@accdate,@deal_no,@item_no,@variety_code,@item_code,@modset_code,@modifier_code,@modset_code,@caption_fn,@caption,@qty,@sprice,@amount,@inpqty ) ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("cmp_no", deal.CustomerId);
                        cmd.Parameters.AddWithValue("str_no", deal.Store_Code);
                        cmd.Parameters.AddWithValue("pos_no", deal.Pos_No);
                        cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(deal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("deal_no", deal.Deal_No);
                        cmd.Parameters.AddWithValue("item_no", itemno);
                        cmd.Parameters.AddWithValue("variety_code", item.Variety_Code);
                        cmd.Parameters.AddWithValue("item_code", item.Item_Code);
                        cmd.Parameters.AddWithValue("modset_code", modifier.Modset_Code);
                        cmd.Parameters.AddWithValue("modifier_code", modifier.Modifier_Code);
                        cmd.Parameters.AddWithValue("caption_fn", modifier.Caption_fn);
                        cmd.Parameters.AddWithValue("caption", modifier.Caption);
                        cmd.Parameters.AddWithValue("qty", modifier.Qty);
                        cmd.Parameters.AddWithValue("inpqty", modifier.InpQty);
                        cmd.Parameters.AddWithValue("sprice", modifier.Sprice);
                        cmd.Parameters.AddWithValue("amount", modifier.Amount);
                        cmd.ExecuteNonQuery();
                    }
                }

                App.log.Info("SAVE pstrn03s");
                cmd.CommandText = @"insert into pstrn03s 
                                       (cmp_no,str_no,pos_no,accdate,deal_no, item_no, dc_code , ecp_type, ecp_amt,change_amt,ecp_code,memo,ecp_name,ref_code1,ref_code2 )
                                       values 
                                       (@cmp_no,@str_no,@pos_no,@accdate,@deal_no,@item_no, @dc_code , @ecp_type, @ecp_amt,@change_amt,@ecp_code,@memo,@ecp_name,@ref_code1,@ref_code2 ) ";
                foreach (var payment in deal.Payments)
                {

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("cmp_no", deal.CustomerId);
                    cmd.Parameters.AddWithValue("str_no", deal.Store_Code);
                    cmd.Parameters.AddWithValue("pos_no", deal.Pos_No);
                    cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(deal.AccDate, PRPosUtils.DateFormat ,CultureInfo.InvariantCulture ));
                    cmd.Parameters.AddWithValue("deal_no", deal.Deal_No);
                    cmd.Parameters.AddWithValue("item_no", payment.Item_No);
                    cmd.Parameters.AddWithValue("dc_code", payment.Dc_code);
                    cmd.Parameters.AddWithValue("ecp_type", payment.Ecp_type);
                    cmd.Parameters.AddWithValue("ecp_amt", payment.Ecp_amt);
                    cmd.Parameters.AddWithValue("change_amt", payment.Change_amt);
                    cmd.Parameters.AddWithValue("ecp_code", payment.Epc_code);
                    cmd.Parameters.AddWithValue("memo", payment.Memo);
                    cmd.Parameters.AddWithValue("ecp_name", payment.Ecp_name);
                    cmd.Parameters.AddWithValue("ref_code1", payment.Ref_code1);
                    cmd.Parameters.AddWithValue("ref_code2", payment.Ref_code2);
                    cmd.ExecuteNonQuery();
                }

                App.log.Info("SAVE pstrn01s");
                cmd.CommandText = @"insert into pstrn01s 
                                       (cmp_no,str_no,pos_no,deal_no,tdate,clerk_no,deal_code,tot_amt,tax_amt,card_no
                                        ,buss_no,uld_yn,close_yn,ref_no,service_no,opener_no,order_type,person,accdate, opentime, sendtime,closetime
                                        ,org_deal_no,del_deal_no)
                                       values 
                                       (@cmp_no,@str_no,@pos_no,@deal_no,@tdate,@clerk_no,@deal_code,@tot_amt,@tax_amt,@card_no
                                        ,@buss_no,@uld_yn,@close_yn,@ref_no,@service_no,@opener_no,@order_type,@person,@accdate, @opentime, @sendtime,@closetime
                                        ,@org_deal_no,@del_deal_no) "; cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("cmp_no", deal.CustomerId);
                cmd.Parameters.AddWithValue("str_no", deal.Store_Code);
                cmd.Parameters.AddWithValue("pos_no", deal.Pos_No);
                cmd.Parameters.AddWithValue("accdate", DateTime.ParseExact(deal.AccDate, PRPosUtils.DateFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("deal_no", deal.Deal_No);

                cmd.Parameters.AddWithValue("tdate", DateTime.ParseExact(deal.Tdate, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("clerk_no", deal.Clerk_no);
                cmd.Parameters.AddWithValue("deal_code", deal_code);
                cmd.Parameters.AddWithValue("tot_amt", deal.Tot_amt);
                cmd.Parameters.AddWithValue("tax_amt", deal.Tax_amt);
                cmd.Parameters.AddWithValue("card_no", deal.Card_no);
                cmd.Parameters.AddWithValue("buss_no", deal.Buss_no);
                cmd.Parameters.AddWithValue("uld_yn", "N");
                cmd.Parameters.AddWithValue("close_yn", "N");
                cmd.Parameters.AddWithValue("ref_no", deal.Ref_no);
                cmd.Parameters.AddWithValue("service_no", deal.Service_no);
                cmd.Parameters.AddWithValue("opener_no", deal.Opener_no);
                cmd.Parameters.AddWithValue("order_type", deal.Order_type);
                cmd.Parameters.AddWithValue("person", deal.Person);
                cmd.Parameters.AddWithValue("opentime", DateTime.ParseExact(deal.Opentime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("sendtime", DateTime.ParseExact(deal.Sendtime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("closetime", DateTime.ParseExact(deal.Closetime, PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("org_deal_no", deal.Org_deal_no);
                cmd.Parameters.AddWithValue("del_deal_no", deal.Del_deal_no);
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }
        
    }
}
