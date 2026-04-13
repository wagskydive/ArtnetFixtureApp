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
- [x] Wire `MediaPlaybackController` into `MainScene` with auto-binding fallback for `VideoPlayer`
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
_Progress note: local web settings API endpoint and HTML settings page persistence bridge are now in place for PlayerPrefs-backed load/save._
_Progress note: web settings API handling now marshals Unity/PlayerPrefs operations to the main thread to avoid `LocalWebUiServer` request-loop exceptions._
_Progress note: in-app Android WebView surface now loads the same local `webui.html` UI (`127.0.0.1`) used by LAN clients, and menu show/hide toggles now synchronize WebView visibility._
_Progress note: Unity Editor debugging now supports opening/logging the same local WebUI URL via `InAppWebViewSurface`, enabling quick iteration without Android deploys._
_Progress note: WebUI settings now auto-save on every field change (debounced) and immediately persist to PlayerPrefs-backed `/api/settings` without a dedicated Save button._
_Progress note: WebUI mode behavior is now aligned again so only Surface mode exposes fixture amount controls, non-Surface modes force a single fixture instance, and web-saved universe/start channel values now reapply correctly to spawned fixtures._
_Progress note: universe/start channel persistence now survives consecutive app restarts by preventing stale `UI_DmxSettings` writes from overwriting WebUI-applied receiver addresses during pause/disable saves._
_Progress note: PlayerPrefs persistence now routes through a centralized `SaveLoadSettings` helper used by DMX UI, fixture mode/count, and WebUI settings stores for cleaner debugging and consistency._
_Progress note: DMX universe/start-channel startup hydration now runs in `Awake` with guarded shutdown saves so early lifecycle events cannot overwrite persisted PlayerPrefs before load; LocalWebUiServer settings API tests now cover POST+GET rehydration of saved values._
_Progress note: LAN WebUI availability is now decoupled from in-app menu visibility by keeping the `HtmlUI` host active when the menu is hidden, so `LocalWebUiServer` remains reachable even outside menu view._
_Progress note: settings menu startup behavior is now fixed to always hidden on boot in `UI_SettingsPanelToggle`, removing the startup-show toggle and preventing black-screen menu overlays._
_Progress note: in-app Android WebView overlay now supports transparent background plus normalized position/size controls so the WebUI can run as a resizable non-fullscreen overlay._
_Progress note: in-app settings now run Unity UI only; `InAppWebViewSurface` has been removed, while LAN/localhost WebUI continues through `LocalWebUiServer` as an external client surface._
_Progress note: external WebUI clients now poll `/api/settings` once per second to reflect live settings changes made from Unity UI or another browser session._
_Progress note: `UI_DpadNavigationController` now uses Input System actions only, supports position-based selectable discovery when no explicit list is configured, and suppresses duplicate same-frame submit invocations._
_Progress note: popup panels now enforce modal D-pad behavior by temporarily disabling non-popup `UI_DpadNavigationController` instances while open, preventing background panel navigation/submit/cancel handling._
_Progress note: IAP entitlement flow now supports online backend validation with offline cache fallback, including automatic refunded-product revocation via bidirectional entitlement sync._
_Progress note: revoked purchase responses are now handled gracefully by queueing pending revocations for next startup, then applying removals with a user-facing message instead of immediate mid-session access loss._
_Progress note: refunded-purchase popup content now always refreshes both title and message text and is generated from the actual revoked product IDs to avoid stale or unrelated capability copy in the dialog._
_Progress note: WebUI `/api/settings` IP reporting now uses `IpSolver` and preserves `ipAddress` through settings JSON sanitization, fixing `0.0.0.0` display regressions in LAN clients._
_Progress note: sACN ingest now tracks per-universe source state with highest-priority arbitration, supports staged sync-buffer apply, and advanced-networking entitlement now also unlocks unlimited-universe capability._
_Progress note: WebUI DMX universe edits now enforce unlimited-universe entitlement in-browser, showing the locked-feature modal and reverting to universe 1 when users without the upgrade attempt values above 1._

### Goal:
Enable persistent configuration and easy deployment.

**Tasks:**
- [ ] Implement JSON config file load/save
- [x] Allow selection of DMX universe and start address
- [x] Restrict DMX universe/start address adjustments to UI +/- button controls (no free-form numeric text input)
- [x] Align universe UI as 1-based while preserving 0-based internal ArtNet receiver storage
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
- [x] Migrate D-pad menu navigation to Unity Input System actions (`navigateAction`/`submitAction`) with keyboard/gamepad fallback handling for Input System-only builds

**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build

---

## Phase 8 – Add Moving Head mode

### Goal:
Add a new mode that makes the app function like a Moving Head. The moving head is used to see beams coming out of the projector using haze or smoke.

**Tasks:**

- [x] Create a new Mali-safe shader to visualize patterns. Include a masking cicle that can be adjusted through script
- [x] Add a setting to the settings menu to select the mode
- [x] Save the selected setting in user-prefs
- [x] Add Moving Head pattern library parity in dedicated shader (`MaliSafeMovingHead.shader`) with branchless mask selection and circular beam masking controls
- [x] Add new DMX channel mapping: 1-Master Dimmer, 2-4 RGB, 5-pan, 6-pan fine,7-tilt, 8 tilt-fine, 9-pattern select, 10-pattern speed, 11-pattern parameter, 12-Iris/Scale, 13-Rotate, 14-strobe
- [ ]  

**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build

## Phase 9 – Add Pixel mappoing mode

### Goal:
Add a new mode that makes the app function like a pixel wall.

**Tasks:**


- [x] Create a new Mali-safe shader to function as a pixel wall. Include adjustable grid size for the amount of pixels the shader has, using rows and columns of pixels.
- [x] Add a setting to the settings menu to select the mode
- [x] Replace mode dropdown dependency with +/- button cycling and text-based mode label in fixture mode UI
- [x] Add a setting to the menu to adjust the pixel wall size. Rows and Columns amount. Restricted to maximum 32x32
- [x] Restrict pixel grid controls visibility to Pixel Mapping mode and enforce 8-pixel +/- step sizing (8..32)
- [x] Save the selected setting in user-prefs
- [x] Add new DMX channel mapping: 1-Master Dimmer, 2-Strobe, 3-10 corner pinning X and Y, 11-? RGB values for each pixel
- [x] Add dual corner-pin personalities so Standard/Surface Projection mode uses channels 9-16 while Pixel Mapping mode keeps channels 3-10
- [x] Keep fixture amount controls visible only for Standard mode and force single fixture instance for Moving Head + Pixel Mapping modes
- [x] Fix runtime no-output regression after mode switches by rebinding output controllers to newly assigned renderer materials
- [x] Persist and restore Standard-mode fixture amount when switching away to non-Standard modes and back


**Acceptance Criteria:**
- Fixture passes all stress tests
- Documentation complete for all modules
- Ready for production build




### Developer Notes

- Treat this as a **deterministic embedded lighting fixture**.
- Prioritize **stability, low memory use, and predictable 30 FPS performance**.
- Avoid any runtime memory allocations in Update/Render loops.
- All features that risk performance (high-res video, multi-pass shaders) are optional and must be gated by mode selection.



_Progress note: `DmxModeManager.SetFixtureMode` now ignores no-op mode assignments to avoid unnecessary material rebinds that caused visible white flashes during settings synchronization._
_Progress note: WebUI fixture-name editing now saves on submit (`change`/Enter) instead of per-keystroke autosave, preventing disruptive reload-like updates while typing._
_Progress note: fixture name + resolved local IPv4 are now surfaced in WebUI (`/api/settings` metadata) and Unity UI display fields for faster device identification on-site._
_Progress note: purchase validation debug controls now use `Debug Validation` + `All Is Validated` toggles, and editor-only entitlement bypass in `CapabilityService` has been removed so advanced-networking lock behavior follows validated entitlements consistently in-editor and runtime._
_Progress note: WebUI locked-feature dialogs are now centralized into a reusable modal API with dynamic title/message text, reducing duplicate modal wiring for gated features._
_Progress note: WebUI now includes a dedicated `Custom Gobos` toggle button that gates section visibility behind click-time entitlement validation and shows the reusable lock modal when unavailable._
_Progress note: advanced networking WebUI controls now include protocol/transport/address/merge/universe subscription settings (capability-gated), with `/api/network-debug` live packet telemetry fed by a new `NetworkDebugService`._
_Progress note: `UI_TVKeyboardTextEditor` now supports configurable input validation modes (IP, numeric range, no spaces/special chars, alphanumeric, length range) with popup-based user error feedback before applying text._
_Progress note: WebUI now includes responsive mobile styling for phone-sized screens with larger controls and improved layout density._
_Progress note: Moving Head custom-gobo runtime now caches slot textures by file timestamp and avoids full 16-slot reload/destruction on every poll cycle, reducing periodic stutter after custom gobo integration._
_Progress note: WebUI password flow is now active with persisted password storage and `/api/login` validation endpoint for lock/unlock behavior._
_Progress note: WebUI password protection now uses SHA-256 hashed storage with a Unity UI-managed enable toggle, and browser clients cache successful auth state while still re-locking when protection is enabled/configured._
_Progress note: WebUI HTML is now settings-only (DMX test page/tabs removed), includes a direct feedback button (`dilarium.es/dmx-projector/feedback`), and shows the resolved access URL in a read-only IP field._
_Progress note: IAP validation payloads now always include a stable per-device identifier (with persisted fallback when system ID is unavailable), and backend revocation mapping now treats only canceled/refunded Google Play states as revoked for cleaner multi-device entitlement sync._
_Progress note: Purchase validation entitlement reconciliation now only mutates products that were actually validated in the current pass, preventing unrelated IAPs from being temporarily revoked when another product is re-purchased._
_Progress note: Purchase validation popup flow is now driven only by explicit server `revoked` responses from the current validation pass; legacy startup pending/suspicious fallback revocation paths were removed to prevent false-positive refund popups._
_Progress note: in-app purchase capability scaffolding is now in place with ScriptableObject capability definitions, runtime capability lookup database, in-memory entitlement store abstraction, and a core capability resolution service._
_Progress note: IAP runtime integration now gates universe selection through capabilities, includes locked-feature UI trigger plumbing, adds persisted entitlement storage, and provides development-only debug unlock/reset tools._
_Progress note: IAP purchase rows now show product pricing by reading live Unity IAP localized metadata when available, with an Editor fallback price sourced from each `CapabilityDefinition` asset._
_Progress note: Removed hardcoded `CapabilityIds` usage by resolving IAP capability IDs from `CapabilityDefinition` assets, and wired `MainScene` with capability database/service + locked-capability UI trigger/panel references for scene-level IAP gating._
_Progress note: Capability definitions now support multiple unlock product IDs, MainScene capability database references were cleaned to remove stale advanced-info entries, and a new dynamic `IapPurchasePanel`/`IapPurchasePanelItem` UI script pair can render purchasable capabilities with locked/unlocked status._
_Progress note: Info panel access is no longer IAP-gated; `UI_DmxSettings` now defaults the info panel to enabled and always allows toggle-based control with persisted state._
_Progress note: Purchase execution now routes through a new `UnityIapPurchaseGateway` (`UnityEngine.Purchasing`) that initializes products from capability assets and unlocks entitlements on successful purchase callbacks._
_Progress note: A reusable `Popup` UI component now provides back-button close handling and keeps navigation focus inside active popup dialogs, preventing accidental interaction with underlying menus._
_Progress note: `UI_DpadNavigationController` now supports independent horizontal/vertical navigation toggles plus separate horizontal/vertical wrapping, with improved deterministic vertical wrap behavior._
_Progress note: Settings menu toggling is now show-only in `UI_SettingsPanelToggle`; cancel/back handling moved into panel-local `UI_DpadNavigationController` cancel events so popup cancel input no longer hides the underlying settings panel._
_Progress note: Unity IAP now initializes at app startup in `UnityIapPurchaseGateway`, validates product availability before initiating purchases, and performs an ownership sync sweep on IAP panel open so previously owned non-consumables render unlocked immediately._
_Progress note: IAP entitlement persistence now writes encrypted PlayerPrefs payloads (`enc_v1:`), while still migrating legacy plaintext entitlement saves so offline unlocked state remains valid after app restarts._
_Progress note: `UI_DmxSettings` network warning visibility is now state-driven (`no data` + `warning enabled`) so warning panels hide correctly when Art-Net data resumes and no longer reappear from settings reload side effects._
_Progress note: Capability definitions now support consumable products, with Unity IAP product registration and purchase handling split between consumable quantity tracking (`iap.consumables`) and non-consumable entitlement unlock + validation flows._
_Progress note: Custom gobo IAP groundwork is now implemented with persistent `CustomGobos/slot1..slot16` PNG storage + validation (RGBA/512x512), new WebUI `/images` + `/upload` + `/CustomGobos/<file>` endpoints, Moving Head runtime cycling of uploaded gobos via `_Speed`, and capability asset `custom.gobos.upgrade` gating/fallback behavior._
_Progress note: Revocation-popup dismissal no longer leaves settings navigation frozen: `Popup` now releases blocked `UI_DpadNavigationController` instances when the popup panel is hidden externally (e.g. cancel button `SetActive(false)` path), with EditMode regression coverage for that close flow._
_Progress note: Custom Gobo WebUI now supports per-slot removal with confirmation via new `/remove?slot=X` endpoint, deletes PNGs from persistent storage, and compacts remaining files so slot numbering stays contiguous (`slot1..slotN`) with default shader fallback when no custom gobos remain._

