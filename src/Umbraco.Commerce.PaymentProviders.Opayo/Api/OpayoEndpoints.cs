using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api
{
    internal static class OpayoEndpoints
    {
        public static IDictionary<string, string> TestEndpoints => new Dictionary<string, string>
        {
            { "AUTHORISE", "https://sandbox.opayo.eu.elavon.com/gateway/service/authorise.vsp" },
            { "PAYMENT", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "DEFERRED", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "AUTHENTICATE", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "CANCEL", "https://sandbox.opayo.eu.elavon.com/gateway/service/cancel.vsp" },
            { "REFUND", "https://sandbox.opayo.eu.elavon.com/gateway/service/refund.vsp" },
        };

        public static IDictionary<string, string> LiveEndpoints => new Dictionary<string, string>
        {
            { "AUTHORISE", "https://live.opayo.eu.elavon.com/gateway/service/authorise.vsp" },
            { "PAYMENT", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "DEFERRED", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "AUTHENTICATE", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "CANCEL", "https://live.opayo.eu.elavon.com/gateway/service/cancel.vsp" },
            { "REFUND", "https://live.opayo.eu.elavon.com/gateway/service/refund.vsp" },
        };
    }
}
