using CooperFlow.Models;
using CooperFlow.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CooperFlow;

public partial class MainWindow : Window
{
    private string? _arquivoSelecionado;

    private readonly ProcessadorFolhaService _processador = new();

    private readonly ExcelExporterService _exportador = new();


    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnSelecionar_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new();

        dialog.Filter = "Planilhas Excel (*.xlsx)|*.xlsx";

        if (dialog.ShowDialog() == true)
        {
            _arquivoSelecionado = dialog.FileName;

            TxtArquivo.Text = Path.GetFileName(_arquivoSelecionado);

            BtnProcessar.IsEnabled = true;
        }
    }

    private void BtnProcessar_Click(object sender, RoutedEventArgs e)
    {
        if (_arquivoSelecionado == null)
            return;

        // Primeira classificação
        //var resultado = _processador.Processar(_arquivoSelecionado);
        ResultadoProcessamento resultado;

        try
        {
            resultado = _processador.Processar(_arquivoSelecionado);
        }
        catch (IOException)
        {
            MessageBox.Show(
                "Não foi possível ler a planilha. \n\n" +
                "O arquivo estar aberto ou sendo utilizado por outro programa.\n\n" +
                "Feche a planilha e tente novamente.",
                "Arquivo em uso",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ocorreu um erro ao processar a planilha:\n\n{ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }


        // Resolve os cargos desconhecidos
        if (resultado.CargosPendentes.Any())
        {
            var janela = new ResolverCargoWindow(resultado.CargosPendentes);

            janela.Owner = this;

            bool? resultadoJanela = janela.ShowDialog();

            if (resultadoJanela != true)
            {
                return;
            }

       

            try
            {
                // Atualiza o CargoService com os cargos recém-salvos
                _processador.RecarregarCargos();
                // Lê a planilha novamente aplicando as novas classificações
                resultado = _processador.Processar(_arquivoSelecionado);
            }
            catch (IOException)
            {
                MessageBox.Show(
                    "Não foi possível reler a planilha.\n\n" +
                    "O arquivo pode está aberto ou sendo utilizado por outro programa.\n\n" +
                    "Feche a planilha e tente novamente.",
                    "Arquivo em uso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocorreu um erro ao processar a planilha:\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }


        }

        // Pasta Output
        string pastaOutput = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Output");

        Directory.CreateDirectory(pastaOutput);



        MessageBox.Show(
            $"Solução: {resultado.LinhasSolucao.Count}\n" +
            $"Mais Saúde: {resultado.LinhasMaisSaude.Count}\n" +
            $"Desconhecidos: {resultado.LinhasDesconhecidos.Count}"
        );


        try
        {
            // Exporta Solução
            _exportador.Exportar(
                _arquivoSelecionado,
                resultado.LinhasSolucao,
                resultado.LinhaCabecalho,
                Path.Combine(pastaOutput, "SOLUCAO.xlsx"));


            // Exporta Mais Saúde
            _exportador.Exportar(
                _arquivoSelecionado,
                resultado.LinhasMaisSaude,
                resultado.LinhaCabecalho,
                Path.Combine(pastaOutput, "MAIS_SAUDE.xlsx"));

            _exportador.Exportar(
                _arquivoSelecionado,
                resultado.LinhasDesconhecidos,
                resultado.LinhaCabecalho,
                Path.Combine(pastaOutput, "PENDENCIAS.xlsx"));
        }
        catch (IOException)
        {
            MessageBox.Show(
                "Não foi possível gerar as planilhas.\n\n" +
                "Verifique se alguma das planilhas de saída está aberta no Excel " +
                "ou sendo utilizada por outro programa.\n\n" +
                "Feche os arquivos e tente novamente.",
                "Arquivo em uso",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ocorreu um erro ao gerar as planilhas:\n\n{ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }













        BtnAbrirPasta.IsEnabled = true;

        MessageBox.Show(
            "✔ Processamento concluído!\r\n\r\nPlanilhas geradas:\r\n\r\n• MAIS_SAUDE.xlsx\r\n• SOLUCAO.xlsx\r\n•PENDENCIAS.xlsx\r\n\r\nClique em \"Abrir Pasta\" para visualizar.",
            "BaseFlow",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

    }

    private void BtnAbrirPasta_Click(object sender, RoutedEventArgs e)
    {
        string pastaOutput = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Output");

        if (!Directory.Exists(pastaOutput))
            Directory.CreateDirectory(pastaOutput);

        Process.Start(new ProcessStartInfo
        {
            FileName = pastaOutput,
            UseShellExecute = true
        });
    }



    private void BtnManual_Click(object sender, RoutedEventArgs e)
    {
        string caminhoManual = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Manual",
            "Manual_BaseFlow.pdf");

        if (!File.Exists(caminhoManual))
        {
            MessageBox.Show(
                "O manual não foi encontrado.",
                "BaseFlow",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = caminhoManual,
            UseShellExecute = true
        });
    }







}