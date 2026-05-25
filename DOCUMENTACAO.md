# Documentação Técnica — Sistema de Clínica Médica

> Este documento descreve em profundidade a arquitetura, as decisões de design, a aplicação dos pilares da POO e a relação de cada componente com as especificações do trabalho prático da disciplina de Programação Orientada por Objetos — PUC Minas Betim.

---

## Sumário

1. [Visão Geral do Sistema](#1-visão-geral-do-sistema)
2. [Arquitetura em Camadas](#2-arquitetura-em-camadas)
3. [Camada de Modelos (Models)](#3-camada-de-modelos-models)
4. [Camada de Interfaces](#4-camada-de-interfaces)
5. [Camada de Repositórios (Repositories)](#5-camada-de-repositórios-repositories)
6. [Camada de Serviços (Services)](#6-camada-de-serviços-services)
7. [Camada de Interface com Usuário (UI)](#7-camada-de-interface-com-usuário-ui)
8. [Tratamento de Exceções](#8-tratamento-de-exceções)
9. [Persistência em JSON](#9-persistência-em-json)
10. [Os Quatro Pilares da POO no Projeto](#10-os-quatro-pilares-da-poo-no-projeto)
11. [Relação com as Especificações do Trabalho](#11-relação-com-as-especificações-do-trabalho)
12. [Fluxo de Dados Completo](#12-fluxo-de-dados-completo)
13. [Decisões de Design e Justificativas](#13-decisões-de-design-e-justificativas)

---

## 1. Visão Geral do Sistema

O sistema foi desenvolvido para gerenciar uma clínica médica, permitindo:

- Cadastrar e consultar **médicos** (com CRM e especialidade)
- Cadastrar e consultar **pacientes** (com CPF, convênio e data de nascimento)
- **Agendar, cancelar e concluir consultas**, respeitando regras de negócio rígidas
- Persistir todos os dados em arquivos **JSON** entre as execuções
- Interagir com o usuário por meio de um **menu interativo no console**

O sistema é inteiramente desenvolvido em **C# com .NET 10**, aplicando os quatro pilares da Orientação a Objetos em situações concretas e justificáveis — não como demonstração teórica, mas como consequência natural do design.

---

## 2. Arquitetura em Camadas

O projeto adota uma **arquitetura em camadas** (Layered Architecture), onde cada camada tem uma responsabilidade bem definida e só se comunica com a camada imediatamente abaixo dela:

```
┌─────────────────────────────────────┐
│           UI (MenuConsole)          │  ← Interage com o usuário
├─────────────────────────────────────┤
│       Services (AgendamentoService) │  ← Regras de negócio
├─────────────────────────────────────┤
│   Repositories (MedicoRepositorio,  │  ← Acesso e persistência de dados
│   PacienteRepositorio,              │
│   ConsultaRepositorio)              │
├─────────────────────────────────────┤
│    Models (Pessoa, Medico,          │  ← Entidades do domínio
│    Paciente, Consulta)              │
└─────────────────────────────────────┘
```

**Por que essa estrutura?**

Cada camada tem um único motivo para mudar (Princípio da Responsabilidade Única). Se a interface mudar de console para gráfica, apenas a camada UI precisa ser reescrita. Se o armazenamento mudar de JSON para banco de dados, apenas os repositórios precisam ser alterados. As regras de negócio e os modelos permanecem intactos.

Esse isolamento também facilita a evolução incremental prevista nas especificações: o trabalho exige que o sistema evolua de console para interface gráfica e de JSON para banco de dados — a arquitetura em camadas torna essas evoluções cirúrgicas, sem impacto nas demais partes.

---

## 3. Camada de Modelos (Models)

### 3.1 Pessoa (classe abstrata)

**Arquivo:** [ClinicaMedica/Models/Pessoa.cs](ClinicaMedica/Models/Pessoa.cs)

`Pessoa` é a classe base abstrata de toda a hierarquia de pessoas no sistema. Ela encapsula os atributos comuns a qualquer pessoa: `Id`, `Nome`, `Cpf` e `Telefone`.

```
Pessoa (abstract)
├── Medico
└── Paciente
```

**Por que abstrata?**

Porque nunca faz sentido instanciar uma "pessoa genérica" no contexto deste sistema. Toda pessoa é ou um médico ou um paciente. A abstração garante que a classe não seja instanciada diretamente (o compilador impede isso), forçando o uso das subclasses concretas.

**Validações internas:**

- `Nome` não pode ser vazio (validado no setter via property)
- `Cpf` deve ter exatamente 11 dígitos numéricos

Essas validações estão no setter das properties, não em métodos externos — isso é encapsulamento: o objeto é responsável pela própria integridade.

**Método abstrato `ExibirInformacoes()`:**

Cada subclasse implementa sua própria versão desse método, que é o contrato que garante polimorfismo: a UI pode chamar `pessoa.ExibirInformacoes()` sem saber se a pessoa é um médico ou paciente.

---

### 3.2 Medico

**Arquivo:** [ClinicaMedica/Models/Medico.cs](ClinicaMedica/Models/Medico.cs)

Herda de `Pessoa` e adiciona:

- `Crm`: identificador único do conselho médico (validado: não pode ser vazio)
- `Especialidade`: área de atuação (validada: não pode vazia)

O método `ExibirInformacoes()` é sobrescrito para exibir CRM e especialidade além dos dados básicos.

---

### 3.3 Paciente

**Arquivo:** [ClinicaMedica/Models/Paciente.cs](ClinicaMedica/Models/Paciente.cs)

Herda de `Pessoa` e adiciona:

- `DataNascimento`: validada para não aceitar datas futuras nem anteriores a 1900
- `Convenio`: plano de saúde (valor padrão `"Particular"` para pacientes sem plano)
- `Idade`: **property computada** — calculada dinamicamente a partir da `DataNascimento`, sem ser armazenada. Isso evita inconsistência: a idade nunca "envelhece errado" porque nunca é guardada, sempre calculada no momento da leitura.

---

### 3.4 Consulta

**Arquivo:** [ClinicaMedica/Models/Consulta.cs](ClinicaMedica/Models/Consulta.cs)

Entidade que representa o agendamento. Contém:

- `Id`, `MedicoId`, `PacienteId`, `DataHora`, `Observacoes`
- `Status`: enum com três valores — `Agendada`, `Realizada`, `Cancelada`

**Por que usar enum para status?**

Enum elimina strings mágicas ("cancelada", "CANCELADA", "Cancelada") que causariam bugs de comparação. O compilador garante que apenas valores válidos sejam atribuídos.

**Métodos de transição de estado:**

```csharp
public void Cancelar()        // Agendada → Cancelada
public void MarcarRealizada() // Agendada → Realizada
```

Esses métodos validam a transição: não é possível cancelar uma consulta já realizada, por exemplo. A lógica de "o que pode mudar e quando" fica dentro da própria entidade — isso é encapsulamento de comportamento, não só de dados.

---

## 4. Camada de Interfaces

**Arquivo:** [ClinicaMedica/Interfaces/IRepositorio.cs](ClinicaMedica/Interfaces/IRepositorio.cs)

```csharp
public interface IRepositorio<T>
{
    void Adicionar(T entidade);
    T? BuscarPorId(int id);
    IReadOnlyList<T> ListarTodos();
    void Atualizar(T entidade);
    void Remover(int id);
    void Salvar();
}
```

**Por que uma interface genérica?**

A interface define um **contrato**: qualquer repositório que a implemente garante suporte às operações básicas de CRUD. O parâmetro genérico `<T>` permite que a mesma interface sirva para `Medico`, `Paciente` e `Consulta` sem duplicação de código.

Isso também é o que permite **inversão de dependência** no futuro: a `AgendamentoService` poderia receber `IRepositorio<Medico>` em vez de `MedicoRepositorio` diretamente, tornando fácil substituir a implementação concreta (ex.: trocar JSON por banco de dados) sem alterar a lógica de negócio.

---

## 5. Camada de Repositórios (Repositories)

### 5.1 RepositorioJson (classe abstrata base)

**Arquivo:** [ClinicaMedica/Repositories/RepositorioJson.cs](ClinicaMedica/Repositories/RepositorioJson.cs)

Esta é a peça central da persistência. `RepositorioJson<T>` é uma classe abstrata que implementa `IRepositorio<T>` e fornece a infraestrutura de leitura e escrita em JSON para qualquer tipo de entidade.

**Como funciona internamente:**

- Mantém uma `List<T>` em memória como cache dos dados
- No construtor, chama `Carregar()`, que lê o arquivo JSON do disco e deserializa para a lista
- `Salvar()` serializa a lista de volta para o arquivo JSON com indentação legível
- O caminho dos arquivos é resolvido em `Program.cs` via `[CallerFilePath]`, apontando sempre para o diretório do código-fonte

**Opções de serialização configuradas:**

```csharp
new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,   // Tolera variações de capitalização no JSON
    WriteIndented = true,                 // JSON legível por humanos
    Converters = { new JsonStringEnumConverter() } // Enum gravado como texto ("Agendada"), não número
}
```

Gravar o enum como texto (ex.: `"Status": "Agendada"`) é uma decisão intencional: se alguém abrir o arquivo JSON para depurar ou editar manualmente, o valor é compreensível. Se fosse gravado como `0`, `1`, `2`, seria opaco.

**Por que abstrata e não concreta?**

Porque há comportamentos que variam por tipo de entidade — principalmente `Adicionar()`, que precisa auto-incrementar o `Id` de forma específica para cada tipo. A classe base fornece o mecanismo de persistência; as subclasses especializam os comportamentos de domínio.

---

### 5.2 MedicoRepositorio

**Arquivo:** [ClinicaMedica/Repositories/MedicoRepositorio.cs](ClinicaMedica/Repositories/MedicoRepositorio.cs)

Herda de `RepositorioJson<Medico>` e adiciona:

- **`Adicionar()`** sobrescrito: calcula o próximo `Id` como `Max(ids) + 1`, evitando IDs duplicados mesmo após remoções. Valida unicidade do CRM antes de persistir.
- **`BuscarPorCrm()`**: busca case-insensitive (CRM `"SP-12345"` == `"sp-12345"`)
- **`BuscarPorEspecialidade()`**: busca parcial — `"cardio"` encontra `"Cardiologia"`

---

### 5.3 PacienteRepositorio

**Arquivo:** [ClinicaMedica/Repositories/PacienteRepositorio.cs](ClinicaMedica/Repositories/PacienteRepositorio.cs)

Herda de `RepositorioJson<Paciente>` e adiciona:

- **`Adicionar()`** sobrescrito: auto-incrementa Id, valida unicidade do CPF
- **`BuscarPorCpf()`**: normaliza o CPF antes de comparar — remove pontos e traços (`123.456.789-00` → `12345678900`), evitando falsos negativos por formatação diferente

---

### 5.4 ConsultaRepositorio

**Arquivo:** [ClinicaMedica/Repositories/ConsultaRepositorio.cs](ClinicaMedica/Repositories/ConsultaRepositorio.cs)

O repositório mais rico em métodos de consulta:

- **`BuscarPorMedico(medicoId)`**: filtra consultas de um médico específico
- **`BuscarPorPaciente(pacienteId)`**: histórico do paciente
- **`BuscarPorData(data)`**: agenda do dia
- **`ContarConsultasAtivasMedicoNoDia(medicoId, data)`**: conta consultas não canceladas, usado para validar o limite de 10 por dia
- **`PacienteTemConsultaComMedicoNoDia(pacienteId, medicoId, data)`**: verifica conflito de agendamento (regra: um paciente por médico por dia)

Esses métodos existem aqui e não no Service porque são operações de **consulta de dados** — responsabilidade do repositório. O Service usa esses métodos para tomar decisões de negócio.

---

## 6. Camada de Serviços (Services)

**Arquivo:** [ClinicaMedica/Services/AgendamentoService.cs](ClinicaMedica/Services/AgendamentoService.cs)

Esta é a camada de **regras de negócio**. O `AgendamentoService` recebe os três repositórios via construtor (injeção de dependências manual) e coordena as operações que envolvem mais de uma entidade ou mais de uma validação.

### Método `Agendar()`

É o método mais importante do sistema. Executa as seguintes validações **em ordem**:

1. O médico existe? → lança `ArgumentException`
2. O paciente existe? → lança `ArgumentException`
3. A `DataHora` é no passado? → lança `ArgumentException`
4. O médico já tem 10 consultas ativas nesse dia? → lança `LimiteConsultasDiariasException`
5. O paciente já tem consulta com esse médico nesse dia? → lança `ConsultaConflitanteException`

Somente após todas as validações passarem, a consulta é criada e salva.

**Por que lançar exceções customizadas em vez de retornar bool?**

Porque retornar `bool` perde a informação do **motivo** da falha. Com exceções tipadas, a UI pode capturar `LimiteConsultasDiariasException` e exibir uma mensagem específica sobre o limite diário, e capturar `ConsultaConflitanteException` com uma mensagem diferente sobre conflito de horário. O fluxo de erro é tratado com precisão, sem cascata de `if` na UI.

### Métodos `Cancelar()` e `MarcarRealizada()`

Buscam a consulta por Id, delegam a transição de estado para o próprio objeto `Consulta` (que valida se a transição é permitida), e salvam.

### Métodos de listagem

`ListarPorMedico()`, `ListarPorPaciente()` e `ListarPorData()` delegam para o repositório e retornam resultados ordenados por `DataHora`.

---

## 7. Camada de Interface com Usuário (UI)

**Arquivo:** [ClinicaMedica/UI/MenuConsole.cs](ClinicaMedica/UI/MenuConsole.cs)

O `MenuConsole` é a única classe que interage diretamente com o usuário via `Console.ReadLine()` e `Console.WriteLine()`. Ela conhece o `AgendamentoService` e os repositórios, mas nunca contém lógica de negócio — apenas coleta dados, chama o serviço/repositório e exibe resultados.

**Estrutura de menus:**

```
Menu Principal
├── 1. Médicos
│   ├── Cadastrar médico
│   ├── Listar todos
│   ├── Buscar por CRM
│   └── Buscar por especialidade
├── 2. Pacientes
│   ├── Cadastrar paciente
│   ├── Listar todos
│   └── Buscar por CPF
└── 3. Consultas
    ├── Agendar consulta
    ├── Cancelar consulta
    ├── Marcar como realizada
    ├── Agenda do dia
    ├── Histórico do paciente
    └── Agenda do médico
```

**Tratamento de erros na UI:**

Cada operação é envolta em `try/catch`. Erros de negócio (`LimiteConsultasDiariasException`, `ConsultaConflitanteException`, `ArgumentException`) exibem mensagens amigáveis em vermelho. Isso separa claramente a **detecção** do erro (no Service) da **apresentação** do erro (na UI).

**Por que a UI não valida regras de negócio?**

Para garantir que as regras sejam verificadas independentemente de quem chame o serviço. Se no futuro uma interface gráfica ou uma API chamar `AgendamentoService.Agendar()`, as mesmas regras serão aplicadas — porque elas vivem no Service, não na UI.

---

## 8. Tratamento de Exceções

**Arquivos:**
- [ClinicaMedica/Exceptions/ConsultaConflitanteException.cs](ClinicaMedica/Exceptions/ConsultaConflitanteException.cs)
- [ClinicaMedica/Exceptions/LimiteConsultasDiariasException.cs](ClinicaMedica/Exceptions/LimiteConsultasDiariasException.cs)

O sistema define duas exceções customizadas, ambas herdando de `Exception`:

| Exceção | Quando é lançada |
|---|---|
| `ConsultaConflitanteException` | Paciente já tem consulta com o mesmo médico no mesmo dia |
| `LimiteConsultasDiariasException` | Médico já tem 10 consultas ativas no dia solicitado |

**Por que criar exceções customizadas?**

Exceções customizadas permitem que o código chamador trate cada tipo de erro de forma diferente, usando `catch` tipado. Além disso, o nome da exceção documenta o tipo de problema — `LimiteConsultasDiariasException` é autoexplicativa. Usar `Exception("limite atingido")` seria menos expressivo e menos robusto.

Isso atende diretamente ao requisito do trabalho de **inclusão de validações e tratamento de exceções**.

---

## 9. Persistência em JSON

Os dados são salvos em três arquivos no mesmo diretório do `Program.cs`, versionados junto ao código-fonte para facilitar revisão e testes:

```
ClinicaMedica/
├── Program.cs
├── medicos.json
├── pacientes.json
└── consultas.json
```

O caminho é resolvido em tempo de compilação via `[CallerFilePath]`, garantindo que os arquivos sempre sejam criados ao lado do código-fonte independente de onde o executável é rodado:

```csharp
static string GetSourceDir([CallerFilePath] string path = "") => path;
var dataDir = Path.GetDirectoryName(GetSourceDir())!;
```

**Exemplo de `medicos.json`:**

```json
[
  {
    "crm": "SP-12345",
    "especialidade": "Cardiologia",
    "id": 1,
    "nome": "Dr. João Silva",
    "cpf": "12345678901",
    "telefone": "(11) 99999-0001"
  }
]
```

**Estratégia de carregamento:**

No construtor do `RepositorioJson`, o arquivo é lido e deserializado. Se o arquivo não existir (primeira execução) ou estiver corrompido, a lista começa vazia — sem crash. Isso é tratado explicitamente com `try/catch` no método `Carregar()`.

**Por que JSON e não XML ou TXT?**

O trabalho permite os três formatos. JSON foi escolhido por ser mais compacto que XML, nativamente suportado por `System.Text.Json` (sem dependência externa), e legível por humanos. A estrutura hierárquica do JSON mapeia naturalmente para os objetos C#.

A especificação também prevê **evolução para banco de dados** — a arquitetura facilita isso: basta criar `MedicoRepositorioBD : IRepositorio<Medico>` implementando o mesmo contrato, sem alterar nada nas camadas acima.

---

## 10. Os Quatro Pilares da POO no Projeto

### Abstração

A abstração consiste em modelar apenas o que é relevante para o domínio, ignorando detalhes desnecessários.

**Onde aparece:**

- **`Pessoa`** é uma abstração das características comuns de médicos e pacientes. Ela não existe como entidade concreta no sistema — é um molde.
- **`IRepositorio<T>`** é uma abstração da ideia de "armazenamento de dados". A interface define _o que_ pode ser feito com um repositório, sem dizer _como_ é feito.
- **`RepositorioJson<T>`** é uma abstração intermediária: define o comportamento de persistência em JSON, mas deixa os detalhes de cada entidade (como gerar Id, como validar unicidade) para as subclasses.

**Por que isso importa:**

A UI não sabe como os médicos são salvos. O Service não sabe se os dados vêm de JSON ou de um banco. Cada camada trabalha com abstrações, não com implementações concretas.

---

### Encapsulamento

O encapsulamento protege o estado interno dos objetos, expondo apenas o necessário.

**Onde aparece:**

- **Properties com validação em `Pessoa`**: `Nome` e `Cpf` têm setters que lançam `ArgumentException` se valores inválidos forem atribuídos. O objeto nunca entra em estado inválido.
- **`Idade` em `Paciente`**: property computada com getter apenas — não pode ser definida externamente, pois é calculada automaticamente.
- **`Status` em `Consulta`**: não tem setter público. O status só muda por meio dos métodos `Cancelar()` e `MarcarRealizada()`, que validam a transição. Isso impede que código externo coloque a consulta em um estado ilegal.
- **`_itens` em `RepositorioJson`**: a lista interna é privada. O acesso externo é feito apenas por `ListarTodos()`, que retorna `IReadOnlyList<T>` — uma visão somente leitura. Isso impede que código fora do repositório modifique a coleção diretamente.

---

### Herança

A herança permite que subclasses reutilizem e especializem comportamentos da superclasse.

**Hierarquia de Pessoas:**

```
Pessoa (abstract)
├── Medico       → adiciona CRM, Especialidade, sobrescreve ExibirInformacoes()
└── Paciente     → adiciona DataNascimento, Convenio, Idade, sobrescreve ExibirInformacoes()
```

**Hierarquia de Repositórios:**

```
IRepositorio<T> (interface)
└── RepositorioJson<T> (abstract) → implementa persistência JSON
    ├── MedicoRepositorio    → especializa Adicionar(), adiciona BuscarPorCrm/Especialidade
    ├── PacienteRepositorio  → especializa Adicionar(), adiciona BuscarPorCpf
    └── ConsultaRepositorio  → especializa Adicionar(), adiciona métodos de busca por filtros
```

**O que cada subclasse herda e o que especializa:**

`MedicoRepositorio`, `PacienteRepositorio` e `ConsultaRepositorio` herdam de `RepositorioJson<T>` toda a lógica de leitura/escrita de JSON, a manutenção da lista em memória e os métodos básicos de CRUD. Cada um sobrescreve apenas `Adicionar()` para implementar a geração de Id e as validações específicas do tipo.

---

### Polimorfismo

O polimorfismo permite que objetos de tipos diferentes respondam de forma específica a chamadas feitas por meio de uma referência comum.

**Polimorfismo com `Pessoa`:**

```csharp
Pessoa p1 = new Medico { Nome = "Dr. Ana", Crm = "MG-001", Especialidade = "Neurologia" };
Pessoa p2 = new Paciente { Nome = "Carlos", DataNascimento = new DateTime(1990, 5, 10) };

p1.ExibirInformacoes(); // exibe CRM e especialidade
p2.ExibirInformacoes(); // exibe data de nascimento e convênio
```

A chamada é feita na referência `Pessoa`, mas o comportamento executado é o da subclasse concreta. Isso permite que a UI itere sobre uma lista de `Pessoa` e chame `ExibirInformacoes()` sem `if (p is Medico)`.

**Polimorfismo com `IRepositorio<T>`:**

A interface define o contrato. O `AgendamentoService` poderia receber `IRepositorio<Medico>` e funcionar igualmente com `MedicoRepositorioJson`, `MedicoRepositorioXml` ou `MedicoRepositorioBD` — sem mudar uma linha do Service.

**Polimorfismo com `Consulta`:**

Os métodos `Cancelar()` e `MarcarRealizada()` têm comportamento polimórfico implícito via estado: a mesma chamada `consulta.Cancelar()` valida e executa a transição de forma diferente dependendo do status atual da consulta.

---

## 11. Relação com as Especificações do Trabalho

### Requisitos Funcionais (Tema 6 — Clínica Médica)

| Requisito | Implementação |
|---|---|
| Cadastro de médicos e pacientes | `MedicoRepositorio.Adicionar()`, `PacienteRepositorio.Adicionar()` |
| Agendamento de consultas | `AgendamentoService.Agendar()` |
| Cancelamento de consultas | `AgendamentoService.Cancelar()` |
| Validação de horários | Verificação de `DataHora < DateTime.Now` em `Agendar()` |
| Listagem por médico | `AgendamentoService.ListarPorMedico()` |
| Histórico por paciente | `AgendamentoService.ListarPorPaciente()` |

### Regras de Negócio

| Regra | Onde é verificada |
|---|---|
| Um paciente por médico/dia | `ConsultaRepositorio.PacienteTemConsultaComMedicoNoDia()` → lança `ConsultaConflitanteException` |
| Máximo 10 consultas por médico/dia | `ConsultaRepositorio.ContarConsultasAtivasMedicoNoDia()` → lança `LimiteConsultasDiariasException` |
| Consultas não podem ser no passado | Validação em `AgendamentoService.Agendar()` |

### Requisitos Adicionais do Trabalho

| Requisito | Status |
|---|---|
| Menu interativo no console | Implementado em `MenuConsole.cs` |
| Armazenamento em JSON | Implementado em `RepositorioJson<T>` |
| Validações e tratamento de exceções | Exceções customizadas + validações em models e services |
| Aplicação dos quatro pilares da POO | Abstração, Encapsulamento, Herança e Polimorfismo aplicados concretamente |
| README.md com instruções | Presente em `README.md` |

### Extensões já preparadas pela arquitetura

O trabalho menciona como extensões possíveis: filtro por especialidade, listagem diária e histórico. Todos estão implementados:

- `MedicoRepositorio.BuscarPorEspecialidade()` — filtro por especialidade
- `AgendamentoService.ListarPorData()` — agenda do dia
- `AgendamentoService.ListarPorPaciente()` — histórico do paciente

---

## 12. Fluxo de Dados Completo

### Exemplo: Agendamento de uma consulta

```
Usuário digita: médico Id=1, paciente Id=2, data 2026-06-10 14:00
        │
        ▼
MenuConsole.AgendarConsulta()
  → coleta MedicoId, PacienteId, DataHora do Console.ReadLine()
  → chama AgendamentoService.Agendar(1, 2, dataHora)
        │
        ▼
AgendamentoService.Agendar()
  → MedicoRepositorio.BuscarPorId(1)          → OK: médico existe
  → PacienteRepositorio.BuscarPorId(2)        → OK: paciente existe
  → DataHora > DateTime.Now                   → OK: não é no passado
  → ConsultaRepositorio.ContarConsultasAtivas(1, 2026-06-10) → retorna 3 (< 10, OK)
  → ConsultaRepositorio.PacienteTemConsulta(2, 1, 2026-06-10) → retorna false (OK)
  → cria new Consulta { MedicoId=1, PacienteId=2, DataHora=... }
  → ConsultaRepositorio.Adicionar(consulta)
  → ConsultaRepositorio.Salvar()   → escreve ClinicaMedica/consultas.json
        │
        ▼
MenuConsole exibe: "Consulta agendada com sucesso! ID: 4"
```

### Exemplo: Falha por limite diário

```
AgendamentoService.Agendar()
  → ConsultaRepositorio.ContarConsultasAtivas(1, 2026-06-10) → retorna 10
  → throw LimiteConsultasDiariasException("Médico atingiu o limite de 10 consultas")
        │
        ▼
MenuConsole.catch(LimiteConsultasDiariasException e)
  → Console.WriteLine em vermelho: "Limite diário atingido: ..."
```

---

## 13. Decisões de Design e Justificativas

### Por que `IReadOnlyList<T>` em `ListarTodos()`?

Retornar `List<T>` diretamente permitiria que código externo modificasse a coleção interna sem passar pelo repositório (ex.: `repo.ListarTodos().Add(x)` sem chamar `Salvar()`). `IReadOnlyList<T>` expõe apenas operações de leitura, protegendo a integridade do estado.

### Por que o Id é gerado no repositório e não no model?

Porque a geração de Id depende de contexto (o maior Id existente na coleção). O model `Medico` não tem acesso à lista de médicos — seria uma violação de responsabilidade. O repositório tem esse contexto e é o lugar correto.

### Por que `AgendamentoService` recebe repositórios no construtor?

Isso é **injeção de dependência**: o serviço declara o que precisa, e quem o cria (`Program.cs`) fornece as implementações concretas. Isso facilita testes (seria possível injetar repositórios em memória) e a futura substituição de implementações.

### Por que separar `ConsultaRepositorio` e `AgendamentoService`?

O repositório sabe **como acessar dados**. O serviço sabe **quais regras de negócio aplicar**. Misturar os dois criaria uma classe com dois motivos para mudar: mudança na persistência e mudança nas regras de negócio. Mantê-los separados torna o sistema mais previsível e fácil de evoluir.

### Por que o método `Cancelar()` está em `Consulta` e a validação de existência está em `AgendamentoService`?

`Consulta.Cancelar()` valida a transição de estado (não cancelar o que já foi realizado) — é um comportamento intrínseco do objeto. `AgendamentoService.Cancelar()` valida que a consulta existe — é uma operação que envolve o repositório. Cada responsabilidade está no lugar certo.

---

*Documentação gerada em 25/05/2026 — Sistema de Clínica Médica, PUC Minas Betim.*
