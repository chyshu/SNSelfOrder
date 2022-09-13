using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public class TerminalSetting
    {
        string gen_tickno = "";
        string reset_tickno = "";
        string ask_ordertype = "";
        string ask_tableno = "";
        string ask_cover = "";
        string ask_member_card = "";
        string order_time_out = "";
        string message_time_out = "";
        string keep_local_file = "";
        string spool_folder = "";
        string local_file_folder = "";
        string card_charge = "";
        string card_charge_item = "";
        string card_charge_rate = "";
        string table_service_charge = "";
        string table_service_charge_item = "";
        string table_service_charge_rate = "";
        string holiday_charge = "";
        string holiday_charge_item = "";
        string holiday_charge_rate = "";
        string plasticbag_item = "";
        string bag_Message = "";
        string markup_item = "";
        string markup_Message = "";
        string paycount_tender = "";
        string default_tender = "";
        List<PageImage> pageImages = new List<PageImage>();
        List<BannerImage> bannerImages = new List<BannerImage>();
        List<ButtonImage> buttonImages = new List<ButtonImage>();
        public List<PageImage> PageImages { get => pageImages; set => pageImages = value; }
        public List<BannerImage> BannerImages { get => bannerImages; set => bannerImages = value; }
        public List<ButtonImage> ButtonImages { get => buttonImages; set => buttonImages = value; }
        public string Gen_tickno { get => gen_tickno; set => gen_tickno = value; }
        public string Reset_tickno { get => reset_tickno; set => reset_tickno = value; }
        public string Ask_ordertype { get => ask_ordertype; set => ask_ordertype = value; }
        public string Ask_tableno { get => ask_tableno; set => ask_tableno = value; }
        public string Ask_cover { get => ask_cover; set => ask_cover = value; }
        public string Ask_member_card { get => ask_member_card; set => ask_member_card = value; }
        public string Order_time_out { get => order_time_out; set => order_time_out = value; }
        public string Message_time_out { get => message_time_out; set => message_time_out = value; }
        public string Keep_local_file { get => keep_local_file; set => keep_local_file = value; }
        public string Local_file_folder { get => local_file_folder; set => local_file_folder = value; }
        public string Card_charge_item { get => card_charge_item; set => card_charge_item = value; }
        public string Card_charge_rate { get => card_charge_rate; set => card_charge_rate = value; }
        public string Table_service_charge_item { get => table_service_charge_item; set => table_service_charge_item = value; }
        public string Table_service_charge_rate { get => table_service_charge_rate; set => table_service_charge_rate = value; }
        public string Holiday_charge_item { get => holiday_charge_item; set => holiday_charge_item = value; }
        public string Holiday_charge_rate { get => holiday_charge_rate; set => holiday_charge_rate = value; }
        public string Plasticbag_item { get => plasticbag_item; set => plasticbag_item = value; }
        public string Markup_item { get => markup_item; set => markup_item = value; }
        public string Paycount_tender { get => paycount_tender; set => paycount_tender = value; }
        public string Default_tender { get => default_tender; set => default_tender = value; }
        public string Card_charge { get => card_charge; set => card_charge = value; }
        public string Table_service_charge { get => table_service_charge; set => table_service_charge = value; }
        public string Holiday_charge { get => holiday_charge; set => holiday_charge = value; }
        public string Bag_Message { get => bag_Message; set => bag_Message = value; }
        public string Markup_Message { get => markup_Message; set => markup_Message = value; }
        public string Spool_folder { get => spool_folder; set => spool_folder = value; }
    }

    public class TerminalSettingV3
    {
        string gen_tickno = "";
        string reset_tickno = "";
        string ask_ordertype = "";
        string ask_tableno = "";
        string ask_cover = "";
        string ask_member_card = "";
        string order_time_out = "";
        string message_time_out = "";
        string keep_local_file = "";
        string local_file_folder = "";
        string spool_folder = "";
        string card_charge = "";
        string card_charge_item = "";
        string card_charge_rate = "";
        string table_service_charge = "";
        string table_service_charge_item = "";
        string table_service_charge_rate = "";
        string holiday_charge = "";
        string holiday_charge_item = "";
        string holiday_charge_rate = "";
        string plasticbag_item = "";
        string bag_Message = "";
        string markup_item = "";
        string markup_Message = "";
        string paycount_tender = "";
        string default_tender = "";
        List<PageImage> pageImages = new List<PageImage>();
        List<BannerImage> bannerImages = new List<BannerImage>();
        List<ButtonImage> buttonImages = new List<ButtonImage>();
        List<ParameterSetting> parameters = new List<ParameterSetting>();
        public List<ParameterSetting> Parameters { get => parameters; set => parameters = value; }
        public List<PageImage> PageImages { get => pageImages; set => pageImages = value; }
        public List<BannerImage> BannerImages { get => bannerImages; set => bannerImages = value; }
        public List<ButtonImage> ButtonImages { get => buttonImages; set => buttonImages = value; }
        public string Gen_tickno { get => gen_tickno; set => gen_tickno = value; }
        public string Reset_tickno { get => reset_tickno; set => reset_tickno = value; }
        public string Ask_ordertype { get => ask_ordertype; set => ask_ordertype = value; }
        public string Ask_tableno { get => ask_tableno; set => ask_tableno = value; }
        public string Ask_cover { get => ask_cover; set => ask_cover = value; }
        public string Ask_member_card { get => ask_member_card; set => ask_member_card = value; }
        public string Order_time_out { get => order_time_out; set => order_time_out = value; }
        public string Message_time_out { get => message_time_out; set => message_time_out = value; }
        public string Keep_local_file { get => keep_local_file; set => keep_local_file = value; }
        public string Local_file_folder { get => local_file_folder; set => local_file_folder = value; }
        public string Card_charge_item { get => card_charge_item; set => card_charge_item = value; }
        public string Card_charge_rate { get => card_charge_rate; set => card_charge_rate = value; }
        public string Table_service_charge_item { get => table_service_charge_item; set => table_service_charge_item = value; }
        public string Table_service_charge_rate { get => table_service_charge_rate; set => table_service_charge_rate = value; }
        public string Holiday_charge_item { get => holiday_charge_item; set => holiday_charge_item = value; }
        public string Holiday_charge_rate { get => holiday_charge_rate; set => holiday_charge_rate = value; }
        public string Plasticbag_item { get => plasticbag_item; set => plasticbag_item = value; }
        public string Markup_item { get => markup_item; set => markup_item = value; }
        public string Paycount_tender { get => paycount_tender; set => paycount_tender = value; }
        public string Default_tender { get => default_tender; set => default_tender = value; }
        public string Card_charge { get => card_charge; set => card_charge = value; }
        public string Table_service_charge { get => table_service_charge; set => table_service_charge = value; }
        public string Holiday_charge { get => holiday_charge; set => holiday_charge = value; }
        public string Bag_Message { get => bag_Message; set => bag_Message = value; }
        public string Markup_Message { get => markup_Message; set => markup_Message = value; }
        public string Spool_folder { get => spool_folder; set => spool_folder = value; }

    }
    public class PageImage
    {
        string pageName = "";
        string imagefile = "";
        string upd_date = "";
        public string PageName { get => pageName; set => pageName = value; }
        public string Imagefile { get => imagefile; set => imagefile = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
    }
    public class BannerImage
    {
        string pageName = "";
        string imagefile = "";
        string upd_date = "";
        string disp_order = "";
        string disp_delay = "";
        string del_flag = "";
        public string PageName { get => pageName; set => pageName = value; }
        public string Imagefile { get => imagefile; set => imagefile = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Disp_delay { get => disp_delay; set => disp_delay = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
    }
    public class ButtonImage
    {
        string buttonName = "";
        string imagefile = "";
        string upd_date = "";
        string disp_order = "";
        string disp_delay = "";
        string del_flag = "";
        public string ButtonName { get => buttonName; set => buttonName = value; }
        public string Imagefile { get => imagefile; set => imagefile = value; }
        public string Upd_date { get => upd_date; set => upd_date = value; }
        public string Disp_order { get => disp_order; set => disp_order = value; }
        public string Disp_delay { get => disp_delay; set => disp_delay = value; }
        public string Del_flag { get => del_flag; set => del_flag = value; }
    }
}
