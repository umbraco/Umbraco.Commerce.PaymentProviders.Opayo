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

        [PaymentProviderSetting(Name = "Continue URL", Description = "The URL to continue to after this provider has done processing. eg: /continue/", SortOrder = 500)]
        public string ContinueUrl { get; set; }
        [PaymentProviderSetting(Name = "Cancel URL", Description = "The URL to call if a payment is cancelled. eg: /cancelled/", SortOrder = 500)]
        public string CancelUrl { get; set; }
        [PaymentProviderSetting(Name = "Error URL", Description = "The URL to call if a payment errors. eg: /error/", SortOrder = 500)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(Name = "Vendor Name", Description = "Your unique identifier, assigned to you by Opayo during sign up", SortOrder = 1)]
        public string VendorName { get; set; }

        [PaymentProviderSetting(Name ="Transaction Type", IsAdvanced =true, Description ="Transaction Type: PAYMENT, DEFERRED, AUTHENTICATE", SortOrder = 1000)]
        public string TxType { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing Last Name", Description = "Order property alias containing the billing last name", SortOrder = 100)]
        public string OrderPropertyBillingLastName { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing First Name", Description = "Order property alias containing the billing first name", SortOrder = 101)]
        public string OrderPropertyBillingFirstName { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing Address 1", Description = "Order property alias containing the billing address 1", SortOrder = 102)]
        public string OrderPropertyBillingAddress1 { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing Address 2", Description = "Order property alias containing the billing address 2", SortOrder = 103)]
        public string OrderPropertyBillingAddress2 { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing City", Description = "Order property alias containing the billing city", SortOrder = 104)]
        public string OrderPropertyBillingCity { get; set; }
        [PaymentProviderSetting(Name = "Order property alias: Billing County/State", Description = "Order property alias containing the billing county/state", SortOrder = 105)]
        public string OrderPropertyBillingCounty { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Billing Postcode", Description = "Order property alias containing the billing postcode", SortOrder = 106)]
        public string OrderPropertyBillingPostcode { get; set; }



        [PaymentProviderSetting(Name = "Order property alias: Shipping LastName", Description = "Order property alias containing the shipping last name", SortOrder = 200)]
        public string OrderPropertyShippingLastName { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping First Name", Description = "Order property alias containing the shipping first name", SortOrder = 201)]
        public string OrderPropertyShippingFirstName { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping Address 1", Description = "Order property alias containing the shipping address 1", SortOrder = 202)]
        public string OrderPropertyShippingAddress1 { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping Address 2", Description = "Order property alias containing the shipping address 2", SortOrder = 203)]
        public string OrderPropertyShippingAddress2 { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping City", Description = "Order property alias containing the shipping city", SortOrder = 204)]
        public string OrderPropertyShippingCity { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping County/State", Description = "Order property alias containing the shipping county/state", SortOrder = 205)]
        public string OrderPropertyShippingCounty { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Shipping Postcode", Description = "Order property alias containing the shipping postcode", SortOrder = 206)]
        public string OrderPropertyShippingPostcode { get; set; }

        [PaymentProviderSetting(Name = "Display order lines on Opayo", Description ="Send the order line details to be shown on the payment providers final stage", SortOrder = 5)]
        public bool DisplayOrderLines { get; set; }

        [PaymentProviderSetting(Name = "Order line property alias: Description", Description ="Order line property alias containing the description of the order line item to send to Opayo (if left blank, this defaults to Product Name (Product SKU)", SortOrder = 6)]
        public string OrderLinePropertyDescription { get; set; }

        [PaymentProviderSetting(Name = "Order property alias: Description", Description = "Order property alias containing the description to send to Opayo", SortOrder = 2)]
        public string OrderPropertyDescription { get; set; }


        [PaymentProviderSetting(Name = "Test mode",
            Description = "Set whether to process payments in test mode.",
            SortOrder = 1000000)]
        public bool TestMode { get; set; }


    }
}
