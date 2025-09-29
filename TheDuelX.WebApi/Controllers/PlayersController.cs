using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDuelX.Data;
using TheDuelX.Shared.Models;

namespace TheDuelX.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayersController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        return await _context.Players.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.Collection)
            .ThenInclude(pc => pc.Card)
            .Include(p => p.Decks)
            .ThenInclude(d => d.Cards)
            .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return NotFound();
        }

        return player;
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(CreatePlayerRequest request)
    {
        if (await _context.Players.AnyAsync(p => p.Username == request.Username))
        {
            return BadRequest("Nome de usuário já existe");
        }

        if (await _context.Players.AnyAsync(p => p.Email == request.Email))
        {
            return BadRequest("Email já existe");
        }

        var player = new Player
        {
            Username = request.Username,
            Email = request.Email,
            Coins = 1000, 
            CreatedAt = DateTime.UtcNow,
            Collection = new List<PlayerCard>(),
            Decks = new List<Deck>()
        };

        var initialCards = await _context.Cards
            .Where(c => c.Rarity == CardRarity.Common)
            .Take(5)
            .ToListAsync();

        foreach (var card in initialCards)
        {
            player.Collection.Add(new PlayerCard
            {
                CardId = card.Id,
                Quantity = 2, 
                AcquiredAt = DateTime.UtcNow
            });
        }

        var initialDeck = new Deck
        {
            Name = "Deck Inicial",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Cards = new List<DeckCard>()
        };

        foreach (var card in initialCards)
        {
            initialDeck.Cards.Add(new DeckCard
            {
                CardId = card.Id,
                Quantity = 2
            });
        }

        player.Decks.Add(initialDeck);

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    [HttpPut("{id}/coins")]
    public async Task<IActionResult> UpdatePlayerCoins(int id, [FromBody] UpdateCoinsRequest request)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        player.Coins = request.Coins;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreatePlayerRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateCoinsRequest
{
    public int Coins { get; set; }
}