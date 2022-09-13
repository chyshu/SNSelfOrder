using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class NIC
    {
        string adapterName = "";
        string description = "";
        string mAC = "";
        string iPV6 = "";
        string iPV4 = "";

        public string AdapterName
        {
            get { return adapterName; }
            set { adapterName = value; }
        }
        public string MAC
        {
            get { return mAC; }
            set { mAC = value; }
        }
        public string IPV6
        {
            get { return iPV6; }
            set { iPV6 = value; }
        }
        public string IPV4
        {
            get { return iPV4; }
            set { iPV4 = value; }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
