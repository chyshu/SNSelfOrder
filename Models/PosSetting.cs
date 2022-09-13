using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder
{
    public interface IPOSSetting
    {
        public void SetPos(SelfOrderSettingClass setting);
    }
    public class SelfOrderSettingClass
    {
        private string customerID = "";
        private string storeCode = "";
        private string posCode = "";
        private string filePath = "";
        private string token = "";
        private string connectionCode = "";
        private string hostURL = "";
        private string mAC = "";
        private string iPV4 = "";
        private string connString = "";
        private SecurityController _security;

        public string Token { get => token; set => token = value; }
        public string ConnectionCode { get => connectionCode; set => connectionCode = value; }
        public string HostURL { get => hostURL; set => hostURL = value; }
        public string MAC { get => mAC; set => mAC = value; }
        public string ConnString { get => connString; set => connString = value; }
        public string IPV4 { get => iPV4; set => iPV4 = value; }
        public SecurityController Security { get => _security; set => _security = value; }
        public string CustomerID { get => customerID; set => customerID = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public string StoreCode { get => storeCode; set => storeCode = value; }
        public string PosCode { get => posCode; set => posCode = value; }
    }
    public class PosData
    {
        string token = "";
        string dataType = "";
        string mAC = "";
        string code = "";

        string retcode = "";
        string message = "";
        string retvalue = "";
        string itemValue = "";

        string modifierValue = "";
        string modifierSetValue = "";
        string itemModValue = "";

        string settingValue = "";
        string printersettingValue = "";

        string promotionsValue = "";
        string itemVarietyValue = "";
        string itemSizeValue = "";

        string pos_code = "";
        string posID = "";

        string salePriceColumn = "";
        string takeawayPriceColumn = "";
        string uBereatPriceColumn = "";
        string phoneOrderPriceColumn = "";

        string default_order = "";
        string paymentValue = "";
        string markupItemValue = "";
        string serviceChargeValue = "";
        string mealSetValue = "";
        string comboValue = "";
        string psCategoryValue = "";

        public string MAC { get => mAC; set => mAC = value; }
        public string DataType { get => dataType; set => dataType = value; }
        public string Token { get => token; set => token = value; }
        public string Message { get => message; set => message = value; }
        public string Retcode { get => retcode; set => retcode = value; }
        public string Retvalue { get => retvalue; set => retvalue = value; }
        public string Code { get => code; set => code = value; }
        public string PosID { get => posID; set => posID = value; }
        public string Pos_code { get => pos_code; set => pos_code = value; }
        public string ItemValue { get => itemValue; set => itemValue = value; }
        public string SettingValue { get => settingValue; set => settingValue = value; }
        public string PrinterSettingValue { get => printersettingValue; set => printersettingValue = value; }
        public string ModifierValue { get => modifierValue; set => modifierValue = value; }
        public string ModSetValue { get => modifierSetValue; set => modifierSetValue = value; }
        public string ItemModValue { get => itemModValue; set => itemModValue = value; }
        public string PromotionsValue { get => promotionsValue; set => promotionsValue = value; }
        public string ItemVarietyValue { get => itemVarietyValue; set => itemVarietyValue = value; }
        public string ItemSizeValue { get => itemSizeValue; set => itemSizeValue = value; }
        public string SalePriceColumn { get => salePriceColumn; set => salePriceColumn = value; }
        public string TakeawayPriceColumn { get => takeawayPriceColumn; set => takeawayPriceColumn = value; }
        public string UBereatPriceColumn { get => uBereatPriceColumn; set => uBereatPriceColumn = value; }
        public string PhoneOrderPriceColumn { get => phoneOrderPriceColumn; set => phoneOrderPriceColumn = value; }
        public string Default_order { get => default_order; set => default_order = value; }
        public string PaymentValue { get => paymentValue; set => paymentValue = value; }
        public string MarkupItemValue { get => markupItemValue; set => markupItemValue = value; }
        public string ServiceChargeValue { get => serviceChargeValue; set => serviceChargeValue = value; }
        public string MealSetValue { get => mealSetValue; set => mealSetValue = value; }
        public string ComboValue { get => comboValue; set => comboValue = value; }
        public string PsCategoryValue { get => psCategoryValue; set => psCategoryValue = value; }
    }

    public class PosDataV3
    {
        string token = "";
        string dataType = "";
        string mAC = "";
        string code = "";

        string retcode = "";
        string message = "";
        string retvalue = "";
        string itemValue = "";

        string modifierValue = "";
        string modifierSetValue = "";
        string itemModValue = "";

        string settingValue = "";
        string printersettingValue = "";

        string promotionsValue = "";
        string itemVarietyValue = "";
        string itemSizeValue = "";

        string pos_code = "";
        string posID = "";
        string salePriceColumn = "";
        string takeawayPriceColumn = "";
        string uBereatPriceColumn = "";
        string phoneOrderPriceColumn = "";

        string default_order = "";
        string paymentValue = "";
        string markupItemValue = "";
        string serviceChargeValue = "";
        string mealSetValue = "";
        string comboValue = "";
        string psCategoryValue = "";
        string parametersValue = "";

        public string MAC { get => mAC; set => mAC = value; }
        public string DataType { get => dataType; set => dataType = value; }
        public string Token { get => token; set => token = value; }
        public string Message { get => message; set => message = value; }
        public string Retcode { get => retcode; set => retcode = value; }
        public string Retvalue { get => retvalue; set => retvalue = value; }
        public string Code { get => code; set => code = value; }
        public string PosID { get => posID; set => posID = value; }
        public string Pos_code { get => pos_code; set => pos_code = value; }
        public string ItemValue { get => itemValue; set => itemValue = value; }
        public string SettingValue { get => settingValue; set => settingValue = value; }
        public string PrinterSettingValue { get => printersettingValue; set => printersettingValue = value; }
        public string ModifierValue { get => modifierValue; set => modifierValue = value; }
        public string ModSetValue { get => modifierSetValue; set => modifierSetValue = value; }
        public string ItemModValue { get => itemModValue; set => itemModValue = value; }
        public string PromotionsValue { get => promotionsValue; set => promotionsValue = value; }
        public string ItemVarietyValue { get => itemVarietyValue; set => itemVarietyValue = value; }
        public string ItemSizeValue { get => itemSizeValue; set => itemSizeValue = value; }
        public string SalePriceColumn { get => salePriceColumn; set => salePriceColumn = value; }
        public string TakeawayPriceColumn { get => takeawayPriceColumn; set => takeawayPriceColumn = value; }
        public string UBereatPriceColumn { get => uBereatPriceColumn; set => uBereatPriceColumn = value; }
        public string PhoneOrderPriceColumn { get => phoneOrderPriceColumn; set => phoneOrderPriceColumn = value; }
        public string Default_order { get => default_order; set => default_order = value; }
        public string PaymentValue { get => paymentValue; set => paymentValue = value; }
        public string MarkupItemValue { get => markupItemValue; set => markupItemValue = value; }
        public string ServiceChargeValue { get => serviceChargeValue; set => serviceChargeValue = value; }
        public string MealSetValue { get => mealSetValue; set => mealSetValue = value; }
        public string ComboValue { get => comboValue; set => comboValue = value; }
        public string PsCategoryValue { get => psCategoryValue; set => psCategoryValue = value; }
        public string ParametersValue { get => parametersValue; set => parametersValue = value; }
    }
    public class UploadDataClass
    {
        string token = "";

        string mAC = "";
        string code = "";

        string retcode = "";
        string message = "";
        string retvalue = "";
        string uplDataValue = "";

        public string Token { get => token; set => token = value; }
        public string MAC { get => mAC; set => mAC = value; }
        public string Code { get => code; set => code = value; }
        public string Retcode { get => retcode; set => retcode = value; }
        public string Message { get => message; set => message = value; }
        public string Retvalue { get => retvalue; set => retvalue = value; }
        public string UplDataValue { get => uplDataValue; set => uplDataValue = value; }
    }
}
