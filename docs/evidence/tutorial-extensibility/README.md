# Tutorial 0.3 implementation evidence

Date: 2026-11-16. Package release: 0.3.0. Unity: Linux Editor 6000.4.12f1.

## Implemented scope

- M1: model-owned pending start/advance/skip operations, identity-based persistence retry across view/controller reopen and pause/resume, invalid callback protection, owning-scene 3D physics query.
- M2: optional persistence/progress/events/presentation-ownership/target-diagnostics capabilities. Existing ITutorialService and ITutorialView signatures unchanged. Diagnostics snapshots copy target lists; technical errors and user-facing localization keys are separate.
- M3: token-scoped one-shot gameplay signal handles; predicate/count/All/Any condition binding; independent scoped input leases and disposable policy binding; per-step waiting timeout override. Host retains gameplay, input backend and recovery authority.
- M4: stable-ID checkpoint migration, newer-save revision/invalid-progress guard, catalog/localization/target audit and read-only Inspector/catalog draft preview.
- M5: structured observer events plus four executable integration examples and shared-runtime fixture tests (puzzle/action/idle/simulation). No four full games or new gameplay UI scenes are claimed.
- M6: deliberately deferred per plan: no demonstrated consumer yet for branching graphs/concurrent-flow arbitration.

## Publication

Package [commit dbf1f6e](https://github.com/Dreamy-Game-Foundation/com.dreamy.feature.tutorial/commit/dbf1f6ef6f0ced68d5ee4f73740662c466c7d331) and annotated tag [v0.3.0](https://github.com/Dreamy-Game-Foundation/com.dreamy.feature.tutorial/tree/v0.3.0) were pushed to the package origin/main on 2026-11-16. The sandbox records that exact gitlink along with the plan, documentation and evidence. This is a Git package release, not a player/store or registry publication.

## Matching successful verification

`verification.json` records the command, package/dependency input fingerprints, resolved packages, exit codes and XML summaries. Both runs exited 0 without skipped tests:

| Layer | Result |
| --- | --- |
| Isolated consumer EditMode | 56/56 passed |
| Isolated consumer PlayMode + imported fresh sample | 34/34 passed |
| Sandbox MCP EditMode | 56/56 passed, job 8486f9f899504267963c907715ffbd78 |
| Final Console | No compiler errors; one expected duplicate-target error asserted by the passing LogAssert test |
| Diff/serialization | No existing .meta, scene, prefab or asmdef modified; whitespace checks passed |

Current package input fingerprint: `a6675790b35b294c2aad60c0f1f6d4ac6e215a52e556664c57a47598bca2ccf7`. Scheme `package-inputs-v2` covers package.json and C#/asmdef/JSON/prefab/scene/meta inputs under Runtime, Integration, Editor, Tests and Samples~. The verifier's own hash and Dreamy dependency source hashes are recorded separately. Documentation/receipts are excluded. `sourceUnchangedDuringVerification` is true; fingerprint was recalculated and matched after both runs.

Release verification was repeated after whitespace normalization of a new folder .meta file; its GUID is unchanged. The sandbox MCP run used the same runtime/test C# sources before that metadata-only normalization.

The host's older imported Tutorial sample was preserved. The isolated consumer imported the new sample from Samples~ and tested all four genre bridges. This proves new sample compilation without overwriting consumer-owned imported files. Raw batch logs remain at `/tmp/tutorial-release-0.3.0-20261116`; XML and manifest are archived here.

## Observed failures and their disposition

1. Before the fix, MCP regression InvalidCallback_DoesNotErasePendingSaveRetry failed with Expected Completed / actual ActiveStep (job 74c29d3083b046409662ab599193e02b). Final matching EditMode receipt includes a passing regression. Reopen/pause persistence and local-physics 3D regressions pass in PlayMode.
2. During incremental source edits, Unity compiled changed callers before importing new package files, producing missing-type errors. A scoped recursive import of Packages/com.dreamy.feature.tutorial resolved this; final Console and tests verify the matching source.
3. A read-only UI preview fixture initially failed CodeDom compilation due to duplicate Newtonsoft.Json type resolution. The fixture used an in-memory step built by reflection instead; package source was unaffected.
4. PreviewRenderUtility capture attempts in this project produced background-only images, including after explicitly rebuilding UI graphics. They were personally inspected and rejected as visual evidence. Fixture button-state snapshots showed Next/Skip hidden and Retry visible in pending state, but these are not proof of rendered UI/layout. No blank image is presented as a passing screenshot.
5. Final Console contains the expected duplicate tutorial target cube error from TutorialTargetTests.DuplicateTarget_IsRejectedWithoutReplacingOriginal. That test explicitly uses LogAssert.Expect; the complete MCP run passed. It is not a new unhandled runtime/compiler failure.

## Preserved Editor state and limitations

MCP was pinned to dreamy-package-sandbox@5f78127b991a2b92, rather than the other connected game. BootstrapScene was clean and no prefab stage was open before work. Read-only preview utilities were cleaned up; final scene remains BootstrapScene, clean, outside Play Mode. No player save or source scene/prefab was edited for preview.

Visual review remains degraded: the attempted overlay prefab previews used the actual imported prefab (byte-identical to the package sample), reference resolution 1280x720, match 0.5, and intended fixtures at 1280x720 / 2400x1080 / 1280x960. The capture path failed to render uGUI, so these dimensions are attempted cases, not visually verified support. Portrait, notches, Simulator, physical touch and device/player builds were not exercised. The Editor inspector compiles; its layout has not received an interactive visual review.

Pending persistence is RAM state in the service, not a durable cross-service transaction; process kill/model replacement still requires host gameplay reconciliation. Conditions and input scopes require the host to connect gameplay events and its actual input backend. Minimum Unity 6000.0 remains declared, not tested by this receipt. Referenced Dreamy workflow/compatibility registry is absent; no blanket compatibility or store-readiness claim is made.
