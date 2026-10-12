using System;
using System.IO;
using Dreamy.Datasave;
using NUnit.Framework;

namespace Dreamy.Template.Tests
{
    public sealed class TemplateSaveTests
    {
        private string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "DreamyTemplateSaveTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TearDown]
        public void TearDown() => Directory.Delete(directory, true);

        [Test]
        public void VersionOnePreservesExistingFields()
        {
            WriteFixture(1, "{\"LaunchCount\":7,\"Coins\":125,\"Score\":42}");
            var data = CreateService().Load<TemplateSave>();
            Assert.That(data.LaunchCount, Is.EqualTo(7));
            Assert.That(data.Coins, Is.EqualTo(125));
            Assert.That(data.Score, Is.EqualTo(42));
        }

        [Test]
        public void VersionTwoMigratesRenamedFieldsAndRoundTrips()
        {
            WriteFixture(2, "{\"DemoOpenCount\":1,\"CurrentScore\":36,\"BestScore\":50,\"IsInitialized\":true,\"Version\":2,\"SaveKey\":\"TemplateSave\"}");
            var service = CreateService();
            var data = service.Load<TemplateSave>();
            Assert.That(data.LaunchCount, Is.EqualTo(1));
            Assert.That(data.Score, Is.EqualTo(36));
            Assert.That(data.BestScore, Is.EqualTo(50));
            Assert.That(data.IsInitialized, Is.True);
            data.LaunchCount++;
            data.Score = 40;
            service.Save(data);

            var loaded = CreateService().Load<TemplateSave>();
            Assert.That(loaded.Version, Is.EqualTo(3));
            Assert.That(loaded.LaunchCount, Is.EqualTo(2));
            Assert.That(loaded.Score, Is.EqualTo(40));
            Assert.That(loaded.BestScore, Is.EqualTo(50));
            Assert.That(loaded.IsInitialized, Is.True);
            Assert.That(File.ReadAllText(SavePath), Does.Not.Contain("DemoOpenCount"));
            Assert.That(File.ReadAllText(SavePath), Does.Not.Contain("CurrentScore"));
        }

        [Test]
        public void FutureVersionIsRejectedWithoutChangingFiles()
        {
            WriteFixture(4, "{\"LaunchCount\":8,\"Score\":99}");
            File.Copy(SavePath, SavePath + ".bak");
            var original = File.ReadAllText(SavePath);
            Assert.Throws<DatasaveException>(() => CreateService().Load<TemplateSave>());
            Assert.That(File.ReadAllText(SavePath), Is.EqualTo(original));
            Assert.That(File.ReadAllText(SavePath + ".bak"), Is.EqualTo(original));
        }

        private string SavePath => Path.Combine(directory, "TemplateSave.json");

        private DatasaveService CreateService() => new DatasaveService(new DatasaveOptions
        {
            DirectoryName = directory,
            CreateFileOnFirstLoad = false
        });

        private void WriteFixture(int version, string payload)
        {
            var escapedPayload = payload.Replace("\\", "\\\\").Replace("\"", "\\\"");
            File.WriteAllText(SavePath, "{\"FormatVersion\":1,\"DataType\":\"" +
                typeof(TemplateSave).AssemblyQualifiedName + "\",\"DataVersion\":" + version +
                ",\"Payload\":\"" + escapedPayload + "\"}");
        }
    }
}
