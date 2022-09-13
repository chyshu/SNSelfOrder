using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Interfaces
{
    public enum TimeOutStatus
    {
        None = 0,
        TimeOut = 1,
        OK = 2,
        CANCEL = 3,
        SHUTDOWN = 4,
        CLOSE_APP =5,
    }
    public interface IWindowsHelper
    {
        Task<bool> ShowMessage(string message);
        void ShowBusyMessage(string message);
        void HideBusyMessage();
        Task<bool> AskQuestion(string message, string okText = "Yes", string cancelText = "No");
    }

    public interface ICloseWindows
    {        
        public Action<TimeOutStatus> VMClose { get; set; }
        public bool CanClose();
    }

 
    public interface ITimeOut
    {
 
        int CountDown { get; set; }
        void StartIdleTimer();
        void StopIdleTimer();
        void ResetIdleTimer();
    }

    public interface IEndTranscation
    {
        Action<PRPos.Data.PSTrn01sClass> CloseTranscation { get; set; }
    }

    public interface IInputClose
    {
        Action<object> InputClose{ get; set; }
    }

    public interface IUpdateConnectionCode
    {
        Action<object> UpdateConnectionCode { get; set; }
    }

    public interface IOrderTranscation
    {
        Action<PRPos.Data.FastKeyClass , PRPos.Data.ItemVarietyClass , int, decimal,decimal> AddItem { get; set; }
        Action<PRPos.Data.FastKeyClass, PRPos.Data.ItemVarietyClass> RemoveItem { get; set; }
        Action<PRPos.Data.FastKeyClass, PRPos.Data.ItemVarietyClass, int , decimal, decimal> UpdateItem { get; set; }
    }

    public interface ISelfOrderPrinter
    {
        void SetPrinter(List<Models.PosPrinter>  printer);
        int PrintingReceipt(PRPos.Data.PSTrn01sClass currentdeal);

        int PrintingReceipt(PRPos.Data.PSTrn01sClass currentdeal, bool Reprint = false);
        int PrintingCardReceipt(string recepit);

        
    }

    public interface ISelfOrderLabelPrinter
    {        
        void SetPrinter(List<Models.PosPrinter> printer);
            
        int PrintingLabel(PRPos.Data.PSTrn01sClass currentdeal, decimal offSetX = 0, decimal offSetY = 0);
    }

    public interface ISelfOrderKitchenPrinter
    {
        void SetPrinter(List<Models.PosPrinter> printers);

        int PrintingKitchenOrder(PRPos.Data.PSTrn01sClass currentdeal);
    }
}
