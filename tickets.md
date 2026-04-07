# Tickets

# In-App Purchase Implementation Tickets

## Key Components
- CapabilityDefinition (data)
- CapabilityDatabase (lookup)
- EntitlementStore (ownership)
- CapabilitySystem (logic)

## Guidelines
- No hardcoded product IDs
- No UI logic in core systems
- Keep systems decoupled
- Ensure offline functionality

## Development Workflow
1. Define capability
2. Assign product ID
3. Use capability in logic

---


T22.1 - Advanced network implementation part 1: Refactor artnetReveiver so it implements an interface INetworkReceiver that can be swapped out with different network receivers. Rename the folder "Scripts/ArtNet" into "Scripts/Networking"
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written


T22.2 - Advanced network implementation part 2: Add a new script "SAcnReceiver" that implements the INetworkReceiver interface. Write the code that makes the system work with sACN networking.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written


T22.3 - Advanced network implementation part 3: Add a "NetworkingModeManagar.cs" script that will handle which network protocol will be used. The script should be able to swap out the default ArtnetReceiver.cs with any other script that implements the INetworkReceiver inferface. Also add a "UI_NetworkPanel.cs" script that will be attached to a new panel to change the networking mode and any other network related settings. If sACN is selected through the UI_NetworkPanel, any addidtional settings related to sACN networking will be shown and configurable on the panel by showing or hiding a parent GameObject that is serialized in the UI_NetworkPanel The UI_NetworkPanel will also check if the "Advanced Networking" IAP is owned and if its locked it will not open the panel, but instead show the "LockedCapabilityPanel".
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written


T22.4 - Advanced network implementation part 4: Make the advanced networking system work so it can use sACN and Artnet.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written


T22.5 - Create a playerprefs entry in the save-load manager that remembers the networkmode change. If another mode then the default (Artnet) is in the player prefs and the entitlement is not unlocked, run the validation and if not validated then set it back to the default (Artnet)
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written



T22.6 - Next run: add EditMode coverage for NetworkingModeManager and SAcn packet parsing edge cases (invalid vectors, non-zero start code, universe mismatch) and verify UI_NetworkPanel lock-state wiring in scene prefabs.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T21.1 - Find performance gaining changes and refactorizations. Currently the moving head mode is a bit stuttering and i think this happened since custom gobos were introduced. 
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

Behavior notes:
- Moving Head custom-gobo selection no longer destroys/reloads all textures every 2 seconds.
- Slot textures are now only decoded when file timestamps actually change, reducing disk I/O and texture churn in Update-driven playback.
- Active `_GoboTex` writes are now skipped when the selected texture is unchanged.

T21.2 - Next run: add EditMode tests that cover custom gobo hot-reload behavior (new file, removed file, unchanged file) and verify no texture replacement occurs when slot timestamps are unchanged.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


T20.1 - Add custom gobos. Part 1

Persistent Storage
Create a folder:
```csharp
string folderPath = Path.Combine(Application.persistentDataPath, "CustomGobos");
if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
```
Store images as:
slot1.png, slot2.png, ..., slot16.png
Ensure 16-slot limit.


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written


T20.2 - Add custom gobos. Part 2 > webui is used to upload and manage the custom gobo files

GET	/images	Returns JSON list of current slots.
GET	/CustomGobos/<file>	Serves saved PNGs for preview.
POST	/upload?slot=X	Accepts PNG file upload for slot X.
Requiere the uploaded file to be a PNG file of 512x512 pixels with an alpha channer
Validate requirements


Web UI (LAN Browser)
Display 16 slots with current images:
Slot 1 [Preview] [Upload Button]
Slot 2 [Preview] [Upload Button]
...
Slot 16 [Preview] [Upload Button]
Upload mechanism:
Select PNG file → press upload → POST to /upload?slot=X.
After upload, refresh slot preview.
Use JavaScript fetch API for requests.


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written



T20.3 - Add custom gobos. Part 3 > Unity texture usage

Unity Texture Usage
Load PNGs from Application.persistentDataPath/CustomGobos/slotX.png:
```csharp
public Texture2D LoadSlotTexture(int slot)
{
    string path = Path.Combine(folderPath, $"slot{slot}.png");
    if (!File.Exists(path)) return null;

    byte[] data = File.ReadAllBytes(path);
    Texture2D tex = new Texture2D(2,2);
    tex.LoadImage(data);
    return tex;
}
```

also ensure the png is usable with the alpha channel.


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T20.4 - Add custom gobos. Part 4 > Integrate into the MaliSafeMovingHead.shader
The MaliSafeMovingHead.shader should now look for the saved slots and if they are not empty, use them. Fallback to the default shader should be taken care of. The _PatternType is remains as is, but the shader should use the _Speed parameter to cycle through all the available custom textures.


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T20.5 - Add custom gobos. Part 5 > Lock the custom gobos behind an IAP. Use the capabity system and entitlements to check if the IAP is owned and use the fallback if its not owned. The IAP will be named "custom.gobos.upgrade"


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written


T20.6 - The custom gobos should be using the "Pattern Speed" different. It should not be switching between gobos at a certain rate. the _Speed fader should just be used as a selector. So when the fader is at zero (0) the first gobo that is available shold show. When there are only 2 gobos available, the fader should show the next gobo at any value above 0.5 (DMX value 128) If there are more gobos available that should be scaled accordingly. So if there are 4 gobos, the value 0-0.25 should show gobo 1, 0.25-0.5 the second gobo. 0.5-0.75 the third and 0.75-1 the fourth.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written


T20.7 - Fix the webui Custom Gobo part so it scales better on any screen size. Currently the file choosing text is not fully readable. Also when a file is chose it can be uploaded directly. The button that is used to Upload could be used to open the file browser and should be the only button under the slots. Unused slots don't have to be visable when they are not in use. There can be a Plus button to add more gobos/slots up to 16.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written


T20.8 - Modify the webui so slots can be removed. add a trash or minus button on each slot and when that button is pressed a confirmation popup gets shown. When confirmed for removal, the slot gets removed and the texture gets deleted from persistantData. The remaining slots get reordered so in the remaining names there are no gaps and the slot numnbers start with slot1. if the last slot gets removed the system should use the default texture again.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T20.9 - Next run: bind custom gobos capability to locked-feature UI popup when upload endpoints return 403 and validate on-device throughput for repeated 512x512 uploads over LAN.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written


## 19.1 — in app purchase implementation — create capability data model using ScriptableObjects

### Goal
Create a flexible, data-driven way to define premium capabilities using Unity’s asset system.

### Description
Introduce a new ScriptableObject type that represents a single capability. This asset will act as the central definition of what a premium feature does.

Each capability must:
- have a unique string identifier (used at runtime)
- define its type (boolean or numeric)
- define the value it provides when unlocked
- reference the product ID that unlocks it
- include user-facing metadata (title and description)

### Unity-specific approach
- Use ScriptableObject assets so capabilities can be created via the editor
- Use CreateAssetMenu so new capabilities can be added without code
- Treat these assets as configuration, not logic

### Important constraints
- IDs must be unique and stable
- Product IDs must match Play Store configuration later
- Do not include any logic inside the ScriptableObject

### Acceptance Criteria
- New capability assets can be created from the Unity menu
- All fields are visible and editable in the inspector
- No runtime behavior yet

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

---

## 19.2 — in app purchase implementation — create centralized capability database

### Goal
Create a runtime-accessible registry of all capability definitions.

### Description
Introduce a MonoBehaviour that holds references to all capability ScriptableObjects and builds a runtime lookup table.

### Unity-specific approach
- Use a serialized list in the inspector to assign capability assets
- On initialization (Awake), convert the list into a dictionary for fast lookup

### Important constraints
- The database must exist exactly once
- Must handle duplicate IDs safely
- Must handle missing IDs gracefully

### Acceptance Criteria
- Capabilities can be retrieved by ID at runtime
- Lookup is fast and does not rely on iteration
- Errors are logged for invalid configurations

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

---

## 19.3 — in app purchase implementation — create entitlement store abstraction (no IAP yet)

### Goal
Create a system that represents which purchases the user owns.

### Description
Introduce an EntitlementStore that tracks unlocked product IDs.

### Unity/C# patterns used
- Use an in-memory structure to track ownership
- No MonoBehaviour required (pure logic class)
- Keep it decoupled from Unity lifecycle

### Important constraints
- Must be replaceable later with real IAP backend
- Must not depend on UI or gameplay systems

### Acceptance Criteria
- Can mark a product ID as unlocked
- Can query if a product ID is unlocked
- Works entirely offline

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

---

## 19.4 — in app purchase implementation — create capability resolution system

### Goal
Create the core system that determines what the user is allowed to do.

### Description
Introduce a CapabilitySystem that resolves capabilities using the database and entitlement store.

### Unity/C# patterns used
- Dependency injection for database and store
- No MonoBehaviour required (pure logic class)

### Important constraints
- No UI logic
- No direct IAP calls
- Must be safe to call frequently

### Acceptance Criteria
- Correct values returned for locked and unlocked states
- Works without UI or Play Store integration

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

---

## 19.5 — in app purchase implementation — integrate first real capability into gameplay (universe limit)

### Goal
Use the system in real application logic for the first time.

### Description
Implement the Universe limit using the capability system instead of hardcoded values.
Free users can only use Universe 1 and premium users can select any other universes

### Unity-specific approach
- Create a capability asset representing universe limits
- Use the capability system inside existing universe selection logic

### Important constraints
- Do not hardcode limits anymore
- All values must come from the capability system

### Acceptance Criteria
- Free users are limited correctly
- Unlocking removes the limit

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Universe selection now resolves max allowed universe through `CapabilityService` + `CapabilitySystem` (`CapabilityIds.UniverseLimit`) instead of hardcoded premium checks.
  - WebUI universe payloads are clamped against the same resolved capability limit so browser and Unity UI behavior match.

---

## 19.6 — in app purchase implementation — introduce locked feature UI trigger

### Goal
Provide feedback when a user hits a premium limitation.

### Description
Trigger a UI panel when a capability blocks an action.

### Unity-specific approach
- Use a reusable UI prefab
- Pass capability ID to the UI

### Important constraints
- UI must not contain business logic
- Must support D-pad navigation

### Acceptance Criteria
- UI appears when limit is reached
- Displays correct information

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added reusable locked-capability UI flow (`CapabilityBlockUiTrigger` + `LockedCapabilityPanel`) that receives a capability ID and resolves title/description metadata from capability assets.

---

## 19.7 — in app purchase implementation — support boolean capability use cases

### Goal
Validate system flexibility with a toggle feature.

### Description
Implement a boolean capability such as enabling a setting.

### Important constraints
- No special-case logic
- Must reuse existing system

### Acceptance Criteria
- Feature blocked when locked
- Works when unlocked

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added boolean capability gate path for info-panel activation in `UI_DmxSettings` using `CapabilityIds.AdvancedInfoPanel`.

---

## 19.8 — in app purchase implementation — persist entitlements locally for offline use

### Goal
Ensure purchases work without internet.

### Description
Extend EntitlementStore to persist unlocked product IDs locally.

### Unity-specific approach
- Use local storage (PlayerPrefs or similar)

### Important constraints
- Must work offline
- Must survive restarts

### Acceptance Criteria
- Unlocks persist after restart
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - `EntitlementStore` now supports optional local persistence and hydration via `PlayerPrefs` key `iap.entitlements` for offline unlock continuity.

---

## 19.9 — in app purchase implementation — integrate Unity IAP with entitlement store

### Goal
Connect real purchases to the system.

### Description
On purchase success, unlock the corresponding product ID in the entitlement store.

### Important constraints
- EntitlementStore is the single source of truth
- CapabilitySystem must remain unchanged

### Acceptance Criteria
- Purchase unlocks capability immediately
- Unlock persists

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added `UnityIapEntitlementBridge` purchase callback adapter that routes successful purchase product IDs into `CapabilityService`/`EntitlementStore`.

---

## 19.10 — in app purchase implementation — support multiple capabilities per product

### Goal
Allow one IAP to unlock multiple capabilities.

### Description
The Scriptable object code lets the user add references to multiple product Ids, so a premium feature can be unlocked from multiple purchase ids.
Once any of the the product Ids has been bought, the feature will register as being unlocked
EXAMPLE: The unlimited universes premium feature can be bought directly as a iap or can also be bundled inside the PRO bundle, so the "unlimited universes" iap can reference the PRO bundle iap as its requirement

### Acceptance Criteria
- One purchase unlocks multiple features

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Capability definitions now support `additionalProductIds`, allowing one capability to be unlocked by any of multiple purchase IDs (primary or alternatives).
  - Added EditMode test coverage for alternate-product unlock resolution.

---

## 19.11 — in app purchase implementation — add developer debug tools for testing

### Goal
Speed up development and testing.

### Description
Create debug controls to unlock/reset capabilities and simulate purchases.

### Important constraints
- Must not be included in production builds

### Acceptance Criteria
- Developer can test all features instantly

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added `IapDebugTools` (Editor/Development builds only) for unlock/reset simulation without production inclusion.

---

## 19.13 — iap improvments - remove CapabilityIds.cs - only refer to the data inside the scriptable objects

### Goal
No hard coded premium features anywhere in the code, so adding premium features is only done by adding more scripptable objects

### Acceptance Criteria
- CapabilityIds are inside the Scriptable objects only and are not hardcoded anywhere

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Removed `CapabilityIds.cs` and switched capability gates in `UI_DmxSettings` and `WebUiSettingsBridge` to ScriptableObject references (`CapabilityDefinition`) so runtime IDs come from asset data only.
  - Updated capability tests to use explicit capability IDs without static constants.
---

## 19.14 — next run — wire IAP capability assets + locked panel prefab in MainScene and validate Android purchase callbacks end-to-end

### Goal
Complete scene-level wiring and device validation for the newly added IAP runtime components.

### Acceptance Criteria
- CapabilityDatabase + CapabilityService + UI trigger references are configured in MainScene
- Android test purchase unlocks capability and survives app restart

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Wired `MainScene` with `CapabilityDatabase`, `CapabilityService`, `CapabilityBlockUiTrigger`, and `LockedCapabilityPanel` references plus capability assets (`unlimited-universes`, `advanced-info-panel`).
  - Android purchase callback/end-to-end validation is still pending a physical/CI Android runtime pass.


## 19.15 — remove advanced info panel IAP

### Goal
Revert back to the info panel to be part of the app without an IAP

### Acceptance Criteria
- The info panel always works and defaults to be on and can be switched using the info panel toggle

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Removed the info-panel capability gate from `UI_DmxSettings` so the toggle no longer depends on IAP unlock state.
  - Changed info-panel startup default to enabled (`InfoPanelEnabledKey` fallback now `1`) while preserving user toggle persistence.
  - Added EditMode coverage proving info panel defaults on when no pref exists and can always be toggled/persisted.

## 19.16 — next run — clean up unused advanced info panel capability asset references

### Goal
Remove stale advanced-info-panel scene/database references now that the info panel is no longer an IAP-gated feature.

### Acceptance Criteria
- MainScene and capability database no longer reference an advanced info panel capability asset
- Universe-limit IAP flow keeps working unchanged

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Removed stale advanced-info capability GUID reference from `MainScene` capability database list; scene now only references the active universe-limit capability asset.
  

## 19.17 — Add a IAP user interface panel script that hadles the purchases.

### Goal
A panel that can be accessed through other ui elements that lets the user make the actual purchases.

### Acceptance Criteria
- The IAP panel shows all available IAPs and their locked/unlocked state. It will have purchase buttons that are generated using the capabilty database.

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added `IapPurchasePanel` + `IapPurchasePanelItem` scripts that dynamically generate purchase UI rows from `CapabilityDatabase` and show locked/unlocked state with purchase button handling.
  - Added EditMode test coverage validating generated row states for locked vs unlocked capabilities.

## 19.18 — Implement the purchasing logic using UnityEngine.Purchasing namespace

### Goal
Implement the ability to make the actual in app purchase using Google Play store backend with the help of UnityEngine.Purchasing namespace

