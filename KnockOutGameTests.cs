using System;
using System.Collections.Generic;

public class KnockOutGameTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for KnockOutGame...");
        int passed = 0;
        int failed = 0;

        void AssertContains(string testName, string expectedSubstring, string output)
        {
            if (output.Contains(expectedSubstring))
            {
                Console.WriteLine($"[PASS] {testName}");
                passed++;
            }
            else
            {
                Console.WriteLine($"[FAIL] {testName}: Expected output to contain '{expectedSubstring}', got '{output}'");
                failed++;
            }
        }

        // Test invalid knockout number
        {
            var logs = new List<string>();
            KnockOutGame.PlayKnockOut(5, outputWriter: logs.Add);
            string fullOutput = string.Join("\n", logs);
            AssertContains("Invalid knockOutNumber 5", "Invalid choice. Please pick 6, 7, 8, or 9.", fullOutput);
        }

        // Test invalid knockout number 10
        {
            var logs = new List<string>();
            KnockOutGame.PlayKnockOut(10, outputWriter: logs.Add);
            string fullOutput = string.Join("\n", logs);
            AssertContains("Invalid knockOutNumber 10", "Invalid choice. Please pick 6, 7, 8, or 9.", fullOutput);
        }

        // Test game session where first roll hits knockout number (e.g. 7, using mock random returning 3 and 4)
        {
            var logs = new List<string>();
            var mockRandom = new MockRandom(new[] { 3, 4 });
            KnockOutGame.PlayKnockOut(7, random: mockRandom, outputWriter: logs.Add, inputReader: () => "yes");
            string fullOutput = string.Join("\n", logs);
            AssertContains("Knock out on first roll (sum 7)", "Knocked out! You hit your knockout number: 7", fullOutput);
            AssertContains("Game Over score 0 on immediate knockout", "Game Over! Your final score is: 0", fullOutput);
        }

        // Test game session where roll is safe then knocked out
        {
            var logs = new List<string>();
            // Sequence: 2, 3 (sum 5), then 3, 4 (sum 7)
            var mockRandom = new MockRandom(new[] { 2, 3, 3, 4 });
            KnockOutGame.PlayKnockOut(7, random: mockRandom, outputWriter: logs.Add, inputReader: () => "yes");
            string fullOutput = string.Join("\n", logs);
            AssertContains("Safe roll adds to score", "Safe! Current score: 5", fullOutput);
            AssertContains("Subsequent knockout", "Knocked out! You hit your knockout number: 7", fullOutput);
            AssertContains("Game Over final score 5", "Game Over! Your final score is: 5", fullOutput);
        }

        Console.WriteLine($"\nKnockOutGame Test Results: {passed} passed, {failed} failed.");
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
