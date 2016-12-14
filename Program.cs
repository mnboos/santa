using System;
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
    static void Main(string[] args)
    {
      var distances = new Dictionary<string, double>();
      var allItems = CsvLoader.Load();
      double totalNrDistances = (double)allItems.Length*(double)allItems.Length/2.0;
      for (int i = 0; i < allItems.Length; i++)
      {
        for (int j = 0; j < allItems.Length; j++)
        {
          if (i == j)
            continue;

          var i1 = allItems[i];
          var i2 = allItems[j];

          string id = $"{Math.Min(i1.Id, i2.Id)};{Math.Max(i1.Id, i2.Id)}";

          if (distances.ContainsKey(id))
            continue;

          distances[id] = i1.Location.GetDistanceTo(i2.Location);
          
        }
        var progress = 100.0 / totalNrDistances * distances.Count;
        Console.WriteLine($"Progress: {progress*100} %");
      }
    }
  }

  public class CsvLoader
  {
    public static Item[] Load()
    {
      var content = File.ReadAllLines(@"..\..\Assets\gifts.csv").Skip(1).ToArray();

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