### Acceptance Criteria
- The IAP ui purchase buttons are wired up to a purchasing system that uses UnityEngine.Purchasing namespace

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UnityIapPurchaseGateway` using `UnityEngine.Purchasing` (`IDetailedStoreListener`) to initialize products from `CapabilityDatabase`, initiate purchases, and unlock entitlements when purchases complete.
  - Updated `IapPurchasePanel` purchase-button flow to route purchases through the new gateway, while keeping a local unlock fallback for development/test environments where the store backend is unavailable.
  - Added entitlement-change notifications in `CapabilityService` and rebuild hooks in `IapPurchasePanel` so lock/unlock state refreshes immediately after purchase callbacks.

T19.19 - Modify UnityIapPurchaseGateway.cs so it tries to fetch the price of each IAP and add a fallback for inside the Unity Editor that looks up a test price amount that is sotred in the Scriptable object. So also update the CapabilityDefinition Scriptable object so it has a test-price field. Also add a function in IapPurchasePanelItem.cs that will get the price and adds it to a PriceText Text Ui component.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `editorTestPriceUsd` to `CapabilityDefinition` so each capability can define an Editor-only fallback display price.
  - Extended `UnityIapPurchaseGateway` with live localized-price lookup via Unity IAP metadata plus Editor fallback formatting when store pricing is unavailable.
  - Added panel/item wiring (`IapPurchasePanel.GetDisplayPrice`, `IapPurchasePanelItem.priceText`, `RefreshPriceText`) so generated purchase rows show each product price.
  - Updated EditMode `IapPurchasePanelTests` to verify displayed price text for multiple capabilities.

T19.20 - Next run: add a Play Store restore/ownership sync pass on IAP panel open so previously purchased non-consumables are reflected even before new purchase callbacks.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `UnityIapPurchaseGateway.SyncOwnedPurchases()` to sweep initialized Unity IAP products for non-consumable receipts and unlock matching entitlements immediately.
  - Updated `IapPurchasePanel.Show()` to run ownership sync before rebuilding rows so already-owned items display as unlocked as soon as the panel opens.

T19.21 -  Initialize IAP once at app startup instead of lazily during purchase/price calls, so prices and products are reliably available when the UI loads.
and  Before calling purchase, verify the product exists and is available to avoid silent failures.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - `UnityIapPurchaseGateway` now initializes in `Awake()` with one-shot initialization guarding, rather than initializing inside purchase/price query code paths.
  - Purchase calls now validate store readiness, product existence, and `availableToPurchase` before initiating a transaction; invalid states log warnings and return `false`.
  - `IapPurchasePanel` fallback unlock path is now compiled only for non-Unity-IAP builds to avoid masking store configuration issues in purchasing-enabled builds.

T19.22 - Ensure that purchased items stay unlocked if the device is not online. The purchased state should also be saved in the playerprefs (encrypted to reduce hacking opportunities) and if the state is already unlocked and the device is offline, the unlocked state should still be correct. and the ulocked functionallity should work.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Entitlement persistence now stores encrypted payloads (`enc_v1:` AES-based encoding) in PlayerPrefs so unlocked product IDs are no longer kept as plain text.
  - Added backward-compatible migration so legacy plaintext entitlement saves are still loaded, then rewritten to encrypted format.
  - Added EditMode coverage validating encrypted persistence and legacy-to-encrypted migration behavior.

T19.23 - Next run: add integrity/tamper handling telemetry for encrypted entitlement loads so invalid payloads are detectable in QA logs without crashing entitlement resolution.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written

T19.24 — in app purchase implementation — secure store validation and debugging

## Goal

Ensure that purchases only succeed when connected to the real Google Play Store and prevent FakeStore or offline scenarios from unlocking premium features.

---

## Problem Statement

Current implementation allows purchases to succeed instantly in certain environments (e.g., FakeStore or unsupported devices). This creates a critical security issue where premium features can be unlocked without a real transaction.

---

## Implementation Requirements

### 1. Add Store Type Detection and Logging

**Objective:** Identify which store backend Unity IAP is using.

**Tasks:**

* During IAP initialization, detect the active store
* Log the detected store type
* Log all registered products and their availability status

**Expected Logs:**

* "IAP initialized"
* "Store: GooglePlay" OR "Store: Fake"
* "Product: <id> | availableToPurchase: true/false"

---

### 2. Track Store Type at Runtime

**Objective:** Make store type accessible for validation checks.

**Tasks:**

* Store the detected store type in a runtime variable
* Expose a public read-only property (e.g. IsUsingRealStore)

**Rules:**

* Only Google Play counts as a valid store
* All other stores must be treated as invalid for purchases

---

### 3. Block Purchases on Invalid Store

**Objective:** Prevent purchases when not connected to Google Play.

**Tasks:**

* Before initiating a purchase, check store type
* If not Google Play:

  * Do not call purchase API
  * Log a warning
  * Return failure

**Expected Log:**

* "Purchase blocked: not connected to Google Play store"

---

### 4. Validate Product Before Purchase

**Objective:** Ensure product exists and is purchasable.

**Tasks:**

* Verify store controller is initialized
* Verify product exists
* Verify product is available to purchase

**Failure Handling:**

* Do not initiate purchase
* Log clear reason for failure

---

### 5. Protect Unlock Logic (Critical)

**Objective:** Prevent FakeStore from unlocking premium features.

**Tasks:**

* Inside purchase success handler (ProcessPurchase):

  * Check store type
  * If NOT Google Play:

    * Do NOT unlock anything
    * Log warning
    * Exit early

**Expected Log:**

* "Purchase ignored: FakeStore detected, unlock blocked"

---

### 6. Add Initialization State Guard

**Objective:** Prevent actions before IAP is ready.

**Tasks:**

* Ensure purchases cannot be triggered before initialization completes
* If attempted:

  * Block action
  * Log warning

---

### 7. Improve Price Debugging

**Objective:** Make product loading issues visible.

**Tasks:**

* When price lookup fails, log reason:

  * product not found
  * metadata missing
  * store not initialized

---

## Acceptance Criteria

* Purchases are ONLY possible when connected to Google Play
* FakeStore does NOT unlock any features
* Offline purchases do NOT succeed
* All invalid purchase attempts are logged clearly
* Product availability is logged during initialization
* No silent failures remain in purchase flow

---

## Definition of Done

* System tested on:

  * Google Play test track (real store)
  * Offline mode
  * Unsupported / FakeStore device
* Verified that:

  * Only real purchases unlock features
  * FakeStore cannot bypass monetization

---

## Notes

* This is a critical security ticket and must be completed before release
* Do not rely on UI to enforce restrictions — all checks must exist in core logic
* This system must remain compatible with offline entitlement usage after a valid purchase

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Hardened `UnityIapPurchaseGateway` with runtime store-backend detection (`ActiveStoreBackend`, `ActiveStoreName`, `IsUsingRealStore`) and explicit initialization/product availability logging to surface FakeStore/offline scenarios.
  - Purchase flow now blocks non-Google Play store transactions before `InitiatePurchase`, keeps initialization/product availability guards, and prevents entitlement unlock in `ProcessPurchase` unless the active store is Google Play.
  - Added detailed price lookup diagnostics for uninitialized store, missing products, and missing metadata so panel pricing failures no longer fail silently.


T19.25 - Refund handling: 
Here’s a **clean implementation spec** you can hand off to your agent. It’s tailored to your existing architecture (CapabilityService, SyncOwnedPurchases, encrypted PlayerPrefs, etc.) and adds **online validation + offline fallback** without breaking your current flow.

---

# 🧾 Goal

Implement a **hybrid entitlement system**:

* ✅ Works offline using cached entitlements
* ✅ When online, validates purchases via backend
* ✅ Automatically **revokes refunded purchases**
* ✅ Keeps your existing Unity IAP flow intact

---

# 🧱 High-Level Architecture

### Current (you already have)

* Unity IAP purchase flow
* Local entitlement storage (CapabilityService + encrypted PlayerPrefs)
* `SyncOwnedPurchases()` (but only adds, doesn’t remove)

---

### Add this

**New components:**

1. **Receipt Parser (client)**
2. **Validation Manager (client)**
3. **Backend Validation API**
4. **Entitlement Sync (bidirectional)**

---

# 🧩 1. Receipt Parsing (Client)

### Purpose

Extract `purchaseToken` from Unity IAP receipt.

### Implementation

Create utility:

```csharp
public static class GooglePlayReceiptParser
{
    public static string ExtractPurchaseToken(string receiptJson)
    {
        if (string.IsNullOrEmpty(receiptJson))
            return null;

        var wrapper = JsonUtility.FromJson<ReceiptWrapper>(receiptJson);
        var payload = JsonUtility.FromJson<PayloadWrapper>(wrapper.Payload);
        var purchaseData = JsonUtility.FromJson<PurchaseData>(payload.json);

        return purchaseData.purchaseToken;
    }

    [Serializable]
    private class ReceiptWrapper
    {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    [Serializable]
    private class PayloadWrapper
    {
        public string json;
        public string signature;
    }

