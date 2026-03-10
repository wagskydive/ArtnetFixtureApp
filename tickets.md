# Tickets

T11.1 - Modify the UI_FixtureModeSelector.cs to not work with a dropdown object but to simply have public function to increment and decrement the current mode and cycle trough the modes. The ui will have simple + and - buttons connected to those functions and a text object will display the current mode
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T11.2 - Modify the UI_FixtureModeSelector.cs so that the grid size for the pixel mapping mode only shows when the Pixel Mapping mode is selected and make it work with increment and decrement buttons for X size and Y size in increments of 8 pixels and a maximum of 32 pixels on both dimensions.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.24 - Verify if the Universe selection logic works and fix it if it doesn't work
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Corrected `ArtNetReceiver` default universe to `0` (Art-Net universe 1 in UI terms) so first-run behavior matches the user-facing universe selector.
  - Added EditMode coverage to verify the default universe maps to user-facing universe 1 while preserving 1-based UI <-> 0-based receiver conversion.

T10.25 - Next run: execute Unity EditMode suite on a licensed runner to validate T10.24 universe default + mapping behavior end-to-end.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.23 - Modify MaliSafeLighting.shader so that the black parts of all the patterns are transparent
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `MaliSafeLighting.shader` to render in transparent queue with alpha blending and to derive alpha from pattern/media brightness so black areas render transparent.

T10.21 - Replace the outline pattern in the MaliSafeLighting.shader with an outline of the actual mesh. Now it is a circle, but i want it to outline the quad mesh.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated pattern slot 9 in `MaliSafeLighting.shader` to derive outline/glow from quad edge distance (`uv` border) instead of radial distance so the effect follows mesh bounds.

T10.22 - Next run: execute Unity EditMode tests (licensed runner) and capture visual verification of quad-edge outline behavior on device.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.19 - Replace the gradient pattern in the MaliSafeLighting.shader with a video texture. The videos will be played from a usb drive. Create a fallback where we can input an image instead. The image will be the app logo and can be adewd in the Unity Editor.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Replaced pattern slot 1 with media-texture sampling in `MaliSafeLighting.shader`, with `_MediaTex` for active video frames and `_FallbackTex` for editor-assigned logo/image fallback.
  - Added `PatternMediaTextureController` to push `VideoPlayer.texture` into shader property `_MediaTex` and automatically switch to fallback when no video frame is available.


T10.18 - Replace the voronoi pattern in the MaliSafeLighting.shader with an outline shader where the size parameter adjusts the blur/fuzzyness/glow.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Replaced the former Voronoi-style pattern slot (index 9) with a radial outline/glow pattern where DMX size controls blur/fuzziness width.



T10.20 - Next run: wire `PatternMediaTextureController` + fallback logo texture assignment in `MainScene` material setup and run Unity EditMode tests on a licensed runner for T10.18/T10.19
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.16 - Modify the CornerPinDmxWarp class so that each corner can reach anywhere on the screen. Currently the corner positions are calculated from the middle, but that gives a problem when a mesh needs to be projected in such a way that for example the upper right corner has to be positioned left of the midpoint. So the solution is to calculate each corner from the lower left corner when the dmx channel is at 0 and when the DMX channel is at 255, the position is at the top or right edge.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `CornerPinDmxWarp` so every corner now maps both X/Y DMX values from the same lower-left origin to full screen extents, allowing any corner to move across either half of the screen.
  - Expanded `CornerPinDmxWarpTests` with assertions for lower-left collapse at DMX 0 and midpoint-crossing behavior for the top-right corner.

T10.17 - Next run: execute Unity EditMode suite on a licensed runner to validate T10.16 corner-origin remap behavior end-to-end
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.14 - Create a system to add more meshes that are each listening to their own 16 DMX channels for colors, effects and corner pins. Make a script to extend the UI to add more objects. The UI will work the same as the ui for the starting DMX channel and Universe. The amount of Meshes should be saved in user prefs and is recalled when the app starts. There is a minimum amount of 1 and a maximum of 16 objects. The start DMX channel is only set as it is now and the next object will listen to the the next free channel. So the second object will automatically listen to channel 17 as its starting channel and the third object listens to 33 etc.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_FixtureMeshManager` to grow/shrink fixture mesh instances with 1-16 bounds, persist fixture count in PlayerPrefs, and auto-assign each fixture's start address in 16-channel blocks.
  - Added clone-safe Art-Net receiver option so spawned fixtures share the DMX buffer without binding additional UDP listeners.
  - Added EditMode coverage for fixture-count clamping, persisted count restore, DMX channel stepping (1/17/33...), and spawned receiver networking behavior.

T10.15 - Next run: wire `UI_FixtureMeshManager` buttons/text + fixture template references in `MainScene` and run Unity EditMode tests on a licensed runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.12 - Add 10 more patterns to the MaliSafeLighting.shader. Modify the PatternGenerator also so the new patterns can also be used through the DMX channel.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added ten new low-cost pattern modes to `MaliSafeLighting.shader` (indices 10-19): vertical wave, ring bands, spiral, diamond grid, sparkle, pinwheel, sweep, ripple, plasma, and cross pulse.
  - Expanded `PatternGenerator` DMX mapping to 20 total pattern slots while preserving existing channel 5 behavior.
  - Updated EditMode assertion coverage so max DMX pattern input resolves to the new highest pattern index.

T10.13 - Next run: execute Unity EditMode suite to validate T10.12 expanded 20-pattern shader + DMX mapping behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


T10.10 - Add more patterns to the MaliSafeLighting.shader. At least a horizontal stripes pattern and a voronoi pattern and a couple more. Modify the PatternGenerator also so the new patterns can also be used through the DMX channel
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Extended `MaliSafeLighting.shader` with four new pattern modes: horizontal stripes, checkerboard, diagonal wave, and a lightweight Voronoi-style cell pattern.
  - Updated `PatternGenerator` DMX pattern mapping so channel 5 now spans ten pattern slots (0-9) across the full DMX byte range.
  - Refreshed `OutputComponentsTests` pattern assertion to validate max-DMX selection reaches the new highest pattern index.

T10.11 - Next run: execute Unity EditMode suite to validate T10.10 expanded pattern shader + DMX mapping behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.8 - Rewrite the CornerPinDmxWarp to prevent a sharp crease. Make a subdivided mesh that can prevents the sharp crease. Make a variable to set the subdivision amount.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Rebuilt `CornerPinDmxWarp` to generate a runtime subdivided quad mesh controlled by a serialized `subdivisionAmount` setting.
  - Applied bilinear interpolation across all subdivided vertices using the four DMX-driven corner targets to reduce visible center-diagonal creasing.
  - Expanded EditMode coverage for mesh collapse-at-zero DMX, corner expansion-at-max DMX, and subdivision-driven mesh density.

T10.9 - Next run: execute Unity EditMode suite to validate T10.8 subdivided CornerPin warp behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


T10.6 - Make a script that can hide and show an object by clicking on the settings button of the remote-controller of AndroidTV
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_SettingsPanelToggle` to toggle a target object when the Android TV settings key (`KeyCode.Menu`) is pressed.
  - Added EditMode coverage for hide/show toggle behavior.

T10.5 - Modify CornerPinDmxWarp so that the mesh mesh is warped from the middle of the original object when all the DMX channels are at 0. and when they are at 255, the corners are in the farthes corners of the screen.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `CornerPinDmxWarp` so DMX 0 collapses each corner to the mesh center and DMX 255 expands each corner toward screen-extents using per-axis interpolation.
  - Replaced existing corner warp assertions with new EditMode tests for center-collapse and max-expansion behavior.

T10.7 - Next run: execute Unity EditMode suite to validate T10.5 and T10.6 behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.3 - Change the UI_DmxSettings to use + and - buttons instead of input fields in order to only change the DMX start channel and Universe using plus and minus buttons.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Reworked `UI_DmxSettings` to use increment/decrement methods for channel/universe updates and removed direct text-entry handlers.
  - Switched display bindings from `InputField` to read-only `Text` values and expanded EditMode coverage for +/- behavior, bounds clamping, and receiver synchronization.

