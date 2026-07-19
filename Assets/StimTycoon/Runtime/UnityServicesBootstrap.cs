using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace StimTycoon.Runtime
{
    public enum UnityServicesStartupState
    {
        NotStarted,
        Initializing,
        Ready,
        Failed
    }

    /// <summary>Initializes Unity Gaming Services once, before the first scene, in the build's explicit environment.</summary>
    public static class UnityServicesBootstrap
    {
        public const string DevelopmentEnvironmentName = "development";
        public const string ProductionEnvironmentName = "production";

        private static Task initializationTask;

        public static bool IsInitialized => UnityServices.State == ServicesInitializationState.Initialized;
        public static string EnvironmentName => ResolveEnvironmentName(Application.isEditor, Debug.isDebugBuild);
        public static UnityServicesStartupState StartupState { get; private set; }
        public static string LastError { get; private set; }

        public static string ResolveEnvironmentName(bool isEditor, bool isDebugBuild)
        {
            return isEditor || isDebugBuild
                ? DevelopmentEnvironmentName
                : ProductionEnvironmentName;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeFirstScene()
        {
            _ = InitializeAsync();
        }

        public static Task InitializeAsync()
        {
            if (IsInitialized)
            {
                StartupState = UnityServicesStartupState.Ready;
                LastError = null;
                return Task.CompletedTask;
            }

            return initializationTask ??= InitializeInternalAsync();
        }

        public static Task RetryAsync()
        {
            if (StartupState == UnityServicesStartupState.Initializing)
            {
                return initializationTask;
            }

            initializationTask = null;
            return InitializeAsync();
        }

#if UNITY_EDITOR
        public static void ResetTrackingForTests()
        {
            initializationTask = null;
            StartupState = UnityServicesStartupState.NotStarted;
            LastError = null;
        }
#endif

        private static async Task InitializeInternalAsync()
        {
            var environmentName = EnvironmentName;
            StartupState = UnityServicesStartupState.Initializing;
            LastError = null;
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(environmentName);
                await UnityServices.InitializeAsync(options);
                StartupState = UnityServicesStartupState.Ready;
                Debug.Log($"Unity Gaming Services initialized in '{environmentName}'.");
            }
            catch (Exception exception)
            {
                initializationTask = null;
                StartupState = UnityServicesStartupState.Failed;
                LastError = exception.Message;
                Debug.LogError($"Unity Gaming Services initialization failed for '{environmentName}': " +
                               exception.Message);
            }
        }
    }
}
