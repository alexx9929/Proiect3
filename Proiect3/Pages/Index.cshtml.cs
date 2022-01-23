using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Newtonsoft.Json;
using Proiect3.Models;

namespace Proiect3.Pages
{
    public class IndexModel : PageModel
    {
        private CardsService _cardsService;
        private readonly ILogger<IndexModel> _logger;
        public string Name { get; set; }
        public string Class { get; set; }
        public string Race { get; set; }

        public string ErrorMessage { get; set; }
        public List<Card> Cards { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            _cardsService = new CardsService();
        }

        public void OnGet()
        {

        }

        public void OnPost()
        {
            Cards = new List<Card>();
            ErrorMessage = "";
            Name = Request.Form[nameof(Name)];
            Class = Request.Form[nameof(Class)];
            Race = Request.Form[nameof(Race)];

            if (CanProcessRequest())
            {
                Task task = _cardsService.GetCardsTask(Name, Class, Race,
                (cards) =>
                {
                    Cards = cards.OrderBy(x => x.Name).ToList();
                },
                (errorMessage) =>
                {
                    ErrorMessage = errorMessage;
                }
                );
                task.Wait();
            }
            else
            {
                ErrorMessage = "Name must contain more than one letter";
            }
            Page();
        }

        public bool CanProcessRequest()
        {
            return Name.Length > 1;
        }
    }
}