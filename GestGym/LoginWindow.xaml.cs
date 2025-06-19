/*********************************************************************
 * FICHIER:        LoginWindow.xaml.cs
 * DESCRIPTION:    Fenêtre d'authentification de l'application GestGym
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace GestGym;

/// <summary>
///     Classe représentant la fenêtre de connexion pour authentifier les utilisateurs
///     avant d'accéder à l'application principale.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly Controller _controller;
    private bool _loginSuccessful;

    /*********************************************************************
     * CONSTRUCTEUR: LoginWindow
     * DESCRIPTION:  Initialise la fenêtre de connexion et configure les
     *               événements nécessaires à son fonctionnement
     *********************************************************************/
    [Experimental("WPF0001")]
    public LoginWindow()
    {
        InitializeComponent();
        _controller = new Controller();

        // Forcer les dimensions spécifiées dans le XAML
        Width = 400;
        Height = 450;
        SizeToContent = SizeToContent.Manual;

        IdentifiantTextBox.Focus();

        // S'assurer que lorsque cette fenêtre se ferme sans authentification réussie,
        // l'application entière se ferme
        Closing += LoginWindow_Closing;
    }

    /*********************************************************************
     * MÉTHODE:      LoginWindow_Closing
     * DESCRIPTION:  Gère l'événement de fermeture de la fenêtre de connexion
     *               et ferme l'application si l'authentification a échoué
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void LoginWindow_Closing(object? sender, CancelEventArgs e)
    {
        // Si la fenêtre se ferme sans authentification réussie, fermer l'application
        if (!_loginSuccessful) Application.Current.Shutdown();
    }

    /*********************************************************************
     * MÉTHODE:      ConnexionButton_Click
     * DESCRIPTION:  Gère l'événement de clic sur le bouton de connexion
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    [Experimental("WPF0001")]
    private void ConnexionButton_Click(object sender, RoutedEventArgs e)
    {
        TenterAuthentification();
    }

    /*********************************************************************
     * MÉTHODE:      TextBox_KeyDown
     * DESCRIPTION:  Gère l'événement de touche pressée dans la zone de texte
     *               d'identifiant pour passer à la zone de mot de passe
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement de touche
     *********************************************************************/
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            MotDePassePasswordBox.Focus();
            e.Handled = true;
        }
    }

    /*********************************************************************
     * MÉTHODE:      PasswordBox_KeyDown
     * DESCRIPTION:  Gère l'événement de touche pressée dans la zone de mot
     *               de passe pour valider la connexion
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement de touche
     *********************************************************************/
    [Experimental("WPF0001")]
    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TenterAuthentification();
            e.Handled = true;
        }
    }

    /*********************************************************************
     * MÉTHODE:      TenterAuthentification
     * DESCRIPTION:  Vérifie les informations d'identification saisies et
     *               lance le processus d'authentification
     *********************************************************************/
    [Experimental("WPF0001")]
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

                // Cacher la fenêtre de login
                Hide();

                // Créer et configurer la fenêtre principale
                var mainWindow = new MainWindow();

                // Afficher la fenêtre principale
                Application.Current.MainWindow = mainWindow;
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

    /*********************************************************************
     * MÉTHODE:      AuthentifierUtilisateur
     * DESCRIPTION:  Délègue l'authentification au contrôleur de l'application
     * PARAMÈTRES:   identifiant - Identifiant saisi par l'utilisateur
     *               motDePasse - Mot de passe saisi par l'utilisateur
     * RETOUR:       Booléen indiquant si l'authentification est réussie
     *********************************************************************/
    private bool AuthentifierUtilisateur(string identifiant, string motDePasse)
    {
        // Utilisez votre Controller pour vérifier les identifiants
        return _controller.AuthentifierUtilisateur(identifiant, motDePasse);
    }

    /*********************************************************************
     * MÉTHODE:      AfficherErreur
     * DESCRIPTION:  Affiche un message d'erreur dans la zone prévue
     * PARAMÈTRES:   message - Message d'erreur à afficher
     *********************************************************************/
    private void AfficherErreur(string message)
    {
        ErreurTextBlock.Text = message;
        ErreurTextBlock.Visibility = Visibility.Visible;
    }
}