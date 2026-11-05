using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MathFun.OnlineData
{
    /// <summary>
    /// Reads the OLD local-only data (PlayerPrefs) and converts it into local players.
    /// It only READS PlayerPrefs. It never deletes or changes the old keys, so the original
    /// data stays available until you decide to clean it up.
    ///
    /// PlayerPrefs cannot list its keys, so the keys we know about are listed below.
    /// If you add a new lesson/game flag to the old code, add its name to KnownConditionKeys
    /// (and to Server/config/activities.php).
    /// </summary>
    public static class LegacyPlayerPrefsImporter
    {
        /// <summary>Per-player flags; the real key is Name + "_" + playerId, and the value 1 means done.</summary>
        public static readonly string[] KnownConditionKeys =
        {
            "Level1Pra", "Level2Pra", "Level3Pra",
            "Level1Tahap1", "Level2Tahap1", "Level3Tahap1", "Level4Tahap1",
            "Level1Tahap2", "Level2Tahap2", "Level3Tahap2",
            "Kuiz0", "Kuiz1", "Kuiz2",
            "GamePra", "GameLevel1", "GameLevel2",
            "Menu1", "Menu2", "Menu3", "Menu4", "Menu5", "Menu6",
            "High1", "High2", "High3", "PraGameKuiz",
        };

        /// <summary>Shared values that belong to nobody in particular: saved in the backup only.</summary>
        private static readonly string[] SharedKeysForBackup =
        {
            "QuizScoreEasy", "QuizScoreMedium", "QuizScoreHard", "Tahap1_2", "Tahap1_3", "Tahap1_4",
        };

        /// <summary>True when the old app has stored at least one profile.</summary>
        public static bool HasLegacyPlayers()
        {
            return PlayerPrefs.GetInt("PlayerTotal", 0) > 0;
        }

        /// <summary>Reads every old profile. Missing or malformed entries are skipped, never fatal.</summary>
        public static List<LocalPlayer> ReadPlayers()
        {
            List<LocalPlayer> players = new List<LocalPlayer>();
            int total = PlayerPrefs.GetInt("PlayerTotal", 0);
            HashSet<int> seenIds = new HashSet<int>();

            for (int index = 1; index <= total; index++)
            {
                if (!PlayerPrefs.HasKey("PlayerID_" + index)) continue;   // gap in the numbering
                int legacyId = PlayerPrefs.GetInt("PlayerID_" + index);
                if (!seenIds.Add(legacyId)) continue;                       // same id listed twice

                string name = PlayerPrefs.GetString("PlayerName_" + legacyId, "").Trim();
                if (name.Length == 0) continue;                             // nothing worth keeping

                LocalPlayer player = new LocalPlayer
                {
                    playerId = Guid.NewGuid().ToString(),
                    legacyPlayerId = legacyId,
                    name = name,
                    gender = PlayerPrefs.GetString("PlayerGender_" + legacyId, ""),
                    age = PlayerPrefs.GetInt("PlayerAge_" + legacyId, 0),
                    avatarId = PlayerPrefs.GetString("PlayerAvatar_" + legacyId, ""),
                };
                foreach (string key in KnownConditionKeys)
                {
                    if (PlayerPrefs.GetInt(key + "_" + legacyId, 0) == 1) player.conditionIds.Add(key);
                }
                players.Add(player);
            }
            return players;
        }

        /// <summary>
        /// A private text copy of everything we recognise, including the shared scores that cannot
        /// be given to a single child. It contains children's names: keep it private, never commit it.
        /// </summary>
        public static string BuildBackupText(List<LocalPlayer> players)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("MathFun legacy PlayerPrefs backup " + DateTime.UtcNow.ToString("o"));
            foreach (LocalPlayer player in players)
            {
                text.AppendLine("player " + player.legacyPlayerId + ": " + player.name + ", age " + player.age
                    + ", gender " + player.gender + ", avatar " + player.avatarId
                    + ", done: " + string.Join(",", player.conditionIds));
            }
            foreach (string key in SharedKeysForBackup)
            {
                if (PlayerPrefs.HasKey(key)) text.AppendLine("shared " + key + " = " + PlayerPrefs.GetInt(key));
            }
            return text.ToString();
        }
    }
}
