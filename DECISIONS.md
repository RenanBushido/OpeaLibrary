## Projeto OpeaLibrary

# Cenario

De acordo com o cenário proposto, foi utilizado as seguintes tecnologias:

-   .NET 10
-   EntityFrameworkCore
-   MongoClient
-   Docker
-   xUnit
-   IMapper
-   MediatR
-   Claude Code
-   OpenSpec

As implementações destas tecnologias, principalmente para o .NET, utilizei as recomendações da Microsoft como: Construtores, Switches e Minimal API.

Utilizei a combinação do Claude Code com o OpenSpec para desenvolvimento voltado ao Spec Driven Development. Criei também uma skill para que cada iteração do OpenSpec, eu executava esta skill para verificar a aderência do projeto com as boas práticas e documentação (MCP context7).

# Padrões de Arquitetura e código

-   Clean Architecture
-   CQRS Patthern
-   Domain Driven Design
-   S.O.L.I.D. Principles
-   Event Source

**1 - Clean Architecture** Utilizei mais uma funcionalidade não mencionada nos requisitos, que foi a CrossCutting. No meu entendimento, eu consigo desaclopar os serviços que servirão a aplicação (API) por responsabilidades, capturar erros globais e funcionalidades que não pertençam a uma camada específica, mas necessária para toda aplicação.
**2 - CQRS Pattern** Para este pattern eu dividi as minhas interfaces de persistência em duas: leitura e escrita. (READ e WRITE). A responsabilidade de escrita foram implementadas utilizando o ORM EntityFramework com o padrão IUnitOfWork, para garantir integridade e latência das transações. (Atomicidade ACID).
**3 - DDD** Utilzei o Entity - Id para garantir a identidade única e imutabilidade da entidade.
**4 - S.O.L.I.D.** Todas as classes implementadas foram utilizadas os principios deste patthern. Principalmente a inversão de dependência que com a versão do .NET utilzei no construtor primário, evitando boilerplate que a própria Microsoft recomenda.
**5 - Event Source** Para realizar as atualizações do banco de dados Mongo, utilzei Eventos através do mediator para garantir consistência eventual entre os bancos.


Responda objetivamente:

1. **Adequação da arquitetura** — CQRS com banco relacional + NoSQL é adequado para um sistema de biblioteca comunitária? Justifique. **Em que cenário você NÃO usaria essa arquitetura?**
A aplicação desta arquitetura é voltada para sistemas mais robustos, complexos e sistemas que possui um grande Input/Output. Que necessite a integridade das transações (ACID) e baixa latência. Para sistemas que não necessite de performance em consultas é um bom caso para não se aplicar o CQRS.

2. **Consistência eventual** — sua sincronização é eventual. O que acontece se um usuário solicita empréstimo logo após uma alteração, e o NoSQL ainda não atualizou? Como você lida com essa janela de inconsistência?
Existem algumas formas de se lidar com a consistência eventual. Uma das maneiras é fazer com que o FrontEnd, ao ser solicitado o empréstimo, atualiza a tela para o livro emprestado. Caso a transação falhe, é enviado para o usuário uma mensagem de falha de empréstimo. Caso contrário, atualiza a tela do usuário com o empréstimo realizado. Ou utiliza o Sticky Read, que retorna o id ou objeto após o salvamento de dados no Postgres, sem ter a necessidade do FrontEnd realizar um consulta do Id no Mongo, uma vez que por alguma razão, ele não foi atualizado.


3. **Localização das regras** — onde você colocou cada regra de domínio (ex.: reduzir `QuantidadeDisponivel`, impedir devolução dupla) e por que nessa camada?
Na camada de Domínio, uma vez que se trata de uma regra de negócio.

4. **Concorrência** — como você garante que duas solicitações simultâneas do **último exemplar** não furam a regra de disponibilidade?
Para garantir que a solicitação poderia ser por Bloqueio Otimista. Adicionando um campo de controle e atualize-o quando solicitado. Caso outra pessoa tente o emprestimo, o registro já foi atualizado.

5. **Falha de sincronização** — se o evento que atualiza o NoSQL falhar, como você detecta, recupera e garante idempotência?
Existe um padrão chamado Outbox, onde no momento de gravar no Postgres, grava-se também em uma outra tabela que pode ser chamado de EventOutbox. E um background service pode realizar a leitura desta tabela e atualizá-la se houver sucesso. Outro modo seria uma DLQ (Dead Letter Queue).


6. **Uso de IA** — onde usou IA, o que **aceitou**, o que **rejeitou** do que ela sugeriu, e **por quê**.
Eu utilizei o Claude Code em conjunto com o framework OpenSpec. Com isso também criei uma SKILL e integrei com um MCP (Context7) para realizar o "double check" do contexto e dos prompts que eu havia solicitado.
Como este framework é voltado para o Spec Driven Development, eu realizei o processo em camdas. Especializando o Domínio por exemplo e a própria skill que eu criei orientou a IA a não ser verbosa e foram poucas coisas que rejeitei. A maioria eu aceitei que o claude code sugeriu.

7. **Trade-offs gerais** — liste as 3 decisões mais importantes que tomou, as alternativas que descartou e o motivo.
1 - Arquitetura - Implementar o CQRS envolve algumas decisões porque o próprio pattern não resolve em si todos os trade-offs da arquitetura ou disponibilidade que sistema exige. Tomar decisões que tipo Bloqueio para evitar a concorrência por exemplo.
2 - Consistência eventual - Optar por atualizar o FrontEnd, antes de esperar a atualização do banco de dados é um risco, pois pode envolver indisponibilidade do banco por exemplo e ao mesmo tempo a concorrência.
3 - Concorrência - É complicado tratar deste assunto, devido a importância da operação. Existe algumas formas para resolver este problema: Bloqueios otimistas, pessimistas ou constraints.