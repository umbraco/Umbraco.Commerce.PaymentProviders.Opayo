namespace Umbraco.Commerce.PaymentProviders.Opayo.Api.Models
{
    public class OpayoServerClientConfig
    {
        public string ProviderAlias { get; set; }
        public string ErrorUrl { get; set; }
        public string CancelUrl { get; set; }
        public string ContinueUrl { get; set; }
    }
}
