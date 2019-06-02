using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace SimilarTagsCalculator {
    [MemoryDiagnoser]
    public class Benchmark {
        SimilarTagsCalculator randomCalculator;
        SimilarTagsCalculator ascendantCalculator;
        SimilarTagsCalculator descendantCalculator;
        TagsGroup randomValue;
        TagsGroup allTagsTrue;

        [GlobalSetup]
        public void GlobalSetup() {
            randomCalculator = new SimilarTagsCalculator(Program.CreateRandomGroups(1000000));
            TagsGroup[] ascendantTestGroups = Program.CreateAscendantTestGroups(1000000);
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
            return randomCalculator.GetFiftyMostSimilarGroups(randomValue);
        }

        [Benchmark]
        public TagsGroup[] AscendantTest() {
            return ascendantCalculator.GetFiftyMostSimilarGroups(allTagsTrue);
        }
        
        [Benchmark]
        public TagsGroup[] DescendantTest() {
            return descendantCalculator.GetFiftyMostSimilarGroups(allTagsTrue);
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

    class Program {
        static Random rnd = new Random(31337);
        const int resultLength = 50;
        const int testGroupCount = 100000;        
        
        static void Main() {
#if DEBUG
            DoTests();
#else
            BenchmarkRunner.Run<Benchmark>();
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
            var result = calculator.GetFiftyMostSimilarGroups(etalon);
            if (dummyResult.Length != result.Length) {
                throw new Exception("Test failed");
            }
            for (int i = 0; i < dummyResult.Length; i++) {
                if (result[i] != dummyResult[i]) {
                    throw new Exception("Test failed");
                }
            }
            Console.WriteLine($"{testName} passed!");
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
        static readonly byte[] CountOfSettedBits = GenerateCountOfSettedBits();

        static byte[] GenerateCountOfSettedBits() {
            byte[] result = new byte[256];
            int[] b = new int[8];
            for (int i = 1; i < 256; i++) {
                int settedBitsCount = 0;
                int m = 1;
                for (int j = 0; j < 8; j++) {
                    b[j] += m;
                    m = b[j] >> 1;
                    b[j] = b[j] & 1;
                    settedBitsCount += b[j];
                }
                result[i] = (byte) settedBitsCount;
            }
            return result;
        }

        public const int TagsGroupLength = 4096;
        const int BucketSize = 8;
        byte[] InnerTags { get; }

        public static int MeasureSimilarity(TagsGroup a, TagsGroup b) {
            int result = 0;
            for (int i = 0; i < TagsGroupLength / BucketSize; i++) {
                int t = a.InnerTags[i] & b.InnerTags[i];
                result += CountOfSettedBits[t];
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
            InnerTags = new byte[TagsGroupLength / BucketSize];
            for (int i = 0; i < TagsGroupLength / BucketSize; i++) {
                for (int j = 0; j < BucketSize; j++, index++) {
                    InnerTags[i] <<= 1;
                    InnerTags[i] += (byte) (innerTags[index] ? 1 : 0);
                }
            }
        }
    }

    class SimilarTagsCalculator {
        struct TagsSimilarityInfo : IComparable<TagsSimilarityInfo>, IComparable {
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
            List<TagsSimilarityInfo> list = new List<TagsSimilarityInfo>();
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
    }
}