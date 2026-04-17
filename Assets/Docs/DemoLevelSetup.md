# DemoLevel_01 Setup

This guide builds one mandatory cooperation puzzle:
"Clone stands on left pressure plate while player runs to right plate to open the door."

## 1. Scene and physics

1. Create scene `DemoLevel_01` and save it under `Assets/Scenes`.
2. Add a floor and platforms using `BoxCollider2D`.
3. Add two pressure plates (`PlateLeft`, `PlateRight`) with `BoxCollider2D` as trigger.
4. Add one door object with `DoorController` and `BoxCollider2D`.
5. Add one goal object behind the door with `LevelGoal` and trigger collider.

## 2. Player prefab

1. Create `Player` object:
   - `Rigidbody2D`
   - `BoxCollider2D`
   - `PlayerController`
2. Tag it as `Player`.
3. Create a child object `GroundCheck` and assign it to `PlayerController.groundCheck`.
4. Assign ground layer to `groundMask`.
5. Make prefab at `Assets/Prefabs/Player.prefab`.

## 3. Clone prefab

1. Duplicate player as `Clone`.
2. Keep `PlayerController`, set its initial mode to `Replay` in inspector.
3. Add `ReplayCloneController`, reference its `playerController`.
4. Tag prefab as `Clone`.
5. Save prefab at `Assets/Prefabs/Clone.prefab`.

## 4. Recorder and session

1. Create empty object `GameSystems`.
2. Add components:
   - `ReplayRecorder`
   - `ReplaySessionController`
   - `GameManager`
3. Wire references:
   - `ReplayRecorder.targetPlayer` -> player in scene
   - `ReplaySessionController.livePlayer` -> player
   - `ReplaySessionController.recorder` -> ReplayRecorder
   - `ReplaySessionController.clonePrefab` -> clone prefab
   - `GameManager.sessionController` -> ReplaySessionController

## 5. Door and plate wiring

1. Add `PressurePlate` script to each plate.
2. Add `DoorController` script to door.
3. In both plates:
   - `onPressed` -> call `DoorController.OpenDoor`
   - `onReleased` -> call `DoorController.CloseDoor`
4. To require both plates, create a simple OR gate with two doors or use two-step puzzle:
   - PlateLeft opens access path
   - PlateRight opens final door

## 6. Goal and reset

1. Add `LevelGoal` on finish trigger.
2. Assign `gameManager`.
3. In `ReplaySessionController.resettableBehaviours`, add:
   - both `PressurePlate`s
   - `DoorController`
4. Press `T` during play mode to validate quick reset.

## 7. Expected test flow

1. Press `R` and record player going to PlateLeft.
2. Press `R` again to stop recording and spawn clone replay.
3. During clone replay, run current player to PlateRight.
4. Door opens; player reaches goal; console prints "Level cleared."
