using Microsoft.Win32;
using InserirMatricula.Services;
using System.IO;
using System.Windows;

namespace InserirMatricula;

public partial class MainWindow : Window
{
    private string? arquivoDominio;
    private string? arquivoFolha;

    public MainWindow()
    {
        InitializeComponent();

        // Começa desabilitado
        BtnProcessar.IsEnabled = false;
        BtnAbrirPastaResultados.IsEnabled = false;
    }


    private void SelecionarDominio_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            arquivoDominio = dialog.FileName;

            txtDominio.Text =
                Path.GetFileName(arquivoDominio);

            AtualizarEstadoBotoes();
        }
    }


    private void SelecionarFolha_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            arquivoFolha = dialog.FileName;

            txtFolha.Text =
                Path.GetFileName(arquivoFolha);

            AtualizarEstadoBotoes();
        }
    }


    private void AtualizarEstadoBotoes()
    {
        BtnProcessar.IsEnabled =
            !string.IsNullOrWhiteSpace(arquivoDominio) &&
            !string.IsNullOrWhiteSpace(arquivoFolha);
    }


    private void Processar_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(arquivoDominio))
        {
            MessageBox.Show(
                "Selecione o arquivo do Domínio.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        if (string.IsNullOrWhiteSpace(arquivoFolha))
        {
            MessageBox.Show(
                "Selecione a folha.",
                "Atenção",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }





        // TESTE DE FUNCIONALIDADE ( INDENTIFICAÇÃO DE FORMATO DE FOLHA )

        //var tipoPlanilhaService =
        //    new TipoPlanilhaService();

        //var tipo =
        //    tipoPlanilhaService.Identificar(
        //        arquivoFolha);


        //MessageBox.Show(
        //    $"Tipo identificado: {tipo}",
        //    "Teste",
        //    MessageBoxButton.OK,
        //    MessageBoxImage.Information);

        //return;







        try
        {
            // Evita processamento duplicado
            BtnProcessar.IsEnabled = false;


            var reader =
                new ExcelReaderService();

            var funcionarios =
                reader.LerDominio(
                    arquivoDominio);


            var matriculaService =
                new MatriculaService(
                    funcionarios);


            var comparacaoNomeService =
                new ComparacaoNomeService(
                    funcionarios);


            var processor = new CadastroProcessorService();
                //new ExcelProcessorService();                       SISTEMA MIGRADO PARA SETOR DE CADASTRO (CadastroProcessorService.cs)


            var resultado =
                processor.Processar(
                    arquivoFolha,
                    matriculaService,
                    comparacaoNomeService);


            var relatorioService =
                new RelatorioConferenciaService();


            string arquivoRelatorio =
                relatorioService.Gerar(
                    arquivoFolha,
                    resultado);


            // Processamento concluído com sucesso
            BtnAbrirPastaResultados.IsEnabled = true;


            MessageBox.Show(
                $"""
                PROCESSAMENTO CONCLUÍDO

                Matrículas inseridas:
                {resultado.MatriculasPreenchidas}

                Sugestões para conferência:
                {resultado.Sugestoes.Count}

                CPFs duplicados no Domínio:
                {resultado.Duplicidades}

                Não encontrados:
                {resultado.Pendencias.Count}

                --------------------------------

                Arquivo processado:
                {resultado.ArquivoGerado}

                Relatório de conferência:
                {arquivoRelatorio}
                """,
                "Resultado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // Se ocorrer erro, permite tentar novamente
            BtnProcessar.IsEnabled = true;

            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    private void BtnAbrirPastaResultados_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(arquivoFolha))
            return;

        string pasta =
            Path.GetDirectoryName(arquivoFolha)!;

        if (!Directory.Exists(pasta))
            return;

        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = pasta,
                UseShellExecute = true
            });
    }
}