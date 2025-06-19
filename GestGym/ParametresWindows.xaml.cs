/*********************************************************************
 * FICHIER:        ParametresWindows.xaml.cs
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
public partial class ParametresWindows : Window
{
    private readonly Controller _controller;

    /*********************************************************************
     * CONSTRUCTEUR: ParametresWindows
     * DESCRIPTION:  Initialise la fenêtre des paramètres et charge les
     *               configurations existantes
     * PARAMÈTRES:   controller - Instance du contrôleur principal
     *********************************************************************/
    public ParametresWindows(Controller controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        InitializeComponent();

        // Charger tous les paramètres de l'application
        LoadSettings(SQLServerNameTextBox);
    }


    /*********************************************************************
     * MÉTHODE:      LoadSettings
     * DESCRIPTION:  Charge tous les paramètres de l'application et
     *               remplit les champs du formulaire avec ces valeurs
     *********************************************************************/
    private void LoadSettings(TextBox SQLServerNameTextBox)
    {
        SQLServerNameTextBox.Text = Settings.Default.SQLServerName;
        SQLDBNameTextBox.Text = Settings.Default.SQLServerDBName;
        SQLAccountTextBox.Text = Settings.Default.SQLAccount;
        SQLPasswordBox.Password = Settings.Default.SQLPassword;
    }

    private Controller Get_controller()
    {
        return _controller;
    }

    /*********************************************************************
     * MÉTHODE:      SaveSettings_Click
     * DESCRIPTION:  Sauvegarde les paramètres de connexion SQL et
     *               redémarre l'application pour les appliquer
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    public void SaveSettings_Click(object sender, RoutedEventArgs e)
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
     * MÉTHODE:      TestSQLConnection_Click
     * DESCRIPTION:  Teste la connexion à la base de données avec les
     *               paramètres actuellement saisis
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void TestSQLConnection_Click(object sender, RoutedEventArgs e)
    {
        // Créer une chaîne de connexion temporaire pour le test
        var connectionString = $"Data Source={SQLServerNameTextBox.Text};Initial Catalog={SQLDBNameTextBox.Text};User ID={SQLAccountTextBox.Text};Password={SQLPasswordBox.Password};TrustServerCertificate=True;";

        // Tester la connexion
        if (_controller.TestDatabaseConnection(connectionString))
            MessageBox.Show("La connexion à la base de données a réussi.",
                "Test de connexion", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /*********************************************************************
     * MÉTHODE:      CloseButton_Click
     * DESCRIPTION:  Ferme la fenêtre des paramètres sans enregistrer
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
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

            // Implémenter la logique de réinitialisation de la BD
            _controller.ResetDatabase();
        else

            // Si l'utilisateur annule, afficher un message d'information
            MessageBox.Show("Réinitialisation annulée.",
                "Annulation", MessageBoxButton.OK, MessageBoxImage.Information);
        _controller.SaveSettings(1);
    }
}