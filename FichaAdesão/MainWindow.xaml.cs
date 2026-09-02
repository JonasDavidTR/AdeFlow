using System.Windows;

namespace FichaAdesao;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnFichaAdesao_Click(
    object sender,
    RoutedEventArgs e)
    {
        var janela = new GeradorFichaCadastro.MainWindow();

        janela.Show();

        this.Hide();

        janela.Closed += (s, args) =>
        {
            this.Show();
        };
    }

    private void BtnInserirMatricula_Click(
    object sender,
    RoutedEventArgs e)
    {
        var janela = new InserirMatricula.MainWindow();

        janela.Show();

        this.Hide();

        janela.Closed += (s, args) =>
        {
            this.Show();
        };
    }
}