# Task Tracking Bot 

A Telegram bot that receives daily tasks reports as Arabic text messages and converts them into structured Excel files (.xlsx).

## Features

- **Arabic Message Parsing** — Parses structured Arabic tracking cards using regex
- **Excel Generation** — Produces `.xlsx` files with one row per participant per day, and one column per task
- **Duplicate Detection** — Re-sending a report for the same participant + same day replaces the old entry (with feedback)
- **Coordinator Tracking** — Each participant belongs to a coordinator; both name and ID are tracked
- **Validation** — Task values are clamped to 0–10; missing values default to 0; malformed messages are rejected with guidance

## Message Format

The bot expects messages in this exact Arabic format:

```
بطاقة المتابعة الرمضانية 🌙
اليوم: 1
اسم المشارك : نجم الشمال
رقم المشارك:152
اسم المشرف: منال العلا
رقم المشرف: 500

المهام:

الاستماع لمقطع التدبر: 10
تلاوة جزء من القرآن: 10
السنن الرواتب: 10
صلاة الضحى: 0
أذكار الصباح والمساء: 5
التراويح: 10
عبادة متعدية للغير: 9
```

| Field | Arabic Key | Required |
|-------|-----------|----------|
| Day | اليوم | Yes |
| Participant Name | اسم المشارك | Yes |
| Participant ID | رقم المشارك | Yes |
| Coordinator Name | اسم المشرف | No (defaults to empty) |
| Coordinator ID | رقم المشرف | Yes |
| Tasks | After المهام | Yes |

## Excel Output

Each row represents one participant on one day:

| اليوم | رقم المشرف | اسم المشرف | رقم المشارك | اسم المشارك | الاستماع لمقطع التدبر | تلاوة جزء من القرآن | ... | المجموع |
|-------|-----------|-----------|------------|-----------|---------------------|-------------------|-----|---------|
| 1     | 500       | منال العلا | 152        | نجم الشمال | 10                  | 10                | ... | 64      |

- Rows are sorted by day, then coordinator, then participant
- Right-to-left layout for Arabic readability
- Each task gets its own column; missing tasks default to 0

## Bot Commands

| Command | Description |
|---------|-------------|
| `/start` | Welcome message |
| `/help` | List available commands |
| `/preview` | Preview stored reports (day, name, total) |
| `/stats` | Show coordinator statistics |
| `/generate` | Generate and send the Excel file |
| `/clear` | Clear all stored reports |

## Project Structure

```
TelegramTaskBot/
├── Models/
│   ├── DailyTask.cs       # Single task: name + validated score (0–10)
│   ├── Participant.cs       # Person: ID, name, coordinator link
│   ├── Coordinator.cs       # Supervisor: ID, name, participants list
│   └── DailyReport.cs       # One parsed message = one Excel row
├── MessageParser.cs         # Regex-based Arabic message parser
├── ExcelGenerator.cs        # Excel file generation
├── TaskBot.cs               # Telegram bot handler and orchestration
├── Program.cs               # Entry point
└── TelegramTaskBot.csproj   # .NET 8 project file
```

## Domain Model

```
Coordinator (1) ──── manages ────> (*) Participant
     │                                    │
     └── CoordinatorId                    ├── ParticipantId
     └── CoordinatorName                  ├── ParticipantName
     └── Participants[]                   └── CoordinatorId

DailyReport (one per message)
     ├── Day
     ├── Participant
     ├── Coordinator
     └── Tasks[] ──> DailyTask { Name, Value }
```

## Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Telegram Bot Token from [@BotFather](https://t.me/BotFather)

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd TelegramTaskBot
   ```

2. Create a `.env` file in the project root:
   ```
   TELEGRAM_BOT_TOKEN=your_bot_token_here
   ```

3. Restore dependencies and run:
   ```bash
   dotnet restore
   dotnet run
   ```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) | 22.8.1 | Telegram Bot API client |
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus) | 8.4.2 | Excel file generation |
| [DotNetEnv](https://github.com/tonerdo/dotnet-env) | 3.1.1 | `.env` file loading |

## Task Validation Rules

- Task values must be between **0** and **10** (values outside this range are clamped)
- If a task line has no value (e.g., `صلاة الضحى:`), it defaults to **0**
- Emojis and decorative characters are stripped before parsing
- Extra spaces and line breaks are handled gracefully
- Messages missing required fields (day, participant name/ID, coordinator ID) are rejected
