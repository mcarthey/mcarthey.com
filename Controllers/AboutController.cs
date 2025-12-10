using Microsoft.AspNetCore.Mvc;
using mcarthey.com.Models;

namespace mcarthey.com.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            var model = new AboutViewModel
            {
                Timeline = GetTimelineEvents(),
                Skills = GetSkills()
            };

            return View(model);
        }

        private List<TimelineEvent> GetTimelineEvents()
        {
            return new List<TimelineEvent>
            {
                new TimelineEvent
                {
                    Period = "1980s - The Beginning",
                    Title = "First Steps in Computing",
                    Description = "First encounter with Commodore VIC-20. Learned BASIC programming from magazines and books. Created simple games and utilities, discovered the magic of making computers do what I wanted."
                },
                new TimelineEvent
                {
                    Period = "1990s - The Growth",
                    Title = "Professional Development Begins",
                    Description = "Moved to PC programming with Turbo Pascal and C++. Developed shareware applications and games. Started professional development career with Windows applications and early web technologies."
                },
                new TimelineEvent
                {
                    Period = "2000s - The Professional",
                    Title = "Enterprise Development",
                    Description = "Enterprise development with .NET Framework. Built large-scale business applications, web services, and database-driven systems. Embraced object-oriented programming and design patterns."
                },
                new TimelineEvent
                {
                    Period = "2010s - The Evolution",
                    Title = "Modern Frameworks & Mentorship",
                    Description = "Transitioned to .NET Core and modern web frameworks. Developed cloud-native applications, RESTful APIs, and microservices. Mentored junior developers and led technical teams."
                },
                new TimelineEvent
                {
                    Period = "2020s - The Present",
                    Title = "Teaching & Leading",
                    Description = "Full-stack development with .NET 8 and C# MVC. Focus on clean architecture, test-driven development, and DevOps practices. Serving as C# Developer, Instructor, and Adjunct Professor. Still passionate about efficient, elegant code after 40+ years in computing."
                }
            };
        }

        private List<Skill> GetSkills()
        {
            return new List<Skill>
            {
                // Primary Technologies
                new Skill { Name = "C# / .NET", Percentage = 95, Category = "Primary" },
                new Skill { Name = "ASP.NET MVC", Percentage = 90, Category = "Primary" },
                new Skill { Name = "SQL Server", Percentage = 85, Category = "Primary" },
                new Skill { Name = "JavaScript", Percentage = 80, Category = "Primary" },
                // Secondary Skills
                new Skill { Name = "HTML/CSS", Percentage = 85, Category = "Secondary" },
                new Skill { Name = "Azure/AWS", Percentage = 75, Category = "Secondary" },
                new Skill { Name = "Git/DevOps", Percentage = 85, Category = "Secondary" },
                new Skill { Name = "RESTful APIs", Percentage = 90, Category = "Secondary" }
            };
        }
    }
}
