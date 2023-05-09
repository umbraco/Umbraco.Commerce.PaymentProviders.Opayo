using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api.Models
{
    public class CallbackRequestModel
    {
        public CallbackRequestModel(HttpRequestMessage request)
        {
            RawRequest = request;
        }

        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public string VPSTxId { get; set; }
        public string VPSProtocol { get; set; }
        public string TxType { get; set; }
        public string GiftAid { get; set; }
        public string VPSSignature { get; set; }
        public string VendorTxCode { get; set; }
        public string TxAuthNo { get; set; }
        public string AVSCV2 { get; set; }
        public string AddressResult { get; set; }
        public string AddressStatus { get; set; }
        public string PostCodeResult { get; set; }
        public string CV2Result { get; set; }
        public string SecureStatus {get;set;}
        public string CAVV { get; set; }
        public string PayerStatus { get; set; }
        public string CardType { get; set; }
        public string Last4Digits { get; set; }
        public string DeclineCode { get; set; }
        public string ExpiryDate { get; set; }
        public string FraudResponse { get; set; }
        public string BankAuthCode { get; set; }
        public decimal? Surcharge { get; set; }
        public HttpRequestMessage RawRequest { get; }

        public static async Task<CallbackRequestModel> FromRequestAsync(HttpRequestMessage request)
        {
            var formData = await request.Content.ReadAsFormDataAsync();

            return new CallbackRequestModel(request)
            {
                Status = formData.Get(nameof(Status)),
                StatusDetail = formData.Get(nameof(StatusDetail)),
                GiftAid = formData.Get(nameof(GiftAid)),
                TxType = formData.Get(nameof(TxType)),
                VendorTxCode = formData.Get(nameof(VendorTxCode)),
                VPSProtocol = formData.Get(nameof(VPSProtocol)),
                VPSSignature = formData.Get(nameof(VPSSignature)),
                VPSTxId = formData.Get(nameof(VPSTxId)),
                TxAuthNo = formData.Get(nameof(TxAuthNo)),
                AVSCV2 = HttpUtility.UrlDecode(formData.Get(nameof(AVSCV2))),
                AddressResult = HttpUtility.UrlDecode(formData.Get(nameof(AddressResult))),
                AddressStatus = HttpUtility.UrlDecode(formData.Get(nameof(AddressStatus))),
                PostCodeResult = HttpUtility.UrlDecode(formData.Get(nameof(PostCodeResult))),
                CV2Result = HttpUtility.UrlDecode(formData.Get(nameof(CV2Result))),
                SecureStatus = formData.Get("3DSecureStatus"),
                CAVV = formData.Get(nameof(CAVV)),
                PayerStatus = HttpUtility.UrlDecode(formData.Get(nameof(PayerStatus))),
                CardType = formData.Get(nameof(CardType)),
                Last4Digits = formData.Get(nameof(Last4Digits)),
                DeclineCode = HttpUtility.UrlDecode(formData.Get(nameof(DeclineCode))),
                ExpiryDate = HttpUtility.UrlDecode(formData.Get(nameof(ExpiryDate))),
                FraudResponse = HttpUtility.UrlDecode(formData.Get(nameof(FraudResponse))),
                BankAuthCode = HttpUtility.UrlDecode(formData.Get(nameof(BankAuthCode))),
                Surcharge = formData.AllKeys.Any(k => k.Equals(nameof(Surcharge))) ? decimal.Parse(formData.Get(nameof(Surcharge))) : decimal.Zero
            };
        }
    }
}
