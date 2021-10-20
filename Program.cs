using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleTables;

namespace ApiClient2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to my One List Client!");

            //Use first argument as token, ask for user token if there is none
            var token = (args.Length != 0 ? args[0] : PromptInput("Which list would you like to access?"));

            var keepGoing = true;
            var userInput = "";

            while (keepGoing)
            {
                userInput = PromptInput(
                  "Select one option from the following menu:" + "\n" +
                  "1. Get one item by ID" + "\n" +
                  "2. Get all items" + "\n" +
                  "3. Create a new item" + "\n" +
                  "4. Update an item" + "\n" +
                  "5. Delete an item" + "\n" +
                  "Say 'quit' to exit the program"
                );

                switch (userInput)
                {
                    case "1":
                        //Get one item
                        await GetOneItem(token, PromptInput("What is the item ID?"));
                        break;
                    case "2":
                        //Get all items
                        await ShowAllItems(token);
                        break;
                    case "3":
                        //Create an item
                        await AddOneItem(token);
                        break;
                    case "4":
                        //Update an item
                        break;
                    case "5":
                        //Delete an item
                        break;
                    default:
                        keepGoing = false;
                        break;
                }
            }


        }

        public static String PromptInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        public static async Task ShowAllItems(string token)
        {
            var client = new HttpClient();
            var responseBodyAsStream = await client.GetStreamAsync($"https://one-list-api.herokuapp.com/items?access_token={token}");
            var items = await JsonSerializer.DeserializeAsync<List<Item>>(responseBodyAsStream);
            var table = new ConsoleTable("Description", "Created At", "Completed");

            foreach (var i in items)
            {
                table.AddRow(i.Text, i.CreatedAt, i.Complete);
            }

            table.Write();
        }

        public static async Task GetOneItem(string token, string id)
        {
            try
            {
                var client = new HttpClient();
                var responseBodyAsStream = await client.GetStreamAsync($"https://one-list-api.herokuapp.com/items/{id}?access_token={token}");
                var item = await JsonSerializer.DeserializeAsync<Item>(responseBodyAsStream);
                var table = new ConsoleTable("Description", "Created At", "Completed");
                table.AddRow(item.Text, item.CreatedAt, item.Complete);
                table.Write();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"https://http.cat/{e.StatusCode}");
            }
        }

        public static async Task AddOneItem(string token)
        {
            var client = new HttpClient();

            var url = $"https://one-list-api.herokuapp.com/items?access_token={token}";

            Item newItem = new Item();
            newItem.Text = PromptInput("What is the task?");
            newItem.Complete = PromptInput("Is this task completed? Y/N").ToUpper() == "Y" ? true : false;

            var jsonBody = JsonSerializer.Serialize(newItem);

            var jsonBodyAsContent = new StringContent(jsonBody);
            jsonBodyAsContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(url, jsonBodyAsContent);

            var responseJson = await response.Content.ReadAsStreamAsync();

            Item rItem = await JsonSerializer.DeserializeAsync<Item>(responseJson);

            var table = new ConsoleTable("ID", "Description", "Created At", "Updated At", "Completed");

            table.AddRow(rItem.Id, rItem.Text, rItem.CreatedAt, rItem.UpdatedAt, rItem.Complete);

            table.Write();

        }
    }
}