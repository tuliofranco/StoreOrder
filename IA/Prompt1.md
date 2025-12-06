# Utilização de IA no projeto

## Interação 1:
### Pergunta:
Poderia me fazer um brainstorm sobre esse projeto? Gostaria de saber sobre coisas que eu preciso me atentar. Como resposta, eu não gostaria que tivesse código.

### Resposta

Beleza, bora pensar alto sobre essa PoC então 😄
(sem código, só ideias e pontos de atenção).

---

#### 1. Entender o “jogo” por trás do teste

Eles não querem só “uma API que funciona”. Eles querem ver:

* **Se você entende domínio de pedidos** (regras, estados, invariantes).
* **Separa bem camadas** (API / aplicação / domínio / infraestrutura).
* **Aplica DDD de forma simples e consciente.**
* **Cuida de detalhes**: erros, validação, documentação, testes, organização.

Então tudo que você fizer tem que responder, no final, a essas perguntas:

1. Como você modelou o domínio?
2. Onde estão as regras de negócio?
3. Como a API fica agradável de usar?
4. O que você fez além do mínimo?

---

#### 2. Domínio de Pedidos – onde você mais pode brilhar

Coisas para se atentar aqui:

* **Entidades principais**

  * Pedido
  * Item do Pedido (produto + quantidade + preço unitário)
  * Produto (mesmo que seja algo simples/simulado)

* **Estados do pedido**

  * Aberto vs Fechado (mínimo exigido).
  * Pensar se vale ter Cancelado ou outro status (opcional, mas mostra visão).

* **Invariantes importantes (regras “imutáveis”)**

  * Não permitir adicionar/remover produtos se o pedido estiver fechado.
  * Não permitir fechar pedido vazio (sem itens).
  * Quantidade de item deve ser sempre maior que zero.
  * Talvez: não aceitar preço negativo, nem desconto absurdo, etc.

* **Responsabilidade do Pedido**

  * Pedido sabe dizer se pode ser fechado.
  * Pedido sabe calcular total.
  * Pedido sabe se está aberto ou fechado.

Isso mostra claramente que as regras vivem no domínio, não espalhadas em controller.

---

#### 3. Design das rotas e da API

Mesmo sem código, vale pensar na experiência de quem consome:

* **Ações mínimas exigidas:**

  * Iniciar novo pedido
  * Adicionar produto ao pedido
  * Remover produto do pedido
  * Fechar pedido
  * Listar pedidos
  * Buscar pedido por ID (incluindo itens)

* **Coisas para se atentar:**

  * **Consistência de nomes** de rotas, caminhos e verbos HTTP (coerência REST).
  * **Status HTTP corretos**:

    * Pedido não encontrado → erro de recurso não encontrado.
    * Violação de regra de negócio → erro de validação/regra.
  * **Formato das respostas**:

    * Sempre um padrão: `id`, `status`, `total`, `itens`, timestamps etc.
    * Erros com um corpo claro: mensagem, código de erro, detalhes.

* **Paginação e filtros (tarefas opcionais)**

  * Listagem de pedidos com:

    * Página, tamanho da página.
    * Filtro por status (aberto/fechado).
    * Talvez filtro por data de criação (extra).

---

#### 4. Arquitetura e DDD (sem exagero)

Pontos que o avaliador provavelmente vai olhar:

* **Separação de camadas:**

  * **Domínio**: entidades, value objects, regras de negócio, serviços de domínio se precisar.
  * **Aplicação**: casos de uso (ex: abrir pedido, adicionar item, fechar pedido).
  * **Infraestrutura**: repositórios concretos, acesso ao banco, mapeamentos.
  * **API**: controllers, DTOs de entrada/saída, tratamento de requests/responses.

* **DDD “na prática”:**

  * Um **Aggregate Root** claro: Pedido (que comanda seus Itens).
  * **Value Objects** onde fizer sentido:

    * Dinheiro (preço, total).
    * Status do pedido (pode ser um tipo forte, não só string).
  * As regras principais dentro do Aggregate, não em controller.

* **O que evitar:**

  * Deixar regra de negócio só em controller ou só em “services estáticos”.
  * Misturar DTO de API com entidade de domínio.
  * Repositório sabendo demais (regras de negócio indo parar na infra).

---

#### 5. Persistência e modelagem de dados

Mesmo sendo uma PoC, vale caprichar:

* **Modelagem de tabelas/coleções:**

  * Tabela de Pedidos.
  * Tabela de Itens do Pedido (relacionada ao Pedido).
  * Tabela de Produtos (mesmo que pequena/simulada).

* **Coisas para se atentar:**

  * Chaves primárias, chaves estrangeiras, relacionamentos (1-N).
  * Tipos corretos (ex: decimal para valores monetários).
  * Se vai ou não salvar status como string ou inteiro enumerado.

* **Dados de exemplo:**

  * Alguns produtos pré-criados para facilitar testes.
  * Talvez um script ou seed de dados para deixar tudo pronto.

---

#### 6. Validação e tratamento de erros

Aqui é onde muita gente derrapa e você pode se diferenciar:

* **Validações de entrada:**

  * Campos obrigatórios (produto, quantidade, etc.).
  * Quantidade > 0.
  * Produto precisa existir.
  * Pedido precisa existir para adicionar/remover itens.

* **Validações de regra de negócio (no domínio):**

  * Não permitir ações quando o pedido está fechado.
  * Verificar se tem itens antes de fechar.

* **Tratamento de exceções global:**

  * Middleware/filtro para capturar erros inesperados e retornar resposta amigável.
  * Diferenciar:

    * Erro de validação/regra → mensagem clara pro cliente.
    * Erro interno → mensagem genérica, log detalhado internamente.

---

#### 7. Testes (grande ponto para impressionar)

Mesmo que sejam opcionais, valem muito:

* **Por onde focar:**

  * Testes de domínio:

    * Adicionar item em pedido aberto.
    * Impedir adicionar/remover em pedido fechado.
    * Impedir fechar pedido sem itens.
    * Calcular total corretamente.
  * Isso mostra que você testa o “coração” do sistema.

* **Outros testes possíveis:**

  * Casos de uso (aplicação): se chamar “fechar pedido” com ID errado, o que acontece?
  * Um ou outro teste de integração com repositório, se tiver tempo.

---

#### 8. Observabilidade, robustez e “vida real”

Mesmo em teste, você pode mostrar mentalidade de produção:

* **Logs:**

  * Logar operações importantes: criar pedido, fechar, erro ao adicionar item, etc.
  * Preferir logs estruturados (chaves: pedidoId, status, etc.) – sem precisar mostrar implementação, só mencionar.

* **Healthcheck:**

  * Endpoint de saúde (mesmo básico, tipo “API no ar”).
  * Se quiser elevar nível: health do banco.

* **Idempotência / concorrência:**

  * Pensar (nem que seja só citado no README) em:

    * O que acontece se duas requisições tentarem fechar o mesmo pedido ao mesmo tempo?
    * Ou se tentarem adicionar itens em paralelo?

---

#### 9. Extras que podem te diferenciar

Algumas ideias para “plus”:

* **Histórico de status do pedido**
  Guardar quando foi aberto, quando foi fechado, por quem (se tivesse usuário).

* **Campos de auditoria:**

  * Data de criação, data de atualização.

* **Pequena regra de negócio extra:**

  * Por exemplo: pedido fechado calcula e congela o valor total (não muda mais).
  * Ou: limite mínimo/máximo de itens (só se fizer sentido, sem complicar demais).

* **Swagger bem organizado:**

  * Descrição dos endpoints.
  * Exemplos de requisição e resposta.
  * Documentar os possíveis erros de cada rota.

---

#### 10. Apresentação do projeto (README e organização)

Isso é MUITO mais importante do que a maioria imagina:

* **README com:**

  * Visão geral: o que é a API, qual o objetivo do teste.
  * Decisões de arquitetura:

    * Por que usou camadas X, Y, Z.
    * Como aplicou DDD.
  * Como rodar o projeto:

    * Dependências.
    * Comandos para subir a API.
  * Como rodar testes.
  * Exemplos de payloads (criar pedido, adicionar item, fechar pedido).
  * Se fez extras: listar claramente (“Funcionalidades adicionais implementadas”).

* **Organização do repositório:**

  * Pastas claras (API / aplicação / domínio / infraestrutura).
  * Commits pequenos e bem descritos (ao invés de “ajustes gerais”).

---

Se você quiser, no próximo passo eu posso:

* Te ajudar a desenhar **a estrutura de pastas**.
* Sugerir **títulos de seções para o README**.
* E montar uma espécie de “checklist” para você ir ticando conforme faz a PoC (tipo: domínio ✅, testes de domínio ✅, paginação ✅, etc.).
