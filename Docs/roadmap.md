# Android Projector Lighting Fixture – Development Roadmap

**Target Device:** Magcubic HY300 (Android 11, low-end projector)

**Purpose:** Transform a cheap Android projector into a stable DMX/Art-Net controlled stage lighting fixture with procedural patterns, media playback, and optional projection mapping.

---

## Phase 1 – Core Foundation (2 Weeks)

### Goal:
Establish the basic fixture pipeline: ArtNet → DMX → RGB output.

**Tasks:**
- [ ] Setup Unity project (Built-in RP, ARM64, IL2CPP, High Stripping)
- [ ] Configure Android build settings (API 29+, 30 FPS, 720p internal resolution)
- [x] Implement zero-allocation DMX buffer (`DmxBuffer.cs`)
- [x] Implement ArtNet Receiver scaffold (`ArtNetReceiver.cs`)
- [x] Map DMX channels 1–4 → Master Dimmer + RGB
- [ ] Render full-screen quad to 1280x720 RenderTexture
- [x] Enforce fixed frame rate (30 FPS) and disable VSync
- [ ] Test stability on HY300

**Acceptance Criteria:**
- ArtNet data received correctly
- RGB responds to DMX channels
- App maintains 30 FPS without GC spikes
- No crashes during 10 min continuous run

---

## Phase 2 – Pattern System (3 Weeks)

### Goal:
Implement low-GPU procedural patterns with DMX control.

**Tasks:**
- [x] Create unified shader (`MaliSafeLighting.shader`) with pattern selection
- [x] Implement safe patterns:
    - Solid color
    - Linear gradient
    - Radial gradient
    - Pulse
    - Moving bars
    - Soft edge beam
- [x] Map DMX channels 5–8 to:
    - Pattern select
    - Speed
    - Size
    - Strobe
- [x] Optimize shader for Mali/low-end GPUs
- [ ] Profile and test performance on HY300
- [x] Implement pattern intensity scaling for thermal protection
- [x] Add extended shader pattern library (horizontal stripes, checkerboard, diagonal wave, Voronoi-style cells) with DMX pattern-slot mapping updates
- [x] Expand shader pattern library by 10 additional DMX-selectable low-cost procedural modes (vertical wave, ring bands, spiral, diamond grid, sparkle, pinwheel, sweep, ripple, plasma, cross pulse)
- [x] Replace pattern slot 9 Voronoi cells with a DMX-size-controlled radial outline/glow mode
- [x] Replace pattern slot 1 linear gradient with media texture playback plus editor-configurable fallback image
- [x] Render pattern/media black levels as transparent output alpha for cleaner compositing

**Acceptance Criteria:**
- All patterns render smoothly at 30 FPS
- Switching patterns does not trigger GC or stutter
- Patterns respond correctly to DMX control

---

## Phase 3 – Media Playback Engine (3 Weeks)

### Goal:
Enable video and image playback controlled by DMX.

**Tasks:**
- [x] Integrate Unity VideoPlayer scaffold for MP4 (H.264, 720p)
- [x] Load media from USB / StreamingAssets via prioritized file lookup
- [x] DMX channel mapping for:
    - Media select
    - Play / Pause / Stop
- [x] Looping support
- [x] Memory budget enforcement (<50MB textures)
- [ ] Optionally prepare for NAS streaming (HTTP, later phase)
- [ ] Test with HY300 projector for performance and stability

**Acceptance Criteria:**
- Videos play without dropping frames
- Media responds to DMX commands
- No memory spikes or crashes during 30 min playback

---

## Phase 4 – Stability & Optimization (2 Weeks)

### Goal:
Ensure app reliability on low-end hardware.

**Tasks:**
- [ ] Conduct long-duration tests (white screen, pattern switching)
- [ ] Monitor memory and GC allocations
- [ ] Implement brightness/thermal limiting
- [ ] Handle network disconnects gracefully
- [ ] Implement auto-restart if ArtNet fails
- [ ] Optimize scripts for minimal CPU load

**Acceptance Criteria:**
- No crashes under stress
- Thermal protection activates at sustained white output
- 30 FPS maintained for long durations

---

## Phase 5 – Projection Mapping (Optional, 4 Weeks)

### Goal:
Add basic projection mapping for fixture output.

**Tasks:**
- [x] Implement quad mesh warp with 4 corner offsets
- [ ] Implement vertex shader for keystone correction
- [ ] Save/load preset mapping
- [x] DMX channel control for keystone X/Y
- [x] Add multi-mesh fixture manager with persisted object count (1-16) and automatic 16-channel DMX address stepping per spawned mesh
- [ ] Test on HY300 and record frame rate impact

**Acceptance Criteria:**
- Mapping works with minimal FPS drop
- Presets are saved and restored correctly
- Keystone offsets controlled via DMX

---

## Phase 6 – Configuration & Management (2 Weeks)

_Progress note: hidden settings panel visibility can now be toggled from Android TV settings key input._

### Goal:
Enable persistent configuration and easy deployment.

**Tasks:**
- [ ] Implement JSON config file load/save
- [x] Allow selection of DMX universe and start address
- [x] Restrict DMX universe/start address adjustments to UI +/- button controls (no free-form numeric text input)
- [ ] Implement mode selection (basic, standard, full)
- [x] Auto-load last configuration on boot
- [x] Provide hidden developer settings UI

**Acceptance Criteria:**
- App starts with last config automatically
- Users can switch fixture modes via config file
- No runtime allocation spikes from config management

---

## Phase 7 – Testing & Documentation (1–2 Weeks)

### Goal:
Validate system and prepare for production deployment.

**Tasks:**
- [ ] Full HY300 stress test: 10 min white, 10 min rapid DMX
- [ ] Verify video playback stability and performance
- [ ] Verify pattern responsiveness and accuracy
- [ ] Document DMX personality
- [ ] Create developer guide and code comments
- [x] Add EditMode unit tests for core DMX/ArtNet/output/UI script logic
- [x] Add EditMode unit tests for media USB/StreamingAssets path resolution logic
- [x] Configure Unity EditMode CI workflow (`.github/workflows/unity-editmode-tests.yml`) for licensed runner execution
- [x] Add Editor DMX simulation UI workflow (`UI_DmxEditorSimulator.cs`) for in-Editor channel testing
- [x] Add dedicated RGB + dimmer shader bridge component (`RgbDmxController.cs`) with EditMode coverage
- [x] Add Android TV remote D-pad navigation flow (`UI_DpadNavigationController.cs`) with submit-capable selectable support (`UI_DpadSelectable.cs`)

**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build

---

## Phase 8 – Add Moving Head mode

### Goal:
Add a new mode that makes the app function like a Moving Head. The moving head is used to see beams coming out of the projector using haze or smoke.

**Tasks:**

- [ ] Create a new Mali-safe shader to visualize patterns. Include a masking cicle that can be adjusted through script
- [ ] Add a setting to the settings menu to select the mode
- [ ] Save the selected setting in user-prefs
- [ ] Add new DMX channel mapping: 1-Master Dimmer, 2-4 RGB, 5-pan, 6-pan fine,7-tilt, 8 tilt-fine, 9-pattern select, 10-pattern speed, 11-pattern parameter, 12-Iris/Scale, 13-Rotate, 14-strobe
- [ ]  

**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build

## Phase 9 – Add Pixel mappoing mode

### Goal:
Add a new mode that makes the app function like a pixel wall.

**Tasks:**


- [ ] Create a new Mali-safe shader to function as a pixel wall. 
- [ ] Add a setting to the settings menu to select the mode
- [ ] Add a setting to the menu to adjust the pixel wall size. Rows and Columns amount. Restricted to maximum 32x32
- [ ] Save the selected setting in user-prefs
- [ ] Add new DMX channel mapping: 1-Master Dimmer, 2-Strobe, 3-10 corner pinning X and Y, 11-? RGB values for each pixel


**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build




### Developer Notes

- Treat this as a **deterministic embedded lighting fixture**.
- Prioritize **stability, low memory use, and predictable 30 FPS performance**.
- Avoid any runtime memory allocations in Update/Render loops.
- All features that risk performance (high-res video, multi-pass shaders) are optional and must be gated by mode selection.



