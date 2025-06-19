/*********************************************************************
 * FICHIER:        GestionOptionDeBase.xaml.cs
 * DESCRIPTION:    Contrôle utilisateur pour la gestion des options de base
 * DATE:           Juin 2025
 * AUTEUR:         Benoit Tessier
 *********************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GestGym;

/// <summary>
///     Contrôle utilisateur permettant la gestion des options de base
///     avec ajout, modification et visualisation des données existantes.
/// </summary>
public partial class GestionOptionDeBase : UserControl
{
    private readonly Controller _controller;

    /*********************************************************************
     * CONSTRUCTEUR: GestionOptionDeBase
     * DESCRIPTION:  Initialise le contrôle utilisateur et charge les options
     *               et les fréquences depuis la base de données
     * PARAMÈTRES:   controller - Instance du contrôleur de l'application
     *********************************************************************/
    public GestionOptionDeBase(Controller controller)
    {
        InitializeComponent();
        _controller = controller;
        Frequences = new DataView(); // Initialize Frequences to avoid null
        ChargerFrequence();
        ChargerOptions();
        OptionsDataGrid.ItemsSource = Options;
    }

    /*********************************************************************
     * PROPRIÉTÉ:    Frequences
     * DESCRIPTION:  Vue de données contenant les fréquences disponibles
     *               pour les options de base
     *********************************************************************/
    public DataView Frequences { get; private set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Options
     * DESCRIPTION:  Collection observable contenant les options de base
     *               affichées dans l'interface utilisateur
     *********************************************************************/
    private ObservableCollection<OptionDeBase> Options { get; } = new()
    {
        new OptionDeBase()
    };

    /*********************************************************************
     * MÉTHODE:      ChargerFrequence
     * DESCRIPTION:  Charge les fréquences depuis la base de données
     *               et les prépare pour l'affichage dans la liste déroulante
     *********************************************************************/
    private void ChargerFrequence()
    {
        try
        {
            var frequences = _controller.GetFrequence();
            if (frequences == null)
            {
                MessageBox.Show("Impossible de charger les fréquences.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Vérifiez la présence des colonnes attendues
            if (!frequences.Columns.Contains("Frequence_id") || !frequences.Columns.Contains("Frequence"))
            {
                MessageBox.Show($"La table des fréquences ne contient pas les colonnes attendues. Colonnes trouvées: {string.Join(", ", frequences.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ajoutez la ligne vide
            var emptyFrequenceRow = frequences.NewRow();
            emptyFrequenceRow["Frequence_id"] = DBNull.Value;
            emptyFrequenceRow["Frequence"] = "-- Sélectionner --";
            frequences.Rows.InsertAt(emptyFrequenceRow, 0);

            // Définir directement l'ItemsSource de la colonne ComboBox
            ((DataGridComboBoxColumn)OptionsDataGrid.Columns[7]).ItemsSource = frequences.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des fréquences : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      ChargerOptions
     * DESCRIPTION:  Charge les options de base depuis la base de données
     *               et les ajoute à la collection observable
     *********************************************************************/
    private void ChargerOptions()
    {
        try
        {
            var dt = _controller.ChargerOptionsDepuisLaBase();

            Options.Clear();
            foreach (DataRow row in dt.Rows)
                Options.Add(new OptionDeBase
                {
                    OptionBase_id = Convert.ToInt32(row["Id"]),
                    Nom = row["Nom"]?.ToString() ?? "",
                    Description = row["Description"]?.ToString() ?? "",
                    Prix = Convert.ToSingle(row["Prix"]),
                    Horaire = row["Horaire"]?.ToString() ?? "",
                    NbreSeance = Convert.ToInt32(row["NbreSeance"]),
                    REF_Frequence_ID = Convert.ToInt32(row["REF_Frequence_ID"]),
                    DateDebutDisponible = row["DateDebutDisponible"] as DateTime?,
                    DateFinDisponible = row["DateFinDisponible"] as DateTime?,
                    Statut = Convert.ToInt32(row["Statut"])
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des options : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      OptionsDataGrid_InitializingNewItem
     * DESCRIPTION:  Initialise les valeurs par défaut d'une nouvelle option
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement avec le nouvel élément
     *********************************************************************/
    private void OptionsDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
    {
        if (e.NewItem is OptionDeBase nouvelleOption)
        {
            // Définir le statut par défaut à 1 (actif)
            nouvelleOption.Statut = 1;
            nouvelleOption.IsEditing = true;
        }
    }

    /*********************************************************************
     * MÉTHODE:      OptionsDataGrid_RowEditEnding
     * DESCRIPTION:  Gère la fin de l'édition d'une ligne dans la grille
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement de fin d'édition
     *********************************************************************/
    private void OptionsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.Row.Item is OptionDeBase plan)

            // La propriété IsEditing sera mise à jour une fois 
            // l'édition terminée (afin que le bouton reste visible pendant la validation)
            if (e.EditAction == DataGridEditAction.Cancel)
                plan.IsEditing = false;
    }

    /*********************************************************************
     * MÉTHODE:      SauvegarderButton_Click
     * DESCRIPTION:  Sauvegarde les modifications apportées aux options
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void SauvegarderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var options = OptionsDataGrid.ItemsSource as IEnumerable<OptionDeBase>;
            if (options == null) return;

            var savedCount = 0;
            foreach (var option in options)
            {
                // Sauvegarde tous les éléments, qu'ils soient marqués comme modifiés ou non
                if (option.OptionBase_id == 0)
                {
                    _controller.AjouterOptionDeBase(option);
                    savedCount++;
                }
                else
                {
                    _controller.ModifierOptionDeBase(option);
                    savedCount++;
                }

                option.IsEditing = false; // Reset après sauvegarde
            }

            OptionsDataGrid.Items.Refresh();

            if (savedCount > 0)
                MessageBox.Show($"{savedCount} option(s) sauvegardée(s) avec succès.", "Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Aucune option à sauvegarder.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            // Ferme le formulaire après la sauvegarde
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
                mainWindow.DynamicContent.Content = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      AnnulerButton_Click
     * DESCRIPTION:  Annule les modifications et ferme le formulaire
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void AnnulerButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Demande confirmation si des modifications ont été effectuées
            var options = OptionsDataGrid.ItemsSource as IEnumerable<OptionDeBase>;

            // Ferme le formulaire en vidant le contenu dynamique
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
                mainWindow.DynamicContent.Content = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la fermeture : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

/// <summary>
///     Classe représentant une option de base dans le système GestGym
///     Implémente INotifyPropertyChanged pour la notification des changements de propriétés
/// </summary>
public class OptionDeBase
{
    private bool _isEditing;

    /*********************************************************************
     * PROPRIÉTÉ:    OptionBase_id
     * DESCRIPTION:  Identifiant unique de l'option de base dans la base de données
     *********************************************************************/
    public int OptionBase_id { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Nom
     * DESCRIPTION:  Nom de l'option de base
     *********************************************************************/
    public string Nom { get; set; } = "";

    /*********************************************************************
     * PROPRIÉTÉ:    Description
     * DESCRIPTION:  Description détaillée de l'option
     *********************************************************************/
    public string Description { get; set; } = "";

    /*********************************************************************
     * PROPRIÉTÉ:    Prix
     * DESCRIPTION:  Prix de l'option de base
     *********************************************************************/
    public float Prix { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Horaire
     * DESCRIPTION:  Horaire disponible pour l'option
     *********************************************************************/
    public string Horaire { get; set; } = "";

    /*********************************************************************
     * PROPRIÉTÉ:    NbreSeance
     * DESCRIPTION:  Nombre de séances incluses dans l'option
     *********************************************************************/
    public int NbreSeance { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    REF_Frequence_ID
     * DESCRIPTION:  Identifiant de la fréquence associée à l'option
     *********************************************************************/
    public int REF_Frequence_ID { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    DateDebutDisponible
     * DESCRIPTION:  Date à partir de laquelle l'option est disponible
     *********************************************************************/
    public DateTime? DateDebutDisponible { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    DateFinDisponible
     * DESCRIPTION:  Date jusqu'à laquelle l'option est disponible
     *********************************************************************/
    public DateTime? DateFinDisponible { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Statut
     * DESCRIPTION:  État de l'option (1 = actif, 2 = inactif)
     *********************************************************************/
    public int Statut { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    IsEditing
     * DESCRIPTION:  Indique si l'option est en cours d'édition dans l'interface
     *********************************************************************/
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing != value)
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }
    }

    /*********************************************************************
     * ÉVÉNEMENT:    PropertyChanged
     * DESCRIPTION:  Événement déclenché lorsqu'une propriété est modifiée
     *********************************************************************/
    public event PropertyChangedEventHandler? PropertyChanged;

    /*********************************************************************
     * MÉTHODE:      OnPropertyChanged
     * DESCRIPTION:  Déclenche l'événement PropertyChanged
     * PARAMÈTRES:   propertyName - Nom de la propriété modifiée
     *********************************************************************/
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}