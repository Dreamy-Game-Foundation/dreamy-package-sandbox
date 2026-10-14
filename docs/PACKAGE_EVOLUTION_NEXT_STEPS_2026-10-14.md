# Ban giao packages, Template va Planet Bunker

Ngay: 2026-10-14.

## Trang thai dung

Nguoi dung yeu cau dung phat trien, luu toan bo thay doi cua cong viec nay va
push len nhanh rieng, khong phai main. Day la checkpoint WIP, khong phai release.
Tac vu battle bridge bi huy khi dang viet source/tests. Khong coi source do la
implementation da hoan tat, compile thanh cong hay du 10 objective da chay that.

File nay la diem tiep tuc moi nhat. Ke hoach goc van o
`.omo/plans/dreamy-package-evolution-2026-10-14.md`; cac checkbox va
`execution-state.json` la snapshot cu, khong phai chung nhan trang thai cuoi.
Nhung ghi chu cu "chua duoc phep commit/push" da duoc thay the bang yeu cau
commit/push nhanh rieng ngay 2026-10-14. Khong co yeu cau merge, tao tag hay release.

## Repository va nhanh

| Repository | Thu muc local | Nhanh checkpoint |
| --- | --- | --- |
| Sandbox | `D:/Game/dreamy-package-sandbox` | `feature/package-evolution-20261014` |
| Feature | `LocalPackages/com.dreamy.feature` trong sandbox | `feature/reward-evolution-20261014` |
| Economy | `LocalPackages/com.dreamy.feature.economy` | `feature/reward-evolution-20261014` |
| Daily Reward | `LocalPackages/com.dreamy.feature.daily-reward` | `feature/reward-evolution-20261014` |
| Missions | `LocalPackages/com.dreamy.feature.missions` | `feature/reward-evolution-20261014` |
| Progression | `LocalPackages/com.dreamy.feature.progression` | `feature/reward-evolution-20261014` |
| Shop | `LocalPackages/com.dreamy.feature.shop` | `feature/reward-evolution-20261014` |
| Planet Bunker | `D:/Game/Planet Bunker` | `feature/reward-evolution-20261014` |
| Template consumer | `D:/Game/dreamy-base-project/DreamyTemplateProject` | `feature/reward-evolution-20261014` |

Sandbox theo doi package bang git submodule. Commit package truoc, sau do commit
gitlink cua sandbox; checkout dung gitlink, khong tu dong doi ve main. Gitlink
Settings khac baseline da co tu truoc va duoc giu nguyen.

Khong gom working tree khong thuoc cong viec nay:
`D:/Game/dreamy-base-project` o nhanh dev va
`D:/Game/dreamy-template-project` o nhanh main. Template consumer dung la repo
long trong thu muc `dreamy-base-project/DreamyTemplateProject`, khong phai hai
repo tren. `Assets/generation-report.json` va meta da co san trong consumer nay;
duoc giu trong checkpoint theo yeu cau luu toan bo thay doi.

Consumer manifest dang dung local file dependencies, khong phai release pins.
Can giu layout thu muc tuong ung hoac thay bang exact commit SHA cua cac nhanh
checkpoint khi tiep tuc. Khong coi clone consumer rieng le la da tai du package.
Khong sua tag da phat hanh de tro vao code moi.

### Commit checkpoint da tao truoc khi push

| Repository | Commit |
| --- | --- |
| Feature | `3560c8a20e4feaedc71539833c004f9dcbcdd2b7` |
| Economy | `1049959c1b1b21bcd2985b411eec7f4476f16a8f` |
| Daily Reward | `3727c781ca567cd5eb1fad579ae3df2bcfa81512` |
| Missions | `859098e69dd8b77ae8a3934f8f7a19b995db2331` |
| Progression | `069ca80a49a407f88c8fd631698a4350de225038` |
| Shop | `c66e718663094021464b1ed54cc143a506f7d688` |
| Planet Bunker | `8cb3e5f8d7a7a45aeffb61990aedd5f850dc7f23` |
| Template | `083d116323b9df3d75291d49b792e670f6f01eff` |

Commit sandbox chua file nay va gitlinks moi duoc tao sau cac commit tren.
Ket qua push phai doi chieu remote branch SHA; bang nay chi ghi local commits,
khong tu no chung minh push thanh cong.

## Da co source va bang chung gi

Bang chung nam tai `.omo/evidence/dreamy-package-evolution-2026-10-14/`.
Ket qua duoi day chi ap dung cho source tai thoi diem run. Cac thay doi sau run,
dac biet Daily resolver va consumer migrations/battle, can chay lai tests.

| Hang muc | Da co | Gioi han |
| --- | --- | --- |
| Feature | Dependency DataConfig trong package.json theo asmdef hien co | Can fresh-consumer resolve gate |
| Economy | Typed bundle, identity/fingerprint, embedded journal/coordinator, detached wallet candidate, strict receipts va tests | `A/economy-tests.json`: 41/41 EditMode; khong chung minh backend that |
| Daily package | Bundle config/save, recovery, presenter/sample va resolver boundary | `B/daily-unity-tests.json`: 35/35 PlayMode truoc cac resolver regressions cuoi; can rerun |
| Missions package | Permanent/Daily/Weekly, clock/window, selection, reroll, additive/distinct, dedup va reward journal | `C/missions-unity-tests.json`: 52/52 EditMode; khong chung minh game bridge/cutover |
| Progression | Boundary/XP comparison tests; khong doi runtime ownership | `D/xp-playmode-tests.json`: Daily 35 + Progression 21 pass; khong phai PB legacy-save integration |
| Shop | Test assembly discovery configuration va gap report | Khong co durable IAP fulfillment moi; khong phai SDK certification |
| Samples | Source/imported Daily va Missions scripts/prefabs; periodic standalone panel va screenshots | Demo item owner chi la fixture; khong phai production resolver |
| Planet Bunker | Currency/card owner adapters, Daily strict facade/migration, Quest strict facade/migration, tests va partial battle bridge | Chua co final consumer compile/test/Home/runtime proof |
| Template | Local package wiring, Daily Stop/reset va Permanent Missions config/source/tests | Prefab/Home/Addressables adoption va actual consumer QA chua xong |

Khong cong cac test count tren thanh mot final-suite result. Khong dung static
asmdef check, generated csproj hay source review thay cho Unity compilation.
LSP daemon da khong reachable; khong co C# language-server sign-off.

## Viec tiep theo voi packages

### 1. Dong correctness gate tren dung source checkpoint

- [ ] Resolve sandbox va kiem tra Console/compilation; chay lai Economy,
  Daily, Missions, Progression, Shop voi nonzero discovery, failed/skipped count
  va source SHA ro rang. Daily/Progression da duoc phat hien o PlayMode;
  khong ep filter EditMode roi coi zero tests la pass.
- [ ] Chay cac Daily resolver regressions moi: intent phai persist truoc
  authority call; fixed concrete payload phai persist truoc grant; retry/restart
  dung cung request ID, khong resolve lai payload da Resolved.
- [ ] Chay lai crash matrix: owner save, journal acknowledgement va final claim
  commit that bai; recreate tat ca owner/model tu persisted data. Khong nang
  legacy bool wallet thanh lifetime-idempotent gateway.
- [ ] Import tung sample moi tren consumer sach; kiem missing scripts/references,
  GUID, Pending/retry va close/reopen. Kiem lai label trang thai sau thay doi
  hien thi cuoi, khong dung anh truoc thay doi lam bang chung.
- [ ] Giu Economy contract/coordinator khong biet game; journal van embed trong
  feature save, currency/card mutation va receipt trong chinh owner document.
  Khong tao journal.save/inventory.save hoac generic workflow framework moi.

### 2. Benchmark va optimization

Baseline that:
`E/baseline-1792006778096.json`, fixture
`Assets/_Project/Editor/RewardEvolutionBenchmark.cs`.
Run ket thuc Success=true: 30000 measured operations, 60 warmup operations;
ca ba catalog deu Correct=true, diagnostics rong, 4000 saves/catalog.

| Catalog | Operation elapsed tong | Final retained save UTF8 bytes | Progress total |
| ---: | ---: | ---: | ---: |
| 16 | 67549.747 ms | 1047463 | 6388 |
| 100 | 96276.2612 ms | 1083808 | 6891 |
| 1000 | 468555.038 ms | 1376419 | 7105 |

Day la thoi gian managed operation tren Editor, gom JSON serialization cua
in-memory store; khong phai exclusive CPU, filesystem IO, heap size hay device
performance. Source/environment/fixture hashes va category timings o raw JSON.
Warmup, oracle, result IO va Editor scheduling gaps khong nam trong interval.

Allocation counter tra 0 la **khong hop le de ket luan zero allocation**:
calibration tao byte[1048576] nhung GetAllocatedBytesForCurrentThread van bao 0;
native ProfilerRecorder CurrentValue cung bao delta 0 du Valid=true.
SessionState cua Editor da luu hai probe tai
`Dreamy.Evolution.AllocationProbe` va
`Dreamy.Evolution.NativeAllocationProbe`; day khong phai artifact ben vung.

- [ ] Thu GC/heap allocation capture bang profiler da calibrate; ghi unsupported
  neu khong co provider do hop le. Khong xuat con so zero gia.
- [ ] Dieu tra hotspot `MissionModel.Report`: predicate hien kiem
  `SelectedIds.Contains(m.Id)` truoc `m.EventKey == eventKey`, gay scan long nhau
  ngay ca missing-key. Thu thay doi nho nhat truoc khi them index/cache.
  JSON Clone/Commit va ledger growth cung can tach rieng khi do.
- [ ] Chua co optimization nao duoc chung minh bang before/after. Dung lai dung
  fixture/seed/event hashes va moi truong; giu correctness totals/save counts,
  replay guarantee va failure recovery. Khong prune reward receipts de lay toc do.
- [ ] Do UI rebuild/listener/object/tween/Addressables handles qua 100 lan
  open/close tren panel that. Factory-host unit tests khong thay the actual
  Addressables lifetime proof; thu desktop/mobile/safe area.

### 3. Progression va Shop

Doc `D/conditional-gaps.md` truoc khi mo rong scope.

- [ ] Giu `PlayerProgressSave` la authority XP/level cua PB. Chua co capability
  gap buoc phai rewrite Progression runtime. Neu adopt, preserve legacy XP floor
  (level 25 -> XP 7920), int saturation va match eligibility; khong tao save XP thu hai.
- [ ] Giu virtual purchase la mot `TryExchange` atomic. Khong tach debit va
  reward thanh nhieu save/grant de dung bundle.
- [ ] Neu tiep tuc IAP recovery, can Shop-owned pending receipt va immutable
  reward snapshot, real transaction identity, legacy entry-index alias/tombstone,
  cancellation/redelivery/restore tests. Khong doi namespace de cap lai reward.
- [ ] Chua co SDK/device receipt proof; bridge/simulated billing local khong the
  dong gate production. Khong bat mixed-owner paid purchases khi chua co contract.

## Viec tiep theo voi Planet Bunker

Baseline checkout khi dung la `2330bf8a610a77c678986f71728c66d38708aa91`,
khac baseline ke hoach `20e1264e12ad971d9bc02ca33fc5458df55c8b33`.
Khong restore mu quan cac file installer/wallet da bi xoa o checkout hien tai.

### P0: Hoan tat integration dang do truoc khi choi/test cutover

- [ ] Unity resolve `Packages/manifest.json` va regenerate lock tren dung
  checkpoint; kiem cac foundation pins hien tai thay vi gia dinh baseline cu.
  Khong broad-upgrade hay sua Library/PackageCache.
- [ ] Import cac file moi, tao metadata con thieu bang Unity; compile va chay
  host suites: `HostRewardDestinationTests`, `DailyRewardServiceStrictTests`,
  `QuestServiceStrictTests`, `BattleQuestProgressTests` va existing regressions.
  Chung moi duoc authored, chua co final test run.
- [ ] `GameInstaller.InstallQuestService` van dung constructor legacy
  `new QuestService(questTable, datasave)` va subscribe
  `BattleCompleted += questService.RecordBattleCompleted`. Battle bridge moi
  lai can strict `GetWindowId/ReportProgress`. Wiring nay **chua cutover xong**.
  Doi sang gateway constructor sau khi migration/recovery duoc chung minh;
  thay subscription cu dung luc, tranh duplicate completion/win writer.
- [ ] Review va hoan tat phan battle bi huy: `BattleQuestProgress`,
  `BattleQuestTerrainDestruction` va thay doi trong Character/Controller/Roster/
  Projectile/Damage/Explosion/Burn/EnergyNode. Khong mac dinh no compile hay
  bao toan mechanics chi vi file da ton tai.
- [ ] Producer-to-service tests cho du 10 objective: PlayMatch, WinMatch,
  FireShot, DealDamage, DestroyUnit, ExposeUnit, CollectEnergyNode, HitUnit,
  ExposeByTerrain, UseDifferentUnit. Confirm player/enemy/AI filtering,
  terrain cause, duplicate callbacks, distinct-value va original-window replay.
  Expose phai bam vao transition buried -> exposed that, khong doan tu active team.
- [ ] Review identity capture truoc delayed callback/cutoff. Nonce tao mot lan
  moi battle va ordinal in-memory chi co guarantee trong battle khong resumable;
  khong coi do la durable backend replay log hay process-death recovery.

### Save ownership va migration

- [ ] Preserve legacy `ResourceSave.WalletTransactionIds` va `WalletBalances`
  neu profile cu co cac truong nay. Schema hien tai them `RewardReceipts` nhung
  chua import hai truong legacy; doc `D/conditional-gaps.md` truoc adoption.
  WalletBalances cu la `Dictionary<string,long>`. ID-only receipts khong du
  de doan fingerprint; strict collision phai reject/reconcile, khong regrant.
- [ ] Test currency aliases, overflow int/long, card unknown/overflow va save
  faults/recreation. Mutation + receipt phai cung ResourceSave/UnitProgressSave;
  khong tao economy.save thay the game authority.
- [ ] Daily: verify migration marker/destination atomic, migrate hai lan,
  missing historical cycle khong duoc doan; UTC seven-day Repeat/reset=false,
  rollback/midnight va pending original claim date.
- [ ] Quest: verify snapshot truoc normalization/rollover, selected order,
  progress, distinct values, claimed va reroll; unknown IDs/revisions phai ro loi,
  khong reset. Daily UTC+7, Weekly Monday; 3/6 va 5/10, PreferGroupsThenRelax.
- [ ] Quest card flow da authored: persist Intent, authority resolve cung request,
  save concrete bundle trong QuestSave, recreate period model de Missions persist
  resolved journal truoc delivery. Chua can them shared resolver abstraction chi
  de giai quyet consumer nay; truoc het test flow da co.
- [ ] Backend mystery-card authority chua duoc xac minh. Missing resolver phai
  giu Pending/unavailable, khong random client fallback hay grant placeholder.
  Kiem reason/error duoc hien thi ro va retry khong doi request/payload.

### Home, runtime va rollout

- [ ] Hoan tat selective sample/UI adoption, giu custom art/GUID/Resources keys;
  Home -> Daily/Quest -> Back, daily/weekly tab, reroll, claim, retry/restart va
  repeated opens phai dung mot service/writer; khong de legacy va strict cung grant.
- [ ] Backup whole save generation tren copy truoc migration; rehearse rollback
  code/pins/assets + toan bo owner saves cung generation. Khong rollback journal
  rieng hay chi ha tag; neu can giu progress sau upgrade thi phai co strategy rieng.
- [ ] Packed Android Addressables, player compile/device smoke va injected SDK
  receipt/restore/process-death la cac gate rieng chua chay.

## Template consumer can xong truoc PB rollout

Doc `docs/reward-evolution-source-rehearsal-20261014.md` trong Template repo.
Source da co Home buttons fields/routes/factories, nhung prefab va Addressables
adoption chua duoc root hoan tat. Editor da duoc mo de resolve; lan quan sat cuoi
chua co ready/compile/test proof cho consumer nay, khong ket luan startup thanh cong.

- [ ] Resolve lock, kiem Console/compilation va dung local source refs.
- [ ] Tao/rebind Daily/Missions panel + row prefabs, Home buttons va Addressables
  keys theo runbook; dung adoption script GUIDs, khong copy sample GUID mu quan.
- [ ] Chay sau test moi va existing ownership/Home/Shop/save suites.
- [ ] Test Daily StopAfterLastDay/reset=true va Permanent Missions, shared wallet,
  Home/Back/Shop/Settings; day la policy khac PB.
- [ ] Actual panel 100-cycle lifecycle, visual QA va player build. Khong dung
  simulated billing hoac fixture event buttons de chung minh gameplay/SDK.

## Thu tu tiep tuc va release gate

1. Checkout dung checkpoint branches va sandbox submodule SHAs.
2. Resolve/compile va focused suites tren source cuoi; sua integration dang do.
3. Dong owner/migration/battle correctness gates truoc PB cutover.
4. Hoan tat Template prefab/Home/runtime rehearsal, roi PB copy rehearsal.
5. Calibrated benchmark/profiler + measured optimization + real UI lifetime.
6. Rollback rehearsal tren copy, sau do moi chot release metadata/pins/rollout.

Tat ca tasks 25-30 va final gates F1-F4 cua ke hoach van can audit evidence.
Versions hien tai khong tu dong dai dien capability moi. De xuat Economy 0.3.0,
Daily/Missions 0.2.0 va Feature 0.1.1 trong plan chua phai release da publish.
Branch push chi luu WIP, khong dong device/SDK gates, khong merge main va
khong cho phep overwrite tags. Khi tiep tuc, ghi exact commit/lock hashes,
test discovery/count/outcome va nhung gi chua chay thay vi danh dau tat ca done.

## Kiem tra checkpoint truoc commit

Khong co unmerged Git entries. Kiem tra staged `git diff --cached --check`
phat hien trailing whitespace trong cac prefab/meta moi do Unity sinh, gom ban
source va imported sample, Economy folder/test metadata va
`Assets/generation-report.json.meta` da co san cua Template. Cac file nay duoc
giu nguyen, khong rewrite serialized assets chi de doi formatting. Day la finding
con lai, khong phai mot whitespace check pass tren toan bo staged snapshot.
Khong chay lai build/tests trong lan dung va luu checkpoint nay.

Cache, Library, Temp va log bi .gitignore loai tru khong duoc force-add.
Pattern scan private-key/GitHub-token/GitLab-token/AWS-access-key tren cac text
candidate khong tim thay match; day khong phai comprehensive security audit.
