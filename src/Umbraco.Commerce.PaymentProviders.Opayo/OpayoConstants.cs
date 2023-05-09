namespace Umbraco.Commerce.PaymentProviders.Opayo
{
    public static class OpayoConstants
    {
        public static class TransactionRequestFields
        {
            public const string VpsProtocol = "VPSProtocol";
            public const string TransactionType = "TxType";
            public const string Vendor = "Vendor";
            public const string VendorTxCode = "VendorTxCode";
            public const string Amount = "Amount";
            public const string Currency = "Currency";
            public const string Description = "Description";
            public const string NotificationURL = "NotificationURL";
            public const string Basket = "Basket";
            public static class Billing
            {
                public const string Surname = "BillingSurname";
                public const string Firstnames = "BillingFirstnames";
                public const string Address1 = "BillingAddress1";
                public const string Address2 = "BillingAddress2";
                public const string City = "BillingCity";
                public const string PostCode = "BillingPostCode";
                public const string Country = "BillingCountry";
                public const string State = "BillingState";
            }

            public static class Delivery
            {
                public const string Surname = "DeliverySurname";
                public const string Firstnames = "DeliveryFirstnames";
                public const string Address1 = "DeliveryAddress1";
                public const string Address2 = "DeliveryAddress2";
                public const string City = "DeliveryCity";
                public const string PostCode = "DeliveryPostCode";
                public const string Country = "DeliveryCountry";
                public const string State = "DeliveryState";
            }
        }

        public static class Response
        {
            public const string Status = "Status";
            public static class StatusCodes
            {
                public const string Ok = "OK";
                public const string Repeated = "OK REPEATED";
                public const string Invalid = "INVALID";
                public const string Error = "ERROR";
            }

            public const string NextUrl = "NextURL";
            public const string SecurityKey = "SecurityKey";
            public const string TransactionId = "VPSTxId";
            public const string RedirectUrl = "RedirectURL";
        }

        public static class OrderProperties
        {
            public const string SecurityKey = "opayoSecurityKey";
            public const string TransactionId = "opayoTransactionId";
            public const string TransDetails = "opayoTransactionDetails";
            public const string TransDetailsHash = "opayoTransactionDetailsHash";
        }

        public static class CallbackRequest
        {
            public static class Status
            {
                public const string Ok = "OK";
                public const string NotAuthorised = "NOTAUTHED";
                public const string Pending = "PENDING";
                public const string Abort = "ABORT";
                public const string Rejected = "REJECTED";
                public const string Authenticated = "AUTHENTICATED";
                public const string Registered = "REGISTERED";
                public const string Error = "ERROR";
            }
        }

        public static class PlaceHolders
        {
            public const string OrderId = "##ORDER_ID##";
            public const string OrderReference = "##ORDER_REFERENCE##";
        }
    }
}
