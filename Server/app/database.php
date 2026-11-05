<?php
/* Database connection and two tiny helpers. All SQL uses prepared statements. */
declare(strict_types=1);

/** Returns the shared PDO connection (created on first use). */
function db(): PDO
{
    static $pdo = null;
    if ($pdo === null) {
        $c = app_config();
        $dsn = 'mysql:host=' . $c['db_host'] . ';dbname=' . $c['db_name'] . ';charset=utf8mb4';
        try {
            $pdo = new PDO($dsn, $c['db_user'], $c['db_password'], [
                PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                PDO::ATTR_EMULATE_PREPARES => false,
            ]);
            $pdo->exec("SET time_zone = '+00:00'");
        } catch (PDOException $e) {
            log_line('database connection failed: ' . $e->getCode());
            throw new ApiError(503, 'database_unavailable', 'The service is temporarily unavailable.');
        }
    }
    return $pdo;
}

/** Runs a prepared statement and returns it. */
function run_query(string $sql, array $params = []): PDOStatement
{
    $statement = db()->prepare($sql);
    $statement->execute($params);
    return $statement;
}

/** Returns the first row of a query, or null. */
function fetch_one(string $sql, array $params = []): ?array
{
    $row = run_query($sql, $params)->fetch();
    return $row === false ? null : $row;
}

/** Returns all rows of a query. */
function fetch_all(string $sql, array $params = []): array
{
    return run_query($sql, $params)->fetchAll();
}

/** Runs $work inside one database transaction. Any exception rolls everything back. */
function in_transaction(callable $work)
{
    $pdo = db();
    $pdo->beginTransaction();
    try {
        $result = $work();
        $pdo->commit();
        return $result;
    } catch (Throwable $e) {
        if ($pdo->inTransaction()) {
            $pdo->rollBack();
        }
        throw $e;
    }
}
