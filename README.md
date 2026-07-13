# Stim Tycoon

**Status:** Phase 0 Product Foundation, with domain + runtime scaffold in repo
**Target Platform:** iOS (13+)  
**Engine:** Unity 6.3 LTS  
**Tech Stack:** C# + UI Toolkit + Dialogue System for Unity

A mobile life and wealth simulation game combining choice-driven life progression with deep business, investing, and relationship systems.

## Repo Audit Snapshot

### Verified in repo

- Locked event schema, validator, and risk/reward calculator in C#
- Locked save envelope and save validator in C#
- Runtime composition boundary with no-op vendor adapters
- Catalog-backed event runtime service
- NUnit coverage for event and save validation, plus runtime service tests
- Stim-owned abstractions for dialogue, save, account, cloud save, ads, and event catalog

### Still missing from implementation

- Dialogue System import and authored prototype conversations
- Easy Save 3 import and compile-symbol activation (adapter source is wired)
- Unity Authentication implementation
- Apple GameKit / Game Center implementation
- Unity Cloud Save implementation
- Unity LevelPlay implementation
- UI Toolkit screens and scene/UXML assets
- Representative events authored in Dialogue System

### Package status

The repository currently has only core Unity packages in `Packages/manifest.json`. The third-party packages above are still needed before the vendor adapters can be implemented.

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
│   └── Scripts/
│       ├── Domain/       # Event schema, save schema, abstractions
│       ├── Runtime/      # Composition root and runtime services
│       └── Tests/        # NUnit tests for domain/runtime slices
├── Packages/
│   └── manifest.json     # Package dependencies
├── ProjectSettings/      # Unity configuration
├── docs/
│   ├── MODIFIER_RULES.md       # Event outcome probability system
│   ├── BUSINESS_TURNS.md        # Business turn mechanics
│   ├── EVENT_SCHEMA.md          # Locked event schema and validation rules
│   └── PHASE_0_ARCHITECTURE_CHECKLIST.md # Verified architecture gap list
├── README.md             # This file
├── UNITY_6_UPGRADE_GUIDE.md    # Setup instructions
└── STIM_TYCOON_MASTER_README(2).md  # Full product spec
```

Note: the folders for scenes, prefabs, art, and audio are part of the long-term target structure, but they are not yet populated in the repository.

---

## Documentation

| Document                                                                         | Purpose                                          |
| -------------------------------------------------------------------------------- | ------------------------------------------------ |
| [STIM_TYCOON_MASTER_README(2).md](<STIM_TYCOON_MASTER_README(2).md>)             | Complete product definition and 10-phase roadmap |
| [docs/MODIFIER_RULES.md](docs/MODIFIER_RULES.md)                                 | Event outcome probability system design          |
| [docs/BUSINESS_TURNS.md](docs/BUSINESS_TURNS.md)                                 | Business turn mechanics and economics            |
| [docs/PHASE_0_ARCHITECTURE_CHECKLIST.md](docs/PHASE_0_ARCHITECTURE_CHECKLIST.md) | Verified Phase 0 architecture and package gaps   |
| [UNITY_6_UPGRADE_GUIDE.md](UNITY_6_UPGRADE_GUIDE.md)                             | Unity 6.3 LTS installation                       |

---

## Phase 0: Product Foundation

**Current Work:** Establishing technical foundation and design specs.

### Implemented foundation

- [x] Event schema and validator
- [x] Risk/reward calculator
- [x] Save schema and versioning tests
- [x] Runtime composition root
- [x] Event runtime service
- [x] Stim-owned abstraction layer for vendor integrations

### Exit Criteria

- [ ] Dialogue System prototype + C# bridge
- [x] Save schema & versioning tests
- [x] Risk/reward band calculator
- [x] Event schema validator (C#)
- [x] Runtime composition boundary
- [x] Catalog-backed event runtime service
- [ ] Five representative events in Dialogue System

---

## Tech Stack (Locked)

| Component          | Technology                               |
| ------------------ | ---------------------------------------- |
| Engine             | Unity 6.3 LTS                            |
| Language           | C#                                       |
| UI Framework       | UI Toolkit + UI Builder                  |
| Events & Branching | Dialogue System for Unity                |
| Local Save         | Easy Save 3                              |
| Authentication     | Unity Authentication + Apple Game Center |
| Analytics          | Unity Analytics                          |
| Ads                | Unity LevelPlay                          |
| Testing            | Unity Test Framework                     |

---

## Development Setup

For the short, ordered vendor setup, use [docs/PACKAGE_INSTALL_CHECKLIST.md](docs/PACKAGE_INSTALL_CHECKLIST.md).

### Prerequisites

- **Mac** with macOS 12+
- **Unity 6.3 LTS** installed via Unity Hub
- **iOS Build Support** for testing
- **Git** (pre-installed on macOS)

### First-Time Setup

1. Follow [UNITY_6_UPGRADE_GUIDE.md](UNITY_6_UPGRADE_GUIDE.md)
2. Open project in Unity 6.3 LTS
3. Import Asset Store packages (as needed per phase)

### Current package gaps

- Dialogue System for Unity
- Easy Save 3
- Apple GameKit / Game Center support
- Unity Authentication
- Unity Cloud Save
- Unity LevelPlay / Ads Mediation

---

## Resources

- [Unity 6 Docs](https://docs.unity.com/)
- [UI Toolkit Manual](https://docs.unity.com/Manual/UIToolkit.html)
- [Dialogue System for Unity](https://www.pixelcrushers.com/dialogue_system/)

## Version Control Notes

Unity-generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Builds`, `Logs`, and `UserSettings` are ignored. Commit source assets, package files, project settings, and `.meta` files.
