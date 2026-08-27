using System;
using System.Collections.Generic;

public class FizzBuzzGameTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for FizzBuzzGame...");
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

        // Test specific values in range 1 to 15
        var results = FizzBuzzGame.PlayFizzBuzz(1, 15, outputWriter: _ => { });

        AssertEquals("FizzBuzz at 1", "1", results[0]);
        AssertEquals("Fizz at 3", "Fizz", results[2]);
        AssertEquals("Buzz at 5", "Buzz", results[4]);
        AssertEquals("Fizz at 6", "Fizz", results[5]);
        AssertEquals("Buzz at 10", "Buzz", results[9]);
        AssertEquals("FizzBuzz at 15", "FizzBuzz", results[14]);
        AssertEquals("Total count 1 to 15", "15", results.Count.ToString());

        Console.WriteLine($"\nFizzBuzzGame Test Results: {passed} passed, {failed} failed.");
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
