/* This console program requests users from the "Random User Generator API" at https://randomuser.me/api and displays relevant data
 * Author: Joshua Cullipher
 */  

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UserRequestor
{
    class Program
    {
        private const string Url = "https://randomuser.me/api/";
        private static string[] nationalities = { "au", "br", "ca", "ch", "de", "dk", "es", "fi", "fr", "gb", "ie", "ir", "no", "nl", "nz", "tr", "us" };

        //This method gets the requested number of users from the API and allows the option of choosing their nationalities
        static async Task<JArray> GetUsers(string amount,string nationalities = null)
        {
            JArray results = new JArray();    
            string urlParameters = "?results=" + amount + "&nat=" + nationalities;
            HttpClient client = new HttpClient();
            
            client.BaseAddress = new Uri(Url);

            HttpResponseMessage response = await client.GetAsync(urlParameters);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                JObject userData = JObject.Parse(responseData);

                //This makes the resulting array only contain the relevant user data
                results = (JArray)(userData["results"]);

            }
            else
            {
                Console.WriteLine(response.StatusCode.ToString());
            }
            
            return results;
        }
        static async Task Main(string[] args)
        {   
            //This program requires 2 arguments to function
            if (args.Length != 2)
            {
                throw new Exception("Missing argument(s).");
            }
            if ((args[0] == null) || !(int.TryParse(args[0], out int value)))
            {
                throw new Exception("Amount must be not null and an integer.");
            }
            if (String.IsNullOrWhiteSpace(args[1]) || ((from nationality in nationalities select nationality).Count() > 0))
            {
                JArray users = await GetUsers(args[0], args[1]);
                
                //Groups resulting user list by country and gender and outputs the number of users by country and gender
                var groupByCountryGender = from entry in users group entry by new { country = entry["location"]["country"], gender = entry["gender"] } into newGroup orderby newGroup.Key.country select newGroup;
                
                foreach (var countryGenderGroup in groupByCountryGender)
                {
                    Console.WriteLine(countryGenderGroup.Key);
                    Console.WriteLine(countryGenderGroup.Count());
                }
                
                //Waits for user input
                Console.WriteLine("Press Enter to continue...");
                Console.ReadKey();

                //Groups resulting user list by last name and outputs users last name, first name, username, gender, country, and nationality
                var groupByLastName = from entry in users group entry by entry["name"]["last"] into newGroup orderby newGroup.Key select newGroup;

                Console.WriteLine("\nLast Name, First Name, Username, Gender, Country, Nationality\n");

                foreach (var lastNameGroup in groupByLastName)
                {
                    foreach (var person in lastNameGroup)
                    {
                        Console.WriteLine($"{person["name"]["last"]}, {person["name"]["first"]}, {person["login"]["username"]}, {person["gender"]}, {person["location"]["country"]}, {person["nat"]}");
                    }
                }
            }
            else
            {
                    throw new Exception("Nationaliy(ies) not found.");    
            }
        }
    }
}
