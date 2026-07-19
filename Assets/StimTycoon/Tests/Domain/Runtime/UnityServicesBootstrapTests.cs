using NUnit.Framework;
using StimTycoon.Runtime;

namespace StimTycoon.Tests.Domain.Runtime
{
    public sealed class UnityServicesBootstrapTests
    {
        [SetUp]
        public void SetUp()
        {
            UnityServicesBootstrap.ResetTrackingForTests();
        }

        [Test]
        public void Bootstrap_TargetsDevelopmentEnvironmentInEditor()
        {
            Assert.That(UnityServicesBootstrap.ResolveEnvironmentName(true, false), Is.EqualTo("development"));
        }

        [Test]
        public void Bootstrap_TargetsDevelopmentEnvironmentInDevelopmentBuild()
        {
            Assert.That(UnityServicesBootstrap.ResolveEnvironmentName(false, true), Is.EqualTo("development"));
        }

        [Test]
        public void Bootstrap_TargetsProductionEnvironmentInReleaseBuild()
        {
            Assert.That(UnityServicesBootstrap.ResolveEnvironmentName(false, false), Is.EqualTo("production"));
        }

        [Test]
        public void Bootstrap_ResetHookClearsObservableStartupState()
        {
            Assert.That(UnityServicesBootstrap.StartupState, Is.EqualTo(UnityServicesStartupState.NotStarted));
            Assert.That(UnityServicesBootstrap.LastError, Is.Null);
        }
    }
}
