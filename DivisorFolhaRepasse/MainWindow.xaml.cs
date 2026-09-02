using DivisorFolhaRepasse.Models;
using DivisorFolhaRepasse.Services;
using Microsoft.Win32;
using System.Windows;

namespace DivisorFolhaRepasse
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
                Title = "Selecionar Folha de Repasse",
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
                    "Selecione primeiro o PDF da folha de repasse.",
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
                    $"PDF dividido com sucesso! " +
                    $"Arquivos salvos em: {pastaSaida}";

                MessageBox.Show(
                    $"PDF dividido com sucesso!\n\n" +
                    $"Os arquivos foram salvos em:\n\n" +
                    $"{pastaSaida}",
                    "Divisão concluída",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocorreu um erro ao dividir o PDF:\n\n{ex.Message}",
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


    }
}