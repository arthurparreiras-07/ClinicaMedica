# Entrega Parcial — Modelagem do Sistema em POO

**Disciplina:** Programação Orientada por Objetos  
**Curso:** Análise e Desenvolvimento de Sistemas — PUC Minas Betim  
**Tema:** Sistema de Gerenciamento de Clínica Médica

---

## 1. Apresentação do Sistema

O sistema tem como objetivo gerenciar as operações de uma clínica médica, atendendo recepcionistas e administradores que precisam controlar o cadastro de médicos, pacientes e o agendamento de consultas.

**Principais funcionalidades:**

- Cadastro de médicos (com CRM e especialidade) e pacientes (com convênio e data de nascimento)
- Agendamento de consultas com validação automática de regras de negócio
- Cancelamento e conclusão de consultas
- Consulta da agenda diária, histórico por médico e histórico por paciente
- Persistência automática dos dados em arquivos JSON

**Regras de negócio centrais:**

- Um médico pode ter no máximo 10 consultas por dia
- Um paciente não pode ter mais de uma consulta com o mesmo médico no mesmo dia
- Consultas não podem ser agendadas para datas passadas

---

## 2. Modelagem do Sistema

### Classes e seus papéis

| Classe / Interface | Tipo | Responsabilidade |
|--------------------|------|-----------------|
| `Pessoa` | Classe abstrata | Representa qualquer pessoa no sistema (atributos comuns: Id, Nome, CPF, Telefone) e define o contrato `ExibirInformacoes()` |
| `Medico` | Classe concreta | Especialização de `Pessoa`; acrescenta CRM e Especialidade |
| `Paciente` | Classe concreta | Especialização de `Pessoa`; acrescenta DataNascimento, Convênio e cálculo automático de Idade |
| `Consulta` | Classe concreta | Associa um médico a um paciente em um horário; controla o ciclo de vida (Agendada → Realizada/Cancelada); armazena diagnóstico e prescrições |
| `Prescricao` | Classe concreta | Representa uma prescrição médica vinculada a uma consulta realizada (medicamento, dosagem, instruções) |
| `IRepositorio<T>` | Interface genérica | Define o contrato CRUD para qualquer entidade persistida (Adicionar, BuscarPorId, ListarTodos, Atualizar, Remover, Salvar) |
| `IMedicoRepositorio` | Interface | Estende `IRepositorio<Medico>`; acrescenta `BuscarPorCrm` e `BuscarPorEspecialidade` |
| `IPacienteRepositorio` | Interface | Estende `IRepositorio<Paciente>`; acrescenta `BuscarPorCpf` |
| `IConsultaRepositorio` | Interface | Estende `IRepositorio<Consulta>`; acrescenta buscas por médico, paciente, data e verificações de conflito/limite |
| `RepositorioJson<T>` | Classe abstrata | Implementa leitura e escrita em JSON; deixa os métodos específicos de entidade para as subclasses |
| `MedicoRepositorio` | Classe concreta | Implementa `IMedicoRepositorio` sobre JSON; busca por CRM e especialidade |
| `PacienteRepositorio` | Classe concreta | Implementa `IPacienteRepositorio` sobre JSON; busca por CPF |
| `ConsultaRepositorio` | Classe concreta | Implementa `IConsultaRepositorio` sobre JSON; inclui contagem de consultas por dia e verificação de conflito |
| `AgendamentoService` | Classe de serviço | Centraliza as regras de negócio (validações de limite, conflito, existência) e orquestra os repositórios |
| `MenuConsole` | Classe de UI | Menu principal no console; delega para os submenus especializados |
| `MedicosMenu` | Classe de UI | Submenu de cadastro, listagem e busca de médicos |
| `PacientesMenu` | Classe de UI | Submenu de cadastro, listagem e busca de pacientes |
| `ConsultasMenu` | Classe de UI | Submenu de agendamento, cancelamento, prontuário, agenda diária e prescrições |
| `ConsultaConflitanteException` | Exceção | Lançada quando um paciente já tem consulta com o mesmo médico no dia |
| `LimiteConsultasDiariasException` | Exceção | Lançada quando o médico atinge 10 consultas em um dia |

