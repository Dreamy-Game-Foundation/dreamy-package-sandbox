using Dreamy.Audio;
using Dreamy.Economy;
using UnityEngine;

namespace Dreamy.Feedback.Samples.Economy
{
    /// <summary>Standalone sample composition. Foundation supplies its persistent wallet and audioService instead.</summary>
    public sealed class FeedbackEconomyDemo : MonoBehaviour
    {
        [SerializeField] private FeedbackEconomyPanel panel;
        [SerializeField] private DreamyAudioProfile audioProfile;
        private AudioService audioService;
        private void Start()
        {
            audioService = new AudioService(new MemoryAudioPreferences());
            audioService.Initialize(audioProfile);
            var wallet = new FeedbackDemoWallet(100);
            panel.Configure(wallet, wallet, audioService);
        }
        private void OnDestroy() { if (audioService != null) { foreach (var bus in audioService.Buses) audioService.StopBus(bus.Id); } }
        private sealed class MemoryAudioPreferences : IAudioPreferenceStore
        {
            public bool TryGetFloat(string key, out float value) { value = 0; return false; }
            public void SetFloat(string key, float value) { }
        }
    }
}
