using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantasJourney
{
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
