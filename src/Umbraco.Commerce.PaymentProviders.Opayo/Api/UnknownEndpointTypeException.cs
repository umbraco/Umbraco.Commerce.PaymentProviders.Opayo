using System;
using System.Globalization;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api
{
    [Serializable]
    public class UnknownEndpointTypeException : Exception
    {
        private const string MessageTemplate = "Unknown endpoint type '{0}'";

        public UnknownEndpointTypeException()
            : base("Unknown endpoint type")
        {
        }

        public UnknownEndpointTypeException(string endpointType)
            : base(string.Format(CultureInfo.InvariantCulture, MessageTemplate, endpointType))
        {
        }

        public UnknownEndpointTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
