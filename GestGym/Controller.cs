using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Data.SqlClient;


// Removed unnecessary suppression
// #pragma warning disable WPF0001

namespace GestGym;

public class Controller
{
    // Cha�ne de connexion � la base de donn�es
    private readonly string _connectionString;

    [Experimental("WPF0001")]
    public Controller()
    {
        // Initialize _connectionString to avoid CS8618 error
        _connectionString = string.Empty;

        // V�rifiez si les param�tres de base de donn�es sont d�finis
        if (string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
            string.IsNullOrWhiteSpace(Settings.Default.SQLDBName) ||
            string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
            string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
        {
            // Ouvrir la fenetre des param�tres directement au lieu des popups
            MessageBox.Show("Certains paramêtres de connexion sont manquants. " +
                            "La fenêtre de param�tres va s'ouvrir pour que vous puissiez les configurer.",
                "Configuration requise", MessageBoxButton.OK, MessageBoxImage.Information);

            var parametresWindows = new ParametresWindows(this);
            var result = parametresWindows.ShowDialog();

            // Si l'utilisateur ferme la fen�tre sans configurer, quitter l'application
            if (result != true ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLDBName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
            {
                MessageBox.Show("Les param�tres de connexion à la base de donn�es sont obligatoires pour le fonctionnement de l'application.",
                    "Configuration incomplête", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }
        }

        // Construire la chaine de connexion avec les parametres valid�s
        _connectionString = BuildConnectionString();
        LoadThemeSettings(); // Charger les param�tres de th�me

        // Tester la connexion imm�diatement
        if (!TestDatabaseConnection())
        {
            MessageBox.Show("Impossible de se connecter à la base de données avec les param�tres fournis. " +
                            "Veuillez vérifier les informations de connexion dans les param�tres.",
                "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);

            // Proposer d'ouvrir � nouveau la fen�tre des param�tres
            if (MessageBox.Show("Souhaitez-vous ouvrir la fenêtre des param�tres pour corriger les informations de connexion ?",
                    "Configuration de la connexion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var parametresWindows = new ParametresWindows(this);
                parametresWindows.ShowDialog();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }


    /*******************************************************************
     * METHODES POUR LA GESTION DE LA CONNECTION À LA BASE DE DONN�ES
     *******************************************************************/


    // Méthode pour construire la chaine de connexion
    private string BuildConnectionString()
    {
        return $"Data Source={Settings.Default.SQLServerName};" +
               $"Initial Catalog={Settings.Default.SQLDBName};" +
               $"User ID={Settings.Default.SQLAccount};" +
               $"Password={Settings.Default.SQLPassword};" +
               $"TrustServerCertificate=True;";
    }

    // M�thode pour tester la connexion � la base de donn�es
    public bool TestDatabaseConnection(string? connectionString = null)
    {
        try
        {
            // Si aucune cha�ne de connexion n'est fournie, utiliser celle par d�faut
            using var connection = new SqlConnection(connectionString ?? _connectionString);
            connection.Open();

            // Tester avec une requ�te simple
            using var cmd = new SqlCommand("SELECT 1", connection);
            cmd.ExecuteScalar();

            return true;
        }
        catch (SqlException ex)
        {
            var errorMessage = $"Erreur SQL (Code {ex.Number}): {ex.Message}";

            // Afficher des informations sp�cifiques selon le code d'erreur
            switch (ex.Number)
            {
                case 18456: // Erreur d'authentification
                    errorMessage += "\n\nLes identifiants fournis (nom d'utilisateur/mot de passe) sont incorrects.";
                    break;
                case 4060: // Impossible d'ouvrir la base de donn�es
                    errorMessage += "\n\nLa base de donn�es {dbName} n'existe pas ou l'acc�s a �t� refus�.";
                    break;
                case 53: // Serveur introuvable
                    errorMessage += "\n\nLe serveur {dbServer} est introuvable. V�rifiez le nom du serveur et assurez-vous qu'il est accessible depuis votre r�seau.";
                    break;
                case 40: // D�lai d'attente d�pass�
                    errorMessage += "\n\nLa tentative de connexion a expir�. V�rifiez que le serveur est en cours d'ex�cution et qu'il n'est pas bloqu� par un pare-feu.";
                    break;
            }

            MessageBox.Show(errorMessage, "Erreur de connexion � la base de donn�es", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur inattendue: {ex.Message}", "Erreur de connexion � la base de donn�es", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /*******************************************************************
     * METHODES POUR LA GESTION DES UTILISATEURS
     *******************************************************************/

    // M�thode pour ajouter un utilisateur
    public void AjouterUtilisateur(string nom, string telephone, int numEmploye, string courriel,
                                   string identifiant, string motDePasse, DateTime? dateInscription,
                                   string notes, int? REF_Roles_id, int? REF_Groupes_id)
    {
        // Validation des entr�es
        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(identifiant) || string.IsNullOrWhiteSpace(motDePasse))
        {
            MessageBox.Show("Les champs Nom, Identifiant et Mot de passe sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Construire la requ�te en fonction des colonnes qui existent r�ellement
                var colonnes = "Nom, Telephone, NumEmploye, Courriel, Identifiant, MotDePasse, DateInscription, Notes, REF_Groupes_id, REF_Roles_id";
                var valeurs = "@Nom, @Telephone, @NumEmploye, @Courriel, @Identifiant, @MotDePasse, @DateInscription, @Notes, @Groupe_id, @Role_Id";

                // Utiliser des param�tres pour �viter les injections SQL
                var query = $"INSERT INTO Utilisateurs ({colonnes}) VALUES ({valeurs})";
                var cmd = new SqlCommand(query, connection);

                // Ajout des param�tres
                cmd.Parameters.AddWithValue("@Nom", nom);
                cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrWhiteSpace(telephone) ? DBNull.Value : telephone);
                cmd.Parameters.AddWithValue("@NumEmploye", numEmploye);
                cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrWhiteSpace(courriel) ? DBNull.Value : courriel);
                cmd.Parameters.AddWithValue("@Identifiant", identifiant);
                cmd.Parameters.AddWithValue("@MotDePasse", HashPassword(motDePasse));
                cmd.Parameters.AddWithValue("@DateInscription", dateInscription.HasValue ? dateInscription.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", notes);
                cmd.Parameters.AddWithValue("@Role_id", REF_Roles_id.HasValue ? REF_Roles_id.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Groupe_id", REF_Groupes_id.HasValue ? REF_Groupes_id.Value : DBNull.Value);

                // Ex�cution de la requ�te
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    MessageBox.Show($"Utilisateur {nom} ajout� avec succ�s.", "Succ�s", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("�chec de l'ajout de l'utilisateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout de l'utilisateur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public bool ModifierUtilisateur(int id, string nom, string telephone, int numEmploye, string courriel,
                                    string identifiant, string motDePasse, DateTime? dateInscription,
                                    string notes, int statut, int? Role_id, int? Groupe_id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Si mot de passe non renseign�, ne pas le modifier
            var queryPassword = !string.IsNullOrEmpty(motDePasse) ? ", MotDePasse = @MotDePasse" : "";


            var query = $@"UPDATE Utilisateurs SET 
                    Nom = @Nom,
                    Telephone = @Telephone, 
                    NumEmploye = @NumEmploye, 
                    Courriel = @Courriel,
                    Identifiant = @Identifiant{queryPassword},
                    DateInscription = @DateInscription,
                    Notes = @Notes,
                    Statut = @Statut,
                    REF_Roles_id = @Role_id,
                    REF_Groupes_id = @Groupe_id
                    WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@Statut", statut);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrEmpty(telephone) ? DBNull.Value : telephone);
            cmd.Parameters.AddWithValue("@NumEmploye", numEmploye);
            cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrEmpty(courriel) ? DBNull.Value : courriel);
            cmd.Parameters.AddWithValue("@Identifiant", identifiant);
            cmd.Parameters.AddWithValue("@Role_id", Role_id.HasValue ? Role_id.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Groupe_id", Groupe_id.HasValue ? Groupe_id.Value : DBNull.Value);

            if (!string.IsNullOrEmpty(motDePasse))
                cmd.Parameters.AddWithValue("@MotDePasse", HashPassword(motDePasse));

            cmd.Parameters.AddWithValue("@DateInscription", dateInscription.HasValue ? dateInscription.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : notes);

            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                MessageBox.Show("L'utilisateur a �t� modifi� avec succ�s.", "Succ�s", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            MessageBox.Show("Aucune modification n'a �t� effectu�e.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public DataRow? RechercherUtilisateur(string recherche)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"SELECT * FROM Utilisateurs 
                       WHERE (Nom LIKE @recherche 
                       OR Identifiant LIKE @recherche 
                       OR CONVERT(VARCHAR, NumEmploye) LIKE @recherche) 
                       AND Statut = 1";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@recherche", "%" + recherche + "%");

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count > 0)
                return table.Rows[0];

            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }


    /*******************************************************************
     * METHODES POUR LA GESTION DES CLIENTS
     *******************************************************************/

    public bool AjouterClient(string nom, string telephone, string courriel, int NoMembre, string notes)
    {
        // Validation des entr�es
        if (string.IsNullOrWhiteSpace(nom))
        {
            MessageBox.Show("Les champs Nom et Pr�nom sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Construire la requ�te
            var colonnes = "Nom, Telephone, Courriel, NoMembre, Notes";
            var valeurs = "@Nom, @Telephone, @Courriel, @NoMembre, @Notes";

            // Utiliser des param�tres pour �viter les injections SQL
            var query = $"INSERT INTO Clients ({colonnes}) VALUES ({valeurs})";
            using var cmd = new SqlCommand(query, connection);

            // Ajout des param�tres
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@NoMembre", NoMembre);
            cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrWhiteSpace(telephone) ? DBNull.Value : telephone);
            cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrWhiteSpace(courriel) ? DBNull.Value : courriel);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);

            // Ex�cution de la requ�te
            var rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout du client: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public bool ModifierClient(int id, string nom, string telephone, string courriel, int noMembre, string notes)
    {
        // Validation des entr�es
        if (string.IsNullOrWhiteSpace(nom))
        {
            MessageBox.Show("Le champs Nom est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"UPDATE Clients SET 
                    Nom = @Nom,
                    Telephone = @Telephone, 
                    Courriel = @Courriel,
                    NoMembre = @NoMembre,
                    Notes = @Notes
                    WHERE Client_id = @Id";

            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrEmpty(telephone) ? DBNull.Value : telephone);
            cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrEmpty(courriel) ? DBNull.Value : courriel);
            cmd.Parameters.AddWithValue("@NoMembre", noMembre);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : notes);

            var rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification du client: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }


    public DataRow? RechercherClient(string recherche)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"SELECT * FROM Clients 
                       WHERE Nom LIKE @recherche 
                       OR Telephone LIKE @recherche
                       OR CONVERT(VARCHAR, NoMembre) LIKE @recherche";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@recherche", "%" + recherche + "%");

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count > 0)
                return table.Rows[0];

            return null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la recherche du client: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }


    /*******************************************************************
     * METHODES DE GESTION DES PARAMETRES DE L'APPLICATION
     *******************************************************************/

    [Experimental("WPF0001")]
    public void LoadSettings()
    {
        LoadThemeSettings();
    }


    public void SaveSettings()
    {
        // App theme  
#pragma warning disable WPF0001
        Settings.Default.MyTheme = Application.Current.ThemeMode.ToString();
#pragma warning restore WPF0001

        // Sauvegarder la position et la taille de la fen�tre principale
        if (Application.Current.MainWindow != null)
        {
            Settings.Default.MyMaximized = Application.Current.MainWindow.WindowState == WindowState.Maximized;

            // Enregistrer la position uniquement si la fen�tre n'est pas minimis�e
            if (Application.Current.MainWindow.WindowState != WindowState.Minimized)
            {
                Settings.Default.MyLocationX = Application.Current.MainWindow.Left;
                Settings.Default.MyLocationY = Application.Current.MainWindow.Top;
                Settings.Default.MyWidth = Application.Current.MainWindow.Width;
                Settings.Default.MyHeight = Application.Current.MainWindow.Height;
            }
        }

        // Save settings  
        Settings.Default.Save();
    }


    // M�thode pour charger uniquement les param�tres de th�me
    [Experimental("WPF0001")]
    private void LoadThemeSettings()
    {
        var t = Settings.Default.MyTheme;
        Application.Current.ThemeMode = t switch
        {
            "System" => ThemeMode.System,
            "Light" => ThemeMode.Light,
            "Dark" => ThemeMode.Dark,
            _ => ThemeMode.System
        };

        // Chargement des autres param�tres visuels
        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Left = Settings.Default.MyLocationX;
            Application.Current.MainWindow.Top = Settings.Default.MyLocationY;

            if (Settings.Default.MyMaximized)
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            else
                Application.Current.MainWindow.WindowState = WindowState.Normal;

            Application.Current.MainWindow.Width = Settings.Default.MyWidth;
            Application.Current.MainWindow.Height = Settings.Default.MyHeight;
        }
    }


    /****************************************************************************
     * M�THODES POUR R�CUP�RER LES INFORMATIONS DES LISTES D�ROULANTES
     ***************************************************************************/

    // M�thode pour hacher le mot de passe (� impl�menter avec une biblioth�que de hachage s�curis�e)
    private string HashPassword(string password)
    {
        // Pour une application r�elle, utilisez BCrypt, PBKDF2 ou une autre m�thode de hachage s�curis�e
        // Exemple simple (NE PAS UTILISER EN PRODUCTION):
        return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
    }


    // M�thode pour r�cup�rer tous les r�les
    public DataTable GetRoles()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT Role_id, Nom FROM Roles ORDER BY Nom";
            using var cmd = new SqlCommand(query, connection);

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la r�cup�ration des r�les: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }

    // M�thode pour r�cup�rer tous les groupes
    public DataTable GetGroupes()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT Groupe_id, Nom FROM Groupes ORDER BY Nom";
            using var cmd = new SqlCommand(query, connection);

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la r�cup�ration des groupes: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }


    /***********************************************************
     * M�thodes pour l'Authentification
     **********************************************************/


    public bool AuthentifierUtilisateur(string identifiant, string motDePasse)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Requ�te pour v�rifier l'utilisateur avec statut diff�rent de 2 (inactif)
            var query = @"SELECT COUNT(1) FROM Utilisateurs 
                         WHERE Identifiant = @identifiant 
                         AND MotDePasse = @motDePasse 
                         AND Statut <> 2";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@identifiant", identifiant);
            cmd.Parameters.AddWithValue("@motDePasse", HashPassword(motDePasse));

            var count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'authentification: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}