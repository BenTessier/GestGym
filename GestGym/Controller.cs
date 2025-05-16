using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;


// Removed unnecessary suppression
// #pragma warning disable WPF0001

namespace GestGym
{
    public class Controller
    {

        // Chaîne de connexion à la base de données
        private readonly string _connectionString;

        [Experimental("WPF0001")]
        public Controller()
        {
            // Initialize _connectionString to avoid CS8618 error
            _connectionString = string.Empty;

            // Vérifiez si les paramètres de base de données sont définis
            if (string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLDBName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
            {
                // Ouvrir la fenêtre des paramètres directement au lieu des popups
                MessageBox.Show("Certains paramètres de connexion sont manquants. " +
                              "La fenêtre de paramètres va s'ouvrir pour que vous puissiez les configurer.",
                              "Configuration requise", MessageBoxButton.OK, MessageBoxImage.Information);

                var parametresWindows = new ParametresWindows(this);
                var result = parametresWindows.ShowDialog();

                // Si l'utilisateur ferme la fenêtre sans configurer, quitter l'application
                if (result != true ||
                    string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
                    string.IsNullOrWhiteSpace(Settings.Default.SQLDBName) ||
                    string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
                    string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
                {
                    MessageBox.Show("Les paramètres de connexion à la base de données sont obligatoires pour le fonctionnement de l'application.",
                                  "Configuration incomplète", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                    return;
                }
            }

            // Construire la chaîne de connexion avec les paramètres validés
            _connectionString = BuildConnectionString();
            LoadThemeSettings(); // Charger les paramètres de thème

            // Tester la connexion immédiatement
            if (!TestDatabaseConnection())
            {
                MessageBox.Show("Impossible de se connecter à la base de données avec les paramètres fournis. " +
                              "Veuillez vérifier les informations de connexion dans les paramètres.",
                              "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);

                // Proposer d'ouvrir à nouveau la fenêtre des paramètres
                if (MessageBox.Show("Souhaitez-vous ouvrir la fenêtre des paramètres pour corriger les informations de connexion ?",
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

        public DataRow? RechercherUtilisateur(string recherche)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = @"SELECT * FROM Utilisateurs 
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

        public bool ModifierUtilisateur(int id, string nom, string telephone, int numEmploye, string courriel, string identifiant, string motDePasse, DateTime? dateInscription, string notes,int statut)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Si mot de passe non renseigné, ne pas le modifier
                string queryPassword = !string.IsNullOrEmpty(motDePasse) ? ", MotDePasse = @MotDePasse" : "";

                string query = $@"UPDATE Utilisateurs SET 
                        Nom = @Nom,
                        Telephone = @Telephone, 
                        NumEmploye = @NumEmploye, 
                        Courriel = @Courriel,
                        Identifiant = @Identifiant{queryPassword},
                        DateInscription = @DateInscription,
                        Notes = @Notes,
                        Statut = @Statut
                        WHERE Id = @Id";

                using var cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Statut", statut);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nom", nom);
                cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrEmpty(telephone) ? DBNull.Value : (object)telephone);
                cmd.Parameters.AddWithValue("@NumEmploye", numEmploye);
                cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrEmpty(courriel) ? DBNull.Value : (object)courriel);
                cmd.Parameters.AddWithValue("@Identifiant", identifiant);

                if (!string.IsNullOrEmpty(motDePasse))
                    cmd.Parameters.AddWithValue("@MotDePasse", HashPassword(motDePasse));

                cmd.Parameters.AddWithValue("@DateInscription", dateInscription.HasValue ? (object)dateInscription.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("L'utilisateur a été modifié avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }

                MessageBox.Show("Aucune modification n'a été effectuée.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Méthode pour construire la chaîne de connexion
        private string BuildConnectionString()
        {
            return $"Data Source={Settings.Default.SQLServerName};" +
                   $"Initial Catalog={Settings.Default.SQLDBName};" +
                   $"User ID={Settings.Default.SQLAccount};" +
                   $"Password={Settings.Default.SQLPassword};" +
                   $"TrustServerCertificate=True;";
        }

        // Méthode pour tester la connexion à la base de données
        public bool TestDatabaseConnection(string? connectionString = null)
         {
            try
            {
                // Si aucune chaîne de connexion n'est fournie, utiliser celle par défaut
                using var connection = new SqlConnection(connectionString ?? _connectionString);
                connection.Open();

                // Tester avec une requête simple
                using var cmd = new SqlCommand("SELECT 1", connection);
                cmd.ExecuteScalar();

                return true;
            }
            catch (SqlException ex)
            {
                string errorMessage = $"Erreur SQL (Code {ex.Number}): {ex.Message}";

                // Afficher des informations spécifiques selon le code d'erreur
                switch (ex.Number)
                {
                    case 18456: // Erreur d'authentification
                        errorMessage += "\n\nLes identifiants fournis (nom d'utilisateur/mot de passe) sont incorrects.";
                        break;
                    case 4060: // Impossible d'ouvrir la base de données
                        errorMessage += "\n\nLa base de données {dbName} n'existe pas ou l'accès a été refusé.";
                        break;
                    case 53: // Serveur introuvable
                        errorMessage += "\n\nLe serveur {dbServer} est introuvable. Vérifiez le nom du serveur et assurez-vous qu'il est accessible depuis votre réseau.";
                        break;
                    case 40: // Délai d'attente dépassé
                        errorMessage += "\n\nLa tentative de connexion a expiré. Vérifiez que le serveur est en cours d'exécution et qu'il n'est pas bloqué par un pare-feu.";
                        break;
                }

                MessageBox.Show(errorMessage, "Erreur de connexion à la base de données", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur inattendue: {ex.Message}", "Erreur de connexion à la base de données", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        // Méthode pour ajouter un utilisateur
        public void AjouterUtilisateur(string nom, string telephone, int numEmploye, string courriel,
                                      string identifiant, string motDePasse, DateTime? dateInscription, string notes)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(identifiant) || string.IsNullOrWhiteSpace(motDePasse))
            {
                MessageBox.Show("Les champs Nom, Identifiant et Mot de passe sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Utiliser des paramètres pour éviter les injections SQL
                    SqlCommand cmd = new SqlCommand("INSERT INTO Utilisateurs (Nom, Telephone, NumEmploye, Courriel, Identifiant, MotDePasse, DateInscription, Notes) " +
                                                  "VALUES (@Nom, @Telephone, @NumEmploye, @Courriel, @Identifiant, @MotDePasse, @DateInscription, @Notes)", connection);

                    // Ajout des paramètres
                    cmd.Parameters.AddWithValue("@Nom", nom);
                    cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrWhiteSpace(telephone) ? DBNull.Value : (object)telephone);
                    cmd.Parameters.AddWithValue("@NumEmploye", numEmploye);
                    cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrWhiteSpace(courriel) ? DBNull.Value : (object)courriel);
                    cmd.Parameters.AddWithValue("@Identifiant", identifiant);

                    // Idéalement, le mot de passe devrait être haché avant stockage
                    cmd.Parameters.AddWithValue("@MotDePasse", HashPassword(motDePasse));

                    cmd.Parameters.AddWithValue("@DateInscription", dateInscription.HasValue ? (object)dateInscription.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", notes);

                    // Exécution de la requête
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Utilisateur {nom} ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Échec de l'ajout de l'utilisateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de l'utilisateur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Méthode pour hacher le mot de passe (à implémenter avec une bibliothèque de hachage sécurisée)
        private string HashPassword(string password)
        {
            // Pour une application réelle, utilisez BCrypt, PBKDF2 ou une autre méthode de hachage sécurisée
            // Exemple simple (NE PAS UTILISER EN PRODUCTION):
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        

        //  
        // Settings  
        //  
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

            // Sauvegarder la position et la taille de la fenêtre principale
            if (Application.Current.MainWindow != null)
            {
                Settings.Default.MyMaximized = (Application.Current.MainWindow.WindowState == WindowState.Maximized);

                // Enregistrer la position uniquement si la fenêtre n'est pas minimisée
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


        // Méthode pour charger uniquement les paramètres de thème
        [Experimental("WPF0001")]
        private void LoadThemeSettings()
        {
            string t = Settings.Default.MyTheme;
            Application.Current.ThemeMode = t switch
            {
                "System" => ThemeMode.System,
                "Light" => ThemeMode.Light,
                "Dark" => ThemeMode.Dark,
                _ => ThemeMode.System,
            };

            // Chargement des autres paramètres visuels
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Left = Settings.Default.MyLocationX;
                Application.Current.MainWindow.Top = Settings.Default.MyLocationY;

                if (Settings.Default.MyMaximized == true)
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                else
                    Application.Current.MainWindow.WindowState = WindowState.Normal;

                Application.Current.MainWindow.Width = Settings.Default.MyWidth;
                Application.Current.MainWindow.Height = Settings.Default.MyHeight;
            }
        }
    }
}