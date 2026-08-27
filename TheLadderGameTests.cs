using System;
using System.Collections.Generic;

public class TheLadderGameTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for TheLadderGame...");
        int passed = 0;
        int failed = 0;

        void AssertEquals(string testName, int expected, int actual)
        {
            if (expected == actual)
            {
                Console.WriteLine($"[PASS] {testName}");
                passed++;
            }
            else
            {
                Console.WriteLine($"[FAIL] {testName}: Expected '{expected}', got '{actual}'");
                failed++;
            }
        }

        // Test The Ladder completion with mock random that rolls 1, then 2, ..., 6 immediately (6 throws)
        {
            var mockRandom = new MockRandom(new[] { 1, 2, 3, 4, 5, 6 });
            int throws = TheLadderGame.PlayTheLadder(random: mockRandom, outputWriter: _ => { });
            AssertEquals("Completed ladder in 6 throws", 6, throws);
        }

        // Test The Ladder with some missed rolls before hitting targets
        {
            var mockRandom = new MockRandom(new[] { 5, 1, 2, 3, 4, 5, 6 });
            int throws = TheLadderGame.PlayTheLadder(random: mockRandom, outputWriter: _ => { });
            AssertEquals("Completed ladder with misses in 7 throws", 7, throws);
        }

        Console.WriteLine($"\nTheLadderGame Test Results: {passed} passed, {failed} failed.");
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }

    private class MockRandom : Random
    {
        private readonly Queue<int> _values;

        public MockRandom(IEnumerable<int> values)
        {
            _values = new Queue<int>(values);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (_values.Count > 0)
            {
                return _values.Dequeue();
            }
            return minValue;
        }
    }
}
