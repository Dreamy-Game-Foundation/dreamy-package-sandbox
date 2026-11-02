using System;
using System.IO;
using System.Linq;
using Dreamy.Localization.Editor.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
namespace Dreamy.Template.Tests.Localization
{
    public sealed class LocalizationImportTests
    {
        private LocalizationJsonImportProfile Profile => AssetDatabase.LoadAssetAtPath<LocalizationJsonImportProfile>("Assets/_Project/Localization/Profiles/JsonImport.asset");
        private string[] Snapshot() => Directory.GetFiles("Assets/_Project/Localization", "*",SearchOption.AllDirectories)
            .OrderBy(p=>p,StringComparer.Ordinal).Select(p=>p+Convert.ToBase64String(File.ReadAllBytes(p))).ToArray();
        [Test]
        public void PreviewIsReadOnlyAndReportsCoverage()
        {
            var before=Snapshot(); var plan=LocalizationJsonImporter.Preview(Profile);
            Assert.That(plan.Locales.Count,Is.EqualTo(2));Assert.That(plan.EntryCount,Is.GreaterThanOrEqualTo(17));
            Assert.That(Snapshot(),Is.EqualTo(before));
        }
        [Test]
        public void StalePlanIsRejectedBeforeMutation()
        {
            var profile=Profile;var plan=LocalizationJsonImporter.Preview(profile);var before=Snapshot();var table=profile.Table;
            try {profile.Table+="_stale";Assert.Throws<InvalidOperationException>(()=>LocalizationJsonImporter.Apply(plan));}
            finally {profile.Table=table;}
            Assert.That(Snapshot(),Is.EqualTo(before));
        }
        [Test]
        public void ReimportPreservesTableGuidsEntryIdsAndContent()
        {
            var profile=Profile;
            var collection=LocalizationEditorSettings.GetStringTableCollections().Single(c=>c.TableCollectionName==profile.Table);
            var guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(collection));
            var ids=collection.SharedData.Entries.Select(e=>e.Key+":"+e.Id).OrderBy(s=>s).ToArray();
            LocalizationJsonImporter.Apply(LocalizationJsonImporter.Preview(profile));
            var afterFirst=Snapshot();
            LocalizationJsonImporter.Apply(LocalizationJsonImporter.Preview(profile));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(collection)),Is.EqualTo(guid));
            Assert.That(collection.SharedData.Entries.Select(e=>e.Key+":"+e.Id).OrderBy(s=>s).ToArray(),Is.EqualTo(ids));
            Assert.That(Snapshot(),Is.EqualTo(afterFirst));
            Assert.That(LocalizationEditorSettings.ActiveLocalizationSettings.GetStringDatabase().UseFallback,Is.True);
            var table=(StringTable)collection.GetTable(new UnityEngine.Localization.LocaleIdentifier("vi"));
            Assert.That(table.GetEntry("settings.title").Value,Is.EqualTo("Cài đặt"));
        }
    }
}
