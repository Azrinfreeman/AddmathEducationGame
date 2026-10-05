# FunMath demo verification

These checks are pending. Source review used base commit `ece726d3cea8f163864c0e523224cca10d3e2202`. The documentation branch does not change scripts, scenes, packages, keys, or project settings.

## Prepare the project

- [ ] Obtain the complete project and actual Git LFS package archive. Confirm the local Vuforia dependency resolves successfully.
- [ ] Import with Unity 2021.3.16f1 and record compilation/import errors, missing resources, and any migration prompts before accepting changes.
- [ ] Verify all 61 enabled scene paths and the configured UI hierarchies, animation states, tags, audio, videos, and Inspector references.
- [ ] Review Vuforia license ownership and terms locally without copying key values into public documentation or screenshots.
- [ ] Prepare matching image-target artwork, camera permissions, lighting, and a compatible test device. Verify each AR scene separately from non-AR features.

## Verify the learning and game flow

- [ ] Test first-time and returning-player routing on a disposable app-data profile.
- [ ] Create two fictional player profiles and switch between them; verify the selected player ID, name, progress, and badges.
- [ ] Test blank/non-numeric age input and error recovery. Profile creation currently parses the age text directly as an integer.
- [ ] Visit preschool, time-reading, and geometry lessons, checking page navigation and completion flags.
- [ ] Complete each quiz group with correct/incorrect answers, timer expiry, repeat submission, and scene revisit. Score collection increments the existing score, so verify reset behavior.
- [ ] Exercise the directional-command puzzle across its configured levels, including invalid sequences, replay, and reset.
- [ ] Exercise the other game groups, including dragging, timed play, score feedback, progression, and completion panels.
- [ ] Check shared quiz-score values versus player-specific completion keys after switching profiles and restarting the app.
- [ ] Verify Malay/English object switching in representative scenes; check missing objects and layout behavior rather than assuming full translation coverage.
- [ ] Verify video playback, audio controls, camera permission denial, target acquisition/loss, and navigation back from AR.
- [ ] Build and run on the intended target, recording the editor, package versions, device, and observed limitations.

## Prepare portfolio evidence

- [ ] Preserve the existing developer/education contributor credits and review artwork, audio/video, SDK, and marker redistribution terms.
- [ ] Have the lesson/quiz content reviewed before making curriculum or learning-effectiveness claims.
- [ ] Capture screenshots and a short walkthrough from the actual app with fictional profile data and no key values visible.

No Unity runtime tests, camera sessions, or builds were executed during this documentation pass.
