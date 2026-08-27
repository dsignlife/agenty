using System;
using System.Collections.Generic;

public class RockPaperScissorsGameTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for RockPaperScissorsGame...");
        int passed = 0;
        int failed = 0;

        void AssertEquals(string testName, string expected, string actual)
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

        // Test invalid choice
        {
            var logs = new List<string>();
            string result = RockPaperScissorsGame.PlayRockPaperScissors("lizard", outputWriter: logs.Add);
            string fullOutput = string.Join("\n", logs);
            AssertEquals("Invalid choice returns Invalid", "Invalid", result);
            AssertContains("Invalid choice prints error", "Invalid input! Please choose rock, paper, or scissors.", fullOutput);
        }

        // Test player wins (rock vs scissors) using mock random that returns index 2 ("scissors")
        {
            var logs = new List<string>();
            var mockRandom = new MockRandom(new[] { 2 });
            string result = RockPaperScissorsGame.PlayRockPaperScissors("rock", random: mockRandom, outputWriter: logs.Add);
            string fullOutput = string.Join("\n", logs);
            AssertEquals("Rock beats scissors -> Player wins", "Player wins", result);
            AssertContains("Player wins print", "Result: Player wins!", fullOutput);
        }

        // Test draw (paper vs paper) using mock random that returns index 1 ("paper")
        {
            var logs = new List<string>();
            var mockRandom = new MockRandom(new[] { 1 });
            string result = RockPaperScissorsGame.PlayRockPaperScissors("paper", random: mockRandom, outputWriter: logs.Add);
            string fullOutput = string.Join("\n", logs);
            AssertEquals("Paper vs paper -> Draw", "Draw", result);
            AssertContains("Draw print", "Result: It's a draw!", fullOutput);
        }

        Console.WriteLine($"\nRockPaperScissorsGame Test Results: {passed} passed, {failed} failed.");
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

        public override int Next(int maxValue)
        {
            if (_values.Count > 0)
            {
                return _values.Dequeue();
            }
            return 0;
        }
    }
}
