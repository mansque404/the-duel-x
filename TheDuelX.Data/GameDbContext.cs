using Microsoft.EntityFrameworkCore;
using TheDuelX.Shared.Models;

namespace TheDuelX.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardEffect> CardEffects { get; set; }
    public DbSet<PlayerCard> PlayerCards { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<DeckCard> DeckCards { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopItem> ShopItems { get; set; }
    public DbSet<ShopItemCard> ShopItemCards { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchasedCard> PurchasedCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Card entity
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.HasIndex(e => e.Name);
        });

        // Configure CardEffect entity
        modelBuilder.Entity<CardEffect>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.HasOne<Card>().WithMany(c => c.Effects).HasForeignKey(e => e.CardId);
        });

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure PlayerCard entity
        modelBuilder.Entity<PlayerCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player).WithMany(p => p.Collection).HasForeignKey(e => e.PlayerId);
            entity.HasOne(e => e.Card).WithMany().HasForeignKey(e => e.CardId);
            entity.HasIndex(e => new { e.PlayerId, e.CardId }).IsUnique();
        });

        // Configure Deck entity
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Player).WithMany(p => p.Decks).HasForeignKey(e => e.PlayerId);
        });

        // Configure DeckCard entity
        modelBuilder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Deck).WithMany(d => d.Cards).HasForeignKey(e => e.DeckId);
            entity.HasOne(e => e.Card).WithMany().HasForeignKey(e => e.CardId);
            entity.HasIndex(e => new { e.DeckId, e.CardId }).IsUnique();
        });

        // Configure Shop entity
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure ShopItem entity
        modelBuilder.Entity<ShopItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.HasOne(e => e.Shop).WithMany(s => s.Items).HasForeignKey(e => e.ShopId);
        });

        // Configure ShopItemCard entity
        modelBuilder.Entity<ShopItemCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ShopItem).WithMany(si => si.Cards).HasForeignKey(e => e.ShopItemId);
            entity.HasOne(e => e.Card).WithMany().HasForeignKey(e => e.CardId);
            entity.Property(e => e.DropRate).HasPrecision(3, 2);
        });

        // Configure Purchase entity
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player).WithMany().HasForeignKey(e => e.PlayerId);
            entity.HasOne(e => e.ShopItem).WithMany().HasForeignKey(e => e.ShopItemId);
        });

        // Configure PurchasedCard entity
        modelBuilder.Entity<PurchasedCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Purchase).WithMany(p => p.ReceivedCards).HasForeignKey(e => e.PurchaseId);
            entity.HasOne(e => e.Card).WithMany().HasForeignKey(e => e.CardId);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed basic cards
        modelBuilder.Entity<Card>().HasData(
            new Card
            {
                Id = 1,
                Name = "Guerreiro Iniciante",
                Description = "Um guerreiro jovem e determinado.",
                ManaCost = 1,
                Attack = 2,
                Health = 1,
                Type = CardType.Creature,
                Rarity = CardRarity.Common,
                ImageUrl = "/images/cards/guerreiro-iniciante.png"
            },
            new Card
            {
                Id = 2,
                Name = "Mago das Chamas",
                Description = "Conjura bolas de fogo devastadoras.",
                ManaCost = 2,
                Attack = 3,
                Health = 2,
                Type = CardType.Creature,
                Rarity = CardRarity.Uncommon,
                ImageUrl = "/images/cards/mago-chamas.png"
            },
            new Card
            {
                Id = 3,
                Name = "Bola de Fogo",
                Description = "Causa 3 de dano a qualquer alvo.",
                ManaCost = 3,
                Attack = 0,
                Health = 0,
                Type = CardType.Spell,
                Rarity = CardRarity.Common,
                ImageUrl = "/images/cards/bola-fogo.png"
            },
            new Card
            {
                Id = 4,
                Name = "Dragão Ancião",
                Description = "Uma criatura lendária com poder devastador.",
                ManaCost = 8,
                Attack = 8,
                Health = 8,
                Type = CardType.Creature,
                Rarity = CardRarity.Legendary,
                ImageUrl = "/images/cards/dragao-anciao.png"
            },
            new Card
            {
                Id = 5,
                Name = "Escudo Mágico",
                Description = "Protege uma criatura, dando +0/+3.",
                ManaCost = 1,
                Attack = 0,
                Health = 0,
                Type = CardType.Spell,
                Rarity = CardRarity.Common,
                ImageUrl = "/images/cards/escudo-magico.png"
            }
        );

        // Seed card effects
        modelBuilder.Entity<CardEffect>().HasData(
            new CardEffect
            {
                Id = 1,
                CardId = 3,
                Type = EffectType.Damage,
                Description = "Causa 3 de dano",
                Value = 3,
                Trigger = EffectTrigger.OnPlay
            },
            new CardEffect
            {
                Id = 2,
                CardId = 4,
                Type = EffectType.Damage,
                Description = "Causa 2 de dano a todas as criaturas inimigas",
                Value = 2,
                Trigger = EffectTrigger.OnPlay
            },
            new CardEffect
            {
                Id = 3,
                CardId = 5,
                Type = EffectType.BuffHealth,
                Description = "Dá +3 de vida à criatura alvo",
                Value = 3,
                Trigger = EffectTrigger.OnPlay
            }
        );

        // Seed a default shop
        modelBuilder.Entity<Shop>().HasData(
            new Shop
            {
                Id = 1,
                Name = "Loja Principal",
                Description = "A loja principal do jogo onde você pode comprar cartas.",
                IsActive = true
            }
        );

        // Seed shop items
        modelBuilder.Entity<ShopItem>().HasData(
            new ShopItem
            {
                Id = 1,
                ShopId = 1,
                Name = "Pacote Iniciante",
                Description = "Contém 5 cartas comuns e 1 incomum.",
                Price = 100,
                Type = ShopItemType.CardPack
            },
            new ShopItem
            {
                Id = 2,
                ShopId = 1,
                Name = "Pacote Premium",
                Description = "Contém 3 cartas comuns, 2 incomuns e 1 rara.",
                Price = 250,
                Type = ShopItemType.CardPack
            }
        );
    }
}