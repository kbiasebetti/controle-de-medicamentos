using ControleDeMedicamentos.Dominio.ModuloMedicamento;
using ControleDeMedicamentos.Dominio.ModuloPrescricao;
using ControleDeMedicamentos.Infraestrutura.Arquivos.Compartilhado;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloMedicamento;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloPaciente;
using ControleDeMedicamentos.Infraestrutura.Arquivos.ModuloPrescricao;
using ControleDeMedicamentos.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace ControleDeMedicamentos.WebApp.Controllers;

public class PrescricaoController : Controller
{
    private readonly RepositorioPrescricaoEmArquivo repositorioPrescricao;
    private readonly RepositorioMedicamentoEmArquivo repositorioMedicamento;
    private readonly RepositorioPacienteEmArquivo repositorioPaciente;

    public PrescricaoController(
        RepositorioPrescricaoEmArquivo repositorioPrescricao,
        RepositorioMedicamentoEmArquivo repositorioMedicamento,
        RepositorioPacienteEmArquivo repositorioPaciente
    )
    {
        this.repositorioPrescricao = repositorioPrescricao;
        this.repositorioMedicamento = repositorioMedicamento;
        this.repositorioPaciente = repositorioPaciente;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var prescricoes = repositorioPrescricao.SelecionarRegistros();
        var visualizarVm = new VisualizarPrescricoesViewModel(prescricoes);
        return View(visualizarVm);
    }

    [HttpGet]
    public IActionResult Cadastrar()
    {
        var pacientesDisponiveis = repositorioPaciente.SelecionarRegistros();
        var cadastrarVm = new CadastrarPrescricaoViewModel(pacientesDisponiveis);
        return View(cadastrarVm);
    }

    [HttpPost]
    public IActionResult Cadastrar(CadastrarPrescricaoViewModel cadastrarVm)
    {
        if (!ModelState.IsValid)
        {
            var pacientes = repositorioPaciente.SelecionarRegistros();
            cadastrarVm.PacientesDisponiveis = pacientes
                .Select(p => new SelectListItem(p.Nome, p.Id.ToString())).ToList();
            return View(cadastrarVm);
        }

        var pacienteSelecionado = repositorioPaciente.SelecionarRegistroPorId(cadastrarVm.PacienteId);

        var entidade = new Prescricao(
            cadastrarVm.Descricao,
            cadastrarVm.DataValidade,
            cadastrarVm.CrmMedico,
            pacienteSelecionado
        );

        repositorioPrescricao.CadastrarRegistro(entidade);
        TempData["MensagemSucesso"] = "Prescrição cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Editar(Guid id)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(id);
        if (prescricao == null)
            return NotFound();

        var pacientes = repositorioPaciente.SelecionarRegistros();
        var editarVm = new EditarPrescricaoViewModel(prescricao, pacientes);
        return View(editarVm);
    }

    [HttpPost]
    public IActionResult Editar(EditarPrescricaoViewModel editarVm)
    {
        if (!ModelState.IsValid)
        {
            var pacientes = repositorioPaciente.SelecionarRegistros();
            editarVm.PacientesDisponiveis = pacientes
                .Select(p => new SelectListItem(p.Nome, p.Id.ToString())).ToList();
            return View(editarVm);
        }

        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(editarVm.Id);
        if (prescricao == null)
            return NotFound();

        var pacienteSelecionado = repositorioPaciente.SelecionarRegistroPorId(editarVm.PacienteId);

        prescricao.Descricao = editarVm.Descricao;
        prescricao.DataValidade = editarVm.DataValidade;
        prescricao.CrmMedico = editarVm.CrmMedico;
        prescricao.Paciente = pacienteSelecionado;

        repositorioPrescricao.EditarRegistro(prescricao.Id, prescricao);
        TempData["MensagemSucesso"] = "Prescrição editada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Excluir(Guid id)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(id);
        if (prescricao == null)
            return NotFound();

        var viewModel = new ExcluirPrescricaoViewModel(
            prescricao.Id,
            prescricao.Paciente.Nome
        );
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Excluir(ExcluirPrescricaoViewModel excluirVm)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(excluirVm.Id);
        if (prescricao != null)
        {
            repositorioPrescricao.ExcluirRegistro(excluirVm.Id);
            TempData["MensagemSucesso"] = "Prescrição excluída com sucesso!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Gerenciar(Guid id)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(id);
        if (prescricao == null)
            return NotFound();

        var todosOsMedicamentos = repositorioMedicamento.SelecionarRegistros();
        var viewModel = new GerenciarPrescricaoViewModel(prescricao, todosOsMedicamentos);

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult AdicionarMedicamentoPrescrito(AdicionarMedicamentoPrescritoViewModel viewModel)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(viewModel.PrescricaoId);
        var medicamento = repositorioMedicamento.SelecionarRegistroPorId(viewModel.MedicamentoId);

        if (prescricao != null && medicamento != null)
        {
            prescricao.AdicionarMedicamentoPrescrito(
                medicamento,
                viewModel.Dosagem,
                viewModel.Periodo,
                viewModel.Quantidade
            );

            repositorioPrescricao.EditarRegistro(prescricao.Id, prescricao);
        }

        return RedirectToAction("Gerenciar", new { id = viewModel.PrescricaoId });
    }

    [HttpPost]
    public IActionResult RemoverMedicamentoPrescrito(Guid idPrescricao, Guid idMedicamentoPrescrito)
    {
        var prescricao = repositorioPrescricao.SelecionarRegistroPorId(idPrescricao);
        if (prescricao != null)
        {
            prescricao.RemoverMedicamentoPrescrito(idMedicamentoPrescrito);
            repositorioPrescricao.EditarRegistro(prescricao.Id, prescricao);
        }
        return RedirectToAction("Gerenciar", new { id = idPrescricao });
    }
}