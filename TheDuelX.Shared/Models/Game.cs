namespace TheDuelX.Shared.Models;

public class GameSession
{
    public string Id { get; set; } = string.Empty;
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int CurrentPlayerId { get; set; }
    public int TurnNumber { get; set; }
    public GameState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? WinnerId { get; set; }
    
    public GameBoard Player1Board { get; set; } = new();
    public GameBoard Player2Board { get; set; } = new();
}

public class GameBoard
{
    public int PlayerId { get; set; }
    public int Health { get; set; } = 30;
    public int Mana { get; set; } = 1;
    public int MaxMana { get; set; } = 1;
    public List<Card> Hand { get; set; } = new();
    public List<Card> Field { get; set; } = new();
    public List<Card> Deck { get; set; } = new();
    public List<Card> Graveyard { get; set; } = new();
}

public enum GameState
{
    WaitingForPlayers,
    InProgress,
    Ended,
    Disconnected
}

public class GameAction
{
    public string GameSessionId { get; set; } = string.Empty;
    public int PlayerId { get; set; }
    public GameActionType Type { get; set; }
    public int? CardId { get; set; }
    public int? TargetId { get; set; }
    public int? Position { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum GameActionType
{
    PlayCard,
    Attack,
    EndTurn,
    Concede,
    DrawCard,
    UseHeroPower
}