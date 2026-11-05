<?php
/*
 * Removes expired sessions, old rate-limit counters and old retry answers.
 * Run from cPanel > Cron Jobs (daily is enough):
 *     /usr/local/bin/php /home/YOURUSER/mathfun_private/tools/cleanup.php
 * It refuses to run from a web browser.
 */
if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}

$privateFolder = dirname(__DIR__);
require $privateFolder . '/app/bootstrap.php';

$retentionDays = (int) app_config()['mutation_retention_days'];

$sessions = run_query('DELETE FROM api_sessions WHERE expires_at < ?', [now_utc()])->rowCount();
$buckets = run_query('DELETE FROM rate_limit_buckets WHERE window_start < ?', [gmdate('Y-m-d H:i:s', time() - 86400)])->rowCount();
$mutations = run_query('DELETE FROM api_mutations WHERE created_at < ?', [gmdate('Y-m-d H:i:s', time() - $retentionDays * 86400)])->rowCount();

echo "Removed $sessions expired sessions, $buckets old rate counters, $mutations old retry answers.\n";
