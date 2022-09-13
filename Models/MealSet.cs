using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class Combo
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string item_code = "";
        string variety_code = "";
        string mealset_code = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string description = "";
        string upd_date = "";
        string del_flag = "";
        string disp_order = "";
        string gst = "";
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
        public string Mealset_code { get => mealset_code; set => mealset_code = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Description { get => description; set => description = value; }
        public string Gst { get => gst; set => gst = value; }
    }
    public class MealSet
    {
        string sid = "";
        string customerid = "";
        string store_code = "";
        string mealset_code = "";
        string caption = "";
        string caption_fn = "";
        string description = "";
        string image = "";

        string ctype = "";
        string actived = "";
        string upd_date = "";
        string del_flag = "";
        List<MealSet_Course> mealSet_Course = new List<MealSet_Course>();
        string kitchen_name = "";
        string kitchen_name_fn = "";
        string print_on_kitchen = "";
        string gst = "";
        public string Sid { get => sid; set => sid = value; }
        public string Customerid { get => customerid; set => customerid = value; }
        public string Store_code { get => store_code; set => store_code = value; }
        public string Mealset_code { get => mealset_code; set => mealset_code = value; }
        public string Caption { get => caption; set => caption = value; }
        public string Caption_fn { get => caption_fn; set => caption_fn = value; }
        public string Image { get => image; set => image = value; }

        public string Ctype { get => ctype; set => ctype = value; }
        public string Actived { get => actived; set => actived = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public List<MealSet_Course> MealSet_Course { get => mealSet_Course; set => mealSet_Course = value; }
        public string Description { get => description; set => description = value; }
        public string Kitchen_name { get => kitchen_name; set => kitchen_name = value; }
        public string Kitchen_name_fn { get => kitchen_name_fn; set => kitchen_name_fn = value; }
        public string Print_on_kitchen { get => print_on_kitchen; set => print_on_kitchen = value; }
        public string Gst { get => gst; set => gst = value; }
    }
    public class MealSet_Course
    {
        string sid = "";
        string psid = "";
        string course_name = "";
        string course_name_fn = "";
        string min_selection = "";
        string max_selection = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string is_enabled = "";
        string upd_date = "";
        string del_flag = "";
        string disp_order = "";
        List<MealSet_Course_Item> mealSet_Course_Item = new List<MealSet_Course_Item>();
        public string Sid { get => sid; set => sid = value; }
        public string Psid { get => psid; set => psid = value; }
        public string Course_name { get => course_name; set => course_name = value; }
        public string Course_name_fn { get => course_name_fn; set => course_name_fn = value; }
        public string Min_selection { get => min_selection; set => min_selection = value; }
        public string Max_selection { get => max_selection; set => max_selection = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Is_enabled { get => is_enabled; set => is_enabled = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public List<MealSet_Course_Item> MealSet_Course_Item { get => mealSet_Course_Item; set => mealSet_Course_Item = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
    }
    public class MealSet_Course_Item
    {
        string sid = "";
        string psid = "";
        string item_code = "";
        string variety_code = "";
        string sprice1 = "";
        string sprice2 = "";
        string sprice3 = "";
        string sprice4 = "";
        string sprice5 = "";
        string sprice6 = "";
        string sprice7 = "";
        string sprice8 = "";
        string sprice9 = "";
        string sprice10 = "";
        string is_enabled = "";
        string upd_date = "";
        string del_flag = "";
        string disp_order = "";
        public string Sid { get => sid; set => sid = value; }
        public string Psid { get => psid; set => psid = value; }
        public string Item_code { get => item_code; set => item_code = value; }
        public string Sprice1 { get => sprice1; set => sprice1 = value; }
        public string Sprice2 { get => sprice2; set => sprice2 = value; }
        public string Sprice3 { get => sprice3; set => sprice3 = value; }
        public string Sprice4 { get => sprice4; set => sprice4 = value; }
        public string Sprice5 { get => sprice5; set => sprice5 = value; }
        public string Sprice6 { get => sprice6; set => sprice6 = value; }
        public string Sprice7 { get => sprice7; set => sprice7 = value; }
        public string Sprice8 { get => sprice8; set => sprice8 = value; }
        public string Sprice9 { get => sprice9; set => sprice9 = value; }
        public string Sprice10 { get => sprice10; set => sprice10 = value; }
        public string Is_enabled { get => is_enabled; set => is_enabled = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Variety_code { get => variety_code; set => variety_code = value; }
    }
}
