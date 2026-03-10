# DMX Projector App - Development Roadmap

## Overview
This roadmap outlines the development and release plan for the DMX Projector App. The focus is on a **freemium core** with **modular paid features** and **device unlock options**, designed for hobbyists, professionals, and rental companies. All milestones prioritize **user fairness**, **offline functionality**, and **future scalability**.

---

## 1. Core Freemium Launch
**Goal:** Release a fully functional free version to attract hobbyists and early testers.

- **Features:**
  - 3 devices allowed per account
  - 1 universe, 1 surface/video/gobo
  - Basic effects (corner pinning, simple pixel mapping, basic moving head)
  - Offline usage fully supported
  - Watchfolder / custom gobos limited to one file

- **Deliverables:**
  - Android APK ready for Google Play
  - Settings menu (start address, universe selection, surface count)
  - Basic webserver for local configuration
  - Device registration logic (limit 3 free devices)

- **Milestones:**
  - [ ] MVP functional on cheap Android projectors (Mali GPU)
  - [ ] DMX/sACN input working
  - [ ] Pixel mapping and moving head modes implemented
  - [ ] User-friendly settings menu

---

## 2. Modular Paid Features (from launch)
**Goal:** Offer users flexibility to unlock only the features they need.

- **Modules:**
  - Multi-universe support
  - Watchfolder / unlimited videos/gobos
  - Advanced visual effects (pixel waves, rotations, multi-surface effects)
  - sACN input/output
  - CITP / media server support

- **Implementation Notes:**
  - Tie purchases to Google account, all purchased modules unlock automatically on registered devices
  - Include clear feature matrix in app UI
  - Offer **bundled module purchase** for studios or pros

- **Milestones:**
  - [ ] In-app purchases implemented per module
  - [ ] Module unlock logic integrated with device cap
  - [ ] UI displays available and purchased modules
  - [ ] Testing of module purchases on multiple devices

---

## 3. Device Unlock System (from launch)
**Goal:** Scale usage for multi-projector users while protecting revenue.

- **Device Limits:**
  - 3 devices free
  - €5 per additional device
  - €30 unlimited devices

- **Implementation Notes:**
  - Track devices via unique device ID or lightweight backend
  - Lock premium modules on extra devices if not unlocked
  - Automatically unlock all purchased modules on paid devices

- **Milestones:**
  - [ ] Device registration logic implemented
  - [ ] Extra device IAP functionality
  - [ ] Unlimited device IAP functionality
  - [ ] Clear messaging in app about device limits and costs

---

## 4. Beta Testing Phase
**Goal:** Validate features, gather feedback, and test device/module unlocks.

- **Beta Scope:**
  - Freemium fully available
  - Full module unlock for selected beta testers
  - Device unlocks tested on multi-projector setups

- **Milestones:**
  - [ ] Recruit beta testers (Reddit, Discord)
  - [ ] Implement license keys or promo codes for beta testers
  - [ ] Collect structured feedback (modules, device cap, usability)
  - [ ] Adjust pricing, messaging, and UI based on feedback

---

## 5. Post-Launch Expansion
**Goal:** Grow revenue and add advanced features for professional users.

- **Future Modules / Features:**
  - NDI video input
  - Advanced automation / wave effects in pixel mapping
  - Cloud sync / remote webserver control
  - Commercial bundle for studios and rental companies

- **Marketing & Community:**
  - Create demo videos showcasing app modes
  - Leverage Reddit/Discord communities for adoption
  - Highlight freemium value for hobbyists and modular flexibility for pros
  - Promote device unlock options for rental companies

- **Milestones:**
  - [ ] Add new modules as paid IAPs
  - [ ] Introduce commercial bundles
  - [ ] Evaluate revenue and adoption metrics
  - [ ] Iteratively improve performance on low-end projectors

---

## 6. CI/CD & Update Strategy
**Goal:** Ensure smooth app updates while keeping offline-first functionality.

- Implement Play Store updates as primary mechanism
- Webserver check for updates via connected device
- Notify users to connect projector to internet if updates are available
- Optional: timer-based reminder if offline for long periods

- **Milestones:**
  - [ ] CI/CD pipeline setup (build, test, deploy)
  - [ ] Webserver update check integrated
  - [ ] User notifications for updates

---

## Notes / Design Principles
- **Offline functionality** always supported for freemium and premium
- **Fairness first:** no features are taken away from early users
- **Scalability:** modules and device unlocks allow monetization without limiting adoption
- **Simplicity:** UI clearly shows freemium vs purchased modules and device status
- **Future-proof:** roadmap allows adding professional features over time without disrupting existing users

---

## Timeline Example (Tentative)
| Phase | Target Completion |
|-------|-----------------|
| MVP Freemium + core DMX modes | Month 1–2 |
| Module IAP implementation | Month 2–3 |
| Device unlock system | Month 2–3 |
| Beta testing & feedback | Month 3–4 |
| First official release (Google Play) | Month 4 |
| Post-launch module expansion | Month 5+ |
