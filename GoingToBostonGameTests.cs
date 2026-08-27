using System;
using System.Collections.Generic;

public class GoingToBostonGameTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for GoingToBostonGame...");
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

        // Test Going to Boston scoring with mock random
        // Round 1 (3 dice): rolls 2, 5, 3 -> highest is 5 (running total 5)
        // Round 2 (2 dice): rolls 1, 6 -> highest is 6 (running total 11)
        // Round 3 (1 die): roll 4 -> highest is 4 (final score 15)
        {
            var mockRandom = new MockRandom(new[] { 2, 5, 3, 1, 6, 4 });
            int finalScore = GoingToBostonGame.PlayGoingToBoston(random: mockRandom, outputWriter: _ => { }, inputReader: () => "yes");
            AssertEquals("Going to Boston final score with mock rolls", 15, finalScore);
        }

        Console.WriteLine($"\nGoingToBostonGame Test Results: {passed} passed, {failed} failed.");
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
