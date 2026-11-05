<?php
/*
 * Imports one profile from the OLD local-only app (PlayerPrefs) into the account.
 * "source_install_id" identifies the phone/tablet; "legacy_player_id" is the old number (1, 2, ...).
 * Together they are the key, so retrying an import never creates a second child.
 * Shared quiz scores are NOT accepted here - nobody can tell which child earned them.
 */
declare(strict_types=1);

function import_legacy_player(int $accountId, array $body): void
{
    $sourceInstallId = require_uuid($body['source_install_id'] ?? null, 'source_install_id');
    $legacyPlayerId = optional_int($body['legacy_player_id'] ?? null, 'legacy_player_id', 1000000);
    if ($legacyPlayerId === null) {
        throw invalid('legacy_player_id', 'legacy_player_id is required.');
    }
    $playerUuid = require_uuid($body['player_id'] ?? null, 'player_id');
    $fields = read_player_fields($body, true);
    $conditionIds = read_condition_ids($body['condition_ids'] ?? []);

    $request = ['source_install_id' => $sourceInstallId, 'legacy_player_id' => $legacyPlayerId,
        'player_id' => $playerUuid, 'condition_ids' => $conditionIds] + $fields;
    $payloadHash = fingerprint($request);

    [$status, $data] = run_idempotent($accountId, $body['operation_id'] ?? null, 'legacy_import', $request,
        function () use ($accountId, $sourceInstallId, $legacyPlayerId, $playerUuid, $fields, $conditionIds, $payloadHash) {
            if (was_player_deleted($accountId, $playerUuid, $sourceInstallId, $legacyPlayerId)) {
                throw new ApiError(409, 'player_deleted', 'This player was deleted and cannot be imported again.');
            }

            $earlier = fetch_one(
                'SELECT li.payload_hash, p.player_uuid FROM legacy_imports li JOIN players p ON p.id = li.player_id
                 WHERE li.account_id = ? AND li.source_install_id = ? AND li.legacy_player_id = ?',
                [$accountId, $sourceInstallId, $legacyPlayerId]
            );
            if ($earlier !== null) {
                // Imported before (maybe with another operation id): return the same mapping.
                if ($earlier['payload_hash'] !== $payloadHash) {
                    throw new ApiError(409, 'import_conflict', 'This profile was already imported with different data.');
                }
                $player = find_owned_player($accountId, $earlier['player_uuid']);
                return [200, ['player' => player_to_array($player), 'progress' => load_progress($player)]];
            }

            if (fetch_one('SELECT id FROM players WHERE player_uuid = ?', [$playerUuid]) !== null) {
                throw new ApiError(409, 'player_exists', 'A player with this id already exists.');
            }

            $playerRowId = insert_player($accountId, $playerUuid, $fields);
            add_conditions($playerRowId, $conditionIds, 'legacy_import');
            run_query(
                'INSERT INTO legacy_imports (account_id, source_install_id, legacy_player_id, player_id, payload_hash, created_at) VALUES (?, ?, ?, ?, ?, ?)',
                [$accountId, $sourceInstallId, $legacyPlayerId, $playerRowId, $payloadHash, now_utc()]
            );
            $player = find_owned_player($accountId, $playerUuid);
            return [201, ['player' => player_to_array($player), 'progress' => load_progress($player)]];
        });
    send_success($data, $status);
}