T10.4 - Next run: execute Unity EditMode suite to validate T10.3 plus/minus DMX settings UI behavior on a licensed Unity runner
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T10.1 - Connect UI_DmxSettings to the artnet Receiver so that the correct DMX start channel and universe gets used.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Connected `UI_DmxSettings` to `ArtNetReceiver` so configured DMX start channel and universe are applied at runtime and persisted.
  - Added `ArtNetReceiver` fixture-relative channel helpers and updated output/simulation components to honor the configured DMX start channel offset.
  - Extended EditMode tests to verify start-channel offset behavior and UI-to-receiver synchronization.

T10.2 - Next run: execute Unity EditMode suite to validate T10.1 DMX start-channel/universe integration on a licensed Unity runner
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

T2.7 - Create a SurfaceProjectionDmxPersonality Mode that is used for the original MaliSafeLighting.shader and uses the original dmx channels for corner pinning per fixture. The original DMX mapping uses channels 9-16 for corner pinning. The CornerPinDmxWarp.cs now listens for the channels 3-11 for the PixelMapping mode. The system needs to be able to be used for both modes. Change that file accordingly or create a seperate file per mode.
[x] Started
[x] Behavior Written
[x] Code Written
[ ] Tests Passed
[x] Documentation Written
  - Added `SurfaceProjectionDmxPersonality` for the standard fixture channel contract (including corner pin start channel 9).
  - Updated `CornerPinDmxWarp` to switch corner-pin channel block by fixture mode: Standard/MovingHead uses 9-16 and PixelMapping uses 3-10.
  - Extended `CornerPinDmxWarpTests` to validate both mode mappings.

T2.8 - Next run: execute Unity EditMode suite on a licensed Unity runner to validate T2.7 dual-personality corner-pin mapping end-to-end.
[ ] Started
[ ] Behavior Written
[ ] Code Written
[ ] Tests Passed
[ ] Documentation Written

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
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Wired `MediaPlaybackController` onto `FixtureEffect` in `MainScene` with serialized `ArtNetReceiver` binding and default media list entry; added runtime fallback binding to auto-resolve/add `VideoPlayer` when missing.

T8.3a - Next run: validate HY300 media playback binding on-device (USB + StreamingAssets, play/pause/stop, looping)
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.6 - Create Moving Head mode shader foundation with Mali-safe circular beam mask control
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `MaliSafeMovingHead.shader` as a dedicated Moving Head foundation shader with a script-adjustable circular beam mask (`_BeamRadius`, `_BeamSoftness`, `_BeamOffsetX`, `_BeamOffsetY`, `_BeamRotation`) and transparent compositing for haze-friendly beam rendering.

T8.6a - Add patterns and functionalities to the Moving Head mode shader that function similar to the MaliSafeLighting.shader. 
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added 20 DMX-addressable pattern slots in `MaliSafeMovingHead.shader` using Mali-safe branchless masks and low-cost math patterns aligned with the existing fixture shader behavior.

