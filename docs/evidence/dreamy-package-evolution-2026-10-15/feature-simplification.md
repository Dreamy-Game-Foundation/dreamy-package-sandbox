# Feature simplification

Date: 2026-10-15. Uncommitted migration-branch source; no publication/tag change.

## Final direction

- Feature Runtime has no assembly references and no package dependencies.
  Removed the unused DataConfig GUID reference and manifest dependency together.
  Unity Package Manager resolved the existing local manifest; packages-lock now
  records an empty Feature dependency object. No dependency upgrades requested.
- Existing Feature contracts and prefab/script GUIDs were retained.
- Added FeaturePresenter, implementing existing IFeaturePresenter plus IDisposable.
  Show binds once and refreshes each time; Dispose unbinds once; Show can reopen.
  Failed Bind/Show-time Refresh triggers cleanup, including partial subscription.
  The contract requires nonthrowing Unbind that tolerates partial Bind. It does
  not manage async cancellation/generation guards or business transactions.
- Daily and Missions now consume the base; their views inherit IFeatureView<TState>.
  Direct Feature references were added to the two Runtime/test assemblies and
  source/imported Daily, Mission and Periodic Mission integration assemblies.
- Added lifecycle failure/reopen tests in Feature and actual MissionPresenter tests.
  Existing Daily tests validate consumer adoption. Feature testables is enabled.
- Updated Feature README and source/imported sample adoption notes.

## Verification

Target: Unity MCP, sandbox, 6000.4.12f1, Linux Editor.

| Suite | Mode | Discovered / passed | Failed | Skipped |
| --- | --- | ---: | ---: | ---: |
| Feature | EditMode | 4 / 4 | 0 | 0 |
| Missions | EditMode | 54 / 54 | 0 | 0 |
| Daily | PlayMode | 42 / 42 | 0 | 0 |

Raw results: `feature-simplification-editmode.json` and
`feature-simplification-playmode.json`. Reflection of the compiled
Dreamy.Feature.Runtime assembly returned only `netstandard` as a referenced
assembly. Source/manifest/lock and compiled assembly agree on the C# boundary.
This is not an isolated clean-consumer package-install test.

The Bootstrap start-scene preference was temporarily disabled for PlayMode,
then restored to its previous true value; startScene is BootstrapScene again.
The first consumer compile exposed missing direct Feature references in imported
view assemblies. Both source/imported integration asmdefs were fixed; subsequent
compile and tests succeeded. No package-cache source was edited.

## Compatibility and limits

Existing IFeaturePresenter members were not changed. The base is additive.
Adopted Daily/Missions view interfaces now inherit the Feature view contract;
source consumers must add a direct Feature asmdef reference and rebuild. This
unreleased change does not assert binary drop-in compatibility for precompiled
consumer DLLs. No feature save schemas, IDs, eligibility or reward rules changed.

Optional BaseFeaturePanel/BaseFeatureItem assets still reference Dreamy UI and
need UI installed when used. Their paths/GUIDs were preserved; the C# assembly
does not depend on UI. No fresh UI/Addressables/native-lifetime proof is claimed
for this refactor. Earlier smoke/test artifacts describe their own source snapshots.
