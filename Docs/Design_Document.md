# Android Projector Lighting Fixture
## Complete Design Document

### Target Device
Magcubic HY300 (Low-end Android projector)

---

## Design Philosophy

This application must behave like an embedded lighting appliance.

Priorities:
1. Stability
2. Low memory usage
3. Deterministic performance
4. 30 FPS fixed
5. Zero runtime allocations

---

## Core Features

- Art-Net (UDP port 6454)
- 512 channel DMX buffer
- RGB + Master Dimmer
- Procedural shader-based patterns
- 720p fixed internal resolution
- Optional video playback (720p H.264 only)

---

## Performance Constraints

- Built-in Render Pipeline only
- Single unlit shader
- No post-processing
- No URP
- No HDR
- Fixed RenderTexture 1280x720
- No dynamic allocations per frame

---

## DMX Personality (Basic Mode)

| Channel | Function |
|----------|----------|
| 1 | Master Dimmer |
| 2 | Red |
| 3 | Green |
| 4 | Blue |
| 5 | Pattern |
| 6 | Speed |
| 7 | Size |
| 8 | Strobe |

---

## Development Phases

Phase 1: ArtNet + RGB output  
Phase 2: Pattern system  
Phase 3: Media playback  
Phase 4: Optimization & stability  
Phase 5: Projection mapping (optional)

---

## Golden Rule

If performance and visuals conflict — choose performance.
