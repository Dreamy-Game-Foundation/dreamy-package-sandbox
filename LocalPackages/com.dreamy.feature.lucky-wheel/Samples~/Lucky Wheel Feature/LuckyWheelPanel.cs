using System;
using System.Collections;
using Dreamy.LuckyWheel;
using Dreamy.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Feature.LuckyWheel.Integration
{
    public sealed class LuckyWheelPanel : UIPanel, ILuckyWheelView
    {
        [SerializeField] private RectTransform wheelRoot;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text[] rewardLabels = Array.Empty<TMP_Text>();
        [SerializeField, Min(0.1f)] private float spinDuration = 3f;
        [SerializeField, Min(1)] private int fullRotations = 5;

        private Coroutine spinRoutine;
        private string activeTransactionId;
        private int segmentCount;

        public override bool CanBack => spinRoutine == null;

        public event Action SpinRequested;
        public event Action RetryRequested;
        public event Action Activated;
        public event Action<string> RevealInterrupted;
        public event Action<string> RevealCompleted;
        public event Action CloseRequested;

        private void OnEnable()
        {
            spinButton.onClick.AddListener(RequestSpin);
            retryButton.onClick.AddListener(RequestRetry);
            closeButton.onClick.AddListener(RequestClose);
            Activated?.Invoke();
        }

        protected override void OnDisable()
        {
            spinButton.onClick.RemoveListener(RequestSpin);
            retryButton.onClick.RemoveListener(RequestRetry);
            closeButton.onClick.RemoveListener(RequestClose);
            if (spinRoutine != null)
            {
                StopCoroutine(spinRoutine);
                spinRoutine = null;
                string interruptedTransactionId = activeTransactionId;
                activeTransactionId = null;
                RevealInterrupted?.Invoke(interruptedTransactionId);
            }

            base.OnDisable();
        }

        public void Render(LuckyWheelViewState state)
        {
            segmentCount = state.Segments.Count;
            if (rewardLabels.Length != segmentCount)
            {
                throw new InvalidOperationException(
                    $"Lucky Wheel sample requires {segmentCount} reward labels but has {rewardLabels.Length}.");
            }

            for (int index = 0; index < state.Segments.Count; index++)
            {
                var reward = state.Segments[index].Reward;
                rewardLabels[index].text = $"{reward.ResourceId.Value}\n×{reward.Amount}";
            }

            retryButton.gameObject.SetActive(state.CanRetry);
            string status = state.Availability switch
            {
                LuckyWheelAvailability.Ready => "Ready to spin",
                LuckyWheelAvailability.Cooldown => $"Next spin: {state.NextSpinUtc:HH:mm:ss} UTC",
                LuckyWheelAvailability.PendingGrant => "Reward grant failed — retry safely",
                LuckyWheelAvailability.AwaitingReveal => "Revealing saved reward…",
                _ => state.Availability.ToString()
            };
            if (state.Availability != LuckyWheelAvailability.Ready ||
                !statusText.text.StartsWith("Won ", StringComparison.Ordinal))
            {
                statusText.text = status;
            }
        }

        public void SetSpinInteractable(bool interactable)
        {
            spinButton.interactable = interactable && spinRoutine == null;
            closeButton.interactable = spinRoutine == null;
        }

        public void PlaySpin(LuckyWheelSpinResult result)
        {
            if (!result.HasOutcome || result.SegmentIndex < 0 || result.SegmentIndex >= segmentCount)
            {
                ShowError(LuckyWheelSpinStatus.InvalidReveal);
                return;
            }

            if (spinRoutine != null)
            {
                StopCoroutine(spinRoutine);
            }

            activeTransactionId = result.TransactionId;
            spinRoutine = StartCoroutine(SpinToSegment(result));
        }

        public void ShowError(LuckyWheelSpinStatus status)
        {
            statusText.text = $"Spin error: {status}";
        }

        public void Close() => Hide();

        private IEnumerator SpinToSegment(LuckyWheelSpinResult result)
        {
            spinButton.interactable = false;
            retryButton.interactable = false;
            closeButton.interactable = false;
            statusText.text = "Spinning…";

            float startAngle = wheelRoot.localEulerAngles.z;
            float segmentAngle = 360f / segmentCount;
            float selectedAngle = result.SegmentIndex * segmentAngle;
            float alignmentDelta = Mathf.Repeat(selectedAngle - startAngle, 360f);
            float targetAngle = startAngle + fullRotations * 360f + alignmentDelta;
            float elapsed = 0f;

            while (elapsed < spinDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / spinDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                wheelRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(startAngle, targetAngle, eased));
                yield return null;
            }

            wheelRoot.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
            spinRoutine = null;
            retryButton.interactable = true;
            closeButton.interactable = true;
            statusText.text = $"Won {result.Reward.Value.Amount} {result.Reward.Value.ResourceId.Value}!";
            string completedTransactionId = activeTransactionId;
            activeTransactionId = null;
            RevealCompleted?.Invoke(completedTransactionId);
        }

        private void RequestSpin() => SpinRequested?.Invoke();
        private void RequestRetry() => RetryRequested?.Invoke();
        private void RequestClose() => CloseRequested?.Invoke();
    }
}
