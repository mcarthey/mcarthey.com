using Microsoft.AspNetCore.Mvc;
using mcarthey.com.Models;

namespace mcarthey.com.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Index()
        {
            var projects = GetProjects();
            return View(projects);
        }

        private List<Project> GetProjects()
        {
            return new List<Project>
            {
                new Project
                {
                    Id = "netdbprogramming",
                    Name = "NETDBProgramming",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│E││D││U││C││A│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "C# database programming educational material and tutorials for students learning Entity Framework and SQL Server integration.",
                    Technologies = new List<string> { "C#", "Entity Framework", "SQL Server", "ASP.NET" },
                    Status = "ACTIVE",
                    StatusColor = "green-400",
                    Year = 2024,
                    Category = "web",
                    GitHubUrl = "https://github.com/mcarthey/NETDBProgramming"
                },
                new Project
                {
                    Id = "consoleblog",
                    Name = "Blogs-Console",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│B││L││O││G││S│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Console-based blog application demonstrating C# fundamentals, data structures, and CRUD operations in a terminal environment.",
                    Technologies = new List<string> { "C#", ".NET Core", "Console Apps" },
                    Status = "COMPLETED",
                    StatusColor = "green-400",
                    Year = 2024,
                    Category = "desktop",
                    GitHubUrl = "https://github.com/mcarthey/Blogs-Console"
                },
                new Project
                {
                    Id = "consolebank",
                    Name = "ConsoleBank",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│B││A││N││K││$│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Banking simulation console application for teaching object-oriented programming concepts, including accounts, transactions, and user management.",
                    Technologies = new List<string> { "C#", ".NET", "OOP", "LINQ" },
                    Status = "COMPLETED",
                    StatusColor = "green-400",
                    Year = 2023,
                    Category = "desktop",
                    GitHubUrl = "https://github.com/mcarthey/ConsoleBank"
                },
                new Project
                {
                    Id = "eftutorial",
                    Name = "WCTC EF Tutorial",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│E││F││ ││D││B│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Entity Framework tutorial project for Wisconsin College of Technology students, covering Code First, Database First, and migrations.",
                    Technologies = new List<string> { "Entity Framework", "C#", "SQL Server", "ASP.NET MVC" },
                    Status = "ACTIVE",
                    StatusColor = "green-400",
                    Year = 2024,
                    Category = "web",
                    GitHubUrl = "https://github.com/mcarthey/WCTC_EFTutorial_Blogs"
                },
                new Project
                {
                    Id = "hsscheduler",
                    Name = "HSScheduler",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│S││C││H││E││D│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "High school scheduling application built with Objective-C for iOS, managing student schedules, classes, and timetables.",
                    Technologies = new List<string> { "Objective-C", "iOS", "Core Data", "UIKit" },
                    Status = "COMPLETED",
                    StatusColor = "green-400",
                    Year = 2022,
                    Category = "mobile"
                },
                new Project
                {
                    Id = "zork",
                    Name = "Zork I",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│Z││O││R││K││!│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Fork of the classic Infocom text adventure game, exploring the Great Underground Empire. A tribute to retro gaming and interactive fiction.",
                    Technologies = new List<string> { "ZIL", "Text Adventure", "Interactive Fiction" },
                    Status = "PRESERVED",
                    StatusColor = "amber-400",
                    Year = 2023,
                    Category = "desktop",
                    GitHubUrl = "https://github.com/mcarthey/zork1"
                }
            };
        }
    }
}
