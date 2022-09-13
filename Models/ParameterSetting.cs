using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    [Serializable]
    public class ParameterSetting
    {
        string code = "";
        string code_type = "";
        string f1 = "";
        string f2 = "";
        string f3 = "";
        string i1 = "";
        string i2 = "";
        string n1 = "";
        string n2 = "";
        string disp_order = "";
        string del_flag = "";
        string upd_date = "";

        public string Code { get => code; set => code = value; }
        public string Code_type { get => code_type; set => code_type = value; }
        public string F1 { get => f1; set => f1 = value; }
        public string F2 { get => f2; set => f2 = value; }
        public string F3 { get => f3; set => f3 = value; }
        public string I1 { get => i1; set => i1 = value; }
        public string I2 { get => i2; set => i2 = value; }
        public string N1 { get => n1; set => n1 = value; }
        public string N2 { get => n2; set => n2 = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
    }
}
