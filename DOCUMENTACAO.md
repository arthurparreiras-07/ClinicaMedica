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
9. [Persistência de Dados](#9-persistência-de-dados)
10. [Os Quatro Pilares da POO no Projeto](#10-os-quatro-pilares-da-poo-no-projeto)
11. [Relação com as Especificações do Trabalho](#11-relação-com-as-especificações-do-trabalho)
12. [Fluxo de Dados Completo](#12-fluxo-de-dados-completo)
13. [Decisões de Design e Justificativas](#13-decisões-de-design-e-justificativas)

---

## 1. Visão Geral do Sistema

O sistema foi desenvolvido para gerenciar uma clínica médica, permitindo:

- Cadastrar e consultar **médicos** (com CRM e especialidade)
- Cadastrar e consultar **pacientes** (com CPF validado, convênio e data de nascimento)
- **Agendar, cancelar e concluir consultas**, respeitando três regras de negócio rígidas
- **Registrar diagnósticos e prescrições** médicas em consultas realizadas
- Consultar a **agenda diária**, o histórico por médico e o prontuário por paciente
- Filtrar médicos por **especialidade** no momento do agendamento
- Persistir todos os dados em **banco de dados SQLite** (padrão) ou em **arquivos JSON** (modo alternativo)
- Interagir com o usuário por meio de um **menu interativo no console**

O sistema é inteiramente desenvolvido em **C# com .NET 10**, aplicando os quatro pilares da Orientação a Objetos em situações concretas e justificáveis — não como demonstração teórica, mas como consequência natural do design.

---

## 2. Arquitetura em Camadas

O projeto adota uma **arquitetura em camadas** (Layered Architecture), onde cada camada tem uma responsabilidade bem definida e só se comunica com a camada imediatamente abaixo dela:

```
┌─────────────────────────────────────────────┐
│  UI (MenuConsole + submenus especializados)  │  ← Interage com o usuário
├─────────────────────────────────────────────┤
│       Services (AgendamentoService)          │  ← Regras de negócio
├─────────────────────────────────────────────┤
│  Repositories (JSON ou SQLite, via contrato) │  ← Acesso e persistência de dados
├─────────────────────────────────────────────┤
│    Models (Pessoa, Medico, Paciente,         │  ← Entidades do domínio
│    Consulta, Prescricao)                     │
└─────────────────────────────────────────────┘
```

**Por que essa estrutura?**

Cada camada tem um único motivo para mudar (Princípio da Responsabilidade Única). Se a interface mudar de console para gráfica, apenas a camada UI precisa ser reescrita. Se o armazenamento mudar de SQLite para outro banco, apenas os repositórios precisam ser alterados. As regras de negócio e os modelos permanecem intactos.

Esse isolamento também facilita a evolução incremental prevista nas especificações: o trabalho exige que o sistema evolua de console para interface gráfica — a arquitetura em camadas torna essa evolução cirúrgica, sem impacto nas demais partes.

---

## 3. Camada de Modelos (Models)

### 3.1 Pessoa (classe abstrata)

**Arquivo:** [App/Backend/Core/Models/Pessoa.cs](App/Backend/Core/Models/Pessoa.cs)

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
- `Cpf` deve ter exatamente 11 dígitos numéricos **e** passar pelo algoritmo dos dois dígitos verificadores

A validação do CPF vai além de verificar o comprimento: ela rejeita sequências repetidas (como `000.000.000-00`) e calcula os dois dígitos verificadores segundo o algoritmo oficial da Receita Federal. Isso está encapsulado em `ValidarDigitosCpf()`, método `private static` dentro de `Pessoa`.

```csharp
public string Cpf
{
    get => _cpf;
    set
    {
        var cpfLimpo = value?.Replace(".", "").Replace("-", "").Trim() ?? "";
        if (cpfLimpo.Length != 11 || !cpfLimpo.All(char.IsDigit))
            throw new ArgumentException("CPF inválido. Informe 11 dígitos numéricos.");
        if (!ValidarDigitosCpf(cpfLimpo))
            throw new ArgumentException("CPF inválido. Os dígitos verificadores não conferem.");
        _cpf = cpfLimpo;
    }
}
```

**Método abstrato `ExibirInformacoes()`:**

Cada subclasse implementa sua própria versão desse método, que é o contrato que garante polimorfismo: a UI pode chamar `pessoa.ExibirInformacoes()` sem saber se a pessoa é um médico ou paciente.

---

### 3.2 Medico

**Arquivo:** [App/Backend/Medicos/Models/Medico.cs](App/Backend/Medicos/Models/Medico.cs)

Herda de `Pessoa` e adiciona:

- `Crm`: identificador único do conselho médico (validado: não pode ser vazio; normalizado para maiúsculas)
- `Especialidade`: área de atuação (validada: não pode ser vazia)

O método `ExibirInformacoes()` é sobrescrito para exibir CRM e especialidade além dos dados básicos:

```csharp
public override string ExibirInformacoes() =>
    $"Dr(a). {Nome} | CRM: {Crm} | {Especialidade} | Tel: {Telefone}";
```

---

### 3.3 Paciente

**Arquivo:** [App/Backend/Pacientes/Models/Paciente.cs](App/Backend/Pacientes/Models/Paciente.cs)

Herda de `Pessoa` e adiciona:

- `DataNascimento`: validada para não aceitar datas futuras nem anteriores a 1900
- `Convenio`: plano de saúde (valor padrão `"Particular"` para pacientes sem plano)
- `Idade`: **property computada** — calculada dinamicamente a partir da `DataNascimento`, sem ser armazenada. Isso evita inconsistência: a idade nunca "envelhece errado" porque nunca é guardada, sempre calculada no momento da leitura.

---

### 3.4 Consulta

**Arquivo:** [App/Backend/Consultas/Models/Consulta.cs](App/Backend/Consultas/Models/Consulta.cs)

Entidade que representa o agendamento. Contém:

- `Id`, `MedicoId`, `PacienteId`, `DataHora`, `Observacoes`
- `Status`: enum `StatusConsulta` com três valores — `Agendada`, `Realizada`, `Cancelada`
- `Diagnostico`: texto registrado após a consulta ser marcada como realizada
- `Prescricoes`: lista de medicamentos prescritos

**Backing fields e propriedades somente leitura:**

Os campos `_status`, `_diagnostico` e `_prescricoes` são privados. Externamente ao objeto, `Status`, `Diagnostico` e `Prescricoes` são acessíveis apenas para leitura:

```csharp
public StatusConsulta Status => _status;
public string Diagnostico => _diagnostico;
public IReadOnlyList<Prescricao> Prescricoes => _prescricoes;
```

Isso garante que o estado da consulta só mude por meio dos métodos que validam as transições.

**Métodos de transição de estado:**

```csharp
public void Cancelar()           // Agendada → Cancelada (impede cancelar Realizada)
public void MarcarRealizada()    // Agendada → Realizada (impede realizar Cancelada)
public void RegistrarDiagnostico(string diagnostico)  // exige Status == Realizada
public void AdicionarPrescricao(Prescricao prescricao) // exige Status == Realizada
```

Cada método valida a transição e escreve diretamente no backing field — sem passar pelo setter público (que não existe).

**Construtores especializados:**

`Consulta` tem três construtores:
1. Parameterless (`public Consulta()`) — exigido pela serialização JSON e pelo SQLite.
2. Construtor de criação (`Consulta(id, medicoId, pacienteId, dataHora, observacoes)`) — usado pelo `AgendamentoService` ao agendar. Passa `DataHora` pelo setter, que valida a data no passado.
3. Construtor de hidratação (`[JsonConstructor] Consulta(id, medicoId, pacienteId, dataHora, observacoes, diagnostico, status, prescricoes)`) — usado pelo deserializador JSON e pelo repositório SQLite. Define `_dataHora` diretamente, ignorando a validação de data passada para dados históricos.

O atributo `[JsonConstructor]` instrui o `System.Text.Json` a usar o terceiro construtor, evitando que consultas salvas com datas no passado causem exceção na leitura.

**`DefinirPrescricoes()` (internal):**

O repositório SQLite armazena prescrições em tabela separada (`Prescricoes`) e as carrega depois de hidratar o objeto `Consulta`. O método `internal void DefinirPrescricoes(IEnumerable<Prescricao>)` permite esse preenchimento posterior sem expor a coleção publicamente:

```csharp
internal void DefinirPrescricoes(IEnumerable<Prescricao> prescricoes)
{
    _prescricoes.Clear();
    _prescricoes.AddRange(prescricoes);
}
```

A visibilidade `internal` restringe o acesso ao assembly — apenas os repositórios (que estão no mesmo projeto) podem chamar esse método.

---

### 3.5 Prescricao

**Arquivo:** [App/Backend/Consultas/Models/Prescricao.cs](App/Backend/Consultas/Models/Prescricao.cs)

Representa um medicamento prescrito em uma consulta realizada. Contém:

- `Medicamento`: nome do remédio
- `Dosagem`: dose e frequência
- `Instrucoes`: orientações adicionais (opcional)

`Prescricao` é um **value object** simples: não tem Id próprio nem métodos de negócio. Sua existência é subordinada a `Consulta` — uma composição.

```csharp
public override string ToString() =>
    string.IsNullOrEmpty(Instrucoes)
        ? $"{Medicamento} — {Dosagem}"
        : $"{Medicamento} — {Dosagem} ({Instrucoes})";
```

---

## 4. Camada de Interfaces

**Arquivo:** [App/Backend/Core/Repositories/IRepositorio.cs](App/Backend/Core/Repositories/IRepositorio.cs)

```csharp
public interface IRepositorio<T>
{
    void Adicionar(T entidade);
    T? BuscarPorId(int id);
    IEnumerable<T> ListarTodos();
    void Atualizar(T entidade);
    void Remover(int id);
    void Salvar();
}
```

**Por que uma interface genérica?**

A interface define um **contrato**: qualquer repositório que a implemente garante suporte às operações básicas de CRUD. O parâmetro genérico `<T>` permite que a mesma interface sirva para `Medico`, `Paciente` e `Consulta` sem duplicação de código.

Isso também permite **inversão de dependência**: `AgendamentoService` recebe `IConsultaRepositorio`, `IMedicoRepositorio` e `IPacienteRepositorio` via construtor — nunca as classes concretas. Trocar JSON por SQLite ou vice-versa exige apenas mudar o `Program.cs`.

**Interfaces especializadas:**

Cada entidade tem sua própria interface que estende `IRepositorio<T>` com buscas específicas:

| Interface | Arquivo | Métodos adicionais |
|---|---|---|
| `IMedicoRepositorio` | [App/Backend/Medicos/Interfaces/IMedicoRepositorio.cs](App/Backend/Medicos/Interfaces/IMedicoRepositorio.cs) | `BuscarPorCrm`, `BuscarPorEspecialidade` |
| `IPacienteRepositorio` | [App/Backend/Pacientes/Interfaces/IPacienteRepositorio.cs](App/Backend/Pacientes/Interfaces/IPacienteRepositorio.cs) | `BuscarPorCpf` |
| `IConsultaRepositorio` | [App/Backend/Consultas/Interfaces/IConsultaRepositorio.cs](App/Backend/Consultas/Interfaces/IConsultaRepositorio.cs) | `BuscarPorMedico`, `BuscarPorPaciente`, `BuscarPorData`, `ContarConsultasAtivasMedicoNoDia`, `PacienteTemConsultaComMedicoNoDia` |

---

## 5. Camada de Repositórios (Repositories)

O projeto oferece **duas implementações de repositório** para cada entidade: uma baseada em JSON e outra em SQLite. Ambas implementam as mesmas interfaces (`IMedicoRepositorio`, `IPacienteRepositorio`, `IConsultaRepositorio`) e são intercambiáveis sem alterar nada nas camadas acima.

### 5.1 RepositorioJson (classe abstrata base — JSON)

**Arquivo:** [App/Database/Json/RepositorioJson.cs](App/Database/Json/RepositorioJson.cs)

`RepositorioJson<T>` implementa `IRepositorio<T>` e fornece a infraestrutura de leitura e escrita em JSON para qualquer tipo de entidade.

**Como funciona:**

- Mantém uma `List<T> _dados` em memória como cache dos dados
- No construtor, chama `Carregar()`, que lê o arquivo JSON do disco e deserializa para a lista
- `Salvar()` serializa `_dados` de volta para o arquivo JSON com indentação legível
- Se o arquivo não existir ou estiver corrompido, a lista começa vazia — sem crash

**Opções de serialização configuradas:**

```csharp
new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
}
```

Gravar o enum como texto (ex.: `"Status": "Agendada"`) é uma decisão intencional: o arquivo JSON permanece legível por humanos. Se fosse gravado como número (`0`, `1`, `2`), seria opaco para depuração manual.

**Por que abstrata e não concreta?**

Porque a lógica de auto-incremento de Id e as validações de unicidade variam por tipo de entidade. A classe base fornece o mecanismo de persistência; as subclasses especializam os comportamentos de domínio.

---

### 5.2 MedicoRepositorio (JSON)

**Arquivo:** [App/Backend/Medicos/Repositories/MedicoRepositorio.cs](App/Backend/Medicos/Repositories/MedicoRepositorio.cs)

Herda de `RepositorioJson<Medico>` e adiciona:

- **`Adicionar()`**: calcula o próximo `Id` como `Max(ids) + 1`. Valida unicidade do CRM antes de persistir, lançando `InvalidOperationException` em caso de duplicata.
- **`BuscarPorCrm()`**: busca com normalização (`.Trim().ToUpper()`) — CRM `"sp-12345"` encontra `"SP-12345"`.
- **`BuscarPorEspecialidade()`**: busca parcial case-insensitive — `"cardio"` encontra `"Cardiologia"`.

---

### 5.3 PacienteRepositorio (JSON)

**Arquivo:** [App/Backend/Pacientes/Repositories/PacienteRepositorio.cs](App/Backend/Pacientes/Repositories/PacienteRepositorio.cs)

Herda de `RepositorioJson<Paciente>` e adiciona:

- **`Adicionar()`**: auto-incrementa Id, valida unicidade do CPF.
- **`BuscarPorCpf()`**: normaliza o CPF antes de comparar (remove pontos e traços) — evita falsos negativos por formatação diferente.

---

### 5.4 ConsultaRepositorio (JSON)

**Arquivo:** [App/Backend/Consultas/Repositories/ConsultaRepositorio.cs](App/Backend/Consultas/Repositories/ConsultaRepositorio.cs)

O repositório mais rico em métodos de consulta:

- **`BuscarPorMedico(medicoId)`**: filtra consultas de um médico específico
- **`BuscarPorPaciente(pacienteId)`**: histórico do paciente
- **`BuscarPorData(data)`**: agenda do dia (comparação por `.Date`, ignorando hora)
- **`ContarConsultasAtivasMedicoNoDia(medicoId, data)`**: conta consultas não canceladas, usado para validar o limite de 10 por dia
- **`PacienteTemConsultaComMedicoNoDia(medicoId, pacienteId, data)`**: verifica conflito de agendamento

---

### 5.5 RepositorioBD (classe abstrata base — SQLite)

**Arquivo:** [App/Database/Sqlite/RepositorioBD.cs](App/Database/Sqlite/RepositorioBD.cs)

`RepositorioBD<T>` é o equivalente SQLite de `RepositorioJson<T>`. Implementa `IRepositorio<T>` e fornece:

- `ConexaoBanco _banco`: objeto que gerencia a string de conexão e abre conexões (`CriarConexao()`)
- `Salvar()`: no-op — SQLite persiste imediatamente em cada operação, sem cache em memória
- `ObterUltimoId()`: helper que lê `last_insert_rowid()` após um INSERT

**ConexaoBanco:**

**Arquivo:** [App/Database/Sqlite/ConexaoBanco.cs](App/Database/Sqlite/ConexaoBanco.cs)

Encapsula a string de conexão e abre cada conexão com `PRAGMA foreign_keys = ON`, garantindo que as restrições de chave estrangeira sejam respeitadas (no SQLite, foreign keys são opt-in).

**InicializadorBanco:**

**Arquivo:** [App/Database/Sqlite/InicializadorBanco.cs](App/Database/Sqlite/InicializadorBanco.cs)

Executa o DDL idempotente (`CREATE TABLE IF NOT EXISTS`) na inicialização do sistema, criando as quatro tabelas — `Medicos`, `Pacientes`, `Consultas`, `Prescricoes` — se ainda não existirem. A tabela `Prescricoes` referencia `Consultas` com `ON DELETE CASCADE`, garantindo que ao remover uma consulta, suas prescrições sejam removidas automaticamente.

---

### 5.6 Repositórios SQLite concretos

**Arquivos:**
- [App/Backend/Medicos/Repositories/MedicoRepositorioSql.cs](App/Backend/Medicos/Repositories/MedicoRepositorioSql.cs)
- [App/Backend/Pacientes/Repositories/PacienteRepositorioSql.cs](App/Backend/Pacientes/Repositories/PacienteRepositorioSql.cs)
- [App/Backend/Consultas/Repositories/ConsultaRepositorioSql.cs](App/Backend/Consultas/Repositories/ConsultaRepositorioSql.cs)

Cada um herda de `RepositorioBD<T>` e implementa as operações CRUD via queries SQL parametrizadas (sem concatenação de strings — sem risco de SQL Injection).

`ConsultaRepositorioSql` tem cuidado especial com `Prescricoes`: como estão em tabela separada, após carregar cada `Consulta` o repositório chama `consulta.DefinirPrescricoes(CarregarPrescricoes(conn, consulta.Id))` para popular a coleção interna sem expor um setter público.

---

## 6. Camada de Serviços (Services)

**Arquivo:** [App/Backend/Consultas/Services/AgendamentoService.cs](App/Backend/Consultas/Services/AgendamentoService.cs)

Esta é a camada de **regras de negócio**. O `AgendamentoService` recebe os três repositórios via construtor (injeção de dependências) e coordena as operações que envolvem mais de uma entidade ou mais de uma validação.

### Método `Agendar()`

É o método mais importante do sistema. Executa as seguintes validações **em ordem**:

1. O médico existe? → lança `KeyNotFoundException`
2. O paciente existe? → lança `KeyNotFoundException`
3. O médico já tem 10 consultas ativas nesse dia? → lança `LimiteConsultasDiariasException`
4. O paciente já tem consulta com esse médico nesse dia? → lança `ConsultaConflitanteException`

Somente após todas as validações passarem, a consulta é criada e salva. A validação de data passada é feita pelo setter `DataHora` no construtor de `Consulta`.

**Por que lançar exceções customizadas em vez de retornar bool?**

Porque retornar `bool` perde a informação do **motivo** da falha. Com exceções tipadas, a UI captura `LimiteConsultasDiariasException` com uma mensagem específica sobre o limite diário, e `ConsultaConflitanteException` com uma mensagem diferente sobre conflito. O fluxo de erro é tratado com precisão, sem cascata de `if` na UI.

### Métodos `Cancelar()` e `MarcarRealizada()`

Buscam a consulta por Id, delegam a transição de estado para o próprio objeto `Consulta` (que valida se a transição é permitida) e salvam.

### Métodos de prontuário

```csharp
public void RegistrarDiagnostico(int consultaId, string diagnostico)
public void AdicionarPrescricao(int consultaId, Prescricao prescricao)
```

Ambos buscam a consulta, delegam para o método correspondente na entidade (que valida o status) e persistem.

### Métodos de listagem

`ListarPorMedico()`, `ListarPorPaciente()`, `ListarPorData()` e `ListarTodas()` delegam para o repositório e retornam resultados ordenados por `DataHora`.

---

## 7. Camada de Interface com Usuário (UI)

A UI é organizada em quatro classes, todas no namespace `ClinicaMedica.UI.TextUI`:

| Classe | Arquivo | Responsabilidade |
|---|---|---|
| `MenuConsole` | [App/UI/TextUI/MenuConsole.cs](App/UI/TextUI/MenuConsole.cs) | Menu principal; instancia os submenus e despacha para eles |
| `MedicosMenu` | [App/UI/TextUI/Medicos/MedicosMenu.cs](App/UI/TextUI/Medicos/MedicosMenu.cs) | Submenu de médicos (cadastrar, listar, buscar por CRM, buscar por especialidade) |
| `PacientesMenu` | [App/UI/TextUI/Pacientes/PacientesMenu.cs](App/UI/TextUI/Pacientes/PacientesMenu.cs) | Submenu de pacientes (cadastrar, listar, buscar por CPF) |
| `ConsultasMenu` | [App/UI/TextUI/Consultas/ConsultasMenu.cs](App/UI/TextUI/Consultas/ConsultasMenu.cs) | Submenu de consultas (7 opções: agendar, cancelar, marcar realizada, agenda do dia, prontuário, agenda do médico, diagnóstico/prescrição) |
| `ConsoleHelper` | [App/UI/TextUI/Shared/ConsoleHelper.cs](App/UI/TextUI/Shared/ConsoleHelper.cs) | Utilitários: `Titulo`, `Ler`, `LerInt`, `Sucesso`, `Erro`, `Aviso`, `Pausar` |

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
    ├── 1. Agendar consulta (com filtro por especialidade)
    ├── 2. Cancelar consulta
    ├── 3. Marcar como realizada
    ├── 4. Consultas do dia
    ├── 5. Prontuário do paciente
    ├── 6. Agenda de um médico
    └── 7. Registrar diagnóstico/prescrição
```

**Tratamento de erros na UI:**

Cada operação é envolta em `try/catch`. Erros de negócio (`LimiteConsultasDiariasException`, `ConsultaConflitanteException`, `ArgumentException`, `KeyNotFoundException`) exibem mensagens amigáveis via `ConsoleHelper.Erro()`. Isso separa a **detecção** do erro (no Service ou nos Models) da **apresentação** (na UI).

**Por que a UI não valida regras de negócio?**

Para garantir que as regras sejam verificadas independentemente de quem chame o serviço. Quando a interface gráfica for implementada, ela chamará os mesmos serviços e as mesmas regras serão aplicadas automaticamente.

**Filtro por especialidade no agendamento:**

Ao agendar uma consulta, o sistema pergunta pela especialidade antes de listar os médicos. Se o campo for deixado em branco, lista todos. Isso é implementado em `ConsultasMenu.ListarMedicosInline(string?)`, que delega para `IMedicoRepositorio.BuscarPorEspecialidade()` quando um filtro é informado.

---

## 8. Tratamento de Exceções

**Arquivos:**
- [App/Backend/Consultas/Exceptions/ConsultaConflitanteException.cs](App/Backend/Consultas/Exceptions/ConsultaConflitanteException.cs)
- [App/Backend/Consultas/Exceptions/LimiteConsultasDiariasException.cs](App/Backend/Consultas/Exceptions/LimiteConsultasDiariasException.cs)

O sistema define duas exceções customizadas, ambas herdando de `Exception`:

| Exceção | Quando é lançada |
|---|---|
| `ConsultaConflitanteException` | Paciente já tem consulta com o mesmo médico no mesmo dia |
| `LimiteConsultasDiariasException` | Médico já tem 10 consultas ativas no dia solicitado |

Exceções do .NET também são usadas com semântica precisa:

| Exceção | Onde |
|---|---|
| `KeyNotFoundException` | Repositórios e AgendamentoService: entidade não encontrada por Id |
| `InvalidOperationException` | Repositórios: CPF/CRM duplicado; Consulta: transição de estado inválida |
| `ArgumentException` | Models: valor inválido em setter (CPF, nome, data de nascimento, diagnóstico) |

**Por que criar exceções customizadas?**

Exceções customizadas permitem que o código chamador trate cada tipo de erro de forma diferente via `catch` tipado. `LimiteConsultasDiariasException` é autoexplicativa e exibe uma mensagem diferente de `ConsultaConflitanteException`. Usar `Exception("limite atingido")` seria menos expressivo e impossibilitaria tratamento diferenciado.

---

## 9. Persistência de Dados

O sistema suporta dois modos de persistência, selecionáveis em `Program.cs` sem alterar nenhuma outra classe.

### 9.1 SQLite (modo padrão)

O banco de dados é um arquivo `clinica.db` criado em `App/Database/Sqlite/`. Quatro tabelas:

```sql
Medicos     (Id, Nome, Cpf, Telefone, Crm, Especialidade)
Pacientes   (Id, Nome, Cpf, Telefone, DataNascimento, Convenio)
Consultas   (Id, MedicoId, PacienteId, DataHora, Status, Observacoes, Diagnostico)
Prescricoes (Id, ConsultaId, Medicamento, Dosagem, Instrucoes)
```

`Prescricoes` tem `ON DELETE CASCADE` em `ConsultaId`, garantindo que ao remover uma consulta suas prescrições sejam removidas automaticamente pelo banco.

Todas as queries usam **parâmetros nomeados** (ex.: `@nome`, `@cpf`), prevenindo SQL Injection.

### 9.2 JSON (modo alternativo)

Os dados são salvos em três arquivos no diretório `App/Database/Json/`:

```
App/Database/Json/
├── medicos.json
├── pacientes.json
└── consultas.json
```

O caminho base é resolvido em `Program.cs` via `[CallerFilePath]`, garantindo que os arquivos sempre sejam criados ao lado do código-fonte independente de onde o executável é rodado:

```csharp
static string GetSourceDir([CallerFilePath] string p = "") => p;
var baseDir = Path.GetDirectoryName(GetSourceDir())!;
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

### 9.3 Troca entre modos

Para trocar de SQLite para JSON, basta comentar o bloco SQLite e descomentar o bloco JSON em `Program.cs`. Nenhuma outra classe precisa ser alterada:

```csharp
// Modo SQLite (padrão):
IMedicoRepositorio   medicoRepo   = new MedicoRepositorioSql(banco);
IPacienteRepositorio pacienteRepo = new PacienteRepositorioSql(banco);
IConsultaRepositorio consultaRepo = new ConsultaRepositorioSql(banco);

// Modo JSON (alternativo):
// IMedicoRepositorio   medicoRepo   = new MedicoRepositorio(Path.Combine(jsonDir, "medicos.json"));
// IPacienteRepositorio pacienteRepo = new PacienteRepositorio(Path.Combine(jsonDir, "pacientes.json"));
// IConsultaRepositorio consultaRepo = new ConsultaRepositorio(Path.Combine(jsonDir, "consultas.json"));
```

---

## 10. Os Quatro Pilares da POO no Projeto

### Abstração

A abstração consiste em modelar apenas o que é relevante para o domínio, ignorando detalhes desnecessários.

**Onde aparece:**

- **`Pessoa`** é uma abstração das características comuns de médicos e pacientes. Ela não existe como entidade concreta no sistema — é um molde.
- **`IRepositorio<T>`** é uma abstração da ideia de "armazenamento de dados". A interface define _o que_ pode ser feito com um repositório, sem dizer _como_ é feito — nem se os dados estão em JSON ou SQLite.
- **`RepositorioJson<T>`** é uma abstração intermediária: define o comportamento de persistência em JSON, mas deixa os detalhes de cada entidade para as subclasses.
- **`RepositorioBD<T>`** é a abstração equivalente para SQLite.

**Por que isso importa:**

A UI não sabe como os médicos são salvos. O Service não sabe se os dados vêm de JSON ou de um banco. Cada camada trabalha com abstrações, não com implementações concretas.

---

### Encapsulamento

O encapsulamento protege o estado interno dos objetos, expondo apenas o necessário.

**Onde aparece:**

- **Properties com validação em `Pessoa`**: `Nome` e `Cpf` têm setters que lançam `ArgumentException` se valores inválidos forem atribuídos. O CPF passa pelo algoritmo dos dois dígitos verificadores — o objeto nunca aceita um CPF matematicamente inválido.
- **`Idade` em `Paciente`**: property computada com getter apenas — não pode ser definida externamente. Calculada em tempo real a partir de `DataNascimento`.
- **Backing fields em `Consulta`**: `_status`, `_diagnostico` e `_prescricoes` são campos privados. As propriedades correspondentes são somente leitura (`Status => _status`, `Diagnostico => _diagnostico`, `Prescricoes => _prescricoes`). O único acesso de escrita é pelos métodos `Cancelar()`, `MarcarRealizada()`, `RegistrarDiagnostico()` e `AdicionarPrescricao()` — que validam a operação antes de executar. Código externo não consegue contornar essas validações.
- **`IReadOnlyList<Prescricao>` em `Consulta.Prescricoes`**: expõe a lista de prescrições como somente leitura. O consumidor pode iterar, mas não pode chamar `.Add()` ou `.Remove()` diretamente — essas operações só passam pelo método `AdicionarPrescricao()`.
- **`_dados` em `RepositorioJson`**: a lista interna é `protected` — acessível pelas subclasses, mas não por quem usa o repositório. `ListarTodos()` retorna `_dados.AsReadOnly()`, impedindo modificações externas na coleção.

---

### Herança

A herança permite que subclasses reutilizem e especializem comportamentos da superclasse.

**Hierarquia de Pessoas:**

```
Pessoa (abstract)
├── Medico   → adiciona Crm, Especialidade; sobrescreve ExibirInformacoes()
└── Paciente → adiciona DataNascimento, Convenio, Idade; sobrescreve ExibirInformacoes()
```

**Hierarquia de Repositórios JSON:**

```
IRepositorio<T> (interface)
└── RepositorioJson<T> (abstract) → implementa persistência em JSON
    ├── MedicoRepositorio    → implementa IMedicoRepositorio; especializa Adicionar()
    ├── PacienteRepositorio  → implementa IPacienteRepositorio; especializa Adicionar()
    └── ConsultaRepositorio  → implementa IConsultaRepositorio; especializa Adicionar()
```

**Hierarquia de Repositórios SQLite:**

```
IRepositorio<T> (interface)
└── RepositorioBD<T> (abstract) → gerencia ConexaoBanco; Salvar() é no-op
    ├── MedicoRepositorioSql    → implementa IMedicoRepositorio com SQL
    ├── PacienteRepositorioSql  → implementa IPacienteRepositorio com SQL
    └── ConsultaRepositorioSql  → implementa IConsultaRepositorio com SQL
```

**O que cada subclasse herda e o que especializa:**

Os repositórios JSON herdam de `RepositorioJson<T>` toda a lógica de leitura/escrita de JSON, a manutenção da lista em memória e os métodos `ListarTodos()` e `Salvar()`. Cada um sobrescreve apenas `Adicionar()`, `BuscarPorId()`, `Atualizar()` e `Remover()`.

Os repositórios SQLite herdam de `RepositorioBD<T>` a referência ao `ConexaoBanco` e o helper `ObterUltimoId()`. Implementam todos os cinco métodos abstratos via SQL.

---

### Polimorfismo

O polimorfismo permite que objetos de tipos diferentes respondam de forma específica a chamadas feitas por meio de uma referência comum.

**Polimorfismo com `Pessoa`:**

```csharp
Pessoa p1 = new Medico { Nome = "Dr. Ana", Crm = "MG-001", Especialidade = "Neurologia" };
Pessoa p2 = new Paciente { Nome = "Carlos", DataNascimento = new DateTime(1990, 5, 10) };

p1.ExibirInformacoes(); // "Dr(a). Dr. Ana | CRM: MG-001 | Neurologia | Tel: ..."
p2.ExibirInformacoes(); // "Carlos | 35 anos | Convênio: Particular | Tel: ..."
```

A chamada é feita na referência `Pessoa`, mas o comportamento executado é o da subclasse concreta. Isso permite que a UI itere sobre qualquer lista de pessoas e chame `ExibirInformacoes()` sem `if (p is Medico)`.

**Polimorfismo com interfaces de repositório:**

`AgendamentoService` recebe `IMedicoRepositorio`, `IPacienteRepositorio` e `IConsultaRepositorio`. Em produção, as implementações são SQLite (`MedicoRepositorioSql`, etc.). Se o `Program.cs` for alterado para JSON, o serviço recebe `MedicoRepositorio` — e funciona exatamente da mesma forma, sem mudar uma linha do `AgendamentoService`.

**Polimorfismo com `Consulta` (via estado):**

O método `Cancelar()` tem comportamento diferente dependendo do `_status` atual da consulta. A mesma chamada `consulta.Cancelar()` lança exceção se o status for `Realizada` ou `Cancelada`, e muda o status para `Cancelada` se for `Agendada`. O comportamento correto é selecionado em tempo de execução com base no estado do objeto.

---

## 11. Relação com as Especificações do Trabalho

### Requisitos Funcionais (Tema 6 — Clínica Médica)

| Requisito | Implementação |
|---|---|
| Cadastro de médicos | `MedicoRepositorio.Adicionar()` / `MedicoRepositorioSql.Adicionar()` |
| Cadastro de pacientes | `PacienteRepositorio.Adicionar()` / `PacienteRepositorioSql.Adicionar()` |
| Agendamento de consultas | `AgendamentoService.Agendar()` |
| Cancelamento de consultas | `AgendamentoService.Cancelar()` |
| Validação de horários | Setter `Consulta.DataHora` + validações em `Agendar()` |
| Listagem por médico | `AgendamentoService.ListarPorMedico()` |
| Histórico por paciente | `AgendamentoService.ListarPorPaciente()` |
| Listagem diária | `AgendamentoService.ListarPorData()` |
| Diagnóstico e prescrições | `AgendamentoService.RegistrarDiagnostico()` + `AgendamentoService.AdicionarPrescricao()` |
| Filtro por especialidade | `IMedicoRepositorio.BuscarPorEspecialidade()` + `ConsultasMenu.ListarMedicosInline(string?)` |

### Regras de Negócio

| Regra | Onde é verificada |
|---|---|
| Um paciente por médico/dia | `IConsultaRepositorio.PacienteTemConsultaComMedicoNoDia()` → lança `ConsultaConflitanteException` |
| Máximo 10 consultas por médico/dia | `IConsultaRepositorio.ContarConsultasAtivasMedicoNoDia()` → lança `LimiteConsultasDiariasException` |
| Consultas não podem ser no passado | Setter `Consulta.DataHora` (lança `ArgumentException`) |
| Não cancelar consulta realizada | `Consulta.Cancelar()` (lança `InvalidOperationException`) |
| Diagnóstico apenas em consulta realizada | `Consulta.RegistrarDiagnostico()` (lança `InvalidOperationException`) |

### Requisitos Adicionais do Trabalho

| Requisito | Status |
|---|---|
| Menu interativo no console | `MenuConsole` + 3 submenus + `ConsoleHelper` |
| Armazenamento em JSON | `RepositorioJson<T>` + 3 repositórios JSON |
| Banco de dados | SQLite ativo por padrão: `RepositorioBD<T>` + `ConexaoBanco` + `InicializadorBanco` + 3 repositórios SQL |
| Validações e tratamento de exceções | Exceções customizadas + validações nos models e no service |
| Quatro pilares da POO | Abstração, Encapsulamento, Herança e Polimorfismo aplicados concretamente |
| README.md com instruções | Presente |

---

## 12. Fluxo de Dados Completo

### Exemplo: Agendamento de uma consulta

```
Usuário informa: especialidade "Cardiologia", médico ID=1, paciente ID=2, data 2026-06-10 14:00
        │
        ▼
ConsultasMenu.AgendarConsulta()
  → exibe médicos filtrados por especialidade (IMedicoRepositorio.BuscarPorEspecialidade)
  → coleta MedicoId, PacienteId, DataHora, Observacoes
  → chama AgendamentoService.Agendar(1, 2, dataHora, obs)
        │
        ▼
AgendamentoService.Agendar()
  → IMedicoRepositorio.BuscarPorId(1)                           → OK: médico existe
  → IPacienteRepositorio.BuscarPorId(2)                         → OK: paciente existe
  → IConsultaRepositorio.ContarConsultasAtivas(1, 2026-06-10)   → retorna 3 (< 10, OK)
  → IConsultaRepositorio.PacienteTemConsulta(1, 2, 2026-06-10)  → retorna false (OK)
  → new Consulta(0, 1, 2, dataHora, obs)                        → setter DataHora valida: não é passado
  → IConsultaRepositorio.Adicionar(consulta)                    → persiste (SQLite ou JSON)
        │
        ▼
ConsultasMenu exibe: "Consulta agendada com sucesso! ID: #0001"
```

### Exemplo: Registro de diagnóstico

```
Usuário informa: ID da consulta = 1, diagnóstico = "Hipertensão"
        │
        ▼
ConsultasMenu.RegistrarDiagnosticoPrescricao()
  → AgendamentoService.RegistrarDiagnostico(1, "Hipertensão")
        │
        ▼
AgendamentoService.RegistrarDiagnostico()
  → IConsultaRepositorio.BuscarPorId(1)             → retorna Consulta{Status=Realizada}
  → consulta.RegistrarDiagnostico("Hipertensão")    → valida Status == Realizada, define _diagnostico
  → IConsultaRepositorio.Atualizar(consulta)         → persiste
        │
        ▼
ConsultasMenu exibe: "Prontuário atualizado."
```

### Exemplo: Falha por limite diário

```
AgendamentoService.Agendar()
  → IConsultaRepositorio.ContarConsultasAtivas(1, 2026-06-10) → retorna 10
  → throw LimiteConsultasDiariasException(medicoId, data)
        │
        ▼
ConsultasMenu.catch(LimiteConsultasDiariasException ex)
  → ConsoleHelper.Erro(ex.Message) → exibe em vermelho: "Médico atingiu o limite de 10 consultas..."
```

---

## 13. Decisões de Design e Justificativas

### Por que `IReadOnlyList<T>` em `Consulta.Prescricoes`?

Retornar `List<T>` diretamente permitiria que código externo chamasse `.Add()` na lista sem passar pelo método `AdicionarPrescricao()`, contornando a validação de status. `IReadOnlyList<T>` expõe apenas operações de leitura. Internamente, `_prescricoes` continua sendo um `List<T>` mutável — somente os métodos da classe o modificam.

### Por que `[JsonConstructor]` em `Consulta`?

O setter de `DataHora` valida que a data não está no passado — comportamento correto ao criar uma nova consulta. Mas ao deserializar dados salvos, a data pode ser histórica. O `[JsonConstructor]` instrui o deserializador a usar o construtor de hidratação, que define `_dataHora` diretamente no backing field, ignorando a validação. Sem isso, o sistema crasha ao carregar qualquer consulta com data passada.

### Por que `internal` em `DefinirPrescricoes()`?

O repositório SQLite carrega prescrições de uma tabela separada após hidratar o objeto `Consulta`. Precisa de um ponto de entrada para injetar essas prescrições. `public` exporia o método para qualquer código, quebrando o encapsulamento. `private` impediria o acesso dos repositórios. `internal` é o equilíbrio: acessível apenas dentro do assembly (o mesmo projeto), inacessível externamente.

### Por que o Id é gerado no repositório e não no model?

Porque a geração de Id depende de contexto (o maior Id existente na coleção, ou o `last_insert_rowid()` do banco). O model `Medico` não tem acesso à lista de médicos — seria uma violação de responsabilidade. O repositório tem esse contexto e é o lugar correto.

### Por que `AgendamentoService` recebe repositórios no construtor?

Isso é **injeção de dependência**: o serviço declara o que precisa, e quem o cria (`Program.cs`) fornece as implementações concretas. Facilita testes (seria possível injetar repositórios em memória) e permite trocar a camada de persistência sem alterar o serviço.

### Por que separar `ConsultaRepositorio` e `AgendamentoService`?

O repositório sabe **como acessar dados**. O serviço sabe **quais regras de negócio aplicar**. Misturar os dois criaria uma classe com dois motivos para mudar. Mantê-los separados torna o sistema previsível e fácil de evoluir.

### Por que `Cancelar()` está em `Consulta` e a validação de existência está em `AgendamentoService`?

`Consulta.Cancelar()` valida a transição de estado (não cancelar o que já foi realizado) — é um comportamento intrínseco do objeto. `AgendamentoService.Cancelar()` valida que a consulta existe — é uma operação que envolve o repositório. Cada responsabilidade está no lugar certo.

### Por que usar SQLite e manter JSON como fallback?

SQLite oferece integridade referencial (chaves estrangeiras com `ON DELETE CASCADE`), ausência de conflitos em escritas concorrentes e queries mais expressivas. JSON é mantido como fallback por ser transparente para depuração e por demonstrar que a arquitetura de repositórios é genuinamente intercambiável.

---

*Documentação atualizada em 25/05/2026 — Sistema de Clínica Médica, PUC Minas Betim.*
