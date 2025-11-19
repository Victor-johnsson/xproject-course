using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebShopX.FunctionService.Services
{
    public class SasTokenGenerator
    {
        public static string GenerateSasToken(string namespaceName, string queueName, string sharedAccessKeyName, string sharedAccessKey, DateTime expiry)
        {
            // Construct the resource URI (this is the URI of your queue)
            string resourceUri = $"https://{namespaceName}.servicebus.windows.net/{queueName}/messages";

            // Convert expiry to Unix time
            long expiryUnixTime = ((DateTimeOffset)expiry).ToUnixTimeSeconds();

            // Construct the string to sign
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiryUnixTime;

            // Create the HMACSHA256 signature using the Shared Access Key
            using (HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(sharedAccessKey)))
            {
                byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                string signature = Convert.ToBase64String(signatureBytes);

                // Construct the SAS token
                string sasToken = $"SharedAccessSignature sr={HttpUtility.UrlEncode(resourceUri)}&sig={HttpUtility.UrlEncode(signature)}&se={expiryUnixTime}&skn={sharedAccessKeyName}";

                return sasToken;
            }
        }
    }
}
