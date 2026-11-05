<?php
/*
 * All input rules live here. Limits come from config/config.php.
 * Each function returns a clean value or throws ApiError(422).
 */
declare(strict_types=1);

function invalid(string $field, string $message): ApiError
{
    return new ApiError(422, 'invalid_value', $message, [], ['field' => $field]);
}

function require_uuid($value, string $field): string
{
    if (!is_string($value) || !preg_match('/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/', strtolower($value))) {
        throw invalid($field, "$field must be a UUID.");
    }
    return strtolower($value);
}

function validate_username($value): string
{
    $c = app_config();
    if (!is_string($value)) {
        throw invalid('username', 'Username is required.');
    }
    $value = trim($value);
    $length = strlen($value);
    if ($length < $c['username_min_length'] || $length > $c['username_max_length'] || !preg_match('/^[A-Za-z0-9_.-]+$/', $value)) {
        throw invalid('username', 'Username needs ' . $c['username_min_length'] . '-' . $c['username_max_length'] . ' letters, numbers, dot, dash or underscore.');
    }
    return $value;
}

function validate_new_password($value): string
{
    $c = app_config();
    // bcrypt only reads the first 72 bytes, so longer passwords are refused instead of silently cut.
    if (!is_string($value) || strlen($value) < $c['password_min_length'] || strlen($value) > 72) {
        throw invalid('password', 'Password must be ' . $c['password_min_length'] . ' to 72 characters.');
    }
    return $value;
}

function validate_player_name($value): string
{
    $max = (int) app_config()['player_name_max_length'];
    if (!is_string($value)) {
        throw invalid('name', 'Name is required.');
    }
    $value = trim($value);
    $length = mb_strlen($value, 'UTF-8');
    if ($length < 1 || $length > $max || preg_match('/[\x00-\x1F\x7F]/', $value)) {
        throw invalid('name', "Name must be 1 to $max characters.");
    }
    return $value;
}

/** Accepts the old app's labels too (Malay and English) and returns male/female/other/null. */
function validate_gender($value): ?string
{
    if ($value === null || $value === '') {
        return null;
    }
    if (!is_string($value)) {
        throw invalid('gender', 'Gender is not recognised.');
    }
    $map = [
        'male' => 'male', 'boy' => 'male', 'lelaki' => 'male',
        'female' => 'female', 'girl' => 'female', 'perempuan' => 'female',
        'other' => 'other',
        // The game's third choice means "not given": store nothing.
        'unspecified' => null, 'tidak dinyatakan' => null,
    ];
    $key = mb_strtolower(trim($value), 'UTF-8');
    if (!array_key_exists($key, $map)) {
        throw invalid('gender', 'Gender is not recognised.');
    }
    return $map[$key];
}

function validate_age($value): ?int
{
    if ($value === null || $value === '') {
        return null;
    }
    $c = app_config();
    if (!is_int($value) && !(is_string($value) && ctype_digit($value))) {
        throw invalid('age', 'Age must be a whole number.');
    }
    $age = (int) $value;
    if ($age < $c['player_age_min'] || $age > $c['player_age_max']) {
        throw invalid('age', 'Age must be between ' . $c['player_age_min'] . ' and ' . $c['player_age_max'] . '.');
    }
    return $age;
}

function validate_avatar_id($value): ?string
{
    if ($value === null || $value === '') {
        return null;
    }
    if (!is_string($value) || !preg_match('/^[A-Za-z0-9_.-]{1,64}$/', $value)) {
        throw invalid('avatar_id', 'Avatar id is not valid.');
    }
    return $value;
}

function validate_condition_id($value): string
{
    if (!is_string($value) || !in_array($value, activity_config()['conditions'], true)) {
        throw invalid('condition_id', 'Unknown progress item.');
    }
    return $value;
}

/** Parses an ISO-8601 time from the device into a UTC DATETIME string. */
function validate_device_time($value, string $field): string
{
    $time = is_string($value) ? strtotime($value) : false;
    if ($time === false) {
        throw invalid($field, "$field must be an ISO-8601 time.");
    }
    // A device clock far in the future is almost certainly wrong; keep the record but cap it.
    return gmdate('Y-m-d H:i:s', min($time, time() + 86400));
}

/** Reads an optional non-negative integer within [0, $max]. */
function optional_int($value, string $field, int $max): ?int
{
    if ($value === null) {
        return null;
    }
    if (!is_int($value) || $value < 0 || $value > $max) {
        throw invalid($field, "$field must be a whole number from 0 to $max.");
    }
    return $value;
}

/** Stable fingerprint of a request: same data => same hash. */
function fingerprint(array $data): string
{
    ksort($data);
    return hash('sha256', json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES));
}
