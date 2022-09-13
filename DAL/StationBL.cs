using SNSelfOrder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.DAL
{
    public class StationBL
    {
        public IStationDAL StationDAL;
       

        public StationBL()
        {
            
        }
        public Station GetStation(SelfOrderSettingClass SelfOrderSetting, IStationDAL stationDAL)
        {            
            this.StationDAL = stationDAL;
            return this.StationDAL.SelectStation(SelfOrderSetting);
        }
        public void UpdateStation(SelfOrderSettingClass SelfOrderSetting, IStationDAL stationDAL,Station st)
        {
            this.StationDAL = stationDAL;
            this.StationDAL.UpdateStation(SelfOrderSetting, st);
        }

        public void DeleteStation(SelfOrderSettingClass SelfOrderSetting, IStationDAL stationDAL)
        {
            this.StationDAL = stationDAL;
            this.StationDAL.DeleteStation(SelfOrderSetting);
        }

        public void InsertStation(SelfOrderSettingClass SelfOrderSetting, IStationDAL stationDAL, Station st)
        {
            this.StationDAL = stationDAL;
            this.StationDAL.InsertStation(SelfOrderSetting, st);
        }
    }
}
