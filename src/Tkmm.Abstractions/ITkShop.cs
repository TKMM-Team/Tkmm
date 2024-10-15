using System.Numerics;

namespace Tkmm.Abstractions;

public interface ITkShop
{
    string ShopName { get; set; }

    string NpcName { get; set; }

    string Location { get; set; }

    string RequiredQuest { get; set; }

    string NpcActorName { get; set; }

    string Map { get; set; }

    ICoordinate Coordinates { get; }

    public interface ICoordinate
    {
        double X { get; set; }
        
        double Y { get; set; }
    }
}