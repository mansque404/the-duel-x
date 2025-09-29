using TheDuelX.Shared.Models;

namespace TheDuelX.GameEngine;

public class GameManager
{
    private readonly Dictionary<string, GameSession> _activeSessions = new();

    public GameSession CreateGame(int player1Id, int player2Id, List<Card> player1Deck, List<Card> player2Deck)
    {
        var gameId = Guid.NewGuid().ToString();
        
        var gameSession = new GameSession
        {
            Id = gameId,
            Player1Id = player1Id,
            Player2Id = player2Id,
            CurrentPlayerId = player1Id,
            TurnNumber = 1,
            State = GameState.InProgress,
            CreatedAt = DateTime.UtcNow,
            Player1Board = InitializeBoard(player1Id, player1Deck),
            Player2Board = InitializeBoard(player2Id, player2Deck)
        };

        DrawInitialHand(gameSession.Player1Board);
        DrawInitialHand(gameSession.Player2Board);

        _activeSessions[gameId] = gameSession;
        return gameSession;
    }

    public GameSession? GetGame(string gameId)
    {
        return _activeSessions.TryGetValue(gameId, out var session) ? session : null;
    }

    public GameActionResult ProcessAction(GameAction action)
    {
        var game = GetGame(action.GameSessionId);
        if (game == null)
        {
            return GameActionResult.Failure("Jogo não encontrado");
        }

        if (game.State != GameState.InProgress)
        {
            return GameActionResult.Failure("Jogo não está em andamento");
        }

        if (game.CurrentPlayerId != action.PlayerId)
        {
            return GameActionResult.Failure("Não é seu turno");
        }

        var currentBoard = GetPlayerBoard(game, action.PlayerId);
        var opponentBoard = GetOpponentBoard(game, action.PlayerId);

        return action.Type switch
        {
            GameActionType.PlayCard => ProcessPlayCard(game, currentBoard, opponentBoard, action),
            GameActionType.Attack => ProcessAttack(game, currentBoard, opponentBoard, action),
            GameActionType.EndTurn => ProcessEndTurn(game, action.PlayerId),
            GameActionType.Concede => ProcessConcede(game, action.PlayerId),
            _ => GameActionResult.Failure("Ação inválida")
        };
    }

    private GameBoard InitializeBoard(int playerId, List<Card> deck)
    {
        return new GameBoard
        {
            PlayerId = playerId,
            Health = 30,
            Mana = 1,
            MaxMana = 1,
            Deck = new List<Card>(deck),
            Hand = new List<Card>(),
            Field = new List<Card>(),
            Graveyard = new List<Card>()
        };
    }

    private void DrawInitialHand(GameBoard board)
    {
        for (int i = 0; i < 3 && board.Deck.Count > 0; i++)
        {
            DrawCard(board);
        }
    }

    private void DrawCard(GameBoard board)
    {
        if (board.Deck.Count > 0)
        {
            var cardIndex = Random.Shared.Next(board.Deck.Count);
            var card = board.Deck[cardIndex];
            board.Deck.RemoveAt(cardIndex);
            
            if (board.Hand.Count < 10)
            {
                board.Hand.Add(card);
            }
            else
            {
                board.Graveyard.Add(card);
            }
        }
    }

    private GameBoard GetPlayerBoard(GameSession game, int playerId)
    {
        return playerId == game.Player1Id ? game.Player1Board : game.Player2Board;
    }

    private GameBoard GetOpponentBoard(GameSession game, int playerId)
    {
        return playerId == game.Player1Id ? game.Player2Board : game.Player1Board;
    }

    private GameActionResult ProcessPlayCard(GameSession game, GameBoard currentBoard, GameBoard opponentBoard, GameAction action)
    {
        if (action.CardId == null)
        {
            return GameActionResult.Failure("ID da carta não especificado");
        }

        var card = currentBoard.Hand.FirstOrDefault(c => c.Id == action.CardId);
        if (card == null)
        {
            return GameActionResult.Failure("Carta não encontrada na mão");
        }

        if (card.ManaCost > currentBoard.Mana)
        {
            return GameActionResult.Failure("Mana insuficiente");
        }

       
        currentBoard.Mana -= card.ManaCost;
        
       
        currentBoard.Hand.Remove(card);

       
        switch (card.Type)
        {
            case CardType.Creature:
                if (currentBoard.Field.Count >= 7)
                {
                    return GameActionResult.Failure("Campo está cheio");
                }
                currentBoard.Field.Add(card);
                break;

            case CardType.Spell:
                ApplySpellEffects(card, currentBoard, opponentBoard, action.TargetId);
                currentBoard.Graveyard.Add(card);
                break;

            case CardType.Artifact:
                currentBoard.Field.Add(card);
                break;
        }

        return GameActionResult.Success($"Carta {card.Name} jogada com sucesso");
    }

    private void ApplySpellEffects(Card card, GameBoard currentBoard, GameBoard opponentBoard, int? targetId)
    {
        foreach (var effect in card.Effects.Where(e => e.Trigger == EffectTrigger.OnPlay))
        {
            switch (effect.Type)
            {
                case EffectType.Damage:
                    if (targetId.HasValue)
                    {
                        ApplyDamageToTarget(opponentBoard, targetId.Value, effect.Value);
                    }
                    break;

                case EffectType.Heal:
                    if (targetId.HasValue)
                    {
                        ApplyHealToTarget(currentBoard, targetId.Value, effect.Value);
                    }
                    break;

                case EffectType.DrawCard:
                    for (int i = 0; i < effect.Value; i++)
                    {
                        DrawCard(currentBoard);
                    }
                    break;
            }
        }
    }

    private void ApplyDamageToTarget(GameBoard targetBoard, int targetId, int damage)
    {
        if (targetId == 0)
        {
            targetBoard.Health -= damage;
        }
        else
        {
            var targetCreature = targetBoard.Field.FirstOrDefault(c => c.Id == targetId);
            if (targetCreature != null)
            {
                targetCreature.Health -= damage;
                if (targetCreature.Health <= 0)
                {
                    targetBoard.Field.Remove(targetCreature);
                    targetBoard.Graveyard.Add(targetCreature);
                }
            }
        }
    }

    private void ApplyHealToTarget(GameBoard targetBoard, int targetId, int heal)
    {
        if (targetId == 0)
        {
            targetBoard.Health = Math.Min(30, targetBoard.Health + heal);
        }
        else
        {
            var targetCreature = targetBoard.Field.FirstOrDefault(c => c.Id == targetId);
            if (targetCreature != null)
            {
                targetCreature.Health += heal;
            }
        }
    }

    private GameActionResult ProcessAttack(GameSession game, GameBoard currentBoard, GameBoard opponentBoard, GameAction action)
    {
        if (action.CardId == null)
        {
            return GameActionResult.Failure("Atacante não especificado");
        }

        var attacker = currentBoard.Field.FirstOrDefault(c => c.Id == action.CardId);
        if (attacker == null)
        {
            return GameActionResult.Failure("Criatura atacante não encontrada no campo");
        }

        if (action.TargetId == null)
        {
            return GameActionResult.Failure("Alvo não especificado");
        }

        if (action.TargetId == 0)
        {
            opponentBoard.Health -= attacker.Attack;
        }
        else
        {
            var defender = opponentBoard.Field.FirstOrDefault(c => c.Id == action.TargetId);
            if (defender == null)
            {
                return GameActionResult.Failure("Criatura defensora não encontrada");
            }

           
            attacker.Health -= defender.Attack;
            defender.Health -= attacker.Attack;

           
            if (attacker.Health <= 0)
            {
                currentBoard.Field.Remove(attacker);
                currentBoard.Graveyard.Add(attacker);
            }

            if (defender.Health <= 0)
            {
                opponentBoard.Field.Remove(defender);
                opponentBoard.Graveyard.Add(defender);
            }
        }

       
        if (opponentBoard.Health <= 0)
        {
            game.State = GameState.Ended;
            game.WinnerId = currentBoard.PlayerId;
            game.EndedAt = DateTime.UtcNow;
        }

        return GameActionResult.Success("Ataque realizado");
    }

    private GameActionResult ProcessEndTurn(GameSession game, int playerId)
    {
        var currentBoard = GetPlayerBoard(game, playerId);
        
       
        game.CurrentPlayerId = playerId == game.Player1Id ? game.Player2Id : game.Player1Id;
        game.TurnNumber++;

        var nextBoard = GetPlayerBoard(game, game.CurrentPlayerId);
        
       
        nextBoard.MaxMana = Math.Min(10, nextBoard.MaxMana + 1);
        nextBoard.Mana = nextBoard.MaxMana;
        
       
        DrawCard(nextBoard);

        return GameActionResult.Success("Turno finalizado");
    }

    private GameActionResult ProcessConcede(GameSession game, int playerId)
    {
        game.State = GameState.Ended;
        game.WinnerId = playerId == game.Player1Id ? game.Player2Id : game.Player1Id;
        game.EndedAt = DateTime.UtcNow;

        return GameActionResult.Success("Jogo encerrado por desistência");
    }
}

public class GameActionResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();

    public static GameActionResult Success(string message, Dictionary<string, object>? data = null)
    {
        return new GameActionResult
        {
            IsSuccess = true,
            Message = message,
            Data = data ?? new Dictionary<string, object>()
        };
    }

    public static GameActionResult Failure(string message)
    {
        return new GameActionResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}
