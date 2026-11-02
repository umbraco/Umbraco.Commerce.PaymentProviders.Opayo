using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Opayo.UnitTests;

public class OpayoFormResultTests
{
    private const string CancelUrl = "https://example.com/cancel";
    private const string OrderNumber = "ORDER-0001";

    private static PaymentForm CancelForm() => new(CancelUrl, PaymentFormMethod.Post);

    [Fact]
    public void An_Empty_Registration_Response_Sends_The_Customer_To_The_Cancel_Url()
    {
        var logger = new RecordingLogger();

        PaymentFormResult result = OpayoServerPaymentProvider.CreateFormResult(new Dictionary<string, string>(), CancelForm(), OrderNumber, logger.Proxy);

        Assert.Equal(CancelUrl, result.Form.Action);
        Assert.Null(result.MetaData);

        RecordingLogger.Entry entry = Assert.Single(logger.Entries, x => x.Level == "Error");
        Assert.Contains(OrderNumber, entry.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_Rejected_Registration_Without_A_Status_Detail_Is_Logged()
    {
        var logger = new RecordingLogger();
        var response = new Dictionary<string, string> { ["Status"] = "INVALID" };

        PaymentFormResult result = OpayoServerPaymentProvider.CreateFormResult(response, CancelForm(), OrderNumber, logger.Proxy);

        Assert.Equal(CancelUrl, result.Form.Action);
        Assert.Null(result.MetaData);

        RecordingLogger.Entry entry = Assert.Single(logger.Entries, x => x.Level == "Error");
        Assert.Contains("INVALID", entry.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_Successful_Registration_Posts_To_The_Opayo_Next_Url()
    {
        var logger = new RecordingLogger();
        var response = new Dictionary<string, string>
        {
            ["Status"] = "OK",
            ["NextURL"] = "https://sandbox.opayo.eu.elavon.com/gateway/service/cardselection?vpstxid={TX}",
            ["SecurityKey"] = "KEY",
            ["VPSTxId"] = "{TX}",
        };

        PaymentFormResult result = OpayoServerPaymentProvider.CreateFormResult(response, CancelForm(), OrderNumber, logger.Proxy);

        Assert.Equal(response["NextURL"], result.Form.Action);
        Assert.Equal("{TX}", result.MetaData[OpayoConstants.OrderProperties.TransactionId]);
        Assert.Equal("KEY", result.MetaData[OpayoConstants.OrderProperties.SecurityKey]);
        Assert.DoesNotContain(logger.Entries, x => x.Level == "Error");
    }
}
