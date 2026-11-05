<?php
/*
 * Player (child) profiles.
 * Rule used everywhere: a player is only visible if it belongs to the signed-in account.
 * A UUID alone never gives access.
 */
declare(strict_types=1);

/** Turns a database row into the JSON shape sent to the app. */
function player_to_array(array $row): array
{
    return [
        'player_id' => $row['player_uuid'],
        'name' => $row['name'],
        'gender' => $row['gender'],
        'age' => $row['age'] === null ? null : (int) $row['age'],
        'avatar_id' => $row['avatar_id'],
        'revision' => (int) $row['revision'],
        'created_at' => gmdate('c', strtotime($row['created_at'] . ' UTC')),
        'updated_at' => gmdate('c', strtotime($row['updated_at'] . ' UTC')),
    ];
}

/** Loads a player owned by this account, or throws 404 (same answer for "missing" and "someone else's"). */
function find_owned_player(int $accountId, string $playerUuid, bool $lockRow = false): array
{
    $row = fetch_one(
        'SELECT * FROM players WHERE player_uuid = ? AND account_id = ?' . ($lockRow ? ' FOR UPDATE' : ''),
        [strtolower($playerUuid), $accountId]
    );
    if ($row === null) {
        throw new ApiError(404, 'player_not_found', 'Player not found.');
    }
    return $row;
}

/** True when this player (or this old-app profile) was deleted before. */
function was_player_deleted(int $accountId, string $playerUuid, ?string $sourceInstallId = null, ?int $legacyPlayerId = null): bool
{
    $row = fetch_one(
        'SELECT id FROM deletion_tombstones
         WHERE player_uuid = ? OR (account_id = ? AND source_install_id = ? AND legacy_player_id = ?)',
        [$playerUuid, $accountId, $sourceInstallId, $legacyPlayerId]
    );
    return $row !== null;
}

/** Reads and validates the editable profile fields. All of them are optional except the name. */
function read_player_fields(array $body, bool $nameRequired): array
{
    $fields = [];
    if ($nameRequired || array_key_exists('name', $body)) {
        $fields['name'] = validate_player_name($body['name'] ?? null);
    }
    foreach (['gender' => 'validate_gender', 'age' => 'validate_age', 'avatar_id' => 'validate_avatar_id'] as $key => $validator) {
        if (array_key_exists($key, $body) || $nameRequired) {
            $fields[$key] = $validator($body[$key] ?? null);
        }
    }
    return $fields;
}

/** Inserts a new player row and returns its database id. */
function insert_player(int $accountId, string $playerUuid, array $fields): int
{
    run_query(
        'INSERT INTO players (player_uuid, account_id, name, gender, age, avatar_id, revision, created_at, updated_at)
         VALUES (?, ?, ?, ?, ?, ?, 1, ?, ?)',
        [$playerUuid, $accountId, $fields['name'], $fields['gender'], $fields['age'], $fields['avatar_id'], now_utc(), now_utc()]
    );
    return (int) db()->lastInsertId();
}

function list_players(int $accountId): void
{
    $rows = fetch_all('SELECT * FROM players WHERE account_id = ? ORDER BY id', [$accountId]);
    send_success(['players' => array_map('player_to_array', $rows)]);
}

