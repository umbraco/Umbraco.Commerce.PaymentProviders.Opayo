using System.Text;

namespace Umbraco.Commerce.PaymentProviders.Opayo
{
    public static class ByteArrayExtensions
    {
        public static string ToHex(this byte[] self)
        {   
            StringBuilder hex = new StringBuilder(self.Length * 2);
            foreach (byte b in self)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
            
        }
    }
}
