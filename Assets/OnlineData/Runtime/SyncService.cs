using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MathFun.OnlineData
{
    public enum SyncState
    {
        LocalOnly,       // not signed in: everything stays on this device
        SignInRequired,  // changes are waiting; the adult must sign in
        Offline,         // server not reachable (or busy); will retry
        Synced,          // nothing waiting
        NeedsAttention,  // the server refused some changes (see rejectedOperations)
    }

    /// <summary>
    /// Sends the waiting changes (pendingOperations) to the server, one at a time, in order.
    /// A change is removed from the queue only after the server confirms it, and the file is saved.
    /// </summary>
    public sealed class SyncService
    {
        private readonly ApiClient api;
        private readonly Func<SaveDocument> getDocument;
        private readonly Func<SaveDocument, bool> saveDocument;
        private readonly Func<string> getToken;
        private bool isRunning;

        public SyncService(ApiClient api, Func<SaveDocument> getDocument, Func<SaveDocument, bool> saveDocument, Func<string> getToken)
        {
            this.api = api;
            this.getDocument = getDocument;
            this.saveDocument = saveDocument;
            this.getToken = getToken;
        }

        public bool IsRunning { get { return isRunning; } }

        /// <summary>Seconds the caller should wait before trying again (from Retry-After, or a default).</summary>
        public int SuggestedRetrySeconds { get; private set; }

        public async Task<SyncState> RunAsync()
        {
            if (isRunning) return SyncState.Offline;
            isRunning = true;
            try
            {
                return await SendAllAsync();
            }
            finally
            {
                isRunning = false;
            }
        }

        private async Task<SyncState> SendAllAsync()
        {
            SaveDocument document = getDocument();
            while (document.pendingOperations.Count > 0)
            {
                string token = getToken();
                if (string.IsNullOrEmpty(token)) return SyncState.SignInRequired;

                PendingOperation operation = document.pendingOperations[0];
                ApiResult result = await SendOperationAsync(document, operation, token);

                // The adult may have signed out or switched account while we waited. Stop touching the old queue.
                if (!ReferenceEquals(document, getDocument())) return SyncState.Offline;

                if (result.Status == 401) return SyncState.SignInRequired;
                if (result.ShouldRetryLater)
                {
                    SuggestedRetrySeconds = result.RetryAfterSeconds > 0 ? result.RetryAfterSeconds : 30;
                    return SyncState.Offline;
                }

                document.pendingOperations.Remove(operation);
                if (result.IsSuccess)
                {
                    ApplyConfirmation(document, operation, result);
                }
                else
                {
                    // 404 / 409 / 422 ...: retrying the same data will never work, so set it aside for a human.
                    operation.failureCode = string.IsNullOrEmpty(result.ErrorCode) ? "http_" + result.Status : result.ErrorCode;
                    document.rejectedOperations.Add(operation);
                    Debug.LogWarning("MathFun: server rejected " + operation.type + " (" + operation.failureCode + ")");
                }
                saveDocument(document);
            }
            return document.rejectedOperations.Count > 0 ? SyncState.NeedsAttention : SyncState.Synced;
        }

        private Task<ApiResult> SendOperationAsync(SaveDocument document, PendingOperation operation, string token)
        {
            LocalPlayer player = document.FindPlayer(operation.playerId);
            switch (operation.type)
            {
                case OperationTypes.PlayerCreate:
                    return api.SendAsync("POST", "/v1/players", operation.payloadJson, token);
                case OperationTypes.LegacyImport:
                    return api.SendAsync("POST", "/v1/legacy-imports", operation.payloadJson, token);
                case OperationTypes.PlayerUpdate:
                    // The revision is added now, not when the change was made, because earlier edits
                    // in the queue may have raised it.
                    int revision = player != null ? player.revision : 0;
                    return api.SendAsync("PATCH", "/v1/players/" + operation.playerId,
                        JsonBody.WithInt(operation.payloadJson, "expected_revision", revision), token);
                case OperationTypes.ProgressPut:
                    return api.SendAsync("PUT", "/v1/players/" + operation.playerId + "/progress", operation.payloadJson, token);
                case OperationTypes.Attempt:
                    return api.SendAsync("POST", "/v1/players/" + operation.playerId + "/attempts", operation.payloadJson, token);
                default:
                    throw new InvalidOperationException("Unknown operation type " + operation.type);
            }
        }

        /// <summary>Copies what the server confirmed (revision, acknowledged) into the local player.</summary>
        private static void ApplyConfirmation(SaveDocument document, PendingOperation operation, ApiResult result)
        {
            bool changesProfile = operation.type == OperationTypes.PlayerCreate
                || operation.type == OperationTypes.PlayerUpdate
                || operation.type == OperationTypes.LegacyImport;
            if (!changesProfile) return;

            PlayerEnvelope envelope = result.Parse<PlayerEnvelope>();
            LocalPlayer player = document.FindPlayer(operation.playerId);
            if (envelope == null || envelope.data == null || envelope.data.player == null || player == null) return;

            player.acknowledged = true;
            player.revision = envelope.data.player.revision;
        }
    }
}
