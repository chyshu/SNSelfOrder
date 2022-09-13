using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.DAL
{
    public interface IStationDAL
    {
        Station SelectStation (SelfOrderSettingClass SelfOrderSetting);
        void UpdateStation(SelfOrderSettingClass SelfOrderSetting, Station _station);
        void DeleteStation(SelfOrderSettingClass SelfOrderSetting);

        void InsertStation(SelfOrderSettingClass SelfOrderSetting, Station _station);
    }
    public class StationDAL : IStationDAL
    {
        string connectString = "";
        public Station SelectStation(SelfOrderSettingClass SelfOrderSetting)
        {
            Station ret = new Station();
            using (SQLiteConnection connection = new SQLiteConnection(SelfOrderSetting.ConnString))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"select * from station  where sid=1";
                DataTable stationDT = new DataTable();
                da.Fill(stationDT);
                if (stationDT.Rows.Count == 0)
                {
                    cmd.CommandText = @"insert into station 
                                        (sid, customerid, pos_code, connectioncode,  store_code ) values ( 1,'','','','')";
                    cmd.ExecuteNonQuery();
 
                    cmd.CommandText = @"select * from station  where sid=1";
                    stationDT = new DataTable();
                    da.Fill(stationDT);
                }
                if (stationDT.Rows.Count > 0)
                {
                    DataRow station = stationDT.Rows[0];
                    DateTime? edate = null;
                    DateTime? ldate = null;
                    DateTime? regdate = null;
                    DateTime tdate;
                    if (DateTime.TryParse(station["expiry_date"].ToString(), out tdate))
                        edate = tdate;
                    if (DateTime.TryParse(station["last_checked"].ToString(), out tdate))
                        ldate = tdate;
                    if (DateTime.TryParse(station["registed_date"].ToString(), out tdate))
                        regdate = tdate;
                    ret = new Station()
                    {

                        CustomerID = station["customerid"].ToString(),
                        Store_code = station["store_code"].ToString(),
                        Pos_code = station["pos_code"].ToString(),
                        Pos_name = station["pos_name"].ToString(),
                        Expiry_date = edate,
                        Last_checked = ldate,
                        Connection = station["connectioncode"].ToString(),
                        Token = station["token"].ToString(),
                        stationid = station["stationid"].ToString(),
                        Accesscode = station["accesscode"].ToString(),
                        Vcode = station["vcode"].ToString(),
                        Set_code = station["set_code"].ToString(),
                        Config_code = station["config_code"].ToString(),
                        Pricecolumn1 = station["pricecolumn1"].ToString().Equals("") ? "sprice" : station["pricecolumn1"].ToString(),
                        Pricecolumn2 = station["pricecolumn2"].ToString().Equals("") ? "sprice2" : station["pricecolumn2"].ToString(),
                        Pricecolumn3 = station["pricecolumn3"].ToString().Equals("") ? "sprice3" : station["pricecolumn3"].ToString(),
                        Pricecolumn4 = station["pricecolumn4"].ToString().Equals("") ? "sprice4" : station["pricecolumn4"].ToString(),
                        MAC = station["mac_address"].ToString().Equals("") ? "" : station["mac_address"].ToString(),
                        Registed_date = regdate,
                        SalePriceColumn = station["pricecolumn1"].ToString().Equals("") ? "sprice" : station["pricecolumn1"].ToString(),
                        TakeawayPriceColumn = station["pricecolumn2"].ToString().Equals("") ? "sprice2" : station["pricecolumn2"].ToString(),
                        Sid = station["sid"].ToString().Equals("") ? "1" : station["sid"].ToString(),                        
                    };
                }
            }
            return ret;
        }
    
        public void UpdateStation(SelfOrderSettingClass SelfOrderSetting, Station station )
        {
            
            using (SQLiteConnection connection = new SQLiteConnection(SelfOrderSetting.ConnString))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"update station set 
                                customerid=@customerid,                                
                                 pos_code=@pos_code,
                                expiry_date=@expiry_date, 
                                last_checked=@last_checked,
                                mac_address=@mac_address,                              
                                connectioncode=@connectioncode,
                                token=@token,
                                stationid=@stationid,                                
                                accesscode=@accesscode,
                                vcode=@vcode,                                                               
                                pos_name=@pos_name,
                                set_code=@set_code,                                                                
                                config_code=@config_code ,                                
                                pricecolumn1=@pricecolumn1,
                                pricecolumn2=@pricecolumn2,
                                pricecolumn3=@pricecolumn3,
                                pricecolumn4=@pricecolumn4,
                                store_code=@store_code
                                where sid=@sid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("sid", station.Sid);
                cmd.Parameters.AddWithValue("customerid", station.CustomerID);
                cmd.Parameters.AddWithValue("pos_code", station.Pos_code);
                //                cmd.Parameters.AddWithValue("expiry_date", DateTime.ParseExact(station.Expiry_date.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                //                cmd.Parameters.AddWithValue("last_checked", DateTime.ParseExact(station.Last_checked.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("expiry_date",  station.Expiry_date);
                cmd.Parameters.AddWithValue("last_checked", station.Last_checked);
                cmd.Parameters.AddWithValue("mac_address", station.MAC);
                //   cmd.Parameters.AddWithValue("registed_date", DateTime.ParseExact(station.Registed_date.Value.ToString("dd/MM/yyyy"),"dd/MM/yyyy", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("registed_date",station.Registed_date.Value);
                cmd.Parameters.AddWithValue("connectioncode", station.Connection);
                cmd.Parameters.AddWithValue("token", station.Token);
                cmd.Parameters.AddWithValue("stationid", station.PosID);
                cmd.Parameters.AddWithValue("accesscode", station.Accesscode);
                cmd.Parameters.AddWithValue("vcode", station.Vcode);
                               
                cmd.Parameters.AddWithValue("pos_name", station.Pos_name);
                cmd.Parameters.AddWithValue("set_code", station.Set_code);
                cmd.Parameters.AddWithValue("config_code", station.Config_code);

                cmd.Parameters.AddWithValue("pricecolumn1", station.Pricecolumn1);
                cmd.Parameters.AddWithValue("pricecolumn2", station.Pricecolumn2);
                cmd.Parameters.AddWithValue("pricecolumn3", station.Pricecolumn3);
                cmd.Parameters.AddWithValue("pricecolumn4", station.Pricecolumn4);
                cmd.Parameters.AddWithValue("store_code", station.Store_code);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(Exception err)
                {
                    App.log.Error(" CheckCode error:" + err.Message);
                }
            }
            
        }

        public void DeleteStation(SelfOrderSettingClass SelfOrderSetting)
        {
            using (SQLiteConnection connection = new SQLiteConnection(SelfOrderSetting.ConnString))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"delete from station  where sid=1";
                cmd.ExecuteNonQuery();
            }
        }
        public void InsertStation(SelfOrderSettingClass SelfOrderSetting, Station station)
        {

            using (SQLiteConnection connection = new SQLiteConnection(SelfOrderSetting.ConnString))
            {
                connection.Open();
                SQLiteCommand cmd = connection.CreateCommand();
                SQLiteDataAdapter da = new SQLiteDataAdapter();
                da.SelectCommand = cmd;
                cmd.CommandText = @"Insert into  station 
                     (sid , customerid , pos_code, expiry_date , last_checked , mac_address , registed_date , screens , connectioncode , token , stationid , accesscode ,
                      vcode, pos_name , set_code , config_code , pricecolumn1 , pricecolumn2 , pricecolumn3 , pricecolumn4 , store_code)
                     values
                    ( @sid , @customerid , @pos_code, @expiry_date , @last_checked , @mac_address , @registed_date , @screens , @connectioncode , @token , @stationid , @accesscode ,
                      @vcode, @pos_name , @set_code , @config_code , @pricecolumn1 , @pricecolumn2 , @pricecolumn3 , @pricecolumn4 , @store_code)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("sid", 1);
                cmd.Parameters.AddWithValue("customerid", station.CustomerID);
                cmd.Parameters.AddWithValue("pos_code", station.Pos_code);
                //cmd.Parameters.AddWithValue("expiry_date", DateTime.ParseExact(station.Expiry_date.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                //cmd.Parameters.AddWithValue("last_checked", DateTime.ParseExact(station.Last_checked.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("expiry_date", station.Expiry_date);
                cmd.Parameters.AddWithValue("last_checked", station.Last_checked);
                cmd.Parameters.AddWithValue("mac_address", station.MAC);
                //cmd.Parameters.AddWithValue("registed_date", DateTime.ParseExact(station.Registed_date.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture));                
                cmd.Parameters.AddWithValue("registed_date", station.Registed_date);
                cmd.Parameters.AddWithValue("screens", DBNull.Value);
                cmd.Parameters.AddWithValue("connectioncode", station.Connection);
                cmd.Parameters.AddWithValue("token", station.Token);
                cmd.Parameters.AddWithValue("stationid", station.PosID);
                cmd.Parameters.AddWithValue("accesscode", station.Accesscode);
                cmd.Parameters.AddWithValue("vcode", station.Vcode);

                cmd.Parameters.AddWithValue("pos_name", station.Pos_name);
                cmd.Parameters.AddWithValue("set_code", station.Set_code);
                cmd.Parameters.AddWithValue("config_code", station.Config_code);

                cmd.Parameters.AddWithValue("pricecolumn1", station.Pricecolumn1);
                cmd.Parameters.AddWithValue("pricecolumn2", station.Pricecolumn2);
                cmd.Parameters.AddWithValue("pricecolumn3", station.Pricecolumn3);
                cmd.Parameters.AddWithValue("pricecolumn4", station.Pricecolumn4);
                cmd.Parameters.AddWithValue("store_code", station.Store_code);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    App.log.Error(" Insert Station error:" + err.Message);
                }
            }

        }
    }
}
