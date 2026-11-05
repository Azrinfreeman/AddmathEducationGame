using System;
using System.Collections.Generic;

namespace MathFun.OnlineData
{
    // ------------------------------------------------------------------
    // Part 1: what is saved on the device (Unity JsonUtility format).
    // ------------------------------------------------------------------

    /// <summary>One finished quiz/game result saved locally.</summary>
    [Serializable]
    public class LocalAttempt
    {
        public string attemptId;
        public string activityId;
        public int score = -1;      // -1 means "no score" (outcome-only games)
        public int maxScore = -1;
        public string outcome;      // completed, won or lost
        public string completedAt;  // ISO-8601 UTC time on the device
    }

    /// <summary>Score summary downloaded from the server for results played on other devices.</summary>
    [Serializable]
    public class LocalSummary
    {
        public string activityId;
        public int attempts;
        public int bestScore = -1;
        public int wins;
        public int losses;
    }

    [Serializable]
    public class LocalPlayer
    {
        public string playerId;              // UUID, the identity of this child everywhere
        public int legacyPlayerId = -1;      // old PlayerPrefs number, -1 if the profile is new
        public string name;
        public string gender;                // male / female / other / empty
        public int age;                      // 0 = not set
        public string avatarId;
        public int revision;                 // server revision last seen
        public bool acknowledged;            // true once the server confirmed this profile
        public bool remotelyDeleted;         // the server no longer has it; uploads stopped
        public List<string> conditionIds = new List<string>();
        public List<LocalAttempt> attempts = new List<LocalAttempt>();
        public List<LocalSummary> serverSummaries = new List<LocalSummary>();
    }

    /// <summary>A change waiting to be sent. The JSON body is frozen when the change is made.</summary>
    [Serializable]
    public class PendingOperation
    {
        public string operationId;
        public string type;          // see OperationTypes
        public string playerId;
        public string payloadJson;
        public string failureCode;   // set only on rejected operations
    }

    public static class OperationTypes
    {
        public const string PlayerCreate = "player_create";
        public const string PlayerUpdate = "player_update";
        public const string ProgressPut = "progress_put";
        public const string Attempt = "attempt";
        public const string LegacyImport = "legacy_import";
    }

    /// <summary>The whole local save file.</summary>
    [Serializable]
    public class SaveDocument
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string ownerKey = "guest";      // "guest" or "account:<username>"
        public string sourceInstallId;          // identifies this installation to the server
        public bool legacyConverted;            // old PlayerPrefs backup already written
        public bool mirrorLegacyPrefs;          // account only: keep copying the old PlayerPrefs data to the server
        public List<LocalPlayer> players = new List<LocalPlayer>();
        public List<PendingOperation> pendingOperations = new List<PendingOperation>();
        public List<PendingOperation> rejectedOperations = new List<PendingOperation>();

        public LocalPlayer FindPlayer(string playerId)
        {
            return players.Find(p => p.playerId == playerId);
        }
    }

    // ------------------------------------------------------------------
    // Part 2: what the server sends back. Field names match the JSON exactly
    // (snake_case), because Unity's JsonUtility needs identical names.
    // ------------------------------------------------------------------

    [Serializable] public class ApiErrorBody { public string code; public string message; }

    [Serializable] public class ErrorEnvelope { public string request_id; public ApiErrorBody error; }

    [Serializable]
    public class ServerPlayer
    {
        public string player_id;
        public string name;
        public string gender;
        public int age;
        public string avatar_id;
        public int revision;
    }

    [Serializable] public class ServerProgress { public string condition_id; }

    [Serializable]
    public class ServerSummary
    {
        public string activity_id;
        public int attempts;
        public int best_score = -1;
        public int wins;
        public int losses;
    }

    [Serializable] public class LoginData { public string token; public string expires_at; public string username; }
    [Serializable] public class LoginEnvelope { public LoginData data; }

    [Serializable] public class PlayerData { public ServerPlayer player; }
    [Serializable] public class PlayerEnvelope { public PlayerData data; }

    [Serializable] public class PlayersData { public List<ServerPlayer> players; }
    [Serializable] public class PlayersEnvelope { public PlayersData data; }

    [Serializable]
    public class StateData
    {
        public ServerPlayer player;
        public List<ServerProgress> progress;
        public List<ServerSummary> summaries;
    }
    [Serializable] public class StateEnvelope { public StateData data; }
}
