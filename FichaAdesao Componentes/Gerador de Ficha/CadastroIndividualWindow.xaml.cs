using GeradorFichaCadastro.Data;
using GeradorFichaCadastro.Models;
using GeradorFichaCadastro.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GeradorFichaCadastro;

public partial class CadastroIndividualWindow : Window
{
    private readonly List<CampoFicha> _campos;

    private readonly string _caminhoModelo;

    public CadastroIndividualWindow(string caminhoModelo)
    {
        InitializeComponent();

        _caminhoModelo = caminhoModelo;

        _campos = CamposFicha.Todos();

        CamposFormulario.ItemsSource = _campos;
    }


    private void BtnCancelar_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }


    private void BtnGerar_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            // 1. Ler os valores preenchidos
            // =============================================

            var valores = new Dictionary<string, string?>();

            foreach (var item in CamposFormulario.Items)
            {
                if (item is not CampoFicha campo)
                    continue;

                var container =
                    CamposFormulario.ItemContainerGenerator
                        .ContainerFromItem(item);

                if (container == null)
                    continue;

                var textBox =
                    EncontrarTextBox(container);

                if (textBox == null)
                    continue;

                valores[campo.Marcador] =
                    textBox.Text.Trim();
            }



            // 2. Validar campos obrigatórios
            // =============================================

            var camposObrigatoriosAusentes =
                _campos
                    .Where(c =>
                        c.Obrigatorio &&
                        string.IsNullOrWhiteSpace(
                            valores.GetValueOrDefault(c.Marcador)))
                    .ToList();

            if (camposObrigatoriosAusentes.Count > 0)
            {
                var mensagem =
                    "Preencha os campos obrigatórios:" +
                    Environment.NewLine +
                    Environment.NewLine +
                    string.Join(
                        Environment.NewLine,
                        camposObrigatoriosAusentes
                            .Select(c => $"• {c.Nome}"));

                MessageBox.Show(
                    mensagem,
                    "Campos obrigatórios",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }



            // 3. Criar FichaAdesao
            // =============================================

            var ficha = CriarFicha(valores);



            // 4. Selecionar o modelo (LEGADO)
            // =============================================

            //var dialogoModelo = new OpenFileDialog
            //{
            //    Title = "Selecionar modelo da ficha",
            //    Filter = "Documento Word (*.docx)|*.docx"
            //};

            //if (dialogoModelo.ShowDialog(this) != true)
            //    return;



            // 5. Escolher destino
            // =============================================

            var nomeArquivo =
                string.IsNullOrWhiteSpace(ficha.Nome)
                    ? "Ficha_Individual.docx"
                    : $"Ficha_{SanitizarNomeArquivo(ficha.Nome)}.docx";

            var dialogoDestino = new SaveFileDialog
            {
                Title = "Salvar ficha de cadastro",
                Filter = "Documento Word (*.docx)|*.docx",
                FileName = nomeArquivo
            };

            if (dialogoDestino.ShowDialog(this) != true)
                return;



            // 6. Gerar ficha
            // =============================================

            var writer = new FichaAdesaoWriter();

            //writer.Preencher(
            //    dialogoModelo.FileName,
            //    dialogoDestino.FileName,
            //    ficha);

            // ler Modelo definido na tela inicial
            writer.Preencher(
                _caminhoModelo,
                dialogoDestino.FileName,
                ficha);



            // 7. Sucesso
            // =============================================

            MessageBox.Show(
                $"Ficha gerada com sucesso!\n\n" +
                $"Arquivo:\n{dialogoDestino.FileName}",
                "Ficha gerada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Erro ao gerar ficha",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    private FichaAdesao CriarFicha(
        Dictionary<string, string?> valores)
    {
        string Valor(string marcador)
        {
            return valores.GetValueOrDefault(marcador)
                   ?? string.Empty;
        }

        return new FichaAdesao
        {
            Matricula = Valor("{{MATRICULA}}"),
            Nome = Valor("{{NOME}}"),

            Endereco = Valor("{{ENDERECO}}"),
            Numero = Valor("{{NUMERO}}"),
            Bairro = Valor("{{BAIRRO}}"),
            Cidade = Valor("{{CIDADE}}"),
            CEP = Valor("{{CEP}}"),

            CPF = Valor("{{CPF}}"),

            DataNascimento =
                Valor("{{DATA_NASCIMENTO}}"),

            Naturalidade =
                Valor("{{NATURALIDADE}}"),

            NaturalidadeUF =
                Valor("{{NATURALIDADE_UF}}"),

            Nacionalidade =
                Valor("{{NACIONALIDADE}}"),

            RG = Valor("{{RG}}"),

            RGOrgaoUF =
                Valor("{{RG_ORGAO_UF}}"),

            RGEmissao =
                Valor("{{RG_EMISSAO}}"),

            EstadoCivil =
                Valor("{{ESTADO_CIVIL}}"),

            Regime =
                Valor("{{REGIME}}"),

            Conjuge =
                Valor("{{CONJUGE}}"),

            ConjugeCPF =
                Valor("{{CONJUGE_CPF}}"),

            Profissao =
                Valor("{{PROFISSAO}}"),

            Registro =
                Valor("{{REGISTRO}}"),

            RegistroUF =
                Valor("{{REGISTRO_UF}}"),

            Lotacao =
                Valor("{{LOTACAO}}"),

            Telefone =
                Valor("{{TELEFONE}}"),

            Email =
                Valor("{{EMAIL}}"),

            Banco =
                Valor("{{BANCO}}"),

            Agencia =
                Valor("{{AGENCIA}}"),

            Conta =
                Valor("{{CONTA}}"),

            TipoConta =
                Valor("{{TIPO_CONTA}}"),

            Pix =
                Valor("{{PIX}}")
        };
    }


    private TextBox? EncontrarTextBox(
        DependencyObject elemento)
    {
        if (elemento is TextBox textBox)
            return textBox;

        for (int i = 0;
             i < System.Windows.Media.VisualTreeHelper
                    .GetChildrenCount(elemento);
             i++)
        {
            var filho =
                System.Windows.Media.VisualTreeHelper
                    .GetChild(elemento, i);

            var resultado =
                EncontrarTextBox(filho);

            if (resultado != null)
                return resultado;
        }

        return null;
    }


    private string SanitizarNomeArquivo(
        string nome)
    {
        var caracteresInvalidos =
            Path.GetInvalidFileNameChars();

        return string.Concat(
                nome.Select(c =>
                    caracteresInvalidos.Contains(c)
                        ? '_'
                        : c))
            .Trim();
    }
}
