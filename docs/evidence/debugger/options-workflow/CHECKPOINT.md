# Options workflow follow-up

Completed 2026-11-05. Authorized: convenience registration, owned INotifyPropertyChanged subscription, per-action confirmation, incremental presentation/UI updates, samples/docs/tests. Use Engine for adapters; Runtime registry stays free of Unity/Editor/Core. Preserve card IDs, authorization, GUIDs and game services/save authority. Main scene baseline BootstrapScene clean/Edit Mode. No live reporting or publishing.

Verification complete in isolated source/archive consumers and a project copy. Prior preview.2 receipts are historical for this source change. Known lessons: wait for successful compile before tests (old assembly can return stale passing counts), don't mutate serialized prefab YAML, don't start consumers while assets are staged, and don't claim device behavior from Editor/build results.


Final: preview.3 tgz SHA-256 1bade7f7ce1e9016558be454422fbe14985e7ad140d997b8359058941808bb60. Source/archive each 85 EditMode +41 PlayMode; cloned-game EditMode 10/10. Linux dev/release and Android ARM64 IL2CPP consumer builds passed. Unchanged 659 protected files and existing package meta hashes; source bytes match archive. Main live Editor is not certified because sandbox MCP was unavailable. See verification.json for exact scope and failure/retry history. No commit/publish/live destination actions.
