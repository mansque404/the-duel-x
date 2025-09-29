using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDuelX.Data;
using TheDuelX.Shared.Models;

namespace TheDuelX.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly GameDbContext _context;

    public CardsController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Card>>> GetCards()
    {
        return await _context.Cards
            .Include(c => c.Effects)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Card>> GetCard(int id)
    {
        var card = await _context.Cards
            .Include(c => c.Effects)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null)
        {
            return NotFound();
        }

        return card;
    }

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<IEnumerable<PlayerCard>>> GetPlayerCards(int playerId)
    {
        return await _context.PlayerCards
            .Include(pc => pc.Card)
            .ThenInclude(c => c.Effects)
            .Where(pc => pc.PlayerId == playerId)
            .ToListAsync();
    }

    [HttpGet("rarity/{rarity}")]
    public async Task<ActionResult<IEnumerable<Card>>> GetCardsByRarity(CardRarity rarity)
    {
        return await _context.Cards
            .Include(c => c.Effects)
            .Where(c => c.Rarity == rarity)
            .ToListAsync();
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<Card>>> GetCardsByType(CardType type)
    {
        return await _context.Cards
            .Include(c => c.Effects)
            .Where(c => c.Type == type)
            .ToListAsync();
    }
}