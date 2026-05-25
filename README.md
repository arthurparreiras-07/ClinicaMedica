# Clínica Médica — Sistema de Gerenciamento

Trabalho Prático de Programação Orientada por Objetos — PUC Minas Betim  
Curso: Análise e Desenvolvimento de Sistemas

## Integrantes

| Nome | Responsabilidade |
|------|-----------------|
| (integrante 1) | Models / Domínio |
| (integrante 2) | Repositories / Persistência |
| (integrante 3) | Services / Regras de negócio |
| (integrante 4) | UI Console |
| (integrante 5) | Documentação |
| (integrante 6) | Testes / Validações |

## Descrição

Sistema orientado a objetos para gerenciamento de clínica médica, desenvolvido em C# com .NET 10.  
Permite cadastrar médicos e pacientes, agendar/cancelar consultas, registrar diagnósticos e prescrições, e consultar históricos, com persistência em arquivos JSON.

## Funcionalidades

- Cadastro de médicos (CRM, especialidade) e pacientes (convênio, data de nascimento)
- Agendamento de consultas com validação automática de regras de negócio:
  - Máximo de 10 consultas por médico/dia
  - Um paciente por médico/dia
  - Consultas não podem ser no passado
- Cancelamento e conclusão de consultas
- Registro de diagnóstico e prescrições médicas em consultas realizadas
- Agenda do dia, histórico por médico e prontuário por paciente
- Filtro de médicos por especialidade no agendamento
- Persistência automática em arquivos JSON

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Como compilar e executar

```bash
# Clonar o repositório
git clone https://github.com/arthurparreiras-07/ClinicaMedica
cd ClinicaMedica

# Compilar
dotnet build App/ClinicaMedica.csproj

# Executar
dotnet run --project App/ClinicaMedica.csproj
```

Ou abrir o diretório no Visual Studio / Rider e pressionar F5.

## Estrutura do projeto

```
ClinicaMedica/
├── App/
│   ├── Backend/
│   │   ├── Core/
│   │   │   ├── Models/Pessoa.cs                    # Classe abstrata base (Abstração + Encapsulamento)
│   │   │   └── Repositories/IRepositorio.cs        # Contrato genérico CRUD (Abstração)
│   │   ├── Medicos/
│   │   │   ├── Interfaces/IMedicoRepositorio.cs
│   │   │   ├── Models/Medico.cs                    # Herda de Pessoa (Herança + Polimorfismo)
│   │   │   └── Repositories/MedicoRepositorio.cs   # Persistência em JSON
│   │   ├── Pacientes/
│   │   │   ├── Interfaces/IPacienteRepositorio.cs
│   │   │   ├── Models/Paciente.cs                  # Herda de Pessoa (Herança + Polimorfismo)
│   │   │   └── Repositories/PacienteRepositorio.cs # Persistência em JSON
│   │   └── Consultas/
│   │       ├── Exceptions/
│   │       │   ├── ConsultaConflitanteException.cs
│   │       │   └── LimiteConsultasDiariasException.cs
│   │       ├── Interfaces/IConsultaRepositorio.cs
│   │       ├── Models/
│   │       │   ├── Consulta.cs                     # Entidade com ciclo de vida e prescrições
│   │       │   └── Prescricao.cs
│   │       ├── Repositories/ConsultaRepositorio.cs # Persistência em JSON
│   │       └── Services/AgendamentoService.cs      # Regras de negócio centralizadas
│   ├── Database/
│   │   └── Json/RepositorioJson.cs                 # Base abstrata para persistência em JSON
│   ├── UI/
│   │   └── TextUI/
│   │       ├── MenuConsole.cs                      # Menu principal
│   │       ├── Medicos/MedicosMenu.cs
│   │       ├── Pacientes/PacientesMenu.cs
│   │       ├── Consultas/ConsultasMenu.cs
│   │       └── Shared/ConsoleHelper.cs             # Utilitários de console
│   ├── Program.cs
│   └── ClinicaMedica.csproj
├── README.md
├── ENTREGA_PARCIAL.md
├── PROGRESSO.md
└── DOCUMENTACAO.md
```

## Armazenamento de dados

Os dados são salvos automaticamente em `App/Database/Json/`:
- `App/Database/Json/medicos.json`
- `App/Database/Json/pacientes.json`
- `App/Database/Json/consultas.json`

## Tecnologias

- C# 14 / .NET 10
- `System.Text.Json` para serialização em JSON
- Arquitetura em camadas: Models → Repositories → Services → UI
