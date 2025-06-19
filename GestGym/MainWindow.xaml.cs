/*********************************************************************
 * FICHIER:        MainWindow.xaml.cs
 * DESCRIPTION:    Fenêtre principale de l'application GestGym
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace GestGym;

/// <summary>
///     Classe principale de l'application GestGym qui gère l'interface utilisateur
///     et coordonne l'accès aux différentes fonctionnalités du système.
/// </summary>
public partial class MainWindow
{
    private readonly Controller _controller;

    /*********************************************************************
     * CONSTRUCTEUR: MainWindow
     * DESCRIPTION:  Initialise la fenêtre principale de l'application
     *               et charge les paramètres utilisateur enregistrés
     *********************************************************************/
    [Experimental("WPF0001")]
    public MainWindow()
    {
        InitializeComponent();
        _controller = new Controller();

        // Get user settings
        Left = Settings.Default.MyLocationX;
        Top = Settings.Default.MyLocationY;
        Height = Settings.Default.MyHeight;
        Width = Settings.Default.MyWidth;
        RunNumber = Settings.Default.RunNumber;
        if (Settings.Default.MyMaximized)
            WindowState = WindowState.Maximized;
        else
            WindowState = WindowState.Normal;

        // Associer l'événement Closing à la méthode GestGym_Closing
        Closing += GestGym_Closing;
    }

    public string RunNumber { get; set; }

    /*********************************************************************
     * MÉTHODE:      Parametres_Click
     * DESCRIPTION:  Ouvre la fenêtre des paramètres de l'application
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void Parametres_Click(object sender, RoutedEventArgs e)
    {
        // Crée une instance de la fenêtre des paramètres
        var _parametresWindows = new ParametresWindows(_controller);

        // Affiche la fenêtre en mode modal
        _parametresWindows.ShowDialog();
    }

    /*********************************************************************
     * MÉTHODE:      GestGym_Closing
     * DESCRIPTION:  Sauvegarde les paramètres de l'application lors de
     *               la fermeture de la fenêtre principale
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    [Experimental("WPF0001")]
    private void GestGym_Closing(object? sender, CancelEventArgs e)
    {
        _controller.SaveSettings(0);
    }


    /******************************************************
     * Utilisateur
     *****************************************************/

    /*********************************************************************
     * MÉTHODE:      ConsulterUtilisateur_Click
     * DESCRIPTION:  Affiche l'interface de consultation d'utilisateur
     *               en mode lecture seule
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void ConsulterUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Affiche la fiche utilisateur en mode lecture seule
        DynamicContent.Content = new GestionUtilisateur(_controller, false, true);
    }

    /*********************************************************************
     * MÉTHODE:      AjouterUtilisateur_Click
     * DESCRIPTION:  Affiche l'interface d'ajout d'un nouvel utilisateur
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void AjouterUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Mode ajout : ne pas afficher recherche/statut
        DynamicContent.Content = new GestionUtilisateur(_controller, false, false);
    }

    /*********************************************************************
     * MÉTHODE:      ModifierUtilisateur_Click
     * DESCRIPTION:  Affiche l'interface de modification d'utilisateur
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void ModifierUtilisateur_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionUtilisateur(_controller, true, false);
    }


    /******************************************************
     * Client
     *****************************************************/

    /*********************************************************************
     * MÉTHODE:      ConsulterClient_Click
     * DESCRIPTION:  Affiche l'interface de consultation de client
     *               en mode lecture seule
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void ConsulterClient_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionClient(_controller, true, true);
    }

    /*********************************************************************
     * MÉTHODE:      AjouterClient_Click
     * DESCRIPTION:  Affiche l'interface d'ajout d'un nouveau client
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void AjouterClient_Click(object sender, RoutedEventArgs e)
    {
        DynamicContent.Content = new GestionClient(_controller, false);
    }

    /*********************************************************************
     * MÉTHODE:      ModifierClient_Click
     * DESCRIPTION:  Affiche l'interface de modification de client
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void ModifierClient_Click(object sender, RoutedEventArgs e)
    {
        // Mode modification : afficher recherche/statut
        DynamicContent.Content = new GestionClient(_controller, true);
    }

    /******************************************************
     * Plan et option de base
     *****************************************************/

    /*********************************************************************
     * MÉTHODE:      GererOptionsBaseButton_Click
     * DESCRIPTION:  Affiche l'interface de gestion des options de base
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void GererOptionsBaseButton_Click(object sender, RoutedEventArgs e)
    {
        DynamicContent.Content = new GestionOptionDeBase(_controller);
    }

    /*********************************************************************
     * MÉTHODE:      GererPlansBaseButton_Click
     * DESCRIPTION:  Affiche l'interface de gestion des plans de base
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void GererPlansBaseButton_Click(object sender, RoutedEventArgs e)
    {
        // Affiche la gestion des plans de base dans le contenu dynamique
        DynamicContent.Content = new GestionPlanDeBase(_controller);
    }
}