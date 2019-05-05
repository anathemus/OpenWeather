using System;
using System.Web.Mvc;
using System.Collections.Generic;
using OpenWeather.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace OpenWeather.Controllers
{
    public class HomeController : Controller
    {
        // First tried enum for City. Unable due to space in San Diego, 
        // changed to Dictionary - KeyValuePairs of <string, string> to match city names with states and zipcode
        public Dictionary<string, string> CityInfo
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "Marlborough", "MA (01752)" },
                    { "San Diego","CA (92101)" },
                    { "Cheyenne","WY (82001)" },
                    { "Anchorage","AK (99504)" },
                    { "Austin","TX (78758)" },
                    { "Orlando","FL (32830)" },
                    { "Seattle","WA (98101)" },
                    { "Cleveland","OH (44115)" },
                    { "Portland","ME (04102)" },
                    { "Honolulu","HI (96817)" }
                };
            }
            // no need for set, but could put one if list ever needed to be changed
        }

        // API only accepts city name and country code.
        // Copied exact CityIds to avoid incorrect city data.
        public List<string> CityIds
        {
            get
            {
                return new List<string>
                {
                    "4943170",
                    "5391811",
                    "5821086",
                    "5879400",
                    "4671654",
                    "4167147",
                    "5809844",
                    "5150529",
                    "4975802",
                    "5856195"
                };
            }
        }

        [HttpGet]
        [AsyncTimeout(2500)]
        [HandleError(ExceptionType = typeof(TimeoutException), View = "TimedOut")]
        public ActionResult Index()
        {
            List<OpenWeatherModel> weatherModels = GetOpenWeatherModelData();
            return View(weatherModels);
        }

        // changed from List<CurrentWeather> to List<OpenWeatherModel> to simplify View, refactor code
        private List<OpenWeatherModel> GetOpenWeatherModelData()
        {
            List<OpenWeatherModel> weatherModels = new List<OpenWeatherModel>();
            foreach (var cityId in CityIds)
            {
                OpenWeatherModel model = new OpenWeatherModel();
                CurrentWeather weather = GetWeather(cityId);
                model.City = weather.city.name;
                string result;
                if (CityInfo.TryGetValue(model.City,
                    out result))
                {
                    model.StateZip = result;
                }
                else
                {
                    model.StateZip = "Error!";
                }
                // instantiate lists before adding to them
                model.Precipitation = new List<bool>();
                model.Temperature = new List<double>();
                model.Date = new List<DateTimeOffset>();

                // loop through 5 days
                for (int i = 0; i < 5; i++)
                {
                    DateTimeOffset today = DateTimeOffset.Now.AddDays(i);
                    // get all weather stats for the day
                    IEnumerable<OpenWeather.Models.List> date = new List<OpenWeather.Models.List>();
                    date = from list in weather.list
                           where DateTimeOffset.Parse(list.dt_txt).ToString("yyyy-MM-dd") == today.ToString("yyyy-MM-dd")
                           select list;

                    IEnumerable<float> precipitation = new List<float>();
                    precipitation = from list in date
                                    where list.rain != null
                                    select list.rain._3h;

                    // if else to determine * for rain
                    if (precipitation.Count() > 0)
                    {
                        model.Precipitation.Add(true);
                    }
                    else
                    {
                        model.Precipitation.Add(false);
                    }

                    float avgTemp = 0;
                    foreach (var item in date)
                    {
                        avgTemp += item.main.temp;
                    }
                    avgTemp = avgTemp / date.Count();
                    model.Temperature.Add(avgTemp);

                    model.Date.Add(DateTimeOffset.Parse(date.First().dt_txt));
                }
                weatherModels.Add(model);
            }
            return weatherModels;
        }

        //private async Task<CurrentWeather> GetWeather(string cityId)
        private CurrentWeather GetWeather(string cityId)
        {
            // HttpClient client = new HttpClient();
            using (WebClient client = new WebClient())
            {
                string url = "http://api.openweathermap.org/data/2.5/forecast?id=" + cityId + "&units=imperial&appid=16dc947d12f146c02bd96acad16e117b";

                var result = client.DownloadString(new Uri(url));
                // specify the Type<T> that you're converting to
                var json = JsonConvert.DeserializeObject<CurrentWeather>(result);
                return json;
            }
        }
    }
}