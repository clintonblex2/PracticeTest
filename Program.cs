using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace StubhubTest
{
    /***
     * 
     *  1.1 I can use a linq query to get the matching events for the selected customer using customer's city since it's a 
     *  common relation between the customer object and list of events.

        1.2 Because the sending of the email is not a determining factor to the processing of the loop and it is a void method 
         i.e. we don't need the result, we can put this in a separate thread so as to complete the loop execution faster.
        - I will format the email and add relevant information (i.e. customer name, event name etc.) to the email body, then call AddToEmail

        1.3 Three emails will be sent to John Smitth with 3 events which are Lady Gaga, U2 and Dua Lipa

        1.4 I can improve the code by putting the email sender in a separate thread. I can as well use a background job 
        like hangfire to schedule sending of email, then keep track of the status of the send email jobs and retry failed email jobs.

        2.1 I will use LINQ to get the ascii values for each city, convert it to an integer and sum it up. I'll get the 
        absolute value of the difference between the ascii values of the two cities. This approach will always return 0 when 
        the source city and the destination city are the same since they'll have the same ascii values. 

        2.2 For 5 closest events to the customer, I will first create a new class with the Event object and distance. 
        I will loop through the events, populate the new event list with the distance for each event between the customer's 
        city and the event city. When I'm done with the looping, I will use LINQ to order the new event list by the distance. 
        Finally, I will take the maximum event which is 5 from the new event list.

        2.3 The expected output for John Smith in New York city is below:
        Customer: John Smith, Event: Metallica in New York
        Customer: John Smith, Event: Phantom of the Opera in New York
        Customer: John Smith, Event: LadyGaGa in New York
        Customer: John Smith, Event: LadyGaGa in Chicago
        Customer: John Smith, Event: LadyGaGa in Boston

        2.4 

        3 I can cache my response and for subsequent calls, I'll check my cache if it exists before I'll proceed to make API call 
        by implementing In-Memory Cache because caching will
        * Reduce database call
        * Reduce web service load
        * Quickly find data instead of database call

        4. 

        5. I can use LINQ to sort it since it supports multiple sorting fields separated by comma
     * 
     */
    public class Program
    {
        public class Event
        {
            public string Name { get; set; }
            public string City { get; set; }
        }

        public class EventWithDistancePrice 
        {
            public Event Event { get; set; }
            public decimal Price { get; set; }
            public int Distance { get; set; }
        }

        public class Customer
        {
            public string Name { get; set; }
            public string City { get; set; }
        }

        static void Main(string[] args)
        {

            var events = new List<Event>{
                new Event{ Name = "Phantom of the Opera", City = "New York"},
                new Event{ Name = "Metallica", City = "Los Angeles"},
                new Event{ Name = "Metallica", City = "New York"},
                new Event{ Name = "Metallica", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "New York"},
                new Event{ Name = "LadyGaGa", City = "Boston"},
                new Event{ Name = "LadyGaGa", City = "Chicago"},
                new Event{ Name = "LadyGaGa", City = "San Francisco"},
                new Event{ Name = "LadyGaGa", City = "Washington"}
                };

            //1. find out all events that are in cities of customer
            // then add to email.
            //var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            var customer = new Customer { Name = "John Smith", City = "New York" };

            // LINQ QUERY APPROACH
            var custEvent = from e in events
                            where e.City == customer.City
                            select e;

            // LOOKUP APPROACH O(1)
            var lookupEvents = events.ToLookup(x => x.City, x => x);
            var custCity = lookupEvents[customer.City];

            var query = lookupEvents[customer.City];

            // 1. TASK - This is for customer's city
            //foreach (var item in query)
            //{
            //    Task.Run(() => AddToEmail(customer, item));
            //}

            // Events closest to customer
            var eventsClosestToCustomer = GetClosestCustomerEvents(customer.City, events);
            
            Console.WriteLine("*****************************");

            foreach (var item in eventsClosestToCustomer)
            {
                AddToEmail(customer, item.Event);
                //Task.Run(() => AddToEmail(customer, item));
            }
            
            /*
            * We want you to send an email to this customer with all events in their city
            * Just call AddToEmail(customer, event) for each event you think they should get
            */


            // You do not need to know how these methods work
            static void AddToEmail(Customer c, Event e)
            {
                decimal price = GetPrice(e);

                int distance = GetDistance(c.City, e.City);

                //int distance = Cache.GetDistanceFromCache(c.City, e.City);
                // Map that holds the city and the distance, sort by distance

                Console.Out.WriteLine($"Customer: {c.Name}, Event: {e.Name} in {e.City}");

                // AAdded the distance of the event  and the price to the email body
                //Console.Out.WriteLine($"Customer: {c.Name}, Event: {e.Name} in {e.City}"
                //+ (distance > 0 ? $" ({distance} miles away from {c.City})" : string.Empty)
                //+ $" for {price:C}");
            }

            //static List<Event> SortEvents(List<Event> events)
            //{
            //    // I can introduce new fields like price and distance
            //    // Then I will use LINQ query to sort the events on price field
            //    // The sorting will be based on the configured customer's preference but for the purpose of this task, I'll sort by distance and then price
            //    return (from e in events
            //            orderby e.Distance, e.Price
            //            select e).ToList();
            //}

            //var customers = new List<Customer>{
            //    new Customer{ Name = "Nathan", City = "New York"},
            //    new Customer{ Name = "Bob", City = "Boston"},
            //    new Customer{ Name = "Cindy", City = "Chicago"},
            //    new Customer{ Name = "Lisa", City = "Los Angeles"}
            //    };
        }

        static List<EventWithDistancePrice> GetClosestCustomerEvents(string custmerCity, List<Event> events, int maxEvent = 5)
        {
            var newCustEvents = new List<EventWithDistancePrice>();

            for (int i = 0; i < events.Count; i++)
            {
                var _event = events[i];
                
                newCustEvents.Add(new EventWithDistancePrice { Event = _event, Distance = GetDistance(custmerCity, _event.City), Price = GetPrice(_event) });
            }

            newCustEvents = newCustEvents.OrderBy(x => x.Distance).ThenBy(x => x.Price).Take(maxEvent).ToList();

            //The sorting will be based on the configured customer's preference but for the purpose of this task, I'll sort by distance and then price
            //newCustEvents = newCustEvents.OrderBy(x => x.Distance).ThenBy(x => x.Price).Take(maxEvent).ToList();

            return newCustEvents;
        }

        static int ReturnDefaultMiles(string fromCity, string toCity)
        {
            if (!string.IsNullOrEmpty(fromCity) && fromCity.Equals("New York", StringComparison.OrdinalIgnoreCase))
                return toCity switch
                {
                    "Chicago" => 711,
                    "Washington" => 203,
                    "Los Angeles" => 2445,
                    "Pittsburgh" => 315,
                    "New York" => 0,
                    _ => -1,
                };

            return -1;
        }

        static decimal GetPrice(Event e)
        {
            // I will get an event price using random with an assumption that event prices can vary
            var random = new Random();
            var rDecimal = new decimal(random.NextDouble());
            return rDecimal * (100 - 10) + 10;

        }

        static int GetDistance(string fromCity, string toCity)
        {
            var fromAscii = fromCity.Select(x => (int)x).Sum();
            var toAscii = toCity.Select(x => (int)x).Sum();
            return Math.Abs(fromAscii - toAscii);
        }

        public static class Cache
        {
            public static int GetDistanceFromCache(string fromCity, string toCity)
            {
                ObjectCache cache = MemoryCache.Default;
                var cacheKey = $"{fromCity.Replace(" ", string.Empty)}:{toCity.Replace(" ", string.Empty)}";
                if (cache.Contains(cacheKey))
                    return (int)cache.Get(cacheKey);
                else
                {
                    int dist = Program.GetDistance(fromCity, toCity);
                    // Store data in the cache    
                    CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                    cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddMinutes(120.0);
                    cache.Add(cacheKey, dist, cacheItemPolicy);

                    return dist;
                }
            }
        }
    }
}
