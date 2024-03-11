using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Flurl.Http;
using Flurl.Http.Newtonsoft;
using Newtonsoft.Json;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Opayo.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api
{
    public class OpayoServerClient
    {
        private readonly ILogger<OpayoServerPaymentProvider> _logger;
        private readonly OpayoServerClientConfig _config;

        public OpayoServerClient(ILogger<OpayoServerPaymentProvider> logger, OpayoServerClientConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<Dictionary<string, string>> InitiateTransactionAsync(bool useTestMode, Dictionary<string, string> inputFields, CancellationToken cancellationToken = default)
        {
            var rawResponse = await MakePostRequestAsync(
                OpayoEndpoints.Get(inputFields[OpayoConstants.TransactionRequestFields.TransactionType], useTestMode),
                inputFields,
                cancellationToken)
                .ConfigureAwait(false);

            return GetFields(rawResponse);

        }

        public CallbackResult HandleCallback(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            switch (request.Status)
            {
                case OpayoConstants.CallbackRequest.Status.Abort:
                    return GenerateAbortedCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.Rejected:
                    return GenerateRejectedCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.Registered:
                case OpayoConstants.CallbackRequest.Status.Error:
                    return GenerateErrorCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.Pending:
                    return GeneratePendingCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.Ok:
                    return GenerateOkCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.NotAuthorised:
                    return GenerateNotAuthorisedCallbackResponse(order, request, settings);
                case OpayoConstants.CallbackRequest.Status.Authenticated:
                    return GenerateAuthenticatedCallbackResponse(order, request, settings);
                default:
                    return CallbackResult.Empty;
            }
        }

        private async Task<string> MakePostRequestAsync(string url, IDictionary<string, string> inputFields, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestContents = string.Empty;

                if (inputFields != null)
                {
                    requestContents = string.Join("&", (
                        from i in inputFields
                        select string.Format("{0}={1}", i.Key, HttpUtility.UrlEncode(i.Value))).ToArray<string>());
                }

                var request = new FlurlRequest(url)
                    .WithSettings(x => x.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }))
                    .SetQueryParams(inputFields, Flurl.NullValueHandling.Remove);

                return await request
                    .PostAsync(null, cancellationToken: cancellationToken)
                    .ReceiveString().ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                return string.Empty;
            }
        }

        private Dictionary<string, string> GetFields(string response)
        {
            return response.Split(
                Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .ToDictionary(
                    i => i.Substring(0, i.IndexOf("=", StringComparison.Ordinal)),
                    i => i.Substring(i.IndexOf("=", StringComparison.Ordinal) + 1, i.Length - (i.IndexOf("=", StringComparison.Ordinal) + 1)));
        }

        private CallbackResult GenerateOkCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction okay:\n\tOpayoTx: {VPSTxId}", request.VPSTxId);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                TransactionInfo = validSig
                    ? new TransactionInfo
                    {
                        TransactionId = request.VPSTxId,
                        AmountAuthorized = order.TransactionAmount.Value,
                        TransactionFee = request.Surcharge,
                        PaymentStatus = request.TxType == "PAYMENT" ? PaymentStatus.Captured : PaymentStatus.Authorized
                    }
                    : null,
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateOkCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                },
                MetaData = validSig
                    ? new Dictionary<string, string>
                        {
                            { OpayoConstants.OrderProperties.TransDetails, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits) },
                            { OpayoConstants.OrderProperties.TransDetailsHash, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits).ToMD5Hash() }
                        }
                    : null
            };
        }

        private CallbackResult GenerateAuthenticatedCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction Authenticated:\n\tOpayoTx: {VPSTxId}", request.VPSTxId);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateOkCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                },
                MetaData = validSig
                    ? new Dictionary<string, string>
                        {
                            { OpayoConstants.OrderProperties.TransDetails, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits) },
                            { OpayoConstants.OrderProperties.TransDetailsHash, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits).ToMD5Hash() }
                        }
                    : null
            };
        }

        private CallbackResult GenerateNotAuthorisedCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction not authorised:\n\tOpayoTx: {VPSTxId}", request.VPSTxId);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                TransactionInfo = validSig
                    ? new TransactionInfo
                    {
                        TransactionId = request.VPSTxId,
                        AmountAuthorized = 0,
                        TransactionFee = request.Surcharge,
                        PaymentStatus = PaymentStatus.Error
                    }
                    : null,
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateRejectedCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                },
                MetaData = validSig
                    ? new Dictionary<string, string>
                        {
                            { OpayoConstants.OrderProperties.TransDetails, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits) },
                            { OpayoConstants.OrderProperties.TransDetailsHash, string.Join(":", request.TxAuthNo, request.CardType, request.Last4Digits).ToMD5Hash() }
                        }
                    : null
            };
        }

        private CallbackResult GeneratePendingCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction pending:\n\tOpayoTx: {VPSTxId}", request.VPSTxId);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                TransactionInfo = validSig
                    ? new TransactionInfo
                    {
                        TransactionId = request.VPSTxId,
                        AmountAuthorized = order.TransactionAmount.Value,
                        TransactionFee = request.Surcharge,
                        PaymentStatus = PaymentStatus.PendingExternalSystem
                    }
                    : null,
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateOkCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                }
            };
        }

        private CallbackResult GenerateAbortedCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction aborted:\n\tOpayoTx: {VPSTxId}\n\tDetail: {StatusDetail}", request.VPSTxId, request.StatusDetail);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateAbortCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                }
            };
        }

        private CallbackResult GenerateRejectedCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction rejected:\n\tOpayoTx: {VPSTxId}\n\tDetail: {StatusDetail}", request.VPSTxId, request.StatusDetail);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateRejectedCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                }
            };
        }

        private CallbackResult GenerateErrorCallbackResponse(OrderReadOnly order, CallbackRequestModel request, OpayoSettings settings)
        {
            _logger.Warn("Payment transaction error:\n\tOpayoTx: {VPSTxId}\n\tDetail: {StatusDetail}", request.VPSTxId, request.StatusDetail);

            var validSig = ValidateVpsSigniture(order, request, settings);

            return new CallbackResult
            {
                HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = validSig
                        ? GenerateRejectedCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                }
            };
        }

        private bool ValidateVpsSigniture(OrderReadOnly order, CallbackRequestModel callbackRequest, OpayoSettings settings)
        {
            var md5Values = new List<string>
            {
                callbackRequest.VPSTxId,
                callbackRequest.VendorTxCode,
                callbackRequest.Status,
                callbackRequest.TxAuthNo,
                settings.VendorName.ToLowerInvariant(),
                callbackRequest.AVSCV2,
                order.Properties[OpayoConstants.OrderProperties.SecurityKey]?.Value,
                callbackRequest.AddressResult,
                callbackRequest.PostCodeResult,
                callbackRequest.CV2Result,
                callbackRequest.GiftAid,
                callbackRequest.SecureStatus,
                callbackRequest.CAVV,
                callbackRequest.AddressStatus,
                callbackRequest.PayerStatus,
                callbackRequest.CardType,
                callbackRequest.Last4Digits,
                callbackRequest.DeclineCode,
                callbackRequest.ExpiryDate,
                callbackRequest.FraudResponse,
                callbackRequest.BankAuthCode
            };

            string calcedMd5Hash = string.Join("", md5Values.Where(v => string.IsNullOrEmpty(v) == false)).ToMD5Hash().ToUpperInvariant();
            return callbackRequest.VPSSignature == calcedMd5Hash;
        }

        private HttpContent GenerateOkCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ContinueUrl}");
            return new StringContent(responseBody.ToString());
        }

        private HttpContent GenerateAbortCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.CancelUrl}");
            return new StringContent(responseBody.ToString());
        }

        private HttpContent GenerateRejectedCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ErrorUrl}");
            return new StringContent(responseBody.ToString());
        }

        private HttpContent GenerateInvalidCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Error}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ErrorUrl}");
            return new StringContent(responseBody.ToString());
        }

    }
}
