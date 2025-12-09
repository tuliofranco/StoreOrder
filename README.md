# StoreOrder

API de gestão de pedidos desenvolvida em **.NET**, com foco em:

- Modelagem de domínio rica (DDD light)
- Boas práticas de arquitetura (camadas `Core`, `Infrastructure`, `Api`)
- **Outbox pattern** para registrar eventos de domínio
- **Cache de leitura** com Redis
- Módulo opcional de **IA/Analytics** que responde perguntas em linguagem natural sobre os pedidos, usando **OpenAI** e **MongoDB**

Este repositório implementa o desafio de backend (“Avaliação DEV backend”), que pede uma API de pedidos com cálculo de totais, persistência em banco relacional e regras de negócio claras (criar pedido, adicionar itens, fechar pedido, etc).

---

## Índice

- [Visão geral da solução](#visao-geral)
- [Stack e tecnologias](#stack)
- [Arquitetura e organização de pastas](#arquitetura)
- [Subindo com Docker](#docker)
- [Configuração de ambiente (.env)](#env)
- [Domínio e regras de negócio](#dominio)
- [API HTTP de pedidos](#api)
- [Módulo de IA / Analytics (Assistant.Ai)](#ia)
- [Testes](#testes)
- [Diagramas](#diagramas)
  - [Order sem IA e sem Mongo](#diagrama-sem-ia)
  - [Order com IA + Mongo](#diagrama-com-ia)
- [Possíveis evoluções](#evolucao)

---

<a id="visao-geral"></a>

## Visão geral da solução

**StoreOrder** é um backend focado em pedidos, dividido em dois blocos principais:

1. **Core + API de Pedidos (`order.Api`)**
   - Criação de pedidos
   - Gerenciamento de itens (adicionar, atualizar quantidade, remover)
   - Cálculo de total
   - Fechamento do pedido com validações de negócio
   - Uso de **Outbox** para registrar eventos de domínio (ex.: `OrderCreated`, `OrderClosed`)
   - Cache de leitura (Redis / in-memory)

2. **Módulo de IA / Analytics (`assistant.Ai`)**
   - API minimalista que recebe uma pergunta em linguagem natural
   - Usa **OpenAI** para gerar SQL e interpretar os resultados
   - Consulta a API de pedidos (endpoint interno)
   - Devolve uma resposta amigável para o usuário
   - Persiste histórico de perguntas/respostas em **MongoDB**

Você pode rodar **apenas a API de pedidos** (sem IA) ou o conjunto completo (API + IA + MongoDB).

---

<a id="stack"></a>

## Stack e tecnologias

**Linguagem / Runtime**

- .NET **9** (SDK 9.x)
- ASP.NET Core Web API (estilo *top-level statements* com `Program.cs` minimalista)

**Backend – Pedidos**

- **Order.Api**
  - ASP.NET Core Web API com Controllers
  - **Swagger / OpenAPI** para documentação
  - `MediatR` para orquestrar casos de uso
  - Tratamento centralizado de erros / ProblemDetails
- **Order.Core**
  - Modelagem de domínio: `Order`, `OrderItem`, `OrderStatus`, `OrderNumber`, `Money`
  - Regras de negócio encapsuladas em *Domain Rules*
  - Eventos de domínio: `OrderCreatedDomainEvent`, `OrderClosedDomainEvent`
- **Order.Infrastructure**
  - **Entity Framework Core** (Npgsql)
  - `StoreOrderDbContext` com mapeamentos e migrations
  - Implementações de repositórios (`IOrderRepository`, etc.)
  - **Unit of Work** (`EfUnitOfWork`) com **transação** + Outbox
  - Integração com **Redis** (`StackExchange.Redis`, `IDistributedCache`)
  - Serviço de cache (`ICacheService`) com implementação **Redis** e **fallback in-memory**

**Banco de dados e ferramentas**

- **PostgreSQL 16** (imagem `postgres:16-alpine`)
- **Redis 7** (imagem `redis:7-alpine`)
- **MongoDB 6** (imagem `mongo:6`)
- **Mongo Express** para visualização do Mongo
- **pgAdmin 4** (`dpage/pgadmin4:8`) para gerenciar o Postgres

**Módulo de IA**

- Projeto `assistant.Ai`:
  - ASP.NET Core **Minimal API**
  - **OpenAI .NET SDK** (`OpenAIClient`)
  - `HttpClient` tipado para consumir a API de pedidos
  - `MongoDB.Driver` para persistir histórico

**Testes**

- `xUnit`
- Testes de unidade e de integração (com containers Docker dedicados via Testcontainers)

**Infraestrutura**

- **Docker / Docker Compose** para orquestrar:
  - API de pedidos
  - IA API
  - Postgres
  - Redis
  - MongoDB + Mongo Express
  - pgAdmin

---

<a id="arquitetura"></a>

## Arquitetura e organização de pastas

Estrutura principal (resumida):

```text
StoreOrder/
├─ docker-compose.yml
├─ .env.example
└─ backend/
   ├─ src/
   │  ├─ order.Api/            # API HTTP principal de pedidos
   │  ├─ order.Core/           # Domínio e casos de uso
   │  ├─ order.Infrastructure/ # Persistência, cache, UoW, Outbox
   │  └─ assistant.Ai/         # Módulo de IA (API + aplicação)
   └─ tests/
      ├─ Order.UnitTests/      # Testes de unidade
      └─ Order.IntegrationTests/ # Testes de integração (Postgres etc.)
````

* A API de pedidos referencia `order.Core` e `order.Infrastructure`.
* `order.Infrastructure` referencia `order.Core`, EF Core, Redis, etc.
* `assistant.Ai` é um serviço separado (sua própria API), mas fala com a API de pedidos.

---

<a id="docker"></a>

## Subindo com Docker

### 1. Copiar o arquivo de exemplo

Na raiz do repositório:

```bash
cp .env.example .env
```

Ajuste variáveis se desejar (porta da API, credenciais do banco, etc.).

### 2. Subir os serviços

```bash
docker compose up --build -d
```

Serviços principais (valores padrão do `.env.example`):

* **API de Pedidos**:

  * URL base: `http://localhost:8900`
  * Swagger: `http://localhost:8900/swagger`
* **IA API (Assistant.Ai)**:

  * URL base: `http://localhost:5233`
  * Swagger: `http://localhost:5233/swagger`
* **PgAdmin**: `http://localhost:5050`
* **Mongo Express**: `http://localhost:8081`
* **PostgreSQL**: `localhost:5432`
* **Redis**: `localhost:6379`

> Se você quiser rodar apenas a API de pedidos, pode comentar os serviços de Mongo, IA, Redis etc. no `docker-compose.yml` e subir somente `db`, `pgadmin` e `api`.

---

<a id="env"></a>

## Configuração de ambiente (.env)

O projeto utiliza um arquivo `.env` na raiz para configurar banco, cache e serviços auxiliares.

Exemplo (resumo do `.env.example`):

```env
# =========================
# PostgreSQL
# =========================
POSTGRES_DB=orders_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432
STRING_CONNECTION=Host=db;Port=5432;Database=orders_db;Username=postgres;Password=postgres

# =========================
# Redis
# =========================
REDIS_PORT=6379
REDIS_CONNECTION=redis:6379

# =========================
# PgAdmin
# =========================
PGADMIN_EMAIL=admin@example.com
PGADMIN_PASSWORD=admin123
PGADMIN_PORT=5050

# =========================
# API
# =========================
API_PORT=8900

# =========================
# OPEN AI
# =========================
OPENAI_API_KEY=
IA_API_PORT=5233

# =========================
# MongoDB
# =========================
MONGO_CONNECTION=mongodb://mongo:27017
MONGO_DB=orderIa
ME_CONFIG_MONGODB_SERVER=mongo
```

> Para ambiente local sem Docker, você pode ajustar a `STRING_CONNECTION` para `Host=localhost` e apontar `REDIS_CONNECTION` para o Redis local.

---

<a id="dominio"></a>

## Domínio e regras de negócio

### Entidade `Order`

Propriedades principais:

* `Id` (GUID)
* `OrderNumber` (string gerada a partir da data/hora, ex: `20251208123-00001`)
- `ClientName` (string – nome do cliente associado ao pedido)
* `Status` (`Open` ou `Closed`)
* `CreatedAt`, `UpdatedAt`, `ClosedAt`, `DeletedAt`
* Coleção de `Items` (lista de `OrderItem`)
* `Total` (value object `Money`)

### Entidade `OrderItem`

* `Id`
* `Description`
* `Quantity`
* `UnitPrice`
* `Subtotal` (`Quantity * UnitPrice`)

### Regras de negócio principais

* Um pedido **sempre nasce `Open`**.
* Não é permitido:

  * adicionar/remover itens em pedidos **fechados**;
  * alterar quantidade de item em pedido fechado.
* Ao **adicionar item**:

  * valida se o pedido está aberto;
  * recalcula o `Total`.
* Ao **alterar quantidade**:

  * se quantidade chegar a `0`, o item é removido do pedido;
  * o `Total` é recalculado com base na diferença de subtotal.
* Ao **remover item**:

  * o `Total` é atualizado subtraindo o subtotal do item removido.
* Ao **fechar o pedido** (`Close()`):

  * verifica se o pedido está `Open`;
  * garante que existe pelo menos **1 item** no pedido;
  * define `Status = Closed`, `ClosedAt` e `UpdatedAt`;
  * dispara um `OrderClosedDomainEvent`.

### Eventos de domínio

* `OrderCreatedDomainEvent`

  * disparado em `Order.Create()`
  * carrega `OrderId`, `OrderNumber` e `OccurredOn`
* `OrderClosedDomainEvent`

  * disparado em `Order.Close()`
  * carrega `OrderId`, `OrderNumber` e timestamp do fechamento

Esses eventos são usados para:

* Persistir registros na **tabela de Outbox** (`OutboxMessages`)
* Permitir integrações/handlers assíncronos (por exemplo, invalidação de cache)

---

<a id="api"></a>

## API HTTP de pedidos (`order.Api`)

A API expõe endpoints REST para:

* **Criar pedido**
* **Listar pedidos**
* **Consultar detalhes de um pedido** (por `OrderNumber` ou `Id`, conforme a rota)
* **Adicionar itens** a um pedido
* **Atualizar quantidade de um item**
* **Remover item**
* **Fechar pedido**

Características:

* Documentação via **Swagger** em `/swagger`
* Uso de `MediatR` para desacoplar Controllers de casos de uso (`Commands` e `Queries`)
* **Migrations automáticas**: ao subir a API, se o banco for relacional, as migrations são aplicadas no startup
* Tratamento centralizado de erros com `ProblemDetails` e middleware de exceção

> Para testar rapidamente, basta subir com Docker e usar o Swagger em `http://localhost:8900/swagger` ou qualquer cliente HTTP.

---

<a id="ia"></a>

## Módulo de IA / Analytics (`assistant.Ai`)

O módulo de IA é uma API separada que permite fazer perguntas em linguagem natural sobre os pedidos.

### Principais responsabilidades

* Receber uma pergunta do usuário (`/ask`)
* Usar **OpenAI** para:

  * gerar um SQL seguro a partir da pergunta
  * interpretar o resultado do SQL em uma resposta em português
* Chamar a API de pedidos (via `HttpClient`) para executar o SQL em um endpoint interno (ex.: `internal/sql`) ou para recuperar dados agregados
* Persistir histórico (pergunta, resposta, modelo e tokens) em **MongoDB**
* Expor o histórico via `GET /history`

### Endpoints principais

* `POST /ask`

  * Request (exemplo):

    ```json
    {
      "pergunta": "Quantos pedidos foram fechados hoje?"
    }
    ```

  * Response (resumo):

    ```json
    {
      "answer": "Hoje foram fechados 3 pedidos, somando R$ 1.250,00 em totais."
    }
    ```

* `GET /history`

  * Retorna os últimos registros de perguntas/respostas, ordenados por data de criação (limitado, ex.: 50 últimos).

### Persistência em MongoDB

* Database: `orderIa`
* Collection: `history`
* Cada documento armazena, por exemplo:

  * `Question`
  * `Answer`
  * `ModelUsed` (ex.: `gpt-4o-mini`)
  * `TokensUsed`
  * `CreatedAt`

### Health checks

A API de IA expõe health checks, por exemplo:

* `GET /health` – resumo do estado da IA e do Mongo
* `GET /health-summary` – detalha cada check registrado

---

<a id="testes"></a>

## Testes

A solução inclui:

* **Testes de unidade**

  * Na pasta `backend/tests/Order.UnitTests`
  * Cobrem principalmente:

    * Regras de domínio (`Order`, `OrderItem`, regras de fechamento etc.)
    * Casos de uso (Handlers de `MediatR`)
* **Testes de integração**

  * Na pasta `backend/tests/Order.IntegrationTests`
  * Sobe containers de Postgres (e demais dependências) para validar:

    * Criação completa de pedido via API
    * Fluxos de escrita/leitura com o banco real
    * Comportamento de Outbox, repositórios e UoW

Exemplo para rodar todos os testes (dentro de `backend`):

```bash
dotnet test
```

Você também pode rodar apenas um projeto de testes específico, por exemplo:

```bash
dotnet test ./tests/Order.UnitTests/Order.UnitTests.csproj
```

---

<a id="diagramas"></a>

## Diagramas

### 1. Diagrama do Order **sem** IA e **sem** Mongo

<a id="diagrama-sem-ia"></a>

Neste cenário, consideramos apenas a API de pedidos, Postgres e Redis (sem o módulo de IA nem MongoDB).

```mermaid
graph LR
  subgraph Cliente
    C[Cliente HTTP / Postman / Insomnia / Swagger]
  end

  subgraph Backend
    API[Order.Api (ASP.NET Core Web API)]
    DB[PostgreSQL 16 (orders_db)]
    Cache[Redis 7 (Cache de leitura)]
    Outbox[OutboxMessages]
  end

  C --> API
  API --> DB
  API --> Cache
  DB --- Outbox
```

**Fluxo resumido:**

1. O cliente chama a **Order.Api**.
2. A API executa casos de uso (via `MediatR`), valida as regras de negócio e persiste dados no **PostgreSQL**.
3. Durante o `Commit` da `UnitOfWork`, eventos de domínio são convertidos em registros na tabela de **Outbox**.
4. Leituras podem ser otimizadas via **Redis** (quando configurado).

---

### 2. Diagrama do Order **com IA** e **com Mongo**

<a id="diagrama-com-ia"></a>

Neste cenário entram o módulo `assistant.Ai`, MongoDB e OpenAI.

```mermaid
graph LR
  subgraph Usuario
    U[Usuario - perguntas em linguagem natural]
  end

  subgraph Core
    API[Order.Api (ASP.NET Core Web API)]
    DB[(PostgreSQL 16)]
    Cache[(Redis 7)]
    Outbox[(OutboxMessages)]
  end

  subgraph IA
    IA[assistant.Ai API (Minimal API + OpenAI)]
    OpenAI[(OpenAI Chat API)]
    Mongo[(MongoDB - orderIa.history)]
  end

  subgraph Ferramentas
    PgAdmin[PgAdmin 4]
    ME[Mongo Express]
  end

  U --> IA
  IA --> OpenAI
  IA --> API
  API --> DB
  API --> Cache
  DB --- Outbox
  IA --> Mongo

  PgAdmin --- DB
  ME --- Mongo
```

**Fluxo típico de uma pergunta de IA:**

1. O usuário envia uma pergunta para `assistant.Ai` (`POST /ask`).
2. `assistant.Ai` usa **OpenAI** para:

   * entender a pergunta
   * gerar um SQL adequado, ou decidir quais endpoints da Order.Api chamar
3. `assistant.Ai` consulta a **Order.Api** (via `HttpClient`) e obtém os dados.
4. A resposta é montada em linguagem natural.
5. Pergunta + resposta + metadados são salvos em **MongoDB**.
6. O usuário recebe a resposta final.

---

<a id="evolucao"></a>

## Possíveis evoluções

Algumas ideias para evolução futura do projeto:

* Implementar um **worker** que consome registros da Outbox e publica em alguma fila (ex.: Azure Service Bus, RabbitMQ), tornando a mensageria realmente assíncrona.
* Adicionar um frontend simples (React/Next.js) para:

  * Listar pedidos
  * Criar pedidos e gerenciar itens
  * Consumir o endpoint da IA visualmente
* Criar uma trilha de auditoria mais rica, com histórico de status (timeline completa por pedido).
* Expandir o módulo de IA para permitir:

  * filtros por período
  * métricas agregadas (ticket médio, total por cliente, etc.)
* Adicionar alertas e dashboards (Grafana/Prometheus) usando os health checks já existentes como base.


---

## Avaliação da Solução – StoreOrder

Este documento resume como o meu projeto **StoreOrder** atende aos critérios da *Avaliação DEV backend*.

---

### 1. A WebAPI atende aos requisitos básicos?

Sim, a minha WebAPI atende aos requisitos funcionais esperados para o desafio.

**Rotas / funcionalidades essenciais (requisitos do desafio):**

* [x] **Iniciar um novo pedido**  
  - Implementei um endpoint para criar um pedido com status inicial `Open`.
  - Ao criar um pedido, gero um `OrderNumber` único e registro o `CreatedAt`.

* [x] **Adicionar produtos ao pedido**  
  - Implementei uma rota para adicionar itens a um pedido existente, recebendo:
    - descrição
    - quantidade
    - valor unitário
  - Sempre que um item é adicionado, o total do pedido é atualizado com base nos itens.

* [x] **Remover produtos do pedido**  
  - Implementei um endpoint para remover um item específico do pedido.
  - Após a remoção, o total do pedido é recalculado.

* [x] **Fechar o pedido**  
  - Criei um endpoint que chama o método `Close()` na entidade `Order`.
  - Nesse fluxo, o pedido passa para `Status = Closed`, e eu preencho `ClosedAt` e `UpdatedAt`.

* [x] **Listar pedidos**  
  - Implementei uma rota para listagem de pedidos, retornando:
    - identificador
    - status
    - datas relevantes
    - total
  - A listagem consulta os pedidos já persistidos no banco de dados.

* [x] **Obter um pedido pelo identificador**  
  - Implementei um endpoint de detalhe para buscar um pedido (por `Id` e/ou `OrderNumber`, conforme a rota).
  - A resposta inclui também a lista de itens associada ao pedido.

---

**Regras de negócio respeitadas:**

* [x] **Impedir alterações em pedidos fechados**  
  - Garanti que não é possível adicionar, atualizar ou remover itens quando o `Status` é `Closed`.  
    Essas validações estão encapsuladas na entidade e nas regras de domínio.

* [x] **Impedir fechamento de pedidos sem itens**  
  - Ao tentar fechar um pedido sem itens, lanço uma exceção de domínio específica, impedindo o fechamento incorreto.

* [x] **Cálculo correto do total do pedido**  
  - O total do pedido é calculado a partir da soma de `Quantity * UnitPrice` de todos os itens.
  - Toda operação de adicionar, remover ou alterar a quantidade de Itens atualiza o `Total` de forma consistente.

---

### 2. O código é limpo, organizado e segue boas práticas?

De forma geral, sim. Eu estruturei a solução com cuidado em relação a design, organização e legibilidade.

**Pontos fortes de organização:**

- Separei o projeto em camadas/projetos bem definidos:
  - `order.Core` → domínio + aplicação (casos de uso).
  - `order.Infrastructure` → persistência, cache, outbox, Unit of Work.
  - `order.Api` → camada HTTP (controllers, DI, middlewares).
  - `assistant.Ai` → módulo de IA separado, que conversa com a API de pedidos via HTTP.
- Mantive boas fronteiras entre:
  - Domínio
  - Infraestrutura
  - API

**Boas práticas que eu adotei:**

- Usei **MediatR** para organizar casos de uso (`Commands` / `Queries` + handlers), evitando lógica de negócio em controllers.
- Implementei uma **Unit of Work** (`IUnitOfWork`) para controlar transação e gravação em Outbox de forma coesa.
- Modelei **repositórios** como interfaces (`IOrderRepository`) desacopladas de EF Core, com implementações na camada de infraestrutura.
- Modelei **Value Objects** (`OrderNumber`, `Money`) para dar mais semântica, encapsular regras e evitar primitivos soltos.
- Padronizei o tratamento de erros com:
  - `ProblemDetails`
  - Um middleware global de exceções.
- Utilize métodos assíncronos (`async/await`) em operações de banco e cache.
- Escrevi **testes unitários e de integração** cobrindo as principais regras de domínio e o fluxo da API.

Como resultado, considero que o projeto está fácil de navegar, entender e evoluir.

---

### 3. Como os princípios de DDD foram aplicados na estrutura do projeto?

Eu apliquei conceitos de DDD de forma pragmática, compatível com o escopo de um desafio técnico, mas sem abrir mão de uma boa modelagem.

**1. Camada de Domínio explícita**

No projeto `order.Core` eu concentrei o domínio e a aplicação:

- Modelei a entidade agregada `Order` como **Aggregate Root**.
- Modelei `OrderItem` como parte do agregado de `Order`.
- Usei o enum `OrderStatus` para representar os estados possíveis do pedido.
- Criei Value Objects como `OrderNumber` e `Money` para dar mais significado aos tipos e encapsular regras de validação e cálculo.
- Encapsulei as regras de negócio em métodos da própria entidade (`Create`, `AddItem`, `RemoveItem`, `ChangeItemQuantity`, `Close`, etc.) e em classes de regra (`OrderItemsRule`, `OrderStatusRule`).

**2. Eventos de domínio**

Eu utilizei eventos de domínio para representar fatos importantes:

- `OrderCreatedDomainEvent`
- `OrderClosedDomainEvent`

Esses eventos são disparados dentro da entidade `Order` quando algo relevante acontece, como:

- criação do pedido
- fechamento do pedido

Na sequência, a **Unit of Work** recolhe esses eventos e os transforma em registros de Outbox, permitindo que o sistema evolua para integrações assíncronas sem acoplar diretamente o domínio a uma tecnologia de mensageria.

**3. Repositórios e abstrações**

- Defini interfaces como `IOrderRepository` na camada de domínio/aplicação.
- As implementações concretas ficam somente na camada de infraestrutura (usando EF Core).
- Os handlers de casos de uso dependem dessas interfaces, e não do `DbContext` diretamente, o que reduz o acoplamento e melhora a testabilidade.

**4. Camada de Aplicação separada da Infraestrutura**

Nos casos de uso (`CreateOrderCommandHandler`, `AddOrderItemCommandHandler`, `CloseOrderCommandHandler`, etc.) eu:

- Orquestro o fluxo da operação.
- Chamo os métodos do domínio (`Order`) para aplicar regras.
- Uso `IOrderRepository` + `IUnitOfWork` para persistir os dados de forma transacional.

Dessa forma, a camada de aplicação não conhece detalhes de:

- `DbContext`
- mapeamentos do EF Core
- strings de conexão

**5. Bounded Contexts**

- Tratei o contexto de **Pedidos** (Orders) como um Bounded Context claro:
  - todas as regras e modelos de pedidos estão concentrados em `order.Core`, `order.Infrastructure` e `order.Api`.
- O módulo de **IA** (`assistant.Ai`) funciona como outro contexto:
  - expõe sua própria API.
  - conversa com a API de pedidos via HTTP.
  - persiste histórico em MongoDB.
  - não acessa diretamente o domínio ou o banco de dados de pedidos, respeitando a separação entre contextos.

