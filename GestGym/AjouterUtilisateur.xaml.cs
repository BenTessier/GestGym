using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GestGym
{
   
    public partial class AjouterUtilisateur : UserControl
    {
        private readonly Controller _controller;
        public AjouterUtilisateur(Controller controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        // Bouton d'ajout d'utilisateur
        [Experimental("WPF0001")]
        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Validation des entrées
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Le champ Nom est obligatoire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Obtenir une référence au Controller
            //var mainWindow = Window.GetWindow(this) as MainWindow;
            //var controller = mainWindow?.DataContext as Controller;

            // Récupérer les valeurs des champs
            string nom = NomTextBox.Text;
            string telephone = TelephoneTextBox.Text;

            // Convertir le texte en entier
            int numEmploye = 0;
            if (!string.IsNullOrWhiteSpace(NumEmployeTextBox.Text) &&
                !int.TryParse(NumEmployeTextBox.Text, out numEmploye))
            {
                MessageBox.Show("Le numéro d'employé doit être un entier valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string courriel = CourrielTextBox.Text;
            string identifiant = IdentifiantTextBox.Text;
            string motDePasse = MotDePasseTextBox.Password;
            DateTime? dateInscription = DatePicker.SelectedDate;

            // Récupérer le texte des notes (assurez-vous que ce soit compatible nvarchar(max))
            TextRange textRange = new TextRange(
                NotesRichTextBox.Document.ContentStart,
                NotesRichTextBox.Document.ContentEnd);

            // S'assurer que le texte est traité comme un nvarchar(max)
            string notes = textRange.Text ?? string.Empty;

            // Appeler la méthode du controller pour ajouter l'utilisateur
            _controller.AjouterUtilisateur(nom, telephone, numEmploye, courriel, identifiant, motDePasse, dateInscription, notes);
            ReinitialiserFormulaireUtilisateur();
        }

        // Bouton d'annulation
        [Experimental("WPF0001")]
        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Obtenir une référence au Controller
            //var mainWindow = Window.GetWindow(this) as MainWindow;
            //var controller = mainWindow?.DataContext as Controller;

            ReinitialiserFormulaireUtilisateur();
        }

        public void ReinitialiserFormulaireUtilisateur()
        {
            NomTextBox.Clear();
            TelephoneTextBox.Clear();
            NumEmployeTextBox.Clear();
            CourrielTextBox.Clear();
            IdentifiantTextBox.Clear();
            MotDePasseTextBox.Clear();
            DatePicker.SelectedDate = null;
            NotesRichTextBox.Document.Blocks.Clear();
        }
    }
}
