using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace cache_worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IMemoryCache _cache;

        public Worker(ILogger<Worker> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int attempt = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                attempt++;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Adding person");
                AddPerson();
                _logger.LogInformation("Getting person");
                GetPeople();
                if (attempt == 10) ClearPeople();
                await Task.Delay(1000, stoppingToken);
            }
        }

        void AddPerson()
        {
            string personList = "";

            personList += $"|=|{JsonSerializer.Serialize(new Person { Name = RandomString(7), Surname = RandomString(6) })}";

            if (!_cache.TryGetValue("newPersons", out string people))
            {
                _cache.Set("newPersons", personList);
            }
            else
            {
                people += $"|=|{personList}";
                _cache.Set("newPersons", people);
            }
        }

        void GetPeople()
        {
            if (_cache.TryGetValue("newPersons", out string people))
            {

            }
            string[] list = people.Split("|=|");

            List<Person> objectList = new List<Person>();
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item)) objectList.Add(JsonSerializer.Deserialize<Person>(item));
            }

            foreach (Person item in objectList)
            {
                Console.WriteLine($"{item.Name} {item.Surname}");
            }
        }

        void ClearPeople()
        {
            Console.WriteLine("Clear");
            _cache.Remove("newPersons");
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }

    class Person
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
