<?php
/* Adult accounts: register, login, logout, change password, delete. */
declare(strict_types=1);

function register_account(array $body): void
{
    enforce_rate_limit('register', client_ip(), app_config()['rate_limit_register']);
    $username = validate_username($body['username'] ?? null);
    $password = validate_new_password($body['password'] ?? null);

    try {
        run_query(
            'INSERT INTO accounts (username_norm, username, password_hash, created_at, updated_at) VALUES (?, ?, ?, ?, ?)',
            [strtolower($username), $username, password_hash($password, PASSWORD_DEFAULT), now_utc(), now_utc()]
        );
    } catch (PDOException $e) {
        if ($e->getCode() === '23000') {
            throw new ApiError(409, 'username_taken', 'That username is already used.');
        }
        throw $e;
    }
    send_success(['username' => $username], 201);
}

function login_account(array $body): void
{
    $username = is_string($body['username'] ?? null) ? trim($body['username']) : '';
    $password = is_string($body['password'] ?? null) ? $body['password'] : '';
    enforce_rate_limit('login', client_ip() . '|' . strtolower($username), app_config()['rate_limit_login']);

    $account = fetch_one('SELECT id, username, password_hash FROM accounts WHERE username_norm = ?', [strtolower($username)]);

    // Do one hashing step either way so unknown and known usernames take about the same time.
    $passwordMatches = false;
    if ($account !== null) {
        $passwordMatches = password_verify($password, $account['password_hash']);
    } else {
        password_hash($password, PASSWORD_DEFAULT); // result is thrown away on purpose
    }
    if (!$passwordMatches) {
        throw new ApiError(401, 'invalid_credentials', 'Username or password is not correct.');
    }

    if (password_needs_rehash($account['password_hash'], PASSWORD_DEFAULT)) {
        run_query('UPDATE accounts SET password_hash = ?, updated_at = ? WHERE id = ?',
            [password_hash($password, PASSWORD_DEFAULT), now_utc(), $account['id']]);
    }

    [$token, $expiresAt] = create_session((int) $account['id']);
    send_success(['token' => $token, 'expires_at' => gmdate('c', strtotime($expiresAt . ' UTC')), 'username' => $account['username']]);
}

function logout_account(): void
{
    require_account_id();
    revoke_current_session();
    send_success(['signed_out' => true]);
}

function change_password(int $accountId, array $body): void
{
    require_current_password($accountId, $body['current_password'] ?? null);
    $newPassword = validate_new_password($body['new_password'] ?? null);
    run_query('UPDATE accounts SET password_hash = ?, updated_at = ? WHERE id = ?',
        [password_hash($newPassword, PASSWORD_DEFAULT), now_utc(), $accountId]);
    revoke_all_sessions($accountId); // every device must sign in again
    send_success(['password_changed' => true]);
}

/** Deletes the account and (through foreign keys) all its players, progress and results. */
function delete_account(int $accountId, array $body): void
{
    if (($body['confirm'] ?? false) !== true) {
        throw invalid('confirm', 'Deletion must be confirmed.');
    }
    require_current_password($accountId, $body['password'] ?? null);
    run_query('DELETE FROM accounts WHERE id = ?', [$accountId]);
    send_success(['deleted' => true]);
}
