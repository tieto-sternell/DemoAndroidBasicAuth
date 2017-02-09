using System;
using System.Net;
using Xamarin.Android.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoAndroidBasicAuth.Droid
{
    public class TestStuff
    {
        private static string dummyUsername = "bob";
        private static string dummyPassword = "secret";
        private static string basicUri = "http://httpbin.org/basic-auth/" + dummyUsername + "/" + dummyPassword;

        public async Task TestBasicAuthentication()
        {
            //Setup AndroidClientHandler with ICredentials
            var handler = GetBasicHandler();
            var client = new HttpClient(handler);
            //Get a 401 response from the server
            var badResponse = await client.GetAsync(basicUri) as AndroidHttpResponseMessage;
            //Double-check that the server and credentials really work, by spoofing the Auth header.
            var spoofedSuccessResponse = await FakeBasicAuthCredentials(client);
        }

        private async Task<AndroidHttpResponseMessage> FakeBasicAuthCredentials(HttpClient client)
        {
            byte[] byteToken = System.Text.Encoding.UTF8.GetBytes(dummyUsername + ":" + dummyPassword);
            var tokenValue = Convert.ToBase64String(byteToken);
            var token = "Basic " + tokenValue;

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(basicUri));
            requestMessage.Headers.Add("Authorization", token.ToString());
            return await client.SendAsync(requestMessage) as AndroidHttpResponseMessage;
        }

        private AndroidClientHandler GetBasicHandler()
        {
            ICredentials credentials = new NetworkCredential(dummyUsername, dummyPassword);

            var handler = new AndroidClientHandler();
            handler.Credentials = credentials;
            return handler;
        }
    }
}