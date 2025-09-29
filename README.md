# The Duel X - Trading Card Game

Um jogo de cartas colecionáveis (TCG) simplificado inspirado em Hearthstone e Magic: The Gathering.

## Tecnologias Utilizadas

- **Blazor Server**: Interface do usuário com campo de batalha interativo
- **gRPC**: Comunicação em tempo real para turnos e ações de jogo  
- **ASP.NET Core Web API**: Gerenciamento de loja e inventário
- **Entity Framework Core**: ORM para persistência de dados
- **SQL Server**: Banco de dados principal

## Estrutura do Projeto

```
TheDuelX/
├── TheDuelX.BlazorApp/          # Interface Blazor Server
├── TheDuelX.GameEngine/         # Lógica central do jogo
├── TheDuelX.GrpcService/        # Serviços gRPC para tempo real
├── TheDuelX.WebApi/             # API REST para loja e inventário
├── TheDuelX.Data/               # Camada de dados (Entity Framework)
├── TheDuelX.Shared/             # Modelos compartilhados
└── TheDuelX.Tests/              # Testes unitários
```

## Funcionalidades Principais

### 🎮 Gameplay
- Sistema de turnos baseado em tempo real
- Cartas com atributos: Ataque, Defesa, Custo de Mana, Efeitos especiais
- Campo de batalha interativo com animações
- Gerenciamento de pontos de vida e mana

### 🃏 Cartas e Baralhos
- Coleção pessoal de cartas
- Montagem de baralhos customizados
- Sistema de raridade das cartas
- Efeitos especiais programáveis

### 🏪 Sistema de Loja
- Compra de pacotes de cartas
- Inventário de cartas por jogador
- Sistema de moedas/economia do jogo

## Como Executar

1. Clone o repositório
2. Configure a string de conexão no `appsettings.json`
3. Execute as migrações do Entity Framework:
   ```bash
   dotnet ef database update
   ```
4. Inicie os projetos na seguinte ordem:
   - TheDuelX.GrpcService
   - TheDuelX.WebApi
   - TheDuelX.BlazorApp

## Desenvolvimento

### Pré-requisitos
- .NET 9.0 SDK
- SQL Server (LocalDB para desenvolvimento)
- Visual Studio 2022 ou VS Code

### Configuração do Banco de Dados
O projeto utiliza SQL Server com Entity Framework Core. Para desenvolvimento local, configure LocalDB no arquivo `appsettings.json`.

---

*Projeto em desenvolvimento - Trading Card Game simplificado para demonstração de tecnologias .NET*