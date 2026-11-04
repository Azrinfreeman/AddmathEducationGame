# Lesson narration review

## Correction outcome (2026-11-04)

The approved correction pass replaced the original extracted scripts with 216 authored Malay/English narrations on 108 editable lesson-page components. All 216 regenerated MP3s are assigned across ten scenes. Forty-nine editable caption changes address the confirmed time, shape, answer-key and wording conflicts; their before/after values are in [narration-caption-corrections.tsv](narration-caption-corrections.tsv). The perimeter example now uses 8 + 8 + 6 = 22 cm.

Compilation, all 216 runtime text/hash/recording checks, stale-recording rejection, playback controls, language and voice-setting changes, volume updates, and five generator safety tests passed. FFmpeg decoded all 216 MP3s without errors (29.3 minutes, 10.55 MB of MP3 sources). See [runtime results](narration-runtime-verification.tsv), [audio results](narration-audio-verification.tsv) and the [editable maintenance workflow](kids-ui-revamp.md#bundled-lesson-recordings-offline-voice-over-2026-11-04).

The full orientation run captured 432 views with matching recordings and no TMP overflow flags. Visual inspection found obsolete inline symbols overlapping the area-page explanation; disabling those two Image components resolved the overlap, confirmed in four targeted language/orientation captures.

The original findings below and `narration-review.tsv` are historical evidence for the previous 166-clip set. Their old hashes and measured durations do not describe the replacement recordings. The current script export is [lesson-voice-script.tsv](lesson-voice-script.tsv). All replacement recordings still need fluent-speaker listening and Android acceptance; successful decoding does not establish pronunciation or teaching approval.

## Original review (before corrections)

Reviewed 2026-11-04. The narration needs content corrections before acceptance. File coverage passes, but several scripts contain incorrect maths, conflicting times, incomplete formulas, or mixed languages.

## Scope and evidence

Read all 216 exported scripts: 108 pages in 10 lessons, each in Malay and English. Checked all 166 unique MP3s (83 per language) with FFmpeg, recalculated each script's FNV-1a fingerprint, and compared each serialized scene recording reference with its MP3 metadata GUID and expected hash. All checks passed; there are no missing or extra MP3 assets.

Malay clips last 2.69–35.54 seconds; English clips last 1.94–26.16 seconds. Measured peaks range from −7.6 to −3.3 dBFS for Malay and −6.7 to −4.0 dBFS for English. These measurements show successful decoding and peak headroom; they do not certify pronunciation, intelligibility, perceived loudness, or the words actually spoken.

The [page-by-page record](narration-review.tsv) contains the script, source line, clip path, duration, volume measurements, and findings for every page/language combination. Blank findings mean no specific issue was recorded in this text review, not teaching-content approval. Every row remains **not listened**. No recordings, scenes, or runtime code were changed during this review. Playback was inspected in source; it was not rerun in Unity.

## Teaching corrections — high priority

### N01 — Addition is read in the wrong order

**OperasiNombor, pages 6–11, both languages.** For example, page 6 says “1 plus equals 1 2”; page 10 says “4 plus equals 3 7.” Reading scene hierarchy order puts the equals sign before the second operand. Use complete spoken equations: “One plus one equals two” / “Satu campur satu sama dengan dua.” Correct all six examples, rather than changing global reading order without checking other pages.

### N02 — Formula layout becomes broken speech

**Luas, pages 4–6, 9–10 and 12; Isipadu, pages 2, 5–6; both languages.** Fractions, multiplication signs, and superscripts occupy separate labels. Scripts contain fragments such as “Area equals Area of the rectangle 2” and “equals 64 cm. 3.” Area units must be square units; volume units must be cubic units. The triangle formula must say “one half times base times perpendicular height,” with the division and units explicit. Review both candidate calculations on Luas page 12 against the diagram before explaining which is correct. Do not silently turn a deliberately incorrect candidate into a correct answer.

Representative wording: “The area is half of eight times six, which equals twenty-four square centimetres.” / “Luas ialah separuh daripada lapan darab enam, iaitu dua puluh empat sentimeter persegi.” The illustrated dimensions must be checked before applying this example.

### N03 — Time examples contradict their own text or each other

**SebutDanTulisWaktu, page 3, both languages.** Malay says “Pukul 10” followed by “12:20 pm”; English says “10 o'clock” followed by “12.00 pm.” Check the clock artwork and make the caption and narration agree.

**Page 5, both languages.** “Twelve oh five” / “Pukul dua belas lima minit” is paired with 2:05. The date 10/05/2018 is Thursday (Khamis), while English says Wednesday. Confirm the example and remove incidental date narration if it serves no teaching purpose.

**Page 14, English.** Says “twelve twenty,” while Malay says eleven twenty and the answer page gives 11:20. English should agree with the approved question.

**Pages 12–13, both languages.** Time displays are flattened into “12 3 0” and “11 0 5.” Confirm the displayed times, then read them as complete times rather than separate digits.

### N04 — Minutes exercise and answer list need reconciliation

**Kenali Minit, page 11.** Malay asks about movement from 12 to 7 (35 minutes), while English asks from 12 to 6 (30 minutes). The two versions must ask the same question.

**Page 13, both languages.** There are six tests on pages 7–12, but the spoken answer list has only five numbered entries. Test 4 asks about 40 marks, yet answer 4 is 35; test 6 concerns movement from 12 to 11, which is 55 minutes, and has no sixth entry. Check the pictured clocks for tests 1–3 and reconcile the full key before regenerating audio.

### N05 — Shape names are incorrect

**Perimeter, page 4, English.** “Isosceles triangle” disagrees with Malay “segi tiga sama sisi” and the equal-side calculation 3 × 7. Use “equilateral triangle” for the approved example.

**Page 8, both languages.** Says octagon / oktagon sekata, followed by only four sides: 18 + 10 + 18 + 10 = 56 metres. Check the diagram; the calculation suggests a rectangle. The formula alone cannot establish what the illustration depicts.

### N06 — Area lesson includes unrelated or incomplete teaching text

**Luas, page 3, both languages.** A sentence about the red ribbon/vine and the perimeter of a hexagon is copied into an area explanation. Remove or replace it after checking the page's intended example.

**Page 8.** English says “A square can be divided into two equal parts” where Malay describes a rectangle, followed by “An area of a is actually half an area of a.” The nouns are missing. Explain that the illustrated rectangle is split into two equal-area triangles and that one triangle has half its area.

### N07 — Some narration mixes languages

**Perkaitan Waktu, pages 2–3.** Both language scripts include text from the other language; English ends with “24 JAM equals 1HARI,” and Malay repeats an English equation. Malay page 3 also says midnight twice, while English distinguishes midnight and noon. Check both labels against the artwork.

**Luas.** English page 6 reads “panjang x lebar”; pages 9 and 12 contain untranslated formula labels (Malay page 9 reads English too). Provide a complete single-language narration for each page, including measurement units.

## Clarity and coverage corrections

### N08 — Wording needs an editorial pass

**SebutDanTulisWaktu, English pages 6–16.** Examples include “convertion wrong,” “are these two have the same time,” and “twelve twenty o'clock.” Use “o'clock” for exact hours; say “two fifteen,” “quarter past two,” or the approved equivalent for other times. Malay page 6 has “menukaran”; Kenali Minit page 4 has “mint.”

**Perimeter, English pages 1–3.** “Vines” translates Malay “reben” incorrectly in this card-decorating context; use “ribbon.”

**RekodWaktu, English pages 3–6.** Wording is repetitive, and “petang” becomes “evening.” Check the pictured time/activity before choosing afternoon or evening. Prefer one clear sentence per activity.

### N09 — Recordings do not describe much of the pictured teaching content

**Memahami Siri, pages 2–19; Pengalaman Pranombor, pages 1–14, both languages.** Many distinct pages share only a generic title or matching instruction. For example, most number-sequence pages say only “number sequences.” Recordings are present, but this does not give a child who cannot read the pictured numbers or objects equivalent guidance. Review the illustrations and add page-specific explanations or a clear instruction, without revealing exercise answers prematurely. Text embedded in pictures is not included by the TMP-label extractor.

### N10 — AR navigation hints leak into lesson speech

**OperasiNombor, English pages 2, 6 and 8; Perkaitan Waktu, page 2 in both languages.** Scripts include “Choose Try AR to explore” / “Tekan Cuba AR untuk meneroka,” despite the runtime's documented exclusion of that hint. The exclusion matches object names; labels with other names can still enter narration. Decide whether to retain the instruction consistently or exclude the actual hint labels.

## Maintenance and playback observations

**N11 — Override instructions are not implemented.** `Tools/generate_lesson_voice.py` declares an override path but never reads it. The comments in `lesson-voice-overrides.tsv` suggest that adding rows and regenerating is enough; the main maintenance tutorial correctly warns that it is a placeholder. Also, the generator skips any existing nonempty clip unless `--force` is supplied. Do not edit the extracted TSV text alone: runtime matching fingerprints the page text, so an independently changed script will no longer match. A maintainable spoken-text override needs an explicit runtime/editor integration and a reliable regeneration rule.

**N12 — Playback setting changes need a runtime check.** `LessonVoice.Update()` stops speech on a page change and `OnDisable()` stops it on scene/component shutdown. It does not explicitly stop a playing recording when `UI.Voice` or `EnglishLang` changes; volume is applied only when Listen starts. Check these cases through the available settings flow. This is a source observation, not a reproduced failure.

## Acceptance after corrections

1. Correct visible teaching content and both language versions together. Approve ambiguous diagrams and answer keys with the lesson author.
2. Make formula narration editable through a supported per-page mechanism, then re-export, regenerate affected clips, and assign them using the [maintenance workflow](kids-ui-revamp.md#bundled-lesson-recordings-offline-voice-over-2026-11-04).
3. Repeat hash/reference checks and the editor recording-match review after changes.
4. Have a Malay speaker and an English speaker listen to all 166 current unique clips, or the revised unique set. Check the actual words, numbers, units, names, pauses, warmth, pacing, and consistency between languages. Use `narration-review.tsv` to locate each clip; repeated hashes share the same recording.
5. On Android, check first playback, repeat/stop, page changes, scene exit, language changes, voice off, volume, background music balance, and offline playback. These device and listening checks were not performed in this review.
