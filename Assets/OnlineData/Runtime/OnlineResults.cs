using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathFun.OnlineData
{
    /// <summary>
    /// One-line calls for the old game scripts to report a finished quiz or game.
    /// The current child is found from the old PlayerPrefs "PlayerID", so no scene changes are needed.
    /// Activity IDs must exist in Server/config/activities.php.
    ///
    ///   OnlineResults.Scored("quiz_level1", score, maxScore);
    ///   OnlineResults.Outcome(OnlineResults.SceneActivity("game_algorithm_", "algoLvl"), won);
    /// </summary>
    public static class OnlineResults
    {
        /// <summary>Records a finished game that has a score. Use maxScore -1 if unknown.</summary>
        public static void Scored(string activityId, int score, int maxScore)
        {
            LocalPlayer child = CurrentChild();
            if (child == null) return;
            if (maxScore >= 0) score = Mathf.Clamp(score, 0, maxScore);
            PlayerDataService.Instance.RecordScoredResult(child.playerId, activityId, Mathf.Max(0, score), maxScore);
        }

        /// <summary>Records a finished game that is only won or lost.</summary>
        public static void Outcome(string activityId, bool won)
        {
            LocalPlayer child = CurrentChild();
            if (child == null) return;
            PlayerDataService.Instance.RecordOutcomeResult(child.playerId, activityId, won);
        }

        /// <summary>
        /// Builds an ID from the scene name, e.g. SceneActivity("game_algorithm_", "algoLvl") in scene
        /// "algoLvl7" gives "game_algorithm_7". Without removePrefix, the cleaned scene name is appended.
        /// </summary>
        public static string SceneActivity(string idPrefix, string removePrefix = "")
        {
            string scene = SceneManager.GetActiveScene().name;
            if (removePrefix.Length > 0 && scene.StartsWith(removePrefix)) scene = scene.Substring(removePrefix.Length);

            StringBuilder cleaned = new StringBuilder();
            foreach (char c in scene.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) && c < 128) cleaned.Append(c);
            }
            return idPrefix + cleaned;
        }

        private static LocalPlayer CurrentChild()
        {
            if (!PlayerPrefs.HasKey("PlayerID")) return null;
            return PlayerDataService.Instance.FindLegacyPlayer(PlayerPrefs.GetInt("PlayerID"));
        }
    }
}
