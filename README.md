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
Permite cadastrar médicos e pacientes, agendar/cancelar consultas e consultar históricos, com persistência em JSON.

## Funcionalidades

- Cadastro de médicos (CRM, especialidade)
- Cadastro de pacientes (convênio, data de nascimento)
- Agendamento de consultas com validação de regras de negócio:
  - Máximo de 10 consultas por médico/dia
  - Um paciente por médico/dia
  - Consultas não podem ser no passado
- Cancelamento e conclusão de consultas
- Consulta da agenda do dia, por médico ou histórico do paciente
- Persistência automática em arquivos JSON

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Como compilar e executar

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd ClinicaMedica

# Compilar
dotnet build

# Executar
dotnet run --project ClinicaMedica/ClinicaMedica.csproj
```

Ou abrir `ClinicaMedica.sln` no Visual Studio / Rider e pressionar F5.

## Estrutura do projeto

```
ClinicaMedica/
├── Models/
│   ├── Pessoa.cs          # Classe abstrata base (Abstração + Encapsulamento)
│   ├── Medico.cs          # Herda de Pessoa (Herança + Polimorfismo)
│   ├── Paciente.cs        # Herda de Pessoa (Herança + Polimorfismo)
│   └── Consulta.cs        # Entidade de agendamento
├── Interfaces/
│   └── IRepositorio.cs    # Contrato genérico de repositório (Abstração)
├── Repositories/
│   ├── RepositorioJson.cs # Implementação base em JSON (Herança)
│   ├── MedicoRepositorio.cs
│   ├── PacienteRepositorio.cs
│   └── ConsultaRepositorio.cs
├── Services/
│   └── AgendamentoService.cs  # Regras de negócio
├── Exceptions/
│   ├── ConsultaConflitanteException.cs
│   └── LimiteConsultasDiariasException.cs
├── UI/
│   └── MenuConsole.cs     # Interface de console interativa
└── Program.cs
```

## Armazenamento de dados

Os dados são salvos automaticamente ao lado do `Program.cs`, versionados junto ao código:
- `ClinicaMedica/medicos.json`
- `ClinicaMedica/pacientes.json`
- `ClinicaMedica/consultas.json`

## Tecnologias

- C# 14 / .NET 10
- `System.Text.Json` para serialização
- Arquitetura em camadas (Models → Repositories → Services → UI)
