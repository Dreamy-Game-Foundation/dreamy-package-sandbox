using System;
using Dreamy.Core;
using Dreamy.Debugger;
using Dreamy.Economy;
using UnityEngine;

namespace Dreamy.Template.Debugging
{
    [DisallowMultipleComponent]
    public sealed class SandboxCheatHost : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameCheats()
        {
            if (!Debug.isDebugBuild) return;
            // Keep game bindings independent of an optional debugger scene prefab.
            if (UnityEngine.Object.FindObjectsByType<SandboxCheatHost>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0) return;
            var root = new GameObject("Dreamy Game Cheats");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.AddComponent<SandboxCheatHost>();
        }
        private IDisposable registration;
        private DebuggerRuntime owner;
        private Dreamy.Debugger.Options.CheatRegistry registeredRegistry;
        private bool registrationFailed;
        public GameCheatOptions Options { get; private set; }

        private void OnEnable() { registrationFailed = false; }
        private void Update()
        {
            if (!Debug.isDebugBuild || registrationFailed) return;
            var runtime = DebuggerRuntime.Instance;
            if (registration != null && owner == runtime && runtime != null && ReferenceEquals(registeredRegistry, runtime.Cheats)) return;
            Release();
            if (runtime == null || GameInstaller.State != BootstrapState.Ready) return;
            if (!ServiceLocator.TryGet<IResourceWallet>(out var wallet) ||
                !ServiceLocator.TryGet<IResourceBalanceProvider>(out var balances)) return;
            try
            {
                Options = new GameCheatOptions(wallet, balances);
                registration = runtime.RegisterCheats(Options, "sandbox.game");
                owner = runtime;
                registeredRegistry = runtime.Cheats;
            }
            catch (Exception exception)
            {
                registrationFailed = true;
                Release();
                Debug.LogException(exception, this);
            }
        }

        private void OnDisable() => Release();
        private void Release()
        {
            registration?.Dispose(); registration = null; owner = null; registeredRegistry = null; Options = null;
        }
    }
}
