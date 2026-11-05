using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace MathFun.OnlineData.Tests
{
    public class OnlineDataTests
    {
        private string folder;
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "mathfun_test_" + System.Guid.NewGuid());
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }

        private PlayerDataService NewService()
        {
            ApiConfiguration offline = ScriptableObject.CreateInstance<ApiConfiguration>();
            offline.onlineEnabled = false;
            host = new GameObject("test");
            PlayerDataService service = host.AddComponent<PlayerDataService>();
            service.Initialize(offline, new LocalSaveStore(folder));
            return service;
        }

        [Test]
        public void JsonBody_WritesNullsAndEscapes()
        {
            string json = new JsonBody().Add("name", "A\"b").AddNullableNumber("score", -1).Add("ok", true).ToString();
            Assert.AreEqual("{\"name\":\"A\\\"b\",\"score\":null,\"ok\":true}", json);
        }

        [Test]
        public void SaveStore_RoundTripsAndFallsBackToBackup()
        {
            LocalSaveStore store = new LocalSaveStore(folder);
            SaveDocument document = store.Load("guest");
            document.players.Add(new LocalPlayer { playerId = "p1", name = "Ali" });
            Assert.IsTrue(store.Save(document));
            document.players[0].name = "Ali2";
            Assert.IsTrue(store.Save(document));       // creates the .bak of the first save

            File.WriteAllText(Path.Combine(folder, "guest.json"), "{broken");
            SaveDocument recovered = store.Load("guest");
            Assert.AreEqual("Ali", recovered.players[0].name);   // last good copy
        }

        [Test]
        public void GuestChanges_AreSavedButNotQueued()
        {
            PlayerDataService service = NewService();
            LocalPlayer player = service.CreatePlayer("Aina", "female", 7, "");
            service.MarkConditionEarned(player.playerId, "Level1Pra");
            Assert.IsTrue(service.IsConditionEarned(player.playerId, "Level1Pra"));
            Assert.AreEqual(0, service.PendingCount);
            Assert.AreEqual(SyncState.LocalOnly, service.State);
        }

        [Test]
        public void ConditionAndScore_AreRemembered()
        {
            PlayerDataService service = NewService();
            LocalPlayer player = service.CreatePlayer("Aina", "female", 7, "");
            service.RecordScoredResult(player.playerId, "quiz_easy", 6, -1);
            service.RecordScoredResult(player.playerId, "quiz_easy", 4, -1);
            Assert.AreEqual(6, service.GetBestScore(player.playerId, "quiz_easy"));
            Assert.AreEqual(-1, service.GetBestScore(player.playerId, "quiz_hard"));
        }
    }
}