T8.6c - Next run: wire `MaliSafeMovingHead.shader` into mode-selection runtime scripts/material assignment and add EditMode coverage for moving-head beam parameter property writes.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UI_FixtureModeSelector` to apply standard vs moving-head materials at runtime and integrated `MovingHeadBeamController` DMX channel mapping for `_BeamOffsetX/_BeamOffsetY/_BeamSoftness/_BeamRadius/_BeamRotation` property writes.
  - Added EditMode coverage for moving-head beam property writes and missing-dependency safety.


T8.7 - Add settings-menu mode selector to switch between existing fixture mode and Moving Head mode
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added settings-ready `UI_FixtureModeSelector` API with dropdown integration (`SetModeFromDropdown`) to switch fixture rendering mode between Standard and Moving Head.

T8.8 - Persist selected fixture mode in PlayerPrefs and auto-restore on app startup
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added fixture mode preference persistence (`dmx.fixture.mode`) and startup restore flow in `UI_FixtureModeSelector`, with EditMode tests for save/load + dropdown synchronization.

T8.13 - Next run: wire `UI_FixtureModeSelector` dropdown/material references in `MainScene` and validate runtime switching behavior on HY300.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.9 - Implement Moving Head DMX personality mapping (1-14) and runtime channel parser
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `MovingHeadDmxPersonality` runtime parser for moving-head channels 1-14 and integrated it into `MovingHeadBeamController` material writes.

T8.9a - Add pan/tilt coarse+fine mapping with normalized output suitable for beam direction controls
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added 16-bit coarse+fine parsing for pan/tilt with normalized output (`0..1`) and mapped to beam offsets (`-1..1`).

T8.9b - Add pattern, speed, parameter, iris/scale, rotate, and strobe mappings for Moving Head mode
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Implemented moving-head pattern/speed/parameter/iris/rotate/strobe channel mapping and expanded EditMode coverage for personality parsing + controller application.

T8.9c - Next run: execute Unity EditMode suite on a licensed runner to validate moving-head 14-channel personality mapping end-to-end.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.10 - Create EditMode tests for Moving Head mode switching, DMX mapping accuracy, and prefs restore
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.11 - Run HY300 validation pass for Moving Head mode (stability, FPS, thermal, DMX response)
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.12 - Document Moving Head DMX personality and operator setup flow in project docs
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.1 - Create a Mali-safe Pixel Mapping shader with configurable row/column grid quantization
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `Assets/Shaders/MaliSafePixelMapping.shader` with deterministic UV quantization driven by configurable `_Rows` and `_Columns` (1-32) to render a pixel-wall grid safely on Mali-class hardware.
  - Added optional `_PixelDataTex` sampling at quantized cell centers so future DMX pixel streams can drive per-cell RGB without changing shader structure.

T9.2 - Extend settings UI with Pixel Mapping mode selection alongside existing fixture modes
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Extended `UI_FixtureModeSelector` with a third `PixelMapping` mode option, dropdown handling for three modes, and pixel-mapping material assignment on the target renderer.

T9.3 - Add Pixel Wall size controls (Rows/Columns) with validation and clamp range of 1-32
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added row/column increment/decrement controls in `UI_FixtureModeSelector` with enforced clamping between 1 and 32 and live UI label synchronization.
  - Wired row/column values into `_Rows` and `_Columns` shader properties for the pixel-mapping material and active renderer material.

T9.4 - Persist Pixel Mapping mode + wall size preferences in PlayerPrefs and restore on startup
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Persisted fixture mode plus pixel grid rows/columns via `PlayerPrefs` keys (`dmx.fixture.mode`, `dmx.pixel.rows`, `dmx.pixel.columns`) and restore/clamp them during startup.
  - Added EditMode tests to cover Pixel Mapping mode switching, row/column clamp behavior, and persisted preference restore.

T9.5 - Implement Pixel Mapping DMX personality (Master Dimmer, Strobe, Corner Pin XY, per-pixel RGB stream)
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `PixelMappingDmxPersonality` DMX parser with fixed channel definitions for master dimmer (ch1), strobe (ch2), corner-pin block start (ch3), and per-pixel RGB stream start (ch11).
  - Added `PixelMappingOutputController` to drive `_Intensity`, `_StrobeGate`, and a point-filtered `_PixelDataTex` from DMX RGB triplets aligned to the configured pixel wall rows/columns.
  - Updated `CornerPinDmxWarp` to consume Pixel Mapping corner channels 3-10 (X/Y for 4 corners) instead of the previous offset block.

T9.6 - Add EditMode tests for Pixel Mapping mode switch, grid-size persistence, and DMX channel parsing
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.7 - Next run: execute HY300 stress/performance validation for Pixel Mapping mode at 32x32 and mixed DMX traffic
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T9.8 - Document Pixel Mapping setup, DMX addressing scheme, and operational limits in project docs
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written
