using NUnit.Framework;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class StimUnityServicesBootstrapTests
    {
        [Test]
        public void Bootstrap_TargetsExistingProductionEnvironment()
        {
            Assert.That(StimUnityServicesBootstrap.EnvironmentName, Is.EqualTo("production"));
        }
    }
}
