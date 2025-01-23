using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Opayo.Api;
using Umbraco.Commerce.PaymentProviders.Opayo.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Opayo
{
    [PaymentProvider("opayo-server", Icon = "icon-credit-card")]
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

            Dictionary<string, string> inputFields = await OpayoInputLoader.LoadInputsAsync(ctx.Order, ctx.Settings, Context, ctx.Urls.CallbackUrl);
            Dictionary<string, string> responseDetails = await client.InitiateTransactionAsync(ctx.Settings.TestMode, inputFields, cancellationToken).ConfigureAwait(false);

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
                _logger.Error("Opayo (" + ctx.Order.OrderNumber + ") - Generate html form error - status: " + status + " | status details: " + responseDetails["StatusDetail"]);
            }

            return new PaymentFormResult()
            {
                MetaData = orderMetaData,
                Form = form
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<OpayoSettings> ctx, CancellationToken cancellationToken = default)
        {
            CallbackRequestModel callbackRequestModel = await CallbackRequestModel.FromRequestAsync(ctx.HttpContext.Request).ConfigureAwait(false);
            var client = new OpayoServerClient(
                _logger,
                new OpayoServerClientConfig
                {
                    ProviderAlias = Alias,
                    ContinueUrl = ctx.Urls.ContinueUrl,
                    CancelUrl = ctx.Urls.CancelUrl,
                    ErrorUrl = ctx.Urls.ErrorUrl
                });

            return client.HandleCallback(ctx.Order, callbackRequestModel, ctx.Settings);

        }
    }
}
