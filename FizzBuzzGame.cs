using System;
using System.Collections.Generic;

public class FizzBuzzGame
{
    public static List<string> PlayFizzBuzz(int start = 1, int end = 100, Action<string>? outputWriter = null)
    {
        Action<string> print = outputWriter ?? Console.WriteLine;
        var results = new List<string>();

        for (int currentNumber = start; currentNumber <= end; currentNumber++)
        {
            string output;
            if (currentNumber % 3 == 0 && currentNumber % 5 == 0)
            {
                output = "FizzBuzz";
            }
            else if (currentNumber % 3 == 0)
            {
                output = "Fizz";
            }
            else if (currentNumber % 5 == 0)
            {
                output = "Buzz";
            }
            else
            {
                output = currentNumber.ToString();
            }

            print(output);
            results.Add(output);
        }

        return results;
    }
}
