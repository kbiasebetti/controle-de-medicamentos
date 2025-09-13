using ControleDeMedicamentos.Dominio.ModuloMedicamento;
using ControleDeMedicamentos.Dominio.ModuloPaciente;
using ControleDeMedicamentos.Dominio.ModuloPrescricao;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace ControleDeMedicamentos.WebApp.Models;

public class CadastrarPrescricaoViewModel
{
    [Required(ErrorMessage = "O campo 'Descrição' é obrigatório.")]
    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Data de Validade' é obrigatório.")]
    public DateTime DataValidade { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "O campo 'CRM do Médico' é obrigatório.")]
    [RegularExpression(@"^\d{4,7}-?[A-Z]{2}$", ErrorMessage = "O campo 'CRM do Médico' deve seguir o padrão 1111000-UF.")]
    public string CrmMedico { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Paciente' é obrigatório.")]
    public Guid PacienteId { get; set; }
    public List<SelectListItem> PacientesDisponiveis { get; set; } = new List<SelectListItem>();

    public CadastrarPrescricaoViewModel() { }

    public CadastrarPrescricaoViewModel(List<Paciente> pacientes) : this()
    {
        PacientesDisponiveis = pacientes
            .Select(p => new SelectListItem(p.Nome, p.Id.ToString()))
            .ToList();
    }
}

public class EditarPrescricaoViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo 'Descrição' é obrigatório.")]
    public string Descricao { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Data de Validade' é obrigatório.")]
    public DateTime DataValidade { get; set; }

    [Required(ErrorMessage = "O campo 'CRM do Médico' é obrigatório.")]
    [RegularExpression(@"^\d{4,7}-?[A-Z]{2}$", ErrorMessage = "O campo 'CRM do Médico' deve seguir o padrão 1111000-UF.")]
    public string CrmMedico { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Paciente' é obrigatório.")]
    public Guid PacienteId { get; set; }
    public List<SelectListItem> PacientesDisponiveis { get; set; } = new List<SelectListItem>();

    public EditarPrescricaoViewModel() { }

    public EditarPrescricaoViewModel(Prescricao prescricao, List<Paciente> pacientes)
    {
        Id = prescricao.Id;
        Descricao = prescricao.Descricao;
        DataValidade = prescricao.DataValidade;
        CrmMedico = prescricao.CrmMedico;
        PacienteId = prescricao.Paciente.Id;
        PacientesDisponiveis = pacientes
            .Select(p => new SelectListItem(p.Nome, p.Id.ToString()))
            .ToList();
    }
}

public class ExcluirPrescricaoViewModel
{
    public Guid Id { get; set; }
    public string Paciente { get; set; } = "";

    public ExcluirPrescricaoViewModel() { }

    public ExcluirPrescricaoViewModel(Guid id, string nomePaciente)
    {
        Id = id;
        Paciente = nomePaciente;
    }
}


public class VisualizarPrescricoesViewModel
{
    public List<DetalhesPrescricaoViewModel> Registros { get; }

    public VisualizarPrescricoesViewModel(List<Prescricao> prescricoes)
    {
        Registros = prescricoes
            .Select(p => new DetalhesPrescricaoViewModel(p))
            .ToList();
    }
}

public class DetalhesPrescricaoViewModel
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = "";
    public string CrmMedico { get; set; } = "";
    public string Paciente { get; set; } = "";
    public DateTime DataEmissao { get; set; }
    public DateTime DataValidade { get; set; }
    public List<DetalhesMedicamentoPrescritoViewModel> MedicamentosPrescritos { get; set; } = new();

    public DetalhesPrescricaoViewModel(Prescricao prescricao)
    {
        Id = prescricao.Id;
        Descricao = prescricao.Descricao;
        CrmMedico = prescricao.CrmMedico;
        Paciente = prescricao.Paciente.Nome;
        DataEmissao = prescricao.DataEmissao;
        DataValidade = prescricao.DataValidade;
        MedicamentosPrescritos = prescricao.MedicamentosPrescritos
           .Select(m => new DetalhesMedicamentoPrescritoViewModel(m))
           .ToList();
    }
}

public class DetalhesMedicamentoPrescritoViewModel
{
    public Guid Id { get; set; }
    public Guid MedicamentoId { get; set; }
    public string Medicamento { get; set; } = "";
    public string Dosagem { get; set; } = "";
    public string Periodo { get; set; } = "";
    public int Quantidade { get; set; }

    public DetalhesMedicamentoPrescritoViewModel() { }

    public DetalhesMedicamentoPrescritoViewModel(MedicamentoPrescrito medPrescrito)
    {
        Id = medPrescrito.Id;
        MedicamentoId = medPrescrito.Medicamento.Id;
        Medicamento = medPrescrito.Medicamento.Nome;
        Dosagem = medPrescrito.Dosagem;
        Periodo = medPrescrito.Periodo;
        Quantidade = medPrescrito.Quantidade;
    }
}

public class GerenciarPrescricaoViewModel
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = "";
    public string CrmMedico { get; set; } = "";
    public Guid PacienteId { get; set; }
    public string Paciente { get; set; } = "";
    public List<SelectListItem> MedicamentosDisponiveis { get; set; } = new();
    public List<DetalhesMedicamentoPrescritoViewModel> MedicamentosPrescritos { get; set; } = new();
    public AdicionarMedicamentoPrescritoViewModel NovoMedicamento { get; set; } = new();

    public GerenciarPrescricaoViewModel() { }

    public GerenciarPrescricaoViewModel(Prescricao prescricao, List<Medicamento> todosOsMedicamentos)
    {
        Id = prescricao.Id;
        Descricao = prescricao.Descricao;
        CrmMedico = prescricao.CrmMedico;
        PacienteId = prescricao.Paciente.Id;
        Paciente = prescricao.Paciente.Nome;

        MedicamentosDisponiveis = todosOsMedicamentos
            .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
            .ToList();

        MedicamentosPrescritos = prescricao.MedicamentosPrescritos
            .Select(m => new DetalhesMedicamentoPrescritoViewModel(m))
            .ToList();
    }
}

public class AdicionarMedicamentoPrescritoViewModel
{
    public Guid PrescricaoId { get; set; }

    [Required(ErrorMessage = "O campo 'Medicamento' é obrigatório.")]
    public Guid MedicamentoId { get; set; }

    [Required(ErrorMessage = "O campo 'Dosagem' é obrigatório.")]
    public string Dosagem { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Período' é obrigatório.")]
    public string Periodo { get; set; } = "";

    [Required(ErrorMessage = "O campo 'Quantidade' é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de no mínimo 1.")]
    public int Quantidade { get; set; }
}