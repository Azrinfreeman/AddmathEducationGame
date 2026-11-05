<?php
/*
 * End-to-end API check. Talks to a running API over HTTP, so it tests the real thing.
 *
 *   php run.php https://your-site.example/api/index.php
 *
 * ONLY use a throw-away (staging) database: it creates fictional accounts and players.
 * It refuses to run unless you add the flag --i-am-using-a-test-database.
 */
if (PHP_SAPI !== 'cli') { exit; }

$baseUrl = $argv[1] ?? null;
if ($baseUrl === null || !in_array('--i-am-using-a-test-database', $argv, true)) {
    fwrite(STDERR, "Usage: php run.php API_URL --i-am-using-a-test-database\n");
    exit(1);
}

$failures = 0;

function uuid4(): string
{
    $b = random_bytes(16);
    $b[6] = chr((ord($b[6]) & 0x0f) | 0x40);
    $b[8] = chr((ord($b[8]) & 0x3f) | 0x80);
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($b), 4));
}

/** Sends one request and returns [httpStatus, decodedBody]. */
function call(string $method, string $route, ?array $body = null, ?string $token = null): array
{
    global $baseUrl;
    $headers = ['Content-Type: application/json'];
    if ($token !== null) { $headers[] = 'Authorization: Bearer ' . $token; }
    $curl = curl_init($baseUrl . '?route=' . $route);
    curl_setopt_array($curl, [CURLOPT_CUSTOMREQUEST => $method, CURLOPT_HTTPHEADER => $headers,
        CURLOPT_RETURNTRANSFER => true, CURLOPT_POSTFIELDS => $body === null ? null : json_encode($body)]);
    $raw = curl_exec($curl);
    $status = (int) curl_getinfo($curl, CURLINFO_RESPONSE_CODE);
    curl_close($curl);
    return [$status, json_decode((string) $raw, true) ?? []];
}

function check(string $label, bool $condition): void
{
    global $failures;
    echo ($condition ? 'PASS  ' : 'FAIL  ') . $label . "\n";
    if (!$condition) { $failures++; }
}

$suffix = substr(bin2hex(random_bytes(4)), 0, 8);
$password = 'test-password-123';

// Two fictional adults.
foreach (['a', 'b'] as $who) {
    [$s] = call('POST', '/v1/auth/register', ['username' => "tester_{$who}_$suffix", 'password' => $password]);
    check("register $who", $s === 201);
}
[$s, $r] = call('POST', '/v1/auth/login', ['username' => "tester_a_$suffix", 'password' => $password]);
$tokenA = $r['data']['token'] ?? '';
[, $r] = call('POST', '/v1/auth/login', ['username' => "tester_b_$suffix", 'password' => $password]);
$tokenB = $r['data']['token'] ?? '';
check('login a', $s === 200 && $tokenA !== '');
[$s] = call('POST', '/v1/auth/login', ['username' => "tester_a_$suffix", 'password' => 'wrong-password']);
check('wrong password is 401', $s === 401);
[$s] = call('GET', '/v1/players');
check('missing token is 401', $s === 401);

// Create a player and retry the same operation.
$playerId = uuid4();
$operationId = uuid4();
$create = ['operation_id' => $operationId, 'player_id' => $playerId, 'name' => 'Ali Test', 'age' => 7, 'gender' => 'male'];
[$s] = call('POST', '/v1/players', $create, $tokenA);
check('create player 201', $s === 201);
[$s] = call('POST', '/v1/players', $create, $tokenA);
check('retry same operation returns stored answer', $s === 201);
[$s] = call('POST', '/v1/players', ['name' => 'Changed'] + $create, $tokenA);
check('same operation, changed data is 409', $s === 409);
[, $r] = call('GET', '/v1/players', null, $tokenA);
check('exactly one player listed', count($r['data']['players'] ?? []) === 1);

// Owner isolation.
[$s] = call('GET', "/v1/players/$playerId/state", null, $tokenB);
check('account B cannot read A\'s player', $s === 404);

