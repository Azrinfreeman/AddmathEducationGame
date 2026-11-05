<?php
/*
 * Lesson completion and earned achievements.
 * Progress only ever grows: an upload can add conditions but never removes one.
 */
declare(strict_types=1);

/** Returns every earned condition of a player. */
function load_progress(array $player): array
{
    $rows = fetch_all(
        'SELECT condition_id, provenance, earned_at FROM player_progress WHERE player_id = ? ORDER BY condition_id',
        [$player['id']]
    );
    return array_map(fn($r) => [
        'condition_id' => $r['condition_id'],
        'provenance' => $r['provenance'],
        'earned_at' => gmdate('c', strtotime($r['earned_at'] . ' UTC')),
    ], $rows);
}

/** Adds conditions (ignoring ones already stored). Used by the progress route and the legacy import. */
function add_conditions(int $playerRowId, array $conditionIds, string $provenance): int
{
    $added = 0;
    foreach ($conditionIds as $conditionId) {
        $statement = run_query(
            'INSERT IGNORE INTO player_progress (player_id, condition_id, provenance, earned_at) VALUES (?, ?, ?, ?)',
            [$playerRowId, $conditionId, $provenance, now_utc()]
        );
        $added += $statement->rowCount();
    }
    return $added;
}

/** Validates a list of condition ids from a request: unique, known, and not too many. */
function read_condition_ids($value): array
{
    $max = (int) app_config()['max_conditions_per_request'];
    if (!is_array($value) || count($value) > $max) {
        throw invalid('condition_ids', "condition_ids must be a list of at most $max items.");
    }
    $clean = [];
    foreach ($value as $id) {
        $clean[validate_condition_id($id)] = true;
    }
    $ids = array_keys($clean);
    sort($ids);
    return $ids;
}

function put_progress(int $accountId, string $playerUuid, array $body): void
{
    $conditionIds = read_condition_ids($body['condition_ids'] ?? null);

    [$status, $data] = run_idempotent($accountId, $body['operation_id'] ?? null, 'progress_put',
        ['player_id' => $playerUuid, 'condition_ids' => $conditionIds],
        function () use ($accountId, $playerUuid, $conditionIds) {
            $player = find_owned_player($accountId, $playerUuid, true);
            $added = add_conditions((int) $player['id'], $conditionIds, 'played');
            return [200, ['added' => $added, 'progress' => load_progress($player)]];
        });
    send_success($data, $status);
}
