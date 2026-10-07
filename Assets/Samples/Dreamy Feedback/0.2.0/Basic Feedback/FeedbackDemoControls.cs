using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feedback.Samples
{
    public sealed class FeedbackDemoControls : MonoBehaviour
    {
        [SerializeField] private FeedbackHost host;
        [SerializeField] private Transform target;
        [SerializeField] private RectTransform iconStart;
        [SerializeField] private RectTransform iconEnd;
        [SerializeField] private Sprite icon;
        [SerializeField] private Button[] channelButtons;
        [SerializeField] private Button playAllButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Toggle hapticToggle;
        [SerializeField] private TMP_Text statusText;
        private Action[] actions;
        private void OnEnable()
        {
            host.Initialize();
            actions = new Action[] { PlayVfx, PlayHaptic, PlayText, PlayIcons, PlayScreen, PlayCamera, PlaySequence };
            listeners = new UnityEngine.Events.UnityAction[channelButtons.Length];
            for (int i = 0; i < channelButtons.Length; i++) { int channel = i; listeners[i] = () => PlayChannel(channel); channelButtons[i].onClick.AddListener(listeners[i]); }
            playAllButton.onClick.AddListener(PlayAll); stopButton.onClick.AddListener(StopAll);
            hapticToggle.onValueChanged.AddListener(SetHaptic); SetHaptic(hapticToggle.isOn);
        }
        // Each button is sample-owned; keep listener delegates to remove only our handlers.
        private UnityEngine.Events.UnityAction[] listeners;
        public void PlayChannel(int channel)
        {
            actions[channel](); statusText.text = "Playing " + channelButtons[channel].GetComponentInChildren<TMP_Text>().text;
        }
        public void PlayAll() { foreach (var action in actions) action(); statusText.text = "All feedback channels played"; }
        public void StopAll() { host.StopAll(); statusText.text = "Stopped and cleared all active effects"; }
        private void PlayVfx() => host.Vfx.Play("reward", target.position);
        private void PlayHaptic() => host.Haptic.Play(HapticType.Light);
        private void PlayText() => host.FloatingText.Play("+100", target.position);
        private void PlayIcons()
        {
            var options = IconFlyOptions.Create(icon, iconStart.position, iconEnd.position); options.Count = 8;
            host.IconFly.Fly(options);
        }
        private void PlayScreen() => host.Screen.Flash(ScreenFlashOptions.WhiteFlash());
        private void PlayCamera() => host.CameraShake.Shake("small");
        private void PlaySequence() => host.Sequence.Play("reward", FeedbackContext.At(target.position));
        private void SetHaptic(bool enabled)
        {
            host.Haptic.Enabled = enabled;
            statusText.text = enabled ? "Haptics enabled: generic vibration on supported mobile devices" : "Haptics disabled";
        }
        private void OnDisable()
        {
            if (listeners != null) for (int i = 0; i < listeners.Length; i++) channelButtons[i].onClick.RemoveListener(listeners[i]);
            playAllButton.onClick.RemoveListener(PlayAll); stopButton.onClick.RemoveListener(StopAll);
            hapticToggle.onValueChanged.RemoveListener(SetHaptic);
            host.StopAll();
        }
    }
}
