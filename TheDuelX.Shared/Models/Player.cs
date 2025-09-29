namespace TheDuelX.Shared.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Coins { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PlayerCard> Collection { get; set; } = new();
    public List<Deck> Decks { get; set; } = new();
}

public class PlayerCard
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int CardId { get; set; }
    public int Quantity { get; set; }
    public DateTime AcquiredAt { get; set; }
    
    public Player Player { get; set; } = null!;
    public Card Card { get; set; } = null!;
}

public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlayerId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DeckCard> Cards { get; set; } = new();
    
    public Player Player { get; set; } = null!;
}

public class DeckCard
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public int CardId { get; set; }
    public int Quantity { get; set; }
    
    public Deck Deck { get; set; } = null!;
    public Card Card { get; set; } = null!;
}