# Features Progress

This subsystem stores local long-term player data such as characters, class choice, level, experience, inventory snapshot, and related progression state.

## Main pieces

- `PlayerProgressService` is the singleton save/load entry point.
- Save data is stored in `player_progress.json` under `Application.persistentDataPath`.
- `PlayerProgressData`, `PlayerProfile`, and `PlayerCharacterState` model the saved profile and active character.
- `PlayerProgressionRules` normalizes level and XP and applies experience gains.
- `ClassProgressionSO`, `ProgressionNodeSO`, `ResearchState`, and related domain classes support broader progression content.

## Runtime flow

1. `PlayerProgressService` loads the save file or creates a default profile.
2. The active character is exposed through `GetActiveCharacter()`.
3. Feature systems read and update class id, level, experience, inventory snapshot, and other character state through the service.
4. `ActiveCharacterChanged` notifies UI and gameplay systems when the active profile changes.

## Integration points

- `Classes` reads the saved class id.
- `Inventory` persists the active character inventory snapshot.
- `Quests` and other reward systems feed XP back into the active character state.
