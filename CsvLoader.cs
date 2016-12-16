using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasJourney
{
  public class CsvLoader
  {
    public static Item[] Load(string path = @"..\..\Assets\gifts.csv")
    {
      var content = File.ReadAllLines(path).Skip(1).ToArray();

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
}
