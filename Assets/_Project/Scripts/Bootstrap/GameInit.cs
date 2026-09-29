using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Audio;
using Dreamy.Audio.Generated;

using UnityEngine;

namespace Dreamy.Template
{
    public sealed class GameInit : MonoBehaviour
    {
        private async void Start()
        {
            await UniTask.WaitUntil(
                () => GameInstaller.State is
                    BootstrapState.Ready or BootstrapState.Failed,
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (GameInstaller.State == BootstrapState.Failed)
            {
                Debug.LogError(
                    $"[DreamyTemplate] Bootstrap failed: " +
                    $"{GameInstaller.InitializationException}");
                return;
            }

            await SceneLoader.Instance.LoadScene(Address.MainScene);
            DreamyAudio.PlayMusic(AudioLibraryIds.Music.MusicMain);
        }
    }
}