# Tickets


T10.1 - Connect UI_DmxSettings to the artnet Receiver so that the correct DMX start channel and universe gets used.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.9 Solve this compile error: Assets\Scripts\UI\UI_DpadNavigationController.cs(76,18): error CS1061: 'Selectable' does not contain a definition for 'OnSubmit' and no accessible extension method 'OnSubmit' accepting a first argument of type 'Selectable' could be found (are you missing a using directive or an assembly reference?)
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Replaced the invalid direct `Selectable.OnSubmit` call with `ExecuteEvents.Execute<ISubmitHandler>` in `UI_DpadNavigationController.SubmitCurrentSelection`, preserving D-pad submit behavior without relying on protected API access.

T9.8 - Create a Selectable object to make the UI_DpadNavigationController work correctly
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_DpadSelectable` (a submit-capable `Selectable`) and hardened `UI_DpadNavigationController` so D-pad traversal skips null/non-interactable entries while preserving wrap behavior.
  - Expanded `UI_DpadNavigationControllerTests` to cover first-valid selection and submit event invocation through `UI_DpadSelectable`.

T9.5 - Create a script that adjusts a quad mesh to use as a corner pin object. The script needs to be able to dynamically change the position of the vertices using artnet input. For each corner we use 2 DMX channels that set the X and Y positions of the corners of the quad.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `CornerPinDmxWarp` with DMX channel pair mapping (X/Y per corner) and EditMode coverage in `CornerPinDmxWarpTests`.
  
T9.6 - Create a system to save the settings in user preferences so those settings are recalled when the app gets started again.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added PlayerPrefs save/load lifecycle handling in `UI_DmxSettings` with regression coverage in `UI_DmxSettingsTests`.
  
T9.7 - Make the UI working with a Android TV remote controller D-Pad
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_DpadNavigationController` plus EditMode coverage in `UI_DpadNavigationControllerTests` to support D-pad traversal and submit behavior.

T9.9 - Next run: execute Unity EditMode suite to validate T9.8 selectable + D-pad navigation updates on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.10 - Next run: execute Unity EditMode suite to validate T9.9 compile-fix behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.3 - Add RGB and dimmer control. Create a RGB controller script that sets the color of the shader similar to how the PatternGenerator sets the patterns.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `RgbDmxController` to map DMX channels 1-4 to shader `_Intensity` + `_Color`, and added EditMode coverage in `OutputComponentsTests`.
  - Blocked locally: Unity EditMode runner is unavailable in this container for execution.

T9.4 - Next run: execute Unity EditMode suite to validate RgbDmxController + output component coverage
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.1 - Create UI to simulate DMX data for use in the Editor
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_DmxEditorSimulator` and EditMode test coverage; Unity EditMode runner is unavailable in this container for execution.

T9.2 - Next run: execute Unity EditMode suite to validate editor DMX simulator behavior
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


T3.7 - Enforce memory budget constraints for media playback selection
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T3.8 - Next run: execute Unity EditMode suite to validate media budget enforcement tests
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Blocked locally: Unity Editor/.NET CLI are unavailable in this container, so EditMode tests must run in CI.

T7.4 - Fix UI_DmxSettings compile error from invalid [Header] attribute usage
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T7.5 - Execute Unity compile/EditMode checks for UI_DmxSettings regression coverage
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Blocked locally: no Unity runtime is installed in this environment; CI workflow is present at `.github/workflows/unity-editmode-tests.yml`.

T8.4 - Next run: execute Unity EditMode suite in CI to validate T8.2 media path tests
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - CI runner is configured; pending execution in a licensed Unity CI environment.

T8.5 - Next run: capture Unity EditMode CI run artifacts and close lingering test-validation tickets
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


T1.3 - Implement zero-allocation DMX buffer (DmxBuffer.cs)
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T1.4 - Implement ArtNet Receiver scaffold (ArtNetReceiver.cs)
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T1.5 - Map DMX channels 1–4 → Master Dimmer
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T1.6 - Render full-screen quad (1280x720 resolution)
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T1.7 - Enforce fixed frame rate (30 FPS) and disable VSync
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T1.8 - Test stability on HY300 hardware platform
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T2.1 - Create unified shader (MaliSafeLighting.shader)
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2a - Solid color pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2b - Linear gradient pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2c - Radial gradient pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2d - Pulse pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2e - Moving bars pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.2f - Soft edge beam pattern implementation
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.3a - Pattern select mapping (DMX channel 5)
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.3b - Speed mapping (DMX channel 6)
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.3c - Size mapping (DMX channel 7)
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.3d - Strobe mapping (DMX channel 8)
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.4 - Optimize shader for Mali/low-end GPUs
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T2.5 - Profile and test performance on target hardware
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T2.6 - Implement thermal management for high-load scenarios
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
T3.1 - Integrate VideoPlayer component
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.2 - Load media from USB storage
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.3 - Load media from StreamingAssets
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.4 - Implement DMX mapping for media selection
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.5 - Implement DMX mapping for play/pause controls
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.6 - Add looping support for media playback
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written

T3.7 - Enforce memory budget constraints
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T2.4a - Profile shader instruction count and simplify heavy branches on HY300
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written
T2.5a - Run on-device profiling pass on HY300 for branchless shader variant
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written

T7.1 - Add EditMode unit tests for DMX, ArtNet validation, output generators, and UI DMX settings
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T7.2 - Integrate Unity Test Runner CI execution and coverage reporting
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T7.1a - Fix EditMode test assembly references and shader setter visibility for UI DMX settings tests
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T7.3 - Execute Unity EditMode suite in CI or local Unity runner to validate assembly fixes
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T8.1 - Implement Unity VideoPlayer scaffold and DMX media transport controls
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written


T8.2 - Add USB/StreamingAssets integration tests for MediaPlaybackController
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T8.3 - Wire MediaPlaybackController into scene and verify VideoPlayer bindings on HY300
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written