    [Serializable]
    private class PurchaseData
    {
        public string purchaseToken;
        public string productId;
    }
}
```

---

# 🌐 2. Backend Validation API

### Purpose

Authoritative source of truth using
Google Play Developer API

---

## Endpoint

```
POST /validatePurchase
```

### Request

```json
{
  "productId": "premium_upgrade",
  "purchaseToken": "token_here"
}
```

### Server Logic

* Call Google API:

  ```
  purchases.products.get
  ```

* Evaluate:

  * `purchaseState == 0` → valid
  * `purchaseState != 0` → invalid/refunded

---

### Response

```json
{
  "productId": "premium_upgrade",
  "valid": true
}
```

---

# 🔄 3. Validation Manager (Client)

### Purpose

Handles online sync and entitlement correction.

---

## New Class

```csharp
public class PurchaseValidationManager : MonoBehaviour
{
    public float validationIntervalHours = 24f;

    private const string LastValidationKey = "last_validation_time";

    public void TryValidatePurchases()
    {
        if (!IsOnline())
            return;

        if (!ShouldValidate())
            return;

        StartCoroutine(ValidateAllPurchases());
    }

    private bool ShouldValidate()
    {
        long lastTicks = LoadLastValidationTicks();
        DateTime last = new DateTime(lastTicks);

        return (DateTime.UtcNow - last).TotalHours >= validationIntervalHours;
    }
}
```

---

## Core Validation Logic

```csharp
private IEnumerator ValidateAllPurchases()
{
    var validProducts = new HashSet<string>();

#if UNITY_PURCHASING
    var products = _storeController.products.all;

    foreach (var product in products)
    {
        if (product.definition.type != ProductType.NonConsumable)
            continue;

        if (!product.hasReceipt)
            continue;

        string token = GooglePlayReceiptParser.ExtractPurchaseToken(product.receipt);

        yield return StartCoroutine(ValidateWithServer(product.definition.id, token, (isValid) =>
        {
            if (isValid)
                validProducts.Add(product.definition.id);
        }));
    }
#endif

    CapabilityService.Instance.SyncEntitlements(validProducts);

    SaveValidationTime();
}
```

---

## Server Call

```csharp
private IEnumerator ValidateWithServer(string productId, string token, Action<bool> callback)
{
    var request = new ValidationRequest
    {
        productId = productId,
        purchaseToken = token
    };

    string json = JsonUtility.ToJson(request);

    using (UnityWebRequest www = UnityWebRequest.Post("https://yourapi.com/validatePurchase", json))
    {
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            callback(false);
            yield break;
        }

        var response = JsonUtility.FromJson<ValidationResponse>(www.downloadHandler.text);
        callback(response.valid);
    }
}
```

---

# 🔁 4. Fix Your Entitlement Logic (CRITICAL)

### Replace your current one-way unlock logic

## New method (you must implement this)

```csharp
public void SyncEntitlements(HashSet<string> validProducts)
{
    var current = GetUnlockedProductIds();

    // 🔴 REVOKE missing ones
    foreach (var unlocked in current)
    {
        if (!validProducts.Contains(unlocked))
        {
            RevokeProduct(unlocked);
        }
    }

    // 🟢 ADD valid ones
    foreach (var valid in validProducts)
    {
        UnlockProduct(valid);
    }
}
```

---

# 🔌 5. Integration Points

### Modify your existing flow:

## On app start

```csharp
SyncOwnedPurchases(); // fast local sync
validationManager.TryValidatePurchases(); // async correction
```

---

## After purchase

Inside `ProcessPurchase`:

```csharp
UnlockProduct(productId);

// trigger validation soon after
validationManager.TryValidatePurchases();
```

---

# 📡 6. Offline Behavior

### When offline:

* Use cached entitlements ✔
* Skip validation ✔

### When back online:

* Validation runs ✔
* Refunded items get revoked ✔

---

# 🧠 7. Important Rules

### NEVER:

* Trust `hasReceipt` alone ❌
* Store permanent entitlement without revalidation ❌

---

### ALWAYS:

* Sync entitlements BOTH directions ✔
* Validate periodically ✔
* Use backend as source of truth ✔

---

# 🧪 8. Testing Scenarios

Your agent should verify:

### Case 1 — Normal purchase

* Buy → unlock → validate → stays unlocked

### Case 2 — Refund WITHOUT revoke (your current issue)

* Refund externally
* Open app online
* Validation runs
* ❗ entitlement gets removed

---

### Case 3 — Offline refund

* Refund
* Open app offline → still unlocked
* Go online → gets revoked

---

# ✅ Final Result

You now have:

* ✅ Offline-capable purchases
* ✅ Automatic refund correction
* ✅ Protection against abuse
* ✅ Minimal changes to your architecture

---

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added bidirectional entitlement support (`MarkLocked`, `RevokeProduct`, `SyncEntitlements`) so refunded/missing products are revoked while valid products remain unlocked and persisted for offline usage.
  - Added `GooglePlayReceiptParser` and `PurchaseValidationManager` to extract Google purchase tokens and periodically validate owned receipts against a backend endpoint when online.
  - Updated `UnityIapPurchaseGateway` to expose owned non-consumable receipts, run local ownership sync after IAP initialization, and trigger async validation after initialization and successful purchases.
  - Added EditMode coverage for entitlement revocation/sync behavior and receipt parsing.

T19.26 - Implement **graceful revocation system**:

* If backend says `revoked: true`
* Do NOT immediately remove access
* Instead:

  * mark entitlement as **pending revocation**
  * remove it on next app startup
  * show user popup

---

## 🧩 1. Extend validation response

Update your Unity model:

```csharp
[Serializable]
private class ValidationResponse
{
    public string productId;
    public bool valid;
    public bool revoked; // NEW
}
```

---

## 🧩 2. Track pending revocations

Add storage:

```csharp
const string PendingRevocationsKey = "iap_pending_revocations";
```

Store as:

* JSON list of productIds

---

## 🧩 3. Modify validation handling

Inside:

```csharp
ValidateWithServer(...)
```

Change logic:

```csharp
if (response.revoked)
{
    AddPendingRevocation(productId);
}
else if (response.valid)
{
    validProducts.Add(productId);
}
```

---

## 🧩 4. Apply revocations on startup

Create method:

```csharp
public void ApplyPendingRevocations()
{
    var pending = LoadPendingRevocations();

    foreach (var productId in pending)
    {
        CapabilityService.Instance.RevokeProduct(productId);
    }

    ClearPendingRevocations();

    if (pending.Count > 0)
    {
        ShowRevocationPopup(pending);
    }
}
```

---

## 🧩 5. Call on app start

In your bootstrap:

```csharp
validationManager.ApplyPendingRevocations();
```

---

## 🧩 6. Popup requirement

Implement:

```csharp
void ShowRevocationPopup(List<string> revokedProducts)
```

Message example:

> “Some purchases were refunded and have been removed.”

Keep it simple — no technical jargon.

---

## 🧩 7. Device ID integration

When sending validation request:

```csharp
string deviceId = SystemInfo.deviceUniqueIdentifier;
```

Add to request:

```csharp
public string deviceId;
```

---

## 🧩 8. DO NOT change

Keep:

* your offline unlock system
* your encrypted PlayerPrefs
* your existing purchase flow

---

# ✅ Final result

You now have:

* ✅ Cached validation (fast + scalable)
* ✅ Refund detection (even without revoke)
* ✅ Graceful UX (no sudden loss mid-session)
* ✅ Offline support intact
* ✅ Anti-sharing protection





---


- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Extended `PurchaseValidationManager` validation request/response payloads with `deviceId` and `revoked` handling.
  - Added deferred-revocation persistence (`iap_pending_revocations`) so revoked products are queued, applied on next startup, then cleared.
  - Added startup revocation application flow (`ApplyPendingRevocations`) and a user-facing popup/log message for revoked purchases.
  - Added EditMode coverage for pending-revocation application and deferred-sync behavior.

T19.27 - Find and fix a bug where the PurchaseValidationManager doesn't calculate the validation time correctly and never does the validation. Currently, the last validation time returns a time one second before the UtcNow and never starts the validation. The expected behavior is to store the correct last validation time. If no previous validation time is present, validation should be preformed. use this kind of implementation:
```csharp
private bool ShouldValidate()
{
    long lastUnixSeconds = SaveLoadSettings.LoadLong(LastValidationUnixKey, 0L);
    if (lastUnixSeconds <= 0)
        return true;

    DateTime lastValidationUtc = DateTimeOffset.FromUnixTimeSeconds(lastUnixSeconds).UtcDateTime;
    double hoursSinceLastValidation = (DateTime.UtcNow - lastValidationUtc).TotalHours;
    return hoursSinceLastValidation >= validationIntervalHours;
}

