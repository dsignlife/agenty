using System;
using System.Collections.Generic;
using System.Linq;

public class GoingToBostonGame
{
    public static int PlayGoingToBoston(Random? random = null, Action<string>? outputWriter = null, Func<string?>? inputReader = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;
        Random rng = random ?? new Random();
        Func<string?> readInput = inputReader ?? Console.ReadLine;

        int totalScore = 0;
        int remainingDice = 3;

        while (remainingDice > 0)
        {
            print($"Confirm roll for {remainingDice} remaining dice by typing 'yes':");
            string? input = readInput();

            if (input == null || !input.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                print("Roll not confirmed. Please type 'yes'.");
                continue;
            }

            var currentRolls = new List<int>();

            for (int count = 1; count <= remainingDice; count++)
            {
                int dieRoll = rng.Next(1, 7);
                currentRolls.Add(dieRoll);
            }

            int highestDie = currentRolls.Max();
            totalScore += highestDie;

            print($"Rolled: [{string.Join(", ", currentRolls)}]. Kept highest: {highestDie}");
            print($"Running Total: {totalScore}");

            remainingDice--;
        }

        print($"Final score for Going to Boston: {totalScore}");
        return totalScore;
    }
}
