using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace StimTycoon.Runtime
{
    /// <summary>Initializes Unity Gaming Services once, before the first scene, against the live project environment.</summary>
    public static class StimUnityServicesBootstrap
    {
        public const string EnvironmentName = "production";

        private static Task initializationTask;

        public static bool IsInitialized => UnityServices.State == ServicesInitializationState.Initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeFirstScene()
        {
            _ = InitializeAsync();
        }

        public static Task InitializeAsync()
        {
            if (IsInitialized) return Task.CompletedTask;
            return initializationTask ??= InitializeInternalAsync();
        }

        private static async Task InitializeInternalAsync()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(EnvironmentName);
                await UnityServices.InitializeAsync(options);
                Debug.Log($"Unity Gaming Services initialized in '{EnvironmentName}'.");
            }
            catch (Exception exception)
            {
                initializationTask = null;
                Debug.LogError($"Unity Gaming Services initialization failed for '{EnvironmentName}': " +
                               exception.Message);
            }
        }
    }
}