private void SaveValidationTime()
{
    long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    SaveLoadSettings.SaveLong(LastValidationUnixKey, unixSeconds);
}
```

Also add Debug.log statements for each step including a "next validation time" log

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed `PurchaseValidationManager` validation timestamp persistence/reads to use Unix seconds (`iap_last_validation_unix`) so validation interval checks compare compatible UTC times.
  - Updated `ShouldValidate` to correctly validate on first run (no saved timestamp), recover from invalid saved values, and emit step-by-step debug logs including the computed next validation UTC time.
  - Updated timestamp saves to log and persist accurate UTC Unix seconds, plus added EditMode regression coverage for no-timestamp, within-interval, and stale-timestamp validation decisions.

T19.28 Ensure Unity app sends purchase tokens and a unique device ID to the Cloudflare Worker, allowing multi-device entitlement for non-consumable IAPs while detecting refunds/revocations.

check the worker.js file in the Backend folder to verify that the implementation will work

Steps for Unity Implementation
Generate a unique device identifier
Use SystemInfo.deviceUniqueIdentifier for each device.
This deviceId will be sent to the Cloudflare Worker alongside the purchase token.
Capture purchase tokens
After each successful purchase via UnityIAP or similar:
Extract the purchase token from the receipt (e.g., via GooglePlayReceiptParser.ExtractPurchaseToken(receiptJson)).

Send purchase validation request to the worker

Endpoint: Your deployed Cloudflare Worker URL
POST body JSON:
{
    "productId": "<iap_product_id>",
    "purchaseToken": "<iap_purchase_token>",
    "deviceId": "<unique_device_id>"
}

Handle validation response

Expected response JSON:
{
    "productId": "<iap_product_id>",
    "valid": true/false,
    "revoked": true/false,
    "deviceIds": ["deviceId1", "deviceId2", ...]
}
Logic:
If valid = true → unlock entitlement for this device.
If revoked = true → mark entitlement as revoked and notify the user.
Optionally, store the validated productId in the Entitlement Store to persist access.
Update Entitlement Store

For each valid purchase returned from the worker:

CapabilityService.Instance.UnlockProduct(productId);

For revoked purchases:

CapabilityService.Instance.RevokeProduct(productId);
Graceful fallback
If the worker cannot be reached (offline), rely on local cached entitlements.
On next startup, attempt validation again.
Testing
Test with multiple devices using the same Google account:
Install app on device A, make a purchase, validate → entitlement unlocked.
Install app on device B with same account, validate using the same token → entitlement should unlock.
Refund a purchase in Google Play → validation should return revoked = true.

This approach ensures multi-device access, refund/revocation detection, and graceful handling of offline devices, all integrated with your current CapabilityService and EntitlementStore.

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - `PurchaseValidationManager` now resolves and reuses a stable device identifier per install, falling back to a persisted generated ID when `SystemInfo.deviceUniqueIdentifier` is unavailable/invalid so every validation payload includes a reliable `deviceId`.
  - Validation POST payload logging and response guards were added (`productId` mismatch check + `deviceIds` visibility warning) to make cross-device entitlement syncing issues observable during QA.
  - Confirmed and updated `Backend/worker.js` response semantics so `revoked` is only true for Google Play canceled/refunded purchases (`purchaseState === 1`) while still returning `deviceIds` for multi-device tracking.


T19.29 - Add a "Consumable" checkmark to the "Capability Definition" Scriptable object and implement the logic that so that when that checkmark is checked. The system doesn't store any entitlement, but does consume the IAP directly. It does store an incremented integer to keep track on the amount of consumables that are purchased. There is no need for checking in witht the validation system.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `consumable` to `CapabilityDefinition` and updated runtime unlock checks so consumable capabilities never resolve via entitlement unlock state.
  - `UnityIapPurchaseGateway` now registers per-product Unity IAP product types (Consumable vs NonConsumable), records consumable purchases as quantity counts, and only triggers server validation flow for non-consumables.
  - `EntitlementStore` now tracks/persists consumable purchase counts separately (`iap.consumables`) while keeping entitlement storage for non-consumables.
  - Updated IAP panel UI status to display consumable purchase count and keep consumable purchase buttons available.


T19.30 - Find and fix a bug where the popup that is shown when a purchase is refunded results in the settings ui to become unresponsive when the popup is dismissed. Currently the popup is shown if the system finds out that a purchase is invalid after a refund. When the user presses cancel, the rest of the ui becomes unresponsive. 
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `Popup` lifecycle handling so modal navigation blocking is only applied while the popup is actually open and is automatically released when the popup panel is hidden externally (for example, by a cancel button wired to `SetActive(false)` instead of `Popup.Close()`).
  - Added EditMode coverage in `PopupTests` for the external-hide dismissal path to ensure blocked `UI_DpadNavigationController` instances are restored and settings navigation remains responsive.

T19.31 - Find and fix a bug where the ui shows the wrong popup content after a refund. Currently when i refund the custom gobo IAP, the revocation popup shows correctly, but also the Unlimited universes popup shows, while that IAP is not refunded. After closing the popup, the funcionality for unlimited universes still works, but the popup is not supposed to show with that content.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `PurchaseValidationManager` revocation popup copy to always overwrite both title and body text, preventing stale popup text from previously-opened capability dialogs.
  - Revocation body text is now built from the actually revoked product IDs (deduped), with capability display-title lookup fallback to product ID for clearer and more accurate user messaging.
  - Added EditMode coverage for revocation message copy generation (single-item wording + duplicate ID dedupe behavior).


T18.3 - rework UI_DpadNavigationController.cs so it works correctly and add a checkbox to allow/disallow horizontal and/or vetical navigation and/or wrapping. Currently Horizontal navigation doesn't work and verticle wrapping is buggy and not reliable.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Reworked directional navigation axis resolution so horizontal navigation functions reliably when enabled and both axes can be independently enabled/disabled.
  - Added separate vertical wrap support (`verticalWrap`) and improved wrap target selection so vertical wrapping is deterministic and no longer skips unpredictably.
  - Expanded EditMode test coverage for horizontal enable/disable behavior and vertical wrapping.


T18.4 - rework popup.cs so that it when the popup is enabled it blocks any onavgation and submitting and canceling for any other panels
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `Popup` to behave as a modal controller by disabling all active `UI_DpadNavigationController` instances outside the popup hierarchy while the popup is open, preventing underlying panels from receiving navigation/submit/cancel input.
  - Added restoration logic so previously blocked navigation controllers are re-enabled when the popup closes or is disabled.
  - Extended `PopupTests` with coverage that verifies background navigation controllers are blocked on open and restored on close.

T18.5 - Find and fix a bug where the UI_DpadNavigationController.cs OnEnable function doesn't work properly. When the Popup.cs script blocks the navigation of any other UI_DpadNavigationController script and the popup object gets disabled, the UI_DpadNavigationController doesn't let navigation work when its enabled again. When that script is disabled by hand in the editor and enabled again normal navigation is possible again, but not directly when the popup is closed using the "SetActive" method with a cancel button wired up. If this is expected behavior but not used correctly, explain how it is supposed to work under this ticket
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed shared Input System action lifecycle in `UI_DpadNavigationController`: action references are now reference-counted across multiple controllers, so disabling one controller (e.g. popup close path) no longer disables `navigate/submit` actions that active controllers still depend on.
  - Added EditMode regression test coverage for shared navigate-action ownership to ensure one controller disabling cannot break another controller's navigation.


T18.6 - rework UI_SettingsPanelToggle so it only is used to show the settings panel and remove the hide option when pressing the cancel/back button. Make the UI_DpadNavigationController handle the cancel/back button, so it doesn't hide the panel when the UI_DpadNavigationController component is inactive. Currently when the user presses the back/cancel button when a modal popup is open it also hides the settings panel
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Reworked `UI_SettingsPanelToggle` so it only listens for show/open input and no longer binds any cancel/back hide action, preventing modal popup cancel presses from closing the settings panel globally.
  - Added cancel/back handling to `UI_DpadNavigationController` through a dedicated cancel action + `onCancel` event so cancel logic now follows panel-local navigation ownership and only fires on active controllers.
  - Expanded EditMode coverage for the new cancel event behavior and updated settings-panel toggle tests to match show-only responsibility.


T18.2 - Add a popup.cs script that can be added as a component to any panel that is used as a popup. It handles the correct behavior of the back button and blocks any ui navigation for any underlying/inactive ui elements.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added reusable `Popup` component for panel-style dialogs with explicit `Open`/`Close`, back-button close handling (Input System + fallback keys), focus restore to previous selection, and navigation confinement to popup descendants.
  - Added `PopupTests` EditMode coverage for open/close visibility and selection handoff behavior.


T18.1 - Add help button in UI. The help button should open a dialog popup to explain how the app is supposed to work.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Split `WebUiPasswordProtection` into its own file so classes are no longer co-located in `SaveLoadSettings.cs`, and restored the expected legacy API surface (`SetProtectionEnabled`, `IsProtectionEnabled`, `GetPasswordForUnityUi`, `ClearPassword`) used by existing UI callers.
  - Added missing `SaveLoadSettings.FixtureNameKey` constant and restored `UI_DmxSettings.ApplyWebUiPasswordFromInput()` so password helper and fixture-name persistence compile again.

T17.1 - Rework the ui idea. abandon the webview approach. The webui will only be used on external devices and the in-app ui will be unity ui. This ui still exists, But the UI_SettingsPanelToggle needs to be modified. and the InAppWebViewSurface is no longer needed. The UnityUi needs to use playerprefs and also the webui uses those, so settings nan be adjusted through both those ways. If a webui browser is open while settings are changed inside the app that should directly update the webui and alse vise versa.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Removed `InAppWebViewSurface` usage and scene wiring so in-app settings now rely purely on Unity UI while LAN WebUI remains served by `LocalWebUiServer`.
  - Updated `webui.html` with periodic `/api/settings` refresh to keep external browser views synchronized with live PlayerPrefs changes coming from Unity UI or other clients.

T17.2 - Implement the new Unity Input System. The old input system was used before in UI_DpadNavigationController, but it needs the system needs to work with the new Unity Input System. So the navigation works with a TV-remote, but also with other input methods. Find out how and fix it.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Migrated `UI_DpadNavigationController` to Unity Input System actions (`navigateAction` + `submitAction`) while preserving fallback support for keyboard/gamepad/legacy input paths.
  - Added directional-input handling that prioritizes vertical navigation and optionally allows horizontal traversal when `horizontalWrap` is enabled, so TV remote D-pad, keyboard, and gamepad flows behave consistently.
  - Expanded EditMode tests with explicit coverage for vector-based navigation behavior (vertical movement, horizontal ignored without wrap, horizontal movement with wrap).



T17.3 - The legacy input path is not needed at all anymore. Currently the UI_DpadNavigationController still needs the Selectables[] to function, however it should work without those. The buttons need to be selectable through the new input system purely based on their position on screen. Currently there is also a bug when a button is pressed it gets pressed 3 times instead of once.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Removed all legacy/keyboard polling paths from `UI_DpadNavigationController` so menu navigation now runs only from Input System action callbacks.
  - Added automatic selectable discovery when no inspector list is configured, sorting by on-screen position and navigating to nearest candidate in the requested direction.
  - Added same-frame submit de-duplication to prevent repeated button activation from overlapping submit triggers.
  - Expanded EditMode tests to cover auto-discovery navigation behavior and submit de-duplication.

T17.4 - Make the unity Ui update when the webui changes a setting. This can be done by making the SaveLoadSetting script fire an event when settings are saved and the webui updates using a callback
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T17.4a - The app now shows a white frame when a setting is changed. Make the system that prevents that so that the white flash is not visible when the universe, dmx channel or mode is changed.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T17.5 - Make the Webui Name field react only after it is completely typed and submitted. Currently it doesn't make typeing a name easy because the player pref gets saved to quickly and the webui reloads during typing.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  
T17.6 - Add Fixture name and IP address in the Unity UI and in the webui.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T17.7 - Make the webui mobile friendly. The scaling is not good on telephone
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T17.8 - Make the password function working
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written


T17.8a - Store a hashed password using sha256 and make a password handling class with a input panel field inside the UnityUi and a toggle to enable/disable password protection from the UnityUI. Default this toggle to be off and save its state in user prefs. The password can only be set and changed in the UnityUI and the webui locks up when the password is not provided. And once its authorized, the password field should dissapear. When the password is toggled or changed in the Unity UI the webui should respond accordingly. The webui should be able to cache the password in the browser.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added `WebUiPasswordProtection` with SHA-256 hashing, legacy plaintext migration, and persisted password-enabled toggle state in PlayerPrefs.
  - Updated `LocalWebUiServer` login flow to validate against hashed passwords and gate auth only when password protection is enabled + configured.
  - Updated WebUI lock/unlock behavior to cache authorization in browser storage and react to Unity-side password toggle changes via `/api/settings` refresh.

T17.8b - Make the webui react directly when a password is enabled or changed. Currently, when the webui is already loaded and a password is enabled, the webui is still functional until the page is refreshed. The enable password toggle needs to directly tell the webui to refresh/reload and show the login screen. Currently the reverse already works, so whe  the login page is shown because the the password toggle is enabled and the user disables the password in the unity ui, the webui automatically leaves the login screen and opens the main settings.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Split `WebUiPasswordProtection` into its own file so classes are no longer co-located in `SaveLoadSettings.cs`, and restored the expected legacy API surface (`SetProtectionEnabled`, `IsProtectionEnabled`, `GetPasswordForUnityUi`, `ClearPassword`) used by existing UI callers.
  - Added missing `SaveLoadSettings.FixtureNameKey` constant and restored `UI_DmxSettings.ApplyWebUiPasswordFromInput()` so password helper and fixture-name persistence compile again.

T17.9 - Redesign the Unity UI to look more professional
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written

T16.1 - The in-app menu is not shown inside the app, but that might require to write a webviewer or if it is aleady written it is a bug that needs to be fixed.
Currently on app start, the app is opening the settings menu, which gives a black screen now and requires the user to press the back button. The app should not open the settings menu on app start. The required behavior is for the app to start running normally and only show the settings menu when the user opens it.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added explicit startup visibility control in `UI_SettingsPanelToggle` via `showMenuOnStart` (default false), so startup now keeps the settings menu hidden unless intentionally enabled.
  - Added EditMode coverage for startup menu visibility behavior (`showMenuOnStart` true/false) to prevent regressions where the app boots into the menu.

T16.2 - Create a webviewer that can show the webui on android inside the app. It should be able to be an overlay with transparency and have an option to be resized in the editor so it is not only a fullscreen overlay
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Extended `InAppWebViewSurface` with configurable transparent overlay behavior and normalized overlay position/size controls so Android WebView can be shown as a non-fullscreen overlay.
  - Added Android layout parameter generation that maps inspector overlay values into pixel margins/sizes at runtime, while preserving Editor URL-preview workflow.

T16.3 - Make sure that the settings menu is never shown on startup. Remove the option to show it at startup. It will only be shown when the user wants to see it and never defaults to be shown at startup.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Removed startup configurability from `UI_SettingsPanelToggle`; startup now always forces settings menu hidden.
  - Updated EditMode tests to cover the new fixed startup-hidden behavior and removed obsolete configurable-startup expectations.

T16.4 - Next run: add focused EditMode coverage for `InAppWebViewSurface` overlay clamping/layout calculations and validate transparency behavior on an Android device build.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T15.1 - Refactor the code so that we have a SaveLoadSettings.cs class that handles all the saving and loading of the playerprefs. Any class that needs to handle saving ofr loding of playerprefs needs to use this class. This makes for cleaner code and makes it easier to debug.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Added `SaveLoadSettings` as a single PlayerPrefs gateway and centralized all DMX/mode/web settings keys in one shared class.
  - Refactored `WebUiSettingsStore`, `UI_DmxSettings`, `UI_FixtureMeshManager`, and `UI_FixtureModeSelector` so all runtime save/load operations route through `SaveLoadSettings`.
  - Verified no direct `PlayerPrefs` usage remains under `Assets/Scripts` besides `SaveLoadSettings`.

T15.2 - Next run: add EditMode unit tests for SaveLoadSettings integration across DMX UI, fixture mode, and WebUI settings persistence paths.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written



T14.1 - Rework the ui system. The new ui system will be an html page. This page will be accessable as a settings menu in the app and also we be on a webserver that the app exposes on the local network. The new input system in unity will be used to navigate through the menu. The menu gets shows when the user presses the OK button and can be hidden again using the back button. The first version of the html page is in Assets/WebUI/webui.html. the settings need to be retreived from the player prefs and saved to player prefs again.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added local HTTP settings endpoints (`/api/settings`) and PlayerPrefs bridge classes to load/save web UI settings and apply DMX/mode/fixture updates at runtime.
  - Updated `UI_SettingsPanelToggle` to support Unity Input System show/hide actions (OK/back behavior) with keyboard/gamepad fallbacks.
  - Updated `webui.html` to load persisted settings from API, save back through API, align universe input as 1-based UI, and add arrow-key focus navigation for remote-friendly control.
  - Fixed request-loop instability by marshalling `/api/settings` PlayerPrefs + Unity object access back to Unity main thread from the web server worker thread, and improved exception diagnostics for startup/request failures.
  - Updated fallback show/hide handling in `UI_SettingsPanelToggle` to use Input System devices directly so menu toggling still works when the legacy Input Manager is disabled.

T14.2 - fix a bug that gives this error when trying to access the webui from a browser: LocalWebUiServer request loop hit an exception: get_bytes can only be called from the main thread.
Constructors and field initializers will be executed from the loading thread when loading a scene.
Don't use this function in the constructor or field initializers, instead move initialization code to the Awake or Start function.
UnityEngine.Debug:LogWarning (object)
LocalWebUiServer:ServerLoop () (at Assets/Scripts/WebUI/LocalWebUiServer.cs:124)
System.Threading.ThreadHelper:ThreadStart ()
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Cached `webUiHtml` as UTF-8 bytes on Unity's main thread during `Awake` and served the cached payload from the HTTP worker thread to eliminate `get_bytes can only be called from the main thread` request failures.


T14.3 - Wire the in-app HTML rendering surface (WebView integration) so the same `Assets/WebUI/webui.html` menu can be displayed directly inside the Unity app panel while still available over local network.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added `InAppWebViewSurface` Android WebView integration that loads the local server URL (`127.0.0.1`) so the same `webui.html` runs in-app and via LAN browser.
  - Wired `UI_SettingsPanelToggle` visibility changes to show/hide the in-app WebView alongside the existing Unity panel lifecycle.

T14.4 - Create functionallity to make the InAppWebViewSurface.cs also work in the Unity editor for debugging.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added Unity Editor debug preview support in `InAppWebViewSurface` with optional external-browser launch and explicit preview URL logging while preserving Android WebView behavior.
  - Added `GetWebUiUrl()` helper so editor/debug tooling and tests can verify the exact in-app URL that should be loaded.


T14.5 - Add saving to player prefs to the webui settings panel.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Hardened the settings save flow in `webui.html` by awaiting `/api/settings` POST responses and rehydrating sanitized persisted values returned by the PlayerPrefs-backed API before showing success.
  - Aligned Web UI fixture amount input clamping (`1-16`) with PlayerPrefs/runtime limits to prevent mismatched values from being displayed after save/load cycles.

T14.6 - I want the settings menu to be more responsive. So the settings should be sent directly without the need for the "Save Settings" button. The settings should be saved intot the playerPrefs everytime any setting is changed.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Removed the explicit save button flow from `webui.html` and added debounced auto-save handlers that POST `/api/settings` whenever any settings field changes.
  - Added an in-page save status message (`Saving…` / `Saved` / `Save failed`) and centralized UI rehydration from persisted API responses so clamped/sanitized PlayerPrefs values are reflected immediately.

T14.7 - Fix a bug where the universe number is not saved in playerprefs when changed in the webui. Also check if any other settings are not being saved in playerprefs
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed WebUI settings application flow so universe/start channel updates are now always applied to the primary `ArtNetReceiver` before fixture address sync, ensuring universe changes persist and propagate to spawned fixtures.
  - Added explicit `UI_FixtureMeshManager.SetPrimaryReceiverAddressFromUserInput(...)` API to avoid address sync using stale primary receiver values after `/api/settings` save.

T14.8 - Modify the web ui and settings logic so that only the "standard/surface" mode shows the "Fixture Amount" part of the ui and wire up the logic for the other modes. The other modes only allow 1 fixture, so when another mode is selected, there should only be 1 instance of the fixture object. When the "Surface" mode is selected again, and an amount > 1 is in the playerprefs, the missing instances should again be created. This feature used to work before switching to the webui, but no longer works in the webui
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Updated `webui.html` mode visibility logic so only Surface shows the fixture amount section, while Pixel-only controls are shown exclusively in Pixel Mapping mode.
  - Updated `WebUiSettingsBridge.ApplySettings(...)` to force fixture count to `1` for non-Surface modes and to apply requested fixture amount only for Surface mode, restoring expected mode-dependent instance behavior when using WebUI.
  - Added EditMode coverage for the WebUI bridge path to verify non-Surface mode enforces one fixture while still applying universe/start-channel settings.


T14.7a - The DMX Universe value and DMX Start Channel still don't persist on consecutive app starts. Other settings are retained, like fixture mode and fixture amount, but the Universe value and DMX channel are always 2 and 1 respectivly. Find out why and fix it.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Fixed persistence drift by syncing `UI_DmxSettings` from the live `ArtNetReceiver` before writing PlayerPrefs, so externally-applied WebUI address changes are no longer overwritten with stale inspector defaults on disable/pause.
  - Corrected universe label rendering to keep the WebUI/UIDMX address model consistently 1-based for user-visible values.


T14.9 - Add integration coverage for `/api/settings` request handling to validate persisted payload rehydration and mode-specific fixture count behavior through LocalWebUiServer.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Refactored `LocalWebUiServer` settings API handling behind reusable request handlers and added an immediate execution path for deterministic EditMode integration tests.
  - Added EditMode coverage that exercises LocalWebUiServer `POST` + `GET` settings flow end-to-end, validating persisted payload rehydration and non-Surface fixture-count enforcement.
  
T14.10 - Work on the webui html code. 
* Remove the "Test DMX" Page.
* Remove the "Settings" Button. (The settings page is the only page)
* Remove the "Restart Fixture" button. 
* Fix the IP address text field so it shows the IP address correctly.
* Add a feedback button that links to "dilarium.es/dmx-projector/feedback"
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Simplified `Assets/WebUI/webui.html` into a single-page settings experience by removing the DMX Test page and top tab navigation.
  - Removed the Restart Fixture action and added a Feedback button linking to `https://dilarium.es/dmx-projector/feedback`.
  - Reworked IP display to a read-only text field that always shows a full URL using the current host IP + active port (for clearer copy/share behavior).

T14.11 - Fix the IP address inside the webui. The IP address should show the local IP inside the network. Currently it shows 0.0.0.0 but it should retreive the local IP address from the IPSolver.cs script and display that IP address in the webui
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed `WebUiSettingsStore.Sanitize` so `ipAddress` survives JSON serialization/deserialization instead of being reset to the `0.0.0.0` default.
  - Updated `LocalWebUiServer` to use `IpSolver.ResolveLocalIpv4Address()` directly for `/api/settings` payloads so WebUI IP display is sourced from the shared resolver.
  - Added EditMode coverage asserting `WebUiSettingsStore.ToJson`/`FromJson` preserve the resolved IP address field.



T19.32 - Find a d fix a bug where the validation logic wrongly finds an IAP to be invalidated when another IP is purchased. Curently after re-buying the Custom Gobo IAP, the popup for Unlimited Universes being locked popup shows up. After a restart of the app ,the Unlimited Universes IAP is validated correctly again. Try to simplify the validation logic and make sure the popup and locking/unlocking system works correctly.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Refactored validation reconciliation so only products actually validated in the current pass are eligible for lock/unlock changes, preventing unrelated entitlements from being revoked when a different purchase is revalidated.
  - Added `CapabilityService.SyncValidatedEntitlements(...)` and updated `PurchaseValidationManager` to track `validatedProducts` + `validProducts` separately for a safer, simpler sync model.
  - Added EditMode regression coverage proving a single-product validation pass no longer revokes other active entitlements.

