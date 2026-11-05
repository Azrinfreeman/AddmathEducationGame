<?php
/*
 * Finished quiz/game results ("attempts").
 * The attempt's own UUID is the retry key: sending the same attempt twice stores it once.
 */
declare(strict_types=1);

/** Checks the request against config/activities.php and returns clean, ready-to-store values. */
function read_attempt_fields(array $body): array
{
    $activities = activity_config();
    $activityId = $body['activity_id'] ?? null;
    $rule = is_string($activityId) ? find_attempt_rule($activities, $activityId) : null;
    if ($rule === null) {
        throw invalid('activity_id', 'Unknown activity.');
    }
    $ceiling = (int) $activities['score_ceiling'];

    $score = optional_int($body['score'] ?? null, 'score', $ceiling);
    $maxScore = optional_int($body['max_score'] ?? null, 'max_score', $ceiling);
    $outcome = $body['outcome'] ?? null;

    if ($rule['kind'] === 'scored') {
        if ($score === null) {
            throw invalid('score', 'This activity needs a score.');
        }
        $limit = $rule['max_score'] ?? $ceiling;
        if ($score > $limit || ($maxScore !== null && $score > $maxScore)) {
            throw invalid('score', 'Score is higher than this activity allows.');
        }
        $outcome = 'completed';
    } else {
        // Outcome-only games never carry a score: no invented zeros.
        if ($score !== null || $maxScore !== null || !in_array($outcome, ['won', 'lost'], true)) {
            throw invalid('outcome', 'This activity needs outcome "won" or "lost" and no score.');
        }
    }

    $contentVersion = optional_int($body['content_version'] ?? 1, 'content_version', 65535) ?? 1;

    return [
        'activity_id' => $activityId,
        'content_version' => $contentVersion,
        'result_kind' => $rule['kind'],
        'outcome' => $outcome,
        'score' => $score,
        'max_score' => $maxScore,
        'duration_ms' => optional_int($body['duration_ms'] ?? null, 'duration_ms', 86400000),
        'completed_at' => validate_device_time($body['completed_at'] ?? null, 'completed_at'),
    ];
}

/** Looks the activity up by exact id, then by pattern (see config/activities.php). Null if unknown. */
function find_attempt_rule(array $activities, string $activityId): ?array
{
    if (isset($activities['attempts'][$activityId])) {
        return $activities['attempts'][$activityId];
    }
    foreach ($activities['attempt_patterns'] ?? [] as $pattern => $rule) {
        if (preg_match($pattern, $activityId)) {
            return $rule;
        }
    }
    return null;
}

function attempt_to_array(array $row): array
{
    return [
        'attempt_id' => $row['attempt_uuid'],
        'activity_id' => $row['activity_id'],
        'content_version' => (int) $row['content_version'],
        'result_kind' => $row['result_kind'],
        'outcome' => $row['outcome'],
        'score' => $row['score'] === null ? null : (int) $row['score'],
        'max_score' => $row['max_score'] === null ? null : (int) $row['max_score'],
        'duration_ms' => $row['duration_ms'] === null ? null : (int) $row['duration_ms'],
        'completed_at' => gmdate('c', strtotime($row['completed_at'] . ' UTC')),
        'received_at' => gmdate('c', strtotime($row['received_at'] . ' UTC')),
    ];
}

function record_attempt(int $accountId, string $playerUuid, array $body): void
{
    $attemptUuid = require_uuid($body['attempt_id'] ?? null, 'attempt_id');
    $fields = read_attempt_fields($body);
    $payloadHash = fingerprint($fields);

    [$status, $attempt] = in_transaction(function () use ($accountId, $playerUuid, $attemptUuid, $fields, $payloadHash) {
        $player = find_owned_player($accountId, $playerUuid, true);

        $existing = fetch_one('SELECT * FROM activity_attempts WHERE attempt_uuid = ?', [$attemptUuid]);
        if ($existing !== null) {
            // Retry of an attempt we already stored: same player + same data => return it again.
            if ((int) $existing['player_id'] !== (int) $player['id'] || $existing['payload_hash'] !== $payloadHash) {
                throw new ApiError(409, 'attempt_conflict', 'This attempt id was already used with different data.');
            }
            return [200, attempt_to_array($existing)];
        }

        run_query(
            'INSERT INTO activity_attempts
               (attempt_uuid, player_id, activity_id, content_version, result_kind, outcome, score, max_score, duration_ms, completed_at, received_at, payload_hash)
             VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)',
            [$attemptUuid, $player['id'], $fields['activity_id'], $fields['content_version'], $fields['result_kind'], $fields['outcome'],
             $fields['score'], $fields['max_score'], $fields['duration_ms'], $fields['completed_at'], now_utc(), $payloadHash]
        );
        return [201, attempt_to_array(fetch_one('SELECT * FROM activity_attempts WHERE attempt_uuid = ?', [$attemptUuid]))];
    });
    send_success(['attempt' => $attempt], $status);
}

/** Paged history, newest first. "before" is the cursor returned as next_cursor. */
function list_attempts(int $accountId, string $playerUuid, array $query): void
{
    $player = find_owned_player($accountId, $playerUuid);
    $maxPage = (int) app_config()['attempts_page_size_max'];
    $limit = isset($query['limit']) && ctype_digit((string) $query['limit']) ? min((int) $query['limit'], $maxPage) : min(50, $maxPage);
    $limit = max(1, $limit);
    $before = isset($query['before']) && ctype_digit((string) $query['before']) ? (int) $query['before'] : PHP_INT_MAX;

    // $limit is a checked integer, so it is safe to write straight into the SQL.
    $rows = fetch_all(
        "SELECT * FROM activity_attempts WHERE player_id = ? AND seq < ? ORDER BY seq DESC LIMIT " . ($limit + 1),
        [$player['id'], $before]
    );
    $hasMore = count($rows) > $limit;
    $rows = array_slice($rows, 0, $limit);
    send_success([
        'attempts' => array_map('attempt_to_array', $rows),
        'next_cursor' => $hasMore ? (string) end($rows)['seq'] : null,
    ]);
}

/**
 * Per-activity summary: how many results, best and latest score, wins and losses.
 * Outcome-only games never contribute to score numbers.
 */
function load_attempt_summaries(array $player): array
{
    $totals = fetch_all(
        "SELECT activity_id, COUNT(*) AS attempts, MAX(score) AS best_score,
                SUM(outcome = 'won') AS wins, SUM(outcome = 'lost') AS losses
         FROM activity_attempts WHERE player_id = ? GROUP BY activity_id",
        [$player['id']]
    );
    $latest = fetch_all(
        'SELECT activity_id, score FROM activity_attempts
         WHERE seq IN (SELECT MAX(seq) FROM activity_attempts WHERE player_id = ? GROUP BY activity_id)',
        [$player['id']]
    );
    $latestScore = [];
    foreach ($latest as $row) {
        $latestScore[$row['activity_id']] = $row['score'] === null ? null : (int) $row['score'];
    }

    $summaries = [];
    foreach ($totals as $row) {
        $id = $row['activity_id'];
        $isScored = ($latestScore[$id] ?? null) !== null || $row['best_score'] !== null;
        $summaries[] = [
            'activity_id' => $id,
            'attempts' => (int) $row['attempts'],
            'best_score' => $isScored ? (int) $row['best_score'] : null,
            'latest_score' => $latestScore[$id] ?? null,
            'wins' => (int) $row['wins'],
            'losses' => (int) $row['losses'],
        ];
    }
    return $summaries;
}
