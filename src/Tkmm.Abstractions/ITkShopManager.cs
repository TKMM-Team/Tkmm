namespace Tkmm.Abstractions;

public interface ITkShopManager
{
    public ITkShop? Selected { get; set; }
    
    public IList<ITkShop> OverflowShops { get; }
    
    void MoveUp()
        => Move(-1);

    void MoveDown()
        => Move(1);
    
    void Move(int offset)
    {
        if (Selected is null) {
            return;
        }

        int currentIndex = OverflowShops.IndexOf(Selected);
        int newIndex = currentIndex + offset;

        if (newIndex < 0 || newIndex >= OverflowShops.Count) {
            return;
        }

        ITkShop shop = OverflowShops[newIndex];
        OverflowShops[newIndex] = Selected;
        OverflowShops[currentIndex] = shop;
        Selected = OverflowShops[newIndex];
    }
    
    public Task Initialize(CancellationToken ct = default);
    
    public Task Save(CancellationToken ct = default);
}