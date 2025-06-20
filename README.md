# Registration Manager (ASP.NET Razor Pages)

This is a student course registration system built using ASP.NET Core Razor Pages. It simulates a basic registration platform where users can view and edit student course records.

## 🔧 Technologies Used

- ASP.NET Core Razor Pages (.NET)
- C#
- JavaScript (for interactivity)
- HTML/CSS
- Session state (no database used)

## ✅ Features

### 1. Dynamic Student Selection
- Automatically loads the selected student's registered courses upon selection from the dropdown.
- No need to click a button to submit; selection triggers a form submission via JavaScript.

### 2. Grade Entry
- Displays registered courses for each student.
- Allows grade input and editing for each course.
- Empty grade fields remain blank and editable.
- A "Submit Grades" button stores grades in memory (via Session).

### 3. Sorting Functionality
- Course table supports sorting by:
  - Course Code
  - Course Title
  - Grade
- Sort order is preserved when switching between students (using Session).
- Table headers are clickable and toggle ascending/descending order.

## 💡 Notable Design Points

- Razor Page model used for modularity and clear separation of logic.
- Lightweight and responsive UI with basic Bootstrap styles.
- No backend database – uses in-memory lists for student/course data.
- JavaScript enhances UX by making the interface feel more dynamic.

