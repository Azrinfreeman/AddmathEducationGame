-- MathFun online player data - schema version 1
-- Import this file once in cPanel > phpMyAdmin > (your database) > Import.
-- Works on MySQL 8.0+ and recent MariaDB. All times are stored in UTC.

SET NAMES utf8mb4;

CREATE TABLE schema_version (
  version     INT UNSIGNED NOT NULL PRIMARY KEY,
  applied_at  DATETIME     NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One row per adult/teacher account. Children never sign in.
CREATE TABLE accounts (
  id             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  username_norm  VARCHAR(30)  NOT NULL,          -- lower-case, used for uniqueness
  username       VARCHAR(30)  NOT NULL,          -- as typed, used for display
  password_hash  VARCHAR(255) NOT NULL,          -- output of PHP password_hash()
  created_at     DATETIME     NOT NULL,
  updated_at     DATETIME     NOT NULL,
  UNIQUE KEY uq_accounts_username (username_norm)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Login sessions. Only the SHA-256 hash of the token is stored.
CREATE TABLE api_sessions (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  account_id  BIGINT UNSIGNED NOT NULL,
  token_hash  CHAR(64)  NOT NULL,
  created_at  DATETIME  NOT NULL,
  expires_at  DATETIME  NOT NULL,
  UNIQUE KEY uq_sessions_token (token_hash),
  KEY ix_sessions_account (account_id),
  KEY ix_sessions_expiry (expires_at),
  CONSTRAINT fk_sessions_account FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Child player profiles. player_uuid is created by the app, never guessed from a number.
CREATE TABLE players (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  player_uuid CHAR(36)     NOT NULL,
  account_id  BIGINT UNSIGNED NOT NULL,
  name        VARCHAR(100) NOT NULL,
  gender      VARCHAR(10)  NULL,                 -- 'male', 'female', 'other' or NULL
  age         TINYINT UNSIGNED NULL,
  avatar_id   VARCHAR(64)  NULL,
  revision    INT UNSIGNED NOT NULL DEFAULT 1,   -- +1 on every edit, used to detect stale edits
  created_at  DATETIME     NOT NULL,
  updated_at  DATETIME     NOT NULL,
  UNIQUE KEY uq_players_uuid (player_uuid),
  KEY ix_players_account (account_id),
  CONSTRAINT fk_players_account FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Completed lessons / earned achievements. A missing row never means "not done on another device".
CREATE TABLE player_progress (
  player_id     BIGINT UNSIGNED NOT NULL,
  condition_id  VARCHAR(40)  NOT NULL,           -- must exist in config/activities.php
  provenance    VARCHAR(20)  NOT NULL,           -- 'played' or 'legacy_import'
  earned_at     DATETIME     NOT NULL,
  PRIMARY KEY (player_id, condition_id),
  CONSTRAINT fk_progress_player FOREIGN KEY (player_id) REFERENCES players (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- One row per finished quiz/game. Scored games fill score; outcome-only games leave it NULL.
CREATE TABLE activity_attempts (
  seq               BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  attempt_uuid      CHAR(36)    NOT NULL,
  player_id         BIGINT UNSIGNED NOT NULL,
  activity_id       VARCHAR(40) NOT NULL,
  content_version   SMALLINT UNSIGNED NOT NULL DEFAULT 1,
  result_kind       VARCHAR(10) NOT NULL,        -- 'scored' or 'outcome'
  outcome           VARCHAR(10) NOT NULL,        -- 'completed', 'won' or 'lost'
  score             INT UNSIGNED NULL,
  max_score         INT UNSIGNED NULL,
  duration_ms       INT UNSIGNED NULL,
  completed_at      DATETIME    NOT NULL,        -- time reported by the device
  received_at       DATETIME    NOT NULL,        -- time the server stored it
  payload_hash      CHAR(64)    NOT NULL,        -- detects the same id re-sent with different data
  UNIQUE KEY uq_attempt_uuid (attempt_uuid),
  KEY ix_attempt_player (player_id, seq),
  KEY ix_attempt_activity (player_id, activity_id),
  CONSTRAINT fk_attempt_player FOREIGN KEY (player_id) REFERENCES players (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Remembers which old-app profile became which online profile, so a retry never duplicates it.
CREATE TABLE legacy_imports (
  id                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  account_id        BIGINT UNSIGNED NOT NULL,
  source_install_id CHAR(36)    NOT NULL,
  legacy_player_id  INT UNSIGNED NOT NULL,
  player_id         BIGINT UNSIGNED NOT NULL,
  payload_hash      CHAR(64)    NOT NULL,
  created_at        DATETIME    NOT NULL,
  UNIQUE KEY uq_import_source (account_id, source_install_id, legacy_player_id),
  KEY ix_import_player (player_id),
  CONSTRAINT fk_import_account FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE,
  CONSTRAINT fk_import_player  FOREIGN KEY (player_id)  REFERENCES players (id)  ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Stored answers for retryable writes: same operation id => same answer, no second effect.
CREATE TABLE api_mutations (
  account_id       BIGINT UNSIGNED NOT NULL,
  operation_uuid   CHAR(36)    NOT NULL,
  operation_type   VARCHAR(30) NOT NULL,
  request_hash     CHAR(64)    NOT NULL,
  http_status      SMALLINT UNSIGNED NOT NULL,
  response_json    TEXT        NOT NULL,
  created_at       DATETIME    NOT NULL,
  PRIMARY KEY (account_id, operation_uuid),
  KEY ix_mutations_created (created_at),
  CONSTRAINT fk_mutations_account FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Prevents an old queued request from bringing a deleted player back. No personal fields.
CREATE TABLE deletion_tombstones (
  id                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  account_id        BIGINT UNSIGNED NOT NULL,
  player_uuid       CHAR(36)    NOT NULL,
  source_install_id CHAR(36)    NULL,
  legacy_player_id  INT UNSIGNED NULL,
  deleted_at        DATETIME    NOT NULL,
  UNIQUE KEY uq_tombstone_player (player_uuid),
  KEY ix_tombstone_source (account_id, source_install_id, legacy_player_id),
  CONSTRAINT fk_tombstone_account FOREIGN KEY (account_id) REFERENCES accounts (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Request counters for login/register/API throttling. Old rows are removed by tools/cleanup.php.
CREATE TABLE rate_limit_buckets (
  bucket_key    CHAR(64)  NOT NULL PRIMARY KEY,
  window_start  DATETIME  NOT NULL,
  hits          INT UNSIGNED NOT NULL,
  KEY ix_rate_window (window_start)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO schema_version (version, applied_at) VALUES (1, UTC_TIMESTAMP());
