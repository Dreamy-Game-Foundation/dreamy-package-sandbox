# Package maintenance — 2026-11-06

Status: complete. Commit all existing project changes and package deltas, with synchronized package version/README/changelog. No tag, push or publication performed in this checkpoint.

| Package | Prepared version | Package commit |
| --- | --- | --- |
| com.dreamy.editor-tools | 1.1.0 | `3f8556b` |
| com.dreamy.feature.economy | 0.8.1 | `c657d2a` |
| com.dreamy.feature.missions | 0.3.1 | `aab0428` |
| com.dreamy.feature.tutorial | 0.1.1 | `529193e` |
| com.dreamy.feedback | 0.4.1 | `02de01d` |

Editor Tools gets a minor version for the additive Game View Screenshot and shortcut features; minimum Unity now matches its already documented 6000.4 toolbar API. Economy/Missions patches cover sample menu cleanup after their last version update. Tutorial patch covers the sample in-memory save service; Feedback patch covers sample-test/evidence maintenance. Runtime contracts of these four patch packages were not changed by this task. Their previously committed source changes remain in history.

Audio 1.0.5 and Debugger 0.1.0 already version their latest source; no additional bump. Shop only finalized the existing 0.3.0 changelog, so it remains 0.3.0. Other clean packages remain unchanged. Battle Pass is parent-owned source, not an initialized independent submodule; it has no pending changes and stays 0.1.0. Existing tags and dependency minimums are preserved; higher local patch versions do not require raising all consumers' minimums.

Fresh verification: Editor Tools compiled in an isolated Unity 6000.4.12f1/Linux consumer and 9/9 EditMode tests passed. The fixture includes the already-tested Debugger tarball and adds local Editor Tools with testables. Exact receipt and manifest/lock are attached. Metadata JSON/README/changelog and source/doc whitespace checks passed. Unity-generated empty-field whitespace in .asset/.meta is retained; no serialized file was manually rewritten. Scanned changed text for common bot/GitHub tokens and private-key headers: no matches. This is a limited pattern scan, not a universal secret audit.

Parent commit preserves the user's existing debugger config/startup settings, imported-sample relocation/customizations, scene bootstrap removal, generated localization font changes, screenshots, package lock/settings and Audio/Editor capture evidence. Imported sample snapshots are not overwritten with package templates. All registered dirty submodules are committed first and the parent records their exact gitlinks.

No whole-game test, new phone/Windows clipboard/GUI capture or device build was run. Earlier screenshot/clipboard/device receipts keep their original source/hash scope. Remaining platform limitations are retained in the Editor Tools README and historical capture evidence. Only documentation/metadata changed in the four patch packages during this checkpoint, so no redundant domain test run was claimed.

Referenced docs/agents/dreamy-workflow.md is absent in this checkout; this is a manual completion checkpoint, not a native CLI/harness receipt.
