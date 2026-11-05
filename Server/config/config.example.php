<?php
/*
 * Copy this file to config.php (same folder) and fill in your values.
 * config.php holds passwords: never upload it to a public folder, never commit it.
 * Every number you may want to change is here, with a plain-English note.
 */
return [
    // --- Database (cPanel > MySQL Databases). cPanel adds a prefix, e.g. "cpuser_mathfun". ---
    'db_host'     => 'localhost',
    'db_name'     => 'cpuser_mathfun',
    'db_user'     => 'cpuser_mathfun_app',
    'db_password' => 'CHANGE_ME',

    // --- Sessions ---
    'token_lifetime_seconds' => 86400,      // 24 hours, then the adult signs in again

    // --- Input limits (the Unity app should use the same numbers) ---
    'username_min_length' => 3,
    'username_max_length' => 30,
    'password_min_length' => 8,
    'player_name_max_length' => 40,         // characters
    'player_age_min' => 2,
    'player_age_max' => 18,
    'max_request_body_bytes' => 65536,      // 64 KB; larger requests get 413
    'max_conditions_per_request' => 100,
    'attempts_page_size_max' => 100,

    // --- Rate limits: [maximum requests, per this many seconds] ---
    'rate_limit_login'    => [10, 900],     // per IP + username
    'rate_limit_register' => [10, 3600],    // per IP
    'rate_limit_api'      => [600, 60],     // per account, all other requests

    // --- Housekeeping ---
    'mutation_retention_days' => 30,        // how long retry answers are kept

    // --- Logging: keep this file OUTSIDE public_html. Passwords/bodies are never logged. ---
    'log_file' => __DIR__ . '/../logs/api.log',

    // --- Safety switch for tests/run.php. Leave false on the real database. ---
    'is_disposable_test_database' => false,
];
