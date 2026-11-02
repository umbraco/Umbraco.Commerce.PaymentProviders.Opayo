using Flurl.Http;
using Flurl.Http.Testing;
using Umbraco.Commerce.PaymentProviders.Opayo.Api;
using Umbraco.Commerce.PaymentProviders.Opayo.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Opayo.UnitTests;

public class OpayoRegistrationTests
{
    private const string CustomerEmail = "jane.doe@example.com";

    private static Dictionary<string, string> RegistrationFields() => new()
    {
        [OpayoConstants.TransactionRequestFields.VpsProtocol] = "4.00",
        [OpayoConstants.TransactionRequestFields.TransactionType] = "PAYMENT",
        [OpayoConstants.TransactionRequestFields.Vendor] = "myvendor",
        [OpayoConstants.TransactionRequestFields.VendorTxCode] = "ORDER-0001",
        [OpayoConstants.TransactionRequestFields.Amount] = "10.00",
        [OpayoConstants.TransactionRequestFields.Billing.Surname] = "Doe",
        [OpayoConstants.TransactionRequestFields.Billing.Firstnames] = "Jane Ann",
        [OpayoConstants.TransactionRequestFields.Billing.CustomerEmail] = CustomerEmail,
        [OpayoConstants.TransactionRequestFields.Billing.Address1] = "1 High St & Co",
        [OpayoConstants.TransactionRequestFields.Billing.State] = null!,
    };

    private static OpayoServerClient CreateClient(RecordingLogger logger)
        => new(logger.Proxy, new OpayoServerClientConfig());

    [Fact]
    public async Task Registration_Fields_Are_Posted_As_A_Form_Body()
    {
        using var http = new HttpTest();
        http.RespondWith("Status=OK" + Environment.NewLine + "VPSTxId={TX}");

        var logger = new RecordingLogger();
        Dictionary<string, string> response = await CreateClient(logger).InitiateTransactionAsync(true, RegistrationFields());

        Assert.Equal("OK", response["Status"]);

        FlurlCall call = Assert.Single(http.CallLog);
        Assert.Equal(HttpMethod.Post, call.HttpRequestMessage.Method);
        Assert.Equal(string.Empty, call.HttpRequestMessage.RequestUri!.Query);
        Assert.Equal("application/x-www-form-urlencoded", call.HttpRequestMessage.Content!.Headers.ContentType!.MediaType);

        Dictionary<string, string> posted = ParseForm(call.RequestBody);
        Assert.Equal("Jane Ann", posted["BillingFirstnames"]);
        Assert.Equal(CustomerEmail, posted["CustomerEmail"]);
        Assert.Equal("1 High St & Co", posted["BillingAddress1"]);
        Assert.Equal("PAYMENT", posted["TxType"]);
        Assert.False(posted.ContainsKey("BillingState"));
    }

    [Fact]
    public async Task Failed_Registration_Is_Logged_Without_Request_Fields()
    {
        using var http = new HttpTest();
        http.RespondWith("Internal error", 500);

        var logger = new RecordingLogger();
        Dictionary<string, string> response = await CreateClient(logger).InitiateTransactionAsync(true, RegistrationFields());

        Assert.Empty(response);

        RecordingLogger.Entry entry = Assert.Single(logger.Entries, x => x.Level == "Error");
        string logged = entry.ToString();
        Assert.Contains("500", logged, StringComparison.Ordinal);
        Assert.Contains("vspserver-register.vsp", logged, StringComparison.Ordinal);
        Assert.DoesNotContain(CustomerEmail, logged, StringComparison.Ordinal);
        Assert.DoesNotContain("Doe", logged, StringComparison.Ordinal);
        Assert.DoesNotContain("?", logged, StringComparison.Ordinal);
    }

    private static Dictionary<string, string> ParseForm(string body)
        => body.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', 2))
            .ToDictionary(
                x => Uri.UnescapeDataString(x[0].Replace('+', ' ')),
                x => Uri.UnescapeDataString(x[1].Replace('+', ' ')));
}
