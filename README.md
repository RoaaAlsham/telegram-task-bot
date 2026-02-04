# 📊 Telegram Task Bot

A Telegram bot that converts task messages into Excel spreadsheets automatically.


## ✨ Features

- 📝 Parse task messages in natural format
- 📊 Generate professional Excel spreadsheets
- 👥 Support multiple people and tasks
- 🔢 Automatic totals calculation
- 💎 Beautiful formatting and styling
- 🔒 Secure token management with .env

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- A Telegram account
- Visual Studio 2022 or VS Code (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/RoaaAlsham/telegram-task-bot.git
   cd telegram-task-bot
   ```

2. **Create a Telegram Bot**
   - Open Telegram and search for [@BotFather](https://t.me/BotFather)
   - Send `/newbot` and follow the instructions
   - Copy your bot token

3. **Configure environment**
   ```bash
   # Copy the example env file
   cp .env.example .env
   
   # Edit .env and add your bot token
   # TELEGRAM_BOT_TOKEN=your_token_here
   ```

4. **Install dependencies**
   ```bash
   dotnet restore
   ```

5. **Run the bot**
   ```bash
   dotnet run
   ```

## 📖 Usage

1. **Start a conversation** with your bot on Telegram
2. **Send** `/start` to see instructions
3. **Send task messages** in this format:
   ```
   person name: Ahmet 
   coding: 8
   testing: 2
   documentation: 3

   person name: Ayşe 
   coding: 5
   testing: 4
   documentation: 6
   ```
4. **Use** `/generate` to create your Excel file
5. **Download** the generated spreadsheet!

### Available Commands

| Command | Description |
|---------|-------------|
| `/start` | Welcome message and instructions |
| `/help` | Show all available commands |
| `/preview` | Preview stored messages |
| `/generate` | Create Excel file |
| `/clear` | Clear all stored messages |

## 📸 Example

**Input Messages:**
```
person name: Ali
frontend: 10
backend: 5

person name: Amine
frontend: 8
backend: 7
```

**Output:** Excel spreadsheet with:
- Persons as rows
- Tasks as columns
- Automatic total calculations
- Formatting

## 🛠️ Technology Stack

- **[Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)** - Telegram Bot API wrapper
- **[EPPlus](https://github.com/EPPlusSoftware/EPPlus)** - Excel file generation
- **[DotNetEnv](https://github.com/tonerdo/dotnet-env)** - Environment variable management
- **.NET 8.0** - Runtime framework

## 📁 Project Structure

```
TelegramTaskBot/
├── Program.cs              # Entry point
├── TaskBot.cs             # Bot logic and handlers
├── TaskData.cs            # Data model
├── MessageParser.cs       # Message parsing
├── ExcelGenerator.cs      # Excel generation
├── .env.example           # Environment template
├── .gitignore            # Git ignore rules
└── README.md             # This file
```

## 🔒 Security

- Never commit your `.env` file
- Keep your bot token secret
- The `.gitignore` file protects sensitive data
- Use environment variables for production

## 🚢 Deployment

### Docker (Recommended)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV TELEGRAM_BOT_TOKEN=""
ENTRYPOINT ["dotnet", "TelegramTaskBot.dll"]
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

