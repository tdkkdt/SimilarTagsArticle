using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace SimilarTagsCalculator {
    public enum SortingAlgorithm {
        List,
        SortedSet,
        Heap,
        Count,
        MultiThread
    }

    [MemoryDiagnoser]
    public class Benchmark {
        SimilarTagsCalculator randomCalculator;
        SimilarTagsCalculator ascendantCalculator;
        SimilarTagsCalculator descendantCalculator;
        TagsGroup randomValue;
        TagsGroup allTagsTrue;

        [Params(SortingAlgorithm.MultiThread /*SortingAlgorithm.List, SortingAlgorithm.SortedSet, SortingAlgorithm.Heap, SortingAlgorithm.Count*/)]
        public SortingAlgorithm SortingAlgorithm { get; set; }

        [Params(/*250000, */1000000)]
        public int GroupsCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup() {
            randomCalculator = new SimilarTagsCalculator(Program.CreateRandomGroups(GroupsCount));
            TagsGroup[] ascendantTestGroups = Program.CreateAscendantTestGroups(GroupsCount);
            TagsGroup[] descendantTestGroups = new TagsGroup[ascendantTestGroups.Length];
            Array.Copy(ascendantTestGroups, descendantTestGroups, ascendantTestGroups.Length);
            Array.Reverse(descendantTestGroups);
            ascendantCalculator = new SimilarTagsCalculator(ascendantTestGroups);
            descendantCalculator = new SimilarTagsCalculator(descendantTestGroups);
            allTagsTrue = new TagsGroup(Program.CreateAllTagsTrue());
        }

        [IterationSetup]
        public void IterationSetup() {
            randomValue = new TagsGroup(Program.GetRandomBools());
        }

        [Benchmark]
        public TagsGroup[] RandomTest() {
            switch (SortingAlgorithm) {
                case SortingAlgorithm.List:
                    return randomCalculator.GetFiftyMostSimilarGroups(randomValue);
                case SortingAlgorithm.SortedSet:
                    return randomCalculator.GetFiftyMostSimilarGroupsSortedSet(randomValue);
                case SortingAlgorithm.Heap:
                    return randomCalculator.GetFiftyMostSimilarGroupsHeap(randomValue);
                case SortingAlgorithm.Count:
                    return randomCalculator.GetFiftyMostSimilarGroupsCount(randomValue);
                case SortingAlgorithm.MultiThread:
                    return randomCalculator.GetFiftyMostSimilarGroupsMultiThread(randomValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Benchmark]
        public TagsGroup[] AscendantTest() {
            switch (SortingAlgorithm) {
                case SortingAlgorithm.List:
                    return ascendantCalculator.GetFiftyMostSimilarGroups(allTagsTrue);
                case SortingAlgorithm.SortedSet:
                    return ascendantCalculator.GetFiftyMostSimilarGroupsSortedSet(allTagsTrue);
                case SortingAlgorithm.Heap:
                    return ascendantCalculator.GetFiftyMostSimilarGroupsHeap(allTagsTrue);
                case SortingAlgorithm.Count:
                    return ascendantCalculator.GetFiftyMostSimilarGroupsCount(allTagsTrue);
                case SortingAlgorithm.MultiThread:
                    return ascendantCalculator.GetFiftyMostSimilarGroupsMultiThread(allTagsTrue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Benchmark]
        public TagsGroup[] DescendantTest() {
            switch (SortingAlgorithm) {
                case SortingAlgorithm.List:
                    return descendantCalculator.GetFiftyMostSimilarGroups(allTagsTrue);
                case SortingAlgorithm.SortedSet:
                    return descendantCalculator.GetFiftyMostSimilarGroupsSortedSet(allTagsTrue);
                case SortingAlgorithm.Heap:
                    return descendantCalculator.GetFiftyMostSimilarGroupsHeap(allTagsTrue);
                case SortingAlgorithm.Count:
                    return descendantCalculator.GetFiftyMostSimilarGroupsCount(allTagsTrue);
                case SortingAlgorithm.MultiThread:
                    return descendantCalculator.GetFiftyMostSimilarGroupsMultiThread(allTagsTrue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class BranchPredictionBenchmark {
        TagsGroup[] unsortedGroups;
        TagsGroup[] sortedGroups;
        TagsGroup etalon;

        [GlobalSetup]
        public void GlobalSetup() {
            unsortedGroups = Program.CreateRandomAscendantTestGroups(1000000);
            sortedGroups = Program.CreateAscendantTestGroups(1000000);
            etalon = new TagsGroup(Program.CreateAllTagsTrue());
        }
        
        int GetSimilaritySum(TagsGroup[] tagsGroups) {
            int result = 0;
            foreach (TagsGroup tagsGroup in tagsGroups) {
                result += TagsGroup.MeasureSimilarity(tagsGroup, etalon);
            }
            return result;
        }

        [Benchmark]
        public int Sorted() => GetSimilaritySum(sortedGroups);


        [Benchmark]
        public int Unsorted() => GetSimilaritySum(unsortedGroups);

    }

    public class SortBenchmark {
        [Params(50, 100, 1000, 10000, 100000, 1000000)]
        public int ItemsCount { get; set; }

        SimilarTagsCalculator.TagsSimilarityInfo[] randomGroups;
        SimilarTagsCalculator.TagsSimilarityInfo[] ascendantGroups;
        SimilarTagsCalculator.TagsSimilarityInfo[] descendantGroups;

        
        
        [GlobalSetup]
        public void GlobalSetup() {
            randomGroups = CalcTagsSimilarityInfo(Program.CreateAscendantTestGroups(ItemsCount));
            ascendantGroups = CalcTagsSimilarityInfo(Program.CreateAscendantTestGroups(ItemsCount));
            descendantGroups = new SimilarTagsCalculator.TagsSimilarityInfo[ItemsCount];
            Array.Copy(ascendantGroups, descendantGroups, ItemsCount);
            Array.Reverse(descendantGroups);
        }

        SimilarTagsCalculator.TagsSimilarityInfo[] CalcTagsSimilarityInfo(TagsGroup[] tagsGroups) {
            SimilarTagsCalculator.TagsSimilarityInfo[] result = new SimilarTagsCalculator.TagsSimilarityInfo[tagsGroups.Length];
            TagsGroup tagsGroup = new TagsGroup(Program.CreateAllTagsTrue());
            for (int i = 0; i < tagsGroups.Length; i++) {
                result[i] = new SimilarTagsCalculator.TagsSimilarityInfo(i, TagsGroup.MeasureSimilarity(tagsGroup, tagsGroups[i]));
            }
            return result;
        }

        static SimilarTagsCalculator.TagsSimilarityInfo[] ArraySort(SimilarTagsCalculator.TagsSimilarityInfo[] array) {
            SimilarTagsCalculator.TagsSimilarityInfo[] result = new SimilarTagsCalculator.TagsSimilarityInfo[array.Length];
            Array.Copy(array, result, array.Length);
            Array.Sort(result);
            return result;
        }

        static SimilarTagsCalculator.TagsSimilarityInfo[] CountSort(SimilarTagsCalculator.TagsSimilarityInfo[] array) {
            List<SimilarTagsCalculator.TagsSimilarityInfo>[] list = new List<SimilarTagsCalculator.TagsSimilarityInfo>[TagsGroup.TagsGroupLength + 1];
            foreach (var tagsSimilarityInfo in array) {
                List<SimilarTagsCalculator.TagsSimilarityInfo> l = list[tagsSimilarityInfo.Similarity];
                if (l == null) {
                    l = new List<SimilarTagsCalculator.TagsSimilarityInfo>();
                    list[tagsSimilarityInfo.Similarity] = l;
                }
                l.Add(tagsSimilarityInfo);
            }
            SimilarTagsCalculator.TagsSimilarityInfo[] result = new SimilarTagsCalculator.TagsSimilarityInfo[array.Length];
            for (int i = TagsGroup.TagsGroupLength, j = 0; i >= 0; i--) {
                if (list[i] == null)
                    continue;
                foreach (var info in list[i]) {
                    result[j++] = info;
                }
            }
            return result;
        }

        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] RandomArraySort() => ArraySort(randomGroups);
        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] RandomCountSort() => CountSort(randomGroups);
        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] AscendantArraySort() => ArraySort(ascendantGroups);
        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] AscendantCountSort() => CountSort(ascendantGroups);
        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] DescendantArraySort() => ArraySort(descendantGroups);
        [Benchmark]
        public SimilarTagsCalculator.TagsSimilarityInfo[] DescendantCountSort() => CountSort(descendantGroups);
    }

    class Program {
        static Random rnd = new Random(31337);
        const int resultLength = 50;
        const int testGroupCount = 100000;        
        
        static void Main() {
#if DEBUG
            DoTests();
#else
//            BenchmarkRunner.Run<SortBenchmark>();
            BenchmarkRunner.Run<Benchmark>();
            //BenchmarkRunner.Run<BranchPredictionBenchmark>();
#endif
        }

        static void DoTests() {
            RandomMeasureSimilarityTest();
            RandomTest();
            SpecialTest();
            AscendantTest();
        }

        static void RandomMeasureSimilarityTest() {
            for (int i = 0; i < 1000; i++) {
                bool[] aBools = GetRandomBools();
                bool[] bBools = GetRandomBools();
                TagsGroup a = new TagsGroup(aBools);
                TagsGroup b = new TagsGroup(bBools);
                int similarity = TagsGroup.MeasureSimilarity(a, b);
                int dummySimilarity = CalcSimilarityDummy(aBools, bBools);
                if (similarity != dummySimilarity) {
                    throw new Exception("Test failed");
                }
            }
            Console.WriteLine("Measure similarity test passed");
        }

        static int CalcSimilarityDummy(bool[] aBools, bool[] bBools) {
            return aBools.Where((t, i) => t && t == bBools[i]).Count();
        }

        static void RandomTest() {
            TagsGroup[] groups = CreateRandomGroups(testGroupCount);
            TestCore(groups, new TagsGroup(GetRandomBools()), "Random test");
        }

        static void SpecialTest() {
            TagsGroup[] groups = new TagsGroup[testGroupCount];
            for (int i = 0; i < testGroupCount; i++) {
                groups[i] = new TagsGroup(GetRandomBools());
            }
            bool[] fullTagsGroup = CreateAllTagsTrue();
            for (int i = 0; i < resultLength; i++) {
                groups[i * 100] = new TagsGroup(fullTagsGroup);
            }
            TestCore(groups, new TagsGroup(fullTagsGroup), "Special test");
        }

        static void AscendantTest() {
            TagsGroup[] groups = CreateRandomAscendantTestGroups(testGroupCount);
            TestCore(groups, new TagsGroup(CreateAllTagsTrue()), "Hard test");
        }

        static void TestCore(TagsGroup[] groups, TagsGroup etalon, string testName) {
            var dummyResult = GetDummyResult(groups, etalon);
            SimilarTagsCalculator calculator = new SimilarTagsCalculator(groups);
            TestCoreCore(dummyResult, calculator.GetFiftyMostSimilarGroupsMultiThread(etalon));
//            TestCoreCore(dummyResult, calculator.GetFiftyMostSimilarGroups(etalon));
//            TestCoreCore(dummyResult, calculator.GetFiftyMostSimilarGroupsSortedSet(etalon));
//            TestCoreCore(dummyResult, calculator.GetFiftyMostSimilarGroupsHeap(etalon));
//            TestCoreCore(dummyResult, calculator.GetFiftyMostSimilarGroupsCount(etalon));
            Console.WriteLine($"{testName} passed!");
        }

        static void TestCoreCore(TagsGroup[] dummyResult, TagsGroup[] result) {
            if (dummyResult.Length != result.Length) {
                throw new Exception("Test failed");
            }
            for (int i = 0; i < dummyResult.Length; i++) {
                if (result[i] != dummyResult[i]) {
                    throw new Exception("Test failed");
                }
            }
        }

        static TagsGroup[] GetDummyResult(TagsGroup[] groups, TagsGroup etalon) {
            List<(int index, TagsGroup tagsGroup, int similarity)> list =
                groups.Select((t, i) => (i, t, TagsGroup.MeasureSimilarity(t, etalon))).ToList();
            list.Sort((a, b) => {
                int similarityCompare = b.similarity.CompareTo(a.similarity);
                return similarityCompare == 0 ? a.index.CompareTo(b.index) : similarityCompare;
            });
            TagsGroup[] result = new TagsGroup[resultLength];
            for (int i = 0; i < resultLength; i++) {
                result[i] = list[i].tagsGroup;
            }
            return result;
        }

        internal static TagsGroup[] CreateRandomGroups(int groupsCount) {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            for (int i = 0; i < groupsCount; i++) {
                groups[i] = new TagsGroup(GetRandomBools());
            }
            return groups;
        }

        internal static TagsGroup[] CreateRandomAscendantTestGroups(int groupsCount) {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            int bucketsCount = groupsCount / TagsGroup.TagsGroupLength;
            int i = 0;
            for (int j = 0; j <= TagsGroup.TagsGroupLength && i < groupsCount; j++) {
                bool[] tags = GetRandomTrueBools(j);
                for (int k = 0; k < bucketsCount && i < groupsCount; k++, i++) {
                    groups[i] = new TagsGroup(tags);
                }
            }
            var fullTags = CreateAllTagsTrue();
            while (i < groupsCount) {
                groups[i++] = new TagsGroup(fullTags);
            }
            return groups;
        }

        internal static TagsGroup[] CreateAscendantTestGroups(int groupsCount) {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            int bucketsCount = groupsCount / TagsGroup.TagsGroupLength;
            int i = 0;
            for (int j = 0; j <= TagsGroup.TagsGroupLength && i < groupsCount; j++) {
                bool[] tags = GetTrueBools(j);
                for (int k = 0; k < bucketsCount && i < groupsCount; k++, i++) {
                    groups[i] = new TagsGroup(tags);
                }
            }
            var fullTags = CreateAllTagsTrue();
            while (i < groupsCount) {
                groups[i++] = new TagsGroup(fullTags);
            }
            return groups;
        }

        static bool[] GetTrueBools(int i) {
            bool[] result = new bool[TagsGroup.TagsGroupLength];
            for (int j = 0; j < i; j++) {
                result[j] = true;
            }
            return result;
        }

        static int[] indices;

        static bool[] GetRandomTrueBools(int n) {
            bool[] result = new bool[TagsGroup.TagsGroupLength];
            if (indices == null) {
                indices = new int[TagsGroup.TagsGroupLength];
                for (int i = 0; i < TagsGroup.TagsGroupLength; i++) {
                    indices[i] = i;
                }
            }
            for (int i = 0; i < TagsGroup.TagsGroupLength; i++) {
                int ti = rnd.Next(TagsGroup.TagsGroupLength);
                int t = indices[i];
                indices[i] = indices[ti];
                indices[ti] = t;
            }
            for (int i = 0; i < n; i++) {
                result[indices[i]] = true;
            }
            return result;
        }

        internal static bool[] CreateAllTagsTrue() {
            bool[] fullTagsGroup = new bool[TagsGroup.TagsGroupLength];
            for (int i = 0; i < TagsGroup.TagsGroupLength; i++) {
                fullTagsGroup[i] = true;
            }
            return fullTagsGroup;
        }           
        
        internal static bool[] GetRandomBools() {
            const int tagsGroupLength = TagsGroup.TagsGroupLength;
            bool[] result = new bool[tagsGroupLength];
            for (int i = 0; i < tagsGroupLength; i++) {
                result[i] = GetRandomBool();
            }
            return result;
        }

        static bool GetRandomBool() {
            return rnd.Next(100) > 50;
        }
    }

    public class TagsGroup {
        public const int TagsGroupLength = 4096;
        const int BucketSize = 64;
        ulong[] InnerTags { get; }

        public static int MeasureSimilarity(TagsGroup a, TagsGroup b) {
            int result = 0;
            for (int i = 0; i < TagsGroupLength / BucketSize; i++) {
                ulong t = a.InnerTags[i] & b.InnerTags[i];
                result += (int) System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount(t);
            }
            return result;
        }

        public TagsGroup(bool[] innerTags) {
            if (innerTags == null)
                throw new ArgumentException(nameof(innerTags));
            if (innerTags.Length != TagsGroupLength) {
                throw new ArgumentException(nameof(innerTags));
            }
            int index = 0;
            InnerTags = new ulong[TagsGroupLength / BucketSize];
            for (int i = 0; i < TagsGroupLength / BucketSize; i++) {
                for (int j = 0; j < BucketSize; j++, index++) {
                    InnerTags[i] <<= 1;
                    InnerTags[i] += (byte) (innerTags[index] ? 1 : 0);
                }
            }
        }
    }

    public class SimilarTagsCalculator {
        public struct TagsSimilarityInfo : IComparable<TagsSimilarityInfo>, IComparable {
            public int Index { get; }

            public int Similarity { get; }

            public TagsSimilarityInfo(int index, int similarity) {
                Index = index;
                Similarity = similarity;
            }

            public bool Equals(TagsSimilarityInfo other) {
                return Index == other.Index && Similarity == other.Similarity;
            }

            public override bool Equals(object obj) {
                return obj is TagsSimilarityInfo other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    return (Index * 397) ^ Similarity;
                }
            }

            public int CompareTo(TagsSimilarityInfo other) {
                int similarityComparison = other.Similarity.CompareTo(Similarity);
                return similarityComparison != 0 ? similarityComparison : Index.CompareTo(other.Index);
            }

            public int CompareTo(object obj) {
                if (ReferenceEquals(null, obj))
                    return 1;
                return obj is TagsSimilarityInfo other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(TagsSimilarityInfo)}");
            }

            public override string ToString() {
                return $"Index: {Index.ToString()}; Similarity: {Similarity.ToString()}";
            }
        }

        TagsGroup[] Groups { get; }

        public SimilarTagsCalculator(TagsGroup[] groups) {
            if (groups == null)
                throw new ArgumentException(nameof(groups));
            Groups = groups;
        }

        public TagsGroup[] GetFiftyMostSimilarGroups(TagsGroup value) {
            const int resultLength = 50;
            List<TagsSimilarityInfo> list = new List<TagsSimilarityInfo>(resultLength);
            for (int groupIndex = 0; groupIndex < Groups.Length; groupIndex++) {
                TagsGroup tagsGroup = Groups[groupIndex];
                int similarityValue = TagsGroup.MeasureSimilarity(value, tagsGroup);
                TagsSimilarityInfo newInfo = new TagsSimilarityInfo(groupIndex, similarityValue);
                if (list.Count == resultLength && list[resultLength - 1].CompareTo(newInfo) == -1) {
                    continue;
                }
                int index = ~list.BinarySearch(newInfo);
                list.Insert(index, newInfo);
                if (list.Count > resultLength) {
                    list.RemoveAt(resultLength);
                }
            }
            TagsGroup[] result = new TagsGroup[resultLength];
            for (int i = 0; i < resultLength; i++) {
                result[i] = Groups[list[i].Index];
            }
            return result;
        }

        public TagsGroup[] GetFiftyMostSimilarGroupsMultiThread(TagsGroup value) {
            const int resultLength = 50;
            const int threadsCount = 4;
            int bucketSize = Groups.Length / threadsCount;
            Task<List<TagsSimilarityInfo>>[] tasks = new Task<List<TagsSimilarityInfo>>[threadsCount];
            for (int i = 0; i < threadsCount; i++) {
                int leftIndex = i * bucketSize;
                int rightIndex = (i + 1) * bucketSize;
                tasks[i] = Task<List<TagsSimilarityInfo>>.Factory.StartNew(() => GetFiftyMostSimilarGroupsMultiThreadCore(value, leftIndex, rightIndex));
            }
            Task.WaitAll(tasks);
            List<TagsSimilarityInfo>[] taskResults = new List<TagsSimilarityInfo>[threadsCount];
            for (int i = 0; i < threadsCount; i++) {
                taskResults[i] = tasks[i].Result;
            }
            return MergeTaskResults(resultLength, threadsCount, taskResults);
        }

        TagsGroup[] MergeTaskResults(int resultLength, int threadsCount, List<TagsSimilarityInfo>[] taskResults) {
            TagsGroup[] result = new TagsGroup[resultLength];
            int[] indices = new int[threadsCount];
            for (int i = 0; i < resultLength; i++) {
                int minIndex = 0;
                TagsSimilarityInfo currentBest = taskResults[minIndex][indices[minIndex]];
                for (int j = 0; j < threadsCount; j++) {
                    var current = taskResults[j][indices[j]];
                    if (current.CompareTo(currentBest) == -1) {
                        minIndex = j;
                        currentBest = taskResults[minIndex][indices[minIndex]];
                    }
                }
                int groupIndex = currentBest.Index;
                result[i] = Groups[groupIndex];
                indices[minIndex]++;
            }
            return result;
        }

        List<TagsSimilarityInfo> GetFiftyMostSimilarGroupsMultiThreadCore(TagsGroup value, int leftIndex, int rightIndex) {
            const int resultLength = 50;
            List<TagsSimilarityInfo> list = new List<TagsSimilarityInfo>(resultLength);
            for (int groupIndex = leftIndex; groupIndex < rightIndex; groupIndex++) {
                TagsGroup tagsGroup = Groups[groupIndex];
                int similarityValue = TagsGroup.MeasureSimilarity(value, tagsGroup);
                TagsSimilarityInfo newInfo = new TagsSimilarityInfo(groupIndex, similarityValue);
                if (list.Count == resultLength && list[resultLength - 1].CompareTo(newInfo) == -1) {
                    continue;
                }
                int index = ~list.BinarySearch(newInfo);
                list.Insert(index, newInfo);
                if (list.Count > resultLength) {
                    list.RemoveAt(resultLength);
                }
            }
            return list;
        }

        public TagsGroup[] GetFiftyMostSimilarGroupsSortedSet(TagsGroup value) {
            const int resultLength = 50;
            SortedSet<TagsSimilarityInfo> sortedSet = new SortedSet<TagsSimilarityInfo>();
            for (int groupIndex = 0; groupIndex < Groups.Length; groupIndex++) {
                TagsGroup tagsGroup = Groups[groupIndex];
                int similarityValue = TagsGroup.MeasureSimilarity(value, tagsGroup);
                TagsSimilarityInfo newInfo = new TagsSimilarityInfo(groupIndex, similarityValue);
                if (sortedSet.Count == resultLength && sortedSet.Max.CompareTo(newInfo) == -1) {
                    continue;
                }
                sortedSet.Add(newInfo);
                if (sortedSet.Count > resultLength) {
                    sortedSet.Remove(sortedSet.Max);
                }
            }
            TagsGroup[] result = new TagsGroup[resultLength];
            int i = 0;
            foreach (var info in sortedSet) {
                result[i++] = Groups[info.Index];
            }
            return result;
        }

        public TagsGroup[] GetFiftyMostSimilarGroupsHeap(TagsGroup value) {
            const int resultLength = 50;
            BinaryHeap<TagsSimilarityInfo> binaryHeap = new BinaryHeap<TagsSimilarityInfo>(50);
            for (int groupIndex = 0; groupIndex < Groups.Length; groupIndex++) {
                TagsGroup tagsGroup = Groups[groupIndex];
                int similarityValue = TagsGroup.MeasureSimilarity(value, tagsGroup);
                TagsSimilarityInfo newInfo = new TagsSimilarityInfo(groupIndex, similarityValue);
                if (binaryHeap.Count == resultLength && binaryHeap.Max.CompareTo(newInfo) == -1) {
                    continue;
                }
                binaryHeap.Add(newInfo);
                if (binaryHeap.Count > resultLength) {
                    binaryHeap.RemoveMax();
                }
            }
            TagsGroup[] result = new TagsGroup[resultLength];
            List<TagsSimilarityInfo> list = new List<TagsSimilarityInfo>(binaryHeap);
            list.Sort();
            for (int i = 0; i < resultLength; i++) {
                result[i] = Groups[list[i].Index];
                binaryHeap.RemoveMax();
            }
            return result;
        }

        public TagsGroup[] GetFiftyMostSimilarGroupsCount(TagsGroup value) {
            const int resultLength = 50;
            List<int>[] buckets = new List<int>[TagsGroup.TagsGroupLength + 1];
            for (int groupIndex = 0; groupIndex < Groups.Length; groupIndex++) {
                var tagsGroup = Groups[groupIndex];
                int similarityValue = TagsGroup.MeasureSimilarity(value, tagsGroup);
                List<int> bucket = buckets[similarityValue];
                if (bucket == null) {
                    bucket = new List<int>();
                    buckets[similarityValue] = bucket;
                }
                bucket.Add(groupIndex);
            }
            TagsGroup[] result = new TagsGroup[resultLength];
            for (int i = TagsGroup.TagsGroupLength, j = 0; i >= 0 && j < resultLength; i--) {
                if (buckets[i] == null)
                    continue;
                for (int index = 0; index < buckets[i].Count && j < resultLength; index++) {
                    int groupIndex = buckets[i][index];
                    result[j++] = Groups[groupIndex];
                }
            }
            return result;
        }
    }

    public class BinaryHeap<T>:IEnumerable<T> where T : IComparable<T> {
        readonly List<T> innerList;

        public BinaryHeap(int capacity) {
            innerList = new List<T>(capacity);
        }

        public int Count => innerList.Count;

        public T Max => innerList[0];

        public void Add(T value) {
            innerList.Add(value);
            int i = Count - 1;
            int parent = (i - 1) >> 1;

            while (i > 0 && innerList[parent].CompareTo(innerList[i]) == -1) {
                Swap(i, parent);

                i = parent;
                parent = (i - 1) >> 1;
            }
        }

        void Swap(int a, int b) {
            T temp = innerList[a];
            innerList[a] = innerList[b];
            innerList[b] = temp;
        }

        void Heapify(int i) {
            for (;;) {
                int leftChild = (i << 1) | 1;
                int rightChild = (i + 1) << 1;
                int largestChild = i;

                if (leftChild < Count && innerList[leftChild].CompareTo(innerList[largestChild]) == 1) {
                    largestChild = leftChild;
                }

                if (rightChild < Count && innerList[rightChild].CompareTo(innerList[largestChild]) == 1) {
                    largestChild = rightChild;
                }

                if (largestChild == i) {
                    break;
                }
                Swap(i, largestChild);
                i = largestChild;
            }
        }

        public void RemoveMax() {
            innerList[0] = innerList[Count - 1];
            innerList.RemoveAt(Count - 1);
            Heapify(0);
        }

        public IEnumerator<T> GetEnumerator() {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) innerList).GetEnumerator();
        }
    }
}