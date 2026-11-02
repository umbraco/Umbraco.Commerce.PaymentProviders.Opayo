using Umbraco.Commerce.PaymentProviders.Opayo.Api;

namespace Umbraco.Commerce.PaymentProviders.Opayo.UnitTests;

public class OpayoHelperTests
{
    [Fact]
    public void GetFields_Parses_The_Registration_Response()
    {
        string response = string.Join(
            Environment.NewLine,
            "VPSProtocol=4.00",
            "Status=OK",
            "StatusDetail=2014 : The Transaction was Registered Successfully.",
            "VPSTxId={6D0B1F14-0E0A-4D6B-9A0B-3A3C5A9F0001}",
            "SecurityKey=SECKEY1234",
            "NextURL=https://sandbox.opayo.eu.elavon.com/gateway/service/cardselection?vpstxid={6D0B1F14-0E0A-4D6B-9A0B-3A3C5A9F0001}");

        Dictionary<string, string> fields = OpayoServerClient.GetFields(response);

        Assert.Equal("OK", fields[OpayoConstants.Response.Status]);
        Assert.Equal("SECKEY1234", fields[OpayoConstants.Response.SecurityKey]);
        Assert.Equal("{6D0B1F14-0E0A-4D6B-9A0B-3A3C5A9F0001}", fields[OpayoConstants.Response.TransactionId]);

        // Values keep any '=' after the first one.
        Assert.Equal(
            "https://sandbox.opayo.eu.elavon.com/gateway/service/cardselection?vpstxid={6D0B1F14-0E0A-4D6B-9A0B-3A3C5A9F0001}",
            fields[OpayoConstants.Response.NextUrl]);
    }

    [Fact]
    public void GetFields_Of_An_Empty_Response_Is_Empty()
        => Assert.Empty(OpayoServerClient.GetFields(string.Empty));

    [Theory]
    [InlineData("PAYMENT", true, "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp")]
    [InlineData("deferred", true, "https://sandbox.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp")]
    [InlineData("PAYMENT", false, "https://live.opayo.eu.elavon.com/gateway/service/vspserver-register.vsp")]
    [InlineData("REFUND", false, "https://live.opayo.eu.elavon.com/gateway/service/refund.vsp")]
    public void Endpoints_Resolve_By_Type_And_Mode(string type, bool testMode, string expected)
        => Assert.Equal(expected, OpayoEndpoints.Get(type, testMode));

    [Fact]
    public void Unknown_Endpoint_Type_Throws()
        => Assert.Throws<UnknownEndpointTypeException>(() => OpayoEndpoints.Get("NOPE", true));

    [Theory]
    [InlineData("", "d41d8cd98f00b204e9800998ecf8427e")]
    [InlineData("The quick brown fox jumps over the lazy dog", "9e107d9d372bb6826bd81d3542a419d6")]
    public void ToMD5Hash_Is_Lower_Case_Hex_Md5(string input, string expected)
        => Assert.Equal(expected, input.ToMD5Hash());

    [Theory]
    [InlineData("Smith", 20, "Smith")]
    [InlineData("A very long surname indeed", 20, "A very long surname ")]
    [InlineData("", 5, "")]
    public void Truncate_Cuts_To_Length(string input, int length, string expected)
        => Assert.Equal(expected, input.Truncate(length));
}
