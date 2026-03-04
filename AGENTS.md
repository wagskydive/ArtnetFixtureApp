
# 🤖 agents.md — AI Agent Instructions

This file defines the behavior, rules, and coding standards for any AI or automation tools contributing to this project.

## 🎯 Purpose
To ensure that AI tools write consistent, maintainable, and modular code that aligns with the docs in the /docs/

## 📘 Design Reference
- Always read the docs in docs/ folder and follow those instructions
- Create or update `tickets.md` for execution tasks
- Create or Update requirements.txt for all dependencies.

## 🔁 Task Behavior

- If no active ticket exists for the next task, create one.
- If a ticket is too complex, break it down into sub-tickets (e.g. T3.2a, T3.2b).
- Always use the checklist format on tickets:
  - [ ] Started
  - [ ] Behavior Written
  - [ ] Code Written
  - [ ] Tests Passed
  - [ ] Documentation Written
- Always verify in roadmap.md which tasks are completed and mark them accordingly.
- Before creating the PR, always check roadmap.md and create new tickets for the next run.

## ✨ Coding Standards

- use BDD principles and Agile and/or scrum

### ✅ General Principles
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple, Stupid)
- SRP (Single Responsibility Principle)
- Modular design (split files and functions logically)
- Comment meaningful logic, not obvious operations

