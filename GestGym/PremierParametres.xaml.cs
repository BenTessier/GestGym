/*********************************************************************
 * FICHIER:        PremierParametres.xaml.cs
 * DESCRIPTION:    Fenêtre de configuration des paramètres du système
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.Windows;
using System.Windows.Controls;

#pragma warning disable WPF0001

namespace GestGym;

/// <summary>
///     Fenêtre permettant de configurer les paramètres de connexion à la base de données
///     et d'effectuer des opérations de maintenance sur celle-ci.
/// </summary>
public partial class PremierParametres : Window
{
    private readonly Controller _controller;

    /*********************************************************************
     * CONSTRUCTEUR: PremierParametres
     * DESCRIPTION:  Initialise la fenêtre des paramètres et charge les
     *               configurations existantes
     * PARAMÈTRES:   controller - Instance du contrôleur principal
     *********************************************************************/
    public PremierParametres(Controller controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));

        InitializeComponent();
        LoadSettings1(SQLServerNameTextBox);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    public void SaveSettings1()
    {
        // Valider que les champs ne sont pas vides
        if (string.IsNullOrWhiteSpace(SQLServerNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(SQLDBNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(SQLAccountTextBox.Text) ||
            string.IsNullOrWhiteSpace(SQLPasswordBox.Password))
        {
            MessageBox.Show("Tous les champs de connexion à la base de données sont obligatoires.",
                "Validation des paramètres", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Sauvegarder les paramètres
        Settings.Default.SQLServerName = SQLServerNameTextBox.Text;
        Settings.Default.SQLServerDBName = SQLDBNameTextBox.Text;
        Settings.Default.SQLAccount = SQLAccountTextBox.Text;
        Settings.Default.SQLPassword = SQLPasswordBox.Password;
        _controller.SaveSettings(1);
    }

    /*********************************************************************
     * MÉTHODE:      LoadSettings
     * DESCRIPTION:  Charge tous les paramètres de l'application et
     *               remplit les champs du formulaire avec ces valeurs
     *********************************************************************/
    private void LoadSettings1(TextBox SQLServerNameTextBox)
    {
        SQLServerNameTextBox.Text = Settings.Default.SQLServerName;
        SQLDBNameTextBox.Text = Settings.Default.SQLServerDBName;
        SQLAccountTextBox.Text = Settings.Default.SQLAccount;
        SQLPasswordBox.Password = Settings.Default.SQLPassword;
    }

    /*********************************************************************
     * MÉTHODE:      ResetBDButton_Click
     * DESCRIPTION:  Réinitialise la base de données après confirmation
     *               de l'utilisateur
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    public void ResetBDButton_Click(object sender, RoutedEventArgs e)
    {
        // Afficher un message de confirmation
        var result = MessageBox.Show(
            "Attention! Cette opération va initialiser la base de données aux valeurs par défaut. " +
            "Toutes les données existantes tel que les utilisateurs, clients, plans et options  seront perdues. " +
            "Voulez-vous continuer?",
            "Réinitialisation de la base de données",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);


        if (result == MessageBoxResult.OK)
        {
            Settings.Default.SQLServerName = SQLServerNameTextBox.Text;
            Settings.Default.SQLServerDBName = SQLDBNameTextBox.Text;
            Settings.Default.SQLAccount = SQLAccountTextBox.Text;
            Settings.Default.SQLPassword = SQLPasswordBox.Password;
            Settings.Default.Save();

            // Implémenter la logique de réinitialisation de la BD
            _controller.ResetDatabase();
        }
        else
        {
            // Si l'utilisateur annule, afficher un message d'information
            MessageBox.Show("Réinitialisation annulée.",
                "Annulation", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        _controller.SaveSettings(1);
    }
}