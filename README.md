# Time Replay

Time Replay is a 2D puzzle-platformer built in Unity. The player controls an explorer trapped in another time-space dimension, where he gains the power to rewind time and create replay clones of his past actions.

To escape, the explorer must use those clones to press plates, open doors, cross hazards, and reach the final portal back home.

## Game Story

An explorer wakes inside a world outside his own time. The path home has disappeared, but the strange dimension gives him two impossible powers: time rewind and living echoes of his past actions.

Across three levels, he learns to cooperate with his own recorded movements. In the ending, he escapes the broken dimension and returns safely to his family.

## Features

- Three connected gameplay levels
- Main menu with animated player and cloud background
- Intro and ending story scenes
- Time recording and replay clone mechanics
- Pressure plates, doors, crates, traps, platforms, and portals
- Layered cloud backgrounds and custom pixel-art assets
- Walking/running player animation

## Controls

| Action | Input |
| --- | --- |
| Move | `A` / `D` or Arrow Keys |
| Jump | `Space`, `W`, or Up Arrow |
| Start/stop recording | `R` |
| Spawn recorded clones | `E` |
| Reset level | `Q` |
| Skip story text | `X` |

## Requirements

- Unity 2022.3.62f3c1
- 2D Unity project setup

## How to Run

1. Clone or download this repository.
2. Open the project folder in Unity Hub.
3. Use Unity 2022.3.62f3c1 or a compatible Unity 2022.3 LTS version.
4. Open Assets/Scenes/MainMenu.unity.
5. Press Play in the Unity Editor.

## Project Structure

Assets/
  Resources/Art/       Game sprites, backgrounds, platforms, portals, and traps
  Scenes/              Main menu, story scenes, and gameplay levels
  Scripts/             Player, clone, replay, platform, UI, and level systems
Packages/              Unity package manifest and lock file
ProjectSettings/       Unity project configuration



MainMenu -> StoryIntro -> SampleScene -> Level2 -> Level3 -> EndingStory -> MainMenu
