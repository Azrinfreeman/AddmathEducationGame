# Maintain the kids UI revamp

The shared UI treatment is applied to 61 scenes. Eight navigation hubs have new responsive layouts, an illustrated owl, voice replay, and parent settings. The three playable quizzes, three account screens, ten lessons, and 22 games also have responsive layouts, frames, or HUDs. Physics-driven game boards retain their original geometry and fit-to-screen scaling. The complete design and device acceptance checklist in [UI_REVAMP_PLAN.md](../../UI_REVAMP_PLAN.md) remains open.

[Use the screens](#use-the-new-screens) · [Profiles and avatars](#maintain-profiles-and-avatars) · [Lesson paths and collections](#maintain-lesson-paths-and-collection-screens-2026-11-05) ? [Lesson layouts](#maintain-lesson-layouts) · [Quiz layouts](#maintain-quiz-layouts-and-result-confetti) · [Game layouts](#maintain-game-huds-pause-and-results) · [Lesson narration](#bundled-lesson-recordings-offline-voice-over-2026-11-04) ? [Verification](#what-was-checked-and-what-remains)

## Use the new screens

Open `Assets/Scenes/GetStarted.unity` in Unity 6000.6.7f1 and enter Play mode. Use Mula/Start to enter the existing profile flow. MainMenu, LearningPage, the three lesson hubs, Quizzes, and GameMenu use the new shell.

Choose Ibu bapa/Parents and solve the addition challenge to open settings. A wrong answer requires returning and trying a fresh challenge. Music, sound, reduced motion, voice guidance, and language controls use the existing audio/language preferences plus two new keys: `UI.ReduceMotion` defaults to 0 and `UI.Voice` to 1. Progress keys remain unchanged.

Dengar/Listen replays the navigation instruction. Android uses its installed Malay or English speech engine. An English WAV is available as the desktop fallback. Malay speech has no desktop recording; the caption explains this when replay is requested. Navigation instructions use this speech path; lesson pages use the bundled recordings described below. Quiz questions do not have full voice coverage. iOS native speech is not implemented.

## Find the editable sources

- `Assets/UI/Resources/AdventureTheme.asset`: colours, fonts, owl, rounded surface, tap sound, press scale, release time.
- `Assets/UI/Runtime/`: button feedback, bilingual captions, hub layouts, safe area, parent challenge, settings, and voice integration.
- `Assets/UI/Editor/AdventureUIBuilder.cs`: reproducible asset generation, hub structure, migration, and audit commands.
- `Assets/UI/Prefabs/`: KidButton and ParentGate, editable in Prefab Mode.
- `Assets/UI/Fonts/`: font binaries, TMP font assets, and font licences.
- `Assets/UI/Art/OwlGuide.png`: generated raster mascot. This PNG is the available image source; there is no layered illustration original.

Scene hierarchies are editable with normal uGUI tools. The generated `AdventureShell` is the visible hub Canvas. Original canvases remain disabled so their controllers and referenced buttons keep their identity. New navigation buttons retain the original UnityEvents and mirror source availability. Do not delete the old source buttons or rearrange their children: existing controllers still use child indices.

## Change a button colour and check it

Select `AdventureTheme.asset`. Change `Sections / Element 0` from peach to another light colour that keeps navy text readable. On a themed Image, use the ThemedSurface component's Apply theme context menu, or reload the scene to refresh its tint. Enter MainMenu Play mode and check the learning button in both orientations.

For immediate refresh after editing theme values, reload the scene. ThemedSurface applies on enable; it does not poll the theme every frame. Change individual hub labels through their BilingualLabel components. Change portrait/landscape placement in `HubLayout.cs`; change the grid counts and spacing through AdaptiveMenu. To retain scene-specific edits, avoid regenerating that hub afterward.

The builder is the authoritative source for regeneration. Rebuilding hubs replaces generated AdventureShell objects and overwrites manual changes within them. Editing owned scene hierarchies directly is supported when regeneration is not wanted. Edit ParentGate.prefab directly only if you will not rerun Build shared assets, which regenerates it.

## Rebuild and audit

Commit or back up your work through your usual workflow before a project-wide migration. Exit Play mode and use the Fun Math > UI menu:

1. Build shared assets creates the theme, font assets, rounded sprite, and prefabs.
2. Apply to project scenes adds the shared treatment to the 61 scenes. Existing shells are skipped.
3. Rebuild generated hub layouts rebuilds the eight hubs from source controls.
4. Update content surfaces updates backgrounds and common panel sprites across the project. AR camera backgrounds are excluded.
5. Audit scene wiring writes `docs/tutorial/ui-scene-audit.tsv`.

Use Preview > Portrait 1080 x 1920 and Preview > Landscape 1920 x 1080 while the Game view is open. These commands add reusable Game view sizes using editor-only reflection; Unity exposes no public Game view sizing API. They do not change the project's player resolution.

For a player build, use the project's existing Build Profiles workflow. An Android speech manifest fragment lives at `Assets/Plugins/Android/AdventureVoice.androidlib/`. The fragment declares speech-service visibility required by [Android's TextToSpeech API](https://developer.android.com/reference/android/speech/tts/TextToSpeech). A full Android build and on-device speech check have not been performed.

## What was checked and what remains

The editor compiled the new scripts. The migration audit retained persistent button-action counts in all 61 scenes. A deeper audit found every button has KidButton feedback and every TMP label has a font. These checks do not prove every callback is valid or every label fits.

Play-mode checks covered main-menu navigation through LearningPage and Tahap1Lesson into Kenali Minit, and parent-gate success, wrong answer, and cancellation. Portrait and landscape screenshots were inspected during development. An initial stale Device Simulator safe area and an overwritten challenge label were found and corrected. Full manual device workflows, pronunciation, audio listening, controller navigation, performance, and every lesson/game path remain untested.

`ui-scene-audit.tsv` reports five missing script components in each AR scene and missing UnityEvent targets in older controls. The current package manifest does not contain Vuforia; the original tracked manifest did. Restore the appropriate project-compatible AR dependency through the project's package workflow before testing tracking. Do not delete missing scripts merely to silence the audit.

Game HUDs and dialogs now reflow, and the answer games have responsive targets. Algorithm, rocket, and balance physics boards retain their existing coordinate systems. Lessons have responsive frames, but some source illustrations are blank, low resolution, or contain baked text. Teaching content and translations still need a page-by-page review. Result count-ups, game-specific feedback bundles, specialty-screen layouts, and full spoken lesson content remain unfinished. Existing scene warnings and the AR dependency prevent declaring the complete revamp accepted.

## Maintain lesson layouts

The ten lesson scenes under `Assets/Scenes/Lessons/Prasekolah/`, `Tahap1/`, and `Tahap2/` contain 108 numbered pages. Open one in Unity and enter Play mode. **Seterusnya / Next** and **Sebelum / Previous** keep the original page actions; progress follows the currently visible page. **Keluar / Exit** uses the existing exit confirmation. Where available, **Cuba AR / Try AR** retains the existing additive AR action. AR tracking remains blocked by the missing project dependency described above.

Portrait places a proportional lesson composition above its heading and hints. Landscape places the composition beside the reading text. The frame respects the screen safe area, uses large footer controls, and reuses KidButton feedback. Old decorative scale-in Animators are disabled only when all their bindings animate scale; other content Animators remain intact. There is no new lesson page-turn animation.

Find the editable sources here:

- `Assets/UI/Runtime/LessonScreenLayout.cs`: orientation scaling and the positions of the lesson heading, exit control, parent settings, and page region.
- `Assets/UI/Runtime/LessonPageLayout.cs`: reading-area proportions, uniform composition scaling, footer controls, and focus recovery when a page changes. Its Inspector references identify the owned content, progress, hints, and actions.
- Each numbered page's `DiagramArea/LessonContent`: the original illustration objects and TMP captions, still editable in the scene. Select only the page being edited and disable other pages for a clear editor preview. Keep numbered page names and navigation references intact.
- `LessonHeading` and `ReadingHint` labels: short frame text, with Malay/English values on BilingualLabel. The original inactive hint objects and copied quiz controls remain in the scene. Hidden answer controls are not turned into new lesson exercises.
- `Assets/CheckPages.cs` and `Assets/LessonController.cs`: visible-page progress and initial page-state allocation. Existing saved-progress keys are unchanged.
- `Assets/UI/Editor/LessonUIBuilder.cs`: conversion, frame refresh, verified final-navigation repairs, and the [lesson audit](lesson-ui-audit.tsv).

For a representative customization, select a numbered page's LessonPageLayout component and increase **Design Size / Y** from 796 to 860. Reload that page in Play mode: the original composition fits with more vertical space around it. Edit a caption by expanding LessonContent and changing its original TMP label; use the corresponding BM/BI objects where present. To change the portrait/landscape reading split, edit the anchors in LessonPageLayout's `ApplyLayout` method and check both Game view sizes.

Outside Play mode, save the current scene and run **Fun Math > UI > 7 - Upgrade lesson layouts**. This converts untouched lessons and refreshes owned frame details in converted lessons. It normalizes composition pivots and progress fills, refreshes AR hints and dialog labels, and reapplies dialog button styling. Back up custom edits to those owned elements before rerunning it. Original teaching compositions are retained rather than rebuilt. The command writes the latest before/after audit; rerunning it after a repair will therefore show that repair as already present.

Six final-page Next controls previously referenced deleted objects. They now use the valid Check action already present on the same page. In SebutDanTulisWaktu, the Check target was also missing; it was restored to the existing confirmation panel after checking that the panel leads to the existing completion dialog. The normal completion buttons retain their original progress-writing and scene-loading callbacks.

Verification on 2026-11-04: compilation passed, all 108 page layouts have their required references, and the latest audit reports no missing targets on available lesson branches. Twenty missing targets remain inside inactive duplicated settings groups. These static checks do not prove every callback's behavior. Play-mode checks covered Kenali Minit Next/Previous and completion, the repaired SebutDanTulisWaktu confirmation-to-completion route, and Luas parent-gate approval/settings opening. Portrait/landscape lesson and completion previews were inspected; the checked runs produced no console errors. Progress-unlock buttons and AR tracking were not exercised.

Some existing image containers have no assigned Sprite. The audit's `spriteLessOpaqueImagesBefore` column is a review aid, not a count of definitively missing illustrations: some containers are intentional. Sprite-less backgrounds are made transparent so they do not cover their captions. Missing art, source label overlaps, baked text, translation quality, every page in both languages, and device readability remain acceptance work. This frame migration does not certify the teaching content.

## Maintain profiles and avatars

The account flow uses `Assets/Scenes/PlayerInformation.unity`, `PlayerInformationMain.unity`, and `LoginAddAccountPage.unity`. Choose an animal, enter a name, adjust age with minus/plus, and select **Mari bermain / Let's play**. Gender is optional. The name is limited to 24 characters; age defaults to 7 and the stepper supports 1–120. Select **Pemain baru / New player** on the saved-player screen to add another player. Saved cards scroll vertically and reflow into two columns in portrait or three in landscape.

Choose **Tukar avatar / Change avatar** on a saved card, or select the avatar/name control at the top of MainMenu. Choose an animal and press **Simpan / Save**. **Kembali / Back** discards the pending choice. The dialog confines keyboard navigation to its controls and restores focus when closed. Buttons reuse `KidButton` press feedback and the reduced-motion setting.

The main editable sources are:

- `Assets/UI/Resources/AvatarCatalog.asset`: eight stable animal IDs, Malay/English names, and artwork prefab references. The first entry is the fallback; keep existing IDs stable.
- `Assets/UI/Prefabs/Avatars/`: owl, cat, bear, fox, rabbit, panda, frog, and penguin artwork. Each prefab contains editable uGUI Images for the face, ears, eyes, nose, and cheeks. These are owned shape compositions, rather than flattened character pictures.
- `Assets/UI/Prefabs/ProfileCard.prefab` and `AvatarPickerDialog.prefab`: saved-player cards and the reusable avatar modal.
- `Assets/UI/Runtime/AvatarPicker.cs`, `AvatarPortrait.cs`, and `ActivePlayerProfile.cs`: selection state, artwork display, and the main-menu profile control.
- `Assets/UI/Runtime/ProfileScreenLayout.cs`, `AccountGridLayout.cs`, and `ScrollSelectionIntoView.cs`: orientation layout, scrolling card dimensions, and bringing keyboard-selected cards into view.
- `Assets/UI/Runtime/AgeStepper.cs` and `ProfileFormLanguage.cs`: age bounds/default and form labels. The stepper's Inspector fields configure age limits; the disabled age input displays the value without opening a keyboard.
- `Assets/SaveInformation.cs`, `AccountController.cs`, and `ProfileChangeController.cs`: account creation, account selection, and persistent profile edits.
- `Assets/UI/Editor/ProfileUIBuilder.cs`: bootstrap commands and the C# source for the small circle/triangle sprites in `Assets/UI/Art/`.

To customize an animal, open `Assets/UI/Prefabs/Avatars/cat.prefab` in Prefab Mode. Select **Face**, change its Image colour, and save. Reload a profile screen and select Cat: the picker, saved card, and MainMenu portrait use that same artwork. Change ear positions through their RectTransforms. Labels come from the catalog when building a picker; existing picker labels remain editable in their scene or dialog prefab. Adding a catalog entry alone does not add a button to existing pickers: duplicate a choice and wire its ID, button, selection mark, artwork, and label in both form scenes and the dialog's AvatarPicker component.

Avatar choices are stored in `PlayerAvatar_<playerId>`. Existing player identity and progress keys are retained. A missing or unknown avatar ID displays the catalog fallback without rewriting the saved record. New form gender labels map to the stable values in SaveInformation's `genderValues` array. Account selection also restores the active age; profile edits update both active fields and the record for the current player ID. Repeated account selection or form submission is guarded during the pending scene load.

Run **Fun Math > UI > 6 - Build profile screens and avatars** outside Play mode to bootstrap unconverted scenes. Save the open scene first. Existing catalog/prefabs and scenes with `ProfileShell` are retained so routine manual edits survive. Edit converted scenes directly: their original form controls now live inside ProfileShell, so deleting that shell would also delete those controls. Original canvases remain hidden with their callback targets intact. Their remaining controls are excluded from keyboard navigation. Rebuilding the MainMenu hub through the existing builder restores its profile control when the avatar assets are available.

Verification on 2026-11-04: compilation and Play-mode checks passed for invalid-name rejection, profile creation with a selected avatar, duplicate-submit protection, age/identity copying on account selection, persistent per-player name edits, legacy avatar fallback, modal Save/Back, the MainMenu avatar callback, empty-list guidance, and revealing a card through keyboard focus in an extended scrolling list. Test preference writes were restored. Portrait/landscape screens were inspected during development. Full gamepad traversal, device keyboard behavior, every long translated label, and the destructive reset flow were not tested. The existing parent-gated reset behavior remains in place.

## Maintain quiz layouts and result confetti

Open `Assets/Scenes/Quizzes/PrasekolahQuiz.unity`, `Tahap1Quiz.unity`, or `Tahap2Quiz.unity`. Their 40 question pages keep the original illustrations, answers, correct-answer markers, and navigation actions. Portrait places the illustration above a two-column answer grid; landscape places it beside the grid. Select an answer to see a written response and a green or peach fill. Answers remain editable before submission, as in the original quiz.

The Canvas has `QuizScreenLayout`, which selects a portrait or landscape reference resolution and reserves the safe area. Each numbered page has `QuizPageLayout` with references to its illustration, question, answers, progress, and arrows. `IllustrationContent` preserves the original illustration proportions. Edit illustrations inside that container with normal RectTransform tools. Do not change the four answer buttons' sibling order or the numbered page names: existing scoring still uses them.

For a representative layout change, edit the portrait answer bounds in `Assets/UI/Runtime/QuizPageLayout.cs`: change the top anchor from `.31f` to `.32f` to slightly increase the answer grid height. Enter Play mode and inspect the longest answer in both languages. `QuizAnswerFeedback.cs` owns the two short response messages and answer colours. `QuizUIBuilder.cs` owns the generated heading and shared text settings. Existing question and answer wording remains editable in the scene's TMP labels.

`CheckQuizNumber.cs` now binds the progress caption and slider to the current page instead of retaining the first page's widgets. `buttonSelectedController.cs` finds its own answer children rather than searching the scene globally. `QuizController.cs` initializes its answer array immediately. Score and progress preference keys are unchanged.

`ResultCelebration.cs` provides an 18-piece confetti burst for quiz scores above zero and existing game win panels. The Inspector exposes its duration and piece references; edit each piece's Image colour and RectTransform size directly. It uses unscaled time, captures no input, and hides the pieces after completion or when disabled. The existing reduced-motion preference suppresses confetti. No new sound asset, package, or gameplay delay is introduced.

Run **Fun Math > UI > 5 - Upgrade quiz layouts and result feedback** outside Play mode to apply this pass. Save the open scene first. This command updates quiz layout components and adds result components across scene assets. Back up manual layout edits before rerunning it: generated headings and references are refreshed. Confetti components already present are retained.

Verification on 2026-11-04: Unity compilation passed; all 40 pages have four answers, one correct-answer marker, and the required layout references. Tahap 1 Play-mode checks confirmed correct/incorrect answer recording, Next/Previous navigation, and page-two progress (`2 / 10`). Portrait and landscape screenshots were inspected. Confetti was checked for normal start, reduced-motion suppression, and automatic cleanup. The final Tahap 1 run produced no console errors. Legacy audio callers now tolerate the shared click source being absent when a scene is opened directly. Every game's natural win path, all quiz wording, device performance, and touch accessibility remain manual acceptance checks.

## Maintain game HUDs, pause, and results

Open a scene under `Assets/Scenes/Games/` and enter Play mode. Where present, the answer, algorithm, and rocket introductions explain the activity; choose **Mari main! / Let's play!** to continue. The balance game starts directly, with **Cara main / How to play** available. **Rehat / Pause**, Escape, or the gamepad Start button opens a menu with Resume, Start again, and Games. Pausing freezes simulation time; the menu uses unscaled time. Leaving or disabling the shell restores the previous game speed. Returning from another app requests a pause after startup.

The original `Canvas` owns game rules, physics, and callback targets. The separate `GameShell` owns the HUD and pause overlay. Keep both. Original home/settings buttons remain hidden; parent settings are available from the navigation hubs. Each visible legacy intro/result is still triggered by its original Animator or controller, with a new fitted body, bilingual labels, owl, and a short opacity fade. Modal selection stays on its actions. Existing win panels use shared reduced-motion-aware confetti.

Find the editing points here:

- `Assets/UI/Runtime/GameChromeLayout.cs` positions the HUD, action buttons, and hints. `GameShell/SafeArea` contains their editable uGUI objects and BilingualLabel captions.
- `GamePause.cs` owns pause/resume, clock restoration, restart, and the Games destination. `GameModalFocus.cs` owns the dialog fade and focus confinement. `ModalBodyFit.cs` fits game dialogs inside the safe area.
- `GameHud.cs` subscribes to change events on `Game1Controller.cs`, `GamePraController.cs`, `TimeGameController.cs`, and `ScaleCounterController.cs`. Existing controllers retain the score, timer, and placement rules. The HUD reads their snapshots when notified; it does not poll PlayerPrefs for scores.
- `GameAnswerBoardLayout.cs` reflows the four or five answer groups in the ten WhackAMole scenes. Portrait uses two columns; landscape puts the question beside the answer grid. Its target references retain their sibling order. The answer ink overrides legacy appearance clips that would otherwise restore white text on yellow fills.
- `RocketDirectionButton.cs` connects each new rocket button to the existing ButtonController. It supports pointer hold/release and holding Space, Enter, or gamepad confirm on a selected direction. `RocketQuestionLayout.cs` fits the separate question artwork below the HUD. It does not move the rocket, bubbles, or colliders.
- The algorithm Run/Clear buttons copy existing actions and mirror source availability through `NavigationState.cs`. The original action objects remain inside the physics board. Clear retains the existing level-reload behavior.
- `Assets/UI/Editor/GameUIBuilder.cs` converts untouched scenes and writes the [migration audit](game-ui-audit.tsv). The [final reference audit](game-ui-verification.tsv) checks shells, input systems, new action targets, layout references, timed panels, and scene destinations.

For a routine wording change, open `Assets/Scenes/Games/AlgorithmGame/algoLvl1.unity`, select `GameShell/SafeArea/Run/ActionLabel`, and edit its BilingualLabel values. Change Malay **Jalan** to **Mula** and keep English **Run**. Enter Play mode: the caption changes while the same four original actions still run. To move these controls, edit the `primaryAction` and `secondaryAction` anchors in GameChromeLayout and inspect both preview sizes. To adjust answer spacing, select a scene's GameAnswerBoardLayout and change **Target Envelope Height** from 260 to 280; targets shrink slightly when their available row height is limited. Check the raised answers as well as their resting holes.

Save the current scene and run **Fun Math > UI > 8 - Upgrade gameplay HUDs and dialogs** outside Play mode to convert an untouched game. Scenes already containing GameShell are retained, including manual edits. Do not delete GameShell to force regeneration: the command also updates existing modal bodies. Save a copy before experimenting with another migration. Keep original game child names, sibling order, callback targets, and physics transforms intact when editing content.

The timed games now reference `againPanel` and `TimeUp` explicitly. Their copied quiz-only LoadScore component is disabled on the timeout banner. The algorithm intro no longer calls a deleted StartTime target; the balance intro no longer calls a deleted object. The balance prototype has one playable level: result buttons that pointed to absent `Level2` and `AddingGame` scenes now return to GameMenu. Existing valid level routes, including the first answer game's next level, are retained. No save-key names were changed.

Verification on 2026-11-04: all 22 games compiled and passed the final static reference checks. The migration audit compared original collider paths, local positions, local scales, sizes, and offsets before and after conversion; those serialized values matched. Play-mode checks covered the timed intro, pause/frozen timer/resume, correct-answer callbacks and win, timeout/restart, next-level navigation, rocket hold/movement/release, leaving a paused game, four physical balls reaching equal basket totals and a win, and a six-arrow algorithm route reaching its correct shape. Pause also restored a non-default game speed when resumed or disabled, and returned keyboard focus to a visible control. Confetti started normally, suppressed itself with reduced motion, and cleaned up. These checks use the actual controllers, pointer handlers, physics, and UnityEvents; they do not replace a child testing the game on a touchscreen.

Portrait and landscape Game view previews were inspected. The sample [portrait answer board](screenshots/game-whack-portrait.png), [landscape answer board](screenshots/game-whack-landscape.png), and [algorithm controls](screenshots/game-algorithm-landscape.png) are captured from the running Game view. `AdventurePreview.CaptureGameView(path)` exports its real rendered texture, including overlay canvases; a camera-only capture can omit this UI. The helper uses editor-only reflection because Unity exposes no public Game view capture API.

Every level's natural win/lose path, continuous touch dragging, full controller traversal, device safe areas, audio listening, performance, and all translations remain untested. Some original answer wording and artwork still contain Malay or baked text when English is selected. The drag games still require dragging; a tap alternative, countdown, score count-up, mascot state machine, and richer hit/thruster feedback are not part of this gameplay pass. The physics-based layouts deliberately retain their existing boards while the surrounding controls adapt.

## Asset sources and export

Fredoka and Nunito were downloaded from the [Google Fonts repository](https://github.com/google/fonts), with the respective OFL licences retained beside them. The owl was generated for this project on 2026-11-04. Existing project illustrations remain in use; their original provenance is unchanged. The English navigation WAV was synthesized locally with Microsoft Zira. The rounded panel is generated from editable C# in AdventureUIBuilder.

The Markdown file is the authoritative tutorial. Regenerate its HTML companion from the repository root:

```powershell
python docs/tutorial/export_tutorial.py
```

The HTML companion was rendered and visually inspected. Open it in a browser and use Print > Save as PDF if a PDF is needed. No PDF export has been verified.

## Badges, credits and acknowledgements

`Fun Math > UI > 9 - Upgrade badges and credits` adds a `SpecialtyShell` canvas to `BadgesPage`, `CreditPage` and `PenghargaanPage`. The original canvas is kept but deactivated, so its objects and callbacks are preserved. Badges use `BadgeShelf` (six bilingual cards, tap for requirements; the save keys and unlock rules are unchanged). Credits and acknowledgements are scrolling bilingual text copied from the old containers. Scenes that already contain `SpecialtyShell` are skipped, so manual edits survive a re-run.

Status (2026-11-04): inspected in the running Game view. [Badges portrait](screenshots/badges-portrait.png) and [landscape](screenshots/badges-landscape.png) (2 columns becoming 3), [credits](screenshots/credits-portrait.png) and [acknowledgements](screenshots/penghargaan-portrait.png) all render and fit. Fixes made after the first look: the full-screen backdrop is now a solid fill (rounded corners showed the camera colour), and four stray quiz-timer labels ("15 MINIT" etc.) were removed from the credits list. Not yet tested: tapping a badge to open its details, English text, touch scrolling on a device, and the landscape Back button looks wide and short. AR overlays are still undone because Vuforia is missing from the project.

### Fix: rounded panels were not 9-sliced (2026-11-04)

`Assets/UI/Art/RoundedPanel.png` imports as a Multiple sprite sheet, and the border set by `AdventureUIBuilder` was ignored, so every rounded surface in the revamp was stretched into soft ellipses. The border is now set on the sprite sheet entry in `RoundedPanel.png.meta` (28 px each side); the sprite name and ID are unchanged, so no scene references changed. If you ever regenerate the PNG, re-check that **Sprite Editor > RoundedPanel_0 > Border** is 28 on all sides. Badges and credits were re-captured in landscape after the fix; **other screens captured earlier (game, quiz, lesson, profile) have not been re-inspected** and should now show crisper corners. The badge details dialog was verified in English by reading its text (title, requirement, progress), but the dialog was not captured visually. The unfocused editor stops ticking in Play mode, so set `Application.runInBackground = true` before scripted UI tests.

### Re-inspection after the border fix (2026-11-04, landscape)

Checked in the running Game view: **MainMenu** (owl, six coloured tiles, parent and listen buttons) and the **Tahap 1 lesson hub** both show crisp rounded corners and fit. Known polish items found:

- MainMenu: the KUIZ tile has no icon while the other tiles do, so the row looks uneven.
- Tahap 1 hub: *Rekod Waktu* and *Perkaitan Waktu* reuse the same thumbnail (original art, not changed).
- Quiz intro dialog (`Tahap1Quiz`): the body text (`Canvas/IntroPanelParent (1)/Panel/AudioPanel/GameObject/Text (TMP) (1)`, legacy, negative top margin, auto-size off) reaches the dialog card's left and right edges. Give it 40 px of left/right margin or enable auto-size; the other quiz scenes likely share the same copied panel.
- Behind that dialog the legacy quiz still shows its black X and gear icons in landscape, as noted earlier.

Not re-inspected: the game scenes, profile screens, portrait versions of the older screens, and the other 20+ lesson scenes.

### Polish fixes applied (2026-11-04)

- **KUIZ tile icon:** `Assets/Images/MainMenu/math-class.png` was imported as a *Multiple* sprite sheet with no sprites, so it never loaded. It is now *Single*, and `MainMenu` has an `ActivityArt` child on `Activity_Option (2)` with matching label placement. Verified in the Game view (landscape).
- **Quiz intro text:** the legacy intro body text (`IntroPanelParent*/Panel/AudioPanel/GameObject/*`) now has 40 px side margins in `PrasekolahQuiz`, `Tahap1Quiz` and `Tahap2Quiz` (`Quizzes.unity` has no such panel). The scene edit was saved but **not re-captured**, so confirm the dialog visually.

### Landscape sweep (2026-11-04)

Also checked in the running Game view, with no layout problems found: **GameMenu** (level buttons with completion ticks), the **WhackAMole intro dialog** (`GameTahap1`: owl, instructions, "Mari main!") and **PlayerInformationMain** (avatar grid, name, age stepper, gender, start button). Wording nit: GameMenu's heading reads "Jom belajar!" ("Let's learn!"), the same as the lesson hubs. Remaining unchecked: gameplay after the intro, the other 20+ game levels, the rest of the lesson scenes, and every portrait view of the older screens.

## In-game extras: countdown, score count-up, tap alternative (2026-11-04)

**3-2-1-Go countdown.** `Assets/UI/Runtime/GameCountdown.cs` shows "3, 2, 1, Pergi! / Go!" after a timed game's intro is dismissed. The legacy intro button calls `StartGame1.StartTime()`, which now calls `GameCountdown.Begin(...)`; the clock (`TimeGameController.isStopping`) is released only when the countdown ends. A scene without a countdown starts immediately as before. The overlay blocks taps while it runs, uses unscaled time (so a pause cannot desync it), plays the UI tap sound per step, and holds still when **Reduce Motion** is on. Edit the words, step length (`Step Seconds`, default 0.8 s) and font in the `GameShell/Countdown` object of any WhackAMole scene. **Fun Math > UI > 10 - Add game countdown** adds it to every timed game scene that lacks one (10 scenes now: GameTahap1, GT1level2-10); it is safe to re-run. Verified in `GameTahap1`: the clock stayed at 30 s through the countdown, then ran (30 to 27.6 s), and the overlay released input.

**Score count-up.** In `GameHud`, the rocket game's score now counts up to its new value (about 0.6 s for any jump, snapping with Reduce Motion or when the score drops). Answer and balance counters are unchanged apart from their existing pulse. Not yet visually checked in play.

**Tap alternative for dragging.** `DragScript` (algorithm arrows and the balance balls) still drags as before, and now also accepts **tap a piece, then tap where it should go**. The picked piece grows slightly and dims; tapping it again, dragging it, or pressing a button cancels. The tap move reuses the exact hold/release steps of a drag (collider and Rigidbody2D off, then restored with the same mass and gravity), so existing drop detection should behave the same. The placement logic was verified in `algoLvl1` by calling it directly: the piece moved to the tapped point and its physics, alpha, scale and collider were restored. **Not verified:** real mouse/touch presses (the editor session was unfocused, so simulated input was discarded), whether a tap-placed piece triggers each level's slot detection, and the balance game. Please try it by hand on a device or in the editor.

Not done: mascot reaction states, hit/thruster particle effects, and a floating "+1" pop.

## Positive feedback layer: owl cheer, +1 pop, confetti (2026-11-04)

`Assets/UI/Runtime/GameFeedback.cs` reacts to the existing change events (`Game1Controller.ProgressChanged`, `GamePraController.ProgressChanged`); it never touches gameplay objects or the clock. On a **correct answer** (WhackAMole answer count rises; rocket score rises) it plays: a floating **+1** that rises from the progress chip, a small ten-piece **confetti burst** in the theme colours, a **tap sound whose pitch climbs** with a run of good answers (6 s window, resets after a pause or a miss; `UIFeedback.Tap` now takes an optional pitch), and a brief **owl banner** at the bottom centre ("Bagus! / Great!", then "Hebat!", "Pandai!" for a streak). On a **drop in the rocket score** the owl only says "Cuba lagi! / Try again!": no red, no shake, no harsh sound. With **Reduce Motion** the pop and confetti are skipped and the banner only fades.

Added to the 10 answer games plus the rocket game (11 scenes) as `GameShell/SafeArea/Feedback`, drawn last so it sits above the HUD. **Fun Math > UI > 11 - Add game feedback** adds it to any answer/rocket scene that lacks one (safe to re-run). Edit phrases, streak window and the origin chip on the `GameFeedback` component; the owl sprite and colours come from the Adventure theme.

Verified in `GameTahap1` by raising the correct-answer count in Play mode and pausing a few frames into the animation: the owl banner ("Bagus!"), confetti and +1 all appeared, and I moved the layer to the top of the draw order after the +1 was found hidden behind the chip ([capture](screenshots/game-feedback.png), taken with the intro dialog still open). Not verified: a real mole tap, the rocket game's score-drop "Cuba lagi!", English text, portrait layout, and how the sound's rising pitch actually sounds. The balance and algorithm games have no score event, so they have no feedback layer. Rocket thrusters are covered in the next section.

## Rocket thruster (2026-11-04)

`GamePrasekolah` has a rocket flame sprite (`Rocket/Image`, sprite *burnfueled*) that nothing ever switched on. `Assets/UI/Runtime/RocketThruster.cs` now drives it: a small idle flame is always lit, and while the rocket moves it grows to full size, flickers (fast noise on its length) and leans 12 degrees away from the direction of travel. It only reads the rocket's `Rigidbody2D` velocity and changes the flame's own scale and rotation, so the rocket's physics, collider and controls are untouched. It uses scaled time, so pausing freezes it. **Reduce Motion** keeps a steady, upright flame. Tune `Idle Size`, `Max Lean Degrees` and `Full Speed` on the component; the flame sprite no longer blocks touches.

Verified in Play mode: the idle flame at about half size, then a held "Kanan" (right) gave full size and a lean back to the left ([capture](screenshots/rocket-thruster.png)). Not verified: left movement, how the flicker looks in motion, and portrait. There are no separate spark particles; it is the one flame sprite only.

## Fix: rocket bubbles fell in the wrong place in landscape (2026-11-04)

**Cause.** `GamePrasekolah` was authored for a 1080-unit-wide portrait canvas. In landscape the canvas becomes about 3413 units wide, but the bubble lanes (`Canvas/bubbles`, stretched to the full width, with bubbles anchored near its left edge) stayed in the left third, the rocket still started at screen centre and the walls sat at the far edges. The bubbles therefore fell far to the left of the rocket, over the Kiri button.

**Fix.** **Fun Math > UI > 12 - Centre rocket playfield** pins the `bubbles` container and the `w` / `borderright` walls to a centred 1080-unit column (anchors move from the edges to the centre with the portrait position preserved, so a 1080-wide canvas is identical to before). The command is safe to re-run. `RocketQuestionLayout` now places the question card in the free space at the left in landscape (anchor 0.17, 0.67) instead of the middle of the lane; portrait is unchanged. Bubbles are still gravity-driven Rigidbody2D objects and were not altered.

**Checked.** In a 1920x1080 Play session the bubbles landed between x 675 and 1071 around the rocket at x 960, inside the walls at 634 and 1281. A scripted pilot that steered the rocket to the correct bubble collected them (score 10 to 40, level 2 to 3, no wrong hits), so the centred lane is reachable at the existing rocket speed. The question card clears the Kiri button ([capture](screenshots/rocket-centred.png), intro dialog still open). **Not checked:** portrait (it should be identical), other aspect ratios, bubble motion on a device, and the same wide-canvas problem in the other game scenes (the algorithm and balance boards were also authored in portrait and may need a similar look).

## Landscape check of the algorithm and balance games (2026-11-04)

Both boards were authored in portrait, so I checked them at 1920x1080 for the wide-canvas problem found in the rocket game.

- **Algorithm (`algoLvl1`)**: no problem. The board is a compact centred column (shape, arrow tray, algorithm box) with Jalan/Padam at the right and the hint at the left, so nothing sits in the wrong place. Not played through in this pass.
- **Balance (`Level1`)**: the board is centred and fine, but the hint overlapped the orange floor bar and the "Cara main" button text had no padding. In `GameChromeLayout`, landscape now puts this game's hint (only the game with a help button) in the empty strip under the title, and the help button is wider and shorter. Portrait positions are unchanged ([capture](screenshots/balance-landscape.png)).
- Left as is (original art/layout): the four balls start overlapping, and their "5 KG" labels sit on top of the balls.

Other game levels (algorithm 2 to 10, WhackAMole 2 to 10) share these scenes' structure but were not each opened in landscape, and no portrait views were rechecked in this pass.

## Landscape sweep of all algorithm and WhackAMole levels (2026-11-04)

A Play-mode script loaded every remaining game level at 1920x1080 (algorithm 2 to 10, WhackAMole 1 to 10), dismissed the intro and captured the Game view; the console showed no errors.

- **Algorithm 2 to 10**: all clean and consistent with level 1.
- **WhackAMole question pictures were wrong in three levels.** `GameAnswerBoardLayout` set the picture's box but left each level's authored compensating `localScale` in place: 0.1 in level 6 (a tiny speck), 1.96 in levels 8 and 9 (the clock and the boy covered the answers and the hint). The layout now resets that scale to 1, so the aspect-preserving box sizes every picture the same way. Confirmed on levels 2, 5, 6, 7, 8 and 9 ([contact sheet](screenshots/whack-levels-fixed.png)); level 10 was not captured again.
- **Long answer labels ("7:00 PAGI", "SEMBILAN") touching their button edges: fixed below.**
- The earlier music persistence warning was fixed on 2026-11-05; see the audio persistence section below.
- Sweep captures happen with the editor unfocused, so a pause dialog sometimes shows (the game pauses itself when the app loses focus). That is expected.

Still unchecked: portrait views of these levels and the older lesson scenes.

### Fix: answer labels overran their buttons (2026-11-04)

My first guess (turn on auto-size) changed nothing because the labels were already auto-sized. The real cause: each legacy answer text box is about 269 units wide inside a 254-unit button, so auto-size shrank the text to fit the *box*, not the button, and long answers ran past the button edges. `GameAnswerBoardLayout` now pins each answer label inside its button (stretched, 14 units of side padding, 6 top and bottom) and lets auto-size shrink to that (minimum 18). It runs every frame but only writes when something differs. Checked in levels 2, 3, 4, 5, 9 and 10: "7:00 PAGI", "PUKUL SEMBILAN", "60 MINIT", "TENGAH HARI" and the rest sit inside their buttons ([contact sheet](screenshots/whack-answer-labels-fixed.png)); long two-word answers wrap onto two lines. Levels 6, 7 and 8 were not re-captured after this change, and English text was not checked.

## Vuforia 11.4.4 on Unity 6 (2026-11-04)

Vuforia Engine **11.4.4** (the Unity 6 era package, added from `Packages/com.ptc.vuforia.engine-11.4.4.tgz`) is installed and working with the project's 11 AR scenes. Vuforia 11.x is only distributed from the developer portal or the Unity Asset Store (both need a login), so it cannot be added from a public registry.

**What was wrong after the install.** Ten of the 11 AR scenes (all except `ARIsipadu`) and `Assets/Resources/VuforiaConfiguration.asset` still referenced the removed 10.x library by its old GUID (`1e108ae5f2133934f948edded555f03e`), so their Vuforia camera and image-target components showed as missing scripts. The new library is `8a9a760f95896c34689febc965510927` (`Vuforia.Unity.Engine.dll`). The script IDs inside a library are derived from class names, so replacing the GUID text in those 11 files reconnected every component (23 references). Originals were copied to `Temp/vuforia-backup/` first (Temp is not version controlled; copy it elsewhere if you want to keep the backups).

**Other changes**
- `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`: the **ARCore loader was removed from Android's XR loaders**. Vuforia 11.4 no longer supports AR Foundation, and the loader would add ARCore build steps and Play Store requirements. Automatic loading was already off, so nothing started at runtime. The `com.unity.xr.arcore` and `com.unity.xr.management` packages are still installed (remove them if nothing else uses them).
- `Assets/VideoController.cs`: it found the Play/Stop buttons by child index and found the video with `GameObject.Find`. That crashed in `ARIsipadu` (buttons are at `Canvas/Panel/2/`) and left the video unfound in seven scenes where the `video` object starts inactive. It now searches by name, including inactive objects, and warns instead of throwing.

**Verified in the editor (Play mode, one webcam).** All 11 scenes: Vuforia initialised, the `FunMathDatabase` dataset was created, the target was registered (status NO_POSE, as expected with no card in view), and the video player plus Play and Stop buttons were found. The console showed no errors. The license key (380 characters) was kept. Each scene's image target name exists in the database (11 of 11).

**Not verified**
- Real tracking of a printed card, the AR video playing, and the camera on an actual Android device. The editor test shows initialisation only.
- An Android build (Vulkan is the only graphics API set; this may need a check with Vuforia 11.4 and your 16 KB page-size and target API choices).
- The AR scenes' on-screen UI: restyled in the next section.
- Housekeeping done: the unused 10.12.3 tarball was deleted, and opening **Window > Vuforia Configuration** let Vuforia update `VuforiaConfiguration.asset` itself (now version 11.4.4, EULA 11.4).

## AR screens (2026-11-04)

**Fun Math > UI > 13 - Upgrade AR screens** adds an `ARShell` canvas to all 11 AR scenes (re-runnable; scenes that already have one are skipped). It does not touch tracking, the video surface, Play/Stop or the original exit dialog; it only replaces the old grey X and the "SCAN KAD" backdrop and text (hidden, not deleted).

- **Back pill** (top-left, "Kembali / Back") replaces the grey X. `ARBackButton` presses the scene's original `exit` button, so the same confirmation dialog and scene unload run. The Android Back key (Escape in the editor) does the same. The overlay hides while that dialog is open.
- **Scan hint**: an owl card ("Halakan kamera pada kad / Point the camera at the card") with four yellow corner brackets that breathe gently (still with Reduce Motion). The old card thumbnail stays as a reference of what to scan.
- **Card found**: `ARScanGuide` listens to the image target's status. When Vuforia tracks the card (TRACKED or EXTENDED_TRACKED) the hint and frame hide and a green "Kad dijumpai! / Card found!" message pops with a rising tap sound; if the card is lost they return. The video surface and Play/Stop are still shown and hidden by the scene's own `DefaultObserverEventHandler`.
- `ARChromeLayout` places everything for portrait and landscape inside the safe area. Edit wording, colours and positions on the `ARShell` objects in any AR scene.

**Also fixed.** `PraKuizController`, a copied quiz-intro component on the exit dialog, threw a NullReferenceException the first time the dialog opened because AR scenes have no quiz timer; it now checks first.

**Verified in the editor** (`ARLuas` in landscape and portrait, then all 11 scenes in Play mode): the hint, frame and Back pill render ([landscape](screenshots/ar-hint-landscape.png), [portrait](screenshots/ar-hint-portrait.png), [card found](screenshots/ar-found-landscape.png)); simulating found and lost swaps the states correctly; tapping Back opens the original dialog and hides the overlay, which returns when the dialog closes; Vuforia initialises in all 11 scenes and each guide is wired to its target; the console shows no errors. **Not verified:** detection of a real printed card (found and lost were simulated, the editor webcam showed an OBS feed), the Android camera, English text on the exit dialog (its wording is the original Malay), and the legacy video Play/Stop buttons keep their old round art.

## Android build settings vs Vuforia 11.4.4 (2026-11-04)

Checked from the editor (no Android build was run; building means switching the active platform and reimporting all assets):

- Vuforia 11.4.4's Android library (`VuforiaEngine.aar`) contains native code for **arm64-v8a** and x86_64 only. Public release notes say 32-bit ARMv7 was deprecated in 10.22 and removed in the 11.x line. The project was set to ARMv7 + ARM64, so the 32-bit slot would have had no Vuforia library. **Android target architecture is now ARM64 only** (which Google Play also requires).
- Graphics API is OpenGL ES 3 (an earlier note here said Vulkan; that was a misreading of the setting), min SDK 26, IL2CPP, auto-rotation with portrait and both landscapes allowed. Vuforia 11.4 needs Unity 6000.0.38 or newer for the 16 KB page size on Android 15+; this project is on 6000.6.7.
- Still to do on a build machine: an actual Android build, and confirming the camera permission prompt on the first AR scene.


## Maintain English wording (2026-11-04)

English wording is stored in the scene assets under `Assets/Scenes/`. This pass corrected 270 labels across 37 scenes: clock-game answer translations, lesson headings and explanations, quiz instructions, and selected account and credit labels. The minutes lesson now correctly says a quarter hour is 15 minutes. `Assets/CheckQuizNumber.cs` formats the English counter as "Question 1 of 15".

For shared labels, select the TextMeshPro object and edit **BilingualLabel > English**. Keep its Malay field intact. For example, open `Assets/Scenes/Games/WhackAMole/GT1level10.unity`, find the answer label containing "SUKU JAM", and edit its English value "QUARTER HOUR". Enter Play mode and switch language in Parents settings: the label should alternate between those two values. The existing `Assets/UI/Runtime/BilingualLabel.cs` reads `EnglishLang`; no new localization framework was added.

Older lesson and quiz objects use `ChangeLanguage` to toggle separate Malay and English branches. Follow that component's **English** reference and edit the referenced TMP text. Rebuilding a screen with a migration command can replace scene-specific wording, so maintain the converted scenes directly. Save scenes normally; no export or asset-generation step is needed for wording changes.

Verification: Unity compiled the counter change without console errors. A Play-mode sweep switched both languages in 22 scenes, including all ten clock-game levels, and found no mismatches between active BilingualLabel text and its selected language field. Callback method names, callback target types, correct-answer flags, and required-answer counts matched the pre-edit scene snapshots in all 37 changed scenes. Landscape screenshots of clock levels 2 and 10, the first minutes-lesson page, and the corrected quarter-hour page were inspected. Clock level 10 was also inspected in portrait; its English answer labels fit. The quiz counter was checked in Play mode as "Question 1 of 10". See the [wording changes](english-wording-changes.tsv) and [language-switch results](english-wording-play-verification.tsv). The sweep did not play through questions or validate every hidden lesson page, text embedded in pictures, video subtitles, audio, or device layouts.

## English wording audit and lesson-art fixes (2026-11-04)

**Method.** A Play-mode script loaded all 61 scenes with English selected and recorded every visible text (620 entries, saved as `docs/tutorial/english-audit-raw.tsv`). Entries were flagged for Malay words and the main screens were read by eye.

**Already in good shape.** Main menu, hubs, game HUDs and dialogs, quizzes, badges, credits and the AR screens read as English. This pass also covered the AR exit dialog, which now has English ("ARE YOU SURE YOU WANT TO LEAVE AR MODE?" / "YES") in all 11 AR scenes.

**Fixed**
- **Lesson hub cards showed "no of topics" in English** (six cards: Preschool *Number operations*; Tahap 1 *Say and write the time*, *Record time*, *Time relationships*; Tahap 2 *Perimeter*, *Volume*). The English field of their `BilingualLabel` had been captured from the wrong legacy text. They now read NUMBER OPERATIONS, SAY AND WRITE THE TIME, RECORD TIME, TIME RELATIONSHIPS, PERIMETER and VOLUME, which are the English names the old cards used. The Malay side was not changed.
- **WhackAMole level 5**: the answer "SIANG" had no English; it now shows **DAYTIME** (my choice of word, please confirm with the content author).
- **Lesson pictures that never showed.** Eight clock and scene illustrations in *Kenali Minit* and *Sebut dan Tulis Waktu* were BMP files Unity could not turn into sprites, and their images were also set fully transparent. The BMPs were converted to PNG (Single sprite), every scene reference was repointed (originals in `Temp/lesson-backup`, which is not version controlled), and the images set to opaque white. All 29 pages of the two lessons were captured in English and the pictures now appear ([contact sheet](screenshots/lessons-art-restored.png)). A check of every lesson scene found no remaining sprite references that fail to load.

**For the content author to check (not changed)**
- *Understanding minutes*, last page ("ANSWERS"): six tests but five answers, listed as "1. 45 minutes, 2. 50 minutes, 3. 40, 4. 35, 5. 35". Answers 3 to 5 carry no unit and do not match the test questions (for example Test 4 asks what 40 marks represent, and Test 6 asks for 12 to 11). This looks like an error in the original answer key.
- Some illustrations have Malay baked into the picture (for example the *Say and write the time* page about wrong numerals), so those stay Malay in English mode.
- Credits keep the original Malay job titles and school names on purpose.

The language setting used for testing was set back to Malay.

## Lesson voice-over (2026-11-04)

Every lesson page now has a **Listen / Dengar** button in the header (blue, between the title and *Parents*). Tapping it reads the current page aloud in the selected language; tapping it again while it speaks stops the voice, and moving to another page or leaving the lesson also stops it. Added to the 10 lesson scenes (Preschool 3, Tahap 1 4, Tahap 2 3) by **Fun Math > UI > 14 - Add lesson voice-over** (re-runnable).

**How it works.** (Superseded for recorded pages by the next section: the lessons now ship with recordings.) On Android, `SpeechService` uses the device's own text-to-speech engine with a Malay (`ms-MY`) or English (`en-GB`) voice, slightly slowed for young listeners. `LessonVoice` builds the text from the page's visible content: it skips button captions, the page counter and the "Choose Try AR" hint, reads in scene order (so a question comes before its answer), and reads the page's summary caption (the `Questions` object) last. ALL-CAPS words are lowercased so engines do not spell them out, and fill-in-the-blank lines are spoken as "blank" / "ruang kosong". The button hides when a parent turns **voice guidance** off in the parent settings. If the device has no voice for the language, a short message says what to install (the Malay voice must be installed in Android's Text-to-speech settings).

**Where to change it.** On each scene's lesson canvas, the `LessonVoice` component has the reading order, the caption name, and the names to skip. The header spacing is in `LessonScreenLayout` (`listen`, `voiceMessage`).

**Also fixed (found while testing).** `ChangeLanguage` picked the language objects as a parent's last two children, which is wrong when a parent has an extra child. On the *Kenali Minit* clock page this showed "1 minit" and "1 minute" on top of each other, and on the Tahap 1 hub it left "SEBUT DAN TULIS WAKTU" showing in English mode. It now prefers children named `BM` and `BI` (only those two places were affected).

**Verified in the editor.** The Listen button renders in the lesson header (portrait capture: [button](screenshots/lesson-listen-button.png), [message](screenshots/lesson-listen-message.png)); tapping it in the editor shows "Suara dimainkan pada peranti Android" because no voice is available there. The text sent to the voice was printed and read through for the first pages of *Kenali Minit*, *Sebut dan Tulis Waktu* and *Pengalaman Pranombor* in both languages.

**Not verified.** Actual spoken audio (the editor has no Android voice), voice quality for Malay on real devices, and the pages after the first few of each lesson. Content quirks the voice reads out as written: in *Sebut dan Tulis Waktu* page 3 the caption says 12:00 while the picture and speech bubble say 10 o'clock; on the last *Kenali Minit* page the answer list does not match the tests (noted earlier).

## Bundled lesson recordings (offline voice-over) (2026-11-04)

All 108 lesson pages now have editable Malay and English narration and bundled offline recordings: 216 scripts and 216 unique MP3s across ten lessons. The regenerated sources total 29.3 minutes and 10.55 MB. Spoken equations use complete sentences, area and volume use square and cubic units, and picture-based pages have their own instructions. Exercise narration asks the question without supplying its answer.

The recordings use Microsoft neural ms-MY Yasmin and en-GB Sonia voices at an 8% slower rate. Generation sends the lesson text to Microsoft's online speech service through `edge-tts`; playing the bundled files sends nothing. Source MP3 size is not the installed app size: Unity imports these assets as mono Vorbis.

### Edit a page's narration

Open its scene under `Assets/Scenes/Lessons/`, find the numbered page under `CheckPages`, and edit the Malay or English field on its **LessonNarration** component. These serialized scene fields are the authoritative runtime text. For example, open `Assets/Scenes/Lessons/Prasekolah/1. PengalamanPranombor.unity`, select page `1`, and change its English text from ?Look at the shirts. Find the two matching shirts.? to ?Look carefully at the shirts. Find the two matching shirts.? Save the scene.

Enter Play mode, select English, and tap Listen. Until a matching recording is generated, the old recording must be rejected. Android falls back to its installed voice; the desktop editor shows the platform guidance message. Regenerate and assign the audio with the steps below, then Listen should play the revised bundled recording. The original strings and playback controls were verified; this example's alternative sentence was not sent for generation.

`LessonVoice.BuildSpeech()` uses the current page's authored text, normalizes whitespace, and falls back to visible-label extraction only if that language field is empty. `LessonVoice.Hash()` fingerprints the text. `LessonVoice.clips` links recordings by page name and language; sibling position is deliberately ignored because navigation reorders pages. A recording plays only when its stored hash matches the current narration.

Changing a visual label does not change authored speech. Keep both language fields and visible captions consistent when editing teaching content. Picture text remains an image asset; edit its available source or replace the image rather than expecting text extraction to read it.

### Regenerate recordings

Run these steps from the project root with Unity out of Play mode:

1. Save scene edits, then run **Fun Math > UI > 17 - Export authored lesson narration**. This validates every page and writes `docs/tutorial/lesson-voice-script.tsv`. It also refreshes the bulk-edit file `lesson-voice-overrides.tsv` from the scene fields.
2. For a bulk edit, change the `spoken text` column in `lesson-voice-overrides.tsv`, retaining scene names, page names and `ms`/`en` language keys. Run **16 - Import authored lesson narration**, then **17 - Export authored lesson narration**. Import writes scene components; the generator does not read the bulk-edit file directly. Export before bulk editing, because exporting again replaces that file with the scene contents.
3. Run `python Tools/generate_lesson_voice.py`. It needs Python, the existing `edge-tts` package, and internet. The generator validates fingerprints and duplicate page/language keys, then regenerates clips whose text, voice or rate changed. `--force` regenerates all clips. Unused recordings are pruned only after successful generation; failures preserve the old manifest and unused recordings.
4. Run **Fun Math > UI > 15 - Assign lesson voice clips**. This imports audio and saves each lesson's `LessonVoice.clips` references. Do not manually edit hash filenames or the generated `lesson-voice-manifest.json` cache.
5. Enter Play mode and run **Fun Math > UI > Review > Verify lesson narration (Play mode)**. Wait until `FunMath.Editor.LessonNarrationVerification.Status` reports Passed, then exit Play mode. The status and per-page results are written to `Temp/narration-audit/`.

The runtime/editor sources are `Assets/UI/Runtime/LessonNarration.cs`, `Assets/UI/Runtime/LessonVoice.cs`, `Assets/UI/Editor/LessonNarrationEditor.cs`, `Assets/UI/Editor/LessonVoiceClipBuilder.cs`, and `Assets/UI/Editor/LessonNarrationVerification.cs`. Voice names, rate, concurrency and retry settings are near the top of `Tools/generate_lesson_voice.py`. After changing voice or rate, run generation and assignment again. Run the generator safety tests with `python -m unittest discover -s Tools/tests -v`.

### Verification and remaining checks

Unity compilation passed. All 216 page/language combinations matched their exported text, fingerprint and assigned recording in Play mode, and rejected altered text. Actual AudioSource playback started in both languages, stopped on a second tap, language change, voice-off, page change and component disable, and followed master/effects volume changes. The five generator safety tests passed. FFmpeg decoded all 216 current MP3s without errors; there were no extra MP3s or partial downloads.

The orientation review captured all 432 language/orientation views, with 432 matching recordings and no TMP overflow flags. Contact sheets of nine corrected clock, shape and area pages were inspected in both languages and orientations. The area page 8 explanation had two obsolete inline shape symbols overlapping its new complete sentence; their Image components are now disabled, with the editable objects retained. The [final four-view check](screenshots/area-explanation-corrected.jpg) was visually inspected after the fix.

Saved evidence: [orientation checks](narration-orientation-verification.tsv), [runtime recording checks](narration-runtime-verification.tsv), [audio decoding checks](narration-audio-verification.tsv), [caption corrections](narration-caption-corrections.tsv), and the [review outcome](narration-review.md). These are verification reports; edit the scene components and export rather than editing reports. The earlier `narration-review.tsv` records the superseded scripts and hashes.

A fluent Malay speaker and an English speaker still need to listen to the new set for pronunciation, phrasing and teaching accuracy. Android offline playback, native speech fallback, background music balance and device performance remain untested. The scene-loading run also produced existing `Sounds` warnings about `DontDestroyOnLoad` on non-root objects; no narration failure was observed.

## Repeat the orientation review (2026-11-04)

Lesson artwork now fits its complete composition, including illustrations outside the original card. `Assets/UI/Runtime/LessonPageLayout.cs` exposes **Design Size** and **Design Center**. Pages with worked examples or reference pictures in their answer groups reserve separate space for those examples, their captions, and navigation. Small diagram labels can shrink within their authored text boxes; the navigation and heading text sizes are separate.

To add or move an illustration, open its lesson scene under `Assets/Scenes/Lessons/`, select the numbered page under the `CheckPages` object, and edit the objects under **DiagramArea > LessonContent**. From the **LessonPageLayout** component menu, choose **Recalculate artwork bounds**, then save the scene. For example, move the explorer on a number-sequence page farther right, recalculate, and preview both orientations: the complete composition should fit without covering the caption. The bounds include inactive language branches, so check both languages and keep unused objects outside this hierarchy.

The balance game in `Assets/Scenes/Games/AddingGae/Level1.unity` now uses **Canvas > Canvas Scaler > Scale With Screen Size**, reference **1440 × 1080**, with **Expand**. This fits its existing baskets and balls into smaller Game views. The original gameplay hierarchy remains editable; change its artwork on `Canvas/body` and its header on `GameShell`.

The [before-and-after comparison](orientation-review.html) uses the saved Game view screenshots. Its HTML source is editable; its image paths point into `screenshots/`.

Two editor commands provide repeatable screenshots without saving scene changes:

- **Fun Math > UI > Review > Capture all lesson pages (Play mode)**: `Assets/UI/Editor/LessonReview.cs` captures all 108 pages in Malay and English at 960 × 540 and 540 × 960. Outputs go to `Temp/lesson-review/`: screenshots, `results.tsv`, and `voice-script.tsv`. Results include overflow flags and whether each page still matches its recording.
- **Fun Math > UI > Review > Capture other screens (Play mode)**: `Assets/UI/Editor/ScreenReview.cs` captures the other enabled scenes in both languages and orientations under `Temp/screen-review/`. Loading is excluded because it redirects automatically. Game board previews hide modal overlays; they check geometry and do not play through a level.

Run one review at a time. Wait for its `Status` property to report Completed, then exit Play mode. The commands restore the selected language. Review overflow flags against screenshots: covered legacy text and mathematical superscripts can produce flags without visible clipping. Screenshots and temporary review data are generated outputs; edit the scenes and scripts to make changes.

The completed review captured 432 lesson views and 212 views across the other 50 scenes. All 432 lesson views matched a bundled recording; the final targeted instruction check reported no text overflow in either language or orientation. The saved [lesson results](orientation-lesson-review.tsv) include recording checks; the [screen results](orientation-screen-review.tsv) include initial screens and board previews. English contact sheets were inspected throughout, with focused checks of Malay layouts and flagged text. The balance board was inspected in [portrait](screenshots/balance-fit-portrait.png) and [landscape](screenshots/balance-fit-landscape.png); a scripted 20 × 10 pixel drag moved a ball by exactly that screen distance after scaling. Worked examples now remain separate from captions ([area example](screenshots/area-reference-portrait.png), [volume example](screenshots/volume-formula-portrait.png)). No device play-through was performed in this pass.

After changing this tutorial, run `python docs/tutorial/export_tutorial.py` from the project root to refresh its HTML companion. Device touch input, safe areas on physical phones, AR tracking with printed cards, voice quality, and device performance still need manual checks. The narration correction pass addressed the minutes answer key, clock-caption mismatch and related maths wording; fluent-speaker listening and lesson-author acceptance remain manual checks.


## Audio persistence and final acceptance (2026-11-05)

`Assets/DontDestroyBG.cs` keeps one `Sounds` hierarchy alive between scenes. Its `Awake` detaches the winning audio object from its scene parent before calling `DontDestroyOnLoad`. Duplicates are destroyed with an immediate return; destroying the active owner clears the static reference. Scene assets and audio settings are unchanged.

To maintain it, keep persistent music sources under the object carrying `DontDestroyBG`. Keep scene-specific objects outside that hierarchy: its children survive scene changes. Existing source clips and mixer routing remain editable on the child AudioSources.

Verification: Unity compiled without errors. In Play mode, the area lesson's audio owner became a root in the persistent scene and retained both children. Instantiating a duplicate preserved the original singleton and removed the duplicate on the next frame. A separate run loaded `Assets/Scenes/AfterLogin/MainMenu.unity` from the area lesson and retained exactly one audio owner with two children; the final run produced no console warnings or errors. An initial probe used an incorrect MainMenu path and failed to load; the corrected-path run passed. Unity was left out of Play mode. Audible music continuity and device playback were not checked.

Remaining acceptance work:

- [ ] Have fluent Malay and English speakers listen to all current recordings for pronunciation, pacing, units and teaching accuracy. Use the narration reports above to locate each page.
- [ ] Have the lesson author approve corrected maths captions, diagrams and answer keys, including text embedded in artwork.
- [ ] On Android, test offline recordings, repeat/stop, page and language changes, voice-off, native speech fallback and music balance.
- [ ] Test touch controls, rotation, physical safe areas, save/progress persistence and complete representative quiz/game routes on the target phone.
- [ ] Test AR entry, printed-card tracking and exit on a camera-equipped Android device.
- [ ] Measure frame rate, memory and loading time on the target Android device against the plan's acceptance criteria.

No Android device was listed by `adb devices -l` on 2026-11-05; T3 device access reported disabled. These manual and device checks remain open. Earlier continuation entries describe historical milestones; use this checklist and the verification sections above for current acceptance limits.


## Illustrated playground backgrounds (2026-11-05)

The MainMenu now puts Learning, Games, Quiz and Badges in a 2 x 2 activity area. Add profile remains in the footer; More opens Appreciation and Credits. Original navigation callbacks and availability mirrors remain in place. Shared scenery is also applied to the other enabled non-AR screens. AR keeps its camera view and existing overlays.

Edit these owned sources:

- `Assets/UI/Prefabs/Backgrounds/MathPlayground.prefab`: sky, hills, clouds, corner gardens and the optional sun. Open Prefab Mode to move or replace layers. Decorative Images have raycasts off; the sun has its own stable hit target.
- `Assets/UI/Resources/Backgrounds/Explore.asset`, `Focus.asset`, `Still.asset`: palette, cloud travel/period, corner sway and reaction cooldown. Hubs use Explore, reading/forms use Focus, and games/quizzes use Still. Change the assigned profile on a scene's `AnimatedBackground` to choose its mode.
- `Assets/UI/Runtime/AnimatedBackground.cs`: scenery-only movement and blockers. It pauses for reduced motion, lost focus, app pause, modal dialogs and narration. It restores rest poses and unsubscribes on disable. Inactive corner Animators are skipped.
- `Assets/UI/Runtime/ModalInputOwner.cs` and `ModalInputScope.cs`: scene-local dialog stack. The owner suspends content CanvasGroups and lower dialogs, confines selection to the top dialog and restores opener focus. Keep modal roots outside the owner's content groups.
- `Assets/UI/Runtime/HubLayout.cs` and `AdaptiveMenu.cs`: responsive activity placement and spacing. Edit a button's `BilingualLabel` for its two captions.
- `Assets/UI/Art/Backgrounds/generate_art.py` and its README: editable originals and export instructions for the eight project-authored illustration sprites. PNGs are generated imports; retain their `.meta` files when re-exporting.
- `Assets/UI/Animation/Backgrounds/Playground.controller` and the three clips: authored corner scale loop, driven through the cached integer `Mode` parameter.

To slow the clouds, select Explore and change `Cloud Period` from 24 to 30 seconds. Keep `Cloud Travel` at 32. The clouds should cover the same bounded distance more slowly when the app is focused, voice is idle and no dialog is open. Enable Reduce Motion to see the authored static composition. An Editor check changed the cloud period to 30 and the grid gap to 32, saved and reopened the scene, reapplied the patch, and confirmed both values survived. Original values of 24 were restored afterward. This checks serialized persistence; an audible or device workflow was not exercised.

To increase activity spacing, select `AdventureShell/SafeArea/Activities` and change `AdaptiveMenu.Gap` from 24 to 32. Grid cells shrink within the same activity region. `HubLayout` owns that region's anchors; edit its source for different portrait/landscape placement.

Use `Fun Math > UI > Playground > 1 - Create missing shared scenery assets` to repair missing shared assets, `2 - Patch selected scene` to update one saved scene, and `3 - Patch remaining project screens` for enabled non-AR scenes. Do not run the older whole-hub rebuild to maintain this revision. Save the scene first and preserve a snapshot before migration.

The targeted patch preserves existing backdrop profile assignments and existing profile asset values. It owns scenery insertion, known background-fill visibility, modal group/scope assignments, the four MainMenu activity captions/art/anchors, supporting-route placement, and hub owl visuals. On MainMenu it restores two grid columns in both orientations; it does not overwrite `AdaptiveMenu.Gap`. It may overwrite activity captions, anchors and assigned navigation art, so reapply customizations to those properties after running it. Ordinary prefab/profile edits do not require rerunning the builder.

To repeat the MainMenu checks, open `Assets/Scenes/AfterLogin/MainMenu.unity`, enter Play mode and run `Fun Math > UI > Playground > 4 - Verify MainMenu (Play mode)`. Wait for `FunMath.Editor.PlaygroundReview.Status` to finish before exiting Play mode. The review temporarily changes language, voice, reduced motion and Game-view resolution. It restores preferences and background-running state, but leaves the last Game-view resolution selected. It invokes controls programmatically and simulates foreground focus during motion observation; it does not certify physical keyboard/touch/gamepad input.

The completed MainMenu run passed 67 checks and saved 12 captures under `review/playground/`, covering 540 x 960, 960 x 540, 360 x 800, 800 x 360, 768 x 1024 and 1024 x 768 in Malay and English. Checks include four primary activities, seven route mirrors, nested dialog selection/input ownership, restored opener focus, immediate reduced motion, static sun feedback, voice-off cancellation, text overflow and focus blockers. The 30-second observation measured cloud travel in a simulated foreground; it is not a visual motion recording. Portrait English and wide-landscape Malay captures were visually inspected. Repeat patching preserved all 283 MainMenu Transform identities.

The rollout migration report is `review/playground/migration.tsv`; each scene passed its callback and Collider2D geometry comparison. The first continuation run exposed missing content groups, fixed by the builder's Unity object-existence check. An early sweep also exposed inactive Animator updates; the presenter now skips them. These failures preceded the final verification runs.

The lesson-path and collection layouts were added in the continuation below. Android speech bridge compilation/playback, physical safe areas, complete route play-throughs, performance measurements and child usability remain manual acceptance work.


Final continuation evidence: [screen results](review/playground/screen-review.tsv) contain 212 captures, with no new overflow strings compared with the preceding orientation report. [Lesson results](review/playground/lesson-review.tsv) contain 432 captures across 108 pages: no overflow flags and no missing matching recordings. English landscape contact sheets for screens and lessons were inspected at overview scale; this is not a fresh full-size visual inspection of every capture. AR initialization logs were present during the mixed-screen sweep; printed-card tracking remains unverified.

The refreshed `ui-scene-audit.tsv` covers 61 scenes with zero missing scripts and zero missing TMP fonts. It still lists 166 missing persistent action targets in legacy controls, compared with 181 in the saved preceding audit; no scene's count increased. This is an existing incomplete wiring inventory, not a clean audit result. The targeted migration compared the existing callbacks before and after each patch and retained them. Investigate those legacy callbacks separately before release, using the audit's scene paths. No new missing-target entries were introduced by this pass.

The MainMenu review source is `Assets/UI/Editor/PlaygroundReview.cs`. Its first failure revealed a missing CanvasGroup; later failures revealed focus assumptions in the test harness. The corrected harness explicitly maintains simulated foreground state during the motion sample and restores actual focus state on exit. `review/playground/customization-check.txt` records the save/reload test. Unity is left out of Play mode on MainMenu after the final checks.

The final MainMenu regression after the inactive-Animator fix again passed all 67 checks. Its isolated Play-mode run produced no console warnings or errors. The HTML companion was regenerated, the new maintenance section was previewed, and tutorial file/anchor links were checked. [Changed-file hashes](review/playground/changed-files.json) identify this revision against the original snapshot; screenshots and reports are generated review outputs.


## Maintain lesson paths and collection screens (2026-11-05)

The three lesson hubs now arrange their existing buttons along a numbered trail: a staggered column in portrait and a compact winding row/grid in wide landscape. Three-node hubs use wide signs in a vertical trail on 4:3 tablets so long translated captions fit. Numbers show the displayed order. The original controls still determine availability; this change introduces no prerequisites, stars or save keys. The badge screen separates collection artwork from its six stickers, with an earned caption or an exact progress count. Credits and appreciation reserve their own illustration and scrolling reading area.

Open `Assets/Scenes/Lessons/PrasekolahLesson.unity`, `Tahap1Lesson.unity` or `Tahap2Lesson.unity` in the same folder. Under `AdventureShell/SafeArea/Activities`, `LessonPathLayout` owns the positions of the existing buttons. Its `Nodes` array defines trail order; `Stepping Stones` contains decorative, non-interactive Images. The old `GridLayoutGroup` and `AdaptiveMenu` remain disabled on these three containers. Leave them disabled while using the trail. Keep every button's `NavigationState.source` and persistent actions intact.

To change spacing, set `LessonPathLayout > Gap` from 24 to 32, save and reopen the scene. The trail should show wider spaces between nodes. `Portrait Stagger` controls the sideways offset as a fraction of container width; zero produces a straight column. `Edge Padding` reserves room for the existing focus scale. The layout also places each stop's `ActivityArt`, `Label` and `PathNumber`: tall stops show art above text, wide stops show art beside text. Change those two arrangements in `LessonPathLayout.cs` when changing the visual structure. These settings and the node array are scene-owned. The targeted patch skips an existing `LessonPathLayout`, preserving manual spacing, order and button art. To add a lesson, first author its original route and availability through the project's existing controller, then add its view to `Nodes` and an optional connector to `Stepping Stones`.

Badge editing starts at `Assets/Scenes/Badges/BadgesPage.unity`. `SpecialtyShell/SafeArea` contains `BadgeShelf` and `RewardScreenLayout`; the latter places `CollectionIllustration` and `BadgeCards` in separate regions. `BadgeCards/AdaptiveMenu` uses two portrait columns and three landscape columns. In `BadgeShelf > Entries`, change the bilingual name/hint or replace an `Artwork` Image's sprite. Keep `Progress Keys` unchanged unless the matching achievement producer changes too. `Earned Tint` and `Exploring Tint` are editable in the Inspector. Captions and counts communicate state alongside the tint.

`BadgeDetails` lives beside `SafeArea`, under `SpecialtyShell`, so its modal owner can suspend the collection controls without suspending the dialog. Keep its `ModalInputScope` and close button. Back closes details and restores selection to the originating sticker. `BadgeShelf` delegates Escape to the modal owner in migrated scenes.

For credits and appreciation, open `Assets/Scenes/CreditPage.unity` or `Assets/Scenes/PenghargaanPage.unity`. Edit the existing bilingual labels inside `SpecialtyShell/SafeArea/ReadingArea/Content`; replace `CollectionIllustration` through its Image component. `RewardScreenLayout` controls these two outer regions in both orientations. Their internal text and scrolling remain ordinary uGUI content.

The authoritative layout sources are `Assets/UI/Runtime/LessonPathLayout.cs`, `Assets/UI/Runtime/RewardScreenLayout.cs` and `Assets/UI/Runtime/BadgeShelf.cs`. The migration lives in `Assets/UI/Editor/PlaygroundUIBuilder.cs`. Use `Fun Math > UI > Playground > 2 - Patch selected scene` on a saved scene. Its first application supplies the family layout; subsequent applications retain existing family components and their references. The patch still owns shared modal wiring and background integration. Do not use the older whole-hub rebuild for maintenance.

To repeat the focused check, enter Play mode and run `Fun Math > UI > Playground > 5 - Verify paths and rewards (Play mode)`. `PathRewardReview.Status` reports progress; [checks and captures](review/playground/path-rewards/checks.tsv) are written under `docs/tutorial/review/playground/path-rewards/`. The review loads these six scenes, captures BM/EN at six sizes, checks trail geometry/availability and text, opens badge details and checks Back/focus. Earned and unfinished badge states use temporary in-memory key arrays, restored afterward; it never writes player IDs or achievement keys. The language preference and background setting are restored on completion or stopping Play mode. The review returns to MainMenu on normal completion. Regenerate this HTML companion with `python docs/tutorial/export_tutorial.py` after editing the Markdown.


Verification for this continuation: the final six-scene Play-mode review passed all 1,142 checks and produced 84 captures across BM/EN and six sizes. No console warnings or errors occurred in that run. Programmatic badge open/Back and focus restoration passed, as did earned/unfinished presentation with temporary in-memory key arrays. Inspected representative phone/tablet, portrait/landscape and dialog captures, including the compact-tablet Malay captions. Reapplying the patch retained all 1,337 Transform identities; the Gap 24-to-32 customization survived save/reload plus patch and was restored to 24. [Migration evidence](review/playground/path-reward-migration.tsv) records those checks. The 61-scene audit remains at zero missing scripts/fonts and 166 existing missing action targets, with no per-scene increases. Unity is out of Play mode on MainMenu.

The tutorial HTML section was visually previewed; tutorial file/anchor links passed. The capture review does not establish physical touch/gamepad operation, real-device safe areas, native Android speech, measured frame budgets, AR tracking or child usability. Scrolling credits and every original route still need hands-on acceptance. Earlier failed reports describe issues corrected during this continuation; `checks.tsv` is the final result. [Browse the screen captures](review/playground/path-rewards/gallery.html) using its screen/language controls.


## Maintain repaired legacy actions (2026-11-05)

The final-page Check/Semak button opens the existing submit confirmation and leaves the question visible behind it. Cancelling returns to that page; confirming uses the original scoring and result actions. A quiz timeout uses explicit references to the existing results and TimeUp panels, so adding background siblings cannot redirect the timeout into the submit confirmation.

`Assets/TimerController.cs` owns countdown and timeout behavior. On each quiz scene's TimerController, `Prompted` references `Canvas/Prompted (2)` (results), and `Timeout Gameobject` references `Canvas/TimeUp`. The runtime fallback looks up those names only when a reference is absent. If you rename the panels, keep the Inspector references assigned. Do not restore child-index lookups. Scoring and saved progress remain in the existing `QuizController`, `LoadScore` and `WriteKeys` components.

`Assets/UI/Editor/LegacyActionRepair.cs` contains the narrow repair rules and produces a decision report for each recognized missing action. Exit Play mode, save the current scene and preserve a file snapshot before running `Fun Math > UI > Wiring > 1 - Inspect known legacy actions`. It visits scenes incrementally and reports through `LegacyActionRepair.Status`. Review `docs/tutorial/review/wiring/inspect-decisions.tsv`; `UNRESOLVED` means the tool could not establish a safe target. Run `2 - Repair known legacy actions` to apply recognized repairs. The repair report and per-scene before/after counts use the `repair-` prefix. Keep a copy before another run because these reports describe the latest run.

Audio/Profile dropdowns bind to a unique existing `AudioPanelParent` or `ProfilePanelParent` AudioController. Play/Stop callbacks bind only to a unique existing VideoPlayer named `video`. The old settings prefab has scene-dependent action slots; its reusable asset cannot reference a scene's panel. Scene-instance overrides hold those assignments. After adding that prefab to another scene, assign each panel target manually or run the inspection/repair command when its recognized named owners are present. The command does not create missing owners or guess between duplicates.

Removed actions are limited to empty callback slots and dead callbacks on retained, superseded controls: hidden WhackAMole prototype execute buttons pointing at `AddingGame`, and copied quiz navigation under disabled credits/appreciation canvases. Their GameObjects are retained. The last numeric question page is detected from the scene (10 or 15). Its retained `nextButton` becomes a visible Check/Semak action and opens the existing submit confirmation. Its new `SubmitLabel/BilingualLabel` is editable in the Inspector. Earlier-page arrows and valid result/scoring callbacks are preserved. Other removed game copies are the hidden HowToPlay Yes button's dead quit/open actions in the balance scene and the rocket scene's copied QuizController scoring call; its existing EndPanel actions remain.

For a representative customization, rename the quiz results panel in the Editor while leaving `TimerController > Prompted` assigned to that same object. Save/reopen and force a short countdown in Play mode. The timeout should still open that results panel and TimeUp, rather than the submit confirmation. The rename workflow is a documented manual check; it is not part of the automated review.

To exercise the repaired flow, enter Play mode and choose `Fun Math > UI > Wiring > 3 - Verify quiz actions (Play mode)`. `Assets/UI/Editor/WiringReview.cs` runs final-page open/cancel/confirm, one-correct-answer scoring, timeout and Audio/Profile callbacks in all three quizzes. It preserves the player's identity and restores the relevant integer score, achievement, menu and language preferences after completion or stopping Play mode. The review returns to MainMenu on normal completion. Read `docs/tutorial/review/wiring/play-checks.tsv`; its captures show actual results panels. AR reference checks establish wiring only; card tracking and native-device video playback still require a device. Run one review at a time.

## Full portrait and landscape sweep (2026-11-05)

A Play-mode script loaded all 61 scenes at 1080x1920 (portrait) and 1920x1080 (landscape) in Malay, dismissed game intros, waited for animations and captured the Game view; the captures were reviewed as contact sheets.

**Fixed**
- **Algorithm games, portrait:** the Run (*Jalan*) and Clear (*Padam*) buttons covered the bottom 140 px of the grey drop box, where longer routes need slots. `GameChromeLayout` now places them in the strip between the arrow tray and the box. Checked on levels 1 and 10 ([sheet](screenshots/portrait-fixes.png)); the other eight algorithm levels share the same layout but were not re-captured. Landscape is unchanged.
- **Get-started screen:** the Malay button read "LET'S START"; it now reads **JOM MULA** (English unchanged).
- **Preschool lessons:** 21 banners were pure red (1,0,0) with dark text, which was hard to read. They are now the theme's orange (*Pengalaman pranombor* 8, *Memahami siri nombor* 10, *Operasi nombor* 3).

**Looked fine in both orientations:** main menu, all hubs, badges, credits, appreciation, profile screens, loading, all 11 AR screens, all WhackAMole levels, the balance game, the rocket game, the 10 lesson scenes' first pages.

**Left as they are (decisions for you)**
- **Quiz screens:** the original black X and gear icons overlapped at the top-left. Restyled afterwards, see the next section.
- **Profile picker (landscape):** the fourth profile card is partly hidden under the "Pemain baru" bar. It appears to be a scrolling list, so I did not change it; worth a quick touch test.
- **Lesson navigation labels** (*Seterusnya*, *Keluar*, *Ibu bapa*) use full-strength dark text but a thin font weight, so they look light at small sizes.
- Portrait for the quizzes was captured with the intro dialog open, so the answer area itself was not inspected in portrait.

## Quiz header restyle (2026-11-05)

The quiz screens' black X and overlapping black gear are now the same cream pills the lessons use: **Keluar / Exit** and **Ibu bapa / Parents**, side by side, then the title and timer. **Fun Math > UI > 16 - Restyle quiz header** applies it to the three quiz scenes (re-runnable). Their actions are unchanged: Exit still opens the original confirmation, and Parents opens the existing parent gate (a short sum) before the audio and profile settings. `QuizScreenLayout` has a new `settings` field and lays out the header row for portrait and landscape; the gear's old copy sits in a duplicate nested `settingDrop` object, which is switched off (not deleted).

Verified in the editor ([all three quizzes, both orientations](screenshots/quiz-header-all.png); [portrait detail](screenshots/quiz-header-portrait.png)): the header no longer overlaps, and tapping Parents opened the parent gate ([capture](screenshots/quiz-header-dropdown.png)). Not verified: tapping Exit through to leaving the quiz, answering the parent-gate sum, English text on the new pills, and touch on a device.
