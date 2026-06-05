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
        public MainWindow()
        {
            InitializeComponent();

            PartidasService partidas = new();
            juegoService = new JuegoService(partidas);

            juegoService.Iniciar();
        }
    }
}