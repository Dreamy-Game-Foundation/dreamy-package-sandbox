# Component API and standalone scene verification — 2026-11-02

Status: passed on Unity 6000.4.12f1, resolved Localization 1.5.8.

Changes: setup/fallback/runtime APIs on all six localization components; preserve serialized fields; atomic Unity SetReference; safe replacement subscriptions; standalone Settings test view with no PanelManager or UIPanel lifecycle.

Initial EditMode fixture errors are retained in editmode-initial-failure.xml. They exposed invalid abstract TMP_Text auto-construction in a fixture and unsupported EditMode SendMessage(OnValidate). Fixtures now create concrete TMP targets and use public setup APIs. An intermediate run passed 40/40 before the same-target regression was added.

Final results:

- editmode.xml: 41 tests passed, 0 failed.
- playmode.xml: 13 tests passed, 0 failed, including existing Settings/JSON regression and new component lifecycle cases.
- Standalone scene smoke: ready, 0 PanelManager, 0 UIPanel, view type StandaloneSettingsView; no new console errors.
- Existing binding script meta/GUID hashes match the earlier source identity. Serialized field ownership/names remain intact.
- Test scene migration retains the original canvas/content and replaces the manager with a plain Settings overlay. Runtime root has no PanelManager/PanelPresenterFactory references.
- Native tests initially paused at a modified-scene save dialog; only LocalizationSettingsTest was saved, preserving its changes. The GUI action response timed out, but subsequent native test XML confirmed the save took effect and the run completed. No repeated click or scene discard was performed.

Known limits: real localized Asset Table load-failure/release coverage, player build, device/landscape validation and clean external consumer remain unverified. Empty components defer loading until configured through their component APIs. CaptureFallback and Reset intentionally overwrite the authored snapshot when called explicitly. Read-only GetComponent discovery is lazy; targets are reacquired when Unity marks an old object destroyed.