### Relacionamentos

```
Pessoa (abstrata)
├── Medico          (herança)
└── Paciente        (herança)

Consulta ──────── Medico      (associação por MedicoId)
         ──────── Paciente    (associação por PacienteId)
         ──────── Prescricao  (composição: List<Prescricao>)

IRepositorio<T> (interface genérica)
└── RepositorioJson<T> (abstrata, implementa IRepositorio<T>)
    ├── MedicoRepositorio    (implementa IMedicoRepositorio)
    ├── PacienteRepositorio  (implementa IPacienteRepositorio)
    └── ConsultaRepositorio  (implementa IConsultaRepositorio)

IMedicoRepositorio   ──> IRepositorio<Medico>   (herança de interface)
IPacienteRepositorio ──> IRepositorio<Paciente> (herança de interface)
IConsultaRepositorio ──> IRepositorio<Consulta> (herança de interface)

AgendamentoService ──── IConsultaRepositorio
                   ──── IMedicoRepositorio
                   ──── IPacienteRepositorio

MenuConsole ──── MedicosMenu    ──── IMedicoRepositorio
            ──── PacientesMenu  ──── IPacienteRepositorio
            ──── ConsultasMenu  ──── AgendamentoService
                                ──── IMedicoRepositorio
                                ──── IPacienteRepositorio
```

---

## 3. Aplicação dos Quatro Pilares da POO

### Abstração

A **abstração** aparece em dois pontos centrais do projeto:

**1. Classe abstrata `Pessoa`**  
`Pessoa` representa o conceito genérico de um indivíduo no sistema, sem existir de forma isolada — não é possível instanciá-la diretamente. Ela define os atributos comuns a todos (`Nome`, `CPF`, `Telefone`) e declara o método abstrato `ExibirInformacoes()`, obrigando cada subclasse a definir como se apresenta:

```csharp
public abstract class Pessoa
{
    public abstract string ExibirInformacoes();
}
```

**2. Interface `IRepositorio<T>` e classe abstrata `RepositorioJson<T>`**  
A interface define o contrato de persistência (Adicionar, Buscar, Listar, Atualizar, Remover) sem se preocupar com como os dados são armazenados. `RepositorioJson<T>` concretiza a leitura/escrita em JSON, mas mantém os métodos específicos (`BuscarPorId`, `Adicionar`, etc.) como abstratos para que cada repositório os implemente conforme a entidade:

```csharp
public interface IRepositorio<T> { ... }

public abstract class RepositorioJson<T> : IRepositorio<T>
{
    public abstract void Adicionar(T entidade);
    public abstract T? BuscarPorId(int id);
    // ...
}
```

### Encapsulamento

Atributos sensíveis são privados e controlados por propriedades com validação embutida. Isso garante que o objeto nunca entre em estado inválido:

- Em `Pessoa`: `_nome` e `_cpf` são campos privados. O setter de `Cpf` rejeita qualquer valor com menos de 11 dígitos ou com letras; o de `Nome` rejeita strings vazias.
- Em `Paciente`: `_dataNascimento` é privado; o setter impede datas futuras ou anteriores a 1900.
- Em `Medico`: `_crm` e `_especialidade` são privados; seus setters rejeitam valores em branco.
- Em `Consulta`: `_dataHora`, `_status`, `_diagnostico` e `_prescricoes` são campos privados. `Status` e `Diagnostico` são propriedades somente leitura (`=> _campo`); o único acesso de escrita é pelos próprios métodos da classe (`Cancelar`, `MarcarRealizada`, `RegistrarDiagnostico`, `AdicionarPrescricao`). Código externo não consegue contornar as validações de transição de estado.

```csharp
public string Cpf
{
    get => _cpf;
    set
    {
        var cpfLimpo = value?.Replace(".", "").Replace("-", "").Trim() ?? "";
        if (cpfLimpo.Length != 11 || !cpfLimpo.All(char.IsDigit))
            throw new ArgumentException("CPF inválido. Informe 11 dígitos numéricos.");
        _cpf = cpfLimpo;
    }
}
```

As regras de negócio estão encapsuladas no `AgendamentoService`, que é o único ponto de entrada para agendar ou cancelar consultas — a UI não acessa os repositórios de consulta diretamente para essas operações.

### Herança

**Hierarquia de pessoas:**

```
Pessoa (abstrata)
├── Medico   — herda Id, Nome, CPF, Telefone; adiciona Crm e Especialidade
└── Paciente — herda Id, Nome, CPF, Telefone; adiciona DataNascimento, Convenio e Idade
```

Ambas as subclasses chamam o construtor de `Pessoa` via `base(id, nome, cpf, telefone)` e são obrigadas a implementar `ExibirInformacoes()`.

**Hierarquia de repositórios:**

```
RepositorioJson<T> (abstrata)
├── MedicoRepositorio   — adiciona BuscarPorCrm e BuscarPorEspecialidade
├── PacienteRepositorio — adiciona buscas específicas de pacientes
└── ConsultaRepositorio — adiciona ContarConsultasAtivasMedicoNoDia, BuscarPorMedico, BuscarPorData
```

A lógica de serialização JSON (carregar arquivo, escrever arquivo, listar todos) vive uma única vez em `RepositorioJson<T>` e é reaproveitada pelas três subclasses sem repetição de código.

### Polimorfismo

O método `ExibirInformacoes()` é declarado como `abstract` em `Pessoa` e implementado de forma diferente em cada subclasse:

```csharp
// Em Medico:
public override string ExibirInformacoes() =>
    $"Dr(a). {Nome} | CRM: {Crm} | {Especialidade} | Tel: {Telefone}";

// Em Paciente:
public override string ExibirInformacoes() =>
    $"{Nome} | {Idade} anos | Convênio: {Convenio} | Tel: {Telefone}";
```

Isso permite que a UI trate tanto `Medico` quanto `Paciente` como `Pessoa` e chame `ExibirInformacoes()` sem saber o tipo concreto:

```csharp
Pessoa p = obterPessoa(); // pode ser Medico ou Paciente
Console.WriteLine(p.ExibirInformacoes()); // comportamento correto em ambos os casos
```

O mesmo ocorre nos repositórios: o `AgendamentoService` recebe `IMedicoRepositorio`, `IPacienteRepositorio` e `IConsultaRepositorio` via construtor. As implementações concretas (`MedicoRepositorio`, `PacienteRepositorio`, `ConsultaRepositorio`) sobrescrevem os métodos abstratos de `RepositorioJson<T>` com comportamentos específicos da entidade — mas o serviço não sabe (e não precisa saber) qual implementação está usando. Isso permitirá trocar JSON por banco de dados sem alterar nenhuma regra de negócio.

---

## 4. Decisões Iniciais

### Formato de armazenamento

O grupo optou por **JSON** como formato de persistência. A escolha se deve à facilidade de leitura humana para depuração, ao suporte nativo do .NET via `System.Text.Json` e à compatibilidade direta com a estrutura de objetos C#. Os arquivos gerados são:

- `data/medicos.json`
- `data/pacientes.json`
- `data/consultas.json`

A arquitetura de repositórios (`IRepositorio<T>` + `RepositorioJson<T>`) já está desenhada para permitir uma futura migração para banco de dados sem reescrever as camadas de serviço e UI.

### Divisão de tarefas

| Integrante | Responsabilidade |
|------------|-----------------|
| _(integrante 1)_ | Models / Domínio (`Pessoa`, `Medico`, `Paciente`, `Consulta`) |
| _(integrante 2)_ | Repositories / Persistência (`RepositorioJson<T>` e subclasses) |
| _(integrante 3)_ | Services / Regras de negócio (`AgendamentoService`) |
| _(integrante 4)_ | UI Console (`MenuConsole`) |
| _(integrante 5)_ | Exceptions e validações |
| _(integrante 6)_ | Documentação e testes manuais |

---

## 5. Repositório GitHub

**Link:** [https://github.com/arthurparreiras-07/ClinicaMedica](https://github.com/arthurparreiras-07/ClinicaMedica)

O repositório contém o código-fonte inicial, o `README.md` com nome do projeto, lista de integrantes, descrição e instruções de compilação/execução.
