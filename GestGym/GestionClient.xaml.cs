/*********************************************************************
 * FICHIER:        GestionClient.xaml.cs
 * DESCRIPTION:    Contrôle utilisateur pour la gestion des clients
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GestGym.Models;

namespace GestGym;

/// <summary>
///     Contrôle utilisateur permettant l'ajout, la modification et la consultation
///     des clients ainsi que de leurs abonnements.
/// </summary>
public partial class GestionClient : UserControl
{
    // Collection pour stocker les abonnements du client
    private readonly ObservableCollection<ClientAbonnementModel> _abonnements = [];
    private readonly Controller _controller;
    private readonly bool _modeModification;
    private readonly bool _readOnly;
    private int? _clientId;

    /*********************************************************************
     * CONSTRUCTEUR: GestionClient
     * DESCRIPTION:  Initialise le contrôle utilisateur et configure le mode
     *               d'affichage selon les paramètres fournis
     * PARAMÈTRES:   controller - Instance du contrôleur de l'application
     *               modeModification - Indique si le mode est en modification
     *               readOnly - Indique si le mode est en lecture seule
     *********************************************************************/
    public GestionClient(Controller controller, bool modeModification, bool readOnly = false)
    {
        InitializeComponent();

        // Ajouter le convertisseur aux ressources
        Resources.Add("BooleanConverter", new BooleanConverter());

        _controller = controller;
        _modeModification = modeModification;
        _readOnly = readOnly;

        ConfigurerMode();
        ChargerPlans();

        // Initialiser le DataGrid des abonnements
        AbonnementsDataGrid.ItemsSource = _abonnements;

        // Définir la date du jour comme date de début par défaut
        if (!_modeModification && !_readOnly)

            // Ajouter un abonnement vide par défaut
            _abonnements.Add(new ClientAbonnementModel
            {
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddMonths(12)
            });

        if (TitreTextBlock.Text == "CONSULTER UN CLIENT" || TitreTextBlock.Text == "MODIFIER UN CLIENT")
            RechercheTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                    BtnRechercher_Click(this, new RoutedEventArgs());
            };

        if (_readOnly)
            SetFieldsReadOnly();
    }

    /*********************************************************************
     * MÉTHODE:      ChargerPlans
     * DESCRIPTION:  Charge les plans disponibles depuis la base de données
     *               pour les afficher dans la liste déroulante
     *********************************************************************/
    private void ChargerPlans()
    {
        try
        {
            var plans = _controller.ChargerPlansDepuisLaBase();

            // Configurer la source de données pour la colonne de plan
            PlanColumn.ItemsSource = plans.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des plans : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      SetFieldsReadOnly
     * DESCRIPTION:  Configure tous les champs du formulaire en mode
     *               lecture seule
     *********************************************************************/
    private void SetFieldsReadOnly()
    {
        NomTextBox.IsReadOnly = true;
        TelephoneTextBox.IsReadOnly = true;
        CourrielTextBox.IsReadOnly = true;
        NoMembreTextBox.IsReadOnly = true;
        NotesRichTextBox.IsReadOnly = true;

        // Désactiver le DataGrid des abonnements
        AbonnementsDataGrid.IsReadOnly = true;
        AbonnementsDataGrid.CanUserAddRows = false;
        AbonnementsDataGrid.CanUserDeleteRows = false;

        // Désactiver les boutons d'ajout/suppression
        BtnAction.IsEnabled = false;
        BtnAction.Visibility = Visibility.Collapsed;
    }

    /*********************************************************************
     * MÉTHODE:      ConfigurerMode
     * DESCRIPTION:  Configure l'interface utilisateur selon le mode actif
     *               (consultation, modification ou ajout)
     *********************************************************************/
    private void ConfigurerMode()
    {
        if (_readOnly)
        {
            TitreTextBlock.Text = "CONSULTER UN CLIENT";
            RecherchePanel.Visibility = Visibility.Visible;
            FormPanel.Visibility = Visibility.Visible;
            BtnAction.Visibility = Visibility.Collapsed;
        }
        else if (_modeModification)
        {
            TitreTextBlock.Text = "MODIFIER UN CLIENT";
            RecherchePanel.Visibility = Visibility.Visible;
            FormPanel.Visibility = Visibility.Collapsed;
            BtnAction.Content = "Sauvegarder";
            BtnAction.Visibility = Visibility.Visible;
        }
        else
        {
            TitreTextBlock.Text = "AJOUTER UN CLIENT";
            RecherchePanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;
            BtnAction.Content = "Ajouter";
            BtnAction.Visibility = Visibility.Visible;
        }
    }

    /*********************************************************************
     * MÉTHODE:      BtnRechercher_Click
     * DESCRIPTION:  Recherche un client selon les critères saisis
     *               et affiche ses informations dans le formulaire
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void BtnRechercher_Click(object sender, RoutedEventArgs e)
    {
        var recherche = RechercheTextBox.Text.Trim();
        if (string.IsNullOrEmpty(recherche))
        {
            MessageBox.Show("Veuillez entrer un nom, prénom ou téléphone.", "Recherche", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Recherche par nom, prénom ou téléphone
        var row = _controller.RechercherClient(recherche);
        if (row == null)
        {
            MessageBox.Show("Aucun client trouvé.", "Recherche", MessageBoxButton.OK, MessageBoxImage.Information);
            FormPanel.Visibility = Visibility.Collapsed;
            return;
        }

        _clientId = Convert.ToInt32(row["Client_id"]);
        NomTextBox.Text = row["Nom"]?.ToString() ?? "";
        TelephoneTextBox.Text = row["Telephone"]?.ToString() ?? "";
        CourrielTextBox.Text = row["Courriel"]?.ToString() ?? "";
        NoMembreTextBox.Text = row["NoMembre"]?.ToString() ?? "";
        NotesRichTextBox.Document.Blocks.Clear();
        NotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(row["Notes"]?.ToString() ?? "")));

        // Charger les abonnements du client s'ils existent
        if (_clientId.HasValue) ChargerAbonnements(_clientId.Value);

        FormPanel.Visibility = Visibility.Visible;
    }

    /*********************************************************************
     * MÉTHODE:      ChargerAbonnements
     * DESCRIPTION:  Charge les abonnements d'un client depuis la base
     *               de données et les affiche dans la grille
     * PARAMÈTRES:   clientId - Identifiant du client dont on veut
     *               charger les abonnements
     *********************************************************************/
    private void ChargerAbonnements(int clientId)
    {
        try
        {
            // Vider la collection actuelle
            _abonnements.Clear();

            // Récupérer les abonnements depuis la base de données
            var abonnementsTable = _controller.GetClientAbonnements(clientId);

            if (abonnementsTable != null && abonnementsTable.Rows.Count > 0)
                foreach (DataRow row in abonnementsTable.Rows)
                {
                    var abonnement = new ClientAbonnementModel
                    {
                        ClientAbonnement_id = Convert.ToInt32(row["ClientAbonnement_id"]),
                        REF_Client_ID = clientId,
                        REF_PlanDeBase_ID = Convert.ToInt32(row["REF_PlanDeBase_ID"]),
                        DateDebut = row["DateDebut"] != DBNull.Value ? Convert.ToDateTime(row["DateDebut"]) : DateTime.Today,
                        DateFin = row["DateFin"] != DBNull.Value ? Convert.ToDateTime(row["DateFin"]) : null,
                        Statut = row["Statut"] != DBNull.Value ? Convert.ToInt32(row["Statut"]) : 1,
                        IsNew = false
                    };

                    _abonnements.Add(abonnement);
                }

            // Si aucun abonnement n'a été trouvé et qu'on est en mode modification, ajouter une ligne vide
            if (_abonnements.Count == 0 && _modeModification)
                _abonnements.Add(new ClientAbonnementModel
                {
                    REF_Client_ID = clientId,
                    DateDebut = DateTime.Today
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des abonnements : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      EffacerAbonnementButton_Click
     * DESCRIPTION:  Supprime un abonnement sélectionné ou le marque comme
     *               supprimé selon le mode d'affichage
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void EffacerAbonnementButton_Click(object sender, RoutedEventArgs e)
    {
        if (AbonnementsDataGrid.SelectedItem is ClientAbonnementModel selectedAbonnement)
        {
            if (_modeModification)
            {
                // If in modification mode, set Statut to 0
                selectedAbonnement.Statut = 0;
                MessageBox.Show($"L'abonnement '{selectedAbonnement.PlanNom}' a été marqué comme supprimé.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Otherwise, remove the subscription from the list
                _abonnements.Remove(selectedAbonnement);
                MessageBox.Show($"L'abonnement '{selectedAbonnement.PlanNom}' a été retiré de la liste.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            MessageBox.Show("Veuillez sélectionner un abonnement à effacer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      BtnAction_Click
     * DESCRIPTION:  Gère l'action principale du formulaire (ajout ou
     *               modification) selon le mode actif
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void BtnAction_Click(object sender, RoutedEventArgs e)
    {
        if (_modeModification && _clientId == null)
        {
            MessageBox.Show("Aucun client sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Validation des entrées communes
        if (string.IsNullOrWhiteSpace(NomTextBox.Text))
        {
            MessageBox.Show("Le champ Nom est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Récupérer les valeurs des champs
        var nom = NomTextBox.Text.Trim();
        var telephone = TelephoneTextBox.Text.Trim();
        var courriel = CourrielTextBox.Text.Trim();

        // Conversion du numéro d'employé en entier
        if (!int.TryParse(NoMembreTextBox.Text, out var noMembre) && !string.IsNullOrWhiteSpace(NoMembreTextBox.Text))
        {
            MessageBox.Show("Le numéro de membre doit être un entier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Récupérer le texte des notes
        var textRange = new TextRange(
            NotesRichTextBox.Document.ContentStart,
            NotesRichTextBox.Document.ContentEnd);
        var notes = textRange.Text?.Trim() ?? string.Empty;

        bool clientSuccess;
        int clientId;

        if (_modeModification)
        {
            // Mode modification
            if (_clientId.HasValue)
            {
                clientSuccess = _controller.ModifierClient(_clientId.Value, nom, telephone, courriel, noMembre, notes);
            }
            else
            {
                MessageBox.Show("Aucun client sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            clientId = _clientId.Value;
        }
        else
        {
            // Mode ajout
            clientId = _controller.AjouterClientEtRetournerID(nom, telephone, courriel, noMembre, notes);
            clientSuccess = clientId > 0;
        }

        if (clientSuccess)
        {
            // Mettre à jour l'ID client pour tous les abonnements
            foreach (var abonnement in _abonnements) abonnement.REF_Client_ID = clientId;

            // Gérer les abonnements
            var abonnementsSuccess = GererAbonnements(clientId);

            if (abonnementsSuccess)
            {
                MessageBox.Show("Client et abonnements sauvegardés avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                if (!_modeModification)
                    ReinitialiserFormulaire();
            }
        }
    }

    /*********************************************************************
     * MÉTHODE:      GererAbonnements
     * DESCRIPTION:  Valide et sauvegarde les abonnements d'un client
     * PARAMÈTRES:   clientId - Identifiant du client propriétaire des abonnements
     * RETOUR:       Booléen indiquant si la sauvegarde a réussi
     *********************************************************************/
    private bool GererAbonnements(int clientId)
    {
        try
        {
            // Validation des abonnements avant sauvegarde
            foreach (var abonnement in _abonnements)
            {
                if (abonnement.REF_PlanDeBase_ID <= 0)
                {
                    MessageBox.Show("Veuillez sélectionner un plan pour chaque abonnement.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Si pas de date de début définie, utiliser la date du jour
                if (abonnement.DateDebut == default) abonnement.DateDebut = DateTime.Today;
            }

            // Passer tous les abonnements à la méthode SauvegarderClientAbonnements
            var success = _controller.SauvegarderClientAbonnements(clientId, _abonnements);

            if (!success) MessageBox.Show("Erreur lors de la sauvegarde des abonnements.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

            return success;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde des abonnements : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /*********************************************************************
     * MÉTHODE:      BtnAnnuler_Click
     * DESCRIPTION:  Annule l'opération en cours et ferme le formulaire
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        // Réinitialiser le formulaire dans tous les cas
        ReinitialiserFormulaire();

        // Trouver le ContentControl parent
        var contentControl = FindParentContentControl();
        if (contentControl != null)
        {
            // Effacer le contenu du ContentControl pour "fermer" le UserControl
            contentControl.Content = null;
        }
        else
        {
            // Fallback si on ne trouve pas le ContentControl
            if (_modeModification)
            {
                FormPanel.Visibility = Visibility.Collapsed;
                RechercheTextBox.Clear();
            }
        }
    }

    /*********************************************************************
     * MÉTHODE:      FindParentContentControl
     * DESCRIPTION:  Recherche le ContentControl parent dans l'arbre visuel
     * RETOUR:       Le ContentControl parent ou null si non trouvé
     *********************************************************************/
    private ContentControl? FindParentContentControl()
    {
        DependencyObject? parent = this;

        // Remonter la hiérarchie visuelle jusqu'à trouver un ContentControl
        while (parent is not null and not ContentControl) parent = VisualTreeHelper.GetParent(parent);

        return parent as ContentControl;
    }

    /*********************************************************************
     * MÉTHODE:      ReinitialiserFormulaire
     * DESCRIPTION:  Réinitialise tous les champs du formulaire
     *********************************************************************/
    private void ReinitialiserFormulaire()
    {
        NomTextBox.Clear();
        TelephoneTextBox.Clear();
        CourrielTextBox.Clear();
        NoMembreTextBox.Clear();
        NotesRichTextBox.Document.Blocks.Clear();

        // Réinitialiser les abonnements
        _abonnements.Clear();
        _abonnements.Add(new ClientAbonnementModel
        {
            DateDebut = DateTime.Today,
            DateFin = DateTime.Today.AddMonths(12)
        });

        if (_modeModification) _clientId = null;
    }
}