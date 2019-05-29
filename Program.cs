//|         Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
//|--------------- |---------:|---------:|---------:|------:|------:|------:|----------:|
//|     RandomTest | 17.536 s | 0.0249 s | 0.0221 s |     - |     - |     - |    2.8 KB |
//|  AscendantTest |  3.543 s | 0.0229 s | 0.0214 s |     - |     - |     - |    2.8 KB |
//| DescendantTest |  3.549 s | 0.0131 s | 0.0116 s |     - |     - |     - |    2.8 KB |

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
            ascendantCalculator = new SimilarTagsCalculator(Program.CreateAscendantTestGroups(1000000));
            descendantCalculator = new SimilarTagsCalculator(Program.CreateDescendantTestGroups(1000000));
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

    class Program {
        static Random rnd = new Random(31337);
        const int resultLength = 50;
        const int groupsCount = 100000;        
        
        static void Main() {
#if DEBUG
            DoTests();
#else
            BenchmarkRunner.Run<Benchmark>();
#endif
        }

        static void DoTests() {
            RandomTest();
            SpecialTest();
            AscendantTest();
        }

        static void RandomTest() {
            TagsGroup[] groups = CreateRandomGroups(groupsCount);
            TestCore(groups, new TagsGroup(GetRandomBools()), "Random test");
        }

        static void SpecialTest() {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            for (int i = 0; i < groupsCount; i++) {
                groups[i] = new TagsGroup(GetRandomBools());
            }
            bool[] fullTagsGroup = CreateAllTagsTrue();
            for (int i = 0; i < resultLength; i++) {
                groups[i * 100] = new TagsGroup(fullTagsGroup);
            }
            TestCore(groups, new TagsGroup(fullTagsGroup), "Special test");
        }

        static void AscendantTest() {
            TagsGroup[] groups = CreateAscendantTestGroups(groupsCount);
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

        internal static TagsGroup[] CreateAscendantTestGroups(int groupsCount) {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            int bucketsCount = groupsCount / TagsGroup.TagsGroupLength;
            int i = 0;
            for (int j = 0; j <= TagsGroup.TagsGroupLength && i < groupsCount; j++) {
                bool[] tags = new bool[TagsGroup.TagsGroupLength];
                for (int k = 0; k < j; k++) {
                    tags[TagsGroup.TagsGroupLength - 1 - k] = true;
                }
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

        internal static TagsGroup[] CreateDescendantTestGroups(int groupsCount) {
            TagsGroup[] groups = new TagsGroup[groupsCount];
            int bucketsCount = groupsCount / TagsGroup.TagsGroupLength;
            int i = groupsCount - 1;
            for (int j = 0; j <= TagsGroup.TagsGroupLength && i >= 0; j++) {
                bool[] tags = new bool[TagsGroup.TagsGroupLength];
                for (int k = 0; k < j; k++) {
                    tags[TagsGroup.TagsGroupLength - 1 - k] = true;
                }
                for (int k = 0; k < bucketsCount && i >= 0; k++, i--) {
                    groups[i] = new TagsGroup(tags);
                }
            }
            var fullTags = CreateAllTagsTrue();
            while (i >= 0) {
                groups[i--] = new TagsGroup(fullTags);
            }
            return groups;
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
        bool[] InnerTags { get; }

        public static int MeasureSimilarity(TagsGroup a, TagsGroup b) {
            int result = 0;
            for (int i = 0; i < TagsGroupLength; i++) {
                if (a.InnerTags[i] == b.InnerTags[i])
                    result++;
            }
            return result;
        }

        public TagsGroup(bool[] innerTags) {
            if (innerTags == null)
                throw new ArgumentException(nameof(innerTags));
            if (innerTags.Length != TagsGroupLength) {
                throw new ArgumentException(nameof(innerTags));
            }
            InnerTags = innerTags;
        }
    }

    class SimilarTagsCalculator {
        TagsGroup[] Groups { get; }

        public SimilarTagsCalculator(TagsGroup[] groups) {
            if (groups == null)
                throw new ArgumentException(nameof(groups));
            Groups = groups;
        }

        public TagsGroup[] GetFiftyMostSimilarGroups(TagsGroup value) {
            const int resultLength = 50;
            List<int> similarity = new List<int>();
            List<TagsGroup> result = new List<TagsGroup>();
            List<int> indices = new List<int>();
            int i = 0;
            foreach (var tagsGroup in Groups) {
                int similarityWithValue = TagsGroup.MeasureSimilarity(tagsGroup, value);
                int index = similarity.BinarySearch(similarityWithValue);
                if (index < 0) {
                    index = ~index;
                }
                else {
                    while (index >= 0 && similarity[index] == similarityWithValue) {
                        index--;
                    }
                    index++;
                }
                similarity.Insert(index, similarityWithValue);
                result.Insert(index, tagsGroup);
                indices.Insert(index, i);
                if (similarity.Count > resultLength) {
                    similarity.RemoveAt(0);
                    result.RemoveAt(0);
                    indices.RemoveAt(0);
                }
                i++;
            }
            result.Reverse();
            return result.ToArray();
        }
    }
}