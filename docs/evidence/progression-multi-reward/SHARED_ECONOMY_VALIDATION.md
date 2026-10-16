# Shared Economy save — 2026-10-16

Working tree: Economy 0.5.0, Progression 0.2.1. No commit, tag or publish.

## Delivered

- Reusable ResourceRewardDefinition and DatasaveResourceWallet.CreateRewardDestination in Economy.
- One owner persists Coin/Gem/Card/Chest/Cosmetic and other defined resources in EconomySaveData v4.
  Adapters share balances, writer guard, receipts and import markers; no new save per destination.
- Checked stackable counts; unique ownership remains 1 while each entitlement gets its own receipt.
- ImportInventory migrates trusted source snapshots and original receipts atomically with a payload-bound marker.
- Reload reconciles an ambiguous write across all adapters, preserving owner identity and subscriptions.
- Existing currency, spend, exchange and UI contracts remain supported; every candidate retains inventory and import state.
- Foundation Bootstrap, Progression controls and Progression Rewards package/imported sample use the shared owner.
- Daily/Missions Foundation gateway's `item.demo-card` also moved from DemoItemDestination to the shared wallet.
- Source game DTOs remain only for reading legacy split saves; old files are retained, not written after cutover.

## Validation

- Managed Economy/Shop recovery harness: 76 NUnit tests passed, plus existing Shop presenter/model checks.
  Command: `python3 LocalPackages/com.dreamy.feature.economy/Tests~/validate-resource-recovery.py`.
- Managed Progression harness: 59 passed, 0 failed, one Unity-only disk fixture excluded.
- Direct NUnit invocation in Unity 6000.4.12f1 Editor: Economy 67 passed; Progression 60 passed.
  Includes file-backed wallet fixtures, explicit v3 envelope → v4 upgrade and inventory writes preserving currency/receipts.
  This is direct Test/TestCase execution with SetUp/TearDown, not a PlayMode Test Runner result.
- Unified tests cover interleaved five-kind writes, same-file persistence, restart/duplicate replay, unique items,
  unknown targets/kind mismatch, cumulative overflow, listener errors, before/after-write failures, one-time imports,
  changed import payload rejection, overlapping receipt rejection, and spend retaining inventory/import markers.
- Isolated Foundation disk smoke: migrate old card quantity + receipt; Card failure; 300 XP; retry duplicate XP;
  ack-write failure; 300 XP; recovery; final completion failure; 300 XP; recovery; all-owner restart.
  Final: XP 900; Coin 750 (includes initial 50); Card 16 (includes imported 2); Chest 7; Cosmetic 1.
  Original legacy inventory file bytes unchanged; original receipt returned AlreadyDelivered; one import marker.
- Bootstrap PlayMode smoke: Ready in MainScene after real migration; no Console errors.

## Real sandbox migration

Backed up relevant primary/backup saves first to:
`/tmp/foundation-economy-before-unify-20261016-040143`.

Live save root:
`/home/tutrinh/.config/unity3d/DefaultCompany/DreamyTemplateProject/DreamySaves`.

After migration: Coin 2400, Gem 111, Card 44, Chest 22, Cosmetic 1, Daily item 1;
Progression XP 3100 retained. Compared against pre-migration backup: all old currency balances unchanged,
all old inventory quantities matched, every original receipt copied byte-for-byte, all split source
files unchanged. Four durable import markers recorded in `economy.json`.

Old inventory files remain for rollback/diagnosis. New writers use only `economy.json` for resources.
Progression `foundation-progression-multi-reward-v2.json` keeps XP/level and delivery journals.
Reloading/importing the same frozen old files cannot add their counts twice.

## Boundaries

No player/device/cloud tests; no guarantee for restore of individual files from inconsistent generations.
One resource file does not make delivery atomic for a complete bundle; coordinator still commits per entry.
Host must keep one wallet instance per save key and retire old split writers before importing. Games requiring
per-instance metadata or backend inventory may provide custom destinations. Existing separate-owner recovery
protocol remains available. No production game data outside the authorized sandbox save root was touched.
