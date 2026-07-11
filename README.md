# Stim Tycoon

**Status:** Phase 0 Product Foundation  
**Target Platform:** iOS (13+)  
**Engine:** Unity 6.3 LTS  
**Tech Stack:** C# + UI Toolkit + Dialogue System for Unity

A mobile life and wealth simulation game combining choice-driven life progression with deep business, investing, and relationship systems.

---

## Quick Start

### 1. Install Unity 6.3 LTS

See [UNITY_6_UPGRADE_GUIDE.md](UNITY_6_UPGRADE_GUIDE.md) for detailed instructions.

### 2. Open the Project

In Unity Hub:
- Click **Open**
- Navigate to this folder
- Select it and wait for import (~5 minutes on first load)

### 3. Verify the Setup

In the Unity editor:
- Window → General → About Unity → Confirm "Unity 6" or "6.3 LTS"
- Window → UI Toolkit → Confirm available
- Console should show no errors

---

## Project Structure

```
stimtycoon/
├── Assets/
│   ├── Scripts/          # C# source code
│   ├── Scenes/           # UXML UI screens
│   ├── Prefabs/          # Reusable components
│   ├── Art/              # Visual assets
│   └── Audio/            # Sound effects, music
├── Packages/
│   ├── manifest.json     # Package dependencies
│   └── packages-lock.json # Locked versions
├── ProjectSettings/      # Unity configuration
├── docs/
│   ├── MODIFIER_RULES.md       # Event outcome probability system
│   ├── BUSINESS_TURNS.md        # Business turn mechanics
│   └── ADR/                     # Architecture decisions (to come)
├── README.md             # This file
├── UNITY_6_UPGRADE_GUIDE.md    # Setup instructions
└── STIM_TYCOON_MASTER_README(2).md  # Full product spec
```

---

## Documentation

| Document | Purpose |
|----------|---------|
| [STIM_TYCOON_MASTER_README(2).md](STIM_TYCOON_MASTER_README(2).md) | Complete product definition and 10-phase roadmap |
| [docs/MODIFIER_RULES.md](docs/MODIFIER_RULES.md) | Event outcome probability system design |
| [docs/BUSINESS_TURNS.md](docs/BUSINESS_TURNS.md) | Business turn mechanics and economics |
| [UNITY_6_UPGRADE_GUIDE.md](UNITY_6_UPGRADE_GUIDE.md) | Unity 6.3 LTS installation |

---

## Phase 0: Product Foundation

**Current Work:** Establishing technical foundation and design specs.

### Exit Criteria

- [ ] Dialogue System prototype + C# bridge
- [ ] Save schema & versioning tests
- [ ] Risk/reward band calculator
- [ ] Event schema validator (C#)
- [ ] Five representative events in Dialogue System

---

## Tech Stack (Locked)

| Component | Technology |
|-----------|-----------|
| Engine | Unity 6.3 LTS |
| Language | C# |
| UI Framework | UI Toolkit + UI Builder |
| Events & Branching | Dialogue System for Unity |
| Local Save | Easy Save 3 |
| Authentication | Unity Authentication + Apple Game Center |
| Analytics | Unity Analytics |
| Ads | Unity LevelPlay |
| Testing | Unity Test Framework |

---

## Development Setup

### Prerequisites

- **Mac** with macOS 12+
- **Unity 6.3 LTS** installed via Unity Hub
- **iOS Build Support** for testing
- **Git** (pre-installed on macOS)

### First-Time Setup

1. Follow [UNITY_6_UPGRADE_GUIDE.md](UNITY_6_UPGRADE_GUIDE.md)
2. Open project in Unity 6.3 LTS
3. Import Asset Store packages (as needed per phase)

---

## Resources

- [Unity 6 Docs](https://docs.unity.com/)
- [UI Toolkit Manual](https://docs.unity.com/Manual/UIToolkit.html)
- [Dialogue System for Unity](https://www.pixelcrushers.com/dialogue_system/)

## Version Control Notes

Unity-generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Builds`, `Logs`, and `UserSettings` are ignored. Commit source assets, package files, project settings, and `.meta` files.
