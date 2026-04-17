# Build and Trailer Checklist

## Build

1. Open `File > Build Settings`.
2. Add `DemoLevel_01` to scene list.
3. Target platform: `Windows`.
4. Build folder suggestion: `Builds/Windows`.
5. Run a smoke test:
   - Start recording with `R`
   - Spawn clone with `R`
   - Quick reset with `T`
   - Clear the level

## Stability checks

- Verify clone always starts from the intended spawn point.
- Verify door state resets correctly after `T`.
- Verify only one active clone exists at a time.
- Verify no exceptions in Console while recording/replaying.

## 30-60 second trailer shot list

1. Show puzzle room overview (3s).
2. Show recording phase (5s).
3. Show clone replay and simultaneous player action (10s).
4. Show door opening from cooperation (5s).
5. Show level clear moment and quick reset (5s).
6. End card with project title and core mechanic text (5s).
