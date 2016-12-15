using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SantasJourney;

namespace Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestSwap()
    {
      var ints = new [] {1, 2, 3};
      Assert.AreEqual(1, ints[0]);
      Assert.AreEqual(3, ints[2]);

      Swap(0,2, ints);
      Assert.AreEqual(3, ints[0]);
      Assert.AreEqual(1, ints[2]);
    }

    [TestMethod]
    public void TestLoadTours()
    {
      var allItems = new List<Item>(CsvLoader.Load());
      var loader = new TourLoader(allItems);
      var allTours = loader.Load(@"..\..\..\Generated Tours_14.12.2016_21.12.9.csv");
    }

    [TestMethod]
    public void LocallyOptimizeTours()
    {
       int? nrTours = null;
      double totalWeariness;
      var twoOptImprovement = $"{TestOptimization(t => t.OptimizeTwoOpt(), out totalWeariness, nrTours):n}";
      //var weightedDistanceImprovement = $"{TestOptimization(t => t.OptimizeByWeightedDistance(), nrTours):n}";

     
    }

    private double TestOptimization(Action<Tour> optimizer, out double totalWeariness, int? nrToursToLoad = null)
    {
      var allItems = new List<Item>(CsvLoader.Load());
      var loader = new TourLoader(allItems);
      var allTours = loader.Load(@"..\..\..\Generated Tours_14.12.2016_21.12.9.csv", nrToursToLoad).ToList();

      Parallel.ForEach(allTours, t => t.UpdateReindeerWeariness());

      var totalWearinessBefore = allTours.Sum(t => t.ReindeerWeariness);

      Parallel.ForEach(allTours, optimizer);

      totalWeariness = allTours.Sum(t => t.ReindeerWeariness);
      bool hasImproved = totalWeariness < totalWearinessBefore;
      var improvement = totalWearinessBefore - totalWeariness;
      return improvement;
    }

    private void Swap(int i1, int i2, int[] items)
    {
      var temp = items[i1];
      items[i1] = items[i2];
      items[i2] = temp;
    }
  }
}
