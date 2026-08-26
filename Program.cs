using System;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("test", StringComparison.OrdinalIgnoreCase))
        {
            GameLogicTests.RunUnitTests();
            return;
        }

        Random random = new Random();
        int target = random.Next(1, 101);
        int attempts = 0;
        bool win = false;

        Console.WriteLine("Welcome to the Number Guessing Game!");
        Console.WriteLine("I have chosen a number between 1 and 100. Try to guess it!");

        while (!win)
        {
            Console.Write("Enter your guess: ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int guess))
            {
                Console.WriteLine("Please enter a valid integer.");
                continue;
            }

            attempts++;

            string result = GameLogic.EvaluateGuess(guess, target);
            if (result == "Win")
            {
                win = true;
                Console.WriteLine($"Congratulations! You found the number {target} in {attempts} attempts.");
            }
            else
            {
                Console.WriteLine(result);
            }
        }
    }
}
