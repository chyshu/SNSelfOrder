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
    public class DownloadService
    {
        private SelfOrderSettingClass posSetting = null;
     
        public DownloadService(SelfOrderSettingClass setting)
        {
            posSetting = setting;
        }

        #region Last_update
        private async Task<int> CheckLastUpdate(string SchemName, DateTime? adate, string StationID)
        {
            using (SQLiteConnection cn = new SQLiteConnection(posSetting.ConnString))
            {
                cn.Open();
                SQLiteCommand cmd = cn.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = "select * from lastupdate where stationid=@stationid  and datatable=@datatable ";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("stationid", StationID);
                cmd.Parameters.AddWithValue("datatable", SchemName);
                DataTable lastupdateDT = new DataTable();
                da.Fill(lastupdateDT);
                if (lastupdateDT.Rows.Count > 0)
                {
                    DataRow lastupdate = lastupdateDT.Rows[0];
                    bool isUpdated = false;
                    if (lastupdate["server_update"] == DBNull.Value)
                    {
                        isUpdated = true;
                    }
                    else
                    {
                        DateTime localUPDDate = DateTime.Parse(lastupdate["server_update"].ToString());
                        if (localUPDDate < adate.Value)
                            isUpdated = true;
                    }
                    if (isUpdated)
                    {
                        cmd.CommandText = "update lastupdate  set server_update=@server_update where stationid=@stationid and datatable=@datatable  ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("stationid", StationID);
                        cmd.Parameters.AddWithValue("server_update", adate.Value);
                        cmd.Parameters.AddWithValue("datatable", SchemName);
                        cmd.ExecuteNonQuery();
                    }
                    // Common.LastCampaignUpdate = campaignlastupdate; 
                }
                else
                {

                    cmd.CommandText = @"insert into lastupdate (stationid, datatable,local_update,server_update ) 
                                        values (@stationid, @datatable,@local_update,@server_update)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("stationid", StationID);
                    cmd.Parameters.AddWithValue("datatable", SchemName);
                    cmd.Parameters.AddWithValue("server_update", adate.Value);
                    cmd.Parameters.AddWithValue("local_update", DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            return 0;
        }
        #endregion
        private async Task<string> ReadURL(string URL, string imgfilename)
        {
            string ret = "";
            try
            {
                HttpClient client = new HttpClient();
                var downloaddata = await client.GetAsync(URL);
                downloaddata.EnsureSuccessStatusCode();
                //PRPosUtils.writelog(downloaddata.Content.Headers);

                using (FileStream fileStream = new FileStream(imgfilename, FileMode.OpenOrCreate))
                {
                    var ms = await downloaddata.Content.ReadAsByteArrayAsync();
                    await fileStream.WriteAsync(ms, 0, ms.Length);
                    fileStream.Flush(true);
                    fileStream.Close();
                }
                ret = "";
            }
            catch (Exception err)
            {
                ret = err.Message;
            }
            return ret;
        }

        public async Task<string> CheckDownload()
        {
            string psec = "1.0";
            string ret = "";

            PosData p = new PosData()
            {
                MAC = posSetting.MAC,
                Code = posSetting.ConnectionCode,
                Token = posSetting.Token,
                DataType = "fastkey",
            };
            
            HttpClient client = new HttpClient();
            string postBody = JsonConvert.SerializeObject(p);
              Debug.WriteLine("POST = " +  postBody );
            HttpResponseMessage response = await client.PostAsync(posSetting.HostURL + "v3/" + "getPosDataV2", new StringContent(postBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            long TimeTicks = DateTime.Now.Ticks;
            string responseBody = await response.Content.ReadAsStringAsync();
            //Debug.WriteLine("response  = " + responseBody);
            PosDataV3 resultPosData = JsonConvert.DeserializeObject<PosDataV3>(responseBody);
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection(posSetting.ConnString))
                {
                    if (cn.State != ConnectionState.Open)
                    {
                        cn.Open();
                    }
                    SQLiteCommand cmd = cn.CreateCommand();
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;
                    List<string> PageImage = new List<string>();
                    string listImg = "";
                    string comma = "";
                    Debug.WriteLine("response Retcode  = " + resultPosData . Retcode);
                    
                    if (resultPosData.Retcode.Equals("0"))
                    {
                        //Debug.WriteLine("SettingValue=" + resultPosData.SettingValue);
                        if (resultPosData.SettingValue != "")
                        {
                            
                            TerminalSettingV3 tsSetting = JsonConvert.DeserializeObject<TerminalSettingV3>(resultPosData.SettingValue);
                            #region TERMINAL
                            if (tsSetting != null)
                            {
                                psec = "1.0.9";
                                
                                foreach(ParameterSetting param in tsSetting.Parameters)
                                {
                                    if (param.Del_flag == "1")
                                    {
                                        cmd.CommandText = @"delete from pssystem where code=@code and code_type=@code_type";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("code", param.Code);
                                        cmd.Parameters.AddWithValue("code_type", param.Code_type);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        DateTime upd_date = DateTime.ParseExact(param.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        cmd.CommandText = @"select * from pssystem where code=@code and code_type=@code_type";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("code", param.Code);
                                        cmd.Parameters.AddWithValue("code_type", param.Code_type);
                                        DataTable pssystem = new DataTable();
                                        da.Fill(pssystem);
                                        if (pssystem.Rows.Count == 0)
                                        {
                                            cmd.CommandText = @"insert into pssystem 
                                                            (code, code_type, f1,  f2, f3, i1,i2, n1, n2, upd_date) values 
                                                            (@code,@code_type,@f1,@f2,@f3,@i1,@i2,@n1,@n2,@upd_date)";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("code", param.Code);
                                            cmd.Parameters.AddWithValue("code_type", param.Code_type);
                                            cmd.Parameters.AddWithValue("f1", param.F1);
                                            cmd.Parameters.AddWithValue("f2", param.F2);
                                            cmd.Parameters.AddWithValue("f3", param.F3);
                                            cmd.Parameters.AddWithValue("i1", (param.I1 ==""? DBNull.Value: param.I1) );
                                            cmd.Parameters.AddWithValue("i2", (param.I2 == "" ? DBNull.Value : param.I2));
                                            cmd.Parameters.AddWithValue("n1", (param.N1 == "" ? DBNull.Value : param.N1));
                                            cmd.Parameters.AddWithValue("n2", (param.N1 == "" ? DBNull.Value : param.N2));
                                            cmd.Parameters.AddWithValue("del_flag", param.Del_flag);
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                            cmd.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            cmd.CommandText = @"update pssystem set f1=@f1,f2=@f2,f3=@f3,i1=@i1,i2=@i2,n1=@n1,n2=@n2,upd_date=@upd_date  where code=@code and code_type=@code_type ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("code", param.Code);
                                            cmd.Parameters.AddWithValue("code_type", param.Code_type);
                                            cmd.Parameters.AddWithValue("f1", param.F1);
                                            cmd.Parameters.AddWithValue("f2", param.F2);
                                            cmd.Parameters.AddWithValue("f3", param.F3);
                                            cmd.Parameters.AddWithValue("i1", (param.I1 == "" ? DBNull.Value : param.I1));
                                            cmd.Parameters.AddWithValue("i2", (param.I2 == "" ? DBNull.Value : param.I2));
                                            cmd.Parameters.AddWithValue("n1", (param.N1 == "" ? DBNull.Value : param.N1));
                                            cmd.Parameters.AddWithValue("n2", (param.N1 == "" ? DBNull.Value : param.N2));
                                            cmd.Parameters.AddWithValue("del_flag", param.Del_flag);
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                psec = "1.1.0";
                                // update page images
                                foreach (PageImage pimage in tsSetting.PageImages)
                                {
                                    PageImage.Add(pimage.PageName);
                                    DateTime upd_date = DateTime.ParseExact(pimage.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    cmd.CommandText = @"select * from pssystem where code=@code and code_type='PageImage'";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("code", pimage.PageName);
                                    DataTable pssystem = new DataTable();
                                    da.Fill(pssystem);
                                    // Debug.WriteLine( pimage.Upd_date);
                                    if (pssystem.Rows.Count > 0)
                                    {
                                        DateTime sysupd_date = DateTime.Now;
                                        DateTime.TryParse(pssystem.Rows[0]["upd_date"].ToString(), out sysupd_date);
                                        if (sysupd_date != upd_date)
                                        {
                                            
                                            cmd.CommandText = @"update pssystem set f1=@f1, dn_flag=@dn_flag,upd_date=@upd_date ,ftime=null
                                                            where code=@code and code_type=@code_type ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("code", pimage.PageName);
                                            cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                            cmd.Parameters.AddWithValue("code_type", "PageImage");
                                            cmd.Parameters.AddWithValue("dn_flag", "N");
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                            cmd.ExecuteNonQuery();

                                        }
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"insert into pssystem (code,f1,code_type, dn_flag,upd_date,ftime) values 
                                                            (@code,@f1,@code_type,@dn_flag,@upd_date,null)";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("code", pimage.PageName);
                                        cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                        cmd.Parameters.AddWithValue("code_type", "PageImage");
                                        cmd.Parameters.AddWithValue("dn_flag", "N");
                                        cmd.Parameters.AddWithValue("upd_date", upd_date);
                                        cmd.ExecuteNonQuery();

                                    }
                                }
                                listImg = "";
                                comma = "";
                                foreach (string str in PageImage)
                                {
                                    listImg += comma + "'" + str + "'";
                                    comma = ",";
                                }

                                cmd.CommandText = @"delete from pssystem   where  code_type='PageImage' and code not in (" + listImg + ")";
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();

                                psec = "1.1.1 ";
                                PageImage = new List<string>();
                                // update banner images
                                foreach (BannerImage pimage in tsSetting.BannerImages)
                                {
                                    PageImage.Add(pimage.PageName);
                                    DateTime upd_date = DateTime.ParseExact(pimage.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    cmd.CommandText = @"select * from pssystem where code=@code and code_type='banner_image'";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("code", pimage.PageName);
                                    DataTable pssystem = new DataTable();
                                    da.Fill(pssystem);
                                    if (pssystem.Rows.Count > 0)
                                    {
                                        DateTime sysupd_date = DateTime.Now;
                                        // if (!pssystem.Rows[0]["upd_date"].ToString().Equals(""))
                                        DateTime.TryParse(pssystem.Rows[0]["upd_date"].ToString(), out sysupd_date);

                                        if (sysupd_date != upd_date)
                                        {
                                            cmd.CommandText = @"update pssystem set f1=@f1, dn_flag=@dn_flag,upd_date=@upd_date,i1=@i1,i2=@i2,f3=@del_flag,ftime=null  where code=@code and code_type=@code_type ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("code", pimage.PageName);
                                            cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                            cmd.Parameters.AddWithValue("code_type", "banner_image");
                                            cmd.Parameters.AddWithValue("dn_flag", "N");
                                            cmd.Parameters.AddWithValue("i1", pimage.Disp_order);
                                            cmd.Parameters.AddWithValue("i2", pimage.Disp_delay);
                                            cmd.Parameters.AddWithValue("del_flag", pimage.Del_flag);
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                            cmd.ExecuteNonQuery();

                                        }
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"insert into pssystem (code,f1,code_type, dn_flag,upd_date,i1,i2,f3,ftime) values
                                                            (@code,@f1,@code_type,@dn_flag,@upd_date,@i1,@i2,@del_flag,null)";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("code", pimage.PageName);
                                        cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                        cmd.Parameters.AddWithValue("code_type", "banner_image");
                                        cmd.Parameters.AddWithValue("dn_flag", "N");
                                        cmd.Parameters.AddWithValue("i1", pimage.Disp_order);
                                        cmd.Parameters.AddWithValue("i2", pimage.Disp_delay);
                                        cmd.Parameters.AddWithValue("del_flag", pimage.Del_flag);
                                        cmd.Parameters.AddWithValue("upd_date", upd_date);
                                        cmd.ExecuteNonQuery();

                                    }
                                }


                                listImg = "";
                                comma = "";
                                foreach (string str in PageImage)
                                {
                                    listImg += comma + "'" + str + "'";
                                    comma = ",";
                                }

                                cmd.CommandText = @"delete from  pssystem  where  code_type='banner_image' and code not in (" + listImg + ")";
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();

                                psec = "1.1.2 ";
                                PageImage = new List<string>();
                                // update banner images
                                foreach (ButtonImage pimage in tsSetting.ButtonImages)
                                {
                                    PageImage.Add(pimage.ButtonName);
                                    DateTime upd_date = DateTime.ParseExact(pimage.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    cmd.CommandText = @"select * from pssystem where code=@code and code_type='mod_button'";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("code", pimage.ButtonName);
                                    DataTable pssystem = new DataTable();
                                    da.Fill(pssystem);
                                    if (pssystem.Rows.Count > 0)
                                    {
                                        DateTime sysupd_date = DateTime.Now;
                                        //if (!pssystem.Rows[0]["upd_date"].ToString().Equals(""))
                                        DateTime.TryParse(pssystem.Rows[0]["upd_date"].ToString(), out sysupd_date);
                                        if (sysupd_date != upd_date)
                                        {
                                            cmd.CommandText = @"update pssystem set f1=@f1, dn_flag=@dn_flag,upd_date=@upd_date,i1=@i1,i2=@i2,f3=@del_flag,ftime=null  where code=@code and code_type=@code_type ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("code", pimage.ButtonName);
                                            cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                            cmd.Parameters.AddWithValue("code_type", "mod_button");
                                            cmd.Parameters.AddWithValue("dn_flag", "N");
                                            cmd.Parameters.AddWithValue("i1", pimage.Disp_order);
                                            cmd.Parameters.AddWithValue("i2", pimage.Disp_delay);
                                            cmd.Parameters.AddWithValue("del_flag", pimage.Del_flag);
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                            cmd.ExecuteNonQuery();

                                        }
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"insert into pssystem (code,f1,code_type, dn_flag,upd_date,i1,i2,f3,ftime) 
                                                        values
                                                        (@code,@f1,@code_type,@dn_flag,@upd_date,@i1,@i2,@del_flag,null)";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("code", pimage.ButtonName);
                                        cmd.Parameters.AddWithValue("f1", pimage.Imagefile);
                                        cmd.Parameters.AddWithValue("code_type", "mod_button");
                                        cmd.Parameters.AddWithValue("dn_flag", "N");
                                        cmd.Parameters.AddWithValue("i1", pimage.Disp_order);
                                        cmd.Parameters.AddWithValue("i2", pimage.Disp_delay);
                                        cmd.Parameters.AddWithValue("del_flag", pimage.Del_flag);
                                        cmd.Parameters.AddWithValue("upd_date", upd_date);
                                        cmd.ExecuteNonQuery();

                                    }
                                }


                                listImg = "";
                                comma = "";
                                foreach (string str in PageImage)
                                {
                                    listImg += comma + "'" + str + "'";
                                    comma = ",";
                                }

                                cmd.CommandText = @"delete from  pssystem  where  code_type='mod_button' and code not in (" + listImg + ")";
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();


                                psec = "1.2";
                                // update parameter
                                cmd.CommandText = @"update pssystem set f1=@f1 where code=@code ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Gen_tickno);
                                cmd.Parameters.AddWithValue("code", "generate_ticket_number");
                                cmd.ExecuteNonQuery();


                                cmd.CommandText = @"update pssystem set f1=@f1 where code=@code ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Reset_tickno);
                                cmd.Parameters.AddWithValue("code", "reset_ticket_number");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Ask_ordertype);
                                cmd.Parameters.AddWithValue("code", "ask_order_type");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Ask_tableno);
                                cmd.Parameters.AddWithValue("code", "ask_table_number");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Ask_cover);
                                cmd.Parameters.AddWithValue("code", "ask_persson_number");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Ask_member_card);
                                cmd.Parameters.AddWithValue("code", "ask_member_card");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Local_file_folder);
                                cmd.Parameters.AddWithValue("code", "output_file_path");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Spool_folder);
                                cmd.Parameters.AddWithValue("code", "Spool_Folder");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Keep_local_file);
                                cmd.Parameters.AddWithValue("code", "keep_local_file");
                                cmd.ExecuteNonQuery();

                                /*
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Holiday_charge);
                                cmd.Parameters.AddWithValue("code", "Holiday_charge");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Holiday_charge_item);
                                cmd.Parameters.AddWithValue("code", "Holiday_charge_item");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Card_charge);
                                cmd.Parameters.AddWithValue("code", "Card_charge");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Card_charge_item);
                                cmd.Parameters.AddWithValue("code", "Card_charge_item");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Table_service_charge);
                                cmd.Parameters.AddWithValue("code", "Table_service_charge");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Table_service_charge_item);
                                cmd.Parameters.AddWithValue("code", "Table_service_charge_item");
                                cmd.ExecuteNonQuery();


                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Plasticbag_item);
                                cmd.Parameters.AddWithValue("code", "Plasticbag_item");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Bag_Message);
                                cmd.Parameters.AddWithValue("code", "Bag_message");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Markup_item);
                                cmd.Parameters.AddWithValue("code", "Markup_item");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Markup_Message);
                                cmd.Parameters.AddWithValue("code", "Markup_message");
                                cmd.ExecuteNonQuery();
                                */
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Default_tender);
                                cmd.Parameters.AddWithValue("code", "Default_tender");
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("f1", tsSetting.Paycount_tender);
                                cmd.Parameters.AddWithValue("code", "Paycount_tender");
                                cmd.ExecuteNonQuery();



                                cmd.CommandText = @"select * from pssystem where code='default_order_type' ";
                                cmd.Parameters.Clear();
                                DataTable default_order_typeDT = new DataTable();
                                da.Fill(default_order_typeDT);
                                if (default_order_typeDT.Rows.Count > 0)
                                {
                                    cmd.CommandText = @"update  pssystem set f1=@f1 where code='default_order_type' ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("f1", resultPosData.Default_order);
                                    cmd.ExecuteNonQuery();

                                }
                                else
                                {
                                    cmd.CommandText = @"insert into  pssystem (code,  f1) values ('default_order_type',@f1) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("f1", resultPosData.Default_order);
                                    cmd.ExecuteNonQuery();

                                }
                                PRPosUtils.DefaultOrderType = PRPosDB.ReadString("default_order_type");

                                if (tsSetting.Table_service_charge_rate != "")
                                {
                                    cmd.CommandText = @"update pssystem set n1=@n1 where code=@code ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Table_service_charge_rate));
                                    cmd.Parameters.AddWithValue("code", "Table_service_charge_rate");
                                    int effect = cmd.ExecuteNonQuery();
                                    if (effect == 0)
                                    {
                                        cmd.CommandText = @"insert into  pssystem  (code,n1 ) values  (@code,@n1) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Table_service_charge_rate));
                                        cmd.Parameters.AddWithValue("code", "Table_service_charge_rate");
                                        cmd.ExecuteNonQuery();
                                    }

                                }

                                if (tsSetting.Card_charge_rate != "")
                                {
                                    cmd.CommandText = @"update pssystem set n1=@n1 where code=@code ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Card_charge_rate));
                                    cmd.Parameters.AddWithValue("code", "Card_charge_rate");
                                    int effect = cmd.ExecuteNonQuery();
                                    if (effect == 0)
                                    {
                                        cmd.CommandText = @"insert into  pssystem  (code,n1) values  (@code,@n1) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Card_charge_rate));
                                        cmd.Parameters.AddWithValue("code", "Card_charge_rate");
                                        cmd.ExecuteNonQuery();
                                    }

                                }

                                if (tsSetting.Holiday_charge_rate != "")
                                {
                                    cmd.CommandText = @"update pssystem set n1=@n1 where code=@code ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Holiday_charge_rate));
                                    cmd.Parameters.AddWithValue("code", "Holiday_charge_rate");
                                    int effect = cmd.ExecuteNonQuery();
                                    if (effect == 0)
                                    {
                                        cmd.CommandText = @"insert into  pssystem  (code,n1) values  (@code,@n1) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("n1", decimal.Parse(tsSetting.Holiday_charge_rate));
                                        cmd.Parameters.AddWithValue("code", "Holiday_charge_rate");
                                        cmd.ExecuteNonQuery();
                                    }

                                }

                                if (tsSetting.Message_time_out != "")
                                {

                                    cmd.CommandText = @"update pssystem set i1=@i1 where code=@code ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("i1", int.Parse(tsSetting.Message_time_out));
                                    cmd.Parameters.AddWithValue("code", "alert_display_time");
                                    int effect= cmd.ExecuteNonQuery();
                                    if(effect == 0)
                                    {
                                        cmd.CommandText = @"insert into  pssystem  (code,i1) values  (@code,@i1) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("i1", int.Parse(tsSetting.Message_time_out));
                                        cmd.Parameters.AddWithValue("code", "alert_display_time");
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                if (tsSetting.Order_time_out != "")
                                {
                                    cmd.CommandText = @"update pssystem set i1=@i1 where code=@code ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("i1", int.Parse(tsSetting.Order_time_out));
                                    cmd.Parameters.AddWithValue("code", "waiting_time");
                                    int effect = cmd.ExecuteNonQuery();

                                    if (effect == 0)
                                    {
                                        cmd.CommandText = @"insert into  pssystem  (code,i1) values  (@code,@i1) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("i1", int.Parse(tsSetting.Order_time_out));
                                        cmd.Parameters.AddWithValue("code", "waiting_time");
                                        cmd.ExecuteNonQuery();
                                    }

                                }

                            }
                            #endregion
                        }
                    }
                    /* Read read all parameter */
                    PRPosDB.ReadParameter();
                    string imgpath = posSetting.FilePath;
                    Debug.WriteLine(posSetting.CustomerID +","+ posSetting.StoreCode+","+ posSetting.PosCode);
                    App.log.Info("image path is "+ imgpath);
                    try
                    {
                        psec = "2.0";
                        cmd.CommandText = @"select * from pssystem where  1=1 and code_type='PageImage' ";
                        // where img_flag='N' ";
                        cmd.Parameters.Clear();
                        DataTable pssystemDT = new DataTable();
                        da.Fill(pssystemDT);
                        psec = "2.01";
                        TimeTicks = DateTime.Now.Ticks;

                        foreach (DataRow row in pssystemDT.Rows)
                        {
                            bool downloadFile = true;
                            FileInfo finfo;
                            psec = "2.02";
                            if (!row["f1"].ToString().Equals(""))
                            {
                                string imgticks = Path.GetFileNameWithoutExtension(row["f1"].ToString());
                                long ticktime = 0;
                                long.TryParse(imgticks, out ticktime);
                                string imgfilename = Path.Combine(imgpath, row["f1"].ToString());
                                psec = "2.03:" + imgfilename;
                                if (File.Exists(imgfilename))
                                {
                                    finfo = new FileInfo(imgfilename);
                                    DateTime ftime;
                                    if (!row["ftime"].ToString().Equals(""))
                                    {
                                        if (DateTime.TryParse(row["ftime"].ToString(), out ftime))
                                        {
                                            //PRPosUtils.writelog(imgfilename + " " + finfo.LastWriteTime.ToString("yyyyMMddHHmmss") + "," + ftime.ToString("yyyyMMddHHmmss"));
                                            var diffsec = new TimeSpan(finfo.LastWriteTime.Ticks - ftime.Ticks).TotalSeconds;
                                            if (diffsec < 120)
                                                downloadFile = false;

                                            //if ((ftime.Ticks > ticktime) && (ticktime != 0))
                                            //    downloadFile = false;
                                        }
                                    }
                                }
                                if (downloadFile)
                                {
                                    psec = "2.04:" + imgfilename;
                                    string URL = posSetting.HostURL + "items/" + posSetting.CustomerID + "/" + row["f1"].ToString();
                                    //PRPosUtils.writelog(URL);
                                    var retresult = await ReadURL(URL, imgfilename);
                                    /*
                                    var downloaddata = await client.GetAsync(URL);
                                    downloaddata.EnsureSuccessStatusCode();
                                    //PRPosUtils.writelog(downloaddata.Content.Headers);                                                                       
                                    using (FileStream fileStream = new FileStream(imgfilename, FileMode.OpenOrCreate))
                                    {
                                        var ms = await downloaddata.Content.ReadAsByteArrayAsync();
                                        fileStream.Write(ms, 0, ms.Length);
                                        fileStream.Flush();
                                        fileStream.Close();
                                    }*/
                                    psec = "2.05:" + imgfilename;
                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    // PRPosUtils.writelog(Path.Combine(Common.FilePath, campain["campaign_name"].ToString() + @"\" + campain["FileName"].ToString()));

                                    psec = "2.11:" + imgfilename;
                                    cmd.CommandText = @"update pssystem set dn_flag='Y',ftime=@ftime where code=@code and code_type=@code_type";
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("code", row["code"].ToString());
                                    cmd.Parameters.AddWithValue("code_type", row["code_type"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat)));
                                    psec = "2.12:" + imgfilename;
                                    //PRPosUtils.writelog("PageImage " + finfo2.FullName + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    cmd.ExecuteNonQuery();

                                    psec = "2.14";
                                }
                            }
                        }//end for

                        App.log.Info(" PageImage:" + (DateTime.Now.Ticks - TimeTicks).ToString());
                        Debug.WriteLine(" PageImage:" + (DateTime.Now.Ticks - TimeTicks).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        Debug.WriteLine(psec + " Check Download IMAGE :" + errhttp.Message);
                        App.log.Error(" PageImage:" + (DateTime.Now.Ticks - TimeTicks).ToString());
                    }

                    psec = "2.2";
                    try
                    {
                        cmd.CommandText = @"select * from pssystem where 1=1  and code_type='banner_image'  ";// where img_flag='N' ";
                        cmd.Parameters.Clear();
                        DataTable pssystemDT = new DataTable();
                        psec = "2.21";
                        da.Fill(pssystemDT);
                        TimeTicks = DateTime.Now.Ticks;

                        foreach (DataRow row in pssystemDT.Rows)
                        {
                            bool downloadFile = true;
                            FileInfo finfo;
                            psec = "2.22";
                            if (!row["f1"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["f1"].ToString());
                                if (File.Exists(imgfilename))
                                {
                                    finfo = new FileInfo(imgfilename);
                                    DateTime ftime;
                                    if (!row["ftime"].ToString().Equals(""))
                                    {
                                        if (DateTime.TryParse(row["ftime"].ToString(), out ftime))
                                        {
                                            var diffsec = new TimeSpan(finfo.LastWriteTime.Ticks - ftime.Ticks).TotalSeconds;
                                            if (diffsec < 120)
                                                downloadFile = false;
                                        }
                                    }
                                }
                                if (downloadFile)
                                {
                                    psec = "2.23:" + imgfilename;
                                    string URL = posSetting.HostURL + "items/" + posSetting.CustomerID + "/" + row["f1"].ToString();
                                    var retresult = await ReadURL(URL, imgfilename);
                                    /*
                                    //PRPosUtils.writelog(URL);
                                    var downloaddata = await client.GetAsync(URL);
                                    downloaddata.EnsureSuccessStatusCode();
                                    //PRPosUtils.writelog(downloaddata.Content.Headers);

                                    using (FileStream fileStream = new FileStream(imgfilename, FileMode.OpenOrCreate))
                                    {
                                        var ms = await downloaddata.Content.ReadAsByteArrayAsync();
                                        fileStream.Write(ms, 0, ms.Length);
                                        fileStream.Flush();
                                        fileStream.Close();
                                    }
                                    */
                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    // PRPosUtils.writelog(Path.Combine(Common.FilePath, campain["campaign_name"].ToString() + @"\" + campain["FileName"].ToString()));

                                    psec = "2.24:" + imgfilename;
                                    cmd.CommandText = @"update pssystem set dn_flag='Y',ftime=@ftime where code=@code and code_type=@code_type";
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("code", row["code"].ToString());
                                    cmd.Parameters.AddWithValue("code_type", row["code_type"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString(PRPosUtils.DateFormat+" "+ PRPosUtils.TimeFormat)));
                                    psec = "2.25:" + imgfilename;
                                    App.log.Info("banner_image " + finfo2.FullName + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));                                    
                                    cmd.ExecuteNonQuery();

                                }
                            }
                          //  App.log.Info(psec + " banner_image");
                        }// end for

                        //PRPosUtils.writelog(" banner_image:" + ((DateTime.Now.Ticks - TimeTicks)/10000).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " Check Download banner:" + errhttp.Message);
                        Debug.WriteLine(psec + " Check Download banner :" + errhttp.Message);
                    }
                    psec = "2.3";
                    try
                    {
                        TimeTicks = DateTime.Now.Ticks;
                        cmd.CommandText = @"select * from pssystem where  1=1  and code='mod_button'  ";
                        cmd.Parameters.Clear();
                        DataTable pssystemDT = new DataTable();
                        da.Fill(pssystemDT);

                        foreach (DataRow row in pssystemDT.Rows)
                        {
                            bool downloadFile = true;
                            FileInfo finfo;

                            if (!row["f1"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["f1"].ToString());
                                psec = "2.3.2:" + imgfilename;
                                if (File.Exists(imgfilename))
                                {
                                    finfo = new FileInfo(imgfilename);
                                    DateTime ftime;
                                    if (!row["ftime"].ToString().Equals(""))
                                    {
                                        if (DateTime.TryParse(row["ftime"].ToString(), out ftime))
                                        {
                                            var diffsec = new TimeSpan(finfo.LastWriteTime.Ticks - ftime.Ticks).TotalSeconds;
                                            if (diffsec < 120)
                                                downloadFile = false;
                                        }
                                    }
                                }
                                if (downloadFile)
                                {
                                    psec = "2.3.3:" + imgfilename;
                                    string URL = posSetting.HostURL + "items/" + posSetting.CustomerID + "/" + row["f1"].ToString();
                                    //PRPosUtils.writelog(URL);
                                    App.log.Info(imgfilename);
                                    var retresult = await ReadURL(URL, imgfilename);
                                    /*
                                    var downloaddata = await client.GetAsync(URL);
                                    downloaddata.EnsureSuccessStatusCode();
                                    //PRPosUtils.writelog(downloaddata.Content.Headers);

                                    using (FileStream fileStream = new FileStream(imgfilename, FileMode.OpenOrCreate))
                                    {
                                        var ms = await downloaddata.Content.ReadAsByteArrayAsync();
                                        fileStream.Write(ms, 0, ms.Length);
                                        fileStream.Flush();
                                        fileStream.Close();
                                    }*/
                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    

                                    psec = "2.3.4:" + imgfilename;
                                    cmd.CommandText = @"update pssystem set dn_flag='Y' ,ftime=@ftime where code=@code and code_type=@code_type";
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("code", row["code"].ToString());
                                    cmd.Parameters.AddWithValue("code_type", row["code_type"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat)));
                                    App.log.Info(" mod_button " + finfo2.FullName + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    psec = "2.3.5:" + imgfilename;
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        } // end for

                        App.log.Info(" mod_button:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " mod_button:" + errhttp.Message);
                        Debug.WriteLine(psec + " mod_button :" + errhttp.Message);
                    }

                    cmd.CommandText = @"delete from  posfastkeyset  where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();


                    cmd.CommandText = @"delete from  posfastkey02  where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    psec = "3.0";
                    // PRPosUtils.writelog(resultPosData.Retvalue);
                    if (resultPosData.Retvalue != "")
                    {
                        psec = "3.1";
                        
                        TimeTicks = DateTime.Now.Ticks;
                        List<FASTKeySet> FastKeys = JsonConvert.DeserializeObject<List<FASTKeySet>>(resultPosData.Retvalue);

                        foreach (FASTKeySet fastkeyset in FastKeys)
                        {
                            // PRPosUtils.writelog("FastKeys sid=" + fastkeyset.Sid+" , " + fastkeyset);
                            psec = "3.1.2";
                            if (fastkeyset.Del_flag.Equals("Y"))
                            {
                                psec = "3.1.2.1";
                                cmd.CommandText = @"delete from posfastkeyset where sid=@sid and customerid=@customerid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", fastkeyset.Sid);
                                cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                psec = "3.1.2.2";
                                cmd.CommandText = @"select * from posfastkeyset where sid=@sid and customerid=@customerid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", fastkeyset.Sid);
                                cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                DataTable posfastkeysetDT = new DataTable();
                                da.Fill(posfastkeysetDT);
                                if (posfastkeysetDT.Rows.Count > 0)
                                {
                                    DateTime? UpdDate = null;

                                    DateTime? ServerUpdDate = null;
                                    if (!fastkeyset.Upd_date.Equals(""))
                                        ServerUpdDate = DateTime.Parse(fastkeyset.Upd_date, CultureInfo.CreateSpecificCulture("en-AU"));
                                    if (!posfastkeysetDT.Rows[0]["upd_date"].ToString().Equals(""))
                                        UpdDate = DateTime.Parse(posfastkeysetDT.Rows[0]["upd_date"].ToString());

                                    psec = "3.1.3";
                                    if (UpdDate != ServerUpdDate)
                                    {
                                        cmd.CommandText =
                                            @"update posfastkeyset set store_code=@store_code, location_code=@location_code, 
                                               set_code=@set_code,set_name=@set_name,del_flag=@del_flag,upd_date=@upd_date 
                                           where sid=@sid and  customerid=@customerid";

                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", fastkeyset.Sid);
                                        cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                        cmd.Parameters.AddWithValue("store_code", fastkeyset.Store_code);
                                        cmd.Parameters.AddWithValue("location_code", fastkeyset.Location);
                                        cmd.Parameters.AddWithValue("set_code", fastkeyset.Set_code);
                                        cmd.Parameters.AddWithValue("set_name", fastkeyset.Set_name);
                                        cmd.Parameters.AddWithValue("del_flag", fastkeyset.Del_flag);
                                        if (ServerUpdDate == null)
                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                        else
                                            cmd.Parameters.AddWithValue("upd_date", ServerUpdDate);
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                else
                                {
                                    psec = "3.1.4";
                                    cmd.CommandText = @"insert into posfastkeyset " +
                                                      @"  (sid,customerid,store_code, location_code,set_code,set_name,del_flag,upd_date)" +
                                                      @"  values " +
                                                      @"  (@sid,@customerid,@store_code, @location_code,@set_code,@set_name,@del_flag,@upd_date) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", fastkeyset.Sid);
                                    cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                    cmd.Parameters.AddWithValue("store_code", fastkeyset.Store_code);
                                    cmd.Parameters.AddWithValue("location_code", fastkeyset.Location);
                                    cmd.Parameters.AddWithValue("set_code", fastkeyset.Set_code);
                                    cmd.Parameters.AddWithValue("set_name", fastkeyset.Set_name);
                                    cmd.Parameters.AddWithValue("del_flag", fastkeyset.Del_flag);
                                    if (fastkeyset.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(fastkeyset.Upd_date, CultureInfo.GetCultureInfo("en-AU")));

                                    cmd.ExecuteNonQuery();

                                }
                            }
                            psec = "3.1.5";
                            await CheckLastUpdate("posfastkeyset", DateTime.Parse(fastkeyset.Upd_date, CultureInfo.GetCultureInfo("en-AU")), resultPosData.PosID);
                            
                            foreach (FASTKey fastkey in fastkeyset.FastKeys)
                            {
                                psec = "3.1.6";
                           //     Debug.WriteLine("FastKeys sid=" + fastkey.Sid+" , " + fastkey.Caption +" " + fastkey.Del_flag+" "+ fastkeyset.CustomerID);
                                if (fastkey.Del_flag.Equals("Y"))
                                {
                                    if (fastkey.Op_code.Equals("1"))
                                    {
                                        cmd.CommandText = @"delete from posfastkey02 where psid=@sid  and customerid=@customerid and  op_code='2' ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                        cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                        cmd.ExecuteNonQuery();

                                        cmd.CommandText = @"delete from posfastkey02 where sid=@sid  and customerid=@customerid  and  op_code='1' ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                        cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"delete from posfastkey02 where sid=@sid  and customerid=@customerid  and  op_code='2' ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                        cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                else
                                {
                                    cmd.CommandText = @"select * from posfastkey02 where sid=@sid  and customerid=@customerid ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                    cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                    DataTable posfastkey02DT = new DataTable();
                                    da.Fill(posfastkey02DT);
                                    if (posfastkey02DT.Rows.Count > 0)
                                    {
                                        psec = "3.1.7";
                                        DataRow r = posfastkey02DT.Rows[0];

                                        DateTime? UpdDate = null;
                                        DateTime? ServerUpdDate = null;
                                        if (!fastkeyset.Upd_date.Equals(""))
                                            ServerUpdDate = DateTime.ParseExact(fastkey.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("en-AU"));
                                        if (!r["upd_date"].ToString().Equals(""))
                                            UpdDate = DateTime.Parse(r["upd_date"].ToString());
                                        //  PRPosUtils.writelog("317 sid=" + fastkey.Sid + "," + fastkey.Caption + "," + fastkey.Del_flag +
                                        //                    ","+ fastkey.Upd_date+"," + ServerUpdDate.ToString()) ; 
                                      //  Debug.WriteLine(fastkey.Sid + "," + fastkey.Caption + "," + fastkey.Del_flag +
                                       //                     ","+ fastkey.Upd_date+"," + ServerUpdDate.ToString() );
                                        if (UpdDate != ServerUpdDate)
                                        {
                                            cmd.CommandText =
                                            @"update posfastkey02 set 
                                           store_code=@store_code, set_code=@set_code, psid=@psid, caption=@caption,caption2=@caption2,caption3=@caption3, 
                                           op_code=@op_code, ref_code=@ref_code, width=@width, height=@height, display_yn=@display_yn,  default_yn=@default_yn,
                                           caption_yn =@caption_yn, caption2_yn=@caption2_yn,caption3_yn=@caption3_yn, fontcolor=@fontcolor, bgcolor=@bgcolor, 
                                           fontfamily=@fontfamily, fontsize=@fontsize, fontstyle=@fontstyle, ftime=@ftime,
                                           imagefile=@imagefile, disp_order=@disp_order, del_flag=@del_flag, upd_date=@upd_date,img_flag=@img_flag, img_flag=@img_flag,
                                           fullimage_yn=@fullimage_yn,textheight=@textheight,textbgcolor=@textbgcolor,priceline=@priceline 
                                           where sid=@sid and customerid =@customerid ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                            cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                            cmd.Parameters.AddWithValue("store_code", fastkey.Store_code);
                                            cmd.Parameters.AddWithValue("set_code", fastkey.Set_code);
                                            cmd.Parameters.AddWithValue("psid", fastkey.Psid);
                                            cmd.Parameters.AddWithValue("caption", fastkey.Caption);
                                            cmd.Parameters.AddWithValue("caption2", fastkey.Caption2);
                                            cmd.Parameters.AddWithValue("caption3", fastkey.Caption3);
                                            cmd.Parameters.AddWithValue("op_code", fastkey.Op_code);
                                            cmd.Parameters.AddWithValue("ref_code", fastkey.Ref_code);
                                            cmd.Parameters.AddWithValue("width", fastkey.Width);
                                            cmd.Parameters.AddWithValue("height", fastkey.Height);
                                            cmd.Parameters.AddWithValue("display_yn", fastkey.Display_yn);
                                            cmd.Parameters.AddWithValue("default_yn", fastkey.Default_yn);
                                            cmd.Parameters.AddWithValue("caption_yn", fastkey.Caption_yn);
                                            cmd.Parameters.AddWithValue("caption2_yn", fastkey.Caption2_yn);
                                            cmd.Parameters.AddWithValue("caption3_yn", fastkey.Caption3_yn);
                                            cmd.Parameters.AddWithValue("fontcolor", fastkey.Fontcolor);
                                            cmd.Parameters.AddWithValue("bgcolor", fastkey.Bgcolor);
                                            cmd.Parameters.AddWithValue("fontfamily", fastkey.Fontfamily);
                                            cmd.Parameters.AddWithValue("fontsize", fastkey.Fontsize);
                                            cmd.Parameters.AddWithValue("fontstyle", fastkey.Fontstyle);
                                            cmd.Parameters.AddWithValue("imagefile", fastkey.Imagefile);
                                            cmd.Parameters.AddWithValue("disp_order", fastkey.Disp_order);
                                            cmd.Parameters.AddWithValue("del_flag", fastkey.Del_flag);

                                            cmd.Parameters.AddWithValue("img_flag", "N");
                                            cmd.Parameters.AddWithValue("priceline", fastkey.PriceLine);
                                            cmd.Parameters.AddWithValue("fullimage_yn", fastkey.Fullimage_yn);
                                            cmd.Parameters.AddWithValue("textheight", fastkey.TextHeight);
                                            cmd.Parameters.AddWithValue("textbgcolor", fastkey.TextBGColor);
                                            cmd.Parameters.AddWithValue("ftime", DBNull.Value);
                                            if (fastkey.Upd_date.Equals(""))
                                                cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                            else
                                                cmd.Parameters.AddWithValue("upd_date", ServerUpdDate);
                                            cmd.ExecuteNonQuery();
                                        }

                                    }
                                    else
                                    {
                                        psec = "3.1.8";
                                        // PRPosUtils.writelog("posfastkey02 318 sid=" + fastkey.Sid + "," + fastkey.Caption +" "+ fastkey.Del_flag);
                                        cmd.CommandText =
                                            @"insert into posfastkey02 
                                         ( sid, customerid, store_code, set_code, psid, caption, caption2, caption3, op_code, ref_code, 
                                           width, height, display_yn,  default_yn, caption_yn, fontcolor, bgcolor, fontfamily, 
                                           fontsize, fontstyle, imagefile, disp_order, del_flag, upd_date,img_flag,fullimage_yn,textheight,
                                           textbgcolor,priceline,caption2_yn,caption3_yn, ftime) 
                                          values 
                                         (@sid, @customerid, @store_code, @set_code, @psid, @caption,@caption2, @caption3,  @op_code, @ref_code, 
                                          @width, @height, @display_yn,   @default_yn, @caption_yn, @fontcolor, @bgcolor, @fontfamily, 
                                          @fontsize, @fontstyle, @imagefile, @disp_order, @del_flag, @upd_date,@img_flag,@fullimage_yn,@textheight,
                                          @textbgcolor,@priceline,@caption2_yn,@caption3_yn,@ftime) ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", fastkey.Sid);
                                        cmd.Parameters.AddWithValue("customerid", fastkeyset.CustomerID);
                                        cmd.Parameters.AddWithValue("store_code", fastkey.Store_code);
                                        cmd.Parameters.AddWithValue("set_code", fastkey.Set_code);
                                        cmd.Parameters.AddWithValue("psid", fastkey.Psid);
                                        cmd.Parameters.AddWithValue("caption2", fastkey.Caption2);
                                        cmd.Parameters.AddWithValue("caption3", fastkey.Caption3);
                                        cmd.Parameters.AddWithValue("caption", fastkey.Caption);
                                        cmd.Parameters.AddWithValue("op_code", fastkey.Op_code);
                                        cmd.Parameters.AddWithValue("ref_code", fastkey.Ref_code);
                                        cmd.Parameters.AddWithValue("width", fastkey.Width);
                                        cmd.Parameters.AddWithValue("height", fastkey.Height);
                                        cmd.Parameters.AddWithValue("display_yn", fastkey.Display_yn);
                                        cmd.Parameters.AddWithValue("default_yn", fastkey.Default_yn);
                                        cmd.Parameters.AddWithValue("caption_yn", fastkey.Caption_yn);
                                        cmd.Parameters.AddWithValue("caption2_yn", fastkey.Caption2_yn);
                                        cmd.Parameters.AddWithValue("caption3_yn", fastkey.Caption3_yn);
                                        cmd.Parameters.AddWithValue("fontcolor", fastkey.Fontcolor);
                                        cmd.Parameters.AddWithValue("bgcolor", fastkey.Bgcolor);
                                        cmd.Parameters.AddWithValue("fontfamily", fastkey.Fontfamily);
                                        cmd.Parameters.AddWithValue("fontsize", fastkey.Fontsize);
                                        cmd.Parameters.AddWithValue("fontstyle", fastkey.Fontstyle);
                                        cmd.Parameters.AddWithValue("imagefile", fastkey.Imagefile);
                                        cmd.Parameters.AddWithValue("disp_order", fastkey.Disp_order);
                                        cmd.Parameters.AddWithValue("del_flag", fastkey.Del_flag);
                                        cmd.Parameters.AddWithValue("img_flag", "N");
                                        cmd.Parameters.AddWithValue("priceline", fastkey.PriceLine);
                                        cmd.Parameters.AddWithValue("ftime", DBNull.Value);
                                        if (fastkey.Upd_date.Equals(""))
                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                        else
                                            cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(fastkey.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                        cmd.Parameters.AddWithValue("fullimage_yn", fastkey.Fullimage_yn);
                                        cmd.Parameters.AddWithValue("textheight", fastkey.TextHeight);
                                        cmd.Parameters.AddWithValue("textbgcolor", fastkey.TextBGColor);
                                        cmd.ExecuteNonQuery();

                                    }
                                }
                            }
                        }
                        App.log.Info(" posfastkey02:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());

                    } // end 


                    psec = "4.1";

                    try
                    {
                        cmd.CommandText = @"select * from posfastkey02 where customerid=@customerid  ";//and img_flag='N'   and ifnull(imagefile,'')<>''";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        DataTable posfastkey02DT2 = new DataTable();
                        da.Fill(posfastkey02DT2);
                        client = new HttpClient();
                        TimeTicks = DateTime.Now.Ticks;
                        SQLiteTransaction trans = cn.BeginTransaction();

                        foreach (DataRow row in posfastkey02DT2.Rows)
                        {
                            psec = "4.2";

                            bool downloadFile = false;
                            FileInfo finfo;
                            if (!row["imagefile"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["imagefile"].ToString());
                                psec = "4.3:" + imgfilename;
                                if (row["img_flag"].ToString().Equals("N"))
                                    downloadFile = true;
                                else if (!File.Exists(imgfilename))
                                    downloadFile = true;

                                if (downloadFile)
                                {
                                    psec = "4.4:" + imgfilename;
                                    string URL = posSetting.HostURL + "items/" + posSetting.CustomerID + "/" + row["imagefile"].ToString();

                                    var retresult = await ReadURL(URL, imgfilename);


                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    // PRPosUtils.writelog(Path.Combine(Common.FilePath, campain["campaign_name"].ToString() + @"\" + campain["FileName"].ToString()));

                                    psec = "4.5";
                                    cmd.CommandText = @"update posfastkey02 set img_flag='Y',ftime=@ftime    where sid=@sid";
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("sid", row["sid"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat)));
                                    // PRPosUtils.writelog(finfo.FullName + " " + finfo.LastWriteTime);
                                    // PRPosUtils.writelog(psec + " posfastkey02 :" + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }//end for

                        trans.Commit();
                        trans.Dispose();
                        App.log.Info(" posfastkey02 image:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " posfastkey02:" + errhttp.Message);
                        Debug.WriteLine(psec + " posfastkey02:" + errhttp.Message);
                    }

                    psec = "5.1";
                    cmd.CommandText = @"delete  from psitem where ifnull(customerid,'XX')<>@customerid  ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.ItemValue != "")
                    {
                        TimeTicks = DateTime.Now.Ticks;
                        psec = "5.2";
                        List<PosItem> PosItems = JsonConvert.DeserializeObject<List<PosItem>>(resultPosData.ItemValue);
                        SQLiteTransaction trans = cn.BeginTransaction();
                        foreach (PosItem positem in PosItems)
                        {


                            cmd.CommandText = @"select * from psitem where customerid=@customerid and item_code=@item_code";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("item_code", positem.Item_code);
                            cmd.Parameters.AddWithValue("customerid", positem.Customerid);
                            DataTable psitemDT = new DataTable();
                            da.Fill(psitemDT);
                            if (psitemDT.Rows.Count > 0)
                            {
                                psec = "5.3";
                                DataRow r = psitemDT.Rows[0];

                                DateTime? UpdDate = null;

                                DateTime? ServerUpdDate = null;
                                if (!positem.Upd_date.Equals(""))
                                    ServerUpdDate = DateTime.Parse(positem.Upd_date, CultureInfo.GetCultureInfo("en-AU"));
                                if (!r["upd_date"].ToString().Equals(""))
                                    UpdDate = DateTime.Parse(r["upd_date"].ToString());

                                // PRPosUtils.writelog("psitem 52 code=" + positem.Item_code + "," + positem.Item_name + " " + positem.Del_flag + " " + positem.Soldout +" "+ positem.Upd_date+" "+ r["upd_date"].ToString());

                                if (UpdDate != ServerUpdDate)
                                {

                                    cmd.CommandText = @"update psitem set item_type=@item_type, item_kind=@item_kind,item_name=@item_name,item_name_fn=@item_name_fn,
                                                    description=@description,gst=@gst,cate_code=@cate_code,mod_code=@mod_code,set_code=@set_code,
                                                    kitchen_name=@kitchen_name,kitchen_name_f=@kitchen_name_f,kitchen_remark=@kitchen_remark,dept=@dept,
                                                    buttonid=@buttonid,sortorder=@sortorder,
                                                    p1=@p1,p2=@p2,p3=@p3,p4=@p4,p5=@p5,p6=@p6,m1=@m1,m2=@m2,m3=@m3,
                                                    s1=@s1,s2=@s2,s3=@s3,s4=@s4,s5=@s5,s6=@s6,s7=@s7,s8=@s8,s9=@s9,
                                                    sprice=@sprice,sprice2=@sprice2,sprice3=@sprice3,sprice4=@sprice4,sprice5=@sprice5,
                                                    sprice6=@sprice6,sprice7=@sprice7,sprice8=@sprice8,sprice9=@sprice9,sprice10=@sprice10,
                                                    vegetarian=@vegetarian,spicy=@spicy,printer_name=@printer_name,kds_name=@kds_name,image=@image,
                                                    del_flag=@del_flag,upd_date=@upd_date ,rest_usr=@rest_usr,img_flag=@img_flag,soldout=@soldout,ftime=@ftime
                                                    where  customerid=@customerid and item_code=@item_code";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("item_code", positem.Item_code);
                                    cmd.Parameters.AddWithValue("customerid", positem.Customerid);
                                    cmd.Parameters.AddWithValue("item_type", positem.Item_type);
                                    cmd.Parameters.AddWithValue("item_kind", positem.Item_kind);
                                    cmd.Parameters.AddWithValue("item_name", positem.Item_name);
                                    cmd.Parameters.AddWithValue("item_name_fn", positem.Item_name_fn);
                                    cmd.Parameters.AddWithValue("description", positem.Description);
                                    if (positem.Sprice1.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice", positem.Sprice1);
                                    if (positem.Sprice2.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice2", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice2", positem.Sprice2);
                                    if (positem.Sprice3.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice3", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice3", positem.Sprice3);
                                    if (positem.Sprice4.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice4", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice4", positem.Sprice4);
                                    if (positem.Sprice5.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice5", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice5", positem.Sprice5);
                                    if (positem.Sprice6.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice6", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice6", positem.Sprice6);
                                    if (positem.Sprice7.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice7", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice7", positem.Sprice7);
                                    if (positem.Sprice8.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice8", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice8", positem.Sprice8);
                                    if (positem.Sprice9.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice9", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice9", positem.Sprice9);
                                    if (positem.Sprice10.Equals(""))
                                        cmd.Parameters.AddWithValue("sprice10", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("sprice10", positem.Sprice10);

                                    cmd.Parameters.AddWithValue("soldout", positem.Soldout);
                                    cmd.Parameters.AddWithValue("gst", positem.Gst);
                                    cmd.Parameters.AddWithValue("cate_code", positem.Cate_code);
                                    cmd.Parameters.AddWithValue("mod_code", positem.Mod_code);
                                    cmd.Parameters.AddWithValue("set_code", positem.Set_code);
                                    cmd.Parameters.AddWithValue("kitchen_name", positem.Kitchen_name);
                                    cmd.Parameters.AddWithValue("kitchen_name_f", positem.Kitchen_name_f);
                                    cmd.Parameters.AddWithValue("kitchen_remark", positem.Kitchen_remark);
                                    cmd.Parameters.AddWithValue("dept", positem.Dept);
                                    cmd.Parameters.AddWithValue("buttonid", positem.Buttonid);
                                    cmd.Parameters.AddWithValue("sortorder", positem.Disp_order);
                                    cmd.Parameters.AddWithValue("p1", positem.P1);
                                    cmd.Parameters.AddWithValue("p2", positem.P2);
                                    cmd.Parameters.AddWithValue("p3", positem.P3);
                                    cmd.Parameters.AddWithValue("p4", positem.P4);
                                    cmd.Parameters.AddWithValue("p5", positem.P5);
                                    cmd.Parameters.AddWithValue("p6", positem.P6);
                                    cmd.Parameters.AddWithValue("m1", positem.M1);
                                    cmd.Parameters.AddWithValue("m2", positem.M2);
                                    cmd.Parameters.AddWithValue("m3", positem.M3);
                                    cmd.Parameters.AddWithValue("s1", positem.S1);
                                    cmd.Parameters.AddWithValue("s2", positem.S2);
                                    cmd.Parameters.AddWithValue("s3", positem.S3);
                                    cmd.Parameters.AddWithValue("s4", positem.S4);
                                    cmd.Parameters.AddWithValue("s5", positem.S5);
                                    cmd.Parameters.AddWithValue("s6", positem.S6);
                                    cmd.Parameters.AddWithValue("s7", positem.S7);
                                    cmd.Parameters.AddWithValue("s8", positem.S8);
                                    cmd.Parameters.AddWithValue("s9", positem.S9);
                                    cmd.Parameters.AddWithValue("spicy", positem.Spicy);
                                    cmd.Parameters.AddWithValue("vegetarian", positem.Vegetarian);
                                    cmd.Parameters.AddWithValue("printer_name", positem.Printer_name);
                                    cmd.Parameters.AddWithValue("kds_name", positem.Kds_name);
                                    cmd.Parameters.AddWithValue("image", positem.Imagefile);
                                    cmd.Parameters.AddWithValue("rest_usr", positem.Rest_usr);
                                    if (positem.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", ServerUpdDate);
                                    cmd.Parameters.AddWithValue("img_flag", "N");
                                    cmd.Parameters.AddWithValue("ftime", DBNull.Value);
                                    cmd.Parameters.AddWithValue("del_flag", positem.Del_flag);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                psec = "5.4";
                                cmd.CommandText =
                                    @"insert into  psitem
                                     (customerid,item_code, item_type,item_kind, item_name,item_name_fn,description,sprice,gst,
                                     cate_code,mod_code,set_code,kitchen_name,kitchen_name_f,kitchen_remark,dept,buttonid,sortorder,
                                     p1,p2,p3,p4,p5,p6,m1,m2,m3,
                                     s1,s2,s3,s4,s5,s6,s7,s8,s9,
                                     sprice,sprice2,sprice3,sprice4,sprice5,
                                     sprice6,sprice7,sprice8,sprice9,sprice10,
                                     vegetarian, spicy,printer_name,image,kds_name,del_flag,upd_date,rest_usr,ftime,img_flag)
                                   values 
                                  (  @customerid,@item_code, @item_type,@item_kind, @item_name,@item_name_fn,@description,@sprice,@gst,
                                     @cate_code,@mod_code,@set_code, @kitchen_name,@kitchen_name_f,@kitchen_remark,@dept,@buttonid,@sortorder,
                                     @p1,@p2,@p3,@p4,@p5,@p6,@m1,@m2,@m3,
                                     @s1,@s2,@s3,@s4,@s5,@s6,@s7,@s8,@s9,
                                     @sprice,@sprice2,@sprice3,@sprice4,@sprice5,
                                     @sprice6,@sprice7,@sprice8,@sprice9,@sprice10,
                                     @vegetarian, @spicy,@printer_name,@image,@kds_name,@del_flag, @upd_date,@rest_usr,null,'N')";

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("item_code", positem.Item_code);
                                cmd.Parameters.AddWithValue("customerid", positem.Customerid);
                                cmd.Parameters.AddWithValue("item_type", positem.Item_type);
                                cmd.Parameters.AddWithValue("item_kind", positem.Item_kind);
                                cmd.Parameters.AddWithValue("item_name", positem.Item_name);
                                cmd.Parameters.AddWithValue("item_name_fn", positem.Item_name_fn);
                                cmd.Parameters.AddWithValue("description", positem.Description);

                                if (positem.Sprice1.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice", positem.Sprice1);
                                if (positem.Sprice2.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice2", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice2", positem.Sprice2);
                                if (positem.Sprice3.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice3", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice3", positem.Sprice3);
                                if (positem.Sprice4.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice4", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice4", positem.Sprice4);
                                if (positem.Sprice5.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice5", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice5", positem.Sprice5);
                                if (positem.Sprice6.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice6", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice6", positem.Sprice6);
                                if (positem.Sprice7.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice7", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice7", positem.Sprice7);
                                if (positem.Sprice8.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice8", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice8", positem.Sprice8);
                                if (positem.Sprice9.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice9", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice9", positem.Sprice9);
                                if (positem.Sprice10.Equals(""))
                                    cmd.Parameters.AddWithValue("sprice10", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("sprice10", positem.Sprice10);

                                cmd.Parameters.AddWithValue("gst", positem.Gst);
                                cmd.Parameters.AddWithValue("cate_code", positem.Cate_code);
                                cmd.Parameters.AddWithValue("mod_code", positem.Mod_code);
                                cmd.Parameters.AddWithValue("set_code", positem.Set_code);
                                cmd.Parameters.AddWithValue("kitchen_name", positem.Kitchen_name);
                                cmd.Parameters.AddWithValue("kitchen_name_f", positem.Kitchen_name_f);
                                cmd.Parameters.AddWithValue("kitchen_remark", positem.Kitchen_remark);
                                cmd.Parameters.AddWithValue("dept", positem.Dept);
                                cmd.Parameters.AddWithValue("buttonid", positem.Buttonid);
                                cmd.Parameters.AddWithValue("sortorder", positem.Disp_order);
                                cmd.Parameters.AddWithValue("p1", positem.P1);
                                cmd.Parameters.AddWithValue("p2", positem.P2);
                                cmd.Parameters.AddWithValue("p3", positem.P3);
                                cmd.Parameters.AddWithValue("p4", positem.P4);
                                cmd.Parameters.AddWithValue("p5", positem.P5);
                                cmd.Parameters.AddWithValue("p6", positem.P6);
                                cmd.Parameters.AddWithValue("m1", positem.M1);
                                cmd.Parameters.AddWithValue("m2", positem.M2);
                                cmd.Parameters.AddWithValue("m3", positem.M3);
                                cmd.Parameters.AddWithValue("s1", positem.S1);
                                cmd.Parameters.AddWithValue("s2", positem.S2);
                                cmd.Parameters.AddWithValue("s3", positem.S3);
                                cmd.Parameters.AddWithValue("s4", positem.S4);
                                cmd.Parameters.AddWithValue("s5", positem.S5);
                                cmd.Parameters.AddWithValue("s6", positem.S6);
                                cmd.Parameters.AddWithValue("s7", positem.S7);
                                cmd.Parameters.AddWithValue("s8", positem.S8);
                                cmd.Parameters.AddWithValue("s9", positem.S9);
                                cmd.Parameters.AddWithValue("spicy", positem.Spicy);
                                cmd.Parameters.AddWithValue("vegetarian", positem.Vegetarian);
                                cmd.Parameters.AddWithValue("printer_name", positem.Printer_name);
                                cmd.Parameters.AddWithValue("kds_name", positem.Kds_name);
                                cmd.Parameters.AddWithValue("image", positem.Imagefile);
                                cmd.Parameters.AddWithValue("rest_usr", positem.Rest_usr);

                                if (positem.Upd_date.Equals(""))
                                    cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(positem.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                cmd.Parameters.AddWithValue("del_flag", positem.Del_flag);
                                cmd.ExecuteNonQuery();

                            }
                        }

                        trans.Commit();
                        trans.Dispose();
                        App.log.Info(" psitem:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }

                    psec = "6.1";
                    try
                    {
                        cmd.CommandText = @"select * from psitem  where  customerid=@customerid";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        cmd.ExecuteNonQuery();
                        TimeTicks = DateTime.Now.Ticks;
                        DataTable psitemDT2 = new DataTable();
                        da.Fill(psitemDT2);
                        client = new HttpClient();
                        SQLiteTransaction trans = cn.BeginTransaction();

                        foreach (DataRow row in psitemDT2.Rows)
                        {
                            psec = "6.2";
                            bool downloadFile = false;
                            FileInfo finfo;
                            if (!row["image"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["image"].ToString());
                                if (row["img_flag"].ToString().Equals("N")) downloadFile = true;
                                psec = "6.3:" + imgfilename;

                                if (downloadFile)
                                {
                                    psec = "6.4:" + imgfilename;
                                    string URL = posSetting.HostURL + "items/" + posSetting.CustomerID + "/" + row["image"].ToString();
                                    //PRPosUtils.writelog(URL);
                                    var retresult = await ReadURL(URL, imgfilename);

                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    psec = "6.5:" + imgfilename;
                                    cmd.CommandText = @"update psitem set img_flag='Y',ftime=@ftime   where item_code=@item_code and customerid=@customerid";
                                    cmd.Parameters.Clear();
                                    psec = "6.6:" + imgfilename;
                                    cmd.Parameters.AddWithValue("item_code", row["item_code"].ToString());
                                    cmd.Parameters.AddWithValue("customerid", row["customerid"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString(PRPosUtils.DateFormat + " " + PRPosUtils.TimeFormat)));
                                    psec = "6.7:" + imgfilename;
                                    //PRPosUtils.writelog(psec + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }

                        trans.Commit();
                        trans.Dispose();
                        App.log.Info(" psitem image:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " CheckDownload:" + errhttp.Message);
                    }

                    psec = "6.20";
                    cmd.CommandText = @"select *  from mealset where ifnull(customerid,'XX')<>@customerid  ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);

                    DataTable mealsetDT = new DataTable();
                    da.Fill(mealsetDT);

                    foreach (DataRow mealset in mealsetDT.Rows)
                    {
                        cmd.CommandText = @"select *  from mealset_course where psid=@sid ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("sid", mealset["sid"].ToString());
                        DataTable mealset_courseDT = new DataTable();
                        da.Fill(mealset_courseDT);
                        foreach (DataRow mealset_course in mealset_courseDT.Rows)
                        {
                            cmd.CommandText = @"delete from mealset_course_item where psid=@sid ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", mealset_course["sid"].ToString());
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = @"delete from mealset_course where sid=@sid ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", mealset_course["sid"].ToString());
                            cmd.ExecuteNonQuery();

                        }

                        cmd.CommandText = @"delete from mealset where sid=@sid ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("sid", mealset["sid"].ToString());
                        cmd.ExecuteNonQuery();

                    }
                    psec = "6.30";

                    if (resultPosData.MealSetValue != "")
                    {
                        TimeTicks = DateTime.Now.Ticks;
                        psec = "6.31";
                        List<MealSet> MealSetting = JsonConvert.DeserializeObject<List<MealSet>>(resultPosData.MealSetValue);

                        foreach (MealSet mealset in MealSetting)
                        {
                            if (mealset.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete from mealset where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", mealset.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                DateTime upd_date = DateTime.ParseExact(mealset.Upd_date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                                cmd.CommandText = @"select *  from mealset where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", mealset.Sid);
                                mealsetDT = new DataTable();
                                da.Fill(mealsetDT);

                                if (mealsetDT.Rows.Count > 0)
                                {
                                    DateTime sysupd_date = DateTime.Now;
                                    if (!mealsetDT.Rows[0]["upd_date"].ToString().Equals(""))
                                        DateTime.TryParse(mealsetDT.Rows[0]["upd_date"].ToString(), out sysupd_date);
                                    if (sysupd_date != upd_date)
                                    {
                                        cmd.CommandText =
                                             @"update mealset  set
                                               customerid=@customerid, store_code=@store_code, mealset_code=@mealset_code, caption=@caption, caption_fn=@caption_fn, 
                                               actived=@actived, del_flag=@del_flag, imagefile=@imagefile,  upd_date=@upd_date,img_flag='N', description =@description,gst=@gst,
                                               kitchen_name=@kitchen_name,kitchen_name_fn=@kitchen_name_fn,print_on_kitchen=@print_on_kitchen                                       
                                               where sid=@sid ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", mealset.Sid);
                                        cmd.Parameters.AddWithValue("customerid", mealset.Customerid);
                                        cmd.Parameters.AddWithValue("store_code", mealset.Store_code);
                                        cmd.Parameters.AddWithValue("mealset_code", mealset.Mealset_code);
                                        cmd.Parameters.AddWithValue("caption", mealset.Caption);
                                        cmd.Parameters.AddWithValue("caption_fn", mealset.Caption_fn);
                                        cmd.Parameters.AddWithValue("description", mealset.Description);
                                        cmd.Parameters.AddWithValue("actived", mealset.Actived);
                                        cmd.Parameters.AddWithValue("del_flag", mealset.Del_flag);
                                        cmd.Parameters.AddWithValue("imagefile", mealset.Image);

                                        if (mealset.Upd_date.Equals(""))
                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                        else
                                            cmd.Parameters.AddWithValue("upd_date", upd_date);
                                        cmd.Parameters.AddWithValue("kitchen_name", mealset.Kitchen_name);
                                        cmd.Parameters.AddWithValue("kitchen_name_fn", mealset.Kitchen_name_fn);
                                        cmd.Parameters.AddWithValue("print_on_kitchen", mealset.Print_on_kitchen);
                                        cmd.Parameters.AddWithValue("gst", mealset.Gst);
                                        cmd.ExecuteNonQuery();

                                    }
                                }
                                else
                                {
                                    cmd.CommandText =
                                       @"insert into mealset 
                                       (sid, customerid, store_code, mealset_code, caption, caption_fn, description,gst,
                                        actived, del_flag, imagefile,  upd_date,img_flag ,  kitchen_name,kitchen_name_fn,print_on_kitchen) 
                                       values 
                                       (@sid, @customerid, @store_code, @mealset_code, @caption, @caption_fn, @description,@gst,
                                        @actived, @del_flag, @imagefile,  @upd_date,'N' , @kitchen_name ,@kitchen_name_fn , @print_on_kitchen ) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", mealset.Sid);
                                    cmd.Parameters.AddWithValue("customerid", mealset.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", mealset.Store_code);
                                    cmd.Parameters.AddWithValue("mealset_code", mealset.Mealset_code);
                                    cmd.Parameters.AddWithValue("caption", mealset.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", mealset.Caption_fn);
                                    cmd.Parameters.AddWithValue("description", mealset.Description);
                                    cmd.Parameters.AddWithValue("actived", mealset.Actived);
                                    cmd.Parameters.AddWithValue("del_flag", mealset.Del_flag);
                                    cmd.Parameters.AddWithValue("imagefile", mealset.Image);

                                    if (mealset.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mealset.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                    cmd.Parameters.AddWithValue("kitchen_name", mealset.Kitchen_name);
                                    cmd.Parameters.AddWithValue("kitchen_name_fn", mealset.Kitchen_name_fn);
                                    cmd.Parameters.AddWithValue("print_on_kitchen", mealset.Print_on_kitchen);
                                    cmd.Parameters.AddWithValue("gst", mealset.Gst);
                                    cmd.ExecuteNonQuery();

                                }

                                foreach (MealSet_Course mealset_course in mealset.MealSet_Course)
                                {
                                    if (mealset_course.Del_flag.Equals("Y"))
                                    {
                                        cmd.CommandText = @"delete from mealset_course where sid=@sid ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", mealset_course.Sid);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"select *  from mealset_course where sid=@sid ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", mealset_course.Sid);
                                        DataTable mealset_courseDT = new DataTable();
                                        da.Fill(mealset_courseDT);
                                        if (mealset_courseDT.Rows.Count > 0)
                                        {
                                            cmd.CommandText =
                                            @"update mealset_course  set psid=@psid,
                                               course_name=@course_name, course_name_fn=@course_name_fn, max_selection=@max_selection,min_selection=@min_selection,
                                               sprice=@sprice, sprice2=@sprice2, sprice3=@sprice3, sprice4=@sprice4, sprice5=@sprice5, sprice6=@sprice6, 
                                               sprice7=@sprice7, sprice8=@sprice8, sprice9=@sprice9, sprice10=@sprice10,
                                               is_enabled=@is_enabled, del_flag=@del_flag, upd_date=@upd_date,disp_order=@disp_order
                                               where sid=@sid ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("sid", mealset_course.Sid);
                                            cmd.Parameters.AddWithValue("max_selection", mealset_course.Max_selection);
                                            cmd.Parameters.AddWithValue("min_selection", mealset_course.Min_selection);
                                            cmd.Parameters.AddWithValue("psid", mealset_course.Psid);
                                            cmd.Parameters.AddWithValue("course_name", mealset_course.Course_name);
                                            cmd.Parameters.AddWithValue("course_name_fn", mealset_course.Course_name_fn);
                                            cmd.Parameters.AddWithValue("sprice", mealset_course.Sprice1);
                                            cmd.Parameters.AddWithValue("sprice2", mealset_course.Sprice2);
                                            cmd.Parameters.AddWithValue("sprice3", mealset_course.Sprice3);
                                            cmd.Parameters.AddWithValue("sprice4", mealset_course.Sprice4);
                                            cmd.Parameters.AddWithValue("sprice5", mealset_course.Sprice5);
                                            cmd.Parameters.AddWithValue("sprice6", mealset_course.Sprice6);
                                            cmd.Parameters.AddWithValue("sprice7", mealset_course.Sprice7);
                                            cmd.Parameters.AddWithValue("sprice8", mealset_course.Sprice8);
                                            cmd.Parameters.AddWithValue("sprice9", mealset_course.Sprice9);
                                            cmd.Parameters.AddWithValue("sprice10", mealset_course.Sprice10);
                                            cmd.Parameters.AddWithValue("is_enabled", mealset_course.Is_enabled);
                                            cmd.Parameters.AddWithValue("del_flag", mealset_course.Del_flag);
                                            cmd.Parameters.AddWithValue("disp_order", mealset_course.Disp_order);
                                            if (mealset_course.Upd_date.Equals(""))
                                                cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                            else
                                                cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mealset_course.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                            cmd.ExecuteNonQuery();

                                        }
                                        else
                                        {
                                            cmd.CommandText =
                                                @"insert into mealset_course
                                                  (sid, psid,  course_name , course_name_fn,  sprice, sprice2, sprice3, sprice4, sprice5, sprice6, disp_order,
                                                   sprice7, sprice8, sprice9, sprice10, max_selection, min_selection, is_enabled, del_flag,   upd_date) 
                                                  values 
                                                 (@sid, @psid,  @course_name, @course_name_fn, @sprice, @sprice2, @sprice3, @sprice4, @sprice5, @sprice6, @disp_order,
                                                  @sprice7, @sprice8, @sprice9, @sprice10,@max_selection, @min_selection, @is_enabled, @del_flag,  @upd_date) ";
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("sid", mealset_course.Sid);
                                            cmd.Parameters.AddWithValue("psid", mealset_course.Psid);
                                            cmd.Parameters.AddWithValue("course_name", mealset_course.Course_name);
                                            cmd.Parameters.AddWithValue("course_name_fn", mealset_course.Course_name_fn);
                                            cmd.Parameters.AddWithValue("sprice", mealset_course.Sprice1);
                                            cmd.Parameters.AddWithValue("sprice2", mealset_course.Sprice2);
                                            cmd.Parameters.AddWithValue("sprice3", mealset_course.Sprice3);
                                            cmd.Parameters.AddWithValue("sprice4", mealset_course.Sprice4);
                                            cmd.Parameters.AddWithValue("sprice5", mealset_course.Sprice5);
                                            cmd.Parameters.AddWithValue("sprice6", mealset_course.Sprice6);
                                            cmd.Parameters.AddWithValue("sprice7", mealset_course.Sprice7);
                                            cmd.Parameters.AddWithValue("sprice8", mealset_course.Sprice8);
                                            cmd.Parameters.AddWithValue("sprice9", mealset_course.Sprice9);
                                            cmd.Parameters.AddWithValue("sprice10", mealset_course.Sprice10);
                                            cmd.Parameters.AddWithValue("max_selection", mealset_course.Max_selection);
                                            cmd.Parameters.AddWithValue("min_selection", mealset_course.Min_selection);
                                            cmd.Parameters.AddWithValue("is_enabled", mealset_course.Is_enabled);
                                            cmd.Parameters.AddWithValue("disp_order", mealset_course.Disp_order);
                                            cmd.Parameters.AddWithValue("del_flag", mealset_course.Del_flag);
                                            if (mealset_course.Upd_date.Equals(""))
                                                cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                            else
                                                cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mealset_course.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                            cmd.ExecuteNonQuery();
                                            foreach (MealSet_Course_Item mealset_course_item in mealset_course.MealSet_Course_Item)
                                            {
                                                if (mealset_course_item.Del_flag.Equals("Y"))
                                                {
                                                    cmd.CommandText = @"delete from mealset_course_item where sid=@sid ";
                                                    cmd.Parameters.Clear();
                                                    cmd.Parameters.AddWithValue("sid", mealset_course_item.Sid);
                                                    cmd.ExecuteNonQuery();
                                                }
                                                else
                                                {
                                                    cmd.CommandText = @"select *  from mealset_course_item where sid=@sid ";
                                                    cmd.Parameters.Clear();
                                                    cmd.Parameters.AddWithValue("sid", mealset_course_item.Sid);
                                                    DataTable mealset_course_itemDT = new DataTable();
                                                    da.Fill(mealset_course_itemDT);
                                                    if (mealset_course_itemDT.Rows.Count > 0)
                                                    {
                                                        cmd.CommandText =
                                                          @"update mealset_course_item  set psid=@psid,disp_order=@disp_order,variety_code=@variety_code,
                                                               psid=@psid, item_code=@item_code,sprice=@sprice, sprice2=@sprice2, sprice3=@sprice3, sprice4=@sprice4, sprice5=@sprice5, sprice6=@sprice6, 
                                                                sprice7=@sprice7, sprice8=@sprice8, sprice9=@sprice9, sprice10=@sprice10,is_enabled=@is_enabled, del_flag=@del_flag, upd_date=@upd_date
                                                               where sid=@sid ";
                                                        cmd.Parameters.Clear();
                                                        cmd.Parameters.AddWithValue("sid", mealset_course_item.Sid);

                                                        cmd.Parameters.AddWithValue("psid", mealset_course_item.Psid);
                                                        cmd.Parameters.AddWithValue("variety_code", mealset_course_item.Variety_code);
                                                        cmd.Parameters.AddWithValue("item_code", mealset_course_item.Item_code);
                                                        cmd.Parameters.AddWithValue("sprice", mealset_course_item.Sprice1);
                                                        cmd.Parameters.AddWithValue("sprice2", mealset_course_item.Sprice2);
                                                        cmd.Parameters.AddWithValue("sprice3", mealset_course_item.Sprice3);
                                                        cmd.Parameters.AddWithValue("sprice4", mealset_course_item.Sprice4);
                                                        cmd.Parameters.AddWithValue("sprice5", mealset_course_item.Sprice5);
                                                        cmd.Parameters.AddWithValue("sprice6", mealset_course_item.Sprice6);
                                                        cmd.Parameters.AddWithValue("sprice7", mealset_course_item.Sprice7);
                                                        cmd.Parameters.AddWithValue("sprice8", mealset_course_item.Sprice8);
                                                        cmd.Parameters.AddWithValue("sprice9", mealset_course_item.Sprice9);
                                                        cmd.Parameters.AddWithValue("sprice10", mealset_course_item.Sprice10);
                                                        cmd.Parameters.AddWithValue("is_enabled", mealset_course_item.Is_enabled);
                                                        cmd.Parameters.AddWithValue("disp_order", mealset_course_item.Disp_order);
                                                        cmd.Parameters.AddWithValue("del_flag", mealset_course_item.Del_flag);
                                                        if (mealset_course_item.Upd_date.Equals(""))
                                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                                        else
                                                            cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mealset_course_item.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                                        cmd.ExecuteNonQuery();

                                                    }
                                                    else
                                                    {
                                                        cmd.CommandText =
                                                        @"insert into mealset_course_item 
                                                               (sid, psid, variety_code,item_code, sprice, sprice2, sprice3, sprice4, sprice5, sprice6, sprice7, sprice8, sprice9, sprice10, is_enabled, del_flag, upd_date, disp_order)
                                                          values
                                                              (@sid, @psid,@variety_code, @item_code, @sprice, @sprice2, @sprice3, @sprice4, @sprice5, @sprice6, @sprice7, @sprice8, @sprice9, @sprice10, @is_enabled, @del_flag, @upd_date,@disp_order) ";
                                                        cmd.Parameters.Clear();
                                                        cmd.Parameters.AddWithValue("sid", mealset_course_item.Sid);
                                                        cmd.Parameters.AddWithValue("psid", mealset_course_item.Psid);
                                                        cmd.Parameters.AddWithValue("variety_code", mealset_course_item.Variety_code);
                                                        cmd.Parameters.AddWithValue("item_code", mealset_course_item.Item_code);
                                                        cmd.Parameters.AddWithValue("sprice", mealset_course_item.Sprice1);
                                                        cmd.Parameters.AddWithValue("sprice2", mealset_course_item.Sprice2);
                                                        cmd.Parameters.AddWithValue("sprice3", mealset_course_item.Sprice3);
                                                        cmd.Parameters.AddWithValue("sprice4", mealset_course_item.Sprice4);
                                                        cmd.Parameters.AddWithValue("sprice5", mealset_course_item.Sprice5);
                                                        cmd.Parameters.AddWithValue("sprice6", mealset_course_item.Sprice6);
                                                        cmd.Parameters.AddWithValue("sprice7", mealset_course_item.Sprice7);
                                                        cmd.Parameters.AddWithValue("sprice8", mealset_course_item.Sprice8);
                                                        cmd.Parameters.AddWithValue("sprice9", mealset_course_item.Sprice9);
                                                        cmd.Parameters.AddWithValue("sprice10", mealset_course_item.Sprice10);
                                                        cmd.Parameters.AddWithValue("is_enabled", mealset_course_item.Is_enabled);
                                                        cmd.Parameters.AddWithValue("del_flag", mealset_course_item.Del_flag);
                                                        cmd.Parameters.AddWithValue("disp_order", mealset_course_item.Disp_order);
                                                        if (mealset_course_item.Upd_date.Equals(""))
                                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                                        else
                                                            cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mealset_course_item.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                                        cmd.ExecuteNonQuery();

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        App.log.Info(" itemcombo:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }

                    psec = "6.40";
                    //check images files
                    try
                    {
                        cmd.CommandText = @"select * from mealset  where   customerid=@customerid  ";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        cmd.ExecuteNonQuery();

                        DataTable meal_setDT = new DataTable();
                        da.Fill(meal_setDT);
                        client = new HttpClient();

                        foreach (DataRow row in meal_setDT.Rows)
                        {
                            psec = "6.42";
                            bool downloadFile = false;
                            FileInfo finfo;
                            if (!row["imagefile"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["imagefile"].ToString());
                                if (row["img_flag"].ToString().Equals("N")) downloadFile = true;

                                if (downloadFile)
                                {
                                    psec = "6.45:" + imgfilename;
                                    string URL = posSetting.HostURL + "mealset_pic/" + posSetting.CustomerID + "/" + row["imagefile"].ToString();
                                    var retresult = await ReadURL(URL, imgfilename);
                                    //PRPosUtils.writelog(URL);

                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    psec = "6.46:" + imgfilename;
                                    cmd.CommandText = @"update mealset set img_flag='Y',ftime=@ftime   where sid=@sid ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", row["sid"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")));
                                    //PRPosUtils.writelog("mealset_pic" + finfo2.FullName + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    psec = "6.47:" + imgfilename;
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " CheckDownload:" + errhttp.Message);
                    }

                    psec = "6.50";
                    cmd.CommandText = @"delete from itemcombo where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.ComboValue != "")
                    {
                        psec = "6.51";
                        List<Combo> AllCombos = JsonConvert.DeserializeObject<List<Combo>>(resultPosData.ComboValue);

                        foreach (Combo combo in AllCombos)
                        {
                            if (combo.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete from itemcombo where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", combo.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText = @"select * from itemcombo where sid=@sid and customerid=@customerid";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", combo.Sid);
                                cmd.Parameters.AddWithValue("customerid", combo.Customerid);
                                DataTable itemcomboDT = new DataTable();
                                da.Fill(itemcomboDT);
                                if (itemcomboDT.Rows.Count > 0)
                                {
                                    cmd.CommandText =
                                         @"update itemcombo  set  
                                               customerid=@customerid, store_code=@store_code, item_code=@item_code,variety_code=@variety_code,mealset_code=@mealset_code,gst=@gst,
                                               sprice=@sprice, sprice2=@sprice2, sprice3=@sprice3, sprice4=@sprice4, sprice5=@sprice5, sprice6=@sprice6,  sprice7=@sprice7, sprice8=@sprice8,
                                               sprice9=@sprice9, sprice10=@sprice10,description=@description, disp_order=@disp_order,del_flag=@del_flag, upd_date=@upd_date
                                               where sid=@sid ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", combo.Sid);
                                    cmd.Parameters.AddWithValue("customerid", combo.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", combo.Store_code);
                                    cmd.Parameters.AddWithValue("item_code", combo.Item_code);
                                    cmd.Parameters.AddWithValue("variety_code", combo.Variety_code);
                                    cmd.Parameters.AddWithValue("mealset_code", combo.Mealset_code);
                                    cmd.Parameters.AddWithValue("sprice", combo.Sprice1);
                                    cmd.Parameters.AddWithValue("sprice2", combo.Sprice2);
                                    cmd.Parameters.AddWithValue("sprice3", combo.Sprice3);
                                    cmd.Parameters.AddWithValue("sprice4", combo.Sprice4);
                                    cmd.Parameters.AddWithValue("sprice5", combo.Sprice5);
                                    cmd.Parameters.AddWithValue("sprice6", combo.Sprice6);
                                    cmd.Parameters.AddWithValue("sprice7", combo.Sprice7);
                                    cmd.Parameters.AddWithValue("sprice8", combo.Sprice8);
                                    cmd.Parameters.AddWithValue("sprice9", combo.Sprice9);
                                    cmd.Parameters.AddWithValue("sprice10", combo.Sprice10);
                                    cmd.Parameters.AddWithValue("disp_order", combo.Disp_order);
                                    cmd.Parameters.AddWithValue("description", combo.Description);
                                    cmd.Parameters.AddWithValue("del_flag", combo.Del_flag);
                                    cmd.Parameters.AddWithValue("gst", combo.Gst);
                                    if (combo.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(combo.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.ExecuteNonQuery();

                                }
                                else
                                {
                                    cmd.CommandText =
                                       @"insert into itemcombo 
                                           (sid,customerid,store_code,item_code,variety_code,mealset_code,description,gst,
                                            sprice,sprice2,sprice3,sprice4,sprice5,sprice6,sprice7,sprice8,sprice9,sprice10,disp_order,del_flag,upd_date) 
                                         values  
                                           (@sid,@customerid,@store_code,@item_code,@variety_code,@mealset_code,@description,@gst,
                                            @sprice,@sprice2,@sprice3,@sprice4,@sprice5,@sprice6,@sprice7,@sprice8,@sprice9,@sprice10,@disp_order,@del_flag,@upd_date) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", combo.Sid);
                                    cmd.Parameters.AddWithValue("customerid", combo.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", combo.Store_code);
                                    cmd.Parameters.AddWithValue("item_code", combo.Item_code);
                                    cmd.Parameters.AddWithValue("variety_code", combo.Variety_code);
                                    cmd.Parameters.AddWithValue("mealset_code", combo.Mealset_code);
                                    cmd.Parameters.AddWithValue("sprice", combo.Sprice1);
                                    cmd.Parameters.AddWithValue("sprice2", combo.Sprice2);
                                    cmd.Parameters.AddWithValue("sprice3", combo.Sprice3);
                                    cmd.Parameters.AddWithValue("sprice4", combo.Sprice4);
                                    cmd.Parameters.AddWithValue("sprice5", combo.Sprice5);
                                    cmd.Parameters.AddWithValue("sprice6", combo.Sprice6);
                                    cmd.Parameters.AddWithValue("sprice7", combo.Sprice7);
                                    cmd.Parameters.AddWithValue("sprice8", combo.Sprice8);
                                    cmd.Parameters.AddWithValue("sprice9", combo.Sprice9);
                                    cmd.Parameters.AddWithValue("sprice10", combo.Sprice10);
                                    cmd.Parameters.AddWithValue("disp_order", combo.Disp_order);
                                    cmd.Parameters.AddWithValue("description", combo.Description);
                                    cmd.Parameters.AddWithValue("del_flag", combo.Del_flag);
                                    cmd.Parameters.AddWithValue("gst", combo.Gst);
                                    if (combo.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(combo.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }

                    }
                    
                    psec = "7.0";
                    cmd.CommandText = @"delete from PosPrinter where ifnull(customerid,'XX')<>@customerid  ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);                    
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"delete from PosPrinter where  store_code<>@store_code ";
                    cmd.Parameters.Clear();                    
                    cmd.Parameters.AddWithValue("store_code", posSetting.StoreCode);
                    cmd.ExecuteNonQuery();

                    Debug.WriteLine(" PrinterSettingValue " + resultPosData.PrinterSettingValue);
                    if (resultPosData.PrinterSettingValue != "")
                    {
                        psec = "7.1";
                        List<PrinterSetting> PrinterSettings = JsonConvert.DeserializeObject<List<PrinterSetting>>(resultPosData.PrinterSettingValue);
                        //                        

                        foreach (PrinterSetting psetting in PrinterSettings)
                        {
                            cmd.CommandText = @"select * from PosPrinter where sid=@sid and customerid=@customerid and store_code=@store_code";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", psetting.Sid);
                            cmd.Parameters.AddWithValue("customerid", psetting.Customerid);
                            cmd.Parameters.AddWithValue("store_code", psetting.Store_code);
                            DataTable psettingDT = new DataTable();
                            da.Fill(psettingDT);
                            if (psettingDT.Rows.Count > 0)
                            {
                                psec = "7.2";
                                cmd.CommandText = @"update PosPrinter set 
                                                        store_code=@store_code,pos_code=@pos_code,
                                                        device_type=@device_type,config_type=@config_type,port=@port,buad=@buad,
                                                        dataformat=@dataformat,handshake=@handshake,device_name=@device_name,
                                                        kitchen_printer=@kitchen_printer,ipaddress=@ipaddress,del_flag=@del_flag
                                                       where sid=@sid and customerid=@customerid ";
                                cmd.Parameters.Clear();

                                cmd.Parameters.AddWithValue("customerid", psetting.Customerid);
                                cmd.Parameters.AddWithValue("store_code", psetting.Store_code);
                                cmd.Parameters.AddWithValue("pos_code", psetting.Pos_code);
                                cmd.Parameters.AddWithValue("device_type", psetting.Device_type);
                                cmd.Parameters.AddWithValue("config_type", psetting.Config_type);
                                cmd.Parameters.AddWithValue("port", psetting.Port);
                                cmd.Parameters.AddWithValue("buad", psetting.Buad);
                                cmd.Parameters.AddWithValue("dataformat", psetting.Data_format);
                                cmd.Parameters.AddWithValue("handshake", psetting.Handshake);
                                cmd.Parameters.AddWithValue("device_name", psetting.Device_name);
                                cmd.Parameters.AddWithValue("kitchen_printer", psetting.Map_printer);
                                cmd.Parameters.AddWithValue("ipaddress", psetting.Ip_address);
                                cmd.Parameters.AddWithValue("del_flag", psetting.Del_flag);
                                cmd.Parameters.AddWithValue("sid", psetting.Sid);
                                cmd.ExecuteNonQuery();

                            }
                            else
                            {
                                psec = "7.3";
                                cmd.CommandText = @"insert into PosPrinter
                                                    (sid, customerid,store_code,pos_code,device_type,config_type,port,buad,
                                                     dataformat,handshake,device_name, kitchen_printer,ipaddress,del_flag)
                                                     values 
                                                     (@sid, @customerid,@store_code,@pos_code,@device_type,@config_type,@port,@buad,
                                                     @dataformat,@handshake,@device_name, @kitchen_printer,@ipaddress,@del_flag)";
                                cmd.Parameters.Clear();

                                cmd.Parameters.AddWithValue("customerid", psetting.Customerid);
                                cmd.Parameters.AddWithValue("store_code", psetting.Store_code);
                                cmd.Parameters.AddWithValue("pos_code", psetting.Pos_code);
                                cmd.Parameters.AddWithValue("device_type", psetting.Device_type);
                                cmd.Parameters.AddWithValue("config_type", psetting.Config_type);
                                cmd.Parameters.AddWithValue("port", psetting.Port);
                                cmd.Parameters.AddWithValue("buad", psetting.Buad);
                                cmd.Parameters.AddWithValue("dataformat", psetting.Data_format);
                                cmd.Parameters.AddWithValue("handshake", psetting.Handshake);
                                cmd.Parameters.AddWithValue("device_name", psetting.Device_name);
                                cmd.Parameters.AddWithValue("kitchen_printer", psetting.Map_printer);
                                cmd.Parameters.AddWithValue("ipaddress", psetting.Ip_address);
                                cmd.Parameters.AddWithValue("del_flag", psetting.Del_flag);
                                cmd.Parameters.AddWithValue("sid", psetting.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            
                        }
                    }
                    
                    psec = "7.20";
                    cmd.CommandText = @"delete from tender where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.PaymentValue != "")
                    {
                        psec = "7.21";
                        List<PaymentMethod> PosPaymenList = JsonConvert.DeserializeObject<List<PaymentMethod>>(resultPosData.PaymentValue);


                        foreach (PaymentMethod paymentMethod in PosPaymenList)
                        {
                            if (paymentMethod.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete  from tender where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", paymentMethod.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText = @"select * from tender where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", paymentMethod.Sid);
                                cmd.Parameters.AddWithValue("customerid", paymentMethod.Customerid);
                                DataTable paymentMethodDT = new DataTable();
                                da.Fill(paymentMethodDT);
                                if (paymentMethodDT.Rows.Count > 0)
                                {
                                    psec = "7.22";
                                    cmd.CommandText =
                                          @"update tender set 
                                       customerid=@customerid, store_code=@store_code, tender_code=@tender_code, tender_name=@tender_name,
                                       display_name=@display_name, over_flag=@over_flag, over_max=@over_max, eftpos_flag=@eftpos_flag, paymachine_flag=@paymachine_flag,
                                       received_flag=@received_flag, change_flag=@change_flag,
                                       disp_flag=@disp_flag, disp_order=@disp_order,del_flag=@del_flag, print_at_kitchen=@print_at_kitchen,
                                       card_charge_item=@card_charge_item, card_charge_rate=@card_charge_rate,card_charge_flag =@card_charge_flag 
                                      where sid=@sid";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", paymentMethod.Sid);
                                    cmd.Parameters.AddWithValue("customerid", paymentMethod.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", paymentMethod.Store_code);
                                    cmd.Parameters.AddWithValue("tender_code", paymentMethod.Tender_code);
                                    cmd.Parameters.AddWithValue("tender_name", paymentMethod.Tender_name);
                                    cmd.Parameters.AddWithValue("display_name", paymentMethod.Display_name);
                                    cmd.Parameters.AddWithValue("over_flag", paymentMethod.Over_flag);
                                    cmd.Parameters.AddWithValue("over_max", paymentMethod.Over_max);
                                    cmd.Parameters.AddWithValue("eftpos_flag", paymentMethod.Eftpos_flag);
                                    cmd.Parameters.AddWithValue("paymachine_flag", paymentMethod.Paymachine_flag);
                                    cmd.Parameters.AddWithValue("received_flag", paymentMethod.Received_flag);
                                    cmd.Parameters.AddWithValue("change_flag", paymentMethod.Change_flag);
                                    cmd.Parameters.AddWithValue("disp_flag", paymentMethod.Disp_flag);
                                    cmd.Parameters.AddWithValue("disp_order", paymentMethod.Disp_order);


                                    cmd.Parameters.AddWithValue("del_flag", paymentMethod.Del_flag);
                                    cmd.Parameters.AddWithValue("card_charge_item", paymentMethod.Card_charge_item);
                                    cmd.Parameters.AddWithValue("card_charge_rate", paymentMethod.Card_charge_rate);
                                    cmd.Parameters.AddWithValue("card_charge_flag", paymentMethod.Card_charge_flag);
                                    cmd.Parameters.AddWithValue("print_at_kitchen", paymentMethod.Print_at_kitchen);

                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    psec = "7.23";
                                    cmd.CommandText =
                                        @"insert into tender 
                                      ( sid,customerid, store_code, tender_code, tender_name, display_name, over_flag, over_max, eftpos_flag, paymachine_flag,received_flag, change_flag,
                                        disp_flag, disp_order,del_flag, card_charge_item,card_charge_rate,card_charge_flag ,print_at_kitchen )
                                      values 
                                      ( @sid,@customerid,@store_code,  @tender_code, @tender_name, @display_name, @over_flag,@over_max, @eftpos_flag, @paymachine_flag, @received_flag, @change_flag,
                                        @disp_flag, @disp_order,@del_flag,@card_charge_item,@card_charge_rate,@card_charge_flag ,@print_at_kitchen)";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", paymentMethod.Sid);
                                    cmd.Parameters.AddWithValue("customerid", paymentMethod.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", paymentMethod.Store_code);
                                    cmd.Parameters.AddWithValue("tender_code", paymentMethod.Tender_code);
                                    cmd.Parameters.AddWithValue("tender_name", paymentMethod.Tender_name);
                                    cmd.Parameters.AddWithValue("display_name", paymentMethod.Display_name);
                                    cmd.Parameters.AddWithValue("over_flag", paymentMethod.Over_flag);
                                    cmd.Parameters.AddWithValue("over_max", paymentMethod.Over_max);
                                    cmd.Parameters.AddWithValue("eftpos_flag", paymentMethod.Eftpos_flag);
                                    cmd.Parameters.AddWithValue("paymachine_flag", paymentMethod.Paymachine_flag);
                                    cmd.Parameters.AddWithValue("received_flag", paymentMethod.Received_flag);
                                    cmd.Parameters.AddWithValue("change_flag", paymentMethod.Change_flag);
                                    cmd.Parameters.AddWithValue("disp_flag", paymentMethod.Disp_flag);
                                    cmd.Parameters.AddWithValue("disp_order", paymentMethod.Disp_order);


                                    cmd.Parameters.AddWithValue("del_flag", paymentMethod.Del_flag);
                                    cmd.Parameters.AddWithValue("card_charge_item", paymentMethod.Card_charge_item);
                                    cmd.Parameters.AddWithValue("card_charge_rate", paymentMethod.Card_charge_rate);
                                    cmd.Parameters.AddWithValue("card_charge_flag", paymentMethod.Card_charge_flag);
                                    cmd.Parameters.AddWithValue("print_at_kitchen", paymentMethod.Print_at_kitchen);
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }


                    }

                    psec = "7.30";
                    cmd.CommandText = @"delete from markupitem where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.MarkupItemValue != "")
                    {
                        psec = "7.31";
                        List<MakrupItem> MakrupItemList = JsonConvert.DeserializeObject<List<MakrupItem>>(resultPosData.MarkupItemValue);


                        foreach (MakrupItem makrupItem in MakrupItemList)
                        {
                            if (makrupItem.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete  from markupitem where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", makrupItem.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText = @"select * from markupitem where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", makrupItem.Sid);
                                cmd.Parameters.AddWithValue("customerid", makrupItem.Customerid);
                                DataTable markupitemDT = new DataTable();
                                da.Fill(markupitemDT);
                                if (markupitemDT.Rows.Count > 0)
                                {
                                    psec = "7.32";
                                    cmd.CommandText =
                                          @"update markupitem set 
                                               customerid=@customerid, store_code=@store_code, markup_type=@markup_type, message=@message,
                                               item_code=@item_code,variety_code=@variety_code,
                                               sprice=@sprice,sprice2=@sprice2 ,sprice3=@sprice3, sprice4=@sprice4,sprice5=@sprice5,
                                               sprice6=@sprice6,sprice7=@sprice7 ,sprice8=@sprice8, sprice9=@sprice9,sprice10=@sprice10,
                                               del_flag=@del_flag, upd_date=@upd_date ,disp_order=@disp_order
                                              where sid=@sid";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", makrupItem.Sid);
                                    cmd.Parameters.AddWithValue("customerid", makrupItem.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", makrupItem.Store_code);
                                    cmd.Parameters.AddWithValue("markup_type", makrupItem.Markup_type);
                                    cmd.Parameters.AddWithValue("item_code", makrupItem.Item_code);
                                    cmd.Parameters.AddWithValue("variety_code", makrupItem.Variety_code);
                                    cmd.Parameters.AddWithValue("message", makrupItem.Message);
                                    cmd.Parameters.AddWithValue("disp_order", makrupItem.Disp_order);
                                    cmd.Parameters.AddWithValue("sprice", makrupItem.Sprice1);
                                    cmd.Parameters.AddWithValue("sprice2", makrupItem.Sprice2);
                                    cmd.Parameters.AddWithValue("sprice3", makrupItem.Sprice3);
                                    cmd.Parameters.AddWithValue("sprice4", makrupItem.Sprice4);
                                    cmd.Parameters.AddWithValue("sprice5", makrupItem.Sprice5);
                                    cmd.Parameters.AddWithValue("sprice6", makrupItem.Sprice6);
                                    cmd.Parameters.AddWithValue("sprice7", makrupItem.Sprice7);
                                    cmd.Parameters.AddWithValue("sprice8", makrupItem.Sprice8);
                                    cmd.Parameters.AddWithValue("sprice9", makrupItem.Sprice9);
                                    cmd.Parameters.AddWithValue("sprice10", makrupItem.Sprice10);
                                    cmd.Parameters.AddWithValue("del_flag", makrupItem.Del_flag);
                                    if (makrupItem.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(makrupItem.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    psec = "7.33";
                                    cmd.CommandText =
                                        @"insert into markupitem 
                                         ( sid,customerid, store_code, markup_type, item_code, message, del_flag, disp_order, sprice, sprice2, sprice3,sprice4,sprice5,
                                           sprice6,sprice7,sprice8,sprice9,sprice10, del_flag, upd_date, variety_code  )
                                         values 
                                         ( @sid,@customerid, @store_code, @markup_type, @item_code, @message, @del_flag, @disp_order, @sprice, @sprice2, @sprice3,@sprice4,@sprice5,
                                           @sprice6,@sprice7,@sprice8,@sprice9,@sprice10, @del_flag, @upd_date, @variety_code  ) ";
                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("sid", makrupItem.Sid);
                                    cmd.Parameters.AddWithValue("customerid", makrupItem.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", makrupItem.Store_code);
                                    cmd.Parameters.AddWithValue("markup_type", makrupItem.Markup_type);
                                    cmd.Parameters.AddWithValue("item_code", makrupItem.Item_code);
                                    cmd.Parameters.AddWithValue("variety_code", makrupItem.Variety_code);
                                    cmd.Parameters.AddWithValue("message", makrupItem.Message);
                                    cmd.Parameters.AddWithValue("disp_order", makrupItem.Disp_order);
                                    cmd.Parameters.AddWithValue("sprice", makrupItem.Sprice1);
                                    cmd.Parameters.AddWithValue("sprice2", makrupItem.Sprice2);
                                    cmd.Parameters.AddWithValue("sprice3", makrupItem.Sprice3);
                                    cmd.Parameters.AddWithValue("sprice4", makrupItem.Sprice4);
                                    cmd.Parameters.AddWithValue("sprice5", makrupItem.Sprice5);
                                    cmd.Parameters.AddWithValue("sprice6", makrupItem.Sprice6);
                                    cmd.Parameters.AddWithValue("sprice7", makrupItem.Sprice7);
                                    cmd.Parameters.AddWithValue("sprice8", makrupItem.Sprice8);
                                    cmd.Parameters.AddWithValue("sprice9", makrupItem.Sprice9);
                                    cmd.Parameters.AddWithValue("sprice10", makrupItem.Sprice10);
                                    cmd.Parameters.AddWithValue("del_flag", makrupItem.Del_flag);
                                    if (makrupItem.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(makrupItem.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }

                    }

                    psec = "7.40";
                    cmd.CommandText = @"delete from servicecharge where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.ServiceChargeValue != "")
                    {
                        psec = "7.41";
                        List<ServiceCharge> ServiceChargeList = JsonConvert.DeserializeObject<List<ServiceCharge>>(resultPosData.ServiceChargeValue);

                        foreach (ServiceCharge serviceCharge in ServiceChargeList)
                        {
                            if (serviceCharge.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete  from servicecharge where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", serviceCharge.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText = @"select * from servicecharge where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", serviceCharge.Sid);
                                cmd.Parameters.AddWithValue("customerid", serviceCharge.Customerid);
                                DataTable servicechargeDT = new DataTable();
                                da.Fill(servicechargeDT);
                                if (servicechargeDT.Rows.Count > 0)
                                {
                                    psec = "7.42";
                                    cmd.CommandText =
                                          @"update servicecharge set 
                                               customerid=@customerid, store_code=@store_code, charge_type=@charge_type, charge_name=@charge_name,
                                               bdate=@bdate, edate=@edate, charge_flag=@charge_flag, charge_item=@charge_item, charge_rate=@charge_rate,
                                               del_flag=@del_flag, upd_date=@upd_date ,variety_code=@variety_code
                                              where sid=@sid";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", serviceCharge.Sid);
                                    cmd.Parameters.AddWithValue("customerid", serviceCharge.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", serviceCharge.Store_code);
                                    cmd.Parameters.AddWithValue("charge_type", serviceCharge.Charge_type);
                                    cmd.Parameters.AddWithValue("charge_name", serviceCharge.Charge_name);
                                    if (serviceCharge.Bdate.Equals(""))
                                        cmd.Parameters.AddWithValue("bdate", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("bdate", DateTime.Parse(serviceCharge.Bdate, CultureInfo.CreateSpecificCulture("en-AU")));

                                    if (serviceCharge.Bdate.Equals(""))
                                        cmd.Parameters.AddWithValue("edate", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("edate", DateTime.Parse(serviceCharge.Edate, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.Parameters.AddWithValue("charge_flag", serviceCharge.Charge_flag);
                                    cmd.Parameters.AddWithValue("charge_item", serviceCharge.Charge_item);
                                    cmd.Parameters.AddWithValue("variety_code", serviceCharge.Variety_code);
                                    cmd.Parameters.AddWithValue("charge_rate", serviceCharge.Charge_rate);
                                    cmd.Parameters.AddWithValue("del_flag", serviceCharge.Del_flag);
                                    if (serviceCharge.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(serviceCharge.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    psec = "7.43";
                                    cmd.CommandText =
                                        @"insert into servicecharge 
                                         ( sid,customerid, store_code, charge_type, charge_name, bdate, edate, charge_flag, charge_item,variety_code, charge_rate,del_flag, upd_date  )
                                         values 
                                         ( @sid,@customerid, @store_code, @charge_type, @charge_name, @bdate, @edate, @charge_flag, @charge_item,@variety_code, @charge_rate,@del_flag, @upd_date  ) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", serviceCharge.Sid);
                                    cmd.Parameters.AddWithValue("customerid", serviceCharge.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", serviceCharge.Store_code);
                                    cmd.Parameters.AddWithValue("charge_type", serviceCharge.Charge_type);
                                    cmd.Parameters.AddWithValue("charge_name", serviceCharge.Charge_name);
                                    if (serviceCharge.Bdate.Equals(""))
                                        cmd.Parameters.AddWithValue("bdate", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("bdate", DateTime.Parse(serviceCharge.Bdate, CultureInfo.CreateSpecificCulture("en-AU")));

                                    if (serviceCharge.Bdate.Equals(""))
                                        cmd.Parameters.AddWithValue("edate", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("edate", DateTime.Parse(serviceCharge.Edate, CultureInfo.CreateSpecificCulture("en-AU")));


                                    cmd.Parameters.AddWithValue("charge_flag", serviceCharge.Charge_flag);
                                    cmd.Parameters.AddWithValue("charge_item", serviceCharge.Charge_item);
                                    cmd.Parameters.AddWithValue("variety_code", serviceCharge.Variety_code);
                                    cmd.Parameters.AddWithValue("charge_rate", serviceCharge.Charge_rate);
                                    cmd.Parameters.AddWithValue("del_flag", serviceCharge.Del_flag);

                                    if (serviceCharge.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(serviceCharge.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }


                    }

                    psec = "7.50";
                    cmd.CommandText = @"delete from pscategory where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.PsCategoryValue != "")
                    {
                        psec = "7.41";
                        List<PsCategory> PsCategoryList = JsonConvert.DeserializeObject<List<PsCategory>>(resultPosData.PsCategoryValue);

                        foreach (PsCategory psCategory in PsCategoryList)
                        {
                            if (psCategory.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete  from pscategory where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", psCategory.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                cmd.CommandText = @"select * from pscategory where sid=@sid ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", psCategory.Sid);
                                cmd.Parameters.AddWithValue("customerid", psCategory.Customerid);
                                DataTable servicechargeDT = new DataTable();
                                da.Fill(servicechargeDT);
                                if (servicechargeDT.Rows.Count > 0)
                                {
                                    cmd.CommandText =
                                        @"update pscategory set 
                                               customerid=@customerid, cate_code=@cate_code, cate_type=@cate_type, cate_name=@cate_name,
                                                   cate_fn_name=@cate_fn_name,seq=@disp_order,buttonid=@buttonid,publish=@publish,pcate_code=@pcate_code,del_flag=@del_flag
                                              where sid=@sid";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", psCategory.Sid);
                                    cmd.Parameters.AddWithValue("customerid", psCategory.Customerid);
                                    cmd.Parameters.AddWithValue("cate_code", psCategory.Cate_code);
                                    cmd.Parameters.AddWithValue("cate_type", psCategory.Cate_type);
                                    cmd.Parameters.AddWithValue("cate_name", psCategory.Cate_name);
                                    cmd.Parameters.AddWithValue("cate_fn_name", psCategory.Cate_fn_name);
                                    cmd.Parameters.AddWithValue("disp_order", psCategory.Disp_order);
                                    cmd.Parameters.AddWithValue("buttonid", psCategory.Buttonid);
                                    cmd.Parameters.AddWithValue("publish", psCategory.Publish);
                                    cmd.Parameters.AddWithValue("pcate_code", psCategory.Pcate_code);
                                    cmd.Parameters.AddWithValue("del_flag", psCategory.Del_flag);
                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    cmd.CommandText =
                                      @"insert into  pscategory 
                                          (sid,customerid,cate_code,cate_type,cate_name,cate_fn_name,seq,buttonid,publish,pcate_code,del_flag)
                                         values  
                                          (@sid,@customerid,@cate_code,@cate_type,@cate_name,@cate_fn_name,@disp_order,@buttonid,@publish,@pcate_code,@del_flag) ";

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", psCategory.Sid);
                                    cmd.Parameters.AddWithValue("customerid", psCategory.Customerid);
                                    cmd.Parameters.AddWithValue("cate_code", psCategory.Cate_code);
                                    cmd.Parameters.AddWithValue("cate_type", psCategory.Cate_type);
                                    cmd.Parameters.AddWithValue("cate_name", psCategory.Cate_name);
                                    cmd.Parameters.AddWithValue("cate_fn_name", psCategory.Cate_fn_name);
                                    cmd.Parameters.AddWithValue("disp_order", psCategory.Disp_order);
                                    cmd.Parameters.AddWithValue("buttonid", psCategory.Buttonid);
                                    cmd.Parameters.AddWithValue("publish", psCategory.Publish);
                                    cmd.Parameters.AddWithValue("pcate_code", psCategory.Pcate_code);
                                    cmd.Parameters.AddWithValue("del_flag", psCategory.Del_flag);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                    }

                    psec = "8.1";
                    cmd.CommandText = @"delete from modifier where ifnull(customerid,'XX')<>@customerid  ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.ModifierValue != "")
                    {
                        // PRPosUtils.writelog(resultPosData.ModifierValue);
                        TimeTicks = DateTime.Now.Ticks;
                        psec = "8.2";

                        List<PosModifier> PosModifierList = JsonConvert.DeserializeObject<List<PosModifier>>(resultPosData.ModifierValue);
                        //
                        foreach (PosModifier posModifier in PosModifierList)
                        {
                            cmd.CommandText = @"select * from modifier where sid=@sid ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", posModifier.Sid);
                            cmd.Parameters.AddWithValue("customerid", posModifier.Customerid);
                            // PRPosUtils.writelog(posModifier.Customerid+","+ posModifier.Sid);
                            DataTable modifierDT = new DataTable();
                            da.Fill(modifierDT);
                            if (modifierDT.Rows.Count > 0)
                            {
                                DateTime? UpdDate = null;
                                DateTime? ServerUpdDate = null;
                                if (!posModifier.Upd_date.Equals(""))
                                    ServerUpdDate = DateTime.Parse(posModifier.Upd_date, CultureInfo.CreateSpecificCulture("en-AU"));
                                if (!modifierDT.Rows[0]["upd_date"].ToString().Equals(""))
                                    UpdDate = DateTime.Parse(modifierDT.Rows[0]["upd_date"].ToString());
                                if (ServerUpdDate != UpdDate)
                                {
                                    psec = "8.3";
                                    cmd.CommandText =
                                        @"update modifier set 
                                      modifier_code=@modifier_code, caption=@caption, caption_fn=@caption_fn, image=@image, 
                                      customerid=@customerid,description=@description, upd_date=@upd_date, img_flag=@img_flag ,
                                      del_flag=@del_flag, disp_caption=@disp_caption,disp_price=@disp_price
                                      where sid=@sid  ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", posModifier.Sid);
                                    cmd.Parameters.AddWithValue("customerid", posModifier.Customerid);
                                    cmd.Parameters.AddWithValue("modifier_code", posModifier.Modifier_code);
                                    cmd.Parameters.AddWithValue("caption", posModifier.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", posModifier.Caption_fn);
                                    cmd.Parameters.AddWithValue("image", posModifier.Image);
                                    cmd.Parameters.AddWithValue("description", posModifier.Description);

                                    if (posModifier.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(posModifier.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));


                                    cmd.Parameters.AddWithValue("img_flag", "N");
                                    cmd.Parameters.AddWithValue("del_flag", posModifier.Del_flag);
                                    cmd.Parameters.AddWithValue("disp_caption", posModifier.Disp_caption);
                                    cmd.Parameters.AddWithValue("disp_price", posModifier.Disp_price);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                psec = "8.4";
                                cmd.CommandText =
                                    @"insert into modifier 
                                      ( sid,customerid, modifier_code, caption, caption_fn, image, description, upd_date, 
                                       img_flag,del_flag, ftime,disp_caption,disp_price )
                                      values 
                                      ( @sid,@customerid, @modifier_code, @caption, @caption_fn, @image,@description, @upd_date,
                                       @img_flag, @del_flag, null,@disp_caption,@disp_price )";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", posModifier.Sid);
                                cmd.Parameters.AddWithValue("customerid", posModifier.Customerid);
                                cmd.Parameters.AddWithValue("modifier_code", posModifier.Modifier_code);
                                cmd.Parameters.AddWithValue("caption", posModifier.Caption);
                                cmd.Parameters.AddWithValue("caption_fn", posModifier.Caption_fn);
                                cmd.Parameters.AddWithValue("image", posModifier.Image);
                                cmd.Parameters.AddWithValue("description", posModifier.Description);
                                if (posModifier.Upd_date.Equals(""))
                                    cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(posModifier.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                cmd.Parameters.AddWithValue("img_flag", "N");
                                cmd.Parameters.AddWithValue("del_flag", posModifier.Del_flag);
                                cmd.Parameters.AddWithValue("disp_caption", posModifier.Disp_caption);
                                cmd.Parameters.AddWithValue("disp_price", posModifier.Disp_price);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        App.log.Info(" modifier:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());

                    }

                    psec = "9.1";
                    //check images files
                    try
                    {
                        cmd.CommandText = @"select * from modifier where customerid=@customerid  ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);

                        DataTable modifierDT2 = new DataTable();
                        da.Fill(modifierDT2);
                        client = new HttpClient();
                        TimeTicks = DateTime.Now.Ticks;


                        foreach (DataRow row in modifierDT2.Rows)
                        {
                            psec = "9.2";
                            bool downloadFile = false;
                            FileInfo finfo;
                            if (!row["image"].ToString().Equals(""))
                            {

                                string imgfilename = Path.Combine(imgpath, row["image"].ToString());
                                if (row["img_flag"].ToString().Equals("N")) downloadFile = true;
                                psec = "9.3:" + imgfilename;

                                if (downloadFile)
                                {
                                    psec = "9.4:" + imgfilename;
                                    string URL = posSetting.HostURL + "modifiers/" + posSetting.CustomerID + "/" + row["image"].ToString();
                                    PRPosUtils.writelog(URL);
                                    var downloaddata = await client.GetAsync(URL);
                                    downloaddata.EnsureSuccessStatusCode();
                                    //PRPosUtils.writelog(downloaddata.Content.Headers);
                                    using (FileStream fileStream = new FileStream(imgfilename, FileMode.OpenOrCreate))
                                    {
                                        var ms = await downloaddata.Content.ReadAsByteArrayAsync();
                                        fileStream.Write(ms, 0, ms.Length);
                                        fileStream.Flush();
                                        fileStream.Close();
                                    }
                                    finfo = new FileInfo(imgfilename);
                                    psec = "9.6";
                                    cmd.CommandText = @"update modifier set img_flag='Y',ftime=@ftime   where  sid=@sid ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", row["sid"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")));
                                  //  PRPosUtils.writelog("modifier " + finfo.FullName + " " + finfo.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    psec = "9.10";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        App.log.Info(" modifier image:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());

                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " CheckDownload:" + errhttp.Message);
                    }

                    psec = "10.1";
                    cmd.CommandText = @"delete from psmodset01 where ifnull(customerid,'XX')<>@customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    if (resultPosData.ModSetValue != "")
                    {
                        psec = "10.2";
                        List<ModSetClass> ModSetList = JsonConvert.DeserializeObject<List<ModSetClass>>(resultPosData.ModSetValue);

                        foreach (ModSetClass mset in ModSetList)
                        {
                            psec = "10.3";
                            cmd.CommandText = @"select * from psmodset01 where sid=@sid and customerid=@customerid";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", mset.Sid);
                            cmd.Parameters.AddWithValue("customerid", mset.Customerid);
                            DataTable psmodset01DT = new DataTable();
                            da.Fill(psmodset01DT);
                            psec = "10.4";
                            if (psmodset01DT.Rows.Count > 0)
                            {
                                DateTime? UpdDate = null;
                                DateTime? ServerUpdDate = null;
                                if (!mset.Upd_date.Equals(""))
                                    ServerUpdDate = DateTime.Parse(mset.Upd_date, CultureInfo.CreateSpecificCulture("en-AU"));
                                if (!psmodset01DT.Rows[0]["upd_date"].ToString().Equals(""))
                                    UpdDate = DateTime.Parse(psmodset01DT.Rows[0]["upd_date"].ToString());
                                if (ServerUpdDate != UpdDate)
                                {
                                    psec = "10.5";
                                    cmd.CommandText =
                                        @"update psmodset01 set 
                                       modset_code=@modset_code, caption=@caption, caption_fn=@caption_fn, mod_type=@mod_type,
                                       amount=@amount, max_selection=@max, min_selection=@min, next_modset=@next_modset ,
                                       upd_date=@upd_date, del_flag=@del_flag
                                      where sid=@sid and customerid=@customerid";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", mset.Sid);
                                    cmd.Parameters.AddWithValue("customerid", mset.Customerid);
                                    cmd.Parameters.AddWithValue("modset_code", mset.Modset_code);
                                    cmd.Parameters.AddWithValue("caption", mset.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", mset.Caption_fn);
                                    cmd.Parameters.AddWithValue("mod_type", mset.Mod_type);
                                    cmd.Parameters.AddWithValue("amount", mset.Amount);
                                    cmd.Parameters.AddWithValue("max", mset.Max);
                                    cmd.Parameters.AddWithValue("min", mset.Min);
                                    cmd.Parameters.AddWithValue("next_modset", mset.Next_modset);

                                    if (mset.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mset.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                    cmd.Parameters.AddWithValue("del_flag", mset.Del_flag);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                psec = "10.6";
                                cmd.CommandText =
                                    @"insert into psmodset01 
                                      ( sid, customerid, modset_code, caption, caption_fn, mod_type, amount, max_selection, min_selection, next_modset, del_flag, upd_date )
                                      values 
                                      ( @sid, @customerid, @modset_code, @caption, @caption_fn, @mod_type, @amount, @max, @min, @next_modset, @del_flag, @upd_date )";

                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", mset.Sid);
                                cmd.Parameters.AddWithValue("customerid", mset.Customerid);
                                cmd.Parameters.AddWithValue("modset_code", mset.Modset_code);
                                cmd.Parameters.AddWithValue("caption", mset.Caption);
                                cmd.Parameters.AddWithValue("caption_fn", mset.Caption_fn);
                                cmd.Parameters.AddWithValue("mod_type", mset.Mod_type);
                                cmd.Parameters.AddWithValue("amount", mset.Amount);
                                cmd.Parameters.AddWithValue("max", mset.Max);
                                cmd.Parameters.AddWithValue("min", mset.Min);
                                cmd.Parameters.AddWithValue("next_modset", mset.Next_modset);
                                if (mset.Upd_date.Equals(""))
                                    cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(mset.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                cmd.Parameters.AddWithValue("del_flag", mset.Del_flag);
                                cmd.ExecuteNonQuery();
                            }
                            psec = "10.7";
                            foreach (ModSetTiClass msetti in mset.ModSetlist)
                            {
                                cmd.CommandText = @"select * from psmodsetti where sid=@sid  ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", msetti.Sid);
                                DataTable Dtable = new DataTable();
                                da.Fill(Dtable);
                                if (Dtable.Rows.Count > 0)
                                {
                                    psec = "10.8";
                                    DateTime? UpdDate = null;
                                    DateTime? ServerUpdDate = null;
                                    if (!msetti.Upd_date.Equals(""))
                                        ServerUpdDate = DateTime.Parse(msetti.Upd_date, CultureInfo.CreateSpecificCulture("en-AU"));
                                    if (!Dtable.Rows[0]["upd_date"].ToString().Equals(""))
                                        UpdDate = DateTime.Parse(Dtable.Rows[0]["upd_date"].ToString());
                                    if (ServerUpdDate != UpdDate)
                                    {
                                        cmd.CommandText =
                                        @"update psmodsetti set 
                                           psid=@psid , modifier_code=@modifier_code, caption=@caption, caption_fn=@caption_fn,mod_type=@mod_type,
                                           price_type=@price_type, amount=@amount, max_selection=@max, min_selection=@min, next_modset=@next_modset ,soldout=@soldout,
                                           upd_date=@upd_date, del_flag=@del_flag ,img_flag=@img_flag,image=@image, disp_caption=@disp_caption,disp_price=@disp_price
                                          where sid=@sid ";
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("sid", msetti.Sid);
                                        cmd.Parameters.AddWithValue("psid", msetti.Psid);
                                        cmd.Parameters.AddWithValue("modifier_code", msetti.Modifier_code);
                                        cmd.Parameters.AddWithValue("caption", msetti.Caption);
                                        cmd.Parameters.AddWithValue("caption_fn", msetti.Caption_fn);
                                        cmd.Parameters.AddWithValue("mod_type", msetti.Mod_type);
                                        cmd.Parameters.AddWithValue("price_type", msetti.Price_type);
                                        cmd.Parameters.AddWithValue("amount", msetti.Amount);
                                        cmd.Parameters.AddWithValue("max", msetti.Max);
                                        cmd.Parameters.AddWithValue("min", msetti.Min);
                                        cmd.Parameters.AddWithValue("next_modset", msetti.Next_modset);
                                        cmd.Parameters.AddWithValue("image", msetti.Image);

                                        if (msetti.Upd_date.Equals(""))
                                            cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                        else
                                            cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(msetti.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                        cmd.Parameters.AddWithValue("del_flag", msetti.Del_flag);

                                        cmd.Parameters.AddWithValue("img_flag", "N");
                                        cmd.Parameters.AddWithValue("disp_caption", msetti.Disp_caption);
                                        cmd.Parameters.AddWithValue("disp_price", msetti.Disp_price);
                                        cmd.Parameters.AddWithValue("soldout", msetti.SoldOut);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    psec = "10.9";
                                    cmd.CommandText =
                                    @"insert into  psmodsetti 
                                        (sid, psid,  modifier_code, caption, caption_fn, mod_type, price_type, amount, max_selection,  min_selection, next_modset,
                                        img_flag, image,  upd_date, del_flag,disp_caption,disp_price ,soldout )
                                        values 
                                        (@sid, @psid, @modifier_code, @caption, @caption_fn, @mod_type, @price_type, @amount,@max,@min,  @next_modset,
                                        @img_flag, @image, @upd_date, @del_flag,@disp_caption,@disp_price,@soldout ) ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", msetti.Sid);
                                    cmd.Parameters.AddWithValue("psid", msetti.Psid);
                                    cmd.Parameters.AddWithValue("modifier_code", msetti.Modifier_code);
                                    cmd.Parameters.AddWithValue("caption", msetti.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", msetti.Caption_fn);
                                    cmd.Parameters.AddWithValue("mod_type", msetti.Mod_type);
                                    cmd.Parameters.AddWithValue("price_type", msetti.Price_type);
                                    cmd.Parameters.AddWithValue("amount", msetti.Amount);
                                    cmd.Parameters.AddWithValue("max", msetti.Max);
                                    cmd.Parameters.AddWithValue("min", msetti.Min);
                                    cmd.Parameters.AddWithValue("next_modset", msetti.Next_modset);
                                    cmd.Parameters.AddWithValue("image", msetti.Image);

                                    if (msetti.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(msetti.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));


                                    cmd.Parameters.AddWithValue("del_flag", msetti.Del_flag);

                                    cmd.Parameters.AddWithValue("img_flag", "N");
                                    cmd.Parameters.AddWithValue("disp_caption", msetti.Disp_caption);
                                    cmd.Parameters.AddWithValue("disp_price", msetti.Disp_price);
                                    cmd.Parameters.AddWithValue("soldout", msetti.SoldOut);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                        }

                    }

                    psec = "11.1";
                    //check images files
                    try
                    {
                        cmd.CommandText = @"select psmodsetti.* 
                                       from psmodsetti
                                      left outer join  psmodset01 on psmodset01.sid= psmodsetti.psid
                                       where psmodset01.customerid=@customerid  ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        TimeTicks = DateTime.Now.Ticks;
                        DataTable psmodsettiDT = new DataTable();
                        da.Fill(psmodsettiDT);
                        client = new HttpClient();

                        foreach (DataRow row in psmodsettiDT.Rows)
                        {
                            psec = "11.2";
                            bool downloadFile = false;
                            FileInfo finfo;
                            if (!row["image"].ToString().Equals(""))
                            {
                                string imgfilename = Path.Combine(imgpath, row["image"].ToString());
                                if (row["img_flag"].ToString().Equals("N")) downloadFile = true;
                                psec = "11.3:" + imgfilename;

                                if (downloadFile)
                                {
                                    psec = "11.4:" + imgfilename;
                                    string URL = posSetting.HostURL + "modifiers/" + posSetting.CustomerID + "/" + row["image"].ToString();
                                    var retresult = await ReadURL(URL, imgfilename);

                                    FileInfo finfo2 = new FileInfo(imgfilename);
                                    psec = "11.5:" + imgfilename;
                                    cmd.CommandText = @"update psmodsetti set img_flag='Y',ftime=@ftime   where  sid =@sid ";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", row["sid"].ToString());
                                    cmd.Parameters.AddWithValue("ftime", DateTime.Parse(finfo2.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")));
                                  //  PRPosUtils.writelog("psmodsetti " + finfo2.FullName + " " + finfo2.LastWriteTime.ToString("yyyyMMddHHmmss"));
                                    cmd.ExecuteNonQuery();

                                }
                            }
                        }
                        App.log.Info(" psmodsetti image:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }
                    catch (Exception errhttp)
                    {
                        App.log.Error(psec + " CheckDownload:" + errhttp.Message);
                    }

                    psec = "12.1";
                    App.log.Info("  ItemModValue:" + DateTime.Now.Ticks.ToString());

                    if (resultPosData.ItemModValue != "")
                    {
                        var Trans = cn.BeginTransaction();
                        cmd.CommandText = @"delete from itemmodifier ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        cmd.ExecuteNonQuery();
                        TimeTicks = DateTime.Now.Ticks;
                        psec = "12.1.1";


                        List<ItemMod> ItemModList = JsonConvert.DeserializeObject<List<ItemMod>>(resultPosData.ItemModValue);
                        foreach (ItemMod itemMod in ItemModList)
                        {/*
                            cmd.CommandText = @"select * from itemmodifier where sid=@sid and customerid=@customerid";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", itemMod.Sid);
                            cmd.Parameters.AddWithValue("customerid", itemMod.Customerid);
                            DataTable itemmodifier = new DataTable();
                            da.Fill(itemmodifier);
                            psec = "12.1.2";
                            if (itemmodifier.Rows.Count > 0)
                            {
                                cmd.CommandText = @"update  itemmodifier set  store_code=@store_code, variety_code=@variety_code, item_code=@item_code, 
                                                    modset_code=@modset_code, disp_order=@disp_order, disp_step=@disp_step , upd_date=@upd_date
                                                    where sid=@sid and  customerid=@customerid";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", itemMod.Sid);
                                cmd.Parameters.AddWithValue("customerid", itemMod.Customerid );
                                cmd.Parameters.AddWithValue("store_code", itemMod.Store_code );
                                cmd.Parameters.AddWithValue("variety_code", itemMod.Variety_code);
                                cmd.Parameters.AddWithValue("item_code", itemMod.Item_code );
                                cmd.Parameters.AddWithValue("modset_code", itemMod.Modset_code);
                                
                                cmd.Parameters.AddWithValue("disp_order", itemMod.Disp_order );
                                cmd.Parameters.AddWithValue("disp_step", itemMod.Disp_step);
                                if (itemMod.Upd_date.Equals(""))
                                    cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(itemMod.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                                cmd.ExecuteNonQuery();
                            }
                            else
                            {*/
                            if (!itemMod.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"insert into itemmodifier 
                                           (sid, customerid, store_code,variety_code, item_code, modset_code, disp_step,disp_order, upd_date) 
                                           values 
                                           (@sid, @customerid, @store_code,@variety_code, @item_code,@modset_code,  @disp_step,@disp_order, @upd_date) ";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", itemMod.Sid);
                                cmd.Parameters.AddWithValue("customerid", itemMod.Customerid);
                                cmd.Parameters.AddWithValue("store_code", itemMod.Store_code);
                                cmd.Parameters.AddWithValue("variety_code", itemMod.Variety_code);
                                cmd.Parameters.AddWithValue("item_code", itemMod.Item_code);
                                cmd.Parameters.AddWithValue("modset_code", itemMod.Modset_code);

                                cmd.Parameters.AddWithValue("disp_order", itemMod.Disp_order);
                                cmd.Parameters.AddWithValue("disp_step", itemMod.Disp_step);
                                if (itemMod.Upd_date.Equals(""))
                                    cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                else
                                    cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(itemMod.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                cmd.ExecuteNonQuery();
                            }
                            /*}*/
                        }
                        Trans.Commit();
                        Trans.Dispose();
                        App.log.Info(" itemmodifier:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }
                    App.log.Info("  ItemVarietyValue:" + DateTime.Now.Ticks.ToString());
                    psec = "14.1";
                    if (resultPosData.ItemVarietyValue != "")
                    {
                        var Trans = cn.BeginTransaction();
                        cmd.CommandText = @"delete from itemvariety ";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        cmd.ExecuteNonQuery();
                        psec = "14.1.1";


                        List<ItemVariety> ItemVarietyList = JsonConvert.DeserializeObject<List<ItemVariety>>(resultPosData.ItemVarietyValue);
                        TimeTicks = DateTime.Now.Ticks;
                        foreach (ItemVariety itemVariety in ItemVarietyList)
                        {
                            //PRPosUtils.writelog("  ItemVarietyValue:" + itemVariety.Sid);
                            if (itemVariety.Del_flag.Equals("Y"))
                            {
                                cmd.CommandText = @"delete from itemvariety where sid=@sid";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("sid", itemVariety.Sid);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                psec = "14.1.3";
                                try
                                {
                                    cmd.CommandText =
                                    @"insert into itemvariety 
                                    (sid, customerid, store_code, item_code, variety_code,size_code,cook_type,caption,caption_fn,description,default_item,
                                     sprice,sprice2,sprice3,sprice4,sprice5,sprice6,sprice7,sprice8,sprice9,sprice10,
                                     next_modset, disp_order, del_flag, upd_date,soldout, kitchen_name,kitchen_name_fn) 
                                    values 
                                     (@sid, @customerid, @store_code, @item_code, @variety_code,@size_code,@cook_type,@caption,@caption_fn,@description,@default_item,
                                      @sprice,@sprice2,@sprice3,@sprice4,@sprice5,@sprice6,@sprice7,@sprice8,@sprice9,@sprice10,
                                      @next_modset, @disp_order, @del_flag, @upd_date,@soldout, @kitchen_name,@kitchen_name_fn)";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("sid", itemVariety.Sid);
                                    cmd.Parameters.AddWithValue("customerid", itemVariety.Customerid);
                                    cmd.Parameters.AddWithValue("store_code", itemVariety.Store_code);
                                    cmd.Parameters.AddWithValue("item_code", itemVariety.Item_code);
                                    cmd.Parameters.AddWithValue("variety_code", itemVariety.Variety_code);

                                    cmd.Parameters.AddWithValue("caption", itemVariety.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", itemVariety.Caption_fn);
                                    cmd.Parameters.AddWithValue("description", itemVariety.Description);
                                    cmd.Parameters.AddWithValue("size_code", itemVariety.Size_code);
                                    cmd.Parameters.AddWithValue("cook_type", itemVariety.Cook_type);
                                    cmd.Parameters.AddWithValue("default_item", itemVariety.Default_item);

                                    cmd.Parameters.AddWithValue("caption", itemVariety.Caption);
                                    cmd.Parameters.AddWithValue("caption_fn", itemVariety.Caption_fn);
                                    cmd.Parameters.AddWithValue("description", itemVariety.Description);
                                    cmd.Parameters.AddWithValue("sprice", itemVariety.Sprice1);
                                    cmd.Parameters.AddWithValue("sprice2", itemVariety.Sprice2);
                                    cmd.Parameters.AddWithValue("sprice3", itemVariety.Sprice3);
                                    cmd.Parameters.AddWithValue("sprice4", itemVariety.Sprice4);
                                    cmd.Parameters.AddWithValue("sprice5", itemVariety.Sprice5);
                                    cmd.Parameters.AddWithValue("sprice6", itemVariety.Sprice6);
                                    cmd.Parameters.AddWithValue("sprice7", itemVariety.Sprice7);
                                    cmd.Parameters.AddWithValue("sprice8", itemVariety.Sprice8);
                                    cmd.Parameters.AddWithValue("sprice9", itemVariety.Sprice9);
                                    cmd.Parameters.AddWithValue("sprice10", itemVariety.Sprice10);
                                    cmd.Parameters.AddWithValue("next_modset", itemVariety.Next_modset);
                                    cmd.Parameters.AddWithValue("disp_order", itemVariety.Disp_order);
                                    cmd.Parameters.AddWithValue("del_flag", itemVariety.Del_flag);
                                    if (itemVariety.Upd_date.Equals(""))
                                        cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                                    else
                                        cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(itemVariety.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));
                                    cmd.Parameters.AddWithValue("soldout", itemVariety.Soldout);
                                    cmd.Parameters.AddWithValue("kitchen_name", itemVariety.Kitchen_name);
                                    cmd.Parameters.AddWithValue("kitchen_name_fn", itemVariety.Kitchen_name_fn);
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception err)
                                {
                                    App.log.Error(" itemvariety:" + psec + " " + err.Message);
                                }
                            }
                        }
                        App.log.Info(" itemvariety:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                        Trans.Commit();
                        Trans.Dispose();
                    }

                    psec = "15.1";

                    if (resultPosData.PromotionsValue != "")
                    {
                        psec = "15.1.1";
                        TimeTicks = DateTime.Now.Ticks;
                        cmd.CommandText = @"delete from promotions where ifnull(customerid,'XX')=@customerid";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                        cmd.ExecuteNonQuery();
                        List<Promotions> PromotionsList = JsonConvert.DeserializeObject<List<Promotions>>(resultPosData.PromotionsValue);


                        foreach (Promotions promotions in PromotionsList)
                        {
                            psec = "15.1.2";
                            cmd.CommandText = @"insert into promotions 
                                           (sid, customerid, store_code, item_code, size_code, variety_code,caption,bdate,edate,
                                            sprice,sprice2,sprice3,sprice4,sprice5,
                                            sprice6,sprice7,sprice8,sprice9,sprice10,
                                            w1,w2,w3,w4,w5,w6,w7,daily,del_flag, upd_date,btime,etime ) 
                                           values 
                                           (@sid, @customerid, @store_code, @item_code,@size_code, @variety_code,@caption,@bdate,@edate,
                                            @sprice,@sprice2,@sprice3,@sprice4,@sprice5,
                                            @sprice6,@sprice7,@sprice8,@sprice9,@sprice10,
                                            @w1,@w2,@w3,@w4,@w5,@w6,@w7,@daily,@del_flag, @upd_date,@btime,@etime) ";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sid", promotions.Sid);
                            cmd.Parameters.AddWithValue("customerid", promotions.Customerid);
                            cmd.Parameters.AddWithValue("store_code", promotions.Store_code);
                            cmd.Parameters.AddWithValue("item_code", promotions.Item_code);
                            cmd.Parameters.AddWithValue("size_code", promotions.Size_code);
                            cmd.Parameters.AddWithValue("variety_code", promotions.Variety_code);

                            if (promotions.Bdate.Equals(""))
                                cmd.Parameters.AddWithValue("bdate", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("bdate", DateTime.Parse(promotions.Bdate, CultureInfo.CreateSpecificCulture("en-AU")));

                            if (promotions.Edate.Equals(""))
                                cmd.Parameters.AddWithValue("edate", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("edate", DateTime.Parse(promotions.Edate, CultureInfo.CreateSpecificCulture("en-AU")));

                            cmd.Parameters.AddWithValue("caption", promotions.Caption);


                            cmd.Parameters.AddWithValue("sprice", promotions.Sprice1);
                            cmd.Parameters.AddWithValue("sprice2", promotions.Sprice2);
                            cmd.Parameters.AddWithValue("sprice3", promotions.Sprice3);
                            cmd.Parameters.AddWithValue("sprice4", promotions.Sprice4);
                            cmd.Parameters.AddWithValue("sprice5", promotions.Sprice5);
                            cmd.Parameters.AddWithValue("sprice6", promotions.Sprice6);
                            cmd.Parameters.AddWithValue("sprice7", promotions.Sprice7);
                            cmd.Parameters.AddWithValue("sprice8", promotions.Sprice8);
                            cmd.Parameters.AddWithValue("sprice9", promotions.Sprice9);
                            cmd.Parameters.AddWithValue("sprice10", promotions.Sprice10);

                            cmd.Parameters.AddWithValue("w1", promotions.W1);
                            cmd.Parameters.AddWithValue("w2", promotions.W2);
                            cmd.Parameters.AddWithValue("w3", promotions.W3);
                            cmd.Parameters.AddWithValue("w4", promotions.W4);
                            cmd.Parameters.AddWithValue("w5", promotions.W5);
                            cmd.Parameters.AddWithValue("w6", promotions.W6);
                            cmd.Parameters.AddWithValue("w7", promotions.W7);
                            cmd.Parameters.AddWithValue("daily", promotions.Daily);
                            cmd.Parameters.AddWithValue("btime", promotions.Btime);
                            cmd.Parameters.AddWithValue("etime", promotions.Etime);

                            cmd.Parameters.AddWithValue("del_flag", promotions.Del_flag);
                            if (promotions.Upd_date.Equals(""))
                                cmd.Parameters.AddWithValue("upd_date", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("upd_date", DateTime.Parse(promotions.Upd_date, CultureInfo.CreateSpecificCulture("en-AU")));

                            cmd.ExecuteNonQuery();

                        }
                        App.log.Info(" promotions:" + ((DateTime.Now.Ticks - TimeTicks) / 10000).ToString());
                    }

                    psec = "16.0";
                    //insert new items to local "Sold Out" settings
                    /*
                    cmd.CommandText = @"DELETE FROM localitemsetting where customerid != @customerid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("customerid", posSetting.CustomerID);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"INSERT INTO localitemsetting (customerid,item_code,upd_date,soldout) 
                                    SELECT customerid
                                    ,item_code
                                    ,upd_date
                                    ,CASE 
                                        WHEN soldout IS NULL THEN 'N' 
                                        WHEN soldout = 0 THEN 'N' 
                                        WHEN soldout ='' THEN 'N' 
                                        ELSE soldout
                                    END as soldout
                                    FROM psitem 
                                    WHERE item_code not in (select item_code from localitemsetting)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"DELETE FROM localmodifiersetting  where psmodsetti_id not in (select sid from psmodsetti where psid in (select sid from psmodset01))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"INSERT INTO localmodifiersetting (psmodsetti_id,modifier_code,upd_date,soldout)
                                    SELECT sid,modifier_code,upd_date,'N'
                                    FROM psmodsetti 
                                    WHERE sid not in (select psmodsetti_id from localmodifiersetting)";
                    cmd.ExecuteNonQuery();
                    */
                } //using
                App.log.Info(" end download check ");
                PRPosDB.ReadParameterPart2();
            }
            catch (Exception err)
            {
                App.log.Error(" error CheckDownload:" + psec + " " + err.Message);
            }
            return ret;
        }
    }
}
