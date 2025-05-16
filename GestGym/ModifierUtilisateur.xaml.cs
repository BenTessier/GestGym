using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GestGym
{
    public partial class ModifierUtilisateur : UserControl
    {
        private readonly Controller _controller;
        private int? utilisateurId = null;

        [Experimental("WPF0001")]
        public ModifierUtilisateur(Controller controller)
        {
            InitializeComponent();
            _controller = controller;
            RechercheTextBox.KeyDown += RechercheTextBox_KeyDown;
        }

        private void RechercheTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnRechercher_Click(BtnRechercher_Click, new RoutedEventArgs());
            }
        }

        private void BtnRechercher_Click(object sender, RoutedEventArgs e)
        {
            string recherche = RechercheTextBox.Text.Trim();
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

            int statut = 1;
            int.TryParse(row["Statut"]?.ToString(), out statut);
            StatutActifRadio.IsChecked = (statut == 1);
            StatutInactifRadio.IsChecked = (statut == 2);
            utilisateurId = Convert.ToInt32(row["Id"]);
            NomTextBox.Text = row["Nom"]?.ToString();
            TelephoneTextBox.Text = row["Telephone"]?.ToString();
            NumEmployeTextBox.Text = row["NumEmploye"]?.ToString();
            CourrielTextBox.Text = row["Courriel"]?.ToString();
            IdentifiantTextBox.Text = row["Identifiant"]?.ToString();
            MotDePasseTextBox.Password = row["MotDePasse"]?.ToString();
            DatePicker.SelectedDate = row["DateInscription"] as DateTime?;
            NotesRichTextBox.Document.Blocks.Clear();
            NotesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(row["Notes"]?.ToString() ?? "")));

            FormPanel.Visibility = Visibility.Visible;
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            if (utilisateurId == null)
            {
                MessageBox.Show("Aucun utilisateur sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //var mainWindow = Window.GetWindow(this) as MainWindow;
            //var controller = mainWindow?.DataContext as Controller;

            // Conversion du numéro d'employé en entier
            if (!int.TryParse(NumEmployeTextBox.Text, out int numEmploye))
            {
                MessageBox.Show("Le numéro d'employé doit être un entier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TextRange textRange = new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd);
            string notes = textRange.Text ?? string.Empty;

            int statut = StatutActifRadio.IsChecked == true ? 1 : 2;
            _controller.ModifierUtilisateur(
                utilisateurId.Value,
                NomTextBox.Text,
                TelephoneTextBox.Text,
                numEmploye, // Variable convertie en int
                CourrielTextBox.Text,
                IdentifiantTextBox.Text,
                MotDePasseTextBox.Password,
                DatePicker.SelectedDate,
                notes,
                statut
            );
        }
    }
}
