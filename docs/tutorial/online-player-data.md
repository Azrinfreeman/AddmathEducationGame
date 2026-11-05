# Online player data: setup and maintenance guide

This guide explains how to put the PHP API on your cPanel hosting, connect the Unity game to it, and change the common settings yourself. Steps marked **(tested)** were run on a local XAMPP copy (PHP 8.2, MariaDB 10.4). Steps marked **(not yet tested)** have not been run on a real cPanel server.

## 1. What you have

| Part | Where | Job |
| --- | --- | --- |
| PHP API | `Server/` | Receives requests from the game, checks them, stores data in MySQL. |
| Database schema | `Server/database/001_initial.sql` | Creates the tables. |
| Unity client | `Assets/OnlineData/Runtime/` | Saves on the device first, then uploads when signed in. |
| Settings | `Server/config/config.php`, `Assets/OnlineData/Resources/OnlineData/ApiConfiguration.asset` | Server address, limits. |

How it works: an adult signs in (one account, several child profiles). Every change is saved on the device first, then queued and uploaded. Without internet the game keeps working and uploads later.

## 2. Server folders

On cPanel, keep private files **outside** `public_html`:

```
/home/YOURUSER/public_html/api/index.php      <- from Server/public/index.php  (public)
/home/YOURUSER/mathfun_private/app/           <- from Server/app/
/home/YOURUSER/mathfun_private/config/        <- from Server/config/  (you create config.php here)
/home/YOURUSER/mathfun_private/tools/         <- from Server/tools/
/home/YOURUSER/mathfun_private/logs/          <- created automatically
```

Do not upload `Server/tests/` to the server.

## 3. Step by step (not yet tested on a real cPanel)

1. **Backup**: download whatever exists in `public_html` first. Use a staging subdomain or folder before production.
2. **Database**: cPanel > *MySQL Databases*. Create a database and a user, add the user to the database with all privileges. cPanel adds a prefix, so the real names look like `youruser_mathfun`.
3. **Schema**: cPanel > *phpMyAdmin* > select your database > *Import* > choose `Server/database/001_initial.sql`. You should see 10 tables.
4. **PHP version**: cPanel > *Select PHP Version*. Use 8.1 or newer and make sure `pdo_mysql` and `mbstring` are ticked.
5. **Upload** files as shown in section 2 (File Manager or FTP).
6. **Config**: in `mathfun_private/config/` copy `config.example.php` to `config.php` and edit the database name, user and password.
7. **Point the entry file at the private folder**: open `public_html/api/index.php` and change the line
   `$privateFolder = dirname(__DIR__);` to
   `$privateFolder = '/home/YOURUSER/mathfun_private';`
8. **HTTPS**: your domain needs a valid certificate (cPanel > *SSL/TLS Status*, AutoSSL).
9. **Check**: open `https://YOURDOMAIN/api/index.php?route=/v1/health`. You should see `{"request_id":"...","data":{"status":"ok"}}`.
10. **Authorization header**: some hosts remove the `Authorization` header. The game also sends `X-Auth-Token`, which the server accepts, so this normally works. Verify by signing in from the game.
11. **Optional cleanup job**: cPanel > *Cron Jobs*, daily:
    `/usr/local/bin/php /home/YOURUSER/mathfun_private/tools/cleanup.php`

If a request returns `"database_unavailable"` (503), recheck step 6. Details are in `mathfun_private/logs/api.log`, which never contains passwords, names or request bodies.

## 4. Unity setup

1. Select `Assets/OnlineData/Resources/OnlineData/ApiConfiguration.asset`.
2. Set **Api Url** to `https://YOURDOMAIN/api/index.php` and tick **Online Enabled**. (It is off by default so builds never call a placeholder address.)
3. Keep the limit fields equal to the same limits in `config.php`.
4. From any script:

```csharp
using MathFun.OnlineData;

var data = PlayerDataService.Instance;
string error = await data.SignInAsync("teacher1", "password123"); // null = success
LocalPlayer child = data.CreatePlayer("Aina", "female", 7, "");
data.MarkConditionEarned(child.playerId, "Level1Pra");
data.RecordScoredResult(child.playerId, "quiz_preschool", 8, -1);
data.RecordOutcomeResult(child.playerId, "game_algorithm_3", true);
```

`PlayerDataService.State` tells the UI: `LocalOnly`, `SignInRequired`, `Offline`, `Synced`, `NeedsAttention`. A local save never means "uploaded"; check `PendingCount`.

### Old local data

`LegacyPlayerPrefsImporter` reads the old PlayerPrefs profiles and flags (it never changes or deletes them). `PlayerDataService` copies them into the **guest** save at start-up and writes a private backup `legacy_playerprefs_backup.txt` in the app's `OnlineData` folder (contains children's names, do not share or commit it). After signing in, call `TransferGuestPlayersToAccount()` **after the adult confirms** to upload them. Shared quiz scores (`QuizScoreEasy/Medium/Hard`) are only backed up, never attributed to a child.

## 5. Common changes