T19.33 - Rework the validation system. Currently the IAP validation popup shows and tells incorrectly that certain purchases were refunded, but when the purchase panel is opened the purchases show correctly as unlocked and they also still work. The validation system should be simplified and fixed so that only once validation is really finding refunded IAPs it should show the popup. 
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [x] Tests Passed
- [x] Documentation Written
  - Simplified `PurchaseValidationManager` revocation flow to only show revocation popups when the validation endpoint explicitly returns `revoked=true` for a validated product.
  - Removed startup/pending revocation replay and suspicious no-receipt fallback revocation paths that could surface false-positive revoked popups while entitlements remained valid.
  - Updated validation result handling to track revoked products separately from currently valid products and added regression coverage for the revised `HandleValidationResult` behavior.

T19.34 - Next run: add EditMode tests for `PurchaseValidationManager.ValidateAllPurchases` covering mixed valid/revoked/invalid responses so popup and entitlement sync behavior is verified end-to-end.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T14.12 - Next run: verify WebUI IP address rendering on LAN clients across Wi-Fi reconnect / DHCP change scenarios.
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T99.1 - Find and fix the bug in which the Fixture Amount button that increases the fixture amount in the UI_FixureMeshManager.cs also increments the Universe on the ArtNetReveiver.cs script
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed ArtNet 1-based UI universe conversion so fixture cloning/address sync no longer drifts universes when fixture count changes.

