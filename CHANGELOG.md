# Changelog

## Submission documentation — 2026-10-12

- Expanded the README with download/extraction steps, controls, gameplay guidance, FAQ, troubleshooting, and sharing/submission instructions.
- Added a professor-facing synopsis of the design, creation, implementation, build process, execution, AI coding assistance, and recorded verification.
- Included the full README and a Notepad-friendly professor synopsis beside the packaged executable, with working local documentation links.
- Published an updated complete submission ZIP while preserving the tested 1.2.0 game and earlier release archives.

## Complete labeled package — 2026-10-12

- Put NeonRelay.exe at the top level of a complete Windows/source hand-in folder.
- Added numbered guide, source/build, and verification folders, plus a contents note in every directory.
- Included the full clean Unity project, build tools, screenshots, reports, folder map, and file hashes.
- Added a reproducible packaging script that verifies runtime, source, and archive integrity.
- Preserved the tested Neon Relay 1.2.0 game and its required runtime folder names.

## 1.2.0 — Auto-run edition

- Replaced manual movement and connector puzzles with forward auto-run and jump timing.
- Rebuilt all three stages as scrolling obstacle courses with progressive speed and gap difficulty.
- Added automatic stage advancement, a finish progress bar, and short preparation countdowns.
- Made bonus cells optional; results report actual collected totals.
- Added click-to-jump alongside keyboard double jump and variable jump height.
- Kept midpoint checkpoints, unlimited retries, pause, help, sound, reduced effects, and fullscreen.
- Added a separate auto-run best-time record and correct main-scene startup from Unity's Play button.
- Reworked simulation and executable tests to traverse every course using jump inputs only.
## 1.1.0 — 2026-10-11

- Added distinct rooftop, factory, and reactor scenery with a city that lights up as sectors are restored.
- Added bounded spark bursts for jumps, landings, energy cells, checkpoint activation, and retries.
- Added live objectives, a first-jump prompt, laser guidance, and laser cycle progress indicators.
- Replaced direct circuit instructions with three increasingly involved logic clues; optional hints reveal the solution without penalty.
- Added a field manual with control reference, sound, reduced decorative effects, and fullscreen options. H opens help; F11 toggles fullscreen.
- Added a title-screen quit button and an end-of-run sector time/retry breakdown.
- Expanded gameplay regression coverage and executable UI checks, including all sector transitions, results, and replay.
- Added the submission guide, updated screenshots, and reproducible verification reports.

## 1.0.0 — 2026-10-07

- Initial three-sector Windows release with double jumps, cells, checkpoints, routing puzzles, saws, moving platforms, spikes, and timed lasers.
