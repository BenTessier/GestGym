using System.ComponentModel;
using System.Windows;
using Windows.UI.Composition.Interactions;

namespace GestGym;
#pragma warning disable WPF0001
public partial class MainWindow : Window
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
        // Crée une instance de la fen�tre des param�tres
        var _parametresWindows = new ParametresWindows(_controller);

        // Affiche la fen�tre en mode modal
        _parametresWindows.ShowDialog();
    }

    private void AjouterUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        //var ecranUtilisateur = new AjouterUtilisateur(); // Mode ajout
        //DynamicContent.Content = ecranUtilisateur;
        DynamicContent.Content = new AjouterUtilisateur(_controller);
    }

    private void ModifierUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        //var ecranUtilisateur = new ModifierUtilisateur(); // Mode Modification
        //DynamicContent.Content = ecranUtilisateur;
        DynamicContent.Content = new ModifierUtilisateur(_controller);
    }


    private void GestGym_Closing(object? sender, CancelEventArgs e)
    {
        _controller.SaveSettings();
        Application.Current.Shutdown();
    }

    private void LightThemeRadioButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.ThemeMode = ThemeMode.Light;
    }

    private void DarkThemeRadioButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.ThemeMode = ThemeMode.Dark;
    }

    private void SystemThemeRadioButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.ThemeMode = ThemeMode.System;
    }
}
#pragma warning restore WPF0001