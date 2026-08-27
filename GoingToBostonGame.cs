using System;
using System.Collections.Generic;
using System.Linq;

public class GoingToBostonGame
{
    public static int PlayGoingToBoston(Random? random = null, Action<string>? outputWriter = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;
        Random rng = random ?? new Random();

        int totalScore = 0;
        int remainingDice = 3;

        while (remainingDice > 0)
        {
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
