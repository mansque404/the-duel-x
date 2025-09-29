# Conventional Commits

Este projeto segue o padrão [Conventional Commits](https://www.conventionalcommits.org/) para mensagens de commit.

## Formato

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

## Tipos

- **feat**: nova funcionalidade para o usuário
- **fix**: correção de bug
- **docs**: mudanças na documentação
- **style**: formatação, ponto e vírgula ausente, etc; sem mudança de código
- **refactor**: refatoração de código de produção
- **test**: adicionar testes, refatorar testes; sem mudança de código de produção
- **chore**: atualizar tarefas de build, configurações do gerenciador de pacotes, etc; sem mudança de código de produção
- **perf**: mudança de código que melhora performance
- **ci**: mudanças relacionadas à integração contínua
- **build**: mudanças que afetam o sistema de build ou dependências externas

## Escopo (opcional)

- **ui**: interface do usuário
- **game**: lógica do jogo
- **api**: API REST
- **grpc**: serviços gRPC
- **database**: banco de dados
- **config**: configurações

## Exemplos

```bash
feat(game): add spell cards functionality
fix(ui): resolve card hover animation issue
docs: update installation instructions
style(ui): format game layout components
refactor(api): simplify shop service logic
test(game): add unit tests for card interactions
chore: update dependencies to latest versions
perf(game): optimize card rendering performance
```

## Configuração

Para usar o template de commit:

```bash
git config commit.template .gitmessage
```