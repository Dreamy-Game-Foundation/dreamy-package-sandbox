# Dreamy package version/changelog audit — 2026-11-10

Scope: all 21 Dreamy packages under LocalPackages. Inspect local HEAD, working tree, package.json history, tags and changelog contents. Missing tags alone do not justify a new version. Preserve existing tags and dependencies; The audit initially prepared local changes; the subsequent user-authorized commit/tag/push is recorded below. Battle Pass is tracked in the sandbox repository, not an independent package repository.

| Package | Final source version | Decision |
|---|---|---|
| `com.dreamy.assets` | 1.0.1 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.audio` | 1.0.5 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.connectivity` | 0.1.1 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.core` | 1.2.0 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.dataconfig` | 1.0.1 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.datasave` | 1.0.1 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.debugger` | 0.1.4 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.editor-tools` | 1.2.0 | 1.1.0 → 1.2.0: Package Manager feature commit c29e0c9 after the 1.1.0 preparation. |
| `com.dreamy.feature` | 0.2.0 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.feature.battle-pass` | 0.1.1 | 0.1.0 → 0.1.1: sample rebuild-menu cleanup 5ff5ceb after initial candidate. |
| `com.dreamy.feature.daily-reward` | 0.3.0 | Keep 0.3.0, already raised by 140f24e; finalize mislabeled Unreleased changelog entry. |
| `com.dreamy.feature.economy` | 0.8.1 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.feature.lucky-wheel` | 0.1.1 | 0.1.0 → 0.1.1: installer/lifecycle refactor 5f97a5e remained Unreleased under version 0.1.0. |
| `com.dreamy.feature.missions` | 0.3.1 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.feature.progression` | 0.2.1 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.feature.settings` | 0.3.0 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.feature.shop` | 0.3.0 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.feature.tutorial` | 0.2.0 | Correct historical Unreleased labels; changes already included in current version. |
| `com.dreamy.feedback` | 0.4.1 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.localization` | 0.2.0 | Version and changelog already cover current source; no bump needed. |
| `com.dreamy.ui` | 1.1.0 | Version and changelog already cover current source; no bump needed. |

Validation: parsed all 21 package manifests; version format and metadata/documentation-only diffs checked; git diff --check passed in each package. No runtime, assembly, dependency, serialized asset or meta GUID edits. Unity compile/tests were not rerun because this audit changes only package versions and documentation; previous test receipts are not fresh verification for these versions.

The instructed docs/agents/dreamy-workflow.md and project compatibility catalog are absent from this workspace. No Dreamy API compatibility claims or dependency upgrades were introduced. Existing unrelated sandbox localization font changes and debugger submodule pointer changes were left untouched.

Status: complete locally.

## Commit/tag/push follow-up

User authorized committing, completing current-version tags and pushing all packages. Existing tags are preserved. Independent package commits and current-version tags are pushed to origin/main. Battle Pass 0.1.1 is committed in the sandbox with the package-scoped tag com.dreamy.feature.battle-pass/v0.1.1. Sandbox submodule pointers record the package commits. This is Git publication, not a GitHub Release or new Unity/device validation. Earlier preparation-time statements about absent tags in package docs/changelogs describe their original checkpoints.