T99.2 - Fix the Ui so that only the Standard/Surface mode shows the Fixture Amount UI panel and also make sure that the other modes only use 1 fixture instance.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added mode-aware fixture count UI visibility and forced single-instance behavior for Moving Head and Pixel Mapping modes.
  
T99.3 - Find out why the app doesn't show anything anymore since the implementation of the Movinghead and PixelMapping Modes. Artnet Data is being sent, soa that can not be the issue
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Resolved no-output regression by correcting universe persistence/display alignment between `UI_DmxSettings` and `ArtNetReceiver` (1-based UI, 0-based receiver).

T99.4 - Find out why the app doesn't show anything anymore since the implementation of the Movinghead and PixelMapping Modes. Artnet Data is being sent, so that can not be the issue. The resolve in ticket 9.3 did not fix it. 
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Investigated multiple no-output hypotheses and implemented/validated a robust fix: output controllers now rebind material instances when mode changes swap renderer shared materials at runtime, preventing stale material writes after switching between Standard/Moving Head/Pixel Mapping.

T99.5 - Make the Fixture amount a saved user pref, so that when the fixture amount is bigger than 1 and the mode is changed from standard mode to another mode and changed back after that, the original amount is retreived and displayed
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added non-destructive fixture rebuild APIs so non-Standard modes force one fixture without overwriting saved preference; returning to Standard restores the previously saved fixture amount and updates UI text accordingly.


T99.6 -Fix the bug where the playerprefs are not correctly loaded when the app restarts. Currently when i run the app in the Unity Editor, the universe number always defaults to the number 2. Check if the app startup works correct and overrule playerprefs over any other input on startup and make sure that the webui also gets populated from the playerprefs and not somehow has some bug where it loads in reverse. Also verify that the settings are properly saved when they are changed in the webui. Refactor the code if that helps to find and fix the bug. Document all the steps that you take under this ticket.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Moved DMX settings preference hydration to `Awake` and added startup/save guards so shutdown/disable writes cannot overwrite already-persisted universe/channel values before load completes.
  - Added regression coverage for LocalWebUiServer `/api/settings` POST+GET flow to confirm WebUI writes persist to PlayerPrefs and GET rehydrates the same universe/start-channel values.

T99.7 - Find and fix a bug where the settings menu webui over LAN on the android app only works when the settings menu is open in the app. The settings menu webui should always work over LAN, even when the app itself is running and is not in the menu itself. The in-app menu is also not shown inside the app, but that might require to write a webviewer, so that can be a new ticket.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added a `keepTargetObjectActiveWhenHidden` mode to `UI_SettingsPanelToggle` so hide/show events can keep background systems active while still toggling menu visibility signals.
  - Enabled this mode for the `HtmlUI` toggle in `MainScene` so closing the in-app menu no longer disables the `LocalWebUiServer` GameObject; LAN `/index.html` and `/api/settings` stay reachable while the app is out of menu view.
  - Added EditMode coverage for hide behavior to verify default hide semantics still work and the new keep-active mode preserves GameObject activation.


T99.8 - Refactor the code so that every class and every interface lives in its own file and fix these bugs: 

- Assets\Scripts\UI\UI_DmxSettings.cs(347,48): error CS0117: 'WebUiPasswordProtection' does not contain a definition for 'SetProtectionEnabled'

- Assets\Scripts\UI\UI_PasswordHelper.cs(44,28): error CS1061: 'UI_DmxSettings' does not contain a definition for 'ApplyWebUiPasswordFromInput' and no accessible extension method 'ApplyWebUiPasswordFromInput' accepting a first argument of type 'UI_DmxSettings' could be found (are you missing a using directive or an assembly reference?)

- Assets\Scripts\UI\UI_DmxSettings.cs(363,51): error CS0117: 'WebUiPasswordProtection' does not contain a definition for 'GetPasswordForUnityUi'

- Assets\Scripts\UI\UI_DmxSettings.cs(391,33): error CS0117: 'WebUiPasswordProtection' does not contain a definition for 'ClearPassword'

- Assets\Scripts\UI\UI_DmxSettings.cs(398,54): error CS0117: 'SaveLoadSettings' does not contain a definition for 'FixtureNameKey'

- Assets\Scripts\UI\UI_DmxSettings.cs(407,86): error CS0117: 'SaveLoadSettings' does not contain a definition for 'FixtureNameKey'

- Assets\Scripts\UI\UI_DmxSettings.cs(439,51): error CS0117: 'WebUiPasswordProtection' does not contain a definition for 'GetPasswordForUnityUi'

- Assets\Scripts\UI\UI_DmxSettings.cs(441,58): error CS0117: 'WebUiPasswordProtection' does not contain a definition for 'IsProtectionEnabled'

- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Split `WebUiPasswordProtection` into its own file so classes are no longer co-located in `SaveLoadSettings.cs`, and restored the expected legacy API surface (`SetProtectionEnabled`, `IsProtectionEnabled`, `GetPasswordForUnityUi`, `ClearPassword`) used by existing UI callers.
  - Added missing `SaveLoadSettings.FixtureNameKey` constant and restored `UI_DmxSettings.ApplyWebUiPasswordFromInput()` so password helper and fixture-name persistence compile again.


T99.9 - Find and fix a bug where the network warning still shows after network data is being received
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Fixed `UI_DmxSettings` network-warning visibility flow so the warning panel only appears when both (a) warning toggle is enabled and (b) the receiver is currently in a no-data state, preventing stale warning UI after data resumes.
  - Added safe ArtNet event subscribe/unsubscribe guards in `Awake`/`OnDestroy` and extracted `RefreshNetworkWarningVisibility()` to keep warning state deterministic across settings reloads.
  - Added EditMode coverage for the regression path (show -> hide) and for settings-load behavior so enabling the warning preference no longer forces the panel visible when there is no active warning condition.


T99.10 - Next run: execute Unity EditMode suite on a licensed Unity runner to validate T99.9 network-warning regression coverage in-engine
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T11.1 - Modify the UI_FixtureModeSelector.cs to not work with a dropdown object but to simply have public function to increment and decrement the current mode and cycle trough the modes. The ui will have simple + and - buttons connected to those functions and a text object will display the current mode
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Replaced dropdown-driven mode selection with text-based mode display and added `IncreaseMode`/`DecreaseMode` public button handlers that cycle through Standard, Moving Head, and Pixel Mapping modes.

T11.2 - Modify the UI_FixtureModeSelector.cs so that the grid size for the pixel mapping mode only shows when the Pixel Mapping mode is selected and make it work with increment and decrement buttons for X size and Y size in increments of 8 pixels and a maximum of 32 pixels on both dimensions.
- [x] Started
- [x] Behavior Written
- [x] Code Written
- [ ] Tests Passed
- [x] Documentation Written
  - Added conditional visibility for the pixel-grid controls container so grid settings only show in Pixel Mapping mode.
  - Updated pixel grid sizing logic to 8-pixel step increments with clamped bounds of 8..32 for rows and columns, compatible with +/- button workflows.

T11.3 - Next run: execute Unity EditMode suite on a licensed runner to validate T11.1/T11.2 selector cycling and pixel-grid UI behavior in-engine.
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

T8.10 - Modify the MaliSafeMovingHead.shader so that the rotation parameter only rotates the beam and not the whole shader. The X and Y offset directions now change with rotation. The behavior should be that the X and Y directions always relate to the screen X and Y coordinates and the rotation rotates the pattern inside the beam
- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written

T8.11 - Modify the MaliSafeMovingHead.shader so that the parameter that scales the pattern doesn't affect the beam size. Currently the sizing of the pattern influences also the total size, but the mask size should bne independent of the pattern scale.
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

T19.12 - Next run: wire IAP capability assets/database into MainScene and add locked-feature UI trigger (ticket 19.6) backed by capability IDs.

- [ ] Started
- [ ] Behavior Written
- [ ] Code Written
- [ ] Tests Passed
- [ ] Documentation Written
