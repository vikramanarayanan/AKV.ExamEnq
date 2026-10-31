using System.Collections.Generic;

namespace AKV.ExamEnq.Models
{
    public class AffAppPaperVm
    {
        public string sem_no { get; set; }
        public string pap_no { get; set; }
        public string papname { get; set; }
        public int papfee { get; set; }
    }

    public class AffAppReceiptLinkVm
    {
        public string refno { get; set; }
        public string url { get; set; }
    }

    public class AffAppSearchResultVm
    {
        public bool alreadyPaid { get; set; }
        public string error { get; set; }
        public string coursecode { get; set; }
        public string coursename { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public string name { get; set; }
        public string regno { get; set; }
        public string rollno { get; set; }
        public string sem_no { get; set; }
        public decimal demand { get; set; }
        public bool paymentAllowed { get; set; }
        public int fine { get; set; }
        public string photo1 { get; set; }
        public List<AffAppPaperVm> papers { get; set; }
        public List<AffAppReceiptLinkVm> receipts { get; set; }
    }

    public class AffAppYearVm
    {
        public string mthyr { get; set; }
        public bool showPayment { get; set; }
        public int fine { get; set; }
    }

    public class AffAppStatusVm
    {
        public string mthyr { get; set; }
        public int rollWithPapers { get; set; }
        public decimal affYearSuccessAmount { get; set; }
        public int affYearSuccess { get; set; }
        public int hallTicketPaid { get; set; }
    }

    public class AffAppPayRequestVm
    {
        public string Regno { get; set; }
    }

    public class AffAppRedirectVm
    {
        public string BankUrl { get; set; }
        public string PlainUrl { get; set; }
        public string EncryptedUrl { get; set; }
        public string MandatoryPlain { get; set; }
        public bool AutoSubmit { get; set; }
        public EazypayService.GatewayForm Form { get; set; }
    }

    public class AffAppReceiptVm
    {
        public string refno { get; set; }
        public string stname { get; set; }
        public string regno { get; set; }
        public string uniquerefnumber { get; set; }
        public string transactiondate { get; set; }
        public string transactionamount { get; set; }
        public string bankstatus { get; set; }
        public bool isSuccess { get; set; }
        public bool isPending { get; set; }
        public string message { get; set; }
        public string responseCode { get; set; }
    }

    public class AffAppBankReturnVm
    {
        public string responseCode { get; set; }
        public string responseMeaning { get; set; }
        public string refno { get; set; }
        public string bankStatus { get; set; }
        public string httpMethod { get; set; }
        public string requestUrl { get; set; }
        public string referrer { get; set; }
        public string rawQuery { get; set; }
        public string rawBody { get; set; }
        public string exception { get; set; }
        public string sentMandatory { get; set; }
        public string sentPlainUrl { get; set; }
        public string sentEncUrl { get; set; }
        public List<KeyValuePair<string, string>> fields { get; set; }
    }
}
