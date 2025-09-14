﻿
using ControleDeMedicamentos.Dominio.ModuloPrescricao;
using ControleDeMedicamentos.Dominio.ModuloRequisicaoMedicamento;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloFuncionario;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloMedicamento;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloPaciente;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloPrescricao;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloRequisicaoMedicamento;
using ControleDeMedicamentos.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleDeMedicamentos.WebApp.Controllers;

public class RequisicaoMedicamentoController : Controller
{
    private readonly RepositorioRequisicaoMedicamentoEmArquivo repositorioRequisicaoMedicamento;
    private readonly RepositorioMedicamentoEmArquivo repositorioMedicamento;
    private readonly RepositorioFuncionarioEmArquivo repositorioFuncionario;
    private readonly RepositorioPacienteEmArquivo repositorioPaciente;
    private readonly RepositorioPrescricaoEmArquivo repositorioPrescricao;

    public RequisicaoMedicamentoController(
        RepositorioRequisicaoMedicamentoEmArquivo repositorioRequisicaoMedicamento,
        RepositorioMedicamentoEmArquivo repositorioMedicamento,
        RepositorioFuncionarioEmArquivo repositorioFuncionario,
        RepositorioPacienteEmArquivo repositorioPaciente,
        RepositorioPrescricaoEmArquivo repositorioPrescricao
    )
    {
        this.repositorioRequisicaoMedicamento = repositorioRequisicaoMedicamento;
        this.repositorioMedicamento = repositorioMedicamento;
        this.repositorioFuncionario = repositorioFuncionario;
        this.repositorioPaciente = repositorioPaciente;
        this.repositorioPrescricao = repositorioPrescricao;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var requisicoesEntrada = repositorioRequisicaoMedicamento.SelecionarRequisicoesEntrada();
        var requisicoesSaida = repositorioRequisicaoMedicamento.SelecionarRequisicoesSaida();

        var visualizarVm = new VisualizarRequisicoesMedicamentoViewModel(requisicoesEntrada, requisicoesSaida);

        return View(visualizarVm);
    }

    [HttpGet]
    public IActionResult CadastrarRequisicaoEntrada()
    {
        var medicamentosDisponiveis = repositorioMedicamento.SelecionarRegistros();
        var funcionariosDisponiveis = repositorioFuncionario.SelecionarRegistros();

        var cadastrarVm = new CadastrarRequisicaoEntradaViewModel(medicamentosDisponiveis, funcionariosDisponiveis);

        return View(cadastrarVm);
    }

    [HttpPost]
    public IActionResult CadastrarRequisicaoEntrada(CadastrarRequisicaoEntradaViewModel cadastrarVm)
    {
        if (!ModelState.IsValid)
        {
            var medicamentosDisponiveis = repositorioMedicamento.SelecionarRegistros();

            cadastrarVm.MedicamentosDisponiveis = medicamentosDisponiveis
                .Select(m => new SelectListItem(m.Nome, m.Id.ToString()))
                .ToList();

            var funcionariosDisponiveis = repositorioFuncionario.SelecionarRegistros();

            cadastrarVm.FuncionariosDisponiveis = funcionariosDisponiveis
                .Select(f => new SelectListItem(f.Nome, f.Id.ToString()))
                .ToList();

            return View(cadastrarVm);
        }

        var funcionarioSelecionado = repositorioFuncionario.SelecionarRegistroPorId(cadastrarVm.FuncionarioId);
        var medicamentoSelecionado = repositorioMedicamento.SelecionarRegistroPorId(cadastrarVm.MedicamentoId);

        var requisicaoEntrada = new RequisicaoEntrada(
            funcionarioSelecionado,
            medicamentoSelecionado,
            cadastrarVm.QuantidadeRequisitada
        );

        medicamentoSelecionado.AdicionarAoEstoque(requisicaoEntrada);

        repositorioRequisicaoMedicamento.CadastrarRequisicaoEntrada(requisicaoEntrada);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult PrimeiraEtapaCadastrarRequisicaoSaida()
    {
        var funcionariosDisponiveis = repositorioFuncionario.SelecionarRegistros();

        var cadastrarVm = new PrimeiraEtapaCadastrarRequisicaoSaidaViewModel(funcionariosDisponiveis);

        return View(cadastrarVm);
    }

    [HttpPost]
    public IActionResult PrimeiraEtapaCadastrarRequisicaoSaida(PrimeiraEtapaCadastrarRequisicaoSaidaViewModel cadastrarVm)
    {
        var funcionarioSelecionado = repositorioFuncionario.SelecionarRegistroPorId(cadastrarVm.FuncionarioId);
        var pacienteSelecionado = repositorioPaciente.SelecionarPacientePorCpf(cadastrarVm.CpfPaciente);

        var prescricoesDoPaciente = repositorioPrescricao.SelecionarPrescricoesDoPaciente(pacienteSelecionado!.Id);

        var segundaEtapaVm = new SegundaEtapaCadastrarRequisicaoSaidaViewModel(
            cadastrarVm.FuncionarioId,
            funcionarioSelecionado.Nome,
            pacienteSelecionado!.Nome,
            prescricoesDoPaciente
        );

        return View(nameof(SegundaEtapaCadastrarRequisicaoSaida), segundaEtapaVm);
    }

    [HttpPost]
    public IActionResult SegundaEtapaCadastrarRequisicaoSaida(Guid FuncionarioId, string Paciente, Guid PrescricaoId)
    {
        var prescricaoSelecionada = repositorioPrescricao.SelecionarRegistroPorId(PrescricaoId);

        if (prescricaoSelecionada == null)
            return NotFound("Prescrição não encontrada");

        var medicamentos = prescricaoSelecionada.MedicamentosPrescritos;

        var ultimaEtapaVm = new TerceiraEtapaCadastrarRequisicaoSaidaViewModel(
            FuncionarioId,
            "",
            prescricaoSelecionada.Id,
            prescricaoSelecionada.Descricao,
            Paciente,
            prescricaoSelecionada.MedicamentosPrescritos
     );

        return View(nameof(TerceiraEtapaCadastrarRequisicaoSaida), ultimaEtapaVm);
    }

    [HttpPost]
    public IActionResult TerceiraEtapaCadastrarRequisicaoSaida(TerceiraEtapaCadastrarRequisicaoSaidaViewModel ultimaEtapaVm)
    {
        var funcionarioSelecionado = repositorioFuncionario.SelecionarRegistroPorId(ultimaEtapaVm.FuncionarioId);

        var prescricaoSelecionada = repositorioPrescricao.SelecionarRegistroPorId(ultimaEtapaVm.PrescricaoId);

        var requisicaoSaida = new RequisicaoSaida(funcionarioSelecionado, prescricaoSelecionada);

        foreach (var mp in prescricaoSelecionada.MedicamentosPrescritos)
        {
            var medicamento = mp.Medicamento;

            medicamento.RemoverDoEstoque(requisicaoSaida);
        }

        repositorioRequisicaoMedicamento.CadastrarRequisicaoSaida(requisicaoSaida);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult DetalhesRequisicaoEntrada(Guid id)
    {
        var entrada = repositorioRequisicaoMedicamento.SelecionarRequisicaoEntradaPorId(id);
        if (entrada == null) return NotFound();

        var vm = new DetalhesRequisicaoEntradaViewModel(entrada);
        return View(vm);
    }

    [HttpGet]
    public IActionResult DetalhesRequisicaoSaida(Guid id)
    {
        var saida = repositorioRequisicaoMedicamento.SelecionarRequisicaoSaidaPorId(id);
        if (saida == null) return NotFound();

        var vm = new DetalhesRequisicaoSaidaViewModel(saida);
        return View(vm);
    }
}
