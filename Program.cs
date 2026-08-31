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

        bool running = true;
        while (running)
        {
            Console.WriteLine("       [LB]                         [RB]");
            Console.WriteLine("     .------.                     .------.");
            Console.WriteLine("    /  ____  \\___________________/  ____  \\");
            Console.WriteLine("   /  /    \\                       /    \\  \\");
            Console.WriteLine();
            Console.WriteLine("  |  |  LT  |                     |  RT  |  |");
            Console.WriteLine("  ;   \\____/                       \\____/   :");
            Console.WriteLine(" /                                           \\");
            Console.WriteLine();
            Console.WriteLine("|     ( L )                                   |");
            Console.WriteLine("|    Thumbstick            [===]        (Y)   |");
            Console.WriteLine("|                        Touchpad    (X)   (B) |");
            Console.WriteLine("|         _                             (A)   |");
            Console.WriteLine("|       _| |_                                 |");
            Console.WriteLine("|      |_   _|             ( R )              |");
            Console.WriteLine("|        |_|             Thumbstick           |");
            Console.WriteLine("|                                             |");
            Console.WriteLine(" \\         /                       \\         /");
            Console.WriteLine("  \\_______/                         \\_______/");
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("Please select a game or app (1-7) or 0 to exit:");
            Console.WriteLine("1. The Ladder Game");
            Console.WriteLine("2. Knock Out Game");
            Console.WriteLine("3. Going to Boston Game");
            Console.WriteLine("4. Rock, Paper, Scissors Game");
            Console.WriteLine("5. FizzBuzz Game");
            Console.WriteLine("6. Number Guessing Game");
            Console.WriteLine("7. To-Do App");
            Console.WriteLine("0. Exit");
            Console.Write("Enter your choice: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    TheLadderGame.PlayTheLadder();
                    break;
                case "2":
                    Console.Write("Enter your knockout number (6, 7, 8, or 9): ");
                    if (int.TryParse(Console.ReadLine(), out int koNum))
                    {
                        KnockOutGame.PlayKnockOut(koNum);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter an integer.");
                    }
                    break;
                case "3":
                    GoingToBostonGame.PlayGoingToBoston();
                    break;
                case "4":
                    Console.Write("Enter your choice (rock, paper, scissors): ");
                    string? rpsChoice = Console.ReadLine();
                    RockPaperScissorsGame.PlayRockPaperScissors(rpsChoice ?? string.Empty);
                    break;
                case "5":
                    FizzBuzzGame.PlayFizzBuzz();
                    break;
                case "6":
                    RunGuessingGame();
                    break;
                case "7":
                    RunTodoAppInteractive();
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("Thank you for playing! Goodbye.");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter a number between 0 and 7.");
                    break;
            }

            if (running)
            {
                Console.WriteLine("\nPress Enter to return to the main menu...");
                Console.ReadLine();
            }
        }
    }

    private static void RunGuessingGame()
    {
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

    private static void RunTodoAppInteractive()
    {
        var app = new TodoApp();
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n--- To-Do App Menu ---");
            Console.WriteLine("1. Show To-Do List");
            Console.WriteLine("2. Add Task");
            Console.WriteLine("3. Toggle Task Status");
            Console.WriteLine("4. Remove Task");
            Console.WriteLine("5. Back to Main Menu");
            Console.Write("Choose an option: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    app.ShowTodoList();
                    break;
                case "2":
                    Console.Write("Enter task description: ");
                    string? desc = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        app.AddTask(desc);
                    }
                    else
                    {
                        Console.WriteLine("Description cannot be empty.");
                    }
                    break;
                case "3":
                    Console.Write("Enter task ID to toggle: ");
                    if (int.TryParse(Console.ReadLine(), out int toggleId))
                    {
                        app.ToggleTaskStatus(toggleId);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                    break;
                case "4":
                    Console.Write("Enter task ID to remove: ");
                    if (int.TryParse(Console.ReadLine(), out int removeId))
                    {
                        app.RemoveTask(removeId);
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID.");
                    }
                    break;
                case "5":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }
}
