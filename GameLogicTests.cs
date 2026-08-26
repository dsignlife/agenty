using System;

public class GameLogicTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for GameLogic...");
        int passed = 0;
        int failed = 0;

        void AssertTest(string testName, string expected, string actual)
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

        // Out of range tests
        AssertTest("Guess 0 (Low Out of Range)", "Your guess must be between 1 and 100.", GameLogic.EvaluateGuess(0, 50));
        AssertTest("Guess -5 (Low Out of Range)", "Your guess must be between 1 and 100.", GameLogic.EvaluateGuess(-5, 50));
        AssertTest("Guess 101 (High Out of Range)", "Your guess must be between 1 and 100.", GameLogic.EvaluateGuess(101, 50));
        AssertTest("Guess 150 (High Out of Range)", "Your guess must be between 1 and 100.", GameLogic.EvaluateGuess(150, 50));

        // Too low tests
        AssertTest("Guess 10 when target 50", "Too low! Try again.", GameLogic.EvaluateGuess(10, 50));
        AssertTest("Guess 49 when target 50", "Too low! Try again.", GameLogic.EvaluateGuess(49, 50));
        AssertTest("Guess 1 when target 50 (Boundary Low)", "Too low! Try again.", GameLogic.EvaluateGuess(1, 50));

        // Too high tests
        AssertTest("Guess 51 when target 50", "Too high! Try again.", GameLogic.EvaluateGuess(51, 50));
        AssertTest("Guess 90 when target 50", "Too high! Try again.", GameLogic.EvaluateGuess(90, 50));
        AssertTest("Guess 100 when target 50 (Boundary High)", "Too high! Try again.", GameLogic.EvaluateGuess(100, 50));

        // Win tests
        AssertTest("Guess 50 when target 50", "Win", GameLogic.EvaluateGuess(50, 50));
        AssertTest("Guess 1 when target 1", "Win", GameLogic.EvaluateGuess(1, 1));
        AssertTest("Guess 100 when target 100", "Win", GameLogic.EvaluateGuess(100, 100));

        Console.WriteLine($"\nTest Results: {passed} passed, {failed} failed.");
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
