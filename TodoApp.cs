using System;
using System.Collections.Generic;

public class TodoItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class TodoApp
{
    private readonly List<TodoItem> _todoList = new List<TodoItem>();
    private int _nextTaskId = 1;
    private readonly Action<string> _print;

    public TodoApp(Action<string>? outputWriter = null)
    {
        _print = outputWriter ?? Console.WriteLine;
    }

    public IReadOnlyList<TodoItem> TodoList => _todoList;

    public void AddTask(string taskDescription)
    {
        var newTask = new TodoItem
        {
            Id = _nextTaskId,
            Description = taskDescription,
            IsCompleted = false
        };

        _todoList.Add(newTask);
        _nextTaskId++;
        _print($"Added task: {taskDescription}");
    }

    public void ToggleTaskStatus(int targetTaskId)
    {
        var task = _todoList.Find(t => t.Id == targetTaskId);

        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            _print($"Status updated for task ID: {targetTaskId}");
        }
        else
        {
            _print("Task not found.");
        }
    }

    public void RemoveTask(int targetTaskId)
    {
        int index = _todoList.FindIndex(t => t.Id == targetTaskId);

        if (index != -1)
        {
            _todoList.RemoveAt(index);
            _print($"Removed task ID: {targetTaskId}");
        }
        else
        {
            _print("Task not found.");
        }
    }

    public void ShowTodoList()
    {
        _print("--- My Todo List ---");
        if (_todoList.Count == 0)
        {
            _print("Your todo list is empty!");
        }
        else
        {
            foreach (var task in _todoList)
            {
                string checkmark = task.IsCompleted ? "[X]" : "[ ]";
                _print($"{task.Id}. {checkmark} {task.Description}");
            }
        }
    }
}
