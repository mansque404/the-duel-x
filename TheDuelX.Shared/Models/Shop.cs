namespace TheDuelX.Shared.Models;

public class Shop
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ShopItem> Items { get; set; } = new();
}

public class ShopItem
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public ShopItemType Type { get; set; }
    public List<ShopItemCard> Cards { get; set; } = new();
    
    public Shop Shop { get; set; } = null!;
}

public class ShopItemCard
{
    public int Id { get; set; }
    public int ShopItemId { get; set; }
    public int CardId { get; set; }
    public int Quantity { get; set; }
    public double DropRate { get; set; } // Probabilidade de aparecer no pack (0.0 a 1.0)
    
    public ShopItem ShopItem { get; set; } = null!;
    public Card Card { get; set; } = null!;
}

public enum ShopItemType
{
    CardPack,
    SingleCard,
    Currency,
    Cosmetic
}

public class Purchase
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ShopItemId { get; set; }
    public int Quantity { get; set; }
    public int TotalPrice { get; set; }
    public DateTime PurchasedAt { get; set; }
    public List<PurchasedCard> ReceivedCards { get; set; } = new();
    
    public Player Player { get; set; } = null!;
    public ShopItem ShopItem { get; set; } = null!;
}

public class PurchasedCard
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public int CardId { get; set; }
    public int Quantity { get; set; }
    
    public Purchase Purchase { get; set; } = null!;
    public Card Card { get; set; } = null!;
}