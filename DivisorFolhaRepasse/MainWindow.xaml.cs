using DivisorComprovanteRepasse.Models;
using DivisorComprovanteRepasse.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace DivisorComprovanteRepasse
{
    public partial class MainWindow : Window
    {
        private readonly PdfReaderService _pdfReaderService;
        private readonly PdfSplitService _pdfSplitService;
        private List<RepassePagina> _paginasLidas = new();

        public MainWindow()
        {
            InitializeComponent();

            _pdfReaderService = new PdfReaderService();
            _pdfReaderService = new PdfReaderService();
            _pdfSplitService = new PdfSplitService();
        }

        private void BtnSelecionarPdf_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Selecionar Comprovante de Repasse",
                Filter = "Arquivos PDF (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                TxtCaminhoPdf.Text = dialog.FileName;

                TxtStatus.Text = "PDF selecionado. Clique em LER PDF.";
            }
        }



        private void BtnLerPdf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCaminhoPdf.Text))
            {
                MessageBox.Show(
                    "Selecione primeiro o PDF de Comprovante de repasse.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                BtnLerPdf.IsEnabled = false;
                BtnSelecionarPdf.IsEnabled = false;

                TxtStatus.Text = "Lendo PDF...";

                //var resultados =
                //    _pdfReaderService.LerPdf(TxtCaminhoPdf.Text);

                _paginasLidas =
                    _pdfReaderService.LerPdf(TxtCaminhoPdf.Text);

                var resultados = _paginasLidas;

                GridResultados.ItemsSource = resultados;

                int total = resultados.Count;

                int encontrados = resultados.Count(x => x.NomeEncontrado);

                int naoEncontrados = total - encontrados;

                TxtStatus.Text =
                    $"Páginas: {total} | " +
                    $"Cooperados identificados: {encontrados} | " +
                    $"Não identificados: {naoEncontrados}";

                BtnDividirPdf.IsEnabled =
                    encontrados > 0;

                MessageBox.Show(
                    $"Leitura concluída!\n\n" +
                    $"Total de páginas: {total}\n" +
                    $"Cooperados identificados: {encontrados}\n" +
                    $"Não identificados: {naoEncontrados}",
                    "Leitura concluída",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocorreu um erro ao ler o PDF:\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                BtnLerPdf.IsEnabled = true;
                BtnSelecionarPdf.IsEnabled = true;
            }
        }



        private void BtnDividirPdf_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (_paginasLidas.Count == 0)
            {
                MessageBox.Show(
                    "Primeiro leia o PDF.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int total = _paginasLidas.Count;

            var identificados =
                _paginasLidas
                    .Where(x => x.NomeEncontrado)
                    .ToList();

            var naoIdentificados =
                _paginasLidas
                    .Where(x => !x.NomeEncontrado)
                    .ToList();

            if (identificados.Count == 0)
            {
                MessageBox.Show(
                    "Nenhum cooperado foi identificado.\n\n" +
                    "A divisão não pode ser realizada.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Se houver páginas não identificadas,
            // pergunta se o usuário deseja continuar.
            if (naoIdentificados.Count > 0)
            {
                string paginasNaoIdentificadas =
                    string.Join(
                        ", ",
                        naoIdentificados
                            .Select(x => x.NumeroPagina));

                string mensagem =
                    $"Foram identificadas {identificados.Count} " +
                    $"de {total} páginas.\n\n" +

                    $"Páginas não identificadas: " +
                    $"{naoIdentificados.Count}\n" +

                    $"Página(s): {paginasNaoIdentificadas}\n\n" +

                    "Os cooperados identificados serão divididos " +
                    "normalmente.\n\n" +

                    "As páginas não identificadas não serão geradas " +
                    "e deverão ser tratadas manualmente.\n\n" +

                    "Deseja continuar?";

                MessageBoxResult resposta =
                    MessageBox.Show(
                        mensagem,
                        "Páginas não identificadas",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                if (resposta != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            try
            {
                BtnDividirPdf.IsEnabled = false;

                TxtStatus.Text =
                    "Dividindo PDF...";

                string pastaSaida =
                    _pdfSplitService.DividirPdf(
                        TxtCaminhoPdf.Text,
                        _paginasLidas);

                TxtStatus.Text =
                    $"Divisão concluída! " +
                    $"Arquivos gerados: {identificados.Count} | " +
                    $"Não identificados: {naoIdentificados.Count}";

                string mensagemFinal =
                    $"Divisão concluída!\n\n" +

                    $"Total de páginas: {total}\n" +

                    $"Cooperados identificados: " +
                    $"{identificados.Count}\n" +

                    $"Arquivos gerados: " +
                    $"{identificados.Count}\n" +

                    $"Não identificados: " +
                    $"{naoIdentificados.Count}\n\n" +

                    $"Pasta de saída:\n" +
                    $"{pastaSaida}";

                if (naoIdentificados.Count > 0)
                {
                    string paginas =
                        string.Join(
                            ", ",
                            naoIdentificados
                                .Select(x => x.NumeroPagina));

                    mensagemFinal +=
                        $"\n\nPágina(s) para verificar manualmente:\n" +
                        $"{paginas}";
                }

                MessageBox.Show(
                    mensagemFinal,
                    "Divisão concluída",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                BtnAbrirPastaPdf.IsEnabled = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocorreu um erro ao dividir o PDF:\n\n" +
                    $"{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                BtnDividirPdf.IsEnabled =
                    _paginasLidas.Any(x => x.NomeEncontrado);
            }
        }

        private void BtnAbrirPasta_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCaminhoPdf.Text))
            {
                MessageBox.Show(
                    "Nenhum PDF foi selecionado.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            try
            {
                string? pastaPdf =
                    Path.GetDirectoryName(TxtCaminhoPdf.Text);

                if (string.IsNullOrWhiteSpace(pastaPdf) ||
                        !Directory.Exists(pastaPdf))
                {
                    MessageBox.Show(
                        "A pasta do PDF não foi encontrada.",
                        "Pasta não encontrada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pastaPdf,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Não foi possível abrir a pasta.\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }


        }





        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pastaManual = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Manual");

                string caminhoManual = Path.Combine(
                    pastaManual,
                    "Manual de Uso - Divisor de Comprovante de Repasse.pdf");

                if (!File.Exists(caminhoManual))
                {
                    MessageBox.Show(
                        "O manual de uso não foi encontrado.\n\n" +
                        "Verifique se o arquivo está dentro da pasta:\n\n" +
                        "Manual",
                        "Manual não encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = caminhoManual,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Não foi possível abrir o manual.\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }






    }
}