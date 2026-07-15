# OpenSpec Review

Objetivo

Revisar uma change OpenSpec antes da execução do Apply.

A revisão deve identificar incosistências, riscos e oportunidades de melhoria para reduzir retrabalho durante a implementação.

---

## Verificar Consistência

Analisar:

* proposal.md
* design.md
* tasks.md
* specs/
* CLAUDE.md

Validar se todos os documentos descrevem a mesma solução.

Identificar:

* Requisitos conflitantes
* Funcionalidades descritas em apenas um artefato
* Tarefas sem requisito associado
* Requisitos sem tarefa correspondente

---

## Verificar Escopo

Identificar:

* Escopo excessivo para uma única change
* Funcionalidades que deveriam ser divididas em novas changes
* Dependências entre funcionalidades

Sugerir possíveis quebras em capacidades menores.

---

## Verificar Arquitetura

Validar aderência ao CLAUDE.md.

Identificar:

* Violações arquiteturais
* Estruturas de pastas inconsistentes
* Componentes excessivamente grantes
* Acoplamento desnecessário
* Riscos para manutenção futura
* Verificar quebra dos princípios do S.O.L.I.D.
* Respeitar a Clean Architecture

---

## Verificar Implementação

Identificar:

* Dependências faltantes
* Serviços ausentes
* Entidades ausentes
* Casos de uso não contemplados
* Fluxos incompletos
* Sempre que possível preparar para Testes Unitários

Avaliar se a implementação pode ser realizada com as informações existentes.

---

## Verificar Banco de Dados

Identificar:

* Entidaes incompletas
* Relacionamentos ausentes
* Dados necessários não previstos
* Problemas potenciais de persistência

---

## Verificar Riscos

Classificar:

* Baixo risco
* Médio risco
* Alto risco

Apontar:

* Possíveis dificuldades de implementação
* Ambiguidades
* Decisões não documentadas

---

##  Material de Apoio

Utilizar:

* Utilizar sempre que possível o material de apoio ou a documentação, consultando o MCP Context7 para confrontar com o código gerado.

## Resultado Esperado

Apresentar:

### Pontos Positivos

Lista dos aspectos bem definidos.

### Problemas Encontrados

Descrição dos problemas identificados.

### Recomendações

Sugestões de melhoria antes da execução do Apply.

### Conclusão

Informar:

* Pronto para Apply
  ou
* Requer ajustes antes do Apply


