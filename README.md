# 📊 Telegram Task Bot (Legacy)

A Telegram bot that converts simple task messages into Excel spreadsheets automatically.

---

## 🚧 Project Evolution 

This repository contains **two major versions** of the Telegram Task Bot:

- **`main` branch (this branch)** → **Legacy version**
  - Simple English message format
  - Flat task parsing
  - Basic Excel generation

- **`v2/architecture-redesign` branch** → **Enhanced & recommended version 🌙**
  - Structured Arabic Ramadan tracking cards
  - Domain-driven design (Coordinator, Participant, DailyReport)
  - One row per participant per day
  - One column per task
  - Duplicate detection (day + participant)
  - Improved validation and reporting

👉 **For the latest implementation, please switch to the `v2/architecture-redesign` branch.**

---

## ✨ Features (Legacy)

-  Parse simple task messages
-  Generate Excel spreadsheets
-  Multiple people per report
-  Automatic total calculation
-  Token management via `.env`

---

## 📖 Message Format

person name: Ali

frontend: 10

backend: 5


person name: Amine

frontend: 8

backend: 7


---

## 📊 Output

- One row per person
- Tasks as columns
- Automatic totals
- Basic Excel formatting

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Telegram bot token from [@BotFather](https://t.me/BotFather)

### Run

```bash
git clone https://github.com/RoaaAlsham/telegram-task-bot.git
cd telegram-task-bot
cp .env.example .env
dotnet restore
dotnet run

🧱 Project Structure
TelegramTaskBot/
├── Program.cs
├── TaskBot.cs
├── TaskData.cs
├── MessageParser.cs
├── ExcelGenerator.cs
└── README.md

🛠 Tech Stack

Telegram.Bot

EPPlus

DotNetEnv

.NET 8


📝 Note

This branch is kept for historical and learning purposes.
