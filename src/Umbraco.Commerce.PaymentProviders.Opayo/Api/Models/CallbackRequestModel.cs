using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api.Models
{
    public class CallbackRequestModel
    {
        public CallbackRequestModel(HttpRequest request)
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
        public string SecureStatus { get; set; }
        public string CAVV { get; set; }
        public string PayerStatus { get; set; }
        public string CardType { get; set; }
        public string Last4Digits { get; set; }
        public string DeclineCode { get; set; }
        public string ExpiryDate { get; set; }
        public string FraudResponse { get; set; }
        public string BankAuthCode { get; set; }
        public decimal? Surcharge { get; set; }
        public HttpRequest RawRequest { get; }

        public static async Task<CallbackRequestModel> FromRequestAsync(HttpRequest request)
        {
            IFormCollection formData = await request.ReadFormAsync().ConfigureAwait(false);

            return new CallbackRequestModel(request)
            {
                Status = formData[nameof(Status)],
                StatusDetail = formData[nameof(StatusDetail)],
                GiftAid = formData[nameof(GiftAid)],
                TxType = formData[nameof(TxType)],
                VendorTxCode = formData[nameof(VendorTxCode)],
                VPSProtocol = formData[nameof(VPSProtocol)],
                VPSSignature = formData[nameof(VPSSignature)],
                VPSTxId = formData[nameof(VPSTxId)],
                TxAuthNo = formData[nameof(TxAuthNo)],
                AVSCV2 = HttpUtility.UrlDecode(formData[nameof(AVSCV2)]),
                AddressResult = HttpUtility.UrlDecode(formData[nameof(AddressResult)]),
                AddressStatus = HttpUtility.UrlDecode(formData[nameof(AddressStatus)]),
                PostCodeResult = HttpUtility.UrlDecode(formData[nameof(PostCodeResult)]),
                CV2Result = HttpUtility.UrlDecode(formData[nameof(CV2Result)]),
                SecureStatus = formData["3DSecureStatus"],
                CAVV = formData[nameof(CAVV)],
                PayerStatus = HttpUtility.UrlDecode(formData[nameof(PayerStatus)]),
                CardType = formData[nameof(CardType)],
                Last4Digits = formData[nameof(Last4Digits)],
                DeclineCode = HttpUtility.UrlDecode(formData[nameof(DeclineCode)]),
                ExpiryDate = HttpUtility.UrlDecode(formData[nameof(ExpiryDate)]),
                FraudResponse = HttpUtility.UrlDecode(formData[nameof(FraudResponse)]),
                BankAuthCode = HttpUtility.UrlDecode(formData[nameof(BankAuthCode)]),
                Surcharge = formData.Keys.Any(k => k.Equals(nameof(Surcharge), StringComparison.OrdinalIgnoreCase)) ? decimal.Parse(formData[nameof(Surcharge)], CultureInfo.InvariantCulture) : decimal.Zero,
            };
        }
    }
}
