using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasJourney
{
  public class TourLoader
  {
    public const string CHeaders = "Tour;Length;GiftIds;TotalWeight;Weights";

    private readonly List<Item> _availableItems;

    public TourLoader(IEnumerable<Item> allItems)
    {
      _availableItems = allItems.OrderBy(i => i.Id).ToList();
    }

    private ConcurrentBag<Tour> _allTours = new ConcurrentBag<Tour>();

    public List<Tour> Load(string filePath, int? nrToursToLoad = null)
    {
      _allTours = new ConcurrentBag<Tour>();
      var lines = File.ReadAllLines(filePath).Skip(1).ToArray();
      if (nrToursToLoad.HasValue)
      {
        lines = lines.Take(nrToursToLoad.Value).ToArray();
      }

      Parallel.ForEach(lines, line =>
      {
        Tour t = CreateTour(line);
        _allTours.Add(t);
      });

      return _allTours.OrderBy(t => t.Id).ToList();
    }

    private Tour CreateTour(string line)
    {
      var v = line.Split(';');
      int id = int.Parse(v[0]);
      //double length = double.Parse(v[1]);
      int[] giftIds = v[2].Split(',').Select(int.Parse).ToArray();
      //double totalWeight = double.Parse(v[3]);

      var allItems = giftIds.Select(GetAndRemoveItem).ToList();
      return new Tour(id, allItems);
    }

    private Item GetAndRemoveItem(int id)
    {
      Item? item = null;
      for (int i = 0; i < _availableItems.Count; i++)
      {
        var tempItem = _availableItems[i];
        if (tempItem.Id == id)
        {
          item = tempItem;
          //_availableItems.RemoveAt(i);
          break;
        }
      }
      if (!item.HasValue)
        throw new Exception($"This item seems to occure twice in any route(s): {id}");

      return item.Value;
    }
  }
}
