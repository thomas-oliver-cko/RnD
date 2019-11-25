using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Rnd.Core.ConsoleApp.Secrets
{
    public class SecretsManager
    {
        public static async Task<string> GetSecret()
        {
            const string secretName = "Test";
            const string region = "us-east-2";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            var request = new GetSecretValueRequest {SecretId = secretName};
            var response = await client.GetSecretValueAsync(request);

            // Decrypts secret using the associated KMS CMK.
            // Depending on whether the secret is a string or binary, one of these fields will be populated.
            if (response.SecretString != null)
            {
                return response.SecretString;
            }
            else
            {
                using var memoryStream = response.SecretBinary;
                using var reader = new StreamReader(memoryStream);

                var result = await reader.ReadToEndAsync();
                return Encoding.UTF8.GetString(Convert.FromBase64String(result));
            }
        }
    }
}
