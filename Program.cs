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
    const double CMaxWeight = 1000.0;

    private static bool GetNearestFittingNeighbour(GeoCoordinate lastLocation, double currentWeight, List<Item> availableItems, out Item itemToAdd, out double distance)
    {
      itemToAdd = new Item();
      distance = 0;
      double minDistance = double.MaxValue;
      bool anyFound = false;
      foreach (var item in availableItems)
      {
        if (currentWeight + item.Weight <= CMaxWeight)
        {
          var dist = lastLocation.GetDistanceTo(item.Location);
          if (dist < minDistance)
          {
            minDistance = dist;
            distance = dist;
            itemToAdd = item;
            anyFound = true;
          }
        }
      }
      return anyFound;
    }

    static void Main(string[] args)
    {
      var allItems = new List<Item>(CsvLoader.Load());
      //double totalNrDistances = (double)allItems.Count*(double)allItems.Count / 2.0;

      var northPole = new GeoCoordinate(90, 0);

      //var allTours = new ConcurrentBag<List<Item>>();

      var n = DateTime.Now;
      var time = $"{n.Date.ToShortDateString()}_{n.Hour}.{n.Minute}.{n.Second}";
      using (var fs = new FileStream($@"..\..\tours_{time}.csv", FileMode.Create, FileAccess.Write))
      {
        using (var writer = new StreamWriter(fs))
        {
          writer.WriteLine("Tour;Length;GiftIds;TotalWeight;Weights");
          int tourCount = 0;
          while (allItems.Any())
          {
            List<Item> tour = new List<Item>();
            double weight = 0;
            double length = 0;
            
            while (weight < CMaxWeight)
            {
              GeoCoordinate lastPosition = tour.Count > 0 ? tour.Last().Location : northPole;
              Item itemToAdd;
              double distance;
              if (!GetNearestFittingNeighbour(lastPosition, weight, allItems, out itemToAdd, out distance))
                break;

              allItems.Remove(itemToAdd);

              length += distance;
              weight += itemToAdd.Weight;
              tour.Add(itemToAdd);
            }

            
            var ids = new StringBuilder();
            var weights = new StringBuilder();
            foreach (var tourItem in tour)
            {
              ids.Append($"{tourItem.Id},");
              weights.Append($"{tourItem.Weight}+");
            }
            var giftIds = ids.ToString().Trim(',');
            var giftWeights = weights.ToString().Trim('+');

            var line = $"{tourCount++};{length};{giftIds};{weight};{giftWeights}";

            Console.WriteLine($"{DateTime.Now}: Remaining items: {allItems.Count}, Weight: {weight}, Length: {length}");

            //Console.ReadLine();
            writer.WriteLine(line);
            writer.Flush();
          }
        }
      }

      

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
  }

  public class CsvLoader
  {
    public static Item[] Load()
    {
      var content = File.ReadAllLines(@"..\..\..\Assets\gifts.csv").Skip(1).ToArray();

      var items = new Item[content.Length];

      Parallel.For(0, content.Length, (i) =>
      {
        var line = content[i];
        var v = line.Split(';');
        var id = int.Parse(v[0]);
        var lat = double.Parse(v[1]);
        var lng = double.Parse(v[2]);
        var weight = double.Parse(v[3]);
        items[i] = new Item(id, lat, lng, weight);
      });
      return items;
    }
  }

  public struct Item
  {
    public Item(int id, double lat, double lng, double weight)
    {
      Id = id;
      Weight = weight;
      Location = new GeoCoordinate(lat, lng);
    }

    public int Id;

    public GeoCoordinate Location { get; }
    public double Weight { get; }
  }
}
