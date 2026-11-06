# SR-style partial authoring prototype — 2026-11-05

Game-owned additions under Assets/_Project/Debugger; package Runtime and original SR remain untouched by this prototype. `GameCheatOptions` is split across base, Time and Application partial files in the same namespace/assembly. Host concatenates its attributed options with the existing resource/ownership catalog, using the same registration/access lifetime.

Added 5 options: TimeScale, ResetTimeScale (confirmed action), TargetFrameRate enum, read-only VSyncCount and ActiveScene. Public methods/properties are reflected by the existing AttributedCheatOptions adapter. A project link.xml preserves all members of the container for stripping; new project IL2CPP build/device validation has not been run. Earlier package candidate builds do not certify these project additions.

Fresh Unity MCP EditMode job `32ead53271714a16b71f3625a16b0e2c`: 9/9 tests passed (3 partial/attribute cases +6 existing wallet/entitlement cases). See tests.json and inputs.json. Tests restore Time.timeScale and Application.targetFrameRate, use isolated save directories, and exercise authorization, range checks, typed enum/read-only mapping, confirmation and disposal. Main scene contains one SandboxCheatHost; reflection inspected all five options. No new PlayMode UI or device run is claimed in this prototype.

Editor remained in Edit Mode, BootstrapScene clean. No actual Telegram/email delivery, real save grant or scene mutation was performed for verification. Resource/No Ads cards keep their existing stable IDs and templates. Attribute-to-card grouping is not added here; this is not a full SR API/layout parity claim.

Authoring instructions and example: Assets/_Project/Debugger/README.md. All partial files must be in the same game-owned assembly; a game cannot extend a package class via partial across assembly boundaries.


Superseded by the complete partial/card migration in ../attribute-migration/README.md. This prototype receipt is historical.
