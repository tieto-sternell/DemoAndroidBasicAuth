using System;
using System.Net;
using Xamarin.Android.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

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
            var spoofedSuccessResponse = await FakeBasicAuthCredentialsOnMessage(client);
            //Add the header to the client.
            var homeMadeClient = GetClientWithHomemadeAuthHeader(handler);
            var anotherSpoofedSuccessResponse = await homeMadeClient.GetAsync(basicUri) as AndroidHttpResponseMessage;
            //use the android java stack:
            var javaResponse = MakeBasicRequestWithJavaStack();
        }

        private static Java.Net.HttpStatus MakeBasicRequestWithJavaStack()
        {
            var javaUri = new Java.Net.URL(basicUri);
            var connection = (Java.Net.HttpURLConnection)new Java.Net.URL(basicUri).OpenConnection();
            connection.RequestMethod = "GET";
            connection.SetRequestProperty("User-Agent", "Mozilla/5.0");

            byte[] byteToken = System.Text.Encoding.UTF8.GetBytes(dummyUsername + ":" + dummyPassword);
            var tokenValue = Convert.ToBase64String(byteToken);
            connection.SetRequestProperty("Authorization","basic " + tokenValue);

            return connection.ResponseCode;
        }

        private HttpClient GetClientWithHomemadeAuthHeader(AndroidClientHandler handler)
        {
            byte[] byteToken = System.Text.Encoding.UTF8.GetBytes(dummyUsername + ":" + dummyPassword);
            var tokenValue = Convert.ToBase64String(byteToken);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", tokenValue);
            return client;
        }

        private async Task<AndroidHttpResponseMessage> FakeBasicAuthCredentialsOnMessage(HttpClient client)
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