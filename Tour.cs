﻿using System;
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

    public Tour(IEnumerable<Item> items)
    {
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
      double remainingWeight = _weight + SledBaseWeight;
      double weariness = 0;
      var previousLocation = NorthPole;
      foreach (var item in _items)
      {
        weariness += remainingWeight*previousLocation.GetDistanceTo(item.Location);
        previousLocation = item.Location;
        remainingWeight -= item.Weight;
      }
      weariness += SledBaseWeight*_items.Last().Location.GetDistanceTo(NorthPole);

      ReindeerWeariness = weariness;
    }
  }
}
