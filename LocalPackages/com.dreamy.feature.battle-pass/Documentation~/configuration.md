# Configuration

Register `BattlePassCatalogConfig` under `battlePassCatalog` before DataConfig initialization. Root JSON: `schemaVersion: 1`, `seasons: [...]`. Each definition has SeasonId, OccurrenceId, Revision, Title, PremiumEntitlementId, Mode, DurationDays (default14), GraceHours (default48), optional StartUtcTicks/EndUtcTicks, and ordered Tiers.

Mode is EnrollmentDuration (0) or FixedUtcWindow (1). Fixed UTC ticks must describe [start,end). Tiers have stable unique Id, strictly increasing positive RequiredXp and Free/Premium bundles (either may be absent; at least one required). Bundle Entries each have unique Id, KindId, stable ResourceId TargetId and positive long Amount. Concrete fixed bundles only; random rewards/choices are deferred.

Validate during config initialization, then freeze a full deep snapshot on enrollment. Definition removal/revision edits do not alter an enrolled season or pending claim. New config applies only to a newly authored occurrence. Do not reuse an occurrence ID for a new season, premium product or modified payload. Author all entitlement targets before wallet construction.

Sample config includes ten tiers at20XP intervals,14days+48hours and a preauthored premium entitlement. Its shop catalog key is `battlePassDemoShop`, separate from a game's existing shop catalog; merge the offer into the host catalog for production rather than create a second shop authority. Exactly one Resources document per registered key.
