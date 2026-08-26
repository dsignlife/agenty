using System;

public class GameLogic
{
    public static string EvaluateGuess(int guess, int target)
    {
        if (guess < 1 || guess > 100)
        {
            return "Your guess must be between 1 and 100.";
        }
        else if (guess < target)
        {
            return "Too low! Try again.";
        }
        else if (guess > target)
        {
            return "Too high! Try again.";
        }
        else
        {
            return "Win";
        }
    }
}
