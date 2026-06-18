# VR Football Linebacker Training — Meta Quest

A fully immersive VR linebacker training application for Meta Quest 2/3/Pro, built in Unity 2022.3 LTS with C#. Simulates offensive plays from a linebacker's on-field perspective inside a stadium environment.

---

## Features

### Core Training Loop
- **Pre-snap read** — View the offensive formation, hear a voice callout of the formation name, and read linebacker cue text (difficulty-dependent)
- **Snap sequence** — Cadence delay with optional hard-count jump fakes (Hard difficulty)
- **Play execution** — Watch 11 animated offensive players execute realistic blocking schemes and routes in real time
- **Gap assignment** — Point with your dominant controller and pull the trigger to identify your defensive gap responsibility
- **Reaction timer** — Countdown forces quick decision-making; changes green→yellow→red as time expires
- **Rep feedback** — Immediate correct/wrong result with reaction time, gap comparison, and performance rating

### Play Types (16 built-in plays)

| Type | Plays |
|------|-------|
| **Inside Zone** | IZ Strong Right, IZ Weak Left, IZ Pistol A-Gap |
| **Outside Zone** | OZ Stretch Right, OZ Stretch Left |
| **Power** | Power Right, Power Left, Counter H |
| **Pass** | Curl-Flat Combo, Four Verts, Mesh Crossers, Boot Waggle, Slant Shoot, Post Corner, Wheel Route |

### Formations (6 built-in)
- I-Formation
- Pro Set (twins right)
- Shotgun (with HB offset)
- Singleback
- Trips Right
- Pistol

All formation positions use accurate NFL-standard alignments (1 Unity unit = 1 yard).

### Blocking Schemes
- **Inside Zone** — simultaneous lateral step with double-teams climbing to linebacker level
- **Outside Zone** — stretch blocking with TE kick-out
- **Power** — guard pull, kick-out block, FB lead through B-gap
- **Counter** — double pull misdirection, cuts back against initial flow
- **Pass Protection** — kick-step and anchor for all OL

### Gap System
Gaps labeled per NFL convention:
- **A-Gap** — Between center and guard (left/right)
- **B-Gap** — Between guard and tackle (left/right)
- **C-Gap** — Between tackle and TE (left/right)
- **D-Gap** — Outside the TE (left/right)

Visual indicators float in each gap and are aligned automatically to the spawned formation's linemen positions.

### Training Modes

**Practice Mode**
- Unlimited reps, no time pressure (unless chosen)
- Slow-motion: 0.25×, 0.5×, 0.75×, 1.0× speed (thumbstick left/right)
- Pause with primary button
- Instant replay with full-speed comparison
- Select specific plays or filter by play type
- Gap highlights visible (configurable by difficulty)

**Test Mode**
- 10/15/20 reps based on difficulty
- Auto-advances through plays
- No pausing or slow-motion
- Final grade (A+ through F) and detailed breakdown on completion

### Difficulty Settings

| Setting | Easy | Medium | Hard |
|---------|------|--------|------|
| Play speed | 0.6× | 1.0× | 1.3× |
| Reaction window | 5.0s | 3.0s | 1.5s |
| Pre-snap view | 6.0s | 4.0s | 2.5s |
| Gap highlights | ✓ | ✓ | ✗ |
| Read cues | ✓ | ✓ | ✗ |
| Formation label | ✓ | ✗ | ✗ |
| Variable snap count | ✗ | ✗ | ✓ |
| Jump fake chance | 0% | 10% | 25% |

### Stats Tracking
- Per-rep: play type, formation, correct gap, selected gap, reaction time, correct/wrong
- Per-session: accuracy %, average/fastest reaction time, rep count
- Lifetime: all-session aggregate, accuracy by play type, trend data (last 5 sessions), weakest formation
- Sessions auto-saved to `Application.persistentDataPath` as JSON

### VR Controls (Meta Quest)

| Input | Action |
|-------|--------|
| Right controller aim | Point at gap zone |
| Right trigger | Confirm gap selection |
| A button | Confirm (alternate) |
| Thumbstick left/right | Speed control (Practice) |
| A button | Pause/resume (Practice) |
| Left menu button | Return to menu |

### Voice Callouts
- Pre-snap formation announced via `AudioClip` on `FormationConfig`
- Fallback to on-screen text if no audio clip assigned
- Hard count ("Red 80! Set! HUT!") on jump-fake frames
- Crowd ambient audio with reactive volume on snap

---

## Project Structure

```
Assets/
  Scripts/
    Core/         AppState.cs, GameManager.cs, SessionManager.cs
    Data/         ScriptableObject definitions (Formation, Play, Route, Block, Difficulty, Rep)
    Formations/   FormationManager.cs, FormationLibrary.cs
    Players/      OffensivePlayerController, LinemenController, SkillPlayerController, QB
    Blocking/     (BlockAssignmentConfig drives LinemenController directly)
    Plays/        PlayDirector.cs, PlayLibrary.cs, PlayCatalog.cs
    GapSystem/    GapZone.cs, GapAssignmentManager.cs
    Reactions/    ReactionTimerController.cs
    Stats/        StatsManager.cs
    VR/           QuestInputHandler.cs, LinebackerCameraRig.cs, GestureDetector.cs
    Audio/        AudioManager.cs, CalloutSystem.cs
    UI/           MainMenuUI, PreSnapHUD, PlayHUD, ResultUI, StatsUI, PlaySelectUI
    Training/     PracticeModeController, TestModeController, DifficultyManager
    Utility/      ObjectPool<T>, WorldSpaceCanvas, FieldLineGenerator
    Scene/        SceneBootstrapper.cs, StadiumController.cs
    Editor/       PlayCatalogEditor.cs, FormationVisualizerWindow.cs
  ScriptableObjects/  (generated by editor tools)
    Formations/
    Plays/
    Difficulty/
Packages/
  manifest.json   (Unity XR + Meta XR SDK + URP + TMP + Input System)
ProjectSettings/
  ProjectSettings.asset
  XRSettings.asset
```

---

## Setup Instructions

### Requirements
- **Unity 2022.3 LTS** (tested on 2022.3.20f1+)
- **Meta Quest 2, 3, or Pro** with developer mode enabled
- **Meta XR SDK** (installed via `Packages/manifest.json`)
- Android Build Support module in Unity Hub

### 1. Open the Project
1. Clone this repository
2. Open Unity Hub → Add project → select the repo root
3. Open with Unity 2022.3 LTS

### 2. Install Packages
Unity will auto-resolve packages from `manifest.json`. If the Meta XR scoped registry fails, install the **Meta XR All-in-One SDK** from the Unity Asset Store manually.

### 3. Configure XR
1. Edit → Project Settings → XR Plug-in Management → Android tab
2. Enable **OpenXR**
3. Under OpenXR → Features, enable **Meta Quest Support**
4. Set **Stereo Rendering Mode** to **Multiview**

### 4. Configure URP
1. Edit → Project Settings → Graphics → Scriptable Render Pipeline Settings
2. Assign the included `URP_FootballTraining` asset (or create via URP template)

### 5. Bake ScriptableObjects
In the Unity menu: **Football Training → Bake All Assets**
This generates all Formation, Play, and Difficulty ScriptableObject assets in `Assets/ScriptableObjects/`.

### 6. Build Scene Setup
Create a scene with the following hierarchy and attach scripts:

```
[Scene Root]
  ─ GameManager          → GameManager.cs, SessionManager.cs, DifficultyManager.cs
  ─ SceneBootstrapper    → SceneBootstrapper.cs
  ─ XROrigin             → XR Origin (Action-Based) + LinebackerCameraRig.cs
      └─ Camera Offset
          └─ Main Camera
      └─ LeftHand Controller
      └─ RightHand Controller → QuestInputHandler.cs, GestureDetector.cs
  ─ FormationRoot        → FormationManager.cs
  ─ PlayDirector         → PlayDirector.cs
  ─ GapSystem            → GapAssignmentManager.cs
      ├─ GapZone_ALeft   → GapZone.cs (Gap = ALeft)
      ├─ GapZone_BLeft   → GapZone.cs (Gap = BLeft)
      ... (8 total gap zones)
  ─ ReactionTimer        → ReactionTimerController.cs (world-space canvas)
  ─ Audio                → AudioManager.cs, CalloutSystem.cs
  ─ Stadium              → StadiumController.cs, FieldLineGenerator.cs
  ─ UI
      ├─ MainMenu        → MainMenuUI.cs
      ├─ PreSnapHUD      → PreSnapHUD.cs
      ├─ PlayHUD         → PlayHUD.cs
      ├─ ResultPanel     → ResultUI.cs
      ├─ StatsPanel      → StatsUI.cs
      └─ PlaySelectPanel → PlaySelectUI.cs
  ─ Training
      ├─ PracticeMode    → PracticeModeController.cs
      └─ TestMode        → TestModeController.cs
```

### 7. Player Prefabs
You need three prefabs with `Animator` components and the corresponding controller scripts:
- `LinemenPrefab` → attach `LinemenController.cs`
- `SkillPlayerPrefab` → attach `SkillPlayerController.cs`
- `QuarterbackPrefab` → attach `QuarterbackController.cs`

Required Animator triggers:
```
PreSnap, Idle, DownBlock, PullBlock, PassSet, KickOut, LeadBlock,
CarryBall, RunRoute, Catch, Snap, ShotgunSnap, DropBack, Handoff, Throw
```

### 8. Gap Zone Prefabs
Each `GapZone` needs:
- A `BoxCollider` (trigger = false, for raycast)
- A child `MeshRenderer` with a semi-transparent URP/Lit material
- A child `TextMeshPro` canvas for the gap label
- The collider layer set to match `GapAssignmentManager._gapLayerMask`

### 9. Build to Quest
1. File → Build Settings → Android
2. Switch Platform
3. Player Settings → Other Settings → Minimum API Level: **26**
4. Connect Quest via USB, enable USB debugging
5. Build and Run

---

## Extending the App

### Adding New Plays
```csharp
// In PlayCatalog.cs, add to BuildPassPlays() / BuildInsideZonePlays() etc.:
MakePlay("IZ Lead Right", PlayType.InsideZone, FormationType.Pistol,
    PlayerRole.HalfBack, 8f, GapLocation.BRight,
    "Pistol with FB offset: read the lead block through the B-gap",
    BlocksForIZ(toRight: true)),
```

### Adding New Formations
```csharp
// In FormationLibrary.cs, add a new method and register in BuildAll():
public static FormationConfig CreateWingT()
{
    var cfg = ScriptableObject.CreateInstance<FormationConfig>();
    cfg.FormationType = FormationType.WingT;
    cfg.Positions = new List<PlayerPositionConfig> { ... };
    return cfg;
}
```

### Adding Routes
```csharp
// In PlayLibrary.cs, add a case to BuildRoute():
RouteType.Comeback => BuildComeback(role, dir),

private static PlayerMovementConfig BuildComeback(PlayerRole role, float dir)
{
    return new PlayerMovementConfig { ... };
}
```

### Adjusting Difficulty
Create a new `DifficultyConfig` ScriptableObject in `Assets/ScriptableObjects/Difficulty/` and set values, or modify `DifficultyManager.BuildDefault()`.

---

## Coordinate System
- **1 Unity unit = 1 yard**
- **X axis** — lateral (positive = offense's right when facing their own end zone)
- **Z axis** — depth (positive = toward defense; negative = toward offense's backfield)
- **LOS** — Z = 0
- **Linebacker position** — Z = +4 (4 yards off the ball), facing Z = 0

---

## Architecture Notes

- `GameManager` is the single source of truth for game state. All other systems subscribe to its events rather than calling each other directly.
- `PlayCatalog` / `FormationLibrary` / `PlayLibrary` are pure static factories — no MonoBehaviour, no scene dependencies. They can be unit tested directly.
- `SceneBootstrapper` does all cross-system wiring at Awake time so individual scripts have no direct references to each other.
- `ObjectPool<T>` is generic and can be used for any pooled component.
- All session data is JSON-serialized via `JsonUtility` to `Application.persistentDataPath` — survives app updates on the Quest.

---

## License
MIT — see LICENSE for details.
