 Overview

The Task Tracker is a simple console-based project management tool designed to help small teams or individuals organize their work.
It allows users to create, search, update, and sort tasks easily from the command line.
The main goal of this project is to demonstrate Object-Oriented Programming, data persistence using JSON, and logging with file I/O while maintaining clean code and simple user interaction.

 How It Works

When the program starts, it loads all existing tasks from a JSON file (tasks.json).

The main menu appears with options to add, update, search, list, and export tasks.

Every time you add or modify a task, it is automatically saved to the JSON file.

All major actions (task creation, status updates, exports) are recorded in a log.txt file.

Tasks can be searched by ID or by title, sorted by priority or due date, and overdue tasks can be exported to CSV.

 Key Features

Create Task: Add a new task with title, description, due date, priority, and assignee.

Search Tasks: Find tasks quickly by typing part of the name or a full task ID.

Update Status: Mark tasks as To-Do, In-Progress, Done, or Archived.

Sorting: Sort tasks by due date (custom insertion sort algorithm) or priority.

Overdue Report: View overdue tasks and export them to a CSV file for record keeping.

Persistent Storage: Automatically saves all tasks in `tasks.json
