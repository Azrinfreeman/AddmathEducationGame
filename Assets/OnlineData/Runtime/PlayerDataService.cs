using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathFun.OnlineData
{
    /// <summary>
    /// The one place the rest of the game talks to for player data.
    ///
    /// Two kinds of workspace:
    ///  - guest   : saved on the device only, nothing is uploaded.
    ///  - account : saved on the device AND queued for upload while an adult is signed in.
    /// Every change is written to the local file first, so the game works offline.
    /// Use PlayerDataService.Instance from any script.
    /// </summary>
    public sealed class PlayerDataService : MonoBehaviour
    {
        private static PlayerDataService instance;

        public static PlayerDataService Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject host = new GameObject("PlayerDataService");
                    DontDestroyOnLoad(host);
                    instance = host.AddComponent<PlayerDataService>();
                    instance.Initialize(ApiConfiguration.LoadOrDisabled(), new LocalSaveStore());
                }
                return instance;
            }
        }

        private ApiConfiguration configuration;
        private ApiClient api;
        private LocalSaveStore store;
        private SyncService sync;
        private SaveDocument document;
        private string token;                 // kept in memory only, never written to disk
        private string signedInUsername;
        private float nextRetryTime;

        /// <summary>Raised whenever the data or the sync state changes (refresh your UI here).</summary>
        public event Action Changed;

        public SyncState State { get; private set; } = SyncState.LocalOnly;
        public bool IsSigningInOrSyncing { get; private set; }
        public bool IsAccountWorkspace { get { return document.ownerKey != LocalSaveStore.GuestOwnerKey; } }
        public bool IsSignedIn { get { return !string.IsNullOrEmpty(token); } }
        public string AccountName { get { return IsAccountWorkspace ? signedInUsername : null; } }
        public int PendingCount { get { return document.pendingOperations.Count; } }
        public int RejectedCount { get { return document.rejectedOperations.Count; } }
        public ApiConfiguration Configuration { get { return configuration; } }

        /// <summary>Profiles shown to the player (profiles deleted online are hidden).</summary>
        public List<LocalPlayer> Players
        {
            get { return document.players.FindAll(p => !p.remotelyDeleted); }
        }

        // ------------------------------------------------------------------
        // Start-up
        // ------------------------------------------------------------------

        /// <summary>Called once. Tests call it with a temporary store and a fake configuration.</summary>
        public void Initialize(ApiConfiguration config, LocalSaveStore saveStore)
        {
            configuration = config;
            store = saveStore;
            api = new ApiClient(config);
            sync = new SyncService(api, () => document, d => store.Save(d), () => token);

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            string ownerKey = store.LoadActiveOwnerKey();
            document = store.Load(ownerKey);
            if (IsAccountWorkspace)
            {
                signedInUsername = ownerKey.Substring("account:".Length);
                State = SyncState.SignInRequired;   // the token lives in memory only, so ask again after a restart
            }
            else
            {
                MergeLegacyPlayersIntoGuest();
            }
        }

        private void Update()
        {
            bool retryDue = State == SyncState.Offline && Time.unscaledTime >= nextRetryTime;
            if (retryDue && IsSignedIn && !sync.IsRunning)
            {
                _ = SyncNowAsync();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // The old game scripts still save to PlayerPrefs. Whenever a scene loads we copy new changes across.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            MirrorLegacyPlayerPrefs();
        }

        private void OnApplicationPause(bool paused)
        {
            if (document == null) return;
            if (paused) MirrorLegacyPlayerPrefs();
            if (paused) store.Save(document);
            else if (IsSignedIn) _ = SyncNowAsync();
        }

        private void OnApplicationQuit()
        {
            if (document != null) store.Save(document);
        }

        // ------------------------------------------------------------------
        // Old PlayerPrefs data -> guest workspace
        // ------------------------------------------------------------------

        /// <summary>
        /// Copies the old PlayerPrefs profiles into the guest save (adds new ones, merges flags of known ones).
        /// The old PlayerPrefs are only read. Returns how many profiles were added or updated.
        /// </summary>
        public int MergeLegacyPlayersIntoGuest()
        {
            if (IsAccountWorkspace || !LegacyPlayerPrefsImporter.HasLegacyPlayers()) return 0;
            try
            {
                List<LocalPlayer> legacyPlayers = LegacyPlayerPrefsImporter.ReadPlayers();
                if (!document.legacyConverted)
                {
                    // Backup first. If this fails we stop and leave everything as it was.
                    string path = store.WriteBackup("legacy_playerprefs_backup.txt", LegacyPlayerPrefsImporter.BuildBackupText(legacyPlayers));
                    Debug.Log("MathFun: private backup of old local data written to " + path);
                }

                int changed = 0;
                foreach (LocalPlayer legacy in legacyPlayers)
                {
                    LocalPlayer existing = document.players.Find(p => p.legacyPlayerId == legacy.legacyPlayerId);
                    if (existing == null)
                    {
                        document.players.Add(legacy);
                        changed++;
                        continue;
                    }
                    foreach (string id in legacy.conditionIds)
                    {
                        if (!existing.conditionIds.Contains(id)) { existing.conditionIds.Add(id); changed++; }
                    }
                }
                document.legacyConverted = true;
                if (changed > 0 && !store.Save(document))
                {
                    Debug.LogError("MathFun: could not save converted players; the old data is still intact.");
                }
                return changed;
            }
            catch (Exception exception)
            {
                Debug.LogError("MathFun: legacy conversion skipped (" + exception.GetType().Name + "); old data untouched.");
                return 0;
            }
        }

        /// <summary>
        /// Account only (after the adult confirmed the transfer): copies new profiles, edits and
        /// completed lessons from the old PlayerPrefs to the upload queue. PlayerPrefs is only read.
        /// </summary>
        public void MirrorLegacyPlayerPrefs()
        {
            if (document == null || !IsAccountWorkspace || !document.mirrorLegacyPrefs) return;
            bool changed = false;
            foreach (LocalPlayer legacy in LegacyPlayerPrefsImporter.ReadPlayers())
            {
                LocalPlayer local = document.players.Find(p => p.legacyPlayerId == legacy.legacyPlayerId);
                if (local == null)
                {
                    document.players.Add(legacy);
                    QueueUploadOfNewPlayer(legacy);
                    changed = true;
                    continue;
                }
                if (local.remotelyDeleted) continue;

                bool profileDiffers = local.name != legacy.name || local.age != legacy.age
                    || local.avatarId != legacy.avatarId || local.gender != legacy.gender;
                if (profileDiffers)
                {
                    local.name = legacy.name; local.age = legacy.age; local.avatarId = legacy.avatarId; local.gender = legacy.gender;
                    if (local.acknowledged) Queue(OperationTypes.PlayerUpdate, local.playerId, ProfileFields(new JsonBody(), local));
                    else RebuildPendingCreate(local);
                    changed = true;
                }
                foreach (string id in legacy.conditionIds)
                {
                    if (local.conditionIds.Contains(id)) continue;
                    local.conditionIds.Add(id);
                    Queue(OperationTypes.ProgressPut, local.playerId, new JsonBody().AddStrings("condition_ids", new[] { id }));
                    changed = true;
                }
            }
            if (changed) Persist();
        }

        // ------------------------------------------------------------------
        // Accounts
        // ------------------------------------------------------------------

        /// <summary>Returns null on success, otherwise an error code such as "username_taken" or "offline".</summary>
        public async Task<string> RegisterAsync(string username, string password)
        {
            ApiResult result = await api.SendAsync("POST", "/v1/auth/register",
                new JsonBody().Add("username", username).Add("password", password).ToString(), null);
            if (result.NetworkFailure) return "offline";
            if (!result.IsSuccess) return result.ErrorCode;
            return await SignInAsync(username, password);
        }

        /// <summary>Returns null on success, otherwise an error code such as "invalid_credentials" or "offline".</summary>
        public async Task<string> SignInAsync(string username, string password)
        {
            IsSigningInOrSyncing = true;
            try
            {
                ApiResult result = await api.SendAsync("POST", "/v1/auth/login",
                    new JsonBody().Add("username", username).Add("password", password).ToString(), null);
                if (result.NetworkFailure) return "offline";
                if (!result.IsSuccess) return result.ErrorCode;

                LoginEnvelope login = result.Parse<LoginEnvelope>();
                if (login == null || login.data == null) return "bad_response";

                OpenAccountWorkspace(login.data.username, login.data.token);
                RequeueRejectedOperations();
                await RestoreFromServerAsync();
                await SyncNowAsync();
                return null;
            }
            finally
            {
                IsSigningInOrSyncing = false;
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Signs out. If changes are still waiting, returns false unless discardPending is true,
        /// so the caller can warn the adult first. The account's cached copy is removed from the device.
        /// </summary>
        public async Task<bool> SignOutAsync(bool discardPending)
        {
            if (!IsAccountWorkspace) return true;
            if (PendingCount > 0 && !discardPending) return false;

            string oldToken = token;
            string oldOwnerKey = document.ownerKey;
            token = null;   // forget the token immediately, even if the server cannot be reached
            if (!string.IsNullOrEmpty(oldToken))
            {
                await api.SendAsync("POST", "/v1/auth/logout", null, oldToken);
            }
            store.Delete(oldOwnerKey);
            signedInUsername = null;
            document = store.Load(LocalSaveStore.GuestOwnerKey);
            store.SaveActiveOwnerKey(document.ownerKey);
            State = SyncState.LocalOnly;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Gives refused changes another chance (they keep their order). Useful after a server fix or an app update.
        /// Changes the server refuses again simply return to the rejected list.
        /// </summary>
        public void RequeueRejectedOperations()
        {
            if (document.rejectedOperations.Count == 0) return;
            foreach (PendingOperation operation in document.rejectedOperations)
            {
                // Updates are skipped: their revision check may no longer make sense.
                LocalPlayer player = document.FindPlayer(operation.playerId);
                if (player == null || player.remotelyDeleted || operation.failureCode == "player_deleted_remotely") continue;
                operation.failureCode = null;
                document.pendingOperations.Add(operation);
            }
            document.rejectedOperations.RemoveAll(o => string.IsNullOrEmpty(o.failureCode));
            store.Save(document);
        }

        private void OpenAccountWorkspace(string username, string newToken)
        {
            string ownerKey = LocalSaveStore.AccountOwnerKey(username);
            if (document.ownerKey != ownerKey)
            {
                store.Save(document);
                document = store.Load(ownerKey);   // another account's queue is never sent with this token
            }
            signedInUsername = username;
            token = newToken;
            store.SaveActiveOwnerKey(ownerKey);
        }

        /// <summary>
        /// Moves the guest profiles (including converted old ones) into the signed-in account and queues them for upload.
        /// Call it only after the adult confirmed. Returns how many profiles were moved.
        /// </summary>
        public int TransferGuestPlayersToAccount()
        {
            if (!IsAccountWorkspace || !IsSignedIn) return 0;
            MergeLegacyPlayersIntoGuest();   // pick up anything the old screens saved since start-up

            SaveDocument guest = store.Load(LocalSaveStore.GuestOwnerKey);
            int moved = 0;
            foreach (LocalPlayer player in guest.players)
            {
                if (document.FindPlayer(player.playerId) != null) continue;
                document.players.Add(player);
                QueueUploadOfNewPlayer(player);
                moved++;
            }
            document.mirrorLegacyPrefs = true;   // from now on new play on this device keeps flowing to the account
            if (moved == 0) { store.Save(document); return 0; }

            // Save the account copy first; only then empty the guest copy, so a crash cannot lose children.
            if (!store.Save(document))
            {
                return 0;
            }
            guest.players.Clear();
            store.Save(guest);
            _ = SyncNowAsync();
            Changed?.Invoke();
            return moved;
        }

        // ------------------------------------------------------------------
        // Profiles, progress and results (what the game calls)
        // ------------------------------------------------------------------

        public LocalPlayer CreatePlayer(string name, string gender, int age, string avatarId)
        {
            LocalPlayer player = new LocalPlayer
            {
                playerId = Guid.NewGuid().ToString(),
                name = name, gender = gender ?? "", age = age, avatarId = avatarId ?? "",
            };
            document.players.Add(player);
            if (IsAccountWorkspace) QueueUploadOfNewPlayer(player);
            Persist();
            return player;
        }

        public void UpdatePlayer(string playerId, string name, string gender, int age, string avatarId)
        {
            LocalPlayer player = document.FindPlayer(playerId);
            if (player == null) return;
            player.name = name; player.gender = gender ?? ""; player.age = age; player.avatarId = avatarId ?? "";
            if (IsAccountWorkspace && player.acknowledged)
            {
                Queue(OperationTypes.PlayerUpdate, playerId, ProfileFields(new JsonBody(), player));
            }
            else if (IsAccountWorkspace)
            {
                // The create is still waiting: replace it so the server receives the latest values.
                RebuildPendingCreate(player);
            }
            Persist();
        }

        /// <summary>
        /// Finds the child that the old screens call "PlayerID n". If this workspace does not know the
        /// child yet, the old PlayerPrefs are read again first. Null when the child is not in this workspace
        /// (for example a guest child while an adult account is open and the transfer is not confirmed).
        /// </summary>
        public LocalPlayer FindLegacyPlayer(int legacyPlayerId)
        {
            LocalPlayer found = document.players.Find(p => p.legacyPlayerId == legacyPlayerId && !p.remotelyDeleted);
            if (found != null) return found;
            if (IsAccountWorkspace) MirrorLegacyPlayerPrefs(); else MergeLegacyPlayersIntoGuest();
            return document.players.Find(p => p.legacyPlayerId == legacyPlayerId && !p.remotelyDeleted);
        }

        public bool IsConditionEarned(string playerId, string conditionId)
        {
            LocalPlayer player = document.FindPlayer(playerId);
            return player != null && player.conditionIds.Contains(conditionId);
        }

        /// <summary>Marks a lesson/achievement as done. Safe to call many times.</summary>
        public void MarkConditionEarned(string playerId, string conditionId)
        {
            LocalPlayer player = document.FindPlayer(playerId);
            if (player == null || player.conditionIds.Contains(conditionId)) return;
            player.conditionIds.Add(conditionId);
            if (IsAccountWorkspace)
            {
                Queue(OperationTypes.ProgressPut, playerId, new JsonBody().AddStrings("condition_ids", new[] { conditionId }));
            }
            Persist();
        }

        /// <summary>Records a finished quiz/game that has a score. Pass maxScore -1 if the maximum is not fixed.</summary>
        public void RecordScoredResult(string playerId, string activityId, int score, int maxScore)
        {
            AddAttempt(playerId, new LocalAttempt { activityId = activityId, score = score, maxScore = maxScore, outcome = "completed" });
        }

        /// <summary>Records a finished game that is only won or lost (no score).</summary>
        public void RecordOutcomeResult(string playerId, string activityId, bool won)
        {
            AddAttempt(playerId, new LocalAttempt { activityId = activityId, outcome = won ? "won" : "lost" });
        }

        /// <summary>Best score including results played on other devices; -1 when there is none.</summary>
        public int GetBestScore(string playerId, string activityId)
        {
            LocalPlayer player = document.FindPlayer(playerId);
            if (player == null) return -1;
            int best = -1;
            foreach (LocalAttempt attempt in player.attempts)
            {
                if (attempt.activityId == activityId) best = Math.Max(best, attempt.score);
            }
            foreach (LocalSummary summary in player.serverSummaries)
            {
                if (summary.activityId == activityId) best = Math.Max(best, summary.bestScore);
            }
            return best;
        }

        private void AddAttempt(string playerId, LocalAttempt attempt)
        {
            LocalPlayer player = document.FindPlayer(playerId);
            if (player == null) return;
            attempt.attemptId = Guid.NewGuid().ToString();
            attempt.completedAt = DateTime.UtcNow.ToString("o");
            player.attempts.Add(attempt);
            if (IsAccountWorkspace) QueueAttempt(player, attempt);
            Persist();
        }

        // ------------------------------------------------------------------
        // Building the upload queue
        // ------------------------------------------------------------------

        private static JsonBody ProfileFields(JsonBody body, LocalPlayer player)
        {
            return body.Add("name", player.name)
                .Add("gender", string.IsNullOrEmpty(player.gender) ? null : player.gender)
                .AddNullableNumber("age", player.age > 0 ? player.age : -1)
                .Add("avatar_id", string.IsNullOrEmpty(player.avatarId) ? null : player.avatarId);
        }

        /// <summary>Queues everything the server needs to know about a player that is new to the account.</summary>
        private void QueueUploadOfNewPlayer(LocalPlayer player)
        {
            RebuildPendingCreate(player);
            // Legacy imports carry their flags inside the import; new profiles send them as progress.
            if (player.legacyPlayerId < 0 && player.conditionIds.Count > 0)
            {
                Queue(OperationTypes.ProgressPut, player.playerId, new JsonBody().AddStrings("condition_ids", player.conditionIds));
            }
            foreach (LocalAttempt attempt in player.attempts) QueueAttempt(player, attempt);
        }

        /// <summary>Puts (or replaces) the create/import operation at the front of this player's changes.</summary>
        private void RebuildPendingCreate(LocalPlayer player)
        {
            PendingOperation existing = document.pendingOperations.Find(
                o => o.playerId == player.playerId && (o.type == OperationTypes.PlayerCreate || o.type == OperationTypes.LegacyImport));
            int position = existing != null ? document.pendingOperations.IndexOf(existing) : document.pendingOperations.Count;
            if (existing != null) document.pendingOperations.Remove(existing);

            JsonBody body = new JsonBody().Add("player_id", player.playerId);
            string type = OperationTypes.PlayerCreate;
            if (player.legacyPlayerId >= 0)
            {
                type = OperationTypes.LegacyImport;
                body.Add("source_install_id", document.sourceInstallId)
                    .Add("legacy_player_id", player.legacyPlayerId)
                    .AddStrings("condition_ids", player.conditionIds);
            }
            ProfileFields(body, player);
            // Keep the original operation id on a replacement so an earlier send is recognised as a retry.
            string operationId = existing != null ? existing.operationId : Guid.NewGuid().ToString();
            body.Add("operation_id", operationId);
            document.pendingOperations.Insert(position, new PendingOperation
            {
                operationId = operationId, type = type, playerId = player.playerId, payloadJson = body.ToString(),
            });
        }

        private void QueueAttempt(LocalPlayer player, LocalAttempt attempt)
        {
            JsonBody body = new JsonBody()
                .Add("attempt_id", attempt.attemptId)
                .Add("activity_id", attempt.activityId)
                .Add("outcome", attempt.outcome)
                .AddNullableNumber("score", attempt.score)
                .AddNullableNumber("max_score", attempt.maxScore)
                .Add("completed_at", attempt.completedAt);
            Queue(OperationTypes.Attempt, player.playerId, body);
        }

        private void Queue(string type, string playerId, JsonBody body)
        {
            string operationId = Guid.NewGuid().ToString();
            body.Add("operation_id", operationId);
            document.pendingOperations.Add(new PendingOperation
            {
                operationId = operationId, type = type, playerId = playerId, payloadJson = body.ToString(),
            });
        }

        // ------------------------------------------------------------------
        // Saving and syncing
        // ------------------------------------------------------------------

        /// <summary>Writes the local file, then asks the sync to run. Local success never means "uploaded".</summary>
        private void Persist()
        {
            if (!store.Save(document))
            {
                Debug.LogError("MathFun: progress could not be saved on this device.");
            }
            if (IsAccountWorkspace && IsSignedIn) _ = SyncNowAsync();
            Changed?.Invoke();
        }

        public async Task SyncNowAsync()
        {
            if (!IsAccountWorkspace) { State = SyncState.LocalOnly; Changed?.Invoke(); return; }
            if (!IsSignedIn) { State = SyncState.SignInRequired; Changed?.Invoke(); return; }
            if (sync.IsRunning) return;

            IsSigningInOrSyncing = true;
            Changed?.Invoke();
            State = await sync.RunAsync();
            if (State == SyncState.Offline) nextRetryTime = Time.unscaledTime + sync.SuggestedRetrySeconds;
            IsSigningInOrSyncing = false;
            Changed?.Invoke();
        }

        /// <summary>Downloads the account's profiles and merges them in. Missing flags never delete anything.</summary>
        public async Task RestoreFromServerAsync()
        {
            SaveDocument target = document;
            ApiResult listResult = await api.SendAsync("GET", "/v1/players", null, token);
            PlayersEnvelope list = listResult.IsSuccess ? listResult.Parse<PlayersEnvelope>() : null;
            if (list == null || list.data == null || list.data.players == null) return;

            HashSet<string> serverIds = new HashSet<string>();
            foreach (ServerPlayer remote in list.data.players)
            {
                serverIds.Add(remote.player_id);
                LocalPlayer local = target.FindPlayer(remote.player_id);
                bool profileEditsWaiting = target.pendingOperations.Exists(o => o.playerId == remote.player_id
                    && (o.type == OperationTypes.PlayerCreate || o.type == OperationTypes.PlayerUpdate || o.type == OperationTypes.LegacyImport));
                if (local == null)
                {
                    local = new LocalPlayer { playerId = remote.player_id, acknowledged = true };
                    target.players.Add(local);
                }
                if (!profileEditsWaiting && remote.revision >= local.revision)
                {
                    local.name = remote.name; local.gender = remote.gender ?? ""; local.age = remote.age;
                    local.avatarId = remote.avatar_id ?? ""; local.revision = remote.revision; local.acknowledged = true;
                }

                ApiResult stateResult = await api.SendAsync("GET", "/v1/players/" + remote.player_id + "/state", null, token);
                if (!ReferenceEquals(target, document)) return;   // account switched while downloading
                StateEnvelope state = stateResult.IsSuccess ? stateResult.Parse<StateEnvelope>() : null;
                if (state == null || state.data == null) continue;

                foreach (ServerProgress progress in state.data.progress)
                {
                    if (!local.conditionIds.Contains(progress.condition_id)) local.conditionIds.Add(progress.condition_id);
                }
                local.serverSummaries.Clear();
                foreach (ServerSummary summary in state.data.summaries)
                {
                    local.serverSummaries.Add(new LocalSummary
                    {
                        activityId = summary.activity_id, attempts = summary.attempts,
                        bestScore = summary.best_score, wins = summary.wins, losses = summary.losses,
                    });
                }
            }

            // A profile we had confirmed before, but the complete list no longer has: deleted elsewhere.
            // Profiles still waiting for their first upload are NOT treated as deleted.
            foreach (LocalPlayer local in target.players)
            {
                if (!local.acknowledged || local.remotelyDeleted || serverIds.Contains(local.playerId)) continue;
                local.remotelyDeleted = true;
                List<PendingOperation> orphaned = target.pendingOperations.FindAll(o => o.playerId == local.playerId);
                foreach (PendingOperation operation in orphaned)
                {
                    target.pendingOperations.Remove(operation);
                    operation.failureCode = "player_deleted_remotely";
                    target.rejectedOperations.Add(operation);   // kept for recovery, never re-created automatically
                }
            }
            store.Save(target);
            Changed?.Invoke();
        }
    }
}
