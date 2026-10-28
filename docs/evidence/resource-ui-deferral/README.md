# ResourceUIHolder refresh deferral — 2026-10-28

Economy 0.8.0 adds instance-owned `DeferRefresh()` scopes and explicit `Unbind()`.
The final release reads current provider state; all text refresh routes honor active
scopes. Old lifecycle/binding/resource tokens cannot affect a new hold. Serialized
fields and manual `autoRefresh=false` behavior are preserved. Disable removes event
subscriptions while retaining the provider for enable; unbind/destroy clear it.

Validation through Unity MCP in sandbox, Unity 6000.4.12f1:

- Refresh/compile succeeded; the compiled API was resolved in Editor.
- 19/19 holder PlayMode tests passed, including Unity lifecycle, simultaneous scopes,
  two independent holders, exception/cancellation and immediate owner-close cleanup
  while an async task remains pending.
- 101/101 Economy Runtime/Recovery EditMode tests passed.
- Serialized field comparison, Runtime/UI Editor-reference checks, JSON validation
  and `git diff --check` passed. Final Console contains no errors.

The first test attempt stalled before executing tests with Play from Bootstrap
set. Cancelled the run, reloaded orphaned MCP state and disabled that preference
for isolated tests. Restored the original preference afterward. Game PlayMode before
tests logged `FoundationTutorialAdapter.cs:87` NullReferenceException outside the
changed holder. This does not establish a passing full-project game smoke test.

Player/mobile builds, device checks and end-to-end Daily/Missions/Shop presentation
were not run. Consumers still need to enable auto refresh on Home, defer the actual
target holder before claim/purchase, and cancel/release immediately from panel close
(including HideTween-only navigation). Daily/Missions targeting Home defer Home;
Shop targeting its own holder defers Shop while Home continues to refresh. README
contains the full before-mutation and async owner cleanup example.

See `results.json` for test job identifiers and SHA-256 fingerprints of validated
package sources, test assembly, tests and documentation. No publish or push occurred.
