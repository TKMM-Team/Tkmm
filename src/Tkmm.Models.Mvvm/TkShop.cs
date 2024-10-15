using CommunityToolkit.Mvvm.ComponentModel;
using Tkmm.Abstractions;

namespace Tkmm.Models.Mvvm;

public sealed partial class TkShop : ObservableObject, ITkShop
{   
    [ObservableProperty]
    private string _shopName = string.Empty;
    
    [ObservableProperty]
    private string _npcName = string.Empty;
    
    [ObservableProperty]
    private string _location = string.Empty;
    
    [ObservableProperty]
    private string _requiredQuest = string.Empty;
    
    [ObservableProperty]
    private string _npcActorName = string.Empty;
    
    [ObservableProperty]
    private string _map = string.Empty;
    
    [ObservableProperty]
    private Coordinate _coordinates = new();
    
    ITkShop.ICoordinate ITkShop.Coordinates => Coordinates;

    public partial class Coordinate : ObservableObject, ITkShop.ICoordinate
    {
        [ObservableProperty]
        private double _x;
        
        [ObservableProperty]
        private double _y;
    }
    
    public override string ToString()
    {
        return $"""
            {ShopName}
            {NpcName} in {Location} after completeing {RequiredQuest}
            [{Coordinates.X}, {Coordinates.Y}, {Map}]
            [{NpcActorName}]
            """;
    }
}