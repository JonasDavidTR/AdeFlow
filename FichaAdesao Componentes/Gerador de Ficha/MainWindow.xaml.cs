using GeradorFichaCadastro.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace GeradorFichaCadastro;

public partial class MainWindow : Window
{
    private string? _caminhoModelo;
    private string? _caminhoDominio;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnSelecionarModelo_Click(
    object sender,
    RoutedEventArgs e)
    {
        var pastaTemplates =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Templates");

        Directory.CreateDirectory(pastaTemplates);

        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar modelo da ficha",
            Filter = "Documento Word (*.docx)|*.docx",
            InitialDirectory = pastaTemplates
        };

        if (dialogo.ShowDialog() == true)
        {
            _caminhoModelo = dialogo.FileName;
            TxtModelo.Text = _caminhoModelo;

            TxtStatus.Text = "Modelo selecionado.";
        }
    }

    private void BtnSelecionarDominio_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Selecionar relatório do DOMÍNIO",
            Filter = "Arquivos Excel (*.xlsx)|*.xlsx"
        };

        if (dialogo.ShowDialog() == true)
        {
            _caminhoDominio = dialogo.FileName;
            TxtDominio.Text = _caminhoDominio;

            TxtStatus.Text = "Relatório do DOMÍNIO selecionado.";
        }
    }

    private void BtnValidarModelo_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_caminhoModelo))
        {
            MessageBox.Show(
                "Selecione o modelo da ficha primeiro.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var validator = new TemplateValidatorService();

        var resultado = validator.Validar(_caminhoModelo);

        if (resultado.Erros.Count > 0)
        {
            TxtStatus.Text = string.Join(
                Environment.NewLine,
                resultado.Erros);

            return;
        }

        if (resultado.Valido)
        {
            var mensagem = "✓ MODELO VÁLIDO!" +
                           Environment.NewLine +
                           Environment.NewLine +
                           "Campos encontrados:" +
                           Environment.NewLine;

            if (resultado.CamposEncontrados.Count > 0)
            {
                mensagem += string.Join(
                    Environment.NewLine,
                    resultado.CamposEncontrados);
            }
            else
            {
                mensagem += "Nenhum.";
            }

            mensagem += Environment.NewLine +
                        Environment.NewLine +
                        "Marcadores desconhecidos:" +
                        Environment.NewLine;

            if (resultado.MarcadoresDesconhecidos.Count > 0)
            {
                mensagem += string.Join(
                    Environment.NewLine,
                    resultado.MarcadoresDesconhecidos);
            }
            else
            {
                mensagem += "Nenhum.";
            }

            TxtStatus.Text = mensagem;

            MessageBox.Show(
                "O modelo foi validado com sucesso!",
                "Modelo válido",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            TxtStatus.Text =
                "❌ Modelo inválido." +
                Environment.NewLine +
                Environment.NewLine +
                "Campos obrigatórios ausentes:" +
                Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    resultado.CamposObrigatoriosAusentes);

            MessageBox.Show(
                "Existem campos obrigatórios ausentes no modelo.",
                "Modelo inválido",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }




    private void BtnLerDominio_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_caminhoDominio))
        {
            MessageBox.Show(
                "Selecione o arquivo do DOMÍNIO primeiro.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            //var reader = new DominioReaderService();
            var reader = new DadosCadastraisReaderService();




            //var diagnostico = reader.DiagnosticarEnderecos(_caminhoDominio);

            //MessageBox.Show(
            //    diagnostico,
            //    "Diagnóstico dos Endereços",
            //    MessageBoxButton.OK,
            //    MessageBoxImage.Information);

            //return;










            var fichas = reader.Ler(_caminhoDominio);

            if (fichas.Count == 0)
            {
                TxtStatus.Text =
                    "Nenhum cooperado foi encontrado no arquivo do DOMÍNIO.";

                return;
            }

            var mensagem =
                $"✓ DOMÍNIO LIDO COM SUCESSO!" +
                Environment.NewLine +
                Environment.NewLine +
                $"Cooperados encontrados: {fichas.Count}" +
                Environment.NewLine +
                Environment.NewLine;

            foreach (var ficha in fichas.Take(5))
            {
                mensagem +=
                    $"Matrícula: {ficha.Matricula}" +
                    Environment.NewLine +
                    $"Nome: {ficha.Nome}" +
                    Environment.NewLine +
                    $"Endereço: {ficha.Endereco}" +
                    Environment.NewLine +
                    $"Número: {ficha.Numero}" +
                    Environment.NewLine +
                    $"Bairro: {ficha.Bairro}" +
                    Environment.NewLine +
                    $"Cidade: {ficha.Cidade}" +
                    Environment.NewLine +
                    $"CEP: {ficha.CEP}" +
                    Environment.NewLine +
                    $"CPF: {ficha.CPF}" +
                    Environment.NewLine +
                    $"Data de Nascimento: {ficha.DataNascimento}" +
                    Environment.NewLine +
                    $"Naturalidade: {ficha.Naturalidade}" +
                    Environment.NewLine +
                    $"UF Naturalidade: {ficha.NaturalidadeUF}" +
                    Environment.NewLine +
                    $"Nacionalidade: {ficha.Nacionalidade}" +
                    Environment.NewLine +
                    $"RG: {ficha.RG}" +
                    Environment.NewLine +
                    $"Órgão/UF RG: {ficha.RGOrgaoUF}" +
                    Environment.NewLine +
                    $"Emissão RG: {ficha.RGEmissao}" +
                    Environment.NewLine +
                    $"Estado Civil: {ficha.EstadoCivil}" +
                    Environment.NewLine +
                    $"Regime: {ficha.Regime}" +
                    Environment.NewLine +
                    $"Cônjuge: {ficha.Conjuge}" +
                    Environment.NewLine +
                    $"CPF do Cônjuge: {ficha.ConjugeCPF}" +
                    Environment.NewLine +
                    $"Profissão: {ficha.Profissao}" +
                    Environment.NewLine +
                    $"Número de Registro: {ficha.Registro}" +
                    Environment.NewLine +
                    $"UF do Registro: {ficha.RegistroUF}" +
                    Environment.NewLine +
                    $"Lotação: {ficha.Lotacao}" +
                    Environment.NewLine +
                    $"Telefone: {ficha.Telefone}" +
                    Environment.NewLine +
                    $"E-mail: {ficha.Email}" +
                    Environment.NewLine +
                    $"Banco: {ficha.Banco}" +
                    Environment.NewLine +
                    $"Agência: {ficha.Agencia}" +
                    Environment.NewLine +
                    $"Conta: {ficha.Conta}" +
                    Environment.NewLine +
                    $"Tipo de Conta: {ficha.TipoConta}" +
                    Environment.NewLine +
                    $"PIX: {ficha.Pix}" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "--------------------------------" +
                    Environment.NewLine +
                    Environment.NewLine;
            }

            if (fichas.Count > 5)
            {
                mensagem +=
                    $"... e mais {fichas.Count - 5} cooperados.";
            }

            TxtStatus.Text = mensagem;
        }
        catch (Exception ex)
        {
            TxtStatus.Text =
                $"Erro ao ler o DOMÍNIO:{Environment.NewLine}{ex.Message}";

            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }




private void BtnGerarFichas_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_caminhoModelo))
        {
            MessageBox.Show(
                "Selecione o modelo da ficha primeiro.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (string.IsNullOrWhiteSpace(_caminhoDominio))
        {
            MessageBox.Show(
                "Selecione a planilha de Dados Cadastrais primeiro.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            var reader =
                new DadosCadastraisReaderService();

            var fichas =
                reader.Ler(_caminhoDominio);

            if (fichas.Count == 0)
            {
                MessageBox.Show(
                    "Nenhum cooperado foi encontrado na planilha.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            /*
             * Cria uma pasta ao lado da planilha.
             */
            var pastaBase =
                Path.GetDirectoryName(
                    _caminhoDominio);

            if (string.IsNullOrWhiteSpace(pastaBase))
                pastaBase = AppDomain.CurrentDomain.BaseDirectory;

            var pastaSaida =
                Path.Combine(
                    pastaBase,
                    "Fichas Geradas");

            Directory.CreateDirectory(pastaSaida);

            var writer =
                new FichaAdesaoWriter();

            int geradas = 0;

            
            foreach (var ficha in fichas)
            {
                var nomeBase =
                    LimparNomeArquivo(
                        $"{ficha.Matricula} - {ficha.Nome}");

                if (string.IsNullOrWhiteSpace(nomeBase))
                {
                    nomeBase = $"Ficha_{geradas + 1}";
                }

                var destino =
                    Path.Combine(
                        pastaSaida,
                        $"{nomeBase}.docx");

                /*
                 * Evita que uma ficha sobrescreva outra
                 * caso matrícula/nome estejam repetidos.
                 */
                int contador = 2;

                while (File.Exists(destino))
                {
                    destino =
                        Path.Combine(
                            pastaSaida,
                            $"{nomeBase} ({contador}).docx");

                    contador++;
                }

                writer.Preencher(
                    _caminhoModelo,
                    destino,
                    ficha);

                geradas++;
            }



            TxtStatus.Text =
                $"✓ FICHAS GERADAS COM SUCESSO!" +
                Environment.NewLine +
                Environment.NewLine +
                $"Fichas geradas: {geradas}" +
                Environment.NewLine +
                Environment.NewLine +
                $"Pasta de saída:" +
                Environment.NewLine +
                pastaSaida;

            MessageBox.Show(
                $"Foram geradas {geradas} fichas.",
                "Fichas geradas",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            TxtStatus.Text =
                $"Erro ao gerar as fichas:" +
                Environment.NewLine +
                ex.Message;

            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    private string LimparNomeArquivo(
        string nome)
    {
        var caracteresInvalidos =
            Path.GetInvalidFileNameChars();

        foreach (var caractere in caracteresInvalidos)
        {
            nome =
                nome.Replace(
                    caractere.ToString(),
                    "");
        }

        return nome.Trim();
    }



    private void BtnCadastroIndividual_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_caminhoModelo))
        {
            MessageBox.Show(
                "Selecione o modelo da ficha primeiro.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var janela = new CadastroIndividualWindow(
            _caminhoModelo)
        {
            Owner = this
        };

        janela.ShowDialog();
    }





}