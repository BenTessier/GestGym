// =============================================================================
//  Fichier : SQLLoginWindow.xaml.cs
//  Description : Fen�tre de saisie des informations de connexion SQL
//  Auteur : Benoit Tessier
//  Date de cr�ation : 2025-06-09
// =============================================================================

using System.Windows;

namespace GestGym;

/// =============================================================================  
/// SQLLoginWindow - Dialogue de connexion � la base de donn�es SQL
/// =============================================================================
public partial class SQLLoginWindow : Window
{
    #region Constructeur

    /// =============================================================================
    /// Initialise la fen�tre de connexion SQL
    /// ==============================================================================
    public SQLLoginWindow()
    {
        InitializeComponent();
        DialogConfirmed = false;
    }

    #endregion

    #region Propri�t�s publiques

    ///Nom d'utilisateur SQL
    public string SQLUsername { get; private set; } = string.Empty;

    ///Mot de passe SQL
    public string SQLPassword { get; private set; } = string.Empty;

    ///�tat de la confirmation du dialogue
    public bool DialogConfirmed { get; private set; }

    #endregion

    #region Gestionnaires d'�v�nements

    /// =============================================================================
    /// Valide et enregistre les informations de connexion
    /// =
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // Validation du nom d'utilisateur
        if (string.IsNullOrWhiteSpace(SQLUsernameTextBox.Text))
        {
            MessageBox.Show("Veuillez entrer un nom d'utilisateur SQL.",
                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Sauvegarde des informations et confirmation
        SQLUsername = SQLUsernameTextBox.Text;
        SQLPassword = SQLPasswordBox.Password;
        DialogConfirmed = true;
        DialogResult = true;
        Close();
    }

    /// ============================================================================
    /// Annule la saisie et ferme la fen�tre
    /// ============================================================================
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogConfirmed = false;
        DialogResult = false;
        Close();
    }

    #endregion
}