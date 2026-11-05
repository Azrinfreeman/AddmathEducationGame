<?php
/*
 * Retry protection. The Unity app gives every write an "operation_id".
 * If the answer is lost and the app sends the same operation again, we return the
 * first answer instead of doing the work twice. The same id with different data is refused.
 */
declare(strict_types=1);

/**
 * Runs $work once per (account, operation id).
 * $work must return [httpStatus, dataArray]. The write and the saved answer commit together.
 * Returns [httpStatus, dataArray] - the new answer, or the stored one on a retry.
 */
function run_idempotent(int $accountId, $operationId, string $type, array $requestForHash, callable $work): array
{
    $operationId = require_uuid($operationId, 'operation_id');
    $hash = fingerprint($requestForHash);

    for ($attempt = 0; $attempt < 2; $attempt++) {
        try {
            return in_transaction(function () use ($accountId, $operationId, $type, $hash, $work) {
                $stored = find_stored_answer($accountId, $operationId, $type, $hash);
                if ($stored !== null) {
                    return $stored;
                }
                [$status, $data] = $work();
                run_query(
                    'INSERT INTO api_mutations (account_id, operation_uuid, operation_type, request_hash, http_status, response_json, created_at)
                     VALUES (?, ?, ?, ?, ?, ?, ?)',
                    [$accountId, $operationId, $type, $hash, $status, json_encode($data, JSON_UNESCAPED_UNICODE), now_utc()]
                );
                return [$status, $data];
            });
        } catch (PDOException $e) {
            // 23000 = duplicate key: a parallel retry stored its answer first. Loop and read it.
            if ($e->getCode() !== '23000' || $attempt === 1) {
                throw $e;
            }
        }
    }
    throw new ApiError(503, 'try_again', 'Please try again.');
}

/** Returns the stored answer for this operation, null if new, or throws 409 if the data differs. */
function find_stored_answer(int $accountId, string $operationId, string $type, string $hash): ?array
{
    $row = fetch_one(
        'SELECT operation_type, request_hash, http_status, response_json FROM api_mutations
         WHERE account_id = ? AND operation_uuid = ?',
        [$accountId, $operationId]
    );
    if ($row === null) {
        return null;
    }
    if ($row['operation_type'] !== $type || $row['request_hash'] !== $hash) {
        throw new ApiError(409, 'operation_conflict', 'This operation id was already used with different data.');
    }
    return [(int) $row['http_status'], json_decode($row['response_json'], true)];
}
