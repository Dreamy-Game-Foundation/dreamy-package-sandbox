using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dreamy.Feedback.Samples.Economy.Tests
{
    public sealed class FeedbackEconomyTests
    {
        [Test] public void RetryDoesNotEmitAnotherCommittedReward()
        {
            var wallet = new FeedbackDemoWallet(100); int rewards = 0;
            using (var presenter = new FeedbackRewardPresenter(wallet, new Dreamy.Economy.ResourceId("currency.coin"), (_, delta) => { if (delta > 0) rewards++; }))
            {
                var grant = new Dreamy.Economy.ResourceGrantRequest("same-transaction", new Dreamy.Economy.ResourceAmount(new Dreamy.Economy.ResourceId("currency.coin"), 100));
                Assert.That(wallet.TryGrant(grant), Is.True); Assert.That(wallet.TryGrant(grant), Is.True);
                Assert.That(wallet.GetBalance(new Dreamy.Economy.ResourceId("currency.coin")), Is.EqualTo(200)); Assert.That(rewards, Is.EqualTo(1));
            }
        }
        [Test] public void DisposedPresenterStopsObservingWallet()
        {
            var wallet = new FeedbackDemoWallet(100); int calls = 0;
            var presenter = new FeedbackRewardPresenter(wallet, new Dreamy.Economy.ResourceId("currency.coin"), (_, delta) => calls++); presenter.Dispose();
            wallet.TryGrant(new Dreamy.Economy.ResourceGrantRequest("new", new Dreamy.Economy.ResourceAmount(new Dreamy.Economy.ResourceId("currency.coin"), 100)));
            Assert.That(calls, Is.Zero);
        }
        [UnityTest] public IEnumerator ImportedPrefabGrantsOnceAndCanReopen()
        {
#if UNITY_EDITOR
            var guid = System.Array.Find(AssetDatabase.FindAssets("FeedbackEconomyPanel t:Prefab"), id => AssetDatabase.GUIDToAssetPath(id).Contains("Dreamy Feedback"));
            Assert.That(guid, Is.Not.Null);
            var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
            var panel = obj.GetComponent<FeedbackEconomyPanel>(); var wallet = new FeedbackDemoWallet(100);
            panel.Configure(wallet, wallet); yield return null;
            panel.Grant(); panel.Retry(); Assert.That(panel.RewardFeedbackCount, Is.EqualTo(1));
            Assert.That(wallet.GetBalance(new Dreamy.Economy.ResourceId("currency.coin")), Is.EqualTo(200));
            obj.SetActive(false); yield return null; obj.SetActive(true); yield return null;
            panel.Grant(); Assert.That(panel.RewardFeedbackCount, Is.EqualTo(2));
            Object.Destroy(obj); yield return null;
#else
            Assert.Ignore("Prefab import verification requires the Editor AssetDatabase."); yield return null;
#endif
        }
    }
}
