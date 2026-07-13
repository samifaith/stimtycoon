# Unity 6.3 LTS Setup Guide

**Status:** Stim Tycoon is being upgraded from Unity 2022.3 LTS → **6.3 LTS**

---

## Step 1: Install Unity Hub (if not already installed)

1. Download from: https://unity.com/download
2. Install Unity Hub
3. Launch it and sign in with your Unity account

---

## Step 2: Install Unity 6.3 LTS Editor

1. In Unity Hub, go to **Installs** → **Install Editor**
2. Select **Unity 6 LTS** (the latest 6.3 patch, e.g., `6000.3.x`)
3. Select **iOS Build Support** (required for Stim Tycoon)
4. Select **Apple GameKit Plugins** (recommended for Game Center integration)
5. Click **Install** and wait (~15–30 min depending on connection)

---

## Step 3: Open the Project

1. In Unity Hub, go to **Projects**
2. Click **Open** and navigate to this repository's folder
3. Select the folder; Unity will detect the project and open it with 6.3 LTS
4. Wait for the first import to complete (~3–5 min)

---

## Expected Changes After Opening

- `ProjectSettings/ProjectVersion.txt` will use a `6000.3.x` editor version
- `Packages/manifest.json` will be updated with 6.3 LTS compatible packages
- `Packages/packages-lock.json` will be regenerated
- A **Library/** folder will be created (ignore; already in .gitignore)

---

## Verify the Upgrade

1. In the Unity editor, open **Window** → **General** → **About Unity**
2. Confirm you see **Unity 6.3 LTS** or **Unity 6** (latest patch)
3. Close the editor and commit:

```bash
git add ProjectSettings/ Packages/
git commit -m "Upgrade to Unity 6.3 LTS"
git push
```

---

## Next: Phase 0 Technical Foundation

Once the project opens in Unity 6.3 LTS:

1. **Dialogue System for Unity** – Import from Asset Store
2. **Easy Save 3** – Import from Asset Store
3. **Apple GameKit** – Already in Packages (verify it's enabled)
4. **UI Toolkit** – Built-in; verify in Window → UI Toolkit

Then proceed to Phase 0 deliverables:

- Dialogue System prototype
- Save schema validator
- Event schema validator
- Risk/reward calculator

---

## Troubleshooting

**Project won't open?**

- Check that you have iOS Build Support installed for 6.3 LTS
- Try removing `Library/` folder and reopening
- Check the Editor.log: `~/Library/Logs/Unity/Editor.log`

**Compilation errors after opening?**

- Use **Assets** → **Reimport All**
- Check Console for any import failures

**Need the old 2022.3 project back?**

- It's still in git history: `git log --oneline`
- Create a branch from that commit if needed
