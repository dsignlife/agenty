using System;
using System.Collections.Generic;
using System.Linq;

public class RockPaperScissorsGame
{
    private static readonly string[] ValidChoices = { "rock", "paper", "scissors" };

    public static string PlayRockPaperScissors(string playerChoice, Random? random = null, Action<string>? outputWriter = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;
        string normalizedChoice = playerChoice?.Trim().ToLower() ?? string.Empty;

        if (!ValidChoices.Contains(normalizedChoice))
        {
            print("Invalid input! Please choose rock, paper, or scissors.");
            return "Invalid";
        }

        Random rng = random ?? new Random();
        string computerChoice = ValidChoices[rng.Next(ValidChoices.Length)];

        print($"Player chose: {normalizedChoice}");
        print($"Computer chose: {computerChoice}");

        if (normalizedChoice == computerChoice)
        {
            print("Result: It's a draw!");
            return "Draw";
        }
        else if ((normalizedChoice == "rock" && computerChoice == "scissors") ||
                 (normalizedChoice == "paper" && computerChoice == "rock") ||
                 (normalizedChoice == "scissors" && computerChoice == "paper"))
        {
            print("Result: Player wins!");
            return "Player wins";
        }
        else
        {
            print("Result: Computer wins!");
            return "Computer wins";
        }
    }
}
