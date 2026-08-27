using System;

public class TheLadderGame
{
    public static int PlayTheLadder(Random? random = null, Action<string>? outputWriter = null, Func<string?>? inputReader = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;
        Random rng = random ?? new Random();
        Func<string?> readInput = inputReader ?? Console.ReadLine;

        int totalThrows = 0;
        int currentTarget = 1;

        print("Starting the game: The Ladder! Goal is to roll 1 to 6 in order.");

        while (currentTarget <= 6)
        {
            print($"Confirm throw for target {currentTarget} by typing 'yes':");
            string? input = readInput();

            if (input == null || !input.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                print("Throw not confirmed. Please type 'yes'.");
                continue;
            }

            int dieRoll = rng.Next(1, 7);
            totalThrows++;

            print($"Throw #{totalThrows}: Rolled {dieRoll}. Looking for {currentTarget}.");

            if (dieRoll == currentTarget)
            {
                print($"Success! You found {currentTarget}.");
                currentTarget++;
            }
        }

        print($"Game Over! You completed the ladder in {totalThrows} throws.");
        return totalThrows;
    }
}
