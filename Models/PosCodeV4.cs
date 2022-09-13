using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class PosCodeV4
    {
        string code = "";
        public string Code
        {
            get { return code; }
            set { code = value; }
        }


        string mAC = "";
        public string MAC { get => mAC; set => mAC = value; }

        string retcode = "";
        public string RetCode
        {
            get { return retcode; }
            set { retcode = value; }
        }

        string message = "";
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        string accessCode = "";
        string token = "";
        public string Token { get => token; set => token = value; }

        string customerid = "";
        public string CustomerID
        {
            get { return customerid; }
            set { customerid = value; }
        }

        string store_code = "";
        public string Store_code { get => store_code; set => store_code = value; }

        string expirydate = "";
        public string ExpiryDate
        {
            get { return expirydate; }
            set { expirydate = value; }
        }

        string pos_code = "";
        public string Pos_Code { get => pos_code; set => pos_code = value; }

        string pos_name = "";
        public string Pos_Name { get => pos_name; set => pos_name = value; }

        string posid = "";
        public string PosID { get => posid; set => posid = value; }


        string cust_name = "";
        public string CustomerName
        {
            get { return cust_name; }
            set { cust_name = value; }
        }

        string set_code = "";
        string config_code = "";
        public string Set_Code { get => set_code; set => set_code = value; }

        public string AccessCode { get => accessCode; set => accessCode = value; }
        public string Config_code { get => config_code; set => config_code = value; }


        string salePriceColumn = "";
        string takeawayPriceColumn = "";
        string uBereatPriceColumn = "";
        string phoneOrderPriceColumn = "";
        public string SalePriceColumn { get => salePriceColumn; set => salePriceColumn = value; }
        public string TakeawayPriceColumn { get => takeawayPriceColumn; set => takeawayPriceColumn = value; }
        public string UBereatPriceColumn { get => uBereatPriceColumn; set => uBereatPriceColumn = value; }
        public string PhoneOrderPriceColumn { get => phoneOrderPriceColumn; set => phoneOrderPriceColumn = value; }

    }
}

