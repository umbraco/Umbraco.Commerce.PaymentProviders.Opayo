using System;
using System.Globalization;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api
{
    [Serializable]
    public class UnknownEndpointTypeException : Exception
    {
        private static readonly CompositeFormat _messageTemplate = CompositeFormat.Parse("Unknown endpoint type '{0}'");

        public UnknownEndpointTypeException()
            : base("Unknown endpoint type")
        {
        }

        public UnknownEndpointTypeException(string endpointType)
            : base(string.Format(CultureInfo.InvariantCulture, _messageTemplate, endpointType))
        {
        }

        public UnknownEndpointTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
