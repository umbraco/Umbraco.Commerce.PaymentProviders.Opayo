using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Opayo
{
    public class OpayoSettings
    {
        public static class Defaults
        {
            public static string VPSProtocol = "3.00";
            public static string TxType = "PAYMENT";
        }

        [PaymentProviderSetting(SortOrder = 500)]
        public string ContinueUrl { get; set; }
        [PaymentProviderSetting(SortOrder = 500)]
        public string CancelUrl { get; set; }
        [PaymentProviderSetting(SortOrder = 500)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 1)]
        public string VendorName { get; set; }

        [PaymentProviderSetting(IsAdvanced = true, SortOrder = 1000)]
        public string TxType { get; set; }

        [PaymentProviderSetting(SortOrder = 100)]
        public string OrderPropertyBillingLastName { get; set; }

        [PaymentProviderSetting(SortOrder = 101)]
        public string OrderPropertyBillingFirstName { get; set; }

        [PaymentProviderSetting(SortOrder = 102)]
        public string OrderPropertyBillingAddress1 { get; set; }

        [PaymentProviderSetting(SortOrder = 103)]
        public string OrderPropertyBillingAddress2 { get; set; }

        [PaymentProviderSetting(SortOrder = 104)]
        public string OrderPropertyBillingCity { get; set; }
        [PaymentProviderSetting(SortOrder = 105)]
        public string OrderPropertyBillingCounty { get; set; }

        [PaymentProviderSetting(SortOrder = 106)]
        public string OrderPropertyBillingPostcode { get; set; }

        [PaymentProviderSetting(SortOrder = 200)]
        public string OrderPropertyShippingLastName { get; set; }

        [PaymentProviderSetting(SortOrder = 201)]
        public string OrderPropertyShippingFirstName { get; set; }

        [PaymentProviderSetting(SortOrder = 202)]
        public string OrderPropertyShippingAddress1 { get; set; }

        [PaymentProviderSetting(SortOrder = 203)]
        public string OrderPropertyShippingAddress2 { get; set; }

        [PaymentProviderSetting(SortOrder = 204)]
        public string OrderPropertyShippingCity { get; set; }

        [PaymentProviderSetting(SortOrder = 205)]
        public string OrderPropertyShippingCounty { get; set; }

        [PaymentProviderSetting(SortOrder = 206)]
        public string OrderPropertyShippingPostcode { get; set; }

        [PaymentProviderSetting(SortOrder = 5)]
        public bool DisplayOrderLines { get; set; }

        [PaymentProviderSetting(SortOrder = 6)]
        public string OrderLinePropertyDescription { get; set; }

        [PaymentProviderSetting(SortOrder = 2)]
        public string OrderPropertyDescription { get; set; }

        [PaymentProviderSetting(
            SortOrder = 1000000)]
        public bool TestMode { get; set; }
    }
}
