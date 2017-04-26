using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wiki.Core.Exceptions;
using Wiki.Service.Common.Clients;

namespace TestClient
{
    [TestClass]
    public class TestHttpClient
    {
        [TestMethod]
        public void TestMethod1()
        {
           // var c = DiscoveryFactory.GetToken("qq", "aa", "http://admin-srv:81/idsrv").Result;
            using (var cl = new TestLibClient())
            {
                var result = cl.Help().Result;

                new HttpClient().GetAsync("http://127.0.0.1:9100/services/stop/" + cl.ServiceId ).Result.EnsureSuccessStatusCode();
                result = cl.Help().Result;

            }

        }
    }

    public class TestLibClient:ServiceClientBase
    {
        public TestLibClient()
            : base("http://127.0.0.1:9100/", "service", "servicepasswd")
        {
        }

        public async Task<string> Help()
        {
            using (var cl = this.GetClient())
            {
                var result=await cl.GetAsync("help").ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    var message = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new WikiApiException(result.StatusCode, message);
                }
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public override string ServiceId
        {
            get { return "TestLib"; }
        }
    }
}
