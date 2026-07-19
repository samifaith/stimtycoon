using NUnit.Framework;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class UnityServicesBootstrapTests
    {
        [Test]
        public void Bootstrap_TargetsExistingProductionEnvironment()
        {
            Assert.That(UnityServicesBootstrap.EnvironmentName, Is.EqualTo("production"));
        }
    }
}