_Progress note: networking input is now protocol-swappable via `INetworkReceiver` + `NetworkingModeManager`, with new `SAcnReceiver` support and advanced-networking-gated `UI_NetworkPanel` mode switching (Art-Net/sACN)._

_Progress note: networking mode selection now persists through `SaveLoadSettings.NetworkModeKey`, and startup now falls back to Art-Net when a saved non-default mode is not entitled (after triggering purchase validation)._

_Progress note: `NetworkingModeManager` is now singleton-backed and exposes active `INetworkReceiver` access so runtime systems consume protocol-agnostic DMX input through `NetworkingModeManager.Instance.NetworkReceiver` (Art-Net or sACN) instead of hard references to `ArtNetReceiver`._

_Progress note: Networking mode selection now swaps runtime receiver components in `NetworkingModeManager` (Art-Net/sACN) using integer mode indices, with a manager-owned DMX buffer assigned to each newly added receiver._
_Progress note: `UI_SAcnSettings` now loads and validates the active `NetworkReceiver` on enable (auto-hiding itself unless sACN is active), and exposes dedicated handlers for sACN multicast/unicast transport settings (mode, addresses, listen port) for upcoming panel wiring._

_Progress note: `PurchaseValidationManager` now includes an Editor-only debug bypass flag (`debugForceValidInEditor`) that treats owned IAP receipts as valid without backend calls, enabling local IAP UI/entitlement workflow debugging in Unity Editor._

_Progress note: WebUI advanced-networking lock modal now defaults to hidden via `aria-hidden="true"` + `.modal-backdrop.hidden { display: none; }`, ensuring the OK button closes it reliably and it never appears before user interaction._

_Progress note: WebUI section ordering now places the Custom Gobos panel directly below its toggle button, keeps the Feedback action as the final bottom section, and uses a unified button color style across action buttons._
_Progress note: Editor debug validation now propagates through `CapabilityService` resolution (`debugValidation` + `allIsValidated`), so WebUI IAP-gated controls follow the same forced-valid entitlement state as in-app capability checks._
