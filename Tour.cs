using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasJourney
{
  public class Tour
  {
    private const double SledBaseWeight = 10.0;
    public const double CMaxWeight = 1000.0;

    public static GeoCoordinate NorthPole = new GeoCoordinate(90, 0);

    private readonly List<Item> _items;
    private double _weight;

    public double ReindeerWeariness { get; private set; }

    public int Id { get; }

    public Tour(int id, IEnumerable<Item> items)
    {
      Id = id;
      _items = items.ToList();
      _weight = _items.Sum(i => i.Weight);
      if (_weight > CMaxWeight)
        throw new Exception("This tour is to heavy, you'll kill all the reindeers");

      if (_items.Count == 0)
        throw new Exception("An empty tour is not allowed");
    }

    /// <summary>
    /// Calcultes the reindeer weariness for the complete tour
    /// We should whereever possible only refresh the weariness for parts of the tour 
    /// </summary>
    public void UpdateReindeerWeariness()
    {
      var weariness = GetWeariness(_items);

      ReindeerWeariness = weariness;
    }

    private double GetWeariness(List<Item> items)
    {
      double remainingWeight = _weight + SledBaseWeight;
      double weariness = 0;
      var previousLocation = NorthPole;
      foreach (var item in items)
      {
        weariness += remainingWeight*previousLocation.GetDistanceTo(item.Location);
        previousLocation = item.Location;
        remainingWeight -= item.Weight;
      }
      weariness += SledBaseWeight*items.Last().Location.GetDistanceTo(NorthPole);
      return weariness;
    }

    public void OptimizeByWeightedDistance()
    {
      var sorted = _items
        //.OrderByDescending(i => i.Weight)
        .OrderByDescending(i => NorthPole.GetDistanceTo(i.Location)/i.Weight)
        .ToList();

      var weariness = GetWeariness(sorted);
      if (weariness < ReindeerWeariness)
      {
        _items.Clear();
        _items.AddRange(sorted);
        ReindeerWeariness = weariness;
      }
    }

    public void OptimizeTwoOpt()
    {
      var tourCopy = _items.ToList();
      double bestWeariness = ReindeerWeariness;
      bool hasImproved = true;
      while (hasImproved)
      {
        hasImproved = false;

        for (int i = 0; i < tourCopy.Count; i++)
        {
          for (int j = i + 1; j < tourCopy.Count; j++)
          {
            Swap(i, j, tourCopy);
            var newWeariness = GetWeariness(tourCopy);
            if (newWeariness <= bestWeariness)
            {
              bestWeariness = newWeariness;
              hasImproved = true;
            }
            else
            {
              Swap(i, j, tourCopy);
            }
          }
        }
      }
      if (bestWeariness < ReindeerWeariness)
      {
        ReindeerWeariness = bestWeariness;
        _items.Clear();
        _items.AddRange(tourCopy);
      }
    }

    private void Swap(int i1, int i2, List<Item> items)
    {
      var temp = items[i1];
      items[i1] = items[i2];
      items[i2] = temp;
    }
  }
}
