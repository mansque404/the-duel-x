using Grpc.Core;
using TheDuelX.GameEngine;
using TheDuelX.Shared.Models;

namespace TheDuelX.GrpcService.Services;

public class GameGrpcService : GameService.GameServiceBase
{
    private readonly GameManager _gameManager;
    private readonly ILogger<GameGrpcService> _logger;
    private readonly Dictionary<string, Shared.Models.Card> _cardDatabase;

    public GameGrpcService(GameManager gameManager, ILogger<GameGrpcService> logger)
    {
        _gameManager = gameManager;
        _logger = logger;
        _cardDatabase = InitializeCardDatabase();
    }

    public override Task<CreateGameResponse> CreateGame(CreateGameRequest request, ServerCallContext context)
    {
        try
        {
            var deck = GetDeckFromCardIds(request.DeckCardIds);
            
           
           
            var dummyOpponentDeck = GetDefaultDeck();
            
            var gameSession = _gameManager.CreateGame(request.PlayerId, 999, deck, dummyOpponentDeck);

            return Task.FromResult(new CreateGameResponse
            {
                GameId = gameSession.Id,
                Success = true,
                Message = "Jogo criado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar jogo para jogador {PlayerId}", request.PlayerId);
            return Task.FromResult(new CreateGameResponse
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    public override Task<JoinGameResponse> JoinGame(JoinGameRequest request, ServerCallContext context)
    {
        try
        {
            var game = _gameManager.GetGame(request.GameId);
            if (game == null)
            {
                return Task.FromResult(new JoinGameResponse
                {
                    Success = false,
                    Message = "Jogo não encontrado"
                });
            }

           
            var gameState = ConvertToGrpcGameState(game, request.PlayerId);

            return Task.FromResult(new JoinGameResponse
            {
                Success = true,
                Message = "Ingressou no jogo com sucesso",
                GameState = gameState
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ingressar no jogo {GameId}", request.GameId);
            return Task.FromResult(new JoinGameResponse
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    public override Task<GameActionResponse> PlayCard(PlayCardRequest request, ServerCallContext context)
    {
        try
        {
            var action = new GameAction
            {
                GameSessionId = request.GameId,
                PlayerId = request.PlayerId,
                Type = GameActionType.PlayCard,
                CardId = request.CardId,
                TargetId = request.HasTargetId ? request.TargetId : null,
                Position = request.HasPosition ? request.Position : null,
                Timestamp = DateTime.UtcNow
            };

            var result = _gameManager.ProcessAction(action);
            var game = _gameManager.GetGame(request.GameId);

            return Task.FromResult(new GameActionResponse
            {
                Success = result.IsSuccess,
                Message = result.Message,
                UpdatedState = game != null ? ConvertToGrpcGameState(game, request.PlayerId) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao jogar carta no jogo {GameId}", request.GameId);
            return Task.FromResult(new GameActionResponse
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    public override Task<GameActionResponse> Attack(AttackRequest request, ServerCallContext context)
    {
        try
        {
            var action = new GameAction
            {
                GameSessionId = request.GameId,
                PlayerId = request.PlayerId,
                Type = GameActionType.Attack,
                CardId = request.AttackerId,
                TargetId = request.TargetId,
                Timestamp = DateTime.UtcNow
            };

            var result = _gameManager.ProcessAction(action);
            var game = _gameManager.GetGame(request.GameId);

            return Task.FromResult(new GameActionResponse
            {
                Success = result.IsSuccess,
                Message = result.Message,
                UpdatedState = game != null ? ConvertToGrpcGameState(game, request.PlayerId) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atacar no jogo {GameId}", request.GameId);
            return Task.FromResult(new GameActionResponse
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    public override Task<GameActionResponse> EndTurn(EndTurnRequest request, ServerCallContext context)
    {
        try
        {
            var action = new GameAction
            {
                GameSessionId = request.GameId,
                PlayerId = request.PlayerId,
                Type = GameActionType.EndTurn,
                Timestamp = DateTime.UtcNow
            };

            var result = _gameManager.ProcessAction(action);
            var game = _gameManager.GetGame(request.GameId);

            return Task.FromResult(new GameActionResponse
            {
                Success = result.IsSuccess,
                Message = result.Message,
                UpdatedState = game != null ? ConvertToGrpcGameState(game, request.PlayerId) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar turno no jogo {GameId}", request.GameId);
            return Task.FromResult(new GameActionResponse
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    public override Task<GameStateResponse> GetGameState(GetGameStateRequest request, ServerCallContext context)
    {
        try
        {
            var game = _gameManager.GetGame(request.GameId);
            if (game == null)
            {
                return Task.FromResult(new GameStateResponse());
            }

            return Task.FromResult(new GameStateResponse
            {
                GameState = ConvertToGrpcGameState(game, request.PlayerId)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estado do jogo {GameId}", request.GameId);
            return Task.FromResult(new GameStateResponse());
        }
    }

    public override async Task SubscribeToGameUpdates(GameUpdatesRequest request, IServerStreamWriter<GameUpdateMessage> responseStream, ServerCallContext context)
    {
       
        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var game = _gameManager.GetGame(request.GameId);
                if (game != null)
                {
                    var updateMessage = new GameUpdateMessage
                    {
                        UpdateType = GameUpdateType.CardPlayed,
                        Message = "Estado do jogo atualizado",
                        GameState = ConvertToGrpcGameState(game, request.PlayerId),
                        AffectedPlayerId = request.PlayerId
                    };

                    await responseStream.WriteAsync(updateMessage);
                }

                await Task.Delay(1000, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Streaming cancelado para jogo {GameId}", request.GameId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no streaming de atualizações do jogo {GameId}", request.GameId);
        }
    }

    private List<Shared.Models.Card> GetDeckFromCardIds(IEnumerable<int> cardIds)
    {
        var deck = new List<Shared.Models.Card>();
        foreach (var cardId in cardIds)
        {
            if (_cardDatabase.TryGetValue(cardId.ToString(), out var card))
            {
                deck.Add(new Shared.Models.Card
                {
                    Id = card.Id,
                    Name = card.Name,
                    Description = card.Description,
                    ManaCost = card.ManaCost,
                    Attack = card.Attack,
                    Health = card.Health,
                    Type = card.Type,
                    Rarity = card.Rarity,
                    ImageUrl = card.ImageUrl,
                    Effects = card.Effects
                });
            }
        }
        return deck;
    }

    private List<Shared.Models.Card> GetDefaultDeck()
    {
       
        return _cardDatabase.Values.Take(10).ToList();
    }

    private GameState ConvertToGrpcGameState(GameSession gameSession, int requestingPlayerId)
    {
        return new GameState
        {
            GameId = gameSession.Id,
            Player1Id = gameSession.Player1Id,
            Player2Id = gameSession.Player2Id,
            CurrentPlayerId = gameSession.CurrentPlayerId,
            TurnNumber = gameSession.TurnNumber,
            Status = ConvertGameStateToGrpc(gameSession.State),
            WinnerId = gameSession.WinnerId ?? 0,
            Player1Board = ConvertPlayerBoardToGrpc(gameSession.Player1Board, requestingPlayerId == gameSession.Player1Id),
            Player2Board = ConvertPlayerBoardToGrpc(gameSession.Player2Board, requestingPlayerId == gameSession.Player2Id)
        };
    }

    private PlayerBoard ConvertPlayerBoardToGrpc(GameBoard board, bool showHand)
    {
        return new PlayerBoard
        {
            PlayerId = board.PlayerId,
            Health = board.Health,
            Mana = board.Mana,
            MaxMana = board.MaxMana,
            DeckSize = board.Deck.Count,
            GraveyardSize = board.Graveyard.Count,
            Hand = { showHand ? board.Hand.Select(ConvertCardToGrpc) : Array.Empty<Card>() },
            Field = { board.Field.Select(ConvertCardToGrpc) }
        };
    }

    private Card ConvertCardToGrpc(Shared.Models.Card card)
    {
        return new Card
        {
            Id = card.Id,
            Name = card.Name,
            Description = card.Description,
            ManaCost = card.ManaCost,
            Attack = card.Attack,
            Health = card.Health,
            Type = ConvertCardTypeToGrpc(card.Type),
            Rarity = ConvertCardRarityToGrpc(card.Rarity),
            ImageUrl = card.ImageUrl
        };
    }

    private GameStatus ConvertGameStateToGrpc(Shared.Models.GameState state)
    {
        return state switch
        {
            Shared.Models.GameState.WaitingForPlayers => GameStatus.WaitingForPlayers,
            Shared.Models.GameState.InProgress => GameStatus.InProgress,
            Shared.Models.GameState.Ended => GameStatus.Ended,
            Shared.Models.GameState.Disconnected => GameStatus.Disconnected,
            _ => GameStatus.WaitingForPlayers
        };
    }

    private CardType ConvertCardTypeToGrpc(Shared.Models.CardType type)
    {
        return type switch
        {
            Shared.Models.CardType.Creature => CardType.Creature,
            Shared.Models.CardType.Spell => CardType.Spell,
            Shared.Models.CardType.Artifact => CardType.Artifact,
            _ => CardType.Creature
        };
    }

    private CardRarity ConvertCardRarityToGrpc(Shared.Models.CardRarity rarity)
    {
        return rarity switch
        {
            Shared.Models.CardRarity.Common => CardRarity.Common,
            Shared.Models.CardRarity.Uncommon => CardRarity.Uncommon,
            Shared.Models.CardRarity.Rare => CardRarity.Rare,
            Shared.Models.CardRarity.Epic => CardRarity.Epic,
            Shared.Models.CardRarity.Legendary => CardRarity.Legendary,
            _ => CardRarity.Common
        };
    }

    private Dictionary<string, Shared.Models.Card> InitializeCardDatabase()
    {
       
        var cards = new Dictionary<string, Shared.Models.Card>
        {
            ["1"] = new Shared.Models.Card
            {
                Id = 1,
                Name = "Guerreiro Iniciante",
                Description = "Um guerreiro jovem e determinado.",
                ManaCost = 1,
                Attack = 2,
                Health = 1,
                Type = Shared.Models.CardType.Creature,
                Rarity = Shared.Models.CardRarity.Common,
                ImageUrl = "/images/cards/guerreiro-iniciante.png",
                Effects = new List<Shared.Models.CardEffect>()
            },
            ["2"] = new Shared.Models.Card
            {
                Id = 2,
                Name = "Mago das Chamas",
                Description = "Conjura bolas de fogo devastadoras.",
                ManaCost = 2,
                Attack = 3,
                Health = 2,
                Type = Shared.Models.CardType.Creature,
                Rarity = Shared.Models.CardRarity.Uncommon,
                ImageUrl = "/images/cards/mago-chamas.png",
                Effects = new List<Shared.Models.CardEffect>()
            }
        };

        return cards;
    }
}
