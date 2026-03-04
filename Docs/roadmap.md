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
- [ ] Enforce fixed frame rate (30 FPS) and disable VSync
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
- [ ] Implement pattern intensity scaling for thermal protection

**Acceptance Criteria:**
- All patterns render smoothly at 30 FPS
- Switching patterns does not trigger GC or stutter
- Patterns respond correctly to DMX control

---

## Phase 3 – Media Playback Engine (3 Weeks)

### Goal:
Enable video and image playback controlled by DMX.

**Tasks:**
- [ ] Integrate Unity VideoPlayer for MP4 (H.264, 720p)
- [ ] Load media from USB / StreamingAssets
- [ ] DMX channel mapping for:
    - Media select
    - Play / Pause / Stop
- [ ] Looping support
- [ ] Memory budget enforcement (<50MB textures)
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
- [ ] Implement quad mesh warp with 4 corner offsets
- [ ] Implement vertex shader for keystone correction
- [ ] Save/load preset mapping
- [ ] DMX channel control for keystone X/Y
- [ ] Test on HY300 and record frame rate impact

**Acceptance Criteria:**
- Mapping works with minimal FPS drop
- Presets are saved and restored correctly
- Keystone offsets controlled via DMX

---

## Phase 6 – Configuration & Management (2 Weeks)

### Goal:
Enable persistent configuration and easy deployment.

**Tasks:**
- [ ] Implement JSON config file load/save
- [ ] Allow selection of DMX universe and start address
- [ ] Implement mode selection (basic, standard, full)
- [ ] Auto-load last configuration on boot
- [ ] Provide hidden developer settings UI

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

**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build

---

### Developer Notes

- Treat this as a **deterministic embedded lighting fixture**.
- Prioritize **stability, low memory use, and predictable 30 FPS performance**.
- Avoid any runtime memory allocations in Update/Render loops.
- All features that risk performance (high-res video, multi-pass shaders) are optional and must be gated by mode selection.
