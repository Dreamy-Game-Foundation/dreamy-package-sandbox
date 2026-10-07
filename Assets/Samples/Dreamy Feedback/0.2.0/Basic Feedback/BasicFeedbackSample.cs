using UnityEngine;
namespace Dreamy.Feedback.Samples
{
    public sealed class BasicFeedbackSample : MonoBehaviour
    {
        [SerializeField] private FeedbackHost host;
        public void PlaySample() { host.Initialize(); host.Sequence.Play("reward", FeedbackContext.At(transform.position)); }
    }
}
