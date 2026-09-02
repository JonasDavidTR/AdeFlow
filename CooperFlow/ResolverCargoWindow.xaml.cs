using CooperFlow.Models;
using CooperFlow.Services;
using System.Collections.Generic;
using System.Windows;

namespace CooperFlow
{
    /// <summary>
    /// Lógica interna para ResolverCargoWindow.xaml
    /// </summary>
    public partial class ResolverCargoWindow : Window
    {
        private readonly List<CargoPendente> _cargos;

        private readonly CargoService _cargoService = new();


        public ResolverCargoWindow(List<CargoPendente> cargos)
        {
            InitializeComponent();

            _cargos = cargos;


            foreach (var cargo in cargos)
            {
                if (!string.IsNullOrWhiteSpace(cargo.CargoSugerido))
                    cargo.CargoPadrao = cargo.CargoSugerido;

                if (!string.IsNullOrWhiteSpace(cargo.CooperativaSugerida))
                    cargo.CooperativaSelecionada = cargo.CooperativaSugerida;
            }


            dgCargos.ItemsSource = _cargos;
        }


        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cargo in _cargos)
            {
                if (string.IsNullOrWhiteSpace(cargo.CargoPadrao))
                {
                    MessageBox.Show(
                        $"Informe o cargo padrão de {cargo.CargoOriginal}");
                    return;
                }


                if (string.IsNullOrWhiteSpace(cargo.CooperativaSelecionada))
                {
                    MessageBox.Show(
                        $"Informe a cooperativa de {cargo.CargoOriginal}");
                    return;
                }


                _cargoService.AdicionarCargo(
                    cargo.CargoOriginal,
                    cargo.CargoPadrao,
                    cargo.CooperativaSelecionada
                );
            }


            MessageBox.Show("Cargos salvos com sucesso!");
            DialogResult = true;

            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var resposta = MessageBox.Show(
                "Deseja realmente cancelar o processamento?\nOs cargos não salvos serão perdidos.",
                "CooperFlow",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }


    }
}
