namespace TheDuelX.Shared.Models;

public class Card
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ManaCost { get; set; }
    public int Attack { get; set; }
    public int Health { get; set; }
    public CardType Type { get; set; }
    public CardRarity Rarity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<CardEffect> Effects { get; set; } = new();
}

public enum CardType
{
    Creature,
    Spell,
    Artifact
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class CardEffect
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public EffectType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public EffectTrigger Trigger { get; set; }
}

public enum EffectType
{
    Damage,
    Heal,
    BuffAttack,
    BuffHealth,
    DrawCard,
    DiscardCard,
    Taunt,
    Charge,
    Divine_Shield
}

public enum EffectTrigger
{
    OnPlay,
    OnDeath,
    OnAttack,
    OnTurnStart,
    OnTurnEnd
}