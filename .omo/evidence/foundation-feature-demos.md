# Foundation feature demos validation

Implemented five buttons: Missions, Progression, Lucky Wheel, Tutorial Demo and Economy. Existing Shop, Daily Reward, Settings and Rate Us remain available. New sample UI/logic is stored in package `Samples~` and mirrored into imported samples.

## Automated checks

- Unity compilation completed without application compile errors.
- Missions EditMode suite: 17 passed, 0 failed, 0 skipped; job `233ec412080340a49580f494eed0f345`. Includes catalog deserialization using DataConfig camel-case settings.
- Five button references and four sample panel references plus demoRoot checked through SerializedObject: all assigned.
- Source/import sample prefab contents and meta files synchronized.
- Runtime assembly source checked for Editor references.
- Whitespace check uses `git -c core.whitespace=-blank-at-eol diff --check` because Unity YAML emits empty scalar values with trailing spaces.

## Editor Play Mode smoke checks

- Bootstrap reached Ready and opened Foundation.
- Economy duplicate open produced one panel; grant added 100 coins; retrying the same grant added zero.
- Missions: three stage-win events completed the first mission; first claim added 100 coins; repeated claim added zero. Collect event and sample claim button added 50 once.
- Progression advanced to max level 3, granted 50 currency.gold; the max-level action granted nothing further and disabled the button.
- Wheel reveal interruption followed by reopen retained transaction `lucky-wheel:sample-wheel:1`; gem reward was granted once. Pending state also survived Play Mode restart.
- Manual tutorial completed welcome, Add Score, Open Shop, Close Shop and done; replay returned to welcome with one controller/overlay. Manual storage is in memory and does not delete player saves.
- Shop navigation recreated Foundation; Missions and Progression reopened successfully afterwards.
- Daily Reward duplicate click produced one panel. Settings and Rate Us opened and closed using their sample buttons.
- Mission list, Progression, Economy and Foundation layouts inspected in offscreen portrait renders. Narrow 600x1000 Foundation render verified scrolling to all five buttons. These are Editor layout checks, not device build validation.
- Final Editor returned to stopped Play Mode with a clean active scene. Demo actions used persistent shared player state; progression/mission completion and rewards remain saved.

Screenshots are stored beside this file. No Android/iOS player build or physical device test was run.
