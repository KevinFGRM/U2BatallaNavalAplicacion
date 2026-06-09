using BatallaNavalServer.Models;
using BatallaNavalServer.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatallaNavalServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private JuegoService juegoService;
        private bool servidorIniciado = false;
        public MainWindow()
        {
            InitializeComponent();


            var partidasService = new PartidasService();
            juegoService = new JuegoService(partidasService);

            txtEstado.Text = "Servidor detenido";
        }
        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (servidorIniciado)
                return;

            try
            {
                juegoService.Iniciar();

                servidorIniciado = true;
                btnIniciar.IsEnabled = false;

                txtEstado.Text = "Servidor iniciado en http://+:8080/batallanaval/";
            }
            catch (Exception ex)
            {
                txtEstado.Text = $"Error: {ex.Message}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (servidorIniciado)
            {
                juegoService.Detener();
            }

            base.OnClosed(e);
        }
    }
}