using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
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
                // Opayo Server takes the registration fields as a URL encoded form body.
                var formFields = (inputFields ?? new Dictionary<string, string>())
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Key, x => x.Value);

                var request = new FlurlRequest(url)
                    .WithSettings(x => x.JsonSerializer = new CustomFlurlJsonSerializer(new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
                    }));

                return await request
                    .PostUrlEncodedAsync(formFields, cancellationToken: cancellationToken)
                    .ReceiveString().ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                // Logs the endpoint and status only: the request fields hold customer details.
                _logger.Error(
                    "Opayo request to {Endpoint} failed with status {StatusCode}: {ErrorType}",
                    GetEndpointForLog(url),
                    ex.StatusCode,
                    ex.InnerException?.GetType().Name ?? ex.GetType().Name);

                return string.Empty;
            }
        }

        private static string GetEndpointForLog(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out Uri uri)
                ? uri.GetLeftPart(UriPartial.Path)
                : "(unknown)";

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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
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
                ActionResult = new ContentResult
                {
                    StatusCode = 200,
                    Content = validSig
                        ? GenerateRejectedCallbackResponseBody()
                        : GenerateInvalidCallbackResponseBody()
                }
            };
        }

        private bool ValidateVpsSigniture(OrderReadOnly order, CallbackRequestModel callbackRequest, OpayoSettings settings)
        {
            string securityKey = order.Properties[OpayoConstants.OrderProperties.SecurityKey]?.Value;

            if (string.IsNullOrEmpty(securityKey))
            {
                _logger.Warn(
                    "Opayo notification for order {OrderNumber} refused: the order has no stored Opayo security key, so the signature can't be checked.\n\tOpayoTx: {VPSTxId}",
                    order.OrderNumber ?? order.CartNumber,
                    callbackRequest.VPSTxId);
                return false;
            }

            if (!IsValidVpsSignature(callbackRequest, settings.VendorName, securityKey))
            {
                _logger.Warn(
                    "Opayo notification for order {OrderNumber} refused: the VPSSignature doesn't match the notification (check the Vendor Name setting if this happens for every order).\n\tOpayoTx: {VPSTxId}",
                    order.OrderNumber ?? order.CartNumber,
                    callbackRequest.VPSTxId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the VPSSignature Opayo sends with a notification: an upper case MD5 of the
        /// notification fields, the vendor name and the transaction's security key, in Opayo's order.
        /// A transaction without a security key never validates.
        /// </summary>
        internal static bool IsValidVpsSignature(CallbackRequestModel callbackRequest, string vendorName, string securityKey)
        {
            if (string.IsNullOrEmpty(securityKey) || string.IsNullOrEmpty(callbackRequest.VPSSignature))
            {
                return false;
            }

            // Hex digest, so compare without regard to case, in constant time.
            byte[] expected = Encoding.ASCII.GetBytes(ComputeVpsSignature(callbackRequest, vendorName, securityKey));
            byte[] actual = Encoding.ASCII.GetBytes(callbackRequest.VPSSignature.Trim().ToUpperInvariant());

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        internal static string ComputeVpsSignature(CallbackRequestModel callbackRequest, string vendorName, string securityKey)
        {
            var md5Values = new List<string>
            {
                callbackRequest.VPSTxId,
                callbackRequest.VendorTxCode,
                callbackRequest.Status,
                callbackRequest.TxAuthNo,
                vendorName.ToLowerInvariant(),
                callbackRequest.AVSCV2,
                securityKey,
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

            return string.Join("", md5Values.Where(v => string.IsNullOrEmpty(v) == false)).ToMD5Hash().ToUpperInvariant();
        }

        private string GenerateOkCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ContinueUrl}");
            return responseBody.ToString();
        }

        private string GenerateAbortCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.CancelUrl}");
            return responseBody.ToString();
        }

        private string GenerateRejectedCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Ok}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ErrorUrl}");
            return responseBody.ToString();
        }

        private string GenerateInvalidCallbackResponseBody()
        {
            var responseBody = new StringBuilder();
            responseBody.AppendLine($"{OpayoConstants.Response.Status}={OpayoConstants.Response.StatusCodes.Error}");
            responseBody.AppendLine($"{OpayoConstants.Response.RedirectUrl}={_config.ErrorUrl}");
            return responseBody.ToString();
        }

    }
}
