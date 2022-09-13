using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    
    public class  CheckConnectionCodeService
    {
        private const int DaySeconds= (24 * 60 * 60);
        
        private SelfOrderSettingClass selfOrderSetting;
        private  System.Threading.SynchronizationContext _syncContext = null;

        public CheckConnectionCodeService(SelfOrderSettingClass SelfOrderSetting)
        {
            selfOrderSetting = SelfOrderSetting;
 
        }

        #region newwork_NIC
        /*
        private List<Nic> GetMacAddress()
        {
            List<Nic> ret = new List<Nic>();

            string macAddress = string.Empty;
            string ipv4 = string.Empty;
            string ipv6 = string.Empty;


            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ConnectorPresent(nic))
                {
                    IPInterfaceProperties adapterProperties = nic.GetIPProperties();
                    UnicastIPAddressInformationCollection allAddress = adapterProperties.UnicastAddresses;
                    //  richTextBox1.AppendText(
                    //     "Found MAC Address: " + nic.GetPhysicalAddress() +
                    //     " Type: " + nic.NetworkInterfaceType + System.Environment.NewLine);

                    if (allAddress.Count > 0)
                    {
                        ///  PRPosUtils.writelog(nic.NetworkInterfaceType);
                        //  PRPosUtils.writelog(nic.Description);
                        //  PRPosUtils.writelog(nic.Name);
                        if ((nic.OperationalStatus == OperationalStatus.Up) &&
                           ((nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet) || (nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet) || (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)))
                        {

                            macAddress = nic.GetPhysicalAddress().ToString();
                            foreach (UnicastIPAddressInformation addr in allAddress)
                            {
                                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    ipv4 = addr.Address.ToString();
                                }
                                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                                {
                                    ipv6 = addr.Address.ToString();
                                }
                            }

                            // PRPosUtils.writelog("MAC=" + macAddress + ",ip4=" + ipv4 + ", ip6=" + ipv6);



                            if (string.IsNullOrWhiteSpace(macAddress) || (string.IsNullOrWhiteSpace(ipv4) && string.IsNullOrWhiteSpace(ipv6)))
                            {
                                macAddress = "";
                                ipv4 = "";
                                ipv6 = "";
                                continue;
                            }
                            else
                            {
                                if (macAddress.Length == 12)
                                {
                                    macAddress = string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                                    macAddress.Substring(0, 2), macAddress.Substring(2, 2), macAddress.Substring(4, 2),
                                    macAddress.Substring(6, 2), macAddress.Substring(8, 2), macAddress.Substring(10, 2));

                                    ret.Add(new Nic() { AdapterName = nic.Name, Description = nic.Description, MAC = macAddress, IPV4 = ipv4, IPV6 = ipv6 });
                                }
                                //  break;
                            }
                        }
                    }
                }
            }

            return ret;
        }
        private static bool ConnectorPresent(NetworkInterface ni)
        {
            ManagementScope scope = new ManagementScope(@"\\localhost\root\StandardCimv2");
            ObjectQuery query = new ObjectQuery(String.Format(
                @"SELECT * FROM MSFT_NetAdapter WHERE ConnectorPresent = True AND DeviceID = '{0}'", ni.Id));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            ManagementObjectCollection result = searcher.Get();
            return result.Count > 0;
        }
        */
        #endregion

        [STAThread]
        public async Task<Models.Station> LaunchCodeCheck(string ConnectionCode)
        {
            string section = "";
            //App.log.Info("LaunchCodeCheck ");
            Models.Station _station = new Models.Station();
            try
            {
                HttpClient client = new HttpClient();
                PosCodeV4 p = new PosCodeV4
                {
                    // Code = "01451007",// this.txtConnectionCode.Text, RetCode = "",
                    Code = ConnectionCode, //  selfOrderSetting.ConnectionCode,
                    // Code= "01451022",
                    RetCode = "",
                    // MAC = "1C-1B-0D-EC-67-CF"

                    MAC =
#if _DEBUG
                 "1C-1B-0D-EC-67-CF"
#else
                    selfOrderSetting.MAC
#endif

                    //  MAC = "1C-1B-0D-EC-67-CF"
                    //4C-72-B9-F8-CA-D1
                };
                string postBody = JsonConvert.SerializeObject(p);
                //PRPosUtils.writelog(" CheckCode:" + HostURL);
                App.log.Info("CheckCode send:" + postBody);
                HttpResponseMessage response = await client.PostAsync(selfOrderSetting.HostURL + "v3/" + "poscode", new StringContent(postBody, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                App.log.Info(" CheckCode return:" + responseBody);
                PosCodeV4 posCodeV4 = JsonConvert.DeserializeObject<PosCodeV4>(responseBody);
                App.log.Info("doCheck ");
                // st = await doCheck(retobject);

                if (posCodeV4.RetCode.Equals("0"))
                {
                    DateTime last_checked = DateTime.Now;
                    string accCode = selfOrderSetting.Security.Decrypt(posCodeV4.AccessCode);
                    if (!accCode.Equals(""))
                    {
                        DAL.StationBL stationBL = new DAL.StationBL();

                        int checksum; DateTime rdate, edate;
                        if (PRPosUtils.ThisStation.Connection != posCodeV4.Code)
                        {
                            // new code for this station
                            // delete current station
                            stationBL.DeleteStation(PRPosUtils.SelfOrderSetting ,  new DAL.StationDAL());
                            if (selfOrderSetting.Security.DcodeCode(accCode, out checksum, out rdate, out edate))
                            {
                                byte[] buffer = Encoding.UTF8.GetBytes(posCodeV4.Code);
                                int ckSUM = 0;

                                foreach (byte b in buffer)
                                    ckSUM += b;
                                if (((ckSUM % 8) == checksum))
                                {
                                    PRPosUtils.ThisStation.Registed_date = rdate;
                                    PRPosUtils.ThisStation.Expiry_date = edate;
                                    if (last_checked >= rdate)
                                    {
                                        TimeSpan tmDiff = PRPosUtils.ThisStation.Expiry_date.Value - last_checked.Date;
                                        string vcode_str = (last_checked.Ticks / 10000000 / DaySeconds).ToString() + "+" + tmDiff.Days.ToString();
                                        string vcode = selfOrderSetting.Security.Encrypt(vcode_str);
                                        PRPosUtils.ThisStation.Vcode = vcode;
                                        PRPosUtils.ThisStation.Last_checked = last_checked;
                                        PRPosUtils.ThisStation.Accesscode = posCodeV4.AccessCode;
                                        PRPosUtils.ThisStation.Connection = posCodeV4.Code;

                                        PRPosUtils.ThisStation.Token = posCodeV4.Token;
                                        PRPosUtils.ThisStation.Config_code = posCodeV4.Config_code;
                                        PRPosUtils.ThisStation.Set_code = posCodeV4.Set_Code;
                                        PRPosUtils.ThisStation.IsAuth = true;
                                        PRPosUtils.ThisStation.MAC = posCodeV4.MAC;
                                        PRPosUtils.ThisStation.SalePriceColumn = posCodeV4.SalePriceColumn;
                                        PRPosUtils.ThisStation.TakeawayPriceColumn = posCodeV4.TakeawayPriceColumn;
                                        PRPosUtils.ThisStation.CustomerID = posCodeV4.CustomerID;
                                        PRPosUtils.ThisStation.Store_code = posCodeV4.Store_code;
                                        PRPosUtils.ThisStation.Pos_code = posCodeV4.Pos_Code;
                                        PRPosUtils.ThisStation.Pos_name = posCodeV4.Pos_Name;
                                        PRPosUtils.ThisStation.Pricecolumn1 = posCodeV4.SalePriceColumn;
                                        PRPosUtils.ThisStation.Pricecolumn2 = posCodeV4.TakeawayPriceColumn;
                                        PRPosUtils.ThisStation.Pricecolumn3 = posCodeV4.UBereatPriceColumn;
                                        PRPosUtils.ThisStation.Pricecolumn4 = posCodeV4.PhoneOrderPriceColumn;
                                        _station = PRPosUtils.ThisStation;

                                        PRPosUtils.SelfOrderSetting.ConnectionCode = PRPosUtils.ThisStation.Connection;
                                        PRPosUtils.SelfOrderSetting.Token = PRPosUtils.ThisStation.Token;
                                        PRPosUtils.SelfOrderSetting.StoreCode = PRPosUtils.ThisStation.Store_code;
                                        PRPosUtils.SelfOrderSetting.PosCode = PRPosUtils.ThisStation.Pos_code;
                                        stationBL.InsertStation(PRPosUtils.SelfOrderSetting, new DAL.StationDAL(), PRPosUtils.ThisStation);

                                        section = "1.8";
                                        PRPosUtils.ThisStation.StatusCode = 0;
                                        PRPosUtils.ThisStation.IsAuth = true;
                                        _station.StatusCode = 0;

                                    }
                                    else
                                    {
                                        _station.StatusCode = -2;// Date Issues; ;
                                    }
                                }
                                else
                                {
                                    _station.StatusCode = -3;// CHECKSUM_ERROR; ;
                                }
                            }
                            else{
                                _station.StatusCode = -4;//DECODE_ERROR; ;
                            }
                        }
                        else
                        {
                            // code exists                                                                          
                            if (selfOrderSetting.Security.DcodeCode(accCode, out checksum, out rdate, out edate))
                            {
                                section = "1.4";

                                byte[] buffer = Encoding.UTF8.GetBytes(PRPosUtils.ThisStation.Connection);
                                int ckSUM = 0;

                                foreach (byte b in buffer)
                                    ckSUM += b;
                                if (((ckSUM % 8) == checksum))
                                {
                                    PRPosUtils.ThisStation.Registed_date = rdate;
                                    PRPosUtils.ThisStation.Expiry_date = edate;

                                    int daycount = 0;
                                    DateTime POSlatestDate = DateTime.Today;
                                    string vcode = PRPosUtils.ThisStation.Vcode;
                                    string strdate = selfOrderSetting.Security.Decrypt(PRPosUtils.ThisStation.Vcode );

                                    string[] strValue = accCode.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strValue.Length == 2)
                                    {
                                        int V1 = 0;
                                        int.TryParse(strValue[0], out V1);

                                        int.TryParse(strValue[1], out daycount);
                                        long ticks = (long)V1 * (long)10000000 * DaySeconds;
                                        POSlatestDate = new DateTime(ticks);
                                    }

 

                                    // check time 
                                    if (last_checked.Date >= POSlatestDate.Date)
                                    {
                                        TimeSpan tmDiff = PRPosUtils.ThisStation.Expiry_date.Value - last_checked.Date;
                                        // ticks of last check date & remainder days
                                        daycount = tmDiff.Days;
                                        string vcode_str = (last_checked.Ticks / 10000000 / DaySeconds).ToString() + "+" + tmDiff.Days.ToString();
                                        PRPosUtils.ThisStation.Vcode= selfOrderSetting.Security.Encrypt(vcode_str);
                                    }
                                    else
                                    {
                                        TimeSpan tmDiff = PRPosUtils.ThisStation.Expiry_date.Value - POSlatestDate.Date;
                                        // ticks of last check date & remainder days
                                        daycount = tmDiff.Days;
                                        string vcode_str = (POSlatestDate.Ticks / 10000000 / DaySeconds).ToString() + "+" + tmDiff.Days.ToString();
                                        PRPosUtils.ThisStation.Vcode= selfOrderSetting.Security.Encrypt(vcode_str);
                                    }

                              
                                    section = "1.6";
                                    PRPosUtils.ThisStation.Last_checked = last_checked;
                                    PRPosUtils.ThisStation.Accesscode = posCodeV4.AccessCode;
                                    PRPosUtils.ThisStation.Connection = posCodeV4.Code;

                                    PRPosUtils.ThisStation.Token = posCodeV4.Token;
                                    PRPosUtils.ThisStation.Config_code = posCodeV4.Config_code;
                                    PRPosUtils.ThisStation.Set_code = posCodeV4.Set_Code;
                                    PRPosUtils.ThisStation.IsAuth = true;
                                    PRPosUtils.ThisStation.MAC = posCodeV4.MAC;
                                    PRPosUtils.ThisStation.SalePriceColumn = posCodeV4.SalePriceColumn;
                                    PRPosUtils.ThisStation.TakeawayPriceColumn = posCodeV4.TakeawayPriceColumn;
                                    PRPosUtils.ThisStation.CustomerID = posCodeV4.CustomerID;
                                    PRPosUtils.ThisStation.Store_code = posCodeV4.Store_code;
                                    PRPosUtils.ThisStation.Pos_code = posCodeV4.Pos_Code;
                                    PRPosUtils.ThisStation.Pos_name = posCodeV4.Pos_Name;
                                    PRPosUtils.ThisStation.Pricecolumn1 = posCodeV4.SalePriceColumn;
                                    PRPosUtils.ThisStation.Pricecolumn2 = posCodeV4.TakeawayPriceColumn;
                                    PRPosUtils.ThisStation.Pricecolumn3 = posCodeV4.UBereatPriceColumn;
                                    PRPosUtils.ThisStation.Pricecolumn4 = posCodeV4.PhoneOrderPriceColumn;
                                    _station = PRPosUtils.ThisStation;

                                    PRPosUtils.SelfOrderSetting.ConnectionCode = PRPosUtils.ThisStation.Connection;
                                    PRPosUtils.SelfOrderSetting.Token = PRPosUtils.ThisStation.Token;
                                    PRPosUtils.SelfOrderSetting.StoreCode = PRPosUtils.ThisStation.Store_code;
                                    PRPosUtils.SelfOrderSetting.PosCode = PRPosUtils.ThisStation.Pos_code;
                                    
                                    stationBL.UpdateStation(PRPosUtils.SelfOrderSetting, new DAL.StationDAL(), PRPosUtils.ThisStation);

                                    if ((daycount > 0) || (daycount < -7))
                                    {
                                        section = "1.8";
                                        PRPosUtils.ThisStation.StatusCode = 0;
                                        PRPosUtils.ThisStation.IsAuth = true;
                                        _station.StatusCode = 0;
                                    }
                                    else
                                    {
                                        _station.StatusCode = -1;// EXPRIRED; ;
                                    }

                                }
                                else
                                {
                                    _station.StatusCode = -3;// CHECKSUM_ERROR; ;
                                }
                            }
 
                        }
                    }
                    else
                    {
                        _station.StatusCode = -5;// DECODE_ERROR; ;
                    }                   
                }
                else
                {
                    //connection code error
                    if (posCodeV4.RetCode.Equals("908"))
                        _station.StatusCode = -8;
                    else if (posCodeV4.RetCode.Equals("909"))
                        _station.StatusCode = -9;
                    else if (posCodeV4.RetCode.Equals("907"))
                        _station.StatusCode = -7;
                    else
                        _station.StatusCode = -6;
                }
            }
            catch (Exception err)
            {
                App.log.Error(" CheckCode error:" + err.Message);
            }
            return _station;
        }

        public async Task<object> CheckCCode(string ConnectionCode)
        {
            Models.Station _station = new Models.Station();
            _syncContext = System.Threading.SynchronizationContext.Current;
            ConnectionCode frmConnection = new ConnectionCode();
            var vm = new ViewModel.ConnectionCodeVM();
            vm.ConnectionCode = ConnectionCode;
            vm.DisplayMessage = "Input Connection Code";
            App.log.Info(" CheckConnectionCodeService Start   ");
             vm.InputClose += async (e) => {
                string strconnection = (e as string);
                App.log.Info(" CheckConnectionCodeService  :" + strconnection);
                var task =await LaunchCodeCheck(strconnection);
                App.log.Info(" CheckCode 1 :" + task.StatusCode);
                _station = task;
                if (_station.StatusCode == 0)
                {
                    System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                    {
                        frmConnection.Close();
                        
                    };
                    _syncContext.Post(methodDelegate, null);
                }
            };
            return _station;
        }

        public async Task< Models.Station> CheckCode(string ConnectionCode)
        {
            Models.Station _station = new Models.Station();
            if ((PRPosUtils.ThisStation.Connection == "") || (PRPosUtils.InputCode == "1"))
            {
                _syncContext = System.Threading.SynchronizationContext.Current;
                ConnectionCode frmConnection = new ConnectionCode();
                var vm = new ViewModel.ConnectionCodeVM();
                vm.ConnectionCode = ConnectionCode;
                vm.DisplayMessage = "Input Connection Code";
                App.log.Info(" CheckConnectionCodeService Start   ");
                vm.InputClose += async(e) =>
                {
                    string strconnection = (e as string);
                    App.log.Info(" CheckConnectionCodeService  :" + strconnection );
                    await LaunchCodeCheck(strconnection).ContinueWith(task =>
                    {
                        App.log.Info(" CheckCode 1 :" + task.Result);
                        _station = task.Result;
                        if (_station.StatusCode == 0)
                        {
                            /* connection code is active */

                            System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                            {

                                frmConnection.Close();
                            };
                            _syncContext.Post(methodDelegate, null);

                        }
                        else
                        {
                            vm.DisplayMessage = StatusToMessage(_station.StatusCode);
                        }
                    });
                };
                frmConnection.DataContext = vm;
                frmConnection.ShowDialog();
                App.log.Info(" CheckConnectionCodeService frmConnection show   ");
            }
            else
            {
                await LaunchCodeCheck(ConnectionCode).ContinueWith(task =>
               {
                   _station = task.Result;
                   App.log.Error(" CheckCode 2 :" + _station.StatusCode);

                    // if can't connect to server show connection code dialog
                    if ((_station.StatusCode != -3) && (_station.StatusCode != -4) && (_station.StatusCode != -5))
                   {
                       ConnectionCode frmConnection = new ConnectionCode();
                       var vm = new ViewModel.ConnectionCodeVM();
                       vm.ConnectionCode = ConnectionCode;
                       vm.DisplayMessage = "Connection Code registered by another station";

                       vm.InputClose += async (e) =>
                       {
                           string strconnection = (e as string);
                           var task1 = await LaunchCodeCheck(strconnection);
                           {
                               _station = task1;
                               if (_station.StatusCode == 0)
                               {
                                    /* connection code is active */

                                   System.Threading.SendOrPostCallback methodDelegate = delegate (object state)
                                   {
                                       frmConnection.Close();
                                   };
                                   _syncContext.Post(methodDelegate, null);

                               }
                               else
                               {
                                   vm.DisplayMessage = StatusToMessage(_station.StatusCode);
                               }
                           }
                       };
                       frmConnection.DataContext = vm;
                       frmConnection.Show();
                   }
               });
            }
            App.log.Error(" CheckCode return  :" + _station);
            return _station;
        }
    
        public string StatusToMessage(int StatusCode)
        {
            string ret = "";

            if (StatusCode == -1)
            {
                ret = "Connection Code EXPRIRED!!";
            }
            else if (StatusCode == -2)
            {
                ret = "Connection Code Expire Date error";
            }
            else if (StatusCode == -3)
            {
                ret = "Checksum Error";
            }
            else if (StatusCode == -4)
            {
                ret = "Decode Error";
            }
            else if (StatusCode == -5)
            {
                ret = "Decode Error";
            }
            else if (StatusCode == -9)
            {
                ret = "station not found";
            }
            else if (StatusCode == -7)
            {
                ret = "Connection Code registered by another station";
            }
            else if (StatusCode == -8)
            {
                ret = "Connection Code not found";
            }
            else if (StatusCode == -9)
            {
                ret = "Terminal not found";
            }
            return ret;
        }
    }
}
