using System.Collections.Generic;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api
{
    internal static class OpayoEndpoints
    {
        private static Dictionary<string, string> TestEndpoints => new()
        {
            { "AUTHORISE", "https://sandbox.opayo.eu.elavon.com/gateway/service/authorise.vsp" },
            { "PAYMENT", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "DEFERRED", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "AUTHENTICATE", "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "CANCEL", "https://sandbox.opayo.eu.elavon.com/gateway/service/cancel.vsp" },
            { "REFUND", "https://sandbox.opayo.eu.elavon.com/gateway/service/refund.vsp" },
        };

        private static Dictionary<string, string> LiveEndpoints => new()
        {
            { "AUTHORISE", "https://live.opayo.eu.elavon.com/gateway/service/authorise.vsp" },
            { "PAYMENT", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "DEFERRED", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "AUTHENTICATE", "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp" },
            { "CANCEL", "https://live.opayo.eu.elavon.com/gateway/service/cancel.vsp" },
            { "REFUND", "https://live.opayo.eu.elavon.com/gateway/service/refund.vsp" },
        };

        /// <summary>
        /// Get Opayo endpoint by type.
        /// </summary>
        /// <param name="endpointType"></param>
        /// <param name="isTestMode"></param>
        /// <returns></returns>
        /// <exception cref="UnknownEndpointTypeException">Throws exception when unable to get the endpoint.</exception>
        public static string Get(string endpointType, bool isTestMode)
        {
            string normalizeEndpointType = endpointType.ToUpperInvariant();

            if (isTestMode)
            {
                if (!TestEndpoints.TryGetValue(normalizeEndpointType, out string testEndpoint))
                {
                    throw new UnknownEndpointTypeException(endpointType);
                }

                return testEndpoint;
            }

            if (!LiveEndpoints.TryGetValue(normalizeEndpointType, out string liveEndpoint))
            {
                throw new UnknownEndpointTypeException(endpointType);
            }

            return liveEndpoint;
        }
    }
}
