using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wiki.DiscoveryService.Common;

namespace TestLib
{
    [TestClass]
    public class DiscoveryServiceTest
    {
        [TestMethod]
        public void TestListening()
        {
            ListeningService service = new ListeningService();
            service.StartListeningAsync();
            var c = 0;
        }
    }
}
