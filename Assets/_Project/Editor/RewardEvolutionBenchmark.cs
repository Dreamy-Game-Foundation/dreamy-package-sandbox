#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Missions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dreamy.Template.Editor
{
    /// <summary>Editor domain baseline, not filesystem DataSave, UI, or device profiling.</summary>
    public static class RewardEvolutionBenchmark
    {
        private const int OperationCount = 10000;
        private const int WarmupCount = 20;
        private const int ChunkSize = 8;
        private const uint Seed = 220014;
        private static readonly int[] Sizes = { 16, 100, 1000 };
        private static Session session;

        public static void Start(string outputPath, string sourceIdentity)
        {
            if (session != null) throw new InvalidOperationException("A benchmark is already running.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run the benchmark in Edit Mode.");
            if (string.IsNullOrWhiteSpace(sourceIdentity))
                throw new ArgumentException("Supply package SHAs plus dirty-source identity.", nameof(sourceIdentity));
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".omo"));
            string path = Path.GetFullPath(Path.IsPathRooted(outputPath) ? outputPath :
                Path.Combine(Application.dataPath, "..", outputPath));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The output must be a JSON file inside this project's .omo directory.");
            if (File.Exists(path))
                throw new ArgumentException("Use a fresh output path so the monitor cannot read a stale result.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            session = new Session { OutputPath = path };
            session.Result.SourceIdentity = sourceIdentity;
            session.Result.UnityVersion = Application.unityVersion;
            session.Result.Runtime = Environment.Version.ToString();
            session.Result.OS = SystemInfo.operatingSystem;
            session.Result.Processor = SystemInfo.processorType;
            session.Result.ProcessorCount = SystemInfo.processorCount;
            session.Result.ActiveBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            session.Result.StartedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
            EditorApplication.quitting += Cancel;
        }

        public static void Cancel()
        {
            if (session == null) return;
            session.Result.Failure = "Cancelled (explicit call, assembly reload, or Editor quit).";
            Finish(false);
        }

        private static void Update()
        {
            try
            {
                Session current = session;
                if (current.Run == null)
                {
                    current.Run = new Run(Sizes[current.SizeIndex]);
                    current.Result.Sizes.Add(current.Run.Result);
                    return;
                }
                Run run = current.Run;
                if (!run.Warmed)
                {
                    int end = Math.Min(run.Position + ChunkSize, WarmupCount);
                    while (run.Position < end) run.Invoke(run.Events[run.Position++]);
                    run.Result.ExecutedWarmupOperations = run.Position;
                    if (run.Position == WarmupCount) run.Warmed = true;
                    return;
                }
                if (!run.Measuring)
                {
                    run.ResetModel();
                    run.Measuring = true;
                    run.Position = 0;
                    run.CaptureGrowth();
                    return;
                }
                if (run.Position < OperationCount)
                {
                    int end = Math.Min(run.Position + ChunkSize, OperationCount);
                    while (run.Position < end) run.Measure();
                    if (run.Position % 1000 == 0) run.CaptureGrowth();
                    return;
                }
                run.Verify();
                if (!run.Result.Correct)
                    current.Result.Failure = "Correctness failed; inspect per-size Diagnostics.";
                current.Result.CompletedOperations += run.Position;
                current.Result.CompletedWarmupOperations += WarmupCount;
                current.Run = null;
                if (++current.SizeIndex == Sizes.Length) Finish(current.Result.Failure == null);
            }
            catch (Exception exception)
            {
                if (session == null) throw; // Publication failed after cleanup; keep that error visible.
                session.Result.Failure = exception.ToString();
                Finish(false);
            }
        }

        private static void Finish(bool success)
        {
            Session finished = session;
            session = null;
            EditorApplication.update -= Update;
            AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            EditorApplication.quitting -= Cancel;
            finished.Result.Success = success;
            finished.Result.FinishedUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            // Only result publication touches the filesystem, outside every measured operation.
            try
            {
                string temporary = finished.OutputPath + ".tmp";
                File.WriteAllText(temporary, JsonConvert.SerializeObject(finished.Result, Formatting.Indented),
                    new UTF8Encoding(false));
                File.Move(temporary, finished.OutputPath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[RewardEvolutionBenchmark] Result publication failed: " + exception);
                throw;
            }
            if (success) Debug.Log("[RewardEvolutionBenchmark] SUCCESS " + finished.OutputPath);
            else Debug.LogError("[RewardEvolutionBenchmark] FAILED " + finished.Result.Failure);
        }

        private sealed class Session
        {
            public string OutputPath;
            public int SizeIndex;
            public Run Run;
            public readonly Report Result = new Report();
        }

        private sealed class Run
        {
            public readonly SizeResult Result;
            public readonly BusinessEvent[] Events = new BusinessEvent[OperationCount];
            public int Position;
            public bool Warmed;
            public bool Measuring;
            private readonly MissionCatalogConfig catalog;
            private readonly long[] expectedProgress;
            private readonly HashSet<string>[] expectedDistinct;
            private readonly HashSet<string> expectedLedger = new HashSet<string>(StringComparer.Ordinal);
            private readonly List<string> expectedLedgerOrder = new List<string>();
            private readonly FixedClock clock = new FixedClock();
            private MissionModel model;
            private SerializedStore store;
            private FixedRandom random;
            private int notifications;

            public Run(int size)
            {
                Result = new SizeResult { CatalogSize = size };
                var definitions = new List<object>();
                expectedProgress = new long[size];
                expectedDistinct = new HashSet<string>[size];
                for (int i = 0; i < size; i++)
                {
                    expectedDistinct[i] = new HashSet<string>(StringComparer.Ordinal);
                    definitions.Add(new
                    {
                        id = "m" + i, titleKey = "benchmark." + i, eventKey = "key" + i,
                        target = 100000, objective = i < size / 2 ? 0 : 1,
                        rewardResourceId = "currency.coin", rewardAmount = 1
                    });
                }
                catalog = JsonConvert.DeserializeObject<MissionCatalogConfig>(JsonConvert.SerializeObject(new
                {
                    catalogId = "e22-benchmark-" + size,
                    revision = "fixture-v1",
                    period = new { Period = 1, OffsetMinutes = 0, CutoffMinutes = 0 },
                    missions = definitions
                }));
                ResetModel();
                string window = model.WindowId;
                var generator = new FixedRandom();
                for (int group = 0; group < OperationCount / 10; group++)
                {
                    int add = generator.Next(size / 2);
                    int distinct = size / 2 + generator.Next(size / 2);
                    int other = generator.Next(size / 2);
                    long amount = 1 + generator.Next(5);
                    string value = "value" + generator.Next(32);
                    int start = group * 10;
                    Events[start] = Event(add, "add" + group, window, amount, null, 0);
                    Events[start + 1] = Event(add, "add" + group, window, amount, null, 1);
                    Events[start + 2] = Event(distinct, "distinct" + group, window, 1, value, 2);
                    Events[start + 3] = Event(distinct, "repeat" + group, window, 1, value, 3);
                    Events[start + 4] = Event(distinct, "distinct" + group, window, 1, value, 4);
                    Events[start + 5] = Event(-1, "missing" + group, window, 1, null, 5);
                    Events[start + 6] = Event(add, "invalid" + group, window, 0, null, 6);
                    Events[start + 7] = Event(add, "wrong" + group, "wrong-window", 1, null, 7);
                    Events[start + 8] = Event(other, "other" + group, window, amount, null, 8);
                    // Long-distance replay proves the earliest ID is retained through ledger growth.
                    Events[start + 9] = Event(Events[0].Mission, "add0", window, 1, null, 9);
                    // Independent arithmetic/set oracle: no calls to MissionModel or its helpers.
                    expectedProgress[add] += amount;
                    expectedProgress[other] += amount;
                    expectedDistinct[distinct].Add(value);
                    foreach (int slot in new[] { 0, 2, 3, 8 })
                    {
                        string id = EncodedEvent(window, Events[start + slot].Id);
                        expectedLedger.Add(id);
                        expectedLedgerOrder.Add(id);
                    }
                }
                for (int i = size / 2; i < size; i++) expectedProgress[i] = expectedDistinct[i].Count;
                Result.FixtureSha256 = Hash(JsonConvert.SerializeObject(Events));
                Result.CatalogSha256 = Hash(JsonConvert.SerializeObject(catalog));
            }

            private static BusinessEvent Event(int mission, string id, string window, long amount,
                string value, int category) => new BusinessEvent
                {
                    Mission = mission, Key = mission < 0 ? "missing-key" : "key" + mission,
                    Id = id, Window = window, Amount = amount, Value = value, Category = category
                };

            public void ResetModel()
            {
                store = new SerializedStore();
                random = new FixedRandom();
                model = new MissionModel(catalog, store, new NoRewards(), clock: clock, random: random);
                notifications = 0;
                model.StateChanged += () => notifications++;
                Result.SetupSaves = store.Saves;
                Result.InitialRetainedSaveUtf8Bytes = store.RetainedBytes;
                store.Saves = 0;
                store.TotalBytes = 0;
                store.MaxBytes = 0;
            }

            public MissionProgressStatus Invoke(BusinessEvent businessEvent) => model.ReportProgress(
                businessEvent.Key, businessEvent.Id, "benchmark", businessEvent.Window,
                businessEvent.Amount, businessEvent.Value);

            public void Measure()
            {
                BusinessEvent businessEvent = Events[Position];
                Metric metric = Result.Categories[businessEvent.Category];
                int savesBefore = store.Saves;
                int notificationsBefore = notifications;
                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                long ticksBefore = Stopwatch.GetTimestamp();
                MissionProgressStatus actual = Invoke(businessEvent);
                long elapsedTicks = Stopwatch.GetTimestamp() - ticksBefore;
                long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                metric.Operations++;
                metric.ElapsedTicks += elapsedTicks;
                metric.AllocatedBytes += allocated;
                metric.Saves += store.Saves - savesBefore;
                Result.OperationElapsedTicks += elapsedTicks;
                Result.OperationAllocatedBytes += allocated;
                Result.ExecutedOperations = ++Position;
                MissionProgressStatus expected = businessEvent.Category == 1 || businessEvent.Category == 4 ||
                    businessEvent.Category == 9 ? MissionProgressStatus.AlreadyProcessed :
                    businessEvent.Category == 5 ? MissionProgressStatus.NoMatchingMission :
                    businessEvent.Category == 6 ? MissionProgressStatus.InvalidEvent :
                    businessEvent.Category == 7 ? MissionProgressStatus.WrongWindow : MissionProgressStatus.Updated;
                int expectedDelta = expected == MissionProgressStatus.Updated ? 1 : 0;
                if (actual != expected || store.Saves - savesBefore != expectedDelta ||
                    notifications - notificationsBefore != expectedDelta)
                    Result.Diagnostics.Add("Operation " + (Position - 1) + ": expected " + expected +
                        "/saves+notifications delta " + expectedDelta + ", actual " + actual + "/" +
                        (store.Saves - savesBefore) + "/" + (notifications - notificationsBefore));
                Result.SaveCount = store.Saves;
                Result.SerializedSaveUtf8Bytes = store.TotalBytes;
                Result.MaxSerializedSaveUtf8Bytes = store.MaxBytes;
            }

            public void CaptureGrowth()
            {
                MissionSaveData saved = store.Load<MissionSaveData>();
                int distinct = 0;
                foreach (MissionProgressData progress in saved.ActiveWindow.Missions.Values)
                    distinct += progress.DistinctValues.Count;
                Result.Growth.Add(new Growth
                {
                    Operations = Position, Saves = store.Saves, LedgerEntries = saved.ActiveWindow.ProcessedEventIds.Count,
                    DistinctEntries = distinct, MissionEntries = saved.ActiveWindow.Missions.Count,
                    RetainedSaveUtf8Bytes = store.RetainedBytes, RetainedSaveUtf16CodeUnits = store.Json.Length
                });
            }

            public void Verify()
            {
                MissionSaveData saved = store.Load<MissionSaveData>();
                Check(Position == OperationCount, "Exactly 10000 measured events");
                Check(store.Saves == 4000, "Exactly 4000 measured saves (including ledger-only distinct repeats)");
                Check(notifications == 4000, "Exactly 4000 state notifications");
                Check(saved.ActiveWindow.Missions.Count == Result.CatalogSize, "Persisted mission count");
                Check(saved.ActiveWindow.ProcessedEventIds.Count == 4000, "Exactly 4000 persisted event IDs");
                Check(expectedLedger.SetEquals(saved.ActiveWindow.ProcessedEventIds), "Exact persisted replay identity set");
                Check(saved.ProcessedEventIds.Count == 4000 && expectedLedger.SetEquals(saved.ProcessedEventIds),
                    "Legacy mirror retains the same ledger");
                for (int i = 0; i < expectedLedgerOrder.Count && i < saved.ActiveWindow.ProcessedEventIds.Count; i++)
                    Check(expectedLedgerOrder[i] == saved.ActiveWindow.ProcessedEventIds[i], "Ledger ordering " + i);
                long total = 0;
                int distinctTotal = 0;
                for (int i = 0; i < Result.CatalogSize; i++)
                {
                    MissionProgressData progress = saved.ActiveWindow.Missions["m" + i];
                    Check(progress.Progress == expectedProgress[i] && !progress.Claimed, "Persisted progress m" + i);
                    Check(expectedDistinct[i].SetEquals(progress.DistinctValues) &&
                        progress.DistinctValues.Count == expectedDistinct[i].Count, "Distinct values m" + i);
                    MissionProgressData mirror = saved.Missions["m" + i];
                    Check(mirror.Progress == expectedProgress[i] && !mirror.Claimed &&
                        expectedDistinct[i].SetEquals(mirror.DistinctValues), "Legacy progress mirror m" + i);
                    total += expectedProgress[i];
                    distinctTotal += expectedDistinct[i].Count;
                }
                int savesBefore = store.Saves;
                // Audit serialization/reconstruction outside the measured event stream.
                var restored = new MissionModel(catalog, store, new NoRewards(), clock: clock, random: random);
                var state = restored.GetState();
                Check(state.Count == Result.CatalogSize, "Restored public state count");
                foreach (MissionState mission in state)
                {
                    int index = int.Parse(mission.Definition.Id.Substring(1), CultureInfo.InvariantCulture);
                    Check(mission.Progress == expectedProgress[index] && !mission.IsClaimed,
                        "Restored public progress " + mission.Definition.Id);
                }
                Check(store.Saves == savesBefore, "Restore/GetState perform no extra saves");
                Check(random.Calls == 0, "No selection/reroll randomness consumed");
                Result.ExpectedProgressTotal = total;
                Result.ExpectedDistinctTotal = distinctTotal;
                foreach (MissionProgressData progress in saved.ActiveWindow.Missions.Values)
                {
                    Result.ActualProgressTotal += progress.Progress;
                    Result.ActualDistinctTotal += progress.DistinctValues.Count;
                }
                Result.ActualLedgerEntries = saved.ActiveWindow.ProcessedEventIds.Count;
                Result.PublicStateUtf8Bytes = Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(state));
                Result.FinalRetainedSaveUtf8Bytes = store.RetainedBytes;
                Result.RetainedSaveGrowthUtf8Bytes = store.RetainedBytes - Result.InitialRetainedSaveUtf8Bytes;
                Result.Correct = Result.Diagnostics.Count == 0;
            }

            private void Check(bool condition, string description)
            {
                Result.Assertions++;
                if (!condition) Result.Diagnostics.Add(description);
            }
        }

        private static string EncodedEvent(string window, string id)
        {
            var builder = new StringBuilder("reward-v1:");
            foreach (string field in new[] { "mission-event", "benchmark", window, id })
                builder.Append(field.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(field);
            return builder.ToString();
        }

        private static string Hash(string text)
        {
            using (var algorithm = System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }

        private sealed class FixedClock : IMissionClock
        {
            public DateTimeOffset UtcNow => new DateTimeOffset(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
        }

        private sealed class FixedRandom : IMissionRandom
        {
            private uint state = Seed;
            public int Calls;
            public int Next(int exclusiveMaximum)
            {
                Calls++;
                state = unchecked(state * 1664525u + 1013904223u);
                return (int)((state >> 8) % (uint)exclusiveMaximum);
            }
        }

        private sealed class NoRewards : IResourceWallet
        {
            public bool TryGrant(ResourceGrantRequest request) => throw new InvalidOperationException("Unexpected reward grant.");
            public bool TryExchange(ResourceExchangeRequest request) => throw new InvalidOperationException("Unexpected exchange.");
        }

        private sealed class SerializedStore : IDatasaveService
        {
            public string Json;
            public int Saves;
            public long TotalBytes;
            public long MaxBytes;
            public long RetainedBytes;
            public T Load<T>(string key = null) where T : SaveData, new() =>
                Json == null ? new T() : JsonConvert.DeserializeObject<T>(Json);
            public void Save<T>(T data, string key = null) where T : SaveData
            {
                Json = JsonConvert.SerializeObject(data);
                RetainedBytes = Encoding.UTF8.GetByteCount(Json);
                Saves++;
                TotalBytes += RetainedBytes;
                MaxBytes = Math.Max(MaxBytes, RetainedBytes);
            }
            public bool Exists(string key) => Json != null;
            public void SaveAll() => throw new NotSupportedException();
            public void Delete(string key) => throw new NotSupportedException();
            public void DeleteAll() => throw new NotSupportedException();
        }

        private sealed class BusinessEvent
        {
            public int Mission;
            public string Key;
            public string Id;
            public string Window;
            public long Amount;
            public string Value;
            public int Category;
        }

        private sealed class Metric
        {
            public string Category;
            public int Operations;
            public long ElapsedTicks;
            public long AllocatedBytes;
            public int Saves;
        }

        private sealed class Growth
        {
            public int Operations;
            public int Saves;
            public int LedgerEntries;
            public int DistinctEntries;
            public int MissionEntries;
            public long RetainedSaveUtf8Bytes;
            public int RetainedSaveUtf16CodeUnits;
        }

        private sealed class SizeResult
        {
            public int CatalogSize;
            public string FixtureSha256;
            public string CatalogSha256;
            public int ExecutedOperations;
            public int ExecutedWarmupOperations;
            public int SetupSaves;
            public int SaveCount;
            public long OperationElapsedTicks;
            public long OperationAllocatedBytes;
            public long SerializedSaveUtf8Bytes;
            public long MaxSerializedSaveUtf8Bytes;
            public long InitialRetainedSaveUtf8Bytes;
            public long FinalRetainedSaveUtf8Bytes;
            public long RetainedSaveGrowthUtf8Bytes;
            public long PublicStateUtf8Bytes;
            public long ExpectedProgressTotal;
            public int ExpectedDistinctTotal;
            public long ActualProgressTotal;
            public int ActualDistinctTotal;
            public int ActualLedgerEntries;
            public int Assertions;
            public bool Correct;
            public readonly List<string> Diagnostics = new List<string>();
            public readonly List<Growth> Growth = new List<Growth>();
            public readonly Metric[] Categories = CreateMetrics();
        }

        private static Metric[] CreateMetrics()
        {
            string[] names = { "additive", "immediate-additive-replay", "distinct-value",
                "distinct-repeat-new-id-ledger-only", "distinct-replay", "true-no-op-missing-key",
                "true-no-op-zero-amount", "true-no-op-wrong-window", "second-additive", "long-distance-replay" };
            var metrics = new Metric[names.Length];
            for (int i = 0; i < names.Length; i++) metrics[i] = new Metric { Category = names[i] };
            return metrics;
        }

        private sealed class Report
        {
            public bool Success;
            public string Failure;
            public string SourceIdentity;
            public string UnityVersion;
            public string Runtime;
            public string OS;
            public string Processor;
            public int ProcessorCount;
            public string ActiveBuildTarget;
            public string StartedUtc;
            public string FinishedUtc;
            public uint FixedSeed = Seed;
            public int[] RequestedCatalogSizes = RewardEvolutionBenchmark.Sizes;
            public int OperationsPerSize = OperationCount;
            public int WarmupOperationsPerSize = WarmupCount;
            public int OperationsPerUpdate = ChunkSize;
            public long StopwatchFrequency = Stopwatch.Frequency;
            public int CompletedOperations;
            public int CompletedWarmupOperations;
            public string Measurement = "Main-thread managed operation elapsed ticks and allocated bytes; includes in-memory JSON save serialization. Stopwatch elapsed is not exclusive CPU process time. Setup, warmup, oracle, growth snapshots, restore, state serialization, result-file IO and Editor scheduling excluded. No device/UI/GPU or real filesystem DataSave measurements.";
            public string Distribution = "1000 groups of 10 per size. Slots 0/8 additive (amount 1..5), 1/4/9 replay, 2 distinct, 3 repeated distinct value with new ID, 5 missing key, 6 zero amount, 7 wrong window. LCG 1664525*x+1013904223, high 24 bits modulo bucket; additive/distinct each occupy half the catalog, one matching mission per valid key; targets 100000, no selection/reroll/reward claims. Fixed daily UTC window, unbounded periodic event ledger.";
            public readonly List<SizeResult> Sizes = new List<SizeResult>();
        }
    }
}
#endif
