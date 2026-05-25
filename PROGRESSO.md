# Progresso do Projeto — Clínica Médica

**Disciplina:** Programação Orientada por Objetos — PUC Minas Betim  
**Entrega final:** 26/06/2026 | **Apresentação:** 26/06/2026  
**Última atualização:** 25/05/2026

---

## Resumo

| Categoria | Concluído | Pendente | Total |
|-----------|:---------:|:--------:|:-----:|
| Requisitos funcionais | 8 | 0 | 8 |
| Pilares da POO | 4 | 0 | 4 |
| Requisitos adicionais | 4 | 3 | 7 |
| Documentação | 3 | 1 | 4 |

---

## Requisitos Funcionais

### Concluído

- [x] **Cadastro de médicos** — Nome, CPF, telefone, CRM e especialidade, com validação em todos os campos (`Medico.cs`, `MedicoRepositorio.cs`)
- [x] **Cadastro de pacientes** — Nome, CPF, telefone, data de nascimento e convênio, com cálculo automático de idade (`Paciente.cs`, `PacienteRepositorio.cs`)
- [x] **Agendamento de consultas** — Com validação das três regras de negócio: máximo 10 consultas/médico/dia, um paciente por médico por dia, sem datas no passado (`AgendamentoService.cs`)
- [x] **Cancelamento de consultas** — Com controle de estado (não cancela consulta já realizada) (`Consulta.cs`, `AgendamentoService.cs`)
- [x] **Validação de horários** — Bloqueio de datas passadas no setter de `Consulta.DataHora`; regras de conflito encapsuladas em exceções próprias
- [x] **Listagem por médico** — Agenda completa de um médico ordenada por data/hora (`AgendamentoService.ListarPorMedico`)
- [x] **Histórico por paciente** — Todas as consultas de um paciente ordenadas por data/hora (`AgendamentoService.ListarPorPaciente`)
- [x] **Listagem diária** — Consultas de um dia específico (padrão: hoje) (`AgendamentoService.ListarPorData`)

### Concluído (adicionado em 25/05/2026)

- [x] **Filtro por especialidade na tela de agendamento** — Ao agendar, o sistema pergunta por especialidade antes de listar os médicos; se deixado em branco, lista todos (`MenuConsole.AgendarConsulta` + `ListarMedicosInline(string?)`)
- [x] **Prontuário / diagnóstico e prescrições** — Modelo `Prescricao` criado; `Consulta` ganhou `Diagnostico` e `List<Prescricao>` com métodos `RegistrarDiagnostico` e `AdicionarPrescricao`; `AgendamentoService` expõe os novos métodos; UI oferece registro imediato ao marcar consulta como realizada e opção dedicada no menu (opção 7); prontuário detalhado exibido no histórico do paciente (opção 5)

---

## Pilares da POO

### Concluído

- [x] **Abstração** — `Pessoa` (classe abstrata com `ExibirInformacoes()` abstrato) e `IRepositorio<T>` (interface genérica de persistência) + `RepositorioJson<T>` (classe abstrata que implementa a interface)
- [x] **Encapsulamento** — Campos privados com setters validados em `Pessoa` (`_nome`, `_cpf`), `Medico` (`_crm`, `_especialidade`), `Paciente` (`_dataNascimento`) e `Consulta` (`_dataHora`); regras de negócio centralizadas em `AgendamentoService`
- [x] **Herança** — `Medico` e `Paciente` herdam de `Pessoa`; `MedicoRepositorio`, `PacienteRepositorio` e `ConsultaRepositorio` herdam de `RepositorioJson<T>`
- [x] **Polimorfismo** — `ExibirInformacoes()` com comportamento distinto em `Medico` e `Paciente`; repositórios concretos sobrescrevem os métodos abstratos de `RepositorioJson<T>`

---

## Requisitos Adicionais

### Concluído

- [x] **Menu interativo no console** — Menu hierárquico completo (Médicos / Pacientes / Consultas) com submenus e navegação em loop (`MenuConsole.cs`)
- [x] **Armazenamento de dados em JSON** — Persistência automática via `System.Text.Json`; três arquivos separados (`medicos.json`, `pacientes.json`, `consultas.json`) em `ClinicaMedica/data/`
- [x] **Validações e tratamento de exceções** — Exceções de domínio próprias (`ConsultaConflitanteException`, `LimiteConsultasDiariasException`); tratamento de `ArgumentException`, `InvalidOperationException` e `KeyNotFoundException` em toda a UI

### Pendente

- [ ] **Interface gráfica** — Nenhuma tela gráfica implementada ainda; apenas o console. Requer decisão de tecnologia (WinForms, WPF ou MAUI)
- [ ] **Banco de dados** — Atualmente tudo em JSON. A interface `IRepositorio<T>` já prepara a migração: basta criar implementações de repositório para o banco escolhido sem alterar `AgendamentoService` ou a UI
- [x] **Padrões de projeto documentados** — Ver seção abaixo
- [ ] **README com instruções completas de compilação/execução** — Existe, mas os nomes dos integrantes estão como placeholders e precisam ser preenchidos

---

## Documentação

### Concluído

- [x] **ENTREGA_PARCIAL.md** — Documento da entrega parcial (31/05/2026) com apresentação do sistema, modelagem, quatro pilares e decisões iniciais
- [x] **README.md** — Estrutura básica com descrição, funcionalidades, pré-requisitos e instruções de execução

### Pendente

- [ ] **Nomes dos integrantes** — `README.md` e `ENTREGA_PARCIAL.md` têm placeholders que precisam ser preenchidos com os nomes reais e a divisão de tarefas
- [ ] **Documentação final (PDF)** — Requerida para a entrega final: descrição detalhada, arquitetura, diagramas de classes/fluxo e padrões de projeto utilizados

---

---

## Padrões de Projeto Utilizados

### 1. Repository Pattern

**Onde:** `Interfaces/IRepositorio.cs`, `Repositories/RepositorioJson.cs`, `MedicoRepositorio`, `PacienteRepositorio`, `ConsultaRepositorio`

**O que é:** Isola a camada de persistência do restante da aplicação. Toda operação de dados passa por um repositório; quem chama o repositório não sabe (nem precisa saber) se os dados estão em JSON, banco de dados ou memória.

**Como está aplicado:**
- `IRepositorio<T>` define o contrato genérico: `Adicionar`, `BuscarPorId`, `ListarTodos`, `Atualizar`, `Remover`, `Salvar`
- `RepositorioJson<T>` implementa a leitura/escrita em disco uma única vez
- Cada repositório concreto especializa apenas as buscas da sua entidade (ex: `MedicoRepositorio.BuscarPorCrm`, `ConsultaRepositorio.BuscarPorData`)
- `AgendamentoService` recebe os repositórios via construtor e nunca acessa arquivos diretamente

**Benefício prático:** Migrar de JSON para banco de dados exige apenas criar novas classes que implementem `IRepositorio<T>` — `AgendamentoService` e a UI não precisam mudar.

---

### 2. Service Layer (Camada de Serviço)

**Onde:** `Services/AgendamentoService.cs`

**O que é:** Concentra as regras de negócio em uma classe separada, impedindo que a lógica fique espalhada pela UI ou pelos repositórios.

**Como está aplicado:**
- Toda operação com efeito colateral (agendar, cancelar, marcar como realizada, registrar diagnóstico, adicionar prescrição) passa obrigatoriamente pelo `AgendamentoService`
- O serviço orquestra múltiplos repositórios e decide a ordem das validações:
  1. Verifica se médico e paciente existem
  2. Verifica o limite de 10 consultas por dia
  3. Verifica conflito de paciente no mesmo dia
  4. Só então cria e persiste a consulta
- A UI (`MenuConsole`) nunca acessa `ConsultaRepositorio` diretamente para operações de escrita

**Benefício prático:** Se a regra "máximo 10 consultas por dia" mudar para 8, a alteração é feita em um único lugar (`AgendamentoService`, constante `LimiteConsultasPorDia`).

---

### 3. Template Method

**Onde:** `Repositories/RepositorioJson.cs` e seus repositórios concretos

**O que é:** A classe base define o esqueleto de um algoritmo (sequência de passos fixos) e deixa alguns passos para as subclasses preencherem.

**Como está aplicado:**
- `RepositorioJson<T>` implementa os passos invariantes: carregar JSON do disco (`Carregar`), serializar e salvar (`Salvar`), e listar todos (`ListarTodos`)
- Declara como `abstract` os métodos que dependem da entidade específica: `Adicionar`, `BuscarPorId`, `Atualizar`, `Remover`
- Cada subclasse preenche apenas esses métodos, aproveitando tudo que `RepositorioJson<T>` já faz

```
RepositorioJson<T>
├── Carregar()      ← implementado (lê JSON)
├── Salvar()        ← implementado (escreve JSON)
├── ListarTodos()   ← implementado
├── Adicionar()     ← abstract → MedicoRepositorio: valida CRM duplicado
├── BuscarPorId()   ← abstract → cada um busca pelo seu critério
├── Atualizar()     ← abstract → cada um localiza pelo seu Id
└── Remover()       ← abstract → cada um remove pelo seu Id
```

**Benefício prático:** Adicionar um novo repositório (ex: `ProntuarioRepositorio`) exige implementar apenas os 4 métodos abstratos — toda a infraestrutura de arquivo já vem herdada.

---

## Próximos Passos (sugestão de prioridade)

| Prioridade | Tarefa | Observação |
|:----------:|--------|-----------|
| 1 | Preencher nomes dos integrantes no README e na entrega parcial | Necessário antes de 31/05 |
| 2 | Entregar `ENTREGA_PARCIAL.md` convertido em PDF no Canvas | Prazo: 31/05/2026 |
| 3 | Definir tecnologia de interface gráfica (WinForms / WPF / MAUI) | Decisão que afeta todo o desenvolvimento seguinte |
| 4 | Implementar interface gráfica | Requisito obrigatório do trabalho |
| 5 | Migração para banco de dados | Arquitetura já prepara essa mudança via `IRepositorio<T>` |
| 6 | Documentação final em PDF | Entrega: 26/06/2026 |

---

## Estrutura atual do projeto

```
ClinicaMedica/
├── Models/
│   ├── Pessoa.cs                     ✓ classe abstrata base
│   ├── Medico.cs                     ✓ herda de Pessoa
│   ├── Paciente.cs                   ✓ herda de Pessoa
│   └── Consulta.cs                   ✓ entidade de agendamento com ciclo de vida
├── Interfaces/
│   └── IRepositorio.cs               ✓ contrato genérico CRUD
├── Repositories/
│   ├── RepositorioJson.cs            ✓ persistência base em JSON
│   ├── MedicoRepositorio.cs          ✓ busca por CRM e especialidade
│   ├── PacienteRepositorio.cs        ✓ busca por CPF
│   └── ConsultaRepositorio.cs        ✓ buscas por médico, paciente e data
├── Services/
│   └── AgendamentoService.cs         ✓ regras de negócio centralizadas
├── Exceptions/
│   ├── ConsultaConflitanteException.cs      ✓
│   └── LimiteConsultasDiariasException.cs   ✓
├── UI/
│   └── MenuConsole.cs                ✓ menu interativo completo
│   └── [interface gráfica]           ✗ pendente
├── data/
│   ├── medicos.json                  ✓
│   ├── pacientes.json                ✓
│   └── consultas.json                ✓
└── Program.cs                        ✓ ponto de entrada com injeção de dependências
```
