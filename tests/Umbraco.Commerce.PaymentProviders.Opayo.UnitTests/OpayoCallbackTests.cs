using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Commerce.PaymentProviders.Opayo.Api;
using Umbraco.Commerce.PaymentProviders.Opayo.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Opayo.UnitTests;

public class OpayoCallbackTests
{
    private const string VendorName = "MyVendor";
    private const string SecurityKey = "SECKEY1234";

    private static Dictionary<string, StringValues> NotificationForm() => new()
    {
        ["VPSProtocol"] = "4.00",
        ["TxType"] = "PAYMENT",
        ["VendorTxCode"] = "ORDER-0001",
        ["VPSTxId"] = "{6D0B1F14-0E0A-4D6B-9A0B-3A3C5A9F0001}",
        ["Status"] = "OK",
        ["StatusDetail"] = "0000 : The Authorisation was Successful.",
        ["TxAuthNo"] = "8661",
        ["AVSCV2"] = "ALL MATCH",
        ["AddressResult"] = "MATCHED",
        ["PostCodeResult"] = "MATCHED",
        ["CV2Result"] = "MATCHED",
        ["GiftAid"] = "0",
        ["3DSecureStatus"] = "OK",
        ["CAVV"] = "AAABARR5kwAAAAAAAAAAAAAAAAA=",
        ["CardType"] = "VISA",
        ["Last4Digits"] = "0006",
        ["DeclineCode"] = "00",
        ["ExpiryDate"] = "1230",
        ["BankAuthCode"] = "999777",
        ["Surcharge"] = "1.50",
    };

    private static async Task<CallbackRequestModel> ParseAsync(Dictionary<string, StringValues> form)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(form);
        return await CallbackRequestModel.FromRequestAsync(context.Request);
    }

    private static string ExpectedSignature(CallbackRequestModel r, bool lowerCase = false)
    {
        // Opayo Server protocol 4.00: MD5 over these fields in this order, vendor name lower case, upper case hex.
        string raw = string.Concat(
            r.VPSTxId, r.VendorTxCode, r.Status, r.TxAuthNo, VendorName.ToLowerInvariant(), r.AVSCV2, SecurityKey,
            r.AddressResult, r.PostCodeResult, r.CV2Result, r.GiftAid, r.SecureStatus, r.CAVV, r.AddressStatus,
            r.PayerStatus, r.CardType, r.Last4Digits, r.DeclineCode, r.ExpiryDate, r.FraudResponse, r.BankAuthCode);
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        return lowerCase ? Convert.ToHexStringLower(hash) : Convert.ToHexString(hash);
    }

    [Fact]
    public async Task FromRequest_Reads_The_Notification_Fields()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());

        Assert.Equal("OK", model.Status);
        Assert.Equal("ORDER-0001", model.VendorTxCode);
        Assert.Equal("OK", model.SecureStatus);
        Assert.Equal("ALL MATCH", model.AVSCV2);
        Assert.Equal("0006", model.Last4Digits);
        Assert.Equal(1.50m, model.Surcharge);
    }

    [Fact]
    public async Task FromRequest_Defaults_Surcharge_To_Zero()
    {
        Dictionary<string, StringValues> form = NotificationForm();
        form.Remove("Surcharge");

        CallbackRequestModel model = await ParseAsync(form);

        Assert.Equal(0m, model.Surcharge);
    }

    [Fact]
    public async Task Signature_Matches_The_Opayo_Algorithm()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());

        Assert.Equal(ExpectedSignature(model), OpayoServerClient.ComputeVpsSignature(model, VendorName, SecurityKey));

        // Fixed value, so a field order change in both places still fails.
        Assert.Equal("6B2937E40D809544B9D3D2FACD1ED44B", OpayoServerClient.ComputeVpsSignature(model, VendorName, SecurityKey));
    }

    [Fact]
    public async Task Valid_Signature_Is_Accepted()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = ExpectedSignature(model);

        Assert.True(OpayoServerClient.IsValidVpsSignature(model, VendorName, SecurityKey));
    }

    [Fact]
    public async Task Tampered_Status_Is_Rejected()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = ExpectedSignature(model);
        model.Status = "NOTAUTHED";

        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, SecurityKey));
    }

    [Fact]
    public async Task Wrong_Security_Key_Is_Rejected()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = ExpectedSignature(model);

        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, "OTHERKEY"));
        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, null!));
    }

    [Fact]
    public async Task Missing_Signature_Is_Rejected()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());

        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, SecurityKey));
    }

    [Fact]
    public async Task Missing_Security_Key_Is_Rejected()
    {
        // A signature built without any security key must not validate against an order that has none stored.
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = OpayoServerClient.ComputeVpsSignature(model, VendorName, null!);

        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, null!));
        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, string.Empty));
    }

    [Fact]
    public async Task Lower_Case_Signature_Is_Accepted()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = ExpectedSignature(model, lowerCase: true);

        Assert.True(OpayoServerClient.IsValidVpsSignature(model, VendorName, SecurityKey));
    }

    [Fact]
    public async Task Signature_Of_Different_Length_Is_Rejected()
    {
        CallbackRequestModel model = await ParseAsync(NotificationForm());
        model.VPSSignature = ExpectedSignature(model) + "00";

        Assert.False(OpayoServerClient.IsValidVpsSignature(model, VendorName, SecurityKey));
    }
}
