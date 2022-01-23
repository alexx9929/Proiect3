using Newtonsoft.Json;
using Proiect3.Models;
using System.Diagnostics;

namespace Proiect3
{
    public class CardsService
    {
        HttpClient _httpClient;
        Dictionary<string, string> _keys;

        public CardsService()
        {
            _httpClient = new HttpClient();
            LoadSecrets();
        }

        public void LoadSecrets()
        {
            using (StreamReader r = new StreamReader("secrets.json"))
            {
                string json = r.ReadToEnd();
                _keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }

        #region  Request creation 
        static string CreateUri(string name, string className, string race)
        {
            string uri = "https://omgvamp-hearthstone-v1.p.rapidapi.com/cards/";
            if (!string.IsNullOrEmpty(name))
            {
                uri += $"search/{name}";
            }
            else
            {
                if (className != "Choose a class")
                {
                    uri += $"classes/{className}";
                }
                else
                {
                    if (race != "Choose a race")
                    {
                        uri += $"races/{race}";
                    }
                    else
                        return "";
                }
            }
            return uri;
        }
        HttpRequestMessage CreateRequest(string uri)
        {
            return new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers ={
                         { "x-rapidapi-host", "omgvamp-hearthstone-v1.p.rapidapi.com" },
                         { "x-rapidapi-key",  _keys["rapidApiKey"].ToString()},
                         },
            };
        }
        public async Task GetCardsTask(string name, string className, string race, Action<List<Card>> onSuccess, Action<string> onError)
        {
            string uri = CreateUri(name, className, race);

            if (!string.IsNullOrEmpty(uri))
            {
                var request = CreateRequest(uri);
                try
                {
                    using (var response = await _httpClient.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();
                        var cards = GetCards(body);
                        FilterCards(ref cards, className, race);
                        onSuccess?.Invoke(cards);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("404"))
					    onSuccess?.Invoke(new List<Card>());
                    else
                        onError?.Invoke(ex.Message);
                }
            }
            else
                onError?.Invoke("You must select at least one filter");
        }
        #endregion


        #region On successful request
        List<Card> GetCards(string requestBody)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Card>>(requestBody);
        }
        void FilterCards(ref List<Card> cards, string className, string race)
        {
            cards = cards.Where(e => !string.IsNullOrEmpty(e.Img)).ToList();
            if (!string.IsNullOrEmpty(className) && className != "Choose a class")
                cards = cards.Where(e => e.PlayerClass == className).ToList();
            if (!string.IsNullOrEmpty(race) && race != "Choose a race")
                cards = cards.Where(e => e.Race == race).ToList();
        }
        #endregion

    }
}
