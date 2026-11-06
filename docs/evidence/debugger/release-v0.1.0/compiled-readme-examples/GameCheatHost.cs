using System;
using Dreamy.Debugger;
using Dreamy.Debugger.Options;
using UnityEngine;

namespace MyGame.Debugging
{
    [DisallowMultipleComponent]
    public sealed class GameCheatHost : MonoBehaviour
    {
        private DebuggerRuntime owner;
        private CheatRegistry registry;
        private IDisposable lease;

        private void Update()
        {
            var runtime = DebuggerRuntime.Instance;
            if (runtime == null || !runtime.isActiveAndEnabled || runtime.Cheats == null)
            {
                Release();
                return;
            }
            if (ReferenceEquals(owner, runtime) && ReferenceEquals(registry, runtime.Cheats)) return;
            Release();
            // Với game thật: đợi wallet/save/entitlements ready trước dòng này,
            // rồi truyền service/delegate vào constructor của container.
            lease = runtime.RegisterCheats(new GameCheatOptions(), "mygame");
            owner = runtime;
            registry = runtime.Cheats;
        }

        private void OnDisable() => Release();

        private void Release()
        {
            lease?.Dispose();
            lease = null;
            owner = null;
            registry = null;
        }
    }
}
