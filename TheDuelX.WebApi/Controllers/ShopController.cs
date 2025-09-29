using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheDuelX.Data;
using TheDuelX.Shared.Models;

namespace TheDuelX.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly GameDbContext _context;

    public ShopController(GameDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Shop>>> GetShops()
    {
        return await _context.Shops
            .Include(s => s.Items)
            .ThenInclude(i => i.Cards)
            .ThenInclude(c => c.Card)
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Shop>> GetShop(int id)
    {
        var shop = await _context.Shops
            .Include(s => s.Items)
            .ThenInclude(i => i.Cards)
            .ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shop == null)
        {
            return NotFound();
        }

        return shop;
    }

    [HttpPost("{shopItemId}/purchase")]
    public async Task<ActionResult<Purchase>> PurchaseItem(int shopItemId, [FromBody] PurchaseRequest request)
    {
        var shopItem = await _context.ShopItems
            .Include(si => si.Cards)
            .ThenInclude(c => c.Card)
            .FirstOrDefaultAsync(si => si.Id == shopItemId);

        if (shopItem == null)
        {
            return NotFound("Item da loja não encontrado");
        }

        var player = await _context.Players.FindAsync(request.PlayerId);
        if (player == null)
        {
            return NotFound("Jogador não encontrado");
        }

        var totalPrice = shopItem.Price * request.Quantity;
        if (player.Coins < totalPrice)
        {
            return BadRequest("Moedas insuficientes");
        }

        var purchase = new Purchase
        {
            PlayerId = request.PlayerId,
            ShopItemId = shopItemId,
            Quantity = request.Quantity,
            TotalPrice = totalPrice,
            PurchasedAt = DateTime.UtcNow,
            ReceivedCards = new List<PurchasedCard>()
        };

        var random = new Random();
        for (int i = 0; i < request.Quantity; i++)
        {
            foreach (var shopItemCard in shopItem.Cards)
            {
                for (int j = 0; j < shopItemCard.Quantity; j++)
                {
                    if (random.NextDouble() <= shopItemCard.DropRate)
                    {
                        purchase.ReceivedCards.Add(new PurchasedCard
                        {
                            CardId = shopItemCard.CardId,
                            Quantity = 1
                        });

                        var playerCard = await _context.PlayerCards
                            .FirstOrDefaultAsync(pc => pc.PlayerId == request.PlayerId && pc.CardId == shopItemCard.CardId);

                        if (playerCard != null)
                        {
                            playerCard.Quantity++;
                        }
                        else
                        {
                            _context.PlayerCards.Add(new PlayerCard
                            {
                                PlayerId = request.PlayerId,
                                CardId = shopItemCard.CardId,
                                Quantity = 1,
                                AcquiredAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }

        player.Coins -= totalPrice;

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        return Ok(purchase);
    }
}

public class PurchaseRequest
{
    public int PlayerId { get; set; }
    public int Quantity { get; set; }
}