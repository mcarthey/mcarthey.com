# mcarthey.com - Gen-X Developer Terminal

A retro-themed personal portfolio website built with ASP.NET Core 8 MVC, celebrating the golden age of computing with a modern twist.

## Overview

This website combines nostalgia for classic computing (Commodore VIC-20, Atari 2600) with modern web development practices. It features a terminal-style interface with CRT effects, scanlines, and phosphor green text on black background.

## Technology Stack

- **Framework**: ASP.NET Core 8 MVC
- **Language**: C#
- **Frontend**:
  - HTML5/CSS3
  - Vanilla JavaScript (ES6+)
  - Tailwind CSS (CDN)
  - Anime.js for animations
  - Typed.js for terminal effects
- **Deployment**: Optimized for SmarterASP.NET hosting

## Features

### Retro Terminal Experience
- **Optimized Boot Sequence**: Fast-loading terminal animation (3x faster than original)
- **Skip Button**: Skip intro with localStorage preference saving
- **CRT Effects**: Authentic scanlines, phosphor glow, and screen curvature
- **Sound Effects**: Retro beeps for keyboard and navigation clicks
- **Easter Eggs**: Konami code, Pacman, and Tetris references

### Content Pages
- **Home**: Interactive terminal interface with command-line navigation
- **About**: Developer biography, 40+ year career timeline, technical skills
- **Projects**: Portfolio showcasing C# projects from GitHub
- **Hobbies**: Personal interests beyond coding

### Technical Improvements
- **MVC Architecture**: Clean separation of concerns
- **Fast Load Times**: Optimized animations and assets
- **Responsive Design**: Works on desktop and mobile
- **User Preferences**: Remembers skip intro choice
- **Keyboard Navigation**: Full terminal-style command support

## Project Structure

```
mcarthey.com/
├── Controllers/          # MVC Controllers
│   ├── HomeController.cs
│   ├── AboutController.cs
│   ├── ProjectsController.cs
│   └── HobbiesController.cs
├── Models/              # Data models
│   ├── Project.cs
│   ├── Hobby.cs
│   ├── Skill.cs
│   └── TimelineEvent.cs
├── Views/               # Razor views
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ViewStart.cshtml
│   │   └── _ViewImports.cshtml
│   ├── Home/
│   ├── About/
│   ├── Projects/
│   └── Hobbies/
├── wwwroot/             # Static files
│   ├── css/
│   │   └── retro-terminal.css
│   ├── js/
│   │   └── terminal.js
│   └── images/
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── mcarthey.com.csproj  # Project file
```

## Running Locally

### Prerequisites
- .NET 8 SDK or later
- Any modern web browser

### Steps
1. Clone the repository
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
4. Open browser to `https://localhost:5001` or `http://localhost:5000`

## Terminal Commands

The home page features an interactive terminal that responds to these commands:

- `ABOUT` - Navigate to About page
- `PROJECTS` - Navigate to Projects page
- `HOBBIES` - Navigate to Hobbies page
- `CONTACT` - Display contact information
- `HELP` - Show available commands
- `CLEAR` - Clear terminal output
- `DIR` - List directory contents
- `VER` - Show system version
- `TIME` - Display current time
- `DATE` - Display current date
- `KONAMI` - Trigger Konami code easter egg
- `PACMAN` - Pac-Man easter egg
- `TETRIS` - Tetris easter egg

## Performance Optimizations

### Boot Sequence
- **Original**: 50ms per character + 200ms per line = ~8-10 seconds
- **Optimized**: 15ms per character + 100ms per line = ~2-3 seconds
- **Skip Option**: Instant load with localStorage preference

### Visual Effects
- Lightweight CSS animations
- No heavy JavaScript frameworks
- Optimized image assets
- CDN-hosted libraries

## Design Philosophy

- **Retro Aesthetic**: Authentic 8-bit computing feel
- **Modern Performance**: Fast, responsive, accessible
- **User-Friendly**: Skip intro, clear navigation
- **Easter Eggs**: Hidden features for exploration
- **Nostalgia**: Celebrates computing history

## Developer

**Mark McArthey**
- C# Developer & Instructor
- 40+ years computing experience
- Master's in Software Engineering
- Adjunct Professor

### Connect
- GitHub: [github.com/mcarthey](https://github.com/mcarthey)
- LinkedIn: [linkedin.com/in/markmcarthey](https://linkedin.com/in/markmcarthey)

## License

© 2025 Mark McArthey. All rights reserved.

## Acknowledgments

- Commodore VIC-20 for inspiring a lifetime of computing
- The Gen-X community of early computing enthusiasts
- Tailwind CSS, Anime.js, and Typed.js for excellent tools
