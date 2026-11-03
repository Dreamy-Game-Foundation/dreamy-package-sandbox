#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Localization;
using Dreamy.Template.Localization;
using Dreamy.UI;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Dreamy.Template.Tests.Localization
{
    public sealed class LocalizationSceneTests
    {
        private const string Key = "dreamy.localization.locale";
        private LocalizationTestRoot root;
        private string saved;
        private bool hadSaved;
        [UnitySetUp]
        public IEnumerator Setup()
        {
            hadSaved=PlayerPrefs.HasKey(Key); saved=PlayerPrefs.GetString(Key);
            PlayerPrefs.SetString(Key,"en");
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Project/Scenes/LocalizationSettingsTest.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return Until(()=> (root=UnityEngine.Object.FindFirstObjectByType<LocalizationTestRoot>()) != null && root.IsReady);
            yield return Until(()=>HasText("Localization test"));
            Assert.That(UnityEngine.Object.FindObjectsByType<PanelManager>(FindObjectsInactive.Include,FindObjectsSortMode.None),Is.Empty,"Test scene must not create a PanelManager");
            Assert.That(UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions.All(s=>s!=null),Is.True,"Runtime defaults must survive a cold settings reload");
        }
        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if(root!=null) UnityEngine.Object.Destroy(root.gameObject);
            yield return null;
            if(hadSaved) PlayerPrefs.SetString(Key,saved); else PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
        [UnityTest]
        public IEnumerator DropdownChangesPanelAndSceneAndReopens()
        {
            root.OpenSettings(); yield return Until(()=>UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>()!=null);
            var panel=UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>();
            var dropdown=panel.GetComponentInChildren<TMP_Dropdown>();
            Assert.That(dropdown.options.Count,Is.EqualTo(root.Service.AvailableLocaleCodes.Count));
            Assert.That(dropdown.options.All(option=>option.image!=null),Is.True,"Every configured locale must have its flag");
            Assert.That(dropdown.captionImage.sprite.name,Is.EqualTo("us"));
            dropdown.value=LocaleIndex("vi");
            yield return Until(()=>root.Service.CurrentLocaleCode=="vi" && HasText("Cài đặt") && HasText("Bạn nhận được 42 xu"));
            Assert.That(PlayerPrefs.GetString(Key),Is.EqualTo("vi"));
            Assert.That(dropdown.captionImage.sprite.name,Is.EqualTo("vn"));
            panel.Close(); yield return null;
            root.OpenSettings(); yield return Until(()=>UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>()!=null);
            panel=UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>();
            dropdown=panel.GetComponentInChildren<TMP_Dropdown>();
            yield return Until(()=>HasText("Cài đặt"));
            Assert.That(dropdown.value,Is.EqualTo(LocaleIndex("vi")));
            dropdown.value=LocaleIndex("en");
            yield return Until(()=>HasText("Settings") && HasText("You received 42 coins"));
        }
        [UnityTest]
        public IEnumerator SameLocaleDoesNotEmitAndUnsupportedLocaleDoesNotChange()
        {
            int changes=0; root.Service.LocaleChanged+=_=>changes++;
            yield return root.Service.SetLocaleAsync("en").ToCoroutine();
            bool accepted=true;
            yield return root.Service.SetLocaleAsync("xx-XX").ToCoroutine(result=>accepted=result);
            Assert.That(accepted,Is.False); Assert.That(changes,Is.Zero); Assert.That(root.Service.CurrentLocaleCode,Is.EqualTo("en"));
        }
        [UnityTest]
        public IEnumerator ExternalLocaleChangeUpdatesOpenSelectorWithoutLoop()
        {
            root.OpenSettings(); yield return Until(()=>UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>()!=null);
            int changes=0;root.Service.LocaleChanged+=_=>changes++;
            yield return root.Service.SetLocaleAsync("vi").ToCoroutine();
            var dropdown=UnityEngine.Object.FindFirstObjectByType<StandaloneSettingsView>().GetComponentInChildren<TMP_Dropdown>();
            yield return Until(()=>dropdown.value==LocaleIndex("vi") && HasText("Cài đặt"));
            Assert.That(changes,Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator CancelledRequestDoesNotCommit()
        {
            using var cancellation=new CancellationTokenSource(); cancellation.Cancel();
            bool cancelled=false;
            yield return root.Service.SetLocaleAsync("vi",cancellation.Token).SuppressCancellationThrow().ToCoroutine(result=>cancelled=result.IsCanceled);
            Assert.That(cancelled,Is.True);Assert.That(root.Service.CurrentLocaleCode,Is.EqualTo("en"));
        }
        [UnityTest]
        public IEnumerator ReloadRestoresPreference()
        {
            yield return root.Service.SetLocaleAsync("vi").ToCoroutine();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Project/Scenes/LocalizationSettingsTest.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return Until(()=>(root=UnityEngine.Object.FindFirstObjectByType<LocalizationTestRoot>())!=null&&root.IsReady);
            yield return Until(()=>HasText("Bạn nhận được 42 xu"));
            Assert.That(root.Service.CurrentLocaleCode,Is.EqualTo("vi"));
        }
        [UnityTest]
        public IEnumerator ArgumentsUpdateAfterLocaleChange()
        {
            yield return root.Service.SetLocaleAsync("vi").ToCoroutine();
            var button=GameObject.Find("Increment Amount").GetComponent<UnityEngine.UI.Button>();button.onClick.Invoke();
            yield return Until(()=>HasText("Bạn nhận được 43 xu"));
            yield return root.Service.SetLocaleAsync("en").ToCoroutine();
            yield return Until(()=>HasText("You received 43 coins"));
        }
        [UnityTest]
        public IEnumerator ConcurrentInitializationAndDisposeAreSafe()
        {
            yield return UniTask.WhenAll(root.Service.InitializeAsync(),root.Service.InitializeAsync()).ToCoroutine();
            var service=root.Service; service.Dispose(); service.Dispose();
            bool rejected=false;
            yield return RejectDisposed(service,()=>rejected=true).ToCoroutine();
            Assert.That(rejected,Is.True);
        }
        [UnityTest]
        public IEnumerator SettingsWithoutGatewayRemainsAvailable()
        {
            var model=new Dreamy.Settings.SettingsModel(new Dreamy.Audio.AudioService());
            Assert.That(model.GetLanguageState().IsAvailable,Is.False);
            yield return model.SetLanguageAsync("vi").ToCoroutine();
            Assert.That(model.GetLanguageState().ErrorKey,Is.Null);
            Assert.That(model.GetState().MusicVolume,Is.InRange(0f,1f));
            var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Dreamy Settings/0.2.0/Settings Feature/Prefabs/SettingsPanel.prefab");
            var instance=UnityEngine.Object.Instantiate(prefab);
            var panel=instance.GetComponent<SettingsPanel>();
            var presenter=new Dreamy.Settings.SettingsPresenter(model,panel);
            try
            {
                presenter.Show();
                var dropdown=panel.GetComponentInChildren<TMP_Dropdown>(true);
                Assert.That(dropdown,Is.Not.Null);
                Assert.That(dropdown.gameObject.activeSelf,Is.False,"No gateway must hide the actual Settings package dropdown");
                var label=panel.GetComponentsInChildren<TMP_Text>(true).Single(t=>t.name=="Language Label");
                Assert.That(label.gameObject.activeSelf,Is.False);
                panel.RenderLanguage(new Dreamy.Settings.SettingsLanguageState(new[]{new Dreamy.Settings.SettingsLanguageOption("en","English")},"en",false,null));
                Assert.That(dropdown.gameObject.activeSelf,Is.True);
                panel.RenderLanguage(default);
                Assert.That(dropdown.gameObject.activeSelf,Is.False);
                Assert.That(label.gameObject.activeSelf,Is.False);
                Assert.That(dropdown.options,Is.Empty,"Unavailable localization must clear stale options");
                Assert.That(dropdown.captionImage.enabled,Is.False,"Unavailable localization must hide the previous flag");
                Assert.That(UnityEngine.Object.FindObjectsByType<PanelManager>(FindObjectsInactive.Include,FindObjectsSortMode.None),Is.Empty);
            }
            finally{presenter.Dispose();UnityEngine.Object.Destroy(instance);}
        }
        [UnityTest]
        public IEnumerator FailedGatewayLeavesSelectorRetryable()
        {
            var model=new Dreamy.Settings.SettingsModel(new Dreamy.Audio.AudioService(),languageGateway:new FailingGateway());
            yield return model.SetLanguageAsync("vi").ToCoroutine();
            Assert.That(model.GetLanguageState().ErrorKey,Is.EqualTo("settings.language_failed"));
            Assert.That(model.GetLanguageState().IsBusy,Is.False);
            Assert.That(model.GetLanguageState().CurrentCode,Is.EqualTo("en"));
        }
        private sealed class FailingGateway : Dreamy.Settings.ISettingsLanguageGateway
        {
            public System.Collections.Generic.IReadOnlyList<Dreamy.Settings.SettingsLanguageOption> Options { get; }=new[]{new Dreamy.Settings.SettingsLanguageOption("en","English")};
            public string CurrentCode=>"en";
            public event Action LanguageChanged { add{} remove{} }
            public UniTask<bool> SetLanguageAsync(string code,CancellationToken token=default)=>UniTask.FromException<bool>(new InvalidOperationException("Injected load failure"));
        }
        [UnityTest]
        public IEnumerator ComponentCanReplaceReferenceWithoutOldCallbacks()
        {
            var owner=new GameObject("Runtime localized label"); owner.SetActive(false);
            var text=owner.AddComponent<TextMeshProUGUI>(); text.text="Authored label";
            var binding=owner.AddComponent<Dreamy.Localization.UI.LocalizedTmpText>();
            try
            {
                binding.Configure(new LocalizationKey("DreamyGame","settings.title"),"Fallback");
                owner.SetActive(true); yield return Until(()=>text.text=="Settings");
                var old=binding.StringReference;
                binding.StringReference=new UnityEngine.Localization.LocalizedString {TableReference="DreamyGame",TableEntryReference="settings.music"};
                yield return Until(()=>text.text=="Music");
                old.SetReference("DreamyGame","settings.language");yield return null;
                Assert.That(text.text,Is.EqualTo("Music"));
                owner.SetActive(false);binding.SetReference("DreamyGame","settings.close");
                Assert.That(text.text,Is.EqualTo("Music"));
                owner.SetActive(true);yield return Until(()=>text.text=="Close");
                Assert.That(binding.Fallback,Is.EqualTo("Fallback"));
            }
            finally{UnityEngine.Object.Destroy(owner);}
        }
        [UnityTest]
        public IEnumerator ChangingTargetAndClearingReferenceRestoresExplicitFallback()
        {
            var owner=new GameObject("Owner");owner.SetActive(false);
            var text=owner.AddComponent<TextMeshProUGUI>();text.text="Original";
            var binding=owner.AddComponent<Dreamy.Localization.UI.LocalizedTmpText>();
            var other=new GameObject("External target");var target=other.AddComponent<TextMeshProUGUI>();target.text="External authored";
            try
            {
                binding.SetReference("DreamyGame","settings.title");owner.SetActive(true);
                yield return Until(()=>text.text=="Settings");
                binding.SetTarget(target);yield return Until(()=>target.text=="Settings");
                Assert.That(binding.Fallback,Is.EqualTo("External authored"));
                binding.SetFallback("Explicit fallback");var old=binding.StringReference;binding.ClearReference();
                Assert.That(target.text,Is.EqualTo("Explicit fallback"));
                old.SetReference("DreamyGame","settings.close");yield return null;
                Assert.That(target.text,Is.EqualTo("Explicit fallback"));Assert.That(text.text,Is.EqualTo("Settings"));
            }
            finally{UnityEngine.Object.Destroy(owner);UnityEngine.Object.Destroy(other);}
        }
        [UnityTest]
        public IEnumerator StringEventListenersSurviveResetAndDisableStopsUpdates()
        {
            var owner=new GameObject("Localized event");owner.SetActive(false);
            var binding=owner.AddComponent<Dreamy.Localization.UI.LocalizedStringEvent>();int count=0;string value=null;
            binding.ValueChanged.AddListener(text=>{count++;value=text;});
            try
            {
                binding.Configure(new LocalizationKey("DreamyGame","settings.title"),"Event fallback");binding.Reset();
                owner.SetActive(true);yield return Until(()=>value=="Settings");
                count=0;owner.SetActive(false);yield return root.Service.SetLocaleAsync("vi").ToCoroutine();yield return null;
                Assert.That(count,Is.Zero);
                owner.SetActive(true);yield return Until(()=>value=="Cài đặt");
                Assert.That(count,Is.GreaterThanOrEqualTo(1));
            }
            finally{UnityEngine.Object.Destroy(owner);}
        }
        [UnityTest]
        public IEnumerator AssetComponentsCaptureAndRestoreWithoutRecapturingTranslatedValue()
        {
            var owner=new GameObject("Runtime asset binding");
            var renderer=owner.AddComponent<SpriteRenderer>();
            var texture=new Texture2D(2,2);var authored=Sprite.Create(texture,new Rect(0,0,2,2),Vector2.zero);
            renderer.sprite=authored;
            var binding=owner.AddComponent<Dreamy.Localization.UI.LocalizedSpriteRenderer>();
            try
            {
                Assert.That(binding.Target,Is.SameAs(renderer));Assert.That(binding.Fallback,Is.SameAs(authored));
                renderer.sprite=null;binding.Refresh();Assert.That(renderer.sprite,Is.SameAs(authored));
                binding.SetFallback(null);binding.Refresh();Assert.That(renderer.sprite,Is.Null);Assert.That(binding.Fallback,Is.Null);
                var replacement=new UnityEngine.Localization.LocalizedSprite();binding.AssetReference=replacement;
                Assert.That(binding.AssetReference,Is.SameAs(replacement));
            }
            finally{UnityEngine.Object.Destroy(owner);UnityEngine.Object.Destroy(authored);UnityEngine.Object.Destroy(texture);}
            yield return null;
        }
        [UnityTest]
        public IEnumerator EveryConfiguredLanguageAppliesThroughDropdownWithGlyphCoverage()
        {
            root.OpenSettings(); yield return null;
            var dropdown=root.SettingsView.GetComponentInChildren<TMP_Dropdown>();
            var cases=new[]
            {
                new[]{"de","Einstellungen","Du hast 42 Münzen erhalten"},
                new[]{"es","Ajustes","Has recibido 42 monedas"},
                new[]{"fr","Paramètres","Vous avez reçu 42 pièces"},
                new[]{"id","Pengaturan","Kamu menerima 42 koin"},
                new[]{"ja","設定","42コインを獲得しました"},
                new[]{"ko","설정","코인 42개를 받았습니다"},
                new[]{"pt-BR","Configurações","Você recebeu 42 moedas"},
                new[]{"ru","Настройки","Вы получили 42 монет"},
                new[]{"th","การตั้งค่า","คุณได้รับ 42 เหรียญ"},
                new[]{"zh-Hans","设置","你获得了42枚金币"},
                new[]{"vi","Cài đặt","Bạn nhận được 42 xu"},
                new[]{"en","Settings","You received 42 coins"}
            };
            CollectionAssert.AreEquivalent(cases.Select(c=>c[0]),root.Service.AvailableLocaleCodes);
            foreach(var test in cases)
            {
                dropdown.value=LocaleIndex(test[0]);
                yield return Until(()=>root.Service.CurrentLocaleCode==test[0] && HasText(test[1]) && HasText(test[2]));
                Assert.That(PlayerPrefs.GetString(Key),Is.EqualTo(test[0]));
                foreach(var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                    foreach(char c in text.text.Where(c=>!char.IsControl(c)))
                        Assert.That(text.font.HasCharacter(c,true,true),Is.True,$"Missing glyph U+{(int)c:X4} in {test[0]}: {text.name}");
            }
        }
        [UnityTest]
        public IEnumerator DropdownRowsDoNotOverlapAndAllLanguagesCanScrollIntoView()
        {
            root.OpenSettings(); yield return null;
            var dropdown=root.SettingsView.GetComponentInChildren<TMP_Dropdown>();
            dropdown.Show(); yield return new WaitForSeconds(.2f);
            Canvas.ForceUpdateCanvases();
            var list=dropdown.transform.Find("Dropdown List");
            Assert.That(list,Is.Not.Null);
            var rows=list.GetComponentsInChildren<UnityEngine.UI.Toggle>().Select(t=>(RectTransform)t.transform).ToArray();
            Assert.That(rows.Length,Is.EqualTo(12));
            for(int i=0;i<rows.Length;i++)
            {
                Assert.That(rows[i].rect.height,Is.GreaterThanOrEqualTo(64));
                var text=rows[i].GetComponentInChildren<TMP_Text>();text.ForceMeshUpdate();
                Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height),text.text);
                Assert.That(text.isTextOverflowing,Is.False,text.text);
                var image=rows[i].Find("Item Flag").GetComponent<UnityEngine.UI.Image>();
                Assert.That(image.sprite,Is.Not.Null);
                var imageCorners=new Vector3[4];var labelCorners=new Vector3[4];
                image.rectTransform.GetWorldCorners(imageCorners);text.rectTransform.GetWorldCorners(labelCorners);
                Assert.That(imageCorners[2].x,Is.LessThanOrEqualTo(labelCorners[0].x),"Flag must not overlap the option text");
                if(i>0) Assert.That(Mathf.Abs(rows[i].anchoredPosition.y-rows[i-1].anchoredPosition.y),Is.GreaterThanOrEqualTo(rows[i].rect.height));
            }
            var scroll=list.GetComponent<UnityEngine.UI.ScrollRect>();
            Assert.That(scroll.vertical,Is.True);Assert.That(scroll.horizontal,Is.False);
            Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
            scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;
            var corners=new Vector3[4];rows.Last().GetWorldCorners(corners);
            Assert.That(scroll.viewport.rect.Contains(scroll.viewport.InverseTransformPoint((corners[0]+corners[2])*.5f)),Is.True,"Last language must be reachable by scrolling");
            dropdown.Hide();
        }
        [UnityTest]
        public IEnumerator TextOnlyDropdownStillChangesLanguageWithoutIconCatalog()
        {
            root.OpenSettings();yield return null;
            var view=root.SettingsView;
            var data=new UnityEditor.SerializedObject(view);data.FindProperty("languageIcons").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
            view.RenderLanguage(root.Settings.GetLanguageState());
            var dropdown=view.GetComponentInChildren<TMP_Dropdown>();
            Assert.That(dropdown.options.All(option=>option.image==null),Is.True);
            Assert.That(dropdown.captionImage.enabled,Is.False);
            dropdown.value=LocaleIndex("vi");
            yield return Until(()=>root.Service.CurrentLocaleCode=="vi" && HasText("Cài đặt"));
        }
        [UnityTest]
        public IEnumerator FoundationStaticAndDynamicTextUpdatesWithoutPanelManager()
        {
            var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Panel/FoundationDemoPanel.prefab");
            var instance=UnityEngine.Object.Instantiate(prefab);
            var panel=instance.GetComponent<Dreamy.Template.Demo.FoundationDemoPanel>();
            try
            {
                panel.SetScore(123);panel.SetHealth(75);
                panel.SetLocalizedStatus("main.saved","Saved",new{value=7});
                yield return Until(()=>HasText("Score: 123") && HasText("Health: 75") && HasText("Saved | Coins: 7"));
                yield return root.Service.SetLocaleAsync("vi").ToCoroutine();
                yield return Until(()=>HasText("Điểm: 123") && HasText("Máu: 75") && HasText("Đã lưu | Xu: 7") && HasText("Thêm điểm"));
                yield return root.Service.SetLocaleAsync("ja").ToCoroutine();
                yield return Until(()=>HasText("スコア: 123") && HasText("体力: 75") && HasText("保存完了 | コイン: 7"));
                foreach(var text in instance.GetComponentsInChildren<TMP_Text>(true))
                {
                    Assert.That(text.GetComponent<Dreamy.Localization.UI.LocalizedTmpText>(),Is.Not.Null,text.name);
                    foreach(char c in text.text.Where(c=>!char.IsControl(c)))Assert.That(text.font.HasCharacter(c,true,true),Is.True,text.text);
                }
                Assert.That(UnityEngine.Object.FindObjectsByType<PanelManager>(FindObjectsInactive.Include,FindObjectsSortMode.None),Is.Empty);
            }
            finally{UnityEngine.Object.Destroy(instance);}
        }
        private int LocaleIndex(string code)=>Array.IndexOf(root.Service.AvailableLocaleCodes.ToArray(),code);
        private static async UniTask RejectDisposed(ILocalizationService service,Action onRejected)
        { try{await service.SetLocaleAsync("vi");}catch(ObjectDisposedException){onRejected();} }
        private static bool HasText(string value)=>UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text==value);
        private static IEnumerator Until(Func<bool> condition)
        { float deadline=Time.realtimeSinceStartup+30;while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),"Timed out waiting for localization/UI");yield return null;} }
    }
}
#endif
