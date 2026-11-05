<?php
/*
 * Allowed IDs for progress flags and finished results.
 * To add a new game: add one line under 'attempts' with a stable ID (never reuse an ID).
 *
 *  kind      'scored'  = has a numeric score;  'outcome' = only won/lost.
 *  max_score highest score the game can give. null = not confirmed yet; the server then
 *            accepts any score up to 'score_ceiling'. Set it once you confirm the real value.
 *
 * The condition names match the old PlayerPrefs keys (without the _<playerId> suffix).
 */
$conditions = [];
foreach ([1, 2, 3] as $n)       { $conditions[] = "Level{$n}Pra"; }    // preschool lessons
foreach ([1, 2, 3, 4] as $n)    { $conditions[] = "Level{$n}Tahap1"; } // level 1 lessons
foreach ([1, 2, 3] as $n)       { $conditions[] = "Level{$n}Tahap2"; } // level 2 lessons
foreach ([0, 1, 2] as $n)       { $conditions[] = "Kuiz{$n}"; }
foreach ([1, 2, 3, 4, 5, 6] as $n) { $conditions[] = "Menu{$n}"; }
foreach ([1, 2, 3] as $n)       { $conditions[] = "High{$n}"; }        // earned achievements
array_push($conditions, 'GamePra', 'GameLevel1', 'GameLevel2', 'PraGameKuiz');

return [
    'score_ceiling' => 1000,
    'conditions' => $conditions,
    // Exact activity IDs.
    'attempts' => [
        'quiz_preschool' => ['kind' => 'scored',  'max_score' => null],  // PrasekolahQuiz
        'quiz_level1'    => ['kind' => 'scored',  'max_score' => null],  // Tahap1Quiz
        'quiz_level2'    => ['kind' => 'scored',  'max_score' => null],  // Tahap2Quiz
        'game_preschool' => ['kind' => 'scored',  'max_score' => null],  // GamePrasekolah
    ],

    // IDs that follow a pattern (one per game level / scene). First matching pattern wins.
    'attempt_patterns' => [
        '/^game_algorithm_[0-9]{1,2}$/'  => ['kind' => 'outcome'],                      // algoLvl1..10
        '/^game_tahap1_[0-9]{1,2}$/'     => ['kind' => 'scored', 'max_score' => null],  // GT1level2..10
        '/^game_balance_[a-z0-9]{1,30}$/' => ['kind' => 'outcome'],                     // balance scales
    ],
];
