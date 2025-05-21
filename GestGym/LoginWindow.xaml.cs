using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace GestGym;

public partial class LoginWindow : Window
{
    private readonly Controller _controller;
    private bool _loginSuccessful;

    [Experimental("WPF0001")]
    public LoginWindow()
    {
        InitializeComponent();
        _controller = new Controller();
        IdentifiantTextBox.Focus();

        // S'assurer que lorsque cette fenêtre se ferme sans authentification réussie,
        // l'application entière se ferme
        Closing += LoginWindow_Closing;
    }

    private void LoginWindow_Closing(object sender, CancelEventArgs e)
    {
        // Si la fenêtre se ferme sans authentification réussie, fermer l'application
        if (!_loginSuccessful) Application.Current.Shutdown();
    }

    private void ConnexionButton_Click(object sender, RoutedEventArgs e)
    {
        TenterAuthentification();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            MotDePassePasswordBox.Focus();
            e.Handled = true;
        }
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TenterAuthentification();
            e.Handled = true;
        }
    }

    private void TenterAuthentification()
    {
        var identifiant = IdentifiantTextBox.Text.Trim();
        var motDePasse = MotDePassePasswordBox.Password.Trim();

        // Validation de base
        if (string.IsNullOrEmpty(identifiant) || string.IsNullOrEmpty(motDePasse))
        {
            AfficherErreur("Veuillez saisir un identifiant et un mot de passe.");
            return;
        }

        try
        {
            // Tenter l'authentification
            var estAuthentifie = AuthentifierUtilisateur(identifiant, motDePasse);

            if (estAuthentifie)
            {
                // Marquer l'authentification comme réussie
                _loginSuccessful = true;

                // Créer et configurer la fenêtre principale
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;

                // Afficher la fenêtre principale
                mainWindow.Show();

                // Fermer la fenêtre de login
                Close();
            }
            else
            {
                // Authentification échouée
                AfficherErreur("Identifiant ou mot de passe incorrect.");
                MotDePassePasswordBox.Clear();
                MotDePassePasswordBox.Focus();
            }
        }
        catch (Exception ex)
        {
            AfficherErreur($"Erreur lors de l'authentification: {ex.Message}");
        }
    }

    private bool AuthentifierUtilisateur(string identifiant, string motDePasse)
    {
        // Utilisez votre Controller pour vérifier les identifiants
        return _controller.AuthentifierUtilisateur(identifiant, motDePasse);
    }

    private void AfficherErreur(string message)
    {
        ErreurTextBlock.Text = message;
        ErreurTextBlock.Visibility = Visibility.Visible;
    }
}