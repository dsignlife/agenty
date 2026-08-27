using System;
using System.Collections.Generic;

public class TodoAppTests
{
    public static void RunUnitTests()
    {
        Console.WriteLine("Running unit tests for TodoApp...");
        int passed = 0;
        int failed = 0;

        void AssertEquals(string testName, string expected, string actual)
        {
            if (expected == actual)
            {
                Console.WriteLine($"[PASS] {testName}");
                passed++;
            }
            else
            {
                Console.WriteLine($"[FAIL] {testName}: Expected '{expected}', got '{actual}'");
                failed++;
            }
        }

        // Test AddTask and ShowTodoList
        {
            var logs = new List<string>();
            var app = new TodoApp(logs.Add);
            app.AddTask("Buy groceries");
            app.AddTask("Learn C#");

            AssertEquals("Task count is 2", "2", app.TodoList.Count.ToString());
            AssertEquals("First task description", "Buy groceries", app.TodoList[0].Description);
            AssertEquals("First task not completed", "False", app.TodoList[0].IsCompleted.ToString());
        }

        // Test ToggleTaskStatus
        {
            var logs = new List<string>();
            var app = new TodoApp(logs.Add);
            app.AddTask("Test task");
            int id = app.TodoList[0].Id;

            app.ToggleTaskStatus(id);
            AssertEquals("Task is now completed", "True", app.TodoList[0].IsCompleted.ToString());

            app.ToggleTaskStatus(999); // Not found
            AssertEquals("Logs contain task not found", "Task not found.", logs[logs.Count - 1]);
        }

        // Test RemoveTask
        {
            var logs = new List<string>();
            var app = new TodoApp(logs.Add);
            app.AddTask("Task to remove");
            int id = app.TodoList[0].Id;

            app.RemoveTask(id);
            AssertEquals("Task list is empty after removal", "0", app.TodoList.Count.ToString());

            app.RemoveTask(999); // Not found
            AssertEquals("Logs contain task not found on remove", "Task not found.", logs[logs.Count - 1]);
        }

        Console.WriteLine($"\nTodoApp Test Results: {passed} passed, {failed} failed.");
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
