using Microsoft.AspNetCore.Mvc;
using mcarthey.com.Models;

namespace mcarthey.com.Controllers
{
    public class HobbiesController : Controller
    {
        public IActionResult Index()
        {
            var hobbies = GetHobbies();
            return View(hobbies);
        }

        private List<Hobby> GetHobbies()
        {
            return new List<Hobby>
            {
                new Hobby
                {
                    Id = "jeep",
                    Name = "Jeep Restoration",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│J││E││E││P││!│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "2004 Wrangler LJ Unlimited restoration project. Building the ultimate off-road capable daily driver with modern upgrades while maintaining classic Jeep character.",
                    ProgressPercentage = 65,
                    ImageUrl = "/images/jeep-restoration.png",
                    Highlights = new List<string>
                    {
                        "Suspension lift and 35-inch tires",
                        "Engine performance upgrades",
                        "Interior restoration and modernization",
                        "Custom lighting and electronics"
                    }
                },
                new Hobby
                {
                    Id = "dnd",
                    Name = "Dungeons & Dragons",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│D││&││D││2││0│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Dungeon Master and player since the 1980s. Creating immersive campaigns and building detailed worlds for epic adventures.",
                    ProgressPercentage = 90,
                    Highlights = new List<string>
                    {
                        "Weekly game sessions",
                        "Custom campaign development",
                        "Miniature painting and terrain building",
                        "Digital tools integration (Roll20, D&D Beyond)"
                    }
                },
                new Hobby
                {
                    Id = "hamradio",
                    Name = "Ham Radio",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│H││A││M││7││3│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Amateur radio enthusiast exploring RF communication, digital modes, and emergency preparedness. Connecting with operators worldwide.",
                    ProgressPercentage = 45,
                    Highlights = new List<string>
                    {
                        "Licensed amateur radio operator",
                        "HF and VHF/UHF operations",
                        "Digital modes (FT8, RTTY)",
                        "Emergency communications training"
                    }
                },
                new Hobby
                {
                    Id = "fitness",
                    Name = "Fitness & Health",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│F││I││T││!││!│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Strength training and overall health optimization. Staying strong and healthy for the long-term coding marathon.",
                    ProgressPercentage = 80,
                    Highlights = new List<string>
                    {
                        "Regular weight training routine",
                        "Nutrition and meal planning",
                        "Cardio and flexibility work",
                        "Recovery and injury prevention"
                    }
                },
                new Hobby
                {
                    Id = "woodworking",
                    Name = "Woodworking",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│W││O││O││D││S│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Hand-crafted furniture and projects. Creating functional art from wood, one project at a time.",
                    ProgressPercentage = 55,
                    Highlights = new List<string>
                    {
                        "Custom furniture building",
                        "Hand tool techniques",
                        "Wood finishing and restoration",
                        "Workshop organization"
                    }
                },
                new Hobby
                {
                    Id = "retrogaming",
                    Name = "Retro Gaming",
                    AsciiArt = "┌─┐┌─┐┌─┐┌─┐┌─┐\n│A││T││A││R││I│\n└─┘└─┘└─┘└─┘└─┘",
                    Description = "Atari 2600, arcade games, and classic console collecting. Preserving gaming history and reliving the golden age of video games.",
                    ProgressPercentage = 95,
                    ImageUrl = "/images/hobbies-collection.png",
                    Highlights = new List<string>
                    {
                        "Atari 2600 collection and maintenance",
                        "Classic arcade cabinet restoration",
                        "Emulation and preservation projects",
                        "Game development on vintage systems"
                    }
                }
            };
        }
    }
}