| I want to... | Edit |
| --- | --- |
| Change the server address | `ApiConfiguration.asset` > Api Url |
| Change name length / age range | `Server/config/config.php` (`player_name_max_length`, `player_age_min`, `player_age_max`) **and** the same numbers in `ApiConfiguration.asset` |
| Session length | `token_lifetime_seconds` in `config.php` |
| Add a new game or quiz | One line in `Server/config/activities.php` under `'attempts'` (`'kind'` is `scored` or `outcome`), upload it, then call `RecordScoredResult` / `RecordOutcomeResult` with that ID |
| Add a new lesson flag | Add its name to `activities.php` (`$conditions`) and to `KnownConditionKeys` in `LegacyPlayerPrefsImporter.cs` |
| Set a quiz's real maximum score | `'max_score' => 10` in `activities.php` |
| Table structure change | Add `Server/database/002_something.sql`, import it, never edit `001_initial.sql` on a live database |

## 6. File map

- `Server/public/index.php` – routes (one `if` per endpoint).
- `Server/app/validation.php` – every input rule.
- `Server/app/accounts.php`, `auth.php` – register, login, sessions.
- `Server/app/players.php`, `progress.php`, `attempts.php`, `legacy_import.php` – the data.
- `Server/app/idempotency.php` – makes retries safe (same `operation_id` = same answer, one effect).
- `Server/app/rate_limits.php` – throttling.
- `Assets/OnlineData/Runtime/PlayerDataService.cs` – what the game calls.
- `SyncService.cs`, `ApiClient.cs`, `LocalSaveStore.cs`, `JsonBody.cs` – upload queue, HTTP, local files, JSON writer.

## 7. API summary

All calls are `https://DOMAIN/api/index.php?route=<route>`. Responses are `{"request_id":..., "data":...}` or `{"request_id":..., "error":{"code":..., "message":...}}`.

| Method and route | Purpose |
| --- | --- |
| `GET /v1/health` | Is the API alive |
| `POST /v1/auth/register`, `/v1/auth/login`, `/v1/auth/logout` | Account and session |
| `PATCH /v1/account/password`, `DELETE /v1/account` | Change password, delete account |
| `GET`, `POST /v1/players` | List, create profiles |
| `PATCH`, `DELETE /v1/players/{id}` | Edit (needs `expected_revision`), delete (needs password) |
| `GET /v1/players/{id}/state` | Profile, progress, score summaries |
| `PUT /v1/players/{id}/progress` | Add completed lessons (never removes) |
| `POST`, `GET /v1/players/{id}/attempts` | Record, list finished results |
| `POST /v1/legacy-imports` | Import one old profile, safe to retry |

## 8. Testing

- Lint PHP (PowerShell, in `Server/`): `Get-ChildItem -Recurse -Filter *.php | ForEach-Object { php -l $_.FullName }` **(tested)**
- API test against a **throw-away** database **(tested locally, 28 checks passed)**: `php Server/tests/run.php https://STAGING/api/index.php --i-am-using-a-test-database`
- Unity: Window > General > Test Runner > EditMode > `MathFun.OnlineData.Tests` **(tested, 4 passed)**

## 9. Backup, recovery, deletion

- Back up the database regularly (cPanel > *Backup* or phpMyAdmin > *Export*) and test restoring into a **separate** database.
- Forgotten password: verify the adult's identity yourself, then on the server run `php mathfun_private/tools/account_maintenance.php reset-password USERNAME` (needs SSH/Terminal; if your host has none, ask hosting support or edit the hash through phpMyAdmin using a hash made by `password_hash`). This also signs out all devices.
- Deleting a player or account removes its data from the live database. Old backups and copies on offline devices remain until they expire or are removed. A small record with no personal data (a deleted-ID marker) is kept so an old queued request cannot bring a deleted child back.
- The game keeps an offline copy on the device. Uninstalling the app loses anything not yet uploaded.

## 10. Troubleshooting

| Code | Meaning | Try |
| --- | --- | --- |
| 401 | Not signed in / session expired | Sign in again. If it happens right after login, the host may strip auth headers: see step 10 |
| 409 | Conflict (`revision_conflict`, `username_taken`, `player_deleted`, `operation_conflict`) | The data changed elsewhere; refresh and decide |
| 422 | A value is not allowed | Check the limits in `config.php` / `activities.php` |
| 429 | Too many requests | Wait; the `Retry-After` header says how long |
| 503 | Database problem | Check `config.php` credentials and the log |

Changes the server refuses permanently (404/409/422) are moved to `rejectedOperations` in the local save instead of being retried forever; `PlayerDataService.RejectedCount` shows them.

## 11. Which results are sent

| Where | Activity ID | Type |
| --- | --- | --- |
| `LoadScore` (quiz result screens) | `quiz_preschool`, `quiz_level1`, `quiz_level2` | score out of the quiz's question count |
| `GamePraController` (end panel) | `game_preschool` | score |
| `Game1Controller` (scenes `GT1level2`..`10`) | `game_tahap1_<n>` | score out of `toCorrect` |
| `AlgoController` (scenes `algoLvl1`..`10`) | `game_algorithm_<n>` | won / lost |
| `ScaleCounterController` | `game_balance_<scene>` | won / lost |

Each is one line calling `OnlineResults.Scored(...)` or `OnlineResults.Outcome(...)`. The child is found from the old `PlayerID`. A result is only kept for a child that belongs to the open workspace: guests keep it locally, and an account keeps it once the adult has uploaded the local players.

### Status light

The small dot on the **Online** button shows sync state: grey = not signed in, green = all saved online, amber = waiting (offline or sign in again), red = the server refused some changes.

## 12. Not done yet

See `IMPLEMENTATION.notes.md`. In short: the existing game scripts still read PlayerPrefs (the server holds a mirrored copy).
