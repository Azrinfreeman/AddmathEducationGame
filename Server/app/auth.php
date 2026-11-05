<?php
/* Sessions: issue, check and revoke bearer tokens. Only the token's SHA-256 hash is stored. */
declare(strict_types=1);

/** Creates a session for the account and returns [rawToken, expiresAtText]. */
function create_session(int $accountId): array
{
    $rawToken = bin2hex(random_bytes(32));
    $expiresAt = gmdate('Y-m-d H:i:s', time() + (int) app_config()['token_lifetime_seconds']);
    run_query(
        'INSERT INTO api_sessions (account_id, token_hash, created_at, expires_at) VALUES (?, ?, ?, ?)',
        [$accountId, hash('sha256', $rawToken), now_utc(), $expiresAt]
    );
    return [$rawToken, $expiresAt];
}

/**
 * Checks the request's token and returns the account id.
 * Throws 401 when the token is missing, unknown or expired.
 */
function require_account_id(): int
{
    $token = read_bearer_token();
    if ($token === null) {
        throw new ApiError(401, 'unauthorized', 'Please sign in.');
    }
    $session = fetch_one(
        'SELECT account_id FROM api_sessions WHERE token_hash = ? AND expires_at > ?',
        [hash('sha256', $token), now_utc()]
    );
    if ($session === null) {
        throw new ApiError(401, 'unauthorized', 'Please sign in.');
    }
    $accountId = (int) $session['account_id'];
    enforce_rate_limit('api', (string) $accountId, app_config()['rate_limit_api']);
    return $accountId;
}

/** Revokes only the session that sent this request. */
function revoke_current_session(): void
{
    $token = read_bearer_token();
    if ($token !== null) {
        run_query('DELETE FROM api_sessions WHERE token_hash = ?', [hash('sha256', $token)]);
    }
}

/** Revokes every session of an account (used after password change/reset). */
function revoke_all_sessions(int $accountId): void
{
    run_query('DELETE FROM api_sessions WHERE account_id = ?', [$accountId]);
}

/** Checks the account's password again for sensitive actions (delete). Throws 403 if wrong. */
function require_current_password(int $accountId, $password): void
{
    $account = fetch_one('SELECT password_hash FROM accounts WHERE id = ?', [$accountId]);
    if ($account === null || !is_string($password) || !password_verify($password, $account['password_hash'])) {
        throw new ApiError(403, 'wrong_password', 'The password is not correct.');
    }
}
