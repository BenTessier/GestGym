using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GestGym;

public partial class GestionUtilisateur : UserControl
{
    private readonly Controller _controller;
    private readonly bool _modeModification;
    private readonly bool _readOnly;
    private int? _utilisateurId;

    public GestionUtilisateur(Controller controller, bool modeModification, bool readOnly)
    {
        InitializeComponent();
        _controller = controller;
        _modeModification = modeModification;
        InitializeComponent();
        _readOnly = readOnly;


        // Configuration selon le mode
        ConfigurerMode();

        // Charger les listes déroulantes
        ChargerRolesEtGroupes();

        // Si mode modification, ajouter le gestionnaire d'événement pour la recherche
        if (TitreTextBlock.Text == "CONSULTER UN UTILISATEUR" || TitreTextBlock.Text == "MODIFIER UN UTILISATEUR")
            RechercheTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter) BtnRechercher_Click(this, new RoutedEventArgs());
            };

        if (_readOnly)
            SetFieldsReadOnly();
    }

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

        var statut = 1;
        int.TryParse(row["Statut"]?.ToString(), out statut);
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
        int Role_id;
        if (int.TryParse(row["REF_Roles_id"]?.ToString(), out Role_id))

            // Sélectionner le rôle correspondant
            foreach (DataRowView rowView in RoleComboBox.Items)
                if (rowView["Role_id"] != DBNull.Value && Convert.ToInt32(rowView["Role_id"]) == Role_id)
                {
                    RoleComboBox.SelectedItem = rowView;
                    break;
                }

        int Groupe_id;
        if (int.TryParse(row["REF_Groupes_id"]?.ToString(), out Groupe_id))

            // Sélectionner le groupe correspondant
            foreach (DataRowView rowView in GroupeComboBox.Items)
                if (rowView["Groupe_id"] != DBNull.Value && Convert.ToInt32(rowView["Groupe_id"]) == Role_id)
                {
                    GroupeComboBox.SelectedItem = rowView;
                    break;
                }

        FormPanel.Visibility = Visibility.Visible;
    }

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

        if (RoleComboBox.SelectedItem != null && RoleComboBox.SelectedValue != null && RoleComboBox.SelectedValue != DBNull.Value) Role_id = Convert.ToInt32(((DataRowView)RoleComboBox.SelectedItem)["Role_id"]);

        if (GroupeComboBox.SelectedItem != null && GroupeComboBox.SelectedValue != null && GroupeComboBox.SelectedValue != DBNull.Value) Groupe_id = Convert.ToInt32(((DataRowView)GroupeComboBox.SelectedItem)["Groupe_id"]);

        // Récupérer le texte des notes
        var textRange = new TextRange(
            NotesRichTextBox.Document.ContentStart,
            NotesRichTextBox.Document.ContentEnd);
        var notes = textRange.Text ?? string.Empty;

        if (_modeModification)
        {
            // Mode modification
            var statut = StatutActifRadio.IsChecked == true ? 1 : 2;
            var success = _controller.ModifierUtilisateur(
                _utilisateurId.Value,
                nom,
                telephone,
                numEmploye, courriel,
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

    // Méthode pour trouver le ContentControl parent
    private ContentControl FindParentContentControl()
    {
        DependencyObject parent = this;

        // Remonter la hiérarchie visuelle jusqu'à trouver un ContentControl
        while (parent != null && !(parent is ContentControl)) parent = VisualTreeHelper.GetParent(parent);

        return parent as ContentControl;
    }

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