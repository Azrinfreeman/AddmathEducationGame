<?php
/*
 * Simple request counters stored in MySQL (works on shared hosting, no extra software).
 * Each counter covers a fixed time window; when the window ends the count restarts.
 */
declare(strict_types=1);

/**
 * Counts one request and throws 429 when the limit is passed.
 * $limit comes from config, as [maxRequests, windowSeconds].
 */
function enforce_rate_limit(string $name, string $identity, array $limit): void
{
    [$maxRequests, $windowSeconds] = $limit;
    $key = hash('sha256', $name . '|' . $identity);
    $now = time();
    $nowText = gmdate('Y-m-d H:i:s', $now);
    $oldestAllowedStart = gmdate('Y-m-d H:i:s', $now - $windowSeconds);

    // One statement does "start a new window or add one", so two requests cannot overwrite each other.
    run_query(
        'INSERT INTO rate_limit_buckets (bucket_key, window_start, hits) VALUES (?, ?, 1)
         ON DUPLICATE KEY UPDATE
           hits = IF(window_start <= ?, 1, hits + 1),
           window_start = IF(window_start <= ?, ?, window_start)',
        [$key, $nowText, $oldestAllowedStart, $oldestAllowedStart, $nowText]
    );

    $bucket = fetch_one('SELECT window_start, hits FROM rate_limit_buckets WHERE bucket_key = ?', [$key]);
    if ($bucket !== null && (int) $bucket['hits'] > $maxRequests) {
        $retryAfter = max(1, strtotime($bucket['window_start'] . ' UTC') + $windowSeconds - $now);
        throw new ApiError(429, 'rate_limited', 'Too many requests. Please try again later.', ['Retry-After' => (string) $retryAfter]);
    }
}

/** The caller's IP address as seen by PHP. */
function client_ip(): string
{
    return $_SERVER['REMOTE_ADDR'] ?? 'unknown';
}
