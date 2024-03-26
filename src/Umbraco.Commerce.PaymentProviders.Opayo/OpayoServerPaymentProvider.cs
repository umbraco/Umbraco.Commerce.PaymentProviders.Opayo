using System;
using System.Collections.Generic;
using System.Text.Json;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.PaymentProviders.Opayo.Api;
using Umbraco.Commerce.PaymentProviders.Opayo.Api.Models;
using Umbraco.Commerce.Common.Logging;
using System.Threading.Tasks;
using Umbraco.Commerce.Extensions;
using System.Threading;

namespace Umbraco.Commerce.PaymentProviders.Opayo
{
    [PaymentProvider("opayo-server", "Opayo Server", "Opayo Server payment provider", Icon = "icon-credit-card")]
    public class OpayoServerPaymentProvider : PaymentProviderBase<OpayoSettings>
    {
        private readonly ILogger<OpayoServerPaymentProvider> _logger;

        public OpayoServerPaymentProvider(UmbracoCommerceContext ctx, ILogger<OpayoServerPaymentProvider> logger)
            : base(ctx)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override string GetCancelUrl(PaymentProviderContext<OpayoSettings> ctx)
        {
            ctx.Settings.MustNotBeNull(nameof(ctx.Settings));
            ctx.Settings.CancelUrl.MustNotBeNullOrWhiteSpace(nameof(ctx.Settings.CancelUrl));

            return ctx.Settings.CancelUrl.ReplacePlaceHolders(ctx.Order);
        }

        public override string GetErrorUrl(PaymentProviderContext<OpayoSettings> ctx)
        {
            ctx.Settings.MustNotBeNull(nameof(ctx.Settings));
            ctx.Settings.ErrorUrl.MustNotBeNullOrWhiteSpace(nameof(ctx.Settings.ErrorUrl));

            return ctx.Settings.ErrorUrl.ReplacePlaceHolders(ctx.Order);
        }

        public override string GetContinueUrl(PaymentProviderContext<OpayoSettings> ctx)
        {
            ctx.Settings.MustNotBeNull(nameof(ctx.Settings));
            ctx.Settings.ContinueUrl.MustNotBeNullOrWhiteSpace(nameof(ctx.Settings.ContinueUrl));

            return ctx.Settings.ContinueUrl.ReplacePlaceHolders(ctx.Order);
        }

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<OpayoSettings> ctx, CancellationToken cancellationToken = default)
        {
            var form = new PaymentForm(ctx.Urls.CancelUrl, PaymentFormMethod.Post);
            var client = new OpayoServerClient(_logger, new OpayoServerClientConfig
            {
                ContinueUrl = ctx.Urls.ContinueUrl,
                CancelUrl = ctx.Urls.CancelUrl,
                ErrorUrl = ctx.Urls.ErrorUrl,
                ProviderAlias = Alias
            });

            var inputFields = OpayoInputLoader.LoadInputs(ctx.Order, ctx.Settings, Context, ctx.Urls.CallbackUrl);
            _logger.Info($"Opayo ({ctx.Order.OrderNumber}) request: {JsonSerializer.Serialize(inputFields)}");

            var responseDetails = await client.InitiateTransactionAsync(ctx.Settings.TestMode, inputFields, cancellationToken).ConfigureAwait(false);
            _logger.Info($"Opayo ({ctx.Order.OrderNumber}) response: {JsonSerializer.Serialize(responseDetails)}");

            var status = responseDetails[OpayoConstants.Response.Status];

            Dictionary<string, string> orderMetaData = null;

            if (status == OpayoConstants.Response.StatusCodes.Ok || status == OpayoConstants.Response.StatusCodes.Repeated)
            {
                orderMetaData = new Dictionary<string, string>
                {
                    { OpayoConstants.OrderProperties.SecurityKey, responseDetails[OpayoConstants.Response.SecurityKey] },
                    { OpayoConstants.OrderProperties.TransactionId, responseDetails[OpayoConstants.Response.TransactionId]}
                };

                form.Action = responseDetails[OpayoConstants.Response.NextUrl];
            }
            else
            {
                _logger.Warn("Opayo (" + ctx.Order.OrderNumber + ") - Generate html form error - status: " + status + " | status details: " + responseDetails["StatusDetail"]);
            }

            return new PaymentFormResult()
            {
                MetaData = orderMetaData,
                Form = form
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<OpayoSettings> ctx, CancellationToken cancellationToken = default)
        {
            var callbackRequestModel = await CallbackRequestModel.FromRequestAsync(ctx.Request).ConfigureAwait(false);
            var client = new OpayoServerClient(
                _logger,
                new OpayoServerClientConfig {
                    ProviderAlias = Alias,
                    ContinueUrl = ctx.Urls.ContinueUrl,
                    CancelUrl = ctx.Urls.CancelUrl,
                    ErrorUrl = ctx.Urls.ErrorUrl
                });

            return client.HandleCallback(ctx.Order, callbackRequestModel, ctx.Settings);

        }
    }
}