// Progress only grows.
[$s] = call('PUT', "/v1/players/$playerId/progress", ['operation_id' => uuid4(), 'condition_ids' => ['Level1Pra', 'Kuiz0']], $tokenA);
check('progress saved', $s === 200);
[$s] = call('PUT', "/v1/players/$playerId/progress", ['operation_id' => uuid4(), 'condition_ids' => ['NotARealThing']], $tokenA);
check('unknown condition is 422', $s === 422);

// Attempts are deduplicated.
$attempt = ['attempt_id' => uuid4(), 'activity_id' => 'quiz_preschool', 'score' => 7, 'completed_at' => gmdate('c')];
[$s] = call('POST', "/v1/players/$playerId/attempts", $attempt, $tokenA);
check('attempt stored 201', $s === 201);
[$s] = call('POST', "/v1/players/$playerId/attempts", $attempt, $tokenA);
check('same attempt again 200 (not duplicated)', $s === 200);
[$s] = call('POST', "/v1/players/$playerId/attempts", ['score' => 8] + $attempt, $tokenA);
check('same attempt id, different score 409', $s === 409);
[$s] = call('POST', "/v1/players/$playerId/attempts", ['attempt_id' => uuid4(), 'activity_id' => 'game_algorithm_3', 'outcome' => 'won', 'completed_at' => gmdate('c')], $tokenA);
check('outcome-only attempt stored', $s === 201);
[$s] = call('POST', "/v1/players/$playerId/attempts", ['attempt_id' => uuid4(), 'activity_id' => 'game_algorithm_3', 'outcome' => 'won', 'score' => 0, 'completed_at' => gmdate('c')], $tokenA);
check('outcome-only attempt with score is 422', $s === 422);

[, $r] = call('GET', "/v1/players/$playerId/state", null, $tokenA);
check('state shows progress and summaries', count($r['data']['progress'] ?? []) === 2 && count($r['data']['summaries'] ?? []) === 2);

// Stale edit.
[, $r] = call('PATCH', "/v1/players/$playerId", ['operation_id' => uuid4(), 'expected_revision' => 1, 'name' => 'Ali Renamed'], $tokenA);
check('edit with current revision works', ($r['data']['player']['revision'] ?? 0) === 2);
[$s] = call('PATCH', "/v1/players/$playerId", ['operation_id' => uuid4(), 'expected_revision' => 1, 'name' => 'Stale'], $tokenA);
check('edit with stale revision is 409', $s === 409);

// Legacy import is retry-safe.
$import = ['operation_id' => uuid4(), 'source_install_id' => uuid4(), 'legacy_player_id' => 1, 'player_id' => uuid4(),
    'name' => 'Old Player', 'age' => 8, 'gender' => 'perempuan', 'condition_ids' => ['Level1Pra']];
[$s] = call('POST', '/v1/legacy-imports', $import, $tokenA);
check('legacy import 201', $s === 201);
[$s] = call('POST', '/v1/legacy-imports', ['operation_id' => uuid4()] + $import, $tokenA);
check('legacy import retried with new operation id returns the same player (200)', $s === 200);
[, $r] = call('GET', '/v1/players', null, $tokenA);
check('still only two players', count($r['data']['players'] ?? []) === 2);

// Deletion cannot be undone by an old create.
[$s] = call('DELETE', "/v1/players/$playerId", ['password' => $password, 'confirm' => true], $tokenA);
check('delete player', $s === 200);
[$s] = call('POST', '/v1/players', ['operation_id' => uuid4()] + $create, $tokenA);
check('deleted player cannot be recreated (409)', $s === 409);

// Bad input.
[$s] = call('POST', '/v1/players', ['operation_id' => uuid4(), 'player_id' => uuid4(), 'name' => "x'; DROP TABLE players;--", 'age' => 999], $tokenA);
check('age 999 is 422', $s === 422);

// Logout and password change.
[$s] = call('POST', '/v1/auth/logout', null, $tokenB);
check('logout', $s === 200);
[$s] = call('GET', '/v1/players', null, $tokenB);
check('token after logout is 401', $s === 401);

// Clean up the fictional accounts.
call('DELETE', '/v1/account', ['password' => $password, 'confirm' => true], $tokenA);

echo $failures === 0 ? "\nAll checks passed.\n" : "\n$failures check(s) FAILED.\n";
exit($failures === 0 ? 0 : 1);
