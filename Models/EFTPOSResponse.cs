using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNSelfOrder.Models
{
    public enum EFTPOSTransType { Purchase, Refund, Settlement, Balance }
    public enum EFTPOSLastTransStatus { Approved, Cancelled, Declined, Failed, OfflineOk, Unknown }
    public class EFTPOSResponse
    {
        private StringBuilder printerText;
        public EFTPOSTransType TransType { get; set; }
        public EFTPOSLastTransStatus Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string PrinterText { get; set; }
        public string ReceiptText { get; set; }
        public string AuthCode { get; set; }
        public string LastTransTxnRef { get; set; }
        public int ResponseType { get; set; }
        public bool TxnSucess { get; set; }
        public string AccountType { get; set; }
        public string CardType { get; set; }
        public string DateOfTxn { get; set; }
        public string CardName { get; set; }
        public int Stan { get; set; }
        public string TimeOfTxn { get; set; }
        public string DateExpiry { get; set; }
        public string Rrn { get; set; }
        public string PurchaseAnalysisData { get; set; }
        public string DataField { get; set; }
        public string Pan { get; set; }
        public string MessageType { get; set; }
        public decimal AmtPurchase { get; set; }
        public decimal AmtCash { get; set; }
        public bool Sucess { get; set; }
        public EFTPOSResponse()
        {
            this.printerText = new StringBuilder();

            this.ResponseCode = "";
            this.ResponseText = "";
            this.Status = EFTPOSLastTransStatus.Unknown;
            this.PrinterText = "";
            this.AuthCode = "";
            this.ResponseType = 0;
            this.TxnSucess = false;
            this.Sucess = false;
            this.AccountType = "";
            this.CardType = "";
            this.DateOfTxn = "";
            this.CardName = "";
            this.Stan = 0;
            this.TimeOfTxn = "";
            this.DateExpiry = "";
            this.Rrn = "";
            this.PurchaseAnalysisData = "";
            this.DataField = "";
            this.Pan = "";
            this.AmtCash = 0;
            this.AmtPurchase = 0;
        }

        public void FinaliseResponse()
        {
            this.PrinterText = this.printerText.ToString();
        }

        public void AppendPrintLine(string text)
        {
            this.printerText.AppendLine(text);
        }
    }
    public class EFTTranscation
    {
        public EFTPOSTransType TransType { get; set; }
        public string DealNo { get; set; }
        public int Merchant { get; set; }
        public string CurrencyCode { get; set; }
        public string TxnRef { get; set; }
        public bool CutReceipt { get; set; }
        public bool ReceiptAutoPrint { get; set; }
        public string PosProductId { get; set; }
        public string PurchaseAnalysisData { get; set; }
        public string DialogTitle { get; set; }
        public decimal AmtPurchase { get; set; }
        public decimal AmtCash { get; set; }
        public EFTTranscation()
        {
            this.Merchant = 0;
            this.PosProductId = "";
            this.CurrencyCode = "AUD";
            this.TxnRef = "";
            this.TransType = EFTPOSTransType.Purchase;
            this.ReceiptAutoPrint = false;
            this.CutReceipt = false;
            this.DialogTitle = "";
            this.PurchaseAnalysisData = "";
            this.AmtCash = 0;
            this.AmtPurchase = 0;
            this.DealNo = "";
        }
    }
}
