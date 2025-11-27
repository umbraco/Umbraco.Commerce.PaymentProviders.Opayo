using System.ComponentModel.DataAnnotations;
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

        [Required]
        [PaymentProviderSetting(SortOrder = 1)]
        public string VendorName { get; set; }

        [PaymentProviderSetting(SortOrder = 2)]
        public string OrderPropertyDescription { get; set; }

        [PaymentProviderSetting(SortOrder = 5)]
        public bool DisplayOrderLines { get; set; }

        [PaymentProviderSetting(SortOrder = 6)]
        public string OrderLinePropertyDescription { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 100)]
        public string OrderPropertyBillingLastName { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 101)]
        public string OrderPropertyBillingFirstName { get; set; }

        [PaymentProviderSetting(SortOrder = 110)]
        public string OrderPropertyCustomerEmail { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 120)]
        public string OrderPropertyBillingAddress1 { get; set; }

        [PaymentProviderSetting(SortOrder = 130)]
        public string OrderPropertyBillingAddress2 { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 140)]
        public string OrderPropertyBillingCity { get; set; }

        [PaymentProviderSetting(SortOrder = 150)]
        public string OrderPropertyBillingCounty { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 160)]
        public string OrderPropertyBillingPostcode { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 200)]
        public string OrderPropertyShippingLastName { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 210)]
        public string OrderPropertyShippingFirstName { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 220)]
        public string OrderPropertyShippingAddress1 { get; set; }

        [PaymentProviderSetting(SortOrder = 230)]
        public string OrderPropertyShippingAddress2 { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 240)]
        public string OrderPropertyShippingCity { get; set; }

        [PaymentProviderSetting(SortOrder = 250)]
        public string OrderPropertyShippingCounty { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 260)]
        public string OrderPropertyShippingPostcode { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 500)]
        public string ContinueUrl { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 501)]
        public string CancelUrl { get; set; }

        [Required]
        [PaymentProviderSetting(SortOrder = 502)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(IsAdvanced = true, SortOrder = 1000)]
        public string TxType { get; set; }

        [PaymentProviderSetting(
            SortOrder = 1000000)]
        public bool TestMode { get; set; }
    }
}
