# C# Games & Apps CLI Suite

A collection of classic dice/board games, mini-games, puzzles, and a To-Do application built in C# (.NET 8.0), featuring an interactive command-line interface (CLI) menu and comprehensive unit tests.

## Features

The interactive CLI provides access to the following games and applications:

1. **The Ladder Game** - Roll dice to climb up a ladder with interactive confirmation prompts.
2. **Knock Out Game** - Choose a knockout number (6, 7, 8, or 9) and score points while avoiding the knockout roll.
3. **Going to Boston Game** - A classic three-dice highest-score accumulation game.
4. **Rock, Paper, Scissors Game** - Play against the computer with automated outcome evaluation.
5. **FizzBuzz Game** - Classic counting game demonstration with Fizz, Buzz, and FizzBuzz logic.
6. **Number Guessing Game** - Guess the secret number between 1 and 100 with feedback on each attempt.
7. **To-Do App** - Interactive task management application to add, complete, view, and remove tasks.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher.

### Building the Project

```bash
dotnet build
```

### Running the Application

Launch the interactive CLI menu:

```bash
dotnet run
```

### Running Tests

Execute the comprehensive unit test suite across all games and application logic:

```bash
dotnet run -- test
```
