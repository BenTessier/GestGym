using System.ComponentModel;
using System.Windows;

namespace GestGym;
#pragma warning disable WPF0001
public partial class MainWindow
{
    private readonly Controller _controller;

    public MainWindow()
    {
        InitializeComponent();
        _controller = new Controller();

        // Associer l'événement Closing à la méthode GestGym_Closing
        Closing += GestGym_Closing;

        // Get user settings
        _controller.LoadSettings();
    }

    private void Parametres_Click(object sender, RoutedEventArgs e)
    {
        // Crée une instance de la fenêtre des paramètres
        var _parametresWindows = new ParametresWindows(_controller);

        // Affiche la fenêtre en mode modal
        _parametresWindows.ShowDialog();
    }


    private void ConsulterUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Affiche la fiche utilisateur en mode lecture seule
        DynamicContent.Content = new GestionUtilisateur(_controller, false, true);
    }

    private void AjouterUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Mode ajout : ne pas afficher recherche/statut
        DynamicContent.Content = new GestionUtilisateur(_controller, false, false);
    }

    private void ModifierUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionUtilisateur(_controller, true, false);
    }

    private void ConsulterClient_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionClient(_controller, true, true);
    }

    private void AjouterClient_Click(object sender, RoutedEventArgs e)
    {
        DynamicContent.Content = new GestionClient(_controller, false, false);
    }

    private void ModifierClient_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionClient(_controller, true, false);
    }

    private void GestGym_Closing(object? sender, CancelEventArgs e)
    {
        _controller.SaveSettings();
        Application.Current.Shutdown();
    }
}
#pragma warning restore WPF0001