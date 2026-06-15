# CW3 Professionalism Portfolio

Student name: [Your name]  
Student ID: [Your Student ID]  
Project title: Temporal Paradox  
Repository: https://github.com/zhangduo822/Games-Programming

---

## 1. GitHub Repository Link

The project repository is available at:

https://github.com/zhangduo822/Games-Programming

I used GitHub as the main version-control record for the Unity project. The commit history shows the development of the gameplay prototype from the first working version through later fixes, visual improvements, additional scenes, and asset integration.

## 2. Development Log Showing Progress Over Time

The project developed in stages rather than as one final upload. My main progress log is summarised below.

| Date | Development activity | Evidence |
|---|---|---|
| 29 May 2026 | Built the initial Unity 2D prototype, including the player controller, recording system, replay clone system, pressure plates, doors, pushable crates, switchable platforms, timeline feedback, and the first scene setup. | Commit `15dc672`: `feat: Implement nested recording system, pushable crates, switchable platforms` |
| 1 June 2026 | Fixed the replay clone spawning and clone colour/visibility problems. I also moved the clone prefab into the Resources workflow so it could be loaded reliably at runtime. | Commit `7489436`: `fix: replay clone spawning and color` |
| 4 June 2026 | Adjusted clone spawning so that spawning clones did not incorrectly move or reset the live player's position. | Commit `ef0e10c`: `fix: keep player position when spawning clones` |
| 5 June 2026 | Tested player jump tuning and made an adjustment to jump force after gameplay review. | Commit `6765e55`: `fix: lower player jump force` |
| 5 June 2026 | Reverted the lower jump-force change after testing showed that it made platforming less suitable for the level layout. | Commit `f138fa7`: `Revert "fix: lower player jump force"` |
| 5 June 2026 | Improved replay clone spawning and added humanoid visual support so the player and clone could be read more clearly in-game. | Commit `3f0e369`: `update replay clone spawn handling and add humanoid visual` |
| 7 June 2026 | Fixed clone humanoid visuals and prefab setup after further testing showed inconsistency between the runtime clone and the intended visual design. | Commit `a10c0f5`: `fix clone humanoid visuals and prefab setup` |
| 13 June 2026 | Expanded the project with additional scenes, art assets, menu/story scenes, animated hazards, Level 2 and Level 3, UI animation scripts, and stronger visual polish. | Commit `346334a`: `feat: update Unity game project` |

This log shows that the project moved from a mechanical prototype to a more complete vertical slice. It also shows that I used testing and revision rather than only adding new features.

## 3. Summary of Important Commits

**`15dc672` - Initial gameplay systems**  
This was the most important foundation commit. It added the main gameplay scripts such as `PlayerController2D`, `ReplayRecorder2D`, `ReplayClone2D`, `CloneManager`, `PressurePlate2D`, `Door2D`, `Goal2D`, `PushableCrate2D`, `SwitchablePlatform2D`, `TemporalParadoxGame`, `TimelineVisualizer`, and `VisualFeedback`. This commit established the central time-clone puzzle loop.

**`7489436` - Replay clone spawning and colour fix**  
This commit fixed an important usability problem: the clone needed to spawn correctly and be visually readable. It also added the `Resources` structure and the `WhiteSquare` sprite fallback, which made prefab loading and sprite setup more reliable.

**`ef0e10c` - Keep player position when spawning clones**  
This was a small but important fix. During testing, clone spawning could interfere with the live player's position. Removing that behaviour made the game feel fairer because spawning a clone should not unexpectedly move the player.

**`6765e55` and `f138fa7` - Jump force test and revert**  
These commits show an example of testing and decision-making. I tried lowering the jump force, but after testing the result against the level layout, I reverted the change. This was useful because it showed that a change can be technically correct but still wrong for the game feel.

**`3f0e369` - Humanoid visual support**  
This commit added `SimpleHumanoidVisual` and improved how the player and clone appear in the game. This supported both clarity and presentation, because the player needs to quickly distinguish between the live character and replay clones.

**`a10c0f5` - Clone visual and prefab setup fix**  
This commit refined the clone prefab setup and clone visual behaviour. It fixed inconsistencies that appeared when the clone was spawned at runtime rather than only viewed in the editor.

**`346334a` - Full project update**  
This commit expanded the prototype into a stronger submission version. It added Level 2, Level 3, menu and story scenes, animated fire hazards, background/cloud assets, additional art, and several support scripts such as `MainMenuController`, `StoryIntroController`, `SpriteFrameAnimator2D`, `UIImageFrameAnimator`, and `HazardZone2D`.

## 4. Evidence of Planning and Task Management

My planning approach was based on breaking the project into systems and then testing them in playable levels. I did not try to build all content first. I first focused on the central mechanic: recording the player and replaying the recording as a clone. Once that worked, I added puzzle objects that could interact with the clone system.

The main task order was:

1. Build basic 2D player movement and reset behaviour.
2. Create the data structure for recorded frames and timelines.
3. Implement recording and clone playback.
4. Add puzzle interactions: pressure plates, doors, goals, crates, and switchable platforms.
5. Fix clone spawning and visual clarity issues.
6. Expand from one test scene into multiple levels.
7. Add menu, intro, ending, background, art, and hazard polish.
8. Test and fix issues found during play.

Git branches and pull requests also show task management. The repository includes merge commits for focused fixes, such as `fix/replay-clone-spawn-and-pink-color`, `fix/clone-spawn-player-position`, `fix/replay-clone-player-position`, and `fix/clone-humanoid-visual`. This shows that I treated fixes as separate tasks rather than mixing every change into one large commit.

## 5. Evidence of Response to Feedback

The project changed in response to testing, self-review, and feedback on playability. The clearest examples are:

- Clone readability was improved after it became clear that the player needed to distinguish the live character from replay clones quickly.
- Clone spawning was adjusted because spawning a clone should not disturb the live player's position.
- The jump force was tested and then reverted because the lower value made the platform layout less comfortable.
- The clone prefab setup was improved after runtime testing showed that editor setup and spawned clone behaviour were not always identical.
- Later levels added clearer hazards, doors, platforms, and background assets to improve readability and presentation.

These changes show that I did not treat the first version as final. I reviewed how the prototype behaved in play, then changed the implementation when the result affected the player experience.

## 6. Testing Log and Bug-Fixing Evidence

| Issue tested | Problem found | Fix or decision | Evidence |
|---|---|---|---|
| Replay clone spawning | Clone spawning and colour were not reliable enough for gameplay readability. | Updated clone spawning, clone colour, prefab loading, and fallback sprite resources. | Commit `7489436` |
| Player position during clone spawn | Spawning a clone could interfere with the live player's position. | Removed the behaviour that changed the player position during clone spawning. | Commit `ef0e10c` |
| Jump feel | Lower jump force was tested as a tuning change. | Reverted the change because it did not fit the level design. | Commits `6765e55` and `f138fa7` |
| Replay clone visual clarity | Clone visuals were not consistent enough when spawned from the prefab. | Added and refined humanoid visual handling and prefab setup. | Commits `3f0e369` and `a10c0f5` |
| Multi-level progression | The game needed more evidence of a vertical slice rather than a single mechanic test. | Added Level 2, Level 3, menu, intro, ending, hazards, and art assets. | Commit `346334a` |

I tested by repeatedly entering Play Mode in Unity, recording movement, spawning clones, checking pressure plates and doors, resetting levels, and confirming that hazards and goals behaved correctly. Testing focused on whether the game was playable from start to finish and whether the main puzzle loop was understandable.

## 7. Screenshots or Short Evidence of Progress Over Time

The project includes both Git history and project-file evidence of progress.

Short evidence from the repository:

- The first full prototype appears in commit `15dc672`, which added the core Unity scenes, scripts, project settings, and gameplay systems.
- The clone spawning and visual fixes appear across commits `7489436`, `ef0e10c`, `3f0e369`, and `a10c0f5`.
- The final project expansion appears in commit `346334a`, which added Level 2, Level 3, story/menu scenes, art resources, hazards, and animation scripts.

Suggested screenshots to include in the final PDF:

1. Screenshot of the GitHub repository main page showing the repository name and recent commits.
2. Screenshot of the commit history showing the dated commits from 29 May to 13 June.
3. Screenshot of Unity Level 1 showing the player, door, and pressure plates.
4. Screenshot of a clone replaying a recorded action.
5. Screenshot of Level 2 or Level 3 showing hazards, platforms, crates, or more complex level design.
6. Screenshot of the `Assets/Scripts` folder showing the main gameplay scripts.

## 8. Explanation of How the Project Changed During Development

The project began as a mechanical prototype for a time-clone puzzle game. At the beginning, the main goal was to prove that the player could record movement and replay it as a clone. The earliest version therefore focused on scripts and systems rather than presentation.

During development, the project changed in three main ways. First, it became more reliable. Clone spawning, player position handling, prefab setup, and clone visuals were all fixed after testing. Second, the game became clearer to play. The clone visual system, fallback sprites, doors, plates, and feedback systems were improved so that the player could understand what was happening. Third, the project became more complete as a vertical slice. It expanded from a single test scene into a project with a main menu, story introduction, three levels, hazards, background art, animated fire traps, and an ending scene.

The main design also became more focused. Instead of adding unrelated mechanics, I kept the project centred on recorded actions, pressure plates, doors, platforms, crates, and hazards. This helped the game stay achievable within the assignment time.

## 9. External Assets / Templates / Tutorials / AI Declaration

### External Resource 1: Unity Engine and Unity 2D Tools

1. Name of resource: Unity Engine, Unity 2D Physics, Unity UI, Unity scene/prefab workflow  
2. Type: Engine / development tool / template workflow  
3. Source: Unity Technologies  
4. Licence or permission: Unity Personal / Unity Terms of Service  
5. What it provided: Editor, scenes, GameObjects, Rigidbody2D, BoxCollider2D, SpriteRenderer, UI tools, project structure  
6. What I used unchanged: Unity engine systems and editor workflow  
7. What I modified: My own scripts, scenes, prefabs, and gameplay logic  
8. What I created myself: The time-clone gameplay systems, level logic, scene setup, and project-specific scripts  
9. Where it appears in my game: Entire Unity project  
10. How it is credited: Mentioned in this declaration as the game engine used  

### External Resource 2: 2D Art and Sprite Assets

1. Name of resource: 2D character, platform, background, door, lava/fire, and cloud sprites  
2. Type: Image assets / sprites  
3. Source: [Add the source website or write "Created by me" if you made them yourself]  
4. Licence or permission: [Add licence, permission, or "Original asset"]  
5. What it provided: Visual assets for player, clone, platforms, hazards, doors, background, and animation frames  
6. What I used unchanged: Some sprite images were imported into `Assets/Resources/Art`  
7. What I modified: Unity import settings, scaling, placement, sliced rendering, animation use, and scene integration  
8. What I created myself: Gameplay implementation, level design, collision setup, prefab setup, animation scripts, and object behaviour  
9. Where it appears in my game: `Assets/Resources/Art`, playable scenes, menu/story scenes  
10. How it is credited: This declaration and any final credits section required by the licence  

### External Resource 3: AI Assistance

1. Tool used: ChatGPT / Codex  
2. What I asked: I used AI assistance for wording support, debugging guidance, project-document drafting, and help checking how to describe my Unity systems professionally.  
3. What output I used: I used selected explanations, structure suggestions, and draft wording for portfolio/documentation.  
4. What I changed: I reviewed the text, connected it to my own project evidence, kept only the parts that matched my actual Unity project, and edited it into my own submission style.  
5. How I tested it: For code-related suggestions, I checked the Unity project in Play Mode and used Git history/build evidence rather than accepting suggestions blindly.  
6. What I understand: I understand the purpose of the main systems, including recording, replay clones, pressure plates, doors, reset logic, hazards, and scene progression.  
7. What I still do not fully understand: Some Unity editor automation and prefab/scene serialization details are still areas where I would need more practice.  
8. Where it appears in the project: Documentation support and some debugging/planning support; final project decisions and testing remained my responsibility.  

## 10. Credits and Licences Where Relevant

Unity Engine: Unity Technologies, used under the Unity licence/terms.  
GitHub: Used for source control and repository hosting.  
Visual Studio / C# tools: Used for C# scripting and code editing.  
2D art assets: [Add exact asset sources and licences before final submission if any assets were downloaded.]  
AI assistance: ChatGPT / Codex was used for support as declared above.

Before final submission, I should confirm the exact source and licence of every imported image asset in `Assets/Resources/Art`. If an asset cannot be credited properly, it should be replaced with an original or clearly licensed asset.

## 11. Reflection on Organisation, Time Management, Independent Work, and Professionalism

This project helped me understand the importance of building a game through small, testable systems. The time-clone mechanic had several connected parts, including recording, data storage, clone playback, spawning, physics, pressure plates, and reset behaviour. If these had all been built at once, it would have been difficult to find bugs. Working system by system made the project easier to manage.

My Git history shows that I used iteration rather than only making one final upload. I made separate fixes for clone spawning, clone visuals, player position handling, and jump tuning. I also used reverts when a change did not improve the game. This is an important professional habit because it shows that I was willing to test decisions and undo them when necessary.

My time management improved during the project. The first stage focused on the main mechanic, and later stages focused on polish, extra levels, story/menu scenes, and visual assets. In future, I would start documenting asset sources and testing notes earlier, because professional evidence is easier to collect during development than after the project is nearly finished.

In terms of independent work, I developed a stronger understanding of Unity's component-based structure. I used separate scripts for separate responsibilities, such as player control, recording, clone management, plates, doors, hazards, and goals. This made the project easier to extend and debug.

Professionalism in this project meant keeping the work organised, using version control, responding to problems through testing, and being honest about external resources and AI support. The final result is not perfect, but it shows a clear development process and a working response to technical issues.

## 12. Known Limitations and How I Managed Them

One limitation is that the clone replay system depends on sampled frame data, so it may not always behave exactly like a fully simulated second player. I managed this by keeping the clone replay short, direct, and focused on puzzle interactions such as pressing plates.

Another limitation is that the project uses a simple UI and simple visual feedback. This is acceptable for the prototype because the main assessment focus is the gameplay system, but clearer UI and sound feedback would improve the final game.

The visual asset pipeline is another limitation. Some imported assets still need exact source and licence confirmation before final submission. I managed this by keeping all art resources in organised folders and by including an external-resource declaration section.

The level design is also limited in scope. The game has a small number of levels rather than a long campaign. I managed this by making each level demonstrate a different use of the core mechanic: basic clone/plate interaction, hazards and platforms, then more complex crate and clone coordination.

Finally, my testing was mainly manual Play Mode testing. This was suitable for a small Unity prototype, but future development would benefit from a more formal test checklist and possibly automated tests for data classes or simple gameplay conditions.

## Conclusion

This portfolio shows that Temporal Paradox was developed through an organised and iterative process. The GitHub history, commit evidence, bug-fixing record, development log, and resource declaration demonstrate progress over time and professional awareness. The project changed from a basic mechanic prototype into a more complete Unity vertical slice with levels, visuals, menus, hazards, and a working time-clone puzzle loop.
