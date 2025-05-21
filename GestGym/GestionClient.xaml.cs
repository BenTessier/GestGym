using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GestGym;

public partial class GestionClient : UserControl
{
    private readonly Controller _controller;
    private readonly bool _modeModification;
    private readonly bool _readOnly;
    private int? _clientId;

    public GestionClient(Controller controller, bool modeModification, bool readOnly = false)
    {
        InitializeComponent();
        _controller = controller;
        _modeModification = modeModification;
        _readOnly = readOnly;

        ConfigurerMode();

        if (TitreTextBlock.Text == "CONSULTER UN CLIENT" || TitreTextBlock.Text == "MODIFIER UN CLIENT")
            RechercheTextBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                    BtnRechercher_Click(this, new RoutedEventArgs());
            };

        if (_readOnly)
            SetFieldsReadOnly();
    }

    private void SetFieldsReadOnly()
    {
        NomTextBox.IsReadOnly = true;
        TelephoneTextBox.IsReadOnly = true;
        CourrielTextBox.IsReadOnly = true;
        NoMembreTextBox.IsReadOnly = true;
        NotesRichTextBox.IsReadOnly = true; // Si ce n'est pas possible, désactivez-le :

        // NotesRichTextBox.IsEnabled = false;

        BtnAction.IsEnabled = false;
        BtnAction.Visibility = Visibility.Collapsed;
    }

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

        FormPanel.Visibility = Visibility.Visible;
    }

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

        if (_modeModification)
        {
            // Mode modification
            var success = _controller.ModifierClient(_clientId.Value, nom, telephone, courriel, noMembre, notes);

            if (success) MessageBox.Show("Client modifié avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Mode ajout
            var success = _controller.AjouterClient(nom, telephone, courriel, noMembre, notes);

            if (success)
            {
                MessageBox.Show("Client ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                ReinitialiserFormulaire();
            }
        }
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
        CourrielTextBox.Clear();
        NoMembreTextBox.Clear();
        NotesRichTextBox.Document.Blocks.Clear();

        if (_modeModification) _clientId = null;
    }
}