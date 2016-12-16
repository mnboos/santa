using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SantasJourney
{
  class Program
  {
    private static bool GetNearestFittingNeighbour(GeoCoordinate lastLocation, double currentWeight, List<Item> availableItems, out Item itemToAdd, out double distance, bool isLatConstraintActive)
    {
      var candidates = availableItems
        .AsParallel()
        .Where(item => currentWeight + item.Weight <= Tour.CMaxWeight
                       && (!isLatConstraintActive || item.Location.Latitude < lastLocation.Latitude))
        .Select(item => new { Item = item, Distance = item.Location.GetDistanceTo(lastLocation) })
        .ToList();

      itemToAdd = new Item();
      distance = 0;
      double minDistance = double.MaxValue;
      bool anyFound = false;
      foreach (var tuple in candidates)
      {
        //if (currentWeight + item.Weight <= Tour.CMaxWeight)
        //{
        var dist = tuple.Distance;
          if (dist < minDistance)
          {
            minDistance = dist;
            distance = dist;
            itemToAdd = tuple.Item;
            anyFound = true;
          }
        //}
      }
      return anyFound;
    }

    static void Main(string[] args)
    {
      int? nrTours = 1;
      var allItems = new List<Item>(CsvLoader.Load());
      var loader = new TourLoader(allItems);
      var allTours = loader.Load(@"..\..\tours_15.12.2016_16.48.37.csv", nrTours).ToList();

      Parallel.ForEach(allTours, t => t.UpdateReindeerWeariness());

      return;

      //var allItems = new List<Item>(CsvLoader.Load());
      var northPole = new GeoCoordinate(90, 0);

      GenerateInitialTours(allItems, northPole);

      //double totalNrDistances = (double)allItems.Count*(double)allItems.Count / 2.0;
      //long count = 0;
      //var distances1 = new Dictionary<string, float>();
      //var distances2 = new Dictionary<string, float>();
      //bool useSecond = false;
      //for (int i = 0; i < allItems.Length; i++)
      //{
      //  for (int j = i+1; j < allItems.Length; j++)
      //  {
      //    //if (i == j)
      //      //continue;

      //    var i1 = allItems[i];
      //    var i2 = allItems[j];

      //    string id = $"{Math.Min(i1.Id, i2.Id)};{Math.Max(i1.Id, i2.Id)}";

      //    //if (distances.ContainsKey(id))
      //      //continue;

      //    //if (distances1.ContainsKey(id))
      //    //  throw new Exception("that should never happen");

      //    if (useSecond)
      //    {
      //      distances2[id] = (float)i1.Location.GetDistanceTo(i2.Location);
      //    }
      //    else
      //    {
      //      distances1[id] = (float) i1.Location.GetDistanceTo(i2.Location);
      //    }
      //    count++;
      //  }
      //  var progress = 100.0 / totalNrDistances * count;
      //  Console.WriteLine($"Progress: {progress * 100} %");
      //  useSecond = progress >= 50;
      //}

      Console.WriteLine("Complete.. presse any key");
      Console.ReadLine();
    }

    private static void GenerateInitialTours(List<Item> allItems, GeoCoordinate northPole)
    {
      var n = DateTime.Now;
      var time = $"{n.Date.ToShortDateString()}_{n.Hour}.{n.Minute}.{n.Second}";
      using (var fs = new FileStream($@"..\..\tours_{time}.csv", FileMode.Create, FileAccess.Write))
      {
        using (var writer = new StreamWriter(fs))
        {
          writer.WriteLine(TourLoader.CHeaders);
          int tourCount = 0;
          while (allItems.Any())
          {
            List<Item> tour = new List<Item>();
            double weight = 0;
            double length = 0;

            bool latitueConstraintActive = true;
            while (weight < Tour.CMaxWeight)
            {
              GeoCoordinate lastPosition = tour.Count > 0 ? tour.Last().Location : northPole;
              Item itemToAdd;
              double distance;
              if (!GetNearestFittingNeighbour(lastPosition, weight, allItems, out itemToAdd, out distance, latitueConstraintActive))
              {
                if (!latitueConstraintActive)
                  break;

                latitueConstraintActive = false;
                continue;
              }

              allItems.Remove(itemToAdd);

              length += distance;
              weight += itemToAdd.Weight;
              tour.Add(itemToAdd);
            }

            var ids = new StringBuilder();
            var weights = new StringBuilder();
            var coords = new StringBuilder();
            foreach (var tourItem in tour)
            {
              ids.Append($"{tourItem.Id},");
              weights.Append($"{tourItem.Weight}+");
              coords.Append($"{tourItem.Location.Latitude} {tourItem.Location.Longitude},");
            }
            var giftIds = ids.ToString().Trim(',');
            var coordinates = coords.ToString().Trim(',');
            var giftWeights = weights.ToString().Trim('+');

            var line = $"{tourCount++};{length};{giftIds};{weight};{giftWeights};{coordinates}";

            Console.WriteLine($"{DateTime.Now}: Remaining items: {allItems.Count}, Weight: {weight}, Length: {length}");

            writer.WriteLine(line);
            writer.Flush();
          }
        }
      }
    }
  }
}
