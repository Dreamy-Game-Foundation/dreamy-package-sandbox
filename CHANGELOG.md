# Changelog

## [Unreleased]

- Correct package release ordering above historical v1.x tags: Assets 1.0.1, Audio 1.0.3, DataConfig 1.0.1, Datasave 1.0.1, Editor Tools 1.0.1, and UI 1.0.5.
- Synchronize package metadata, release notes, Audio sample import documentation, and the UI Assets dependency/lock entry.
- Preserve existing tags, unchanged package versions/minimum dependencies, and imported sample snapshots.

## [0.2.1] - 2026-10-16

- Integrate Economy 0.6.0: load Coin/Gem suggestion panels through PanelManager addresses and register presenters through the shared factory.
- Remove suggestion prefab fields and the NotEnoughResourcesPanel fallback from GameInstaller and Bootstrap; unmapped shortages return PresentationUnavailable.
- Migrate prefab components and Addressables entries through Unity MCP while preserving authored layout and bindings; replace the setup generator with a read-only validator.
- Validate 90 managed tests, 17 recovery EditMode tests and 5 popup PlayMode tests; Unity Console has no errors.

## [0.1.1] - 2026-10-12

- Synchronize portable Economy resource-holder and Shop feedback prefabs with sandbox authoring.
- Release Core 1.2.0 pooling contracts and an importable Core Demo with injected pooling usage.
- Release Economy 0.2.1, Shop 0.1.2 and Feedback 0.4.0; pin their committed submodule revisions.
- Integrate owned LeanPool lifecycle, save initialization, confirmed purchase presentation, Canvas UI bursts and camera rebinding in the sandbox.
- Preserve imported sample version folders as existing Unity import snapshots; fresh imports use the updated package versions. Basic Feedback remains independent of optional DOTween/URP attachments present in the sandbox.

Validation results are recorded in `.omo/evidence/samples-release/summary.json`.