function create_player(int $accountId, array $body): void
{
    $playerUuid = require_uuid($body['player_id'] ?? null, 'player_id');
    $fields = read_player_fields($body, true);

    [$status, $data] = run_idempotent($accountId, $body['operation_id'] ?? null, 'player_create',
        ['player_id' => $playerUuid] + $fields,
        function () use ($accountId, $playerUuid, $fields) {
            if (was_player_deleted($accountId, $playerUuid)) {
                throw new ApiError(409, 'player_deleted', 'This player was deleted and cannot be recreated.');
            }
            $existing = fetch_one('SELECT * FROM players WHERE player_uuid = ?', [$playerUuid]);
            if ($existing !== null) {
                // Same UUID again: fine if it is ours and identical, otherwise refuse (never take over).
                $sameOwner = (int) $existing['account_id'] === $accountId;
                $sameData = $existing['name'] === $fields['name'] && $existing['gender'] === $fields['gender']
                    && ($existing['age'] === null ? null : (int) $existing['age']) === $fields['age']
                    && $existing['avatar_id'] === $fields['avatar_id'];
                if ($sameOwner && $sameData) {
                    return [200, ['player' => player_to_array($existing)]];
                }
                throw new ApiError(409, 'player_exists', 'A player with this id already exists.');
            }
            insert_player($accountId, $playerUuid, $fields);
            return [201, ['player' => player_to_array(find_owned_player($accountId, $playerUuid))]];
        });
    send_success($data, $status);
}

function update_player(int $accountId, string $playerUuid, array $body): void
{
    $expectedRevision = $body['expected_revision'] ?? null;
    if (!is_int($expectedRevision)) {
        throw invalid('expected_revision', 'expected_revision is required.');
    }
    $fields = read_player_fields($body, false);
    if ($fields === []) {
        throw invalid('name', 'Nothing to change.');
    }

    [$status, $data] = run_idempotent($accountId, $body['operation_id'] ?? null, 'player_update',
        ['player_id' => $playerUuid, 'expected_revision' => $expectedRevision] + $fields,
        function () use ($accountId, $playerUuid, $expectedRevision, $fields) {
            $player = find_owned_player($accountId, $playerUuid, true);
            if ((int) $player['revision'] !== $expectedRevision) {
                // Someone else edited first. Send the current profile so the adult can choose.
                throw new ApiError(409, 'revision_conflict', 'The profile was changed on another device.', [], ['current' => player_to_array($player)]);
            }
            $merged = array_merge(
                ['name' => $player['name'], 'gender' => $player['gender'], 'age' => $player['age'], 'avatar_id' => $player['avatar_id']],
                $fields
            );
            run_query(
                'UPDATE players SET name = ?, gender = ?, age = ?, avatar_id = ?, revision = revision + 1, updated_at = ? WHERE id = ?',
                [$merged['name'], $merged['gender'], $merged['age'], $merged['avatar_id'], now_utc(), $player['id']]
            );
            return [200, ['player' => player_to_array(find_owned_player($accountId, $playerUuid))]];
        });
    send_success($data, $status);
}

/** Deletes one player and leaves a tombstone (no personal data) so old queued requests cannot bring it back. */
function delete_player(int $accountId, string $playerUuid, array $body): void
{
    if (($body['confirm'] ?? false) !== true) {
        throw invalid('confirm', 'Deletion must be confirmed.');
    }
    require_current_password($accountId, $body['password'] ?? null);

    in_transaction(function () use ($accountId, $playerUuid) {
        $player = find_owned_player($accountId, $playerUuid, true);
        $import = fetch_one('SELECT source_install_id, legacy_player_id FROM legacy_imports WHERE player_id = ?', [$player['id']]);
        run_query(
            'INSERT INTO deletion_tombstones (account_id, player_uuid, source_install_id, legacy_player_id, deleted_at) VALUES (?, ?, ?, ?, ?)',
            [$accountId, $player['player_uuid'], $import['source_install_id'] ?? null, $import['legacy_player_id'] ?? null, now_utc()]
        );
        run_query('DELETE FROM players WHERE id = ?', [$player['id']]);
    });
    send_success(['deleted' => true]);
}

/** One consistent snapshot: profile, all progress flags and score summaries. No attempt history. */
function get_player_state(int $accountId, string $playerUuid): void
{
    $state = in_transaction(function () use ($accountId, $playerUuid) {
        $player = find_owned_player($accountId, $playerUuid);
        return [
            'player' => player_to_array($player),
            'progress' => load_progress($player),
            'summaries' => load_attempt_summaries($player),
        ];
    });
    send_success($state);
}
