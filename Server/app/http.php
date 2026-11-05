<?php
/*
 * Small helpers for reading requests and sending JSON answers.
 * Every answer looks like:  {"request_id": "...", "data": {...}}  or  {"request_id": "...", "error": {...}}
 */
declare(strict_types=1);

/** Thrown anywhere to stop the request and send an error answer. */
class ApiError extends Exception
{
    public int $status;
    public string $errorCode;
    public array $extraHeaders;
    public array $extraData;

    public function __construct(int $status, string $errorCode, string $message, array $extraHeaders = [], array $extraData = [])
    {
        parent::__construct($message);
        $this->status = $status;
        $this->errorCode = $errorCode;
        $this->extraHeaders = $extraHeaders;
        $this->extraData = $extraData;
    }
}

/** A short random id that appears in the answer and in the log, so problems can be traced. */
function request_id(): string
{
    static $id = null;
    if ($id === null) {
        $id = bin2hex(random_bytes(8));
    }
    return $id;
}

/**
 * Lets the WebGL build (served from another site, e.g. itch.io) call this API from a browser.
 * Safe with '*' because sign-in uses a token header, not cookies.
 */
function send_cors_headers(): void
{
    header('Access-Control-Allow-Origin: *');
    header('Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS');
    header('Access-Control-Allow-Headers: Content-Type, Authorization, X-Auth-Token');
    header('Access-Control-Expose-Headers: Retry-After');
    header('Access-Control-Max-Age: 86400');
}

/** Sends a JSON answer and ends the request. */
function send_json(int $status, array $body, array $extraHeaders = []): void
{
    // Throw away anything printed by accident (e.g. a hidden BOM at the top of an edited file),
    // so the app always receives clean JSON.
    while (ob_get_level() > 0) {
        ob_end_clean();
    }
    http_response_code($status);
    send_cors_headers();
    header('Content-Type: application/json; charset=utf-8');
    header('Cache-Control: no-store');
    header('X-Content-Type-Options: nosniff');
    foreach ($extraHeaders as $name => $value) {
        header($name . ': ' . $value);
    }
    $body = ['request_id' => request_id()] + $body;
    echo json_encode($body, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function send_success(array $data, int $status = 200): void
{
    send_json($status, ['data' => $data]);
}

function send_error(ApiError $error): void
{
    $body = ['error' => ['code' => $error->errorCode, 'message' => $error->getMessage()] + $error->extraData];
    send_json($error->status, $body, $error->extraHeaders);
}

/** Reads and decodes the JSON request body. Returns [] when the body is empty. */
function read_json_body(): array
{
    $maxBytes = (int) app_config()['max_request_body_bytes'];
    $length = (int) ($_SERVER['CONTENT_LENGTH'] ?? 0);
    if ($length > $maxBytes) {
        throw new ApiError(413, 'body_too_large', 'The request is too large.');
    }
    $raw = file_get_contents('php://input', false, null, 0, $maxBytes + 1);
    if ($raw === false || $raw === '') {
        return [];
    }
    if (strlen($raw) > $maxBytes) {
        throw new ApiError(413, 'body_too_large', 'The request is too large.');
    }
    $decoded = json_decode($raw, true);
    if (!is_array($decoded)) {
        throw new ApiError(400, 'invalid_json', 'The request body is not valid JSON.');
    }
    return $decoded;
}

/**
 * Finds the bearer token. Some cPanel hosts remove the Authorization header,
 * so "X-Auth-Token" is accepted as a fallback.
 */
function read_bearer_token(): ?string
{
    $header = $_SERVER['HTTP_AUTHORIZATION']
        ?? $_SERVER['REDIRECT_HTTP_AUTHORIZATION']
        ?? $_SERVER['HTTP_X_AUTH_TOKEN']
        ?? '';
    if (stripos($header, 'Bearer ') === 0) {
        $header = substr($header, 7);
    }
    $header = trim($header);
    return $header === '' ? null : $header;
}

/** Writes one line to the log file. Pass only safe text: no passwords, tokens, names or bodies. */
function log_line(string $message): void
{
    try {
        $file = app_config()['log_file'] ?? null;
        if ($file === null) {
            return;
        }
        $folder = dirname($file);
        if (!is_dir($folder)) {
            @mkdir($folder, 0750, true);
        }
        @file_put_contents($file, gmdate('c') . ' ' . request_id() . ' ' . $message . "\n", FILE_APPEND | LOCK_EX);
    } catch (Throwable $ignored) {
        // Logging must never break the API.
    }
}
