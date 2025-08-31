using ControleDeMedicamentos.Dominio.Compartilhado;
using System.Text.RegularExpressions;

namespace ControleDeMedicamentos.Dominio.ModuloPaciente;

public class Paciente : EntidadeBase<Paciente>
{
    public string Nome { get; set; }
    public string Telefone { get; set; }
    public string CartaoSus { get; set; }
    public string Cpf { get; set; }

    public Paciente() { }

    public Paciente(string nome, string telefone, string cartaoSus, string cpf) : this()
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Telefone = telefone;
        CartaoSus = cartaoSus;
        Cpf = cpf;
    }

    public override void AtualizarRegistro(Paciente registroEditado)
    {
        Nome = registroEditado.Nome;
        Telefone = registroEditado.Telefone;
        CartaoSus = registroEditado.CartaoSus;
        Cpf = registroEditado.Cpf;
    }

    public override string Validar()
    {
        string erros = "";

        if (string.IsNullOrWhiteSpace(Nome))
            erros += "O campo 'Nome' � obrigat�rio.\n";

        if (Nome.Length < 3 || Nome.Length > 100)
            erros += "O campo 'Nome' deve conter entre 3 e 100 caracteres.\n";

        if (!Regex.IsMatch(Telefone, @"^\(?\d{2}\)?\s?(9\d{4}|\d{4})-?\d{4}$"))
            erros += "O campo 'Telefone' � deve seguir o padr�o (DDD) 0000-0000 ou (DDD) 00000-0000.\n";

        if (!Regex.IsMatch(CartaoSus, @"^\d{3}\s?\d{4}\s?\d{4}\s?\d{4}$"))
            erros += "O campo 'Cart�o do Sus' � precisa conter 15 n�meros.\n";

        if (!Regex.IsMatch(Cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$"))
            erros += "O campo 'CPF' deve seguir o formato 000.000.000-00.\n";

        return erros;
    }
}