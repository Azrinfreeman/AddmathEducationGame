<?php
/*
 * Owner-only account recovery. Command line only; it can never be run from the web.
 *
 *   php account_maintenance.php reset-password USERNAME   (asks for the new password)
 *   php account_maintenance.php delete-account USERNAME   (asks you to type the username to confirm)
 *
 * Verify the adult's identity OUTSIDE the app before using this.
 */
if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}

$privateFolder = dirname(__DIR__);
require $privateFolder . '/app/bootstrap.php';

[$script, $command, $username] = array_pad($argv, 3, null);
if (!in_array($command, ['reset-password', 'delete-account'], true) || $username === null) {
    fwrite(STDERR, "Usage: php account_maintenance.php reset-password|delete-account USERNAME\n");
    exit(1);
}

$account = fetch_one('SELECT id, username FROM accounts WHERE username_norm = ?', [strtolower($username)]);
if ($account === null) {
    fwrite(STDERR, "No such account.\n");
    exit(1);
}

if ($command === 'reset-password') {
    echo 'New password (typing is visible; clear your terminal afterwards): ';
    $newPassword = trim((string) fgets(STDIN));
    try {
        validate_new_password($newPassword);
    } catch (ApiError $e) {
        fwrite(STDERR, $e->getMessage() . "\n");
        exit(1);
    }
    run_query('UPDATE accounts SET password_hash = ?, updated_at = ? WHERE id = ?',
        [password_hash($newPassword, PASSWORD_DEFAULT), now_utc(), $account['id']]);
    revoke_all_sessions((int) $account['id']);
    echo "Password changed and all sessions signed out.\n";
} else {
    echo 'Type the username again to delete the account and ALL its players: ';
    if (trim((string) fgets(STDIN)) !== $account['username']) {
        fwrite(STDERR, "Cancelled.\n");
        exit(1);
    }
    run_query('DELETE FROM accounts WHERE id = ?', [$account['id']]);
    echo "Account deleted.\n";
}
