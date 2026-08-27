using System;

public class KnockOutGame
{
    public static void PlayKnockOut(int knockOutNumber, Random? random = null, Action<string>? outputWriter = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;

        if (knockOutNumber != 6 && knockOutNumber != 7 && knockOutNumber != 8 && knockOutNumber != 9)
        {
            print("Invalid choice. Please pick 6, 7, 8, or 9.");
            return;
        }

        Random rng = random ?? new Random();
        int totalScore = 0;
        bool isKnockedOut = false;

        while (!isKnockedOut)
        {
            int dieOne = rng.Next(1, 7);
            int dieTwo = rng.Next(1, 7);
            int currentSum = dieOne + dieTwo;

            print($"Rolled {dieOne} and {dieTwo}. Sum is {currentSum}");

            if (currentSum == knockOutNumber)
            {
                print($"Knocked out! You hit your knockout number: {knockOutNumber}");
                isKnockedOut = true;
            }
            else
            {
                totalScore += currentSum;
                print($"Safe! Current score: {totalScore}");
            }
        }

        print($"Game Over! Your final score is: {totalScore}");
    }
}
