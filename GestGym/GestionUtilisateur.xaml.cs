/*********************************************************************
 * FICHIER:        GestionUtilisateur.xaml.cs
 * DESCRIPTION:    Contrôle utilisateur pour la gestion des utilisateurs
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GestGym;

/// <summary>
///     Contrôle utilisateur permettant l'ajout, la modification et la consultation
///     des utilisateurs du système GestGym.
/// </summary>
public partial class GestionUtilisateur : UserControl
{
    private readonly Controller _controller;
    private readonly bool _modeModification;
    private readonly bool _readOnly;
    private int? _utilisateurId;

    /*********************************************************************
     * CONSTRUCTEUR: GestionUtilisateur
     * DESCRIPTION:  Initialise le contrôle utilisateur et configure le mode
     *               d'affichage selon les paramètres fournis
     * PARAMÈTRES:   controller - Instance du contrôleur de l'application
     *               modeModification - Indique si le mode est en modification
     *               readOnly - Indique si le mode est en lecture seule
     *********************************************************************/
    public GestionUtilisateur(Controller controller, bool modeModification, bool readOnly)
    {
        InitializeComponent();
        _controller = controller;
        _modeModification = modeModification;
        _readOnly = readOnly;

        // Configuration selon le mode
        ConfigurerMode();

        // Charger les listes déroulantes
        ChargerRolesEtGroupes();

        if (!_modeModification && !_readOnly) DatePicker.SelectedDate = DateTime.Today;

        // Si mode modification, ajouter le gestionnaire d'événement pour la recherche
        if (TitreTextBlock.Text == "CONSULTER UN UTILISATEUR" || TitreTextBlock.Text == "MODIFIER UN UTILISATEUR")
            RechercheTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter) BtnRechercher_Click(this, new RoutedEventArgs());
            };

        if (_readOnly)
            SetFieldsReadOnly();
    }

    /*********************************************************************
     * MÉTHODE:      SetFieldsReadOnly
     * DESCRIPTION:  Configure tous les champs du formulaire en mode
     *               lecture seule
     *********************************************************************/
    private void SetFieldsReadOnly()
    {
        // Parcourez tous les champs et mettez-les en lecture seule
        NomTextBox.IsReadOnly = true;
        TelephoneTextBox.IsReadOnly = true;

        // Répétez pour tous les champs pertinents
        // Pour les ComboBox, utilisez IsEnabled = false
        RoleComboBox.IsEnabled = false;
        GroupeComboBox.IsEnabled = false;

        // Désactivez les boutons de sauvegarde/modification si nécessaire
        BtnAction.IsEnabled = false;
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
            // Mode consultation (lecture seule)
            TitreTextBlock.Text = "CONSULTER UN UTILISATEUR";
            RecherchePanel.Visibility = Visibility.Visible;
            StatutPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;
            BtnAction.Visibility = Visibility.Collapsed; // Masquer le bouton d'action
        }
        else if (_modeModification)
        {
            // Mode modification
            TitreTextBlock.Text = "MODIFIER UN UTILISATEUR";
            RecherchePanel.Visibility = Visibility.Visible;
            StatutPanel.Visibility = Visibility.Visible;
            FormPanel.Visibility = Visibility.Collapsed; // Caché jusqu'à la recherche
            BtnAction.Content = "Sauvegarder";
            BtnAction.Visibility = Visibility.Visible;
        }
        else
        {
            // Mode ajout
            TitreTextBlock.Text = "AJOUTER UN UTILISATEUR";
            RecherchePanel.Visibility = Visibility.Collapsed;
            StatutPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;
            BtnAction.Content = "Ajouter";
            BtnAction.Visibility = Visibility.Visible;
        }
    }

    /*********************************************************************
     * MÉTHODE:      ChargerRolesEtGroupes
     * DESCRIPTION:  Charge les rôles et groupes depuis la base de données
     *               et les affiche dans les listes déroulantes
     *********************************************************************/
    private void ChargerRolesEtGroupes()
    {
        // Charger les rôles
        var roles = _controller.GetRoles();
        RoleComboBox.ItemsSource = roles.DefaultView;

        // Ajouter un élément vide en premier
        var emptyRoleRow = roles.NewRow();
        emptyRoleRow["Role_id"] = DBNull.Value;
        emptyRoleRow["Nom"] = "-- Sélectionner --";
        roles.Rows.InsertAt(emptyRoleRow, 0);

        // Charger les groupes
        var groupes = _controller.GetGroupes();
        GroupeComboBox.ItemsSource = groupes.DefaultView;

        // Ajouter un élément vide en premier
        var emptyGroupeRow = groupes.NewRow();
        emptyGroupeRow["Groupe_id"] = DBNull.Value;
        emptyGroupeRow["Nom"] = "-- Sélectionner --";
        groupes.Rows.InsertAt(emptyGroupeRow, 0);
    }

    /*********************************************************************
     * MÉTHODE:      BtnRechercher_Click
     * DESCRIPTION:  Recherche un utilisateur selon les critères saisis
     *               et affiche ses informations dans le formulaire
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void BtnRechercher_Click(object sender, RoutedEventArgs e)
    {
        var recherche = RechercheTextBox.Text.Trim();
        if (string.IsNullOrEmpty(recherche))
        {
            MessageBox.Show("Veuillez entrer un nom, identifiant ou numéro d'employé.", "Recherche", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Recherche par nom, identifiant ou numéro d'employé
        var row = _controller.RechercherUtilisateur(recherche);
        if (row == null)
        {
            MessageBox.Show("Aucun utilisateur trouvé.", "Recherche", MessageBoxButton.OK, MessageBoxImage.Information);
            FormPanel.Visibility = Visibility.Collapsed;
            return;
        }

        if (!int.TryParse(row["Statut"]?.ToString(), out var statut)) statut = 1; // Default value if parsing fails
        StatutActifRadio.IsChecked = statut == 1;
        StatutInactifRadio.IsChecked = statut == 2;
        _utilisateurId = Convert.ToInt32(row["Id"]);
        NomTextBox.Text = row["Nom"]?.ToString();
        TelephoneTextBox.Text = row["Telephone"]?.ToString();
        NumEmployeTextBox.Text = row["NumEmploye"]?.ToString();
        CourrielTextBox.Text = row["Courriel"]?.ToString();
        IdentifiantTextBox.Text = row["Identifiant"]?.ToString();
        MotDePasseTextBox.Password = row["MotDePasse"]?.ToString();
        DatePicker.SelectedDate = row["DateInscription"] as DateTime?;
        NotesRichTextBox.Document.Blocks.Clear();
        NotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(row["Notes"]?.ToString() ?? "")));

        // Ajout pour la sélection de rôle et groupe
        if (int.TryParse(row["REF_Roles_id"]?.ToString(), out var Role_id))

            // Sélectionner le rôle correspondant
            foreach (DataRowView rowView in RoleComboBox.Items)
                if (rowView["Role_id"] != DBNull.Value && Convert.ToInt32(rowView["Role_id"]) == Role_id)
                {
                    RoleComboBox.SelectedItem = rowView;
                    break;
                }

        if (int.TryParse(row["REF_Groupes_id"]?.ToString(), out var Groupe_id))

            // Sélectionner le groupe correspondant
            foreach (DataRowView rowView in GroupeComboBox.Items)
                if (rowView["Groupe_id"] != DBNull.Value && Convert.ToInt32(rowView["Groupe_id"]) == Groupe_id)
                {
                    GroupeComboBox.SelectedItem = rowView;
                    break;
                }

        FormPanel.Visibility = Visibility.Visible;
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
        if (_modeModification && _utilisateurId == null)
        {
            MessageBox.Show("Aucun utilisateur sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Validation des entrées communes
        if (string.IsNullOrWhiteSpace(NomTextBox.Text))
        {
            MessageBox.Show("Le champ Nom est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Conversion du numéro d'employé en entier
        if (!int.TryParse(NumEmployeTextBox.Text, out var numEmploye) && !string.IsNullOrWhiteSpace(NumEmployeTextBox.Text))
        {
            MessageBox.Show("Le numéro d'employé doit être un entier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Récupérer les valeurs des champs
        var nom = NomTextBox.Text;
        var telephone = TelephoneTextBox.Text;
        var courriel = CourrielTextBox.Text;
        var identifiant = IdentifiantTextBox.Text;
        var motDePasse = MotDePasseTextBox.Password;
        var dateInscription = DatePicker.SelectedDate;

        // Récupérer les ID des rôles et groupes sélectionnés
        int? Role_id = null;
        int? Groupe_id = null;

        if (RoleComboBox.SelectedItem != null && RoleComboBox.SelectedValue != null && RoleComboBox.SelectedValue != DBNull.Value)
            Role_id = Convert.ToInt32(((DataRowView)RoleComboBox.SelectedItem)["Role_id"]);

        if (GroupeComboBox.SelectedItem != null && GroupeComboBox.SelectedValue != null && GroupeComboBox.SelectedValue != DBNull.Value)
            Groupe_id = Convert.ToInt32(((DataRowView)GroupeComboBox.SelectedItem)["Groupe_id"]);

        // Récupérer le texte des notes
        var textRange = new TextRange(
            NotesRichTextBox.Document.ContentStart,
            NotesRichTextBox.Document.ContentEnd);
        var notes = textRange.Text ?? string.Empty;

        if (_modeModification)
        {
            // Mode modification
            var statut = StatutActifRadio.IsChecked == true ? 1 : 2;
            if (_utilisateurId.HasValue)
            {
                _controller.ModifierUtilisateur(
                    _utilisateurId.Value,
                    nom,
                    telephone,
                    numEmploye,
                    courriel,
                    identifiant,
                    motDePasse,
                    dateInscription,
                    notes,
                    statut,
                    Role_id,
                    Groupe_id
                );
            }
            else
            {
                MessageBox.Show("Aucun utilisateur sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            // Mode ajout
            _controller.AjouterUtilisateur(
                nom,
                telephone,
                numEmploye,
                courriel,
                identifiant,
                motDePasse,
                dateInscription,
                notes,
                Role_id,
                Groupe_id
            );
        }

        ReinitialiserFormulaire();
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
        NumEmployeTextBox.Clear();
        CourrielTextBox.Clear();
        IdentifiantTextBox.Clear();
        MotDePasseTextBox.Clear();
        DatePicker.SelectedDate = null;
        NotesRichTextBox.Document.Blocks.Clear();

        // Réinitialiser les ComboBox
        if (RoleComboBox.Items.Count > 0)
            RoleComboBox.SelectedIndex = 0;

        if (GroupeComboBox.Items.Count > 0)
            GroupeComboBox.SelectedIndex = 0;

        if (_modeModification)
        {
            _utilisateurId = null;
            StatutActifRadio.IsChecked = true;
        }
    }
}