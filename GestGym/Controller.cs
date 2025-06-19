/*********************************************************************
 * FICHIER:        Controller.cs
 * DESCRIPTION:    Contrôleur principal gérant les accès à la base de données
 *                 et les opérations de l'application GestGym
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using GestGym.Models;
using Microsoft.Data.SqlClient;

namespace GestGym;

/// <summary>
///     Classe contrôleur principale de l'application GestGym qui gère l'accès
///     aux données et la logique métier de l'application
/// </summary>
public class Controller
{
    // Colonnes par défaut pour la recherche
    private static readonly string[] DefaultSearchColumns = { "Nom", "Identifiant", "NumEmploye" };

    // Chaine de connexion à la base de données
    private readonly string _connectionString;

    /*********************************************************************
     * CONSTRUCTEUR: Controller
     * DESCRIPTION:  Initialise le contrôleur et vérifie la configuration
     *               de la base de données
     *********************************************************************/
    [Experimental("WPF0001")]
    public Controller()
    {
        // Initialize _connectionString to avoid CS8618 error
        _connectionString = string.Empty;

        // Définir le chemin du fichier FirstRun.ini
        var firstRunFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirstRun.ini");
        var firstRun = File.Exists(firstRunFilePath);

        // Vérification si c'est la première exécution du logiciel en vérifiant la présence du fichier FirstRun.ini
        if (firstRun)
        {
            MessageBox.Show("C'est la première fois que le logiciel est exécuté sur cet ordinateur. " +
                            "Veuillez saisir les informations de connexion.",
                "Première exécution", MessageBoxButton.OK, MessageBoxImage.Information);

            var premierParametres = new PremierParametres(this);
            premierParametres.ShowDialog();

            Application.Current.Shutdown();
        }

        // Si ce n'est pas la première exécution, procéder avec la vérification habituelle des paramètres
        else if (string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
                 string.IsNullOrWhiteSpace(Settings.Default.SQLServerDBName) ||
                 string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
                 string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
        {
            // Ouvrir la fenetre des paramêtres directement au lieu des popups
            MessageBox.Show("Certains paramètres de connexion sont manquants" +
                            "La fenêtre de paramètres va s'ouvrir pour que vous puissiez les corriger.",
                "Configuration requise", MessageBoxButton.OK, MessageBoxImage.Information);

            var parametresWindows = new ParametresWindows(this);
            var result = parametresWindows.ShowDialog();

            // Si l'utilisateur ferme la fenêtre sans configurer, quitter l'application
            if (result != true ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLServerName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLServerDBName) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLAccount) ||
                string.IsNullOrWhiteSpace(Settings.Default.SQLPassword))
            {
                MessageBox.Show("Les paramètres de connexion à la base de données sont obligatoires pour le fonctionnement de l'application.",
                    "Configuration incomplête", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }
        }

        // Charger tous les paramètres
        LoadAllSettings();

        // Construire la chaine de connexion avec les parametres validés
        _connectionString = BuildConnectionString();

        // Tester la connexion immédiatement
        if (!TestDatabaseConnection())
        {
            MessageBox.Show(
                "Une erreur de connection est survenue. Valider les informations de connexion dans les paramètres.",
                "Configuration requise", MessageBoxButton.OK, MessageBoxImage.Information);

            // Proposer d'ouvrir la fenêtre des paramètres
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


    /*******************************************************************
     * METHODES POUR LA GESTION DE LA CONNECTION À LA BASE DE données
     *******************************************************************/


    /*********************************************************************
     * MÉTHODE:      BuildConnectionString
     * DESCRIPTION:  Construit la chaîne de connexion à la base de données
     *               à partir des paramètres de l'application
     * RETOUR:       Chaîne de connexion formatée pour SQL Server
     *********************************************************************/
    private static string BuildConnectionString()
    {
        return $"Data Source={Settings.Default.SQLServerName};" +
               $"Initial Catalog={Settings.Default.SQLServerDBName};" +
               $"User ID={Settings.Default.SQLAccount};" +
               $"Password={Settings.Default.SQLPassword};" +
               $"TrustServerCertificate=True;";
    }

    /*********************************************************************
     * MÉTHODE:      TestDatabaseConnection
     * DESCRIPTION:  Teste la connexion à la base de données avec la chaîne
     *               de connexion spécifiée ou celle par défaut
     * PARAMÈTRES:   connectionString - Chaîne de connexion optionnelle à tester
     * RETOUR:       Booléen indiquant si la connexion a réussi
     *********************************************************************/
    public bool TestDatabaseConnection(string? connectionString = null)
    {
        // Si aucune chaine de connexion n'est fournie, utiliser celle par défaut
        using var connection = new SqlConnection(connectionString ?? _connectionString);
        connection.Open();

        // Tester avec une requète simple
        using var cmd = new SqlCommand("SELECT 1", connection);
        cmd.ExecuteScalar();

        return true;
    }

    /*******************************************************************
     * METHODES POUR LA GESTION DES UTILISATEURS
     *******************************************************************/

    /*********************************************************************
     * MÉTHODE:      AjouterUtilisateur
     * DESCRIPTION:  Ajoute un nouvel utilisateur dans la base de données
     * PARAMÈTRES:   nom - Nom complet de l'utilisateur
     *               telephone - Numéro de téléphone
     *               numEmploye - Numéro d'employé
     *               courriel - Adresse e-mail
     *               identifiant - Identifiant de connexion
     *               motDePasse - Mot de passe non crypté
     *               dateInscription - Date d'inscription
     *               notes - Notes supplémentaires
     *               REF_Roles_id - ID du rôle associé
     *               REF_Groupes_id - ID du groupe associé
     *********************************************************************/
    public void AjouterUtilisateur(string nom, string telephone, int numEmploye, string courriel,
                                   string identifiant, string motDePasse, DateTime? dateInscription,
                                   string notes, int? REF_Roles_id, int? REF_Groupes_id)
    {
        // Validation des entrées
        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(identifiant) || string.IsNullOrWhiteSpace(motDePasse))
        {
            MessageBox.Show("Les champs Nom, Identifiant et Mot de passe sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Construire la requète en fonction des colonnes qui existent réellement
            var colonnes = "Nom, Telephone, NumEmploye, Courriel, Identifiant, MotDePasse, DateInscription, Notes, REF_Groupes_id, REF_Roles_id";
            var valeurs = "@Nom, @Telephone, @NumEmploye, @Courriel, @Identifiant, @MotDePasse, @DateInscription, @Notes, @Groupe_id, @Role_Id";

            // Utiliser des paramêtres pour éviter les injections SQL
            var query = $"INSERT INTO Utilisateurs ({colonnes}) VALUES ({valeurs})";
            var cmd = new SqlCommand(query, connection);

            // Ajout des paramêtres
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

            // Exécution de la requète
            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
                MessageBox.Show($"Utilisateur {nom} ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("échec de l'ajout de l'utilisateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout de l'utilisateur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      ModifierUtilisateur
     * DESCRIPTION:  Modifie un utilisateur existant dans la base de données
     * PARAMÈTRES:   id - Identifiant unique de l'utilisateur
     *               nom - Nom complet de l'utilisateur
     *               telephone - Numéro de téléphone
     *               numEmploye - Numéro d'employé
     *               courriel - Adresse e-mail
     *               identifiant - Identifiant de connexion
     *               motDePasse - Mot de passe (si vide, conserve l'ancien)
     *               dateInscription - Date d'inscription
     *               notes - Notes supplémentaires
     *               statut - État de l'utilisateur (1=actif, 2=inactif)
     *               Role_id - ID du rôle associé
     *               Groupe_id - ID du groupe associé
     * RETOUR:       Booléen indiquant si la modification a réussi
     *********************************************************************/
    public bool ModifierUtilisateur(int id, string nom, string telephone, int numEmploye, string courriel,
                                    string identifiant, string motDePasse, DateTime? dateInscription,
                                    string notes, int statut, int? Role_id, int? Groupe_id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Si mot de passe non renseigné, ne pas le modifier
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

    /*********************************************************************
     * MÉTHODE:      RechercherUtilisateur
     * DESCRIPTION:  Recherche un utilisateur selon des critères spécifiés
     * PARAMÈTRES:   recherche - Terme de recherche (nom, identifiant, etc.)
     * RETOUR:       Ligne de données de l'utilisateur trouvé ou null
     *********************************************************************/
    public DataRow? RechercherUtilisateur(string recherche)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = $@"SELECT * FROM Utilisateurs 
               WHERE ({string.Join(" LIKE @recherche OR ", DefaultSearchColumns)} LIKE @recherche) 
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

    /*********************************************************************
     * MÉTHODE:      AjouterClient
     * DESCRIPTION:  Ajoute un nouveau client dans la base de données
     * PARAMÈTRES:   nom - Nom complet du client
     *               telephone - Numéro de téléphone
     *               courriel - Adresse e-mail
     *               NoMembre - Numéro de membre
     *               notes - Notes supplémentaires
     * RETOUR:       Booléen indiquant si l'ajout a réussi
     *********************************************************************/
    public bool AjouterClient(string nom, string telephone, string courriel, int NoMembre, string notes)
    {
        // Validation des entrées
        if (string.IsNullOrWhiteSpace(nom))
        {
            MessageBox.Show("Les champs Nom et Prénom sont obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Construire la requète
            var colonnes = "Nom, Telephone, Courriel, NoMembre, Notes, Statut";
            var valeurs = "@Nom, @Telephone, @Courriel, @NoMembre, @Notes, 1";

            // Utiliser des paramêtres pour éviter les injections SQL
            var query = $"INSERT INTO Clients ({colonnes}) VALUES ({valeurs})";
            using var cmd = new SqlCommand(query, connection);

            // Ajout des paramêtres
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@NoMembre", NoMembre);
            cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrWhiteSpace(telephone) ? DBNull.Value : telephone);
            cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrWhiteSpace(courriel) ? DBNull.Value : courriel);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);

            // Exécution de la requète
            var rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout du client: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /*********************************************************************
     * MÉTHODE:      ModifierClient
     * DESCRIPTION:  Modifie un client existant dans la base de données
     * PARAMÈTRES:   id - Identifiant unique du client
     *               nom - Nom complet du client
     *               telephone - Numéro de téléphone
     *               courriel - Adresse e-mail
     *               noMembre - Numéro de membre
     *               notes - Notes supplémentaires
     * RETOUR:       Booléen indiquant si la modification a réussi
     *********************************************************************/
    public bool ModifierClient(int id, string nom, string telephone, string courriel, int noMembre, string notes)
    {
        // Validation des entrées
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

    /*********************************************************************
     * MÉTHODE:      RechercherClient
     * DESCRIPTION:  Recherche un client selon des critères spécifiés
     * PARAMÈTRES:   recherche - Terme de recherche (nom, téléphone, etc.)
     * RETOUR:       Ligne de données du client trouvé ou null
     *********************************************************************/
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

    /*********************************************************************
     * MÉTHODE:      LoadAllSettings
     * DESCRIPTION:  Charge tous les paramètres de l'application depuis
     *               les paramètres enregistrés
     *********************************************************************/
    [Experimental("WPF0001")]
    public static void LoadAllSettings()
    {
        try
        {
            // Charger les paramètres de thème
            var t = Settings.Default.MyTheme;
            Application.Current.ThemeMode = ThemeMode.Light; // Par défaut, charger le mode clair

            // Chargement des paramètres visuels
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Left = Settings.Default.MyLocationX;
                Application.Current.MainWindow.Top = Settings.Default.MyLocationY;
                Application.Current.MainWindow.Width = Settings.Default.MyWidth;
                Application.Current.MainWindow.Height = Settings.Default.MyHeight;

                if (Settings.Default.MyMaximized)
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                else
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
            }

            // Paramètres de SQL peuvent être chargés (mais ne sont pas appliqués directement ici)
            // Ces paramètres seront utilisés par BuildConnectionString()
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des paramètres: {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      SaveSettings
     * DESCRIPTION:  Sauvegarde les paramètres de l'application, notamment
     *               la position et taille de la fenêtre principale
     *********************************************************************/
    [Experimental("WPF0001")]
    public void SaveSettings(int message)
    {
        try
        {
            // Position et taille de la fenêtre
            Settings.Default.MyLocationX = Application.Current.MainWindow.Left;
            Settings.Default.MyLocationY = Application.Current.MainWindow.Top;
            Settings.Default.MyWidth = Application.Current.MainWindow.Width;
            Settings.Default.MyHeight = Application.Current.MainWindow.Height;
            Settings.Default.MyMaximized = Application.Current.MainWindow.WindowState == WindowState.Maximized;

            // Paramètres de l'application
            Settings.Default.MyTheme = "Light";
            Settings.Default.RunNumber = "1";

            // Forcer l'enregistrement de tous les paramètres, même ceux qui n'ont pas changé
            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde des paramètres: {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Définir le chemin du fichier FirstRun.ini
        var firstRunFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirstRun.ini");

        // Vérifier si le fichier existe
        if (File.Exists(firstRunFilePath))
            try
            {
                // Supprimer le fichier
                File.Delete(firstRunFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression du fichier 'FirstRun.ini': {ex.Message}");
            }


        if (message == 1)
            MessageBox.Show("Les paramètres de l'application ont été sauvegardés. " +
                            "Une fermeture de l'application est nécessaire pour appliquer les changements.",
                "Paramètres sauvegardés", MessageBoxButton.OK, MessageBoxImage.Information);
        Application.Current.Shutdown();
    }


    /****************************************************************************
     * MéTHODES POUR RéCUPéRER LES INFORMATIONS DES LISTES DéROULANTES
     ***************************************************************************/

    /*********************************************************************
     * MÉTHODE:      HashPassword
     * DESCRIPTION:  Hache un mot de passe en utilisant SHA256
     * PARAMÈTRES:   password - Mot de passe à hacher
     * RETOUR:       Chaîne représentant le mot de passe haché en Base64
     *********************************************************************/
    private static string HashPassword(string password)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    /*********************************************************************
     * MÉTHODE:      GetRoles
     * DESCRIPTION:  Récupère la liste des rôles disponibles
     * RETOUR:       Table contenant les rôles (Role_id, Nom)
     *********************************************************************/
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
            MessageBox.Show($"Erreur lors de la récupération des rôles: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }

    /*********************************************************************
     * MÉTHODE:      GetGroupes
     * DESCRIPTION:  Récupère la liste des groupes disponibles
     * RETOUR:       Table contenant les groupes (Groupe_id, Nom)
     *********************************************************************/
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
            MessageBox.Show($"Erreur lors de la récupération des groupes: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }


    /***********************************************************
     * Méthodes pour l'Authentification
     **********************************************************/

    /*********************************************************************
     * MÉTHODE:      AuthentifierUtilisateur
     * DESCRIPTION:  Vérifie les identifiants d'un utilisateur
     * PARAMÈTRES:   identifiant - Identifiant de l'utilisateur
     *               motDePasse - Mot de passe non crypté
     * RETOUR:       Booléen indiquant si l'authentification est réussie
     *********************************************************************/
    public bool AuthentifierUtilisateur(string identifiant, string motDePasse)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Requète pour vérifier l'utilisateur avec statut différent de 2 (inactif)
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

    /*******************************************************************
     * METHODES POUR LA GESTION DES OPTIONS DE BASE
     *******************************************************************/

    /*********************************************************************
     * MÉTHODE:      ChargerOptionsDepuisLaBase
     * DESCRIPTION:  Charge toutes les options de base depuis la base
     * RETOUR:       Table contenant les options de base
     *********************************************************************/
    public DataTable ChargerOptionsDepuisLaBase()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"SELECT OptionBase_id AS Id, Nom, Notes AS Description, Prix, Horaire, 
                      NbreSeance, REF_Frequence_ID, DateDebutDisponible, DateFinDisponible, Statut 
                      FROM OptionDeBase";

            using var cmd = new SqlCommand(query, connection);
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la lecture des options : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }

    /*********************************************************************
     * MÉTHODE:      AjouterOptionDeBase
     * DESCRIPTION:  Ajoute une nouvelle option de base dans la base de données
     * PARAMÈTRES:   option - Objet contenant les données de l'option à ajouter
     *********************************************************************/
    public void AjouterOptionDeBase(OptionDeBase option)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"INSERT INTO OptionDeBase (Nom, Notes, Prix, Horaire, NbreSeance, 
                      REF_Frequence_ID, DateDebutDisponible, DateFinDisponible, Statut) 
                      VALUES (@Nom, @Description, @Prix, @Horaire, @NbreSeance, 
                      @REF_Frequence_ID, @DateDebutDisponible, @DateFinDisponible, @Statut);
                      SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nom", option.Nom);
            cmd.Parameters.AddWithValue("@Description", option.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Prix", option.Prix);
            cmd.Parameters.AddWithValue("@Horaire", option.Horaire ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NbreSeance", option.NbreSeance);
            cmd.Parameters.AddWithValue("@REF_Frequence_ID", option.REF_Frequence_ID);
            cmd.Parameters.AddWithValue("@DateDebutDisponible", option.DateDebutDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DateFinDisponible", option.DateFinDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Statut", option.Statut);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout de l'option : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      ModifierOptionDeBase
     * DESCRIPTION:  Modifie une option de base existante
     * PARAMÈTRES:   option - Objet contenant les données modifiées de l'option
     *********************************************************************/
    public void ModifierOptionDeBase(OptionDeBase option)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"UPDATE OptionDeBase 
                          SET Nom = @Nom, 
                              Notes = @Description, 
                              Prix = @Prix, 
                              Horaire = @Horaire, 
                              NbreSeance = @NbreSeance, 
                              REF_Frequence_ID = @REF_Frequence_ID, 
                              DateDebutDisponible = @DateDebutDisponible, 
                              DateFinDisponible = @DateFinDisponible, 
                              Statut = @Statut 
                          WHERE OptionBase_id = @Id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", option.OptionBase_id);
            cmd.Parameters.AddWithValue("@Nom", option.Nom);
            cmd.Parameters.AddWithValue("@Description", option.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Prix", option.Prix);
            cmd.Parameters.AddWithValue("@Horaire", option.Horaire ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NbreSeance", option.NbreSeance);
            cmd.Parameters.AddWithValue("@REF_Frequence_ID", option.REF_Frequence_ID);
            cmd.Parameters.AddWithValue("@DateDebutDisponible", option.DateDebutDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DateFinDisponible", option.DateFinDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Statut", option.Statut);

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification de l'option : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    /*******************************************************************
     * METHODES POUR LA GESTION DES PLANS DE BASE
     *******************************************************************/

    /*********************************************************************
     * MÉTHODE:      ChargerPlansDepuisLaBase
     * DESCRIPTION:  Charge tous les plans de base depuis la base de données
     * RETOUR:       Table contenant les plans de base
     *********************************************************************/
    public DataTable ChargerPlansDepuisLaBase()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"SELECT PlanBase_id AS Id, Nom, Notes AS Description, Prix, Duree, DateDebutDisponible, DateFinDisponible, Statut 
                  FROM PlanDeBase";

            using var cmd = new SqlCommand(query, connection);
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la lecture des options : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }

    /*********************************************************************
     * MÉTHODE:      AjouterPlanDeBase
     * DESCRIPTION:  Ajoute un nouveau plan de base dans la base de données
     * PARAMÈTRES:   option - Objet contenant les données du plan à ajouter
     *********************************************************************/
    public void AjouterPlanDeBase(PlanDeBase option)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"INSERT INTO OptionDeBase (Nom, Notes , Prix, Duree, DateDebutDisponible, DateFinDisponible, Statut) 
                  VALUES (@Nom, @Description, @Prix, @Duree, @DateDebutDisponible, @DateFinDisponible, @Statut);
                  SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nom", option.Nom);
            cmd.Parameters.AddWithValue("@Description", option.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Prix", option.Prix);
            cmd.Parameters.AddWithValue("@Duree", option.Duree);
            cmd.Parameters.AddWithValue("@DateDebutDisponible", option.DateDebutDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DateFinDisponible", option.DateFinDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Statut", option.Statut);

            // Récupérer l'ID généré et l'affecter à l'objet
            var result = cmd.ExecuteScalar();
            if (result != null && int.TryParse(result.ToString(), out var newId)) option.PlanBase_id = newId;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout de l'option : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      ModifierPlanDeBase
     * DESCRIPTION:  Modifie un plan de base existant
     * PARAMÈTRES:   option - Objet contenant les données modifiées du plan
     *********************************************************************/
    public void ModifierPlanDeBase(PlanDeBase option)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"UPDATE PlanDeBase 
                          SET Nom = @Nom, 
                              Notes = @Description, 
                              Prix = @Prix, 
                              Duree = @Duree, 
                              DateDebutDisponible = @DateDebutDisponible, 
                              DateFinDisponible = @DateFinDisponible, 
                              Statut = @Statut 
                          WHERE PlanBase_id = @Id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nom", option.Nom);
            cmd.Parameters.AddWithValue("@Description", option.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Prix", option.Prix);
            cmd.Parameters.AddWithValue("@Duree", option.Duree);
            cmd.Parameters.AddWithValue("@DateDebutDisponible", option.DateDebutDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DateFinDisponible", option.DateFinDisponible ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Statut", option.Statut);
            cmd.Parameters.AddWithValue("@Id", option.PlanBase_id);

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la modification de l'option : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      GetFrequence
     * DESCRIPTION:  Récupère la liste des fréquences disponibles
     * RETOUR:       Table contenant les fréquences (Frequence_id, Frequence)
     *********************************************************************/
    public DataTable GetFrequence()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT DISTINCT Frequence_id, Frequence FROM Frequence ORDER BY Frequence_ID";
            using var cmd = new SqlCommand(query, connection);

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la récupération des rôles: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }


    /***********************************************
     * MÉTHODES POUR LA GESTION DES ABONNEMENTS
     ************************************************/

    /*********************************************************************
     * MÉTHODE:      AjouterClientEtRetournerID
     * DESCRIPTION:  Ajoute un client et retourne son ID généré
     * PARAMÈTRES:   nom - Nom complet du client
     *               telephone - Numéro de téléphone
     *               courriel - Adresse e-mail
     *               NoMembre - Numéro de membre
     *               notes - Notes supplémentaires
     * RETOUR:       ID du client ajouté ou -1 en cas d'erreur
     *********************************************************************/
    public int AjouterClientEtRetournerID(string nom, string telephone, string courriel, int NoMembre, string notes)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Construire la requète
            var colonnes = "Nom, Notes, Telephone, NoMembre, Courriel, REF_Abonnements_ID, Statut";
            var valeurs = "@Nom, @Notes, @Telephone, @NoMembre, @Courriel, @planId, 1";

            // Utiliser des paramêtres pour éviter les injections SQL
            var query = $"INSERT INTO Clients ({colonnes}) VALUES ({valeurs}); SELECT SCOPE_IDENTITY();";
            using var cmd = new SqlCommand(query, connection);

            // Ajout des paramêtres
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@NoMembre", NoMembre);
            cmd.Parameters.AddWithValue("@Telephone", string.IsNullOrWhiteSpace(telephone) ? DBNull.Value : telephone);
            cmd.Parameters.AddWithValue("@Courriel", string.IsNullOrWhiteSpace(courriel) ? DBNull.Value : courriel);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);
            cmd.Parameters.AddWithValue("@planId", DBNull.Value); // Si pas d'abonnement, mettre à NULL ou 0


            // Exécuter et récupérer l'ID inséré
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                var id = Convert.ToInt32(result);
                MessageBox.Show("Client ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                return id;
            }

            return -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'ajout du client: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return -1;
        }
    }

    /*********************************************************************
     * MÉTHODE:      GetClientAbonnements
     * DESCRIPTION:  Récupère tous les abonnements d'un client
     * PARAMÈTRES:   clientId - Identifiant du client
     * RETOUR:       Table contenant les abonnements du client
     *********************************************************************/
    public DataTable GetClientAbonnements(int clientId)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = "SELECT * FROM ClientAbonnement WHERE REF_Client_ID = @clientId";
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la récupération des abonnements : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return new DataTable();
        }
    }

    /*********************************************************************
     * MÉTHODE:      SauvegarderClientAbonnements
     * DESCRIPTION:  Sauvegarde les abonnements d'un client (ajout/modification)
     * PARAMÈTRES:   clientId - Identifiant du client
     *               abonnements - Collection d'abonnements à sauvegarder
     * RETOUR:       Booléen indiquant si la sauvegarde a réussi
     *********************************************************************/
    public bool SauvegarderClientAbonnements(int clientId, IEnumerable<ClientAbonnementModel> abonnements)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            foreach (var abonnement in abonnements)
                if (abonnement.ClientAbonnement_id > 0)
                {
                    // Modification
                    var query = @"UPDATE ClientAbonnement SET 
                              REF_Client_ID = @clientId,
                              REF_PlanDeBase_ID = @planId, 
                              DateDebut = @dateDebut,
                              DateFin = @dateFin,
                              Statut = @statut
                              WHERE ClientAbonnement_id = @abonnementId";

                    using var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@abonnementId", abonnement.ClientAbonnement_id);
                    cmd.Parameters.AddWithValue("@clientId", abonnement.REF_Client_ID);
                    cmd.Parameters.AddWithValue("@planId", abonnement.REF_PlanDeBase_ID);
                    cmd.Parameters.AddWithValue("@dateDebut", abonnement.DateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", abonnement.DateFin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@statut", abonnement.Statut);

                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected <= 0)
                    {
                        MessageBox.Show($"Erreur lors de la modification de l'abonnement avec le plan ID {abonnement.REF_PlanDeBase_ID}.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    // Ajout
                    var query = @"INSERT INTO ClientAbonnement (REF_PlanDeBase_ID, REF_Client_ID, DateDebut, DateFin, Statut) 
                              VALUES (@planId, @clientId, @dateDebut, @dateFin, @statut)";

                    using var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@clientId", abonnement.REF_Client_ID);
                    cmd.Parameters.AddWithValue("@planId", abonnement.REF_PlanDeBase_ID);
                    cmd.Parameters.AddWithValue("@dateDebut", abonnement.DateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", abonnement.DateFin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@statut", abonnement.Statut);

                    var rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected <= 0)
                    {
                        MessageBox.Show($"Erreur lors de l'ajout de l'abonnement avec le plan ID {abonnement.REF_PlanDeBase_ID}.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde des abonnements : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }


    /*******************************************************************
     * MéTHODES POUR LA RéINITIALISATION DE LA BASE DE DONNéES
     *******************************************************************/

    /*********************************************************************
     * MÉTHODE:      ResetDatabase
     * DESCRIPTION:  Réinitialise complètement la base de données en
     *               exécutant des scripts SQL embarqués
     *********************************************************************/
    public void ResetDatabase()
    {
        try
        {
            // Afficher la fenêtre de connexion SQL
            var sqlLoginWindow = new SQLLoginWindow();
            if (sqlLoginWindow.ShowDialog() != true)
            {
                // L'utilisateur a annulé
                MessageBox.Show("Opération annulée par l'utilisateur.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Récupérer les identifiants administrateur
            var adminUsername = sqlLoginWindow.SQLUsername;
            var adminPassword = sqlLoginWindow.SQLPassword;

            // Créer une chaîne de connexion administrateur
            var adminConnectionString = $"Data Source={Settings.Default.SQLServerName};" +
                                        "Initial Catalog=master;" + // Se connecter à 'master' pour avoir des droits de création/suppression de BD
                                        $"User ID={adminUsername};" +
                                        $"Password={adminPassword};" +
                                        "TrustServerCertificate=True;";

            // Tester la connexion administrateur
            if (!TestDatabaseConnection(adminConnectionString))
            {
                MessageBox.Show("Impossible de se connecter à SQL Server avec les identifiants fournis. " +
                                "Veuillez vérifier vos informations et réessayer.",
                    "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Access the embedded resources
            var assembly = Assembly.GetExecutingAssembly();

            // Première étape: supprimer la base de données si elle existe
            var deleteScriptName = "GestGym.Scripts.DeleteDatabase.sql";
            string deleteScript;
            using (var stream = assembly.GetManifestResourceStream(deleteScriptName))
            {
                if (stream == null)
                {
                    MessageBox.Show($"Script de suppression introuvable: {deleteScriptName}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using var reader = new StreamReader(stream);
                deleteScript = reader.ReadToEnd();
            }

            // Deuxième étape: créer et initialiser la base de données
            var resetScriptName = "GestGym.Scripts.ResetDatabase.sql";
            string resetScript;
            using (var stream = assembly.GetManifestResourceStream(resetScriptName))
            {
                if (stream == null)
                {
                    MessageBox.Show($"Script de réinitialisation introuvable: {resetScriptName}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using var reader = new StreamReader(stream);
                resetScript = reader.ReadToEnd();
            }

            // Exécuter les scripts dans l'ordre
            ExecuteScriptWithBatches(deleteScript, adminConnectionString);
            ExecuteScriptWithBatches(resetScript, adminConnectionString);

            // Mise à jour des paramètres de l'application avec le compte par défaut créé par le script
            Settings.Default.SQLAccount = "GestGym_ADM";
            Settings.Default.SQLPassword = "7UqvE$mN%T7qPpQ#rr#$yzv";
            Settings.Default.Save();

            MessageBox.Show("La base de données a été réinitialisée avec succès et les nouvelles informations seront disponibles après le redémarrage de l'application. L'application sera maintenant fermée.",
                "Réinitialisation terminée", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la réinitialisation de la base de données: {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      ExecuteScriptWithBatches
     * DESCRIPTION:  Exécute un script SQL par lots séparés par GO
     * PARAMÈTRES:   sqlScript - Script SQL à exécuter
     *               connectionString - Chaîne de connexion à utiliser
     *********************************************************************/
    private void ExecuteScriptWithBatches(string sqlScript, string connectionString)
    {
        // Diviser le script en lots basés sur les commandes GO
        var batches = sqlScript.Split(new[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" },
            StringSplitOptions.RemoveEmptyEntries);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Exécuter chaque lot séparément
        foreach (var batch in batches)
            if (!string.IsNullOrWhiteSpace(batch))
            {
                using var cmd = new SqlCommand(batch, connection);
                cmd.CommandTimeout = 180; // 3 minutes
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'exécution du lot: {ex.Message}");

                    // Vous pouvez choisir de propager l'erreur ou de continuer
                    // throw; // Décommentez pour propager l'erreur
                }
            }
    }
}