<?php
/*
 * The only public PHP file. Every request comes here as:
 *     index.php?route=/v1/players
 *
 * EDIT THIS LINE if you upload app/, config/ and logs/ somewhere else.
 * Point it to the folder that contains app/ and config/ (outside public_html is best), e.g.
 *     $privateFolder = '/home/YOURCPANELUSER/mathfun_private';
 */
$privateFolder = dirname(__DIR__);

require $privateFolder . '/app/bootstrap.php';

try {
    handle_request();
} catch (ApiError $error) {
    send_error($error);
} catch (Throwable $unexpected) {
    // Log the class and request id only. The message may contain SQL or personal data.
    log_line('unexpected ' . get_class($unexpected) . ' at ' . basename($unexpected->getFile()) . ':' . $unexpected->getLine());
    send_error(new ApiError(500, 'server_error', 'Something went wrong. Quote request id ' . request_id() . ' when asking for help.'));
}

/** Matches the method + route against the table below and calls the right function. */
function handle_request(): void
{
    $method = $_SERVER['REQUEST_METHOD'] ?? 'GET';
    $route = '/' . trim((string) ($_GET['route'] ?? ''), '/');
    $uuid = '([0-9a-fA-F-]{36})';

    // Browser pre-check before a cross-site call (WebGL build): answer "yes, allowed" with no body.
    if ($method === 'OPTIONS') {
        while (ob_get_level() > 0) { ob_end_clean(); }
        http_response_code(204);
        send_cors_headers();
        exit;
    }

    if ($method === 'GET' && $route === '/v1/health') {
        send_success(['status' => 'ok']);
    }

    // Public routes (no token needed).
    if ($method === 'POST' && $route === '/v1/auth/register') { register_account(read_json_body()); }
    if ($method === 'POST' && $route === '/v1/auth/login')    { login_account(read_json_body()); }
    if ($method === 'POST' && $route === '/v1/auth/logout')   { logout_account(); }

    // Everything below needs a valid token.
    $accountId = require_account_id();

    if ($method === 'PATCH'  && $route === '/v1/account/password') { change_password($accountId, read_json_body()); }
    if ($method === 'DELETE' && $route === '/v1/account')          { delete_account($accountId, read_json_body()); }

    if ($method === 'GET'  && $route === '/v1/players') { list_players($accountId); }
    if ($method === 'POST' && $route === '/v1/players') { create_player($accountId, read_json_body()); }
    if ($method === 'POST' && $route === '/v1/legacy-imports') { import_legacy_player($accountId, read_json_body()); }

    if (preg_match('#^/v1/players/' . $uuid . '(/state|/progress|/attempts)?$#', $route, $m)) {
        $playerUuid = strtolower($m[1]);
        $part = $m[2] ?? '';

        if ($part === '' && $method === 'PATCH')  { update_player($accountId, $playerUuid, read_json_body()); }
        if ($part === '' && $method === 'DELETE') { delete_player($accountId, $playerUuid, read_json_body()); }
        if ($part === '/state' && $method === 'GET') { get_player_state($accountId, $playerUuid); }
        if ($part === '/progress' && $method === 'PUT') { put_progress($accountId, $playerUuid, read_json_body()); }
        if ($part === '/attempts' && $method === 'POST') { record_attempt($accountId, $playerUuid, read_json_body()); }
        if ($part === '/attempts' && $method === 'GET')  { list_attempts($accountId, $playerUuid, $_GET); }
    }

    throw new ApiError(404, 'route_not_found', 'No such endpoint.');
}
