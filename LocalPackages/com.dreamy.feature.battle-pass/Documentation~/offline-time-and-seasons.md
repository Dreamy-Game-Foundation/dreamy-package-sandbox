# Offline time and enrollment

The clock must return DateTimeKind.Utc. Effective time is max(clock.UtcNow, durable LastObservedUtcTicks); mutating commands persist a new high-water before checking eligibility. GetState/GetClaimOffer are read-only. Host should call Refresh on resume and meaningful clock boundaries; avoid saving countdown ticks every frame.

EnrollmentDuration starts on explicit host enrollment and runs DurationDays. FixedUtcWindow accepts enrollment only in [StartUtcTicks,EndUtcTicks). XP is accepted only before end. Unlocked claims remain allowed until [end,end+grace); at the exact deadline new claims are Expired. Premium purchase is host-gated to the active XP window. Old pending journal/batch recovery takes precedence and does not require the clock.

Only one active occurrence exists. After its claim deadline and after recovery, host may enroll another preauthored occurrence. Its XP/receipts are independent; previous occurrences and claimed tombstones remain available for replay detection. Calling Enroll on an already recorded occurrence never activates it again.

An offline local high-water reduces rollback reopening but cannot establish trusted time or stop save tampering/forward clock jumps. Server authority and cross-device merge are outside v1. The sample's Advance Day persists a demo offset in its independent profile, with no device-clock mutation.
