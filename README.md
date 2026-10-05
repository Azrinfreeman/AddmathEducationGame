# FunMath — Interactive Mathematics Learning

**A Unity educational application combining mathematics lessons, quizzes, minigames, and image-target AR activities.**

FunMath includes preschool number/addition content, time-reading lessons, and perimeter, area, and volume activities. It connects local player profiles with lesson completion, quiz results, game progression, and achievement badges. The repository remains named `AddmathEducationGame`; its configured Unity product name is `FunMathProject`.

## Features in the source

- **Lessons and quizzes:** content is organized into `Prasekolah`, `Tahap1`, and `Tahap2` scene groups, with page/quiz navigation and score display.
- **Different minigame interactions:** a directional-command puzzle moves a character through a sequence, while other games use answer buttons, draggable UI objects, physics, and timers.
- **Image-target AR:** eleven enabled AR scenes correspond to addition, time, perimeter, area, and volume targets in the included Vuforia database.
- **Local profiles:** name, gender, age, and a numeric player ID are stored with Unity `PlayerPrefs` and used by profile-selection UI.
- **Progress and achievements:** completion flags and high-score conditions drive level and badge views.
- **Language controls:** scripts switch between `BM` and `BI` objects for Malay/English presentation where those objects are configured.
- **Audio and video controls:** lesson/video playback and game sound controls are included.

These are implemented source features, not recorded passing device tests. The enabled build list contains **61 scenes**: 14 under `Lessons`, four under `Quizzes`, 22 under `Games`, 11 under `AR`, and ten other navigation/profile/credits scenes.

## Learning flow

1. The loading scene routes a first-time session to `GetStarted` and a returning session to `MainMenu`.
2. Create or select a local player profile.
3. Explore lessons, complete a quiz, or enter a minigame through the configured menus.
4. Review scores, progress, and badges.
5. For an AR activity, use an appropriate camera-equipped device and the corresponding image target.

Routes and object wiring must be verified in Unity before recording a portfolio walkthrough. This source review does not establish recognition quality, curriculum correctness, or learning effectiveness.

## Implementation map

| Area | Source |
| --- | --- |
| First-time routing and scene navigation | [LoadingStart](Assets/LoadingStart.cs) and [SceneController](Assets/SceneController.cs) |
| Profile creation and selection | [SaveInformation](Assets/SaveInformation.cs) and [AccountController](Assets/AccountController.cs) |
| Lesson and quiz navigation | [LessonController](Assets/LessonController.cs), [CheckPages](Assets/CheckPages.cs), and [QuizController](Assets/QuizController.cs) |
| Quiz score display | [LoadScore](Assets/LoadScore.cs) |
| Directional-command puzzle | [AlgoController](Assets/AlgoController.cs) and [stickmanProperty](Assets/stickmanProperty.cs) |
| Preschool game flow | [GamePraController](Assets/GamePraController.cs) |
| Game completion, dragging, and timers | [Game1Controller](Assets/Game1Controller.cs), [DragScript](Assets/DragScript.cs), and [TimerController](Assets/TimerController.cs) |
| Progress flags and badges | [WriteKeys](Assets/WriteKeys.cs) and [BadgeController](Assets/BadgeController.cs) |
| Malay/English object switching | [LanguageController](Assets/LanguageController.cs) and [ChangeLanguage](Assets/ChangeLanguage.cs) |
| Lesson video UI | [VideoController](Assets/VideoController.cs) |

## Open the project

The checked-in configuration uses **Unity 2021.3.16f1**, **Universal Render Pipeline 12.1.8**, **TextMesh Pro 3.0.6**, **Vuforia Engine 10.12.3**, and **ARCore XR Plugin 4.2.7**. The configured application version is **1.0.0**. These are repository versions, not a claim of current platform or store compatibility.

1. Clone the complete repository with Git LFS support. Retain `Assets`, `Packages`, `ProjectSettings`, and Unity `.meta` files.
2. Retrieve the LFS content before opening Unity:

   ```sh
   git lfs install
   git lfs pull
   ```

3. Confirm `Packages/com.ptc.vuforia.engine-10.12.3.tgz` is the actual package archive rather than a small LFS pointer. [manifest.json](Packages/manifest.json) references that local archive; a source-only checkout or incomplete download is not enough to import the full app.
4. Add the repository root in Unity Hub using **2021.3.16f1**, as recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt). Preserve the initial editor/package versions during the first inspection.
5. Allow Unity to import and resolve [manifest.json](Packages/manifest.json) and [packages-lock.json](Packages/packages-lock.json). The included [Vuforia migration script](Assets/Editor/Migration/AddVuforiaEnginePackage.cs) can present package-installation dialogs; review any proposed changes before accepting them.
6. Review the Vuforia configuration and applicable license ownership/terms before camera or AR testing. Do not copy configured key values into screenshots, logs, or documentation.
7. Open [LoadingScene.unity](Assets/Scenes/Loading/LoadingScene.unity), the first scene in [EditorBuildSettings.asset](ProjectSettings/EditorBuildSettings.asset). Keep the existing scene list and order.

For Android evaluation, use a compatible Unity Android toolchain and camera-equipped target device. The checked-in minimum SDK is **24**, with target SDK selection set to automatic. Import, rendering, camera permission, tracking, and build compatibility remain to be validated on the intended target. No editor upgrade, package migration, application execution, or platform build was performed during this documentation pass.

## AR content and requirements

The [AR scene collection](Assets/Scenes/AR/) references the `FunMathDatabase` image-target database. Its [XML definition](Assets/StreamingAssets/Vuforia/FunMathDatabase.xml) declares eleven targets, including `ApaItuOperasiTambah`, `KenaliMinit`, `Luas`, `Isipadu`, and `perimeter`; the paired `.dat` file is also tracked.

Prepare the matching marker artwork and suitable lighting before a device demo. Merely having the scene and database files does not demonstrate successful target detection. Review the configured Vuforia license in an authorized local setup; this documentation does not replace, validate, or disclose its key values.

## Data and validation status

The inspected application scripts use local `PlayerPrefs` for profiles, progress, and scores, with no application login server or cloud-sync implementation found in those scripts. Local profiles are not secure authenticated accounts. Third-party AR/package behavior is separate and has not been assessed as offline-only.

Progress storage mixes player-specific completion keys with shared values such as `QuizScoreEasy`, `QuizScoreMedium`, and `QuizScoreHard`. Check behavior when switching users; do not assume every score or preference is isolated per player. Use fictional names and ages in portfolio demos, since profile input is also written to the Unity log.

This documentation was checked against source and configuration. Unity import/compilation, all lesson/game flows, device builds, camera permissions, AR tracking, license validity, and educational outcomes remain unverified. See the [demo checklist](docs/DEMO_CHECKLIST.md) for a future validation session.

## Credits and licensing

The existing [credits scene](Assets/Scenes/CreditPage.unity) records the developer and education contributors and is preserved. The repository also includes third-party Unity packages, Vuforia material, artwork, media, and target content whose usage terms need separate review.

No repository-wide license file is included in the inspected tree. Public visibility does not grant reuse or distribution rights to every component. Confirm intended licensing and bundled-content rights before distributing a build or reusing assets.
