using System;
using System.Web.Mvc;
using System.Collections.Generic;
using OpenWeather.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace OpenWeather.Controllers
{
    // must be named to match RouetConfig.cs
    public class HomeController : Controller
    {
        // to be able to pass to web page
        //public CurrentWeather WeatherForecast { get; set; }

        // First tried enum for City. Unable due to space in San Diego, 
        // changed to KeyValuePairs of <string, string> to match city names with states and zipcode
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

        // State as enum, change to string with .ToString() same order as City
        // to make matching city and state easier
        //public enum State
        //{
        //    MA,
        //    CA,
        //    WY,
        //    AK,
        //    TX,
        //    FL,
        //    WA,
        //    OH,
        //    ME,
        //    HI
        //}

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
        public async Task<ActionResult> Index()
        {
            List<CurrentWeather> Weather = await GetWeatherList();
            return View(Weather);
        }

        private async Task<List<CurrentWeather>> GetWeatherList()
        {
            List<CurrentWeather> weatherList = new List<CurrentWeather>();
            foreach (var cityId in CityIds)
            {
                weatherList.Add(await GetWeather(cityId));
            }
            return weatherList;
        }

        // using WebClient instead of HttpClient.
        // HttpClient got ugly and hung. Conflicting threads it seems
        // from comments in StackOverflow

        //public async Task GetWeatherForecastAsync(string cityId)
        //{
        //    WeatherForecast = await GetWeather(cityId);
        //}

        //public CurrentWeather GetWeatherForecast(string cityId)
        //{
        //    await GetWeatherForecastAsync(cityId);
        //    return WeatherForecast;
        //    // BaseAddress means BASEADDRESS, just the web address
        //    //client.BaseAddress = new Uri("http://api.openweathermap.org");
        //    //client.DefaultRequestHeaders.Accept.Clear();
        //    //client.DefaultRequestHeaders.Accept.Add(new 
        //    //    MediaTypeWithQualityHeaderValue("application/json"));
        //    //client.Timeout = TimeSpan.FromSeconds(60);

        //    var result = "";
        //    try
        //    {
        //        result = await GetWeatherAsync(cityId);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //    }
        //    return result;
        //}
        private async Task<CurrentWeather> GetWeather(string cityId)
        {
            HttpClient client = new HttpClient();
            string url = $"http://api.openweathermap.org/data/2.5/forecast?id={cityId}"
                + "&units=imperial&appid=16dc947d12f146c02bd96acad16e117b";

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                // specify the Type<T> that you're converting to
                var json = JsonConvert.DeserializeObject<CurrentWeather>(result);
                return json;
            }
            else
            {
                Debug.WriteLine(response.ToString());
                return null;
            }
        }
    }
}