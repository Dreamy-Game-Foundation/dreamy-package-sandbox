using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Dreamy.Feedback.Samples
{
    public sealed class FeedbackDemoControls : MonoBehaviour
    {
        [SerializeField] private FeedbackHost host;
        [SerializeField] private Transform target;
        [SerializeField] private RectTransform iconStart, iconEnd;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite[] rewardIcons;
        [SerializeField] private FeedbackDefinition[] rewardDefinitions;
        [SerializeField] private Button[] rewardButtons;
        private UnityEngine.Events.UnityAction[] rewardListeners;
        private int selectedReward;
        [SerializeField] private Button[] channelButtons;
        [SerializeField] private Button playAllButton, stopButton, completeButton;
        [SerializeField] private Toggle hapticToggle;
        [SerializeField] private TMP_Text statusText;
        private UnityEngine.Events.UnityAction[] listeners;
        private readonly List<FeedbackHandle> handles = new List<FeedbackHandle>();
        public int ChannelCount => channelButtons.Length;
        private void OnEnable()
        {
            host.Initialize(); listeners = new UnityEngine.Events.UnityAction[channelButtons.Length];
            for (int i=0;i<channelButtons.Length;i++) { int channel=i; listeners[i]=()=>PlayChannel(channel); channelButtons[i].onClick.AddListener(listeners[i]); }
            rewardListeners = new UnityEngine.Events.UnityAction[rewardButtons.Length];
            for (int i = 0; i < rewardButtons.Length; i++)
            { int reward = i; rewardListeners[i] = () => PlayReward(reward); rewardButtons[i].onClick.AddListener(rewardListeners[i]); }
            playAllButton.onClick.AddListener(PlayAll); stopButton.onClick.AddListener(StopAll);
            if (completeButton) completeButton.onClick.AddListener(CompleteAll);
            hapticToggle.onValueChanged.AddListener(SetHaptic); SetHaptic(hapticToggle.isOn);
        }
        public void PlayChannel(int channel)
        {
            FeedbackHandle h = default;
            switch (channel)
            {
                case 0: h=host.Vfx.Play("reward",target.position); break;
                case 1: host.Haptic.Play(HapticType.Light); break;
                case 2: h=host.FloatingText.Play("+100",target.position); break;
                case 3: h=PlayIcons(IconFlyStyle.ScatterMagnet); break;
                case 4: h=host.Screen.Flash(ScreenFlashOptions.WhiteFlash()); break;
                case 5: h=host.CameraShake.Shake("small"); break;
                case 6:
                    h=PlaySelectedReward(); break;
                case 7: h=PlayIcons(IconFlyStyle.Straight); break;
                case 8: h=PlayIcons(IconFlyStyle.Arc); break;
                case 9: h=PlayIcons(IconFlyStyle.Fountain); break;
                case 10: h=PlayIcons(IconFlyStyle.Spiral); break;
                case 11: h=host.Ui.Punch(iconEnd,UiPunchOptions.Default); break;
                case 12:
                    h=host.Feedback.Play(FeedbackRequestBuilder.For(host.GetComponent<FeedbackSampleDefinitions>().Hit).From(this).At(target.position).WithIntensity(1.5f).Build()); break;
                case 13:
                    h=host.Feedback.Play(FeedbackRequestBuilder.For(host.GetComponent<FeedbackSampleDefinitions>().ButtonClick).From(this).To(iconEnd).Build()); break;
            }
            handles.RemoveAll(item=>!item.IsRunning); if (h.IsValid) handles.Add(h);
            statusText.text="Playing "+channelButtons[channel].GetComponentInChildren<TMP_Text>().text;
        }
        public void PlayReward(int index)
        {
            if (index < 0 || index >= rewardDefinitions.Length) return;
            selectedReward = index; icon = rewardIcons[index];
            var image = iconEnd.GetComponent<Image>(); if (image) image.sprite = icon;
            handles.RemoveAll(item => !item.IsRunning);
            var handle = PlaySelectedReward(); if (handle.IsValid) handles.Add(handle);
            statusText.text = "Preview: " + rewardDefinitions[index].name;
        }
        private FeedbackHandle PlaySelectedReward()
        {
            var definition = rewardDefinitions.Length > selectedReward
                ? rewardDefinitions[selectedReward] : host.GetComponent<FeedbackSampleDefinitions>().Reward;
            return host.Feedback.Play(FeedbackRequestBuilder.For(definition).From(this)
                .At(target.position).To(iconEnd).IconsFrom(iconStart).WithIcon(icon).WithAmount(100).Build());
        }
        private FeedbackHandle PlayIcons(IconFlyStyle style)
        {
            var options=IconFlyOptions.Create(icon,iconStart.position,iconEnd.position);
            options.Count=10; options.Style=style; options.Target=iconEnd;
            options.Ease=style==IconFlyStyle.Straight ? FeedbackEase.InCubic : FeedbackEase.InOutSine;
            return host.IconFly.Fly(options);
        }
        public void PlayAll() { for(int i=0;i<ChannelCount;i++) PlayChannel(i); statusText.text="Multiple independent plays; choose Stop or Complete"; }
        public void CompleteAll() { foreach(var h in handles.ToArray()) h.Complete(); handles.Clear(); statusText.text="Completed owned feedback"; }
        public void StopAll() { foreach(var h in handles.ToArray()) h.Stop(); handles.Clear(); host.StopAll(); statusText.text="Stopped / cleared"; }
        private void SetHaptic(bool value) { if(host.Haptic!=null) host.Haptic.Enabled=value; statusText.text=value ? "Mobile vibration enabled" : "Haptics disabled"; }
        private void OnDisable()
        {
            if (rewardListeners != null) for (int i = 0; i < rewardListeners.Length; i++) rewardButtons[i].onClick.RemoveListener(rewardListeners[i]);
            if(listeners!=null) for(int i=0;i<listeners.Length;i++) channelButtons[i].onClick.RemoveListener(listeners[i]);
            playAllButton.onClick.RemoveListener(PlayAll); stopButton.onClick.RemoveListener(StopAll);
            if(completeButton) completeButton.onClick.RemoveListener(CompleteAll);
            hapticToggle.onValueChanged.RemoveListener(SetHaptic);
            foreach(var h in handles.ToArray()) h.Stop(); handles.Clear();
        }
    }
}
