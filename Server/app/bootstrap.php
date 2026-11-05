<?php
/*
 * Loads everything the API needs. Called once by public/index.php.
 * $privateFolder is the folder that contains app/, config/ and logs/.
 */
declare(strict_types=1);

// Never show PHP errors to callers; they are written to the log instead.
ini_set('display_errors', '0');
error_reporting(E_ALL);
date_default_timezone_set('UTC');
ob_start(); // see send_json(): stray output is discarded before the answer is sent

require_once $privateFolder . '/app/http.php';
require_once $privateFolder . '/app/validation.php';
require_once $privateFolder . '/app/database.php';
require_once $privateFolder . '/app/rate_limits.php';
require_once $privateFolder . '/app/idempotency.php';
require_once $privateFolder . '/app/auth.php';
require_once $privateFolder . '/app/accounts.php';
require_once $privateFolder . '/app/players.php';
require_once $privateFolder . '/app/progress.php';
require_once $privateFolder . '/app/attempts.php';
require_once $privateFolder . '/app/legacy_import.php';

/** Returns the settings from config/config.php (loaded once). */
function app_config(): array
{
    static $config = null;
    global $privateFolder;
    if ($config === null) {
        $path = $privateFolder . '/config/config.php';
        if (!is_file($path)) {
            throw new RuntimeException('config/config.php is missing');
        }
        $config = require $path;
    }
    return $config;
}

/** Returns the allowed activity/condition lists from config/activities.php. */
function activity_config(): array
{
    static $activities = null;
    global $privateFolder;
    if ($activities === null) {
        $activities = require $privateFolder . '/config/activities.php';
    }
    return $activities;
}

/** Current time in UTC as a MySQL DATETIME string. */
function now_utc(): string
{
    return gmdate('Y-m-d H:i:s');
}
