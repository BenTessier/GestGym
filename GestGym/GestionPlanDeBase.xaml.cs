/*********************************************************************
 * FICHIER:        GestionPlanDeBase.xaml.cs
 * DESCRIPTION:    Contrôle utilisateur pour la gestion des plans de base
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
///     Contrôle utilisateur permettant la gestion des plans de base
///     avec ajout, modification et visualisation des données existantes.
/// </summary>
public partial class GestionPlanDeBase : UserControl
{
    private readonly Controller _controller;

    /*********************************************************************
     * CONSTRUCTEUR: GestionPlanDeBase
     * DESCRIPTION:  Initialise le contrôle utilisateur et charge les plans
     *               depuis la base de données
     * PARAMÈTRES:   controller - Instance du contrôleur de l'application
     *********************************************************************/
    public GestionPlanDeBase(Controller controller)
    {
        InitializeComponent();
        _controller = controller;
        ChargerPlans();
        PlansDataGrid.ItemsSource = Plans;
    }

    /*********************************************************************
     * PROPRIÉTÉ:    Plans
     * DESCRIPTION:  Collection observable contenant les plans de base
     *               affichés dans l'interface utilisateur
     *********************************************************************/
    private ObservableCollection<PlanDeBase> Plans { get; } = new();

    /*********************************************************************
     * MÉTHODE:      ChargerPlans
     * DESCRIPTION:  Charge les plans de base depuis la base de données
     *               et les ajoute à la collection observable
     *********************************************************************/
    private void ChargerPlans()
    {
        try
        {
            var dt = _controller.ChargerPlansDepuisLaBase();

            Plans.Clear();
            foreach (DataRow row in dt.Rows)
                Plans.Add(new PlanDeBase
                {
                    PlanBase_id = Convert.ToInt32(row["Id"]),
                    Nom = row["Nom"]?.ToString() ?? "",
                    Notes = row["Description"]?.ToString() ?? "",
                    Prix = Convert.ToSingle(row["Prix"]),
                    Duree = Convert.ToInt32(row["Duree"]),
                    DateDebutDisponible = row["DateDebutDisponible"] as DateTime?,
                    DateFinDisponible = row["DateFinDisponible"] as DateTime?,
                    Statut = Convert.ToInt32(row["Statut"])
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des plans : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /*********************************************************************
     * MÉTHODE:      PlansDataGrid_InitializingNewItem
     * DESCRIPTION:  Initialise les valeurs par défaut d'un nouveau plan
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement avec le nouvel élément
     *********************************************************************/
    private void PlansDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
    {
        if (e.NewItem is PlanDeBase nouveauPlan)
        {
            // Définir le statut par défaut à 1 (actif)
            nouveauPlan.Statut = 1;
            nouveauPlan.IsEditing = true;
        }
    }

    /*********************************************************************
     * MÉTHODE:      PlansDataGrid_BeginningEdit
     * DESCRIPTION:  Marque un plan comme étant en cours d'édition
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement d'édition
     *********************************************************************/
    private void PlansDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is PlanDeBase plan) plan.IsEditing = true;
    }

    /*********************************************************************
     * MÉTHODE:      PlansDataGrid_RowEditEnding
     * DESCRIPTION:  Gère la fin de l'édition d'une ligne dans la grille
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement de fin d'édition
     *********************************************************************/
    private void PlansDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.Row.Item is PlanDeBase plan)

            // La propriété IsEditing sera mise à jour une fois 
            // l'édition terminée (afin que le bouton reste visible pendant la validation)
            if (e.EditAction == DataGridEditAction.Cancel)
                plan.IsEditing = false;
    }

    /*********************************************************************
     * MÉTHODE:      SauvegarderButton_Click
     * DESCRIPTION:  Sauvegarde les modifications apportées aux plans
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void SauvegarderButton_Click(object sender, RoutedEventArgs e)
    {
        var plans = PlansDataGrid.ItemsSource as IEnumerable<PlanDeBase>;
        if (plans == null) return;

        var savedCount = 0;

        foreach (var plan in plans.Where(p => p.IsEditing))
        {
            if (plan.PlanBase_id == 0)
            {
                _controller.AjouterPlanDeBase(plan);
                savedCount++;
            }
            else
            {
                _controller.ModifierPlanDeBase(plan);
                savedCount++;
            }

            plan.IsEditing = false; // Reset après sauvegarde
        }

        if (savedCount > 0)
            MessageBox.Show($"{savedCount} plan(s) sauvegardé(s) avec succès.", "Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Aucun plan à sauvegarder.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

        // Ferme le formulaire (UserControl) en retirant le contenu dynamique
        var parentWindow = Window.GetWindow(this);
        if (parentWindow is MainWindow mainWindow) mainWindow.DynamicContent.Content = null;
    }

    /*********************************************************************
     * MÉTHODE:      AnnulerButton_Click
     * DESCRIPTION:  Annule les modifications et ferme le formulaire
     * PARAMÈTRES:   sender - Source de l'événement
     *               e - Arguments de l'événement
     *********************************************************************/
    private void AnnulerButton_Click(object sender, RoutedEventArgs e)
    {
        // Ferme le formulaire (UserControl) en retirant le contenu dynamique
        var parentWindow = Window.GetWindow(this);
        if (parentWindow is MainWindow mainWindow) mainWindow.DynamicContent.Content = null;
    }
}

/// <summary>
///     Classe représentant un plan de base dans le système GestGym
///     Implémente INotifyPropertyChanged pour la notification des changements de propriétés
/// </summary>
public class PlanDeBase
{
    private bool _isEditing;

    /*********************************************************************
     * PROPRIÉTÉ:    PlanBase_id
     * DESCRIPTION:  Identifiant unique du plan de base dans la base de données
     *********************************************************************/
    public int PlanBase_id { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Nom
     * DESCRIPTION:  Nom du plan de base
     *********************************************************************/
    public string Nom { get; set; } = "";

    /*********************************************************************
     * PROPRIÉTÉ:    Notes
     * DESCRIPTION:  Description détaillée ou notes concernant le plan
     *********************************************************************/
    public string Notes { get; set; } = "";

    /*********************************************************************
     * PROPRIÉTÉ:    Prix
     * DESCRIPTION:  Prix du plan de base
     *********************************************************************/
    public float Prix { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Duree
     * DESCRIPTION:  Durée du plan en jours/mois selon la configuration
     *********************************************************************/
    public int Duree { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    DateDebutDisponible
     * DESCRIPTION:  Date à partir de laquelle le plan est disponible
     *********************************************************************/
    public DateTime? DateDebutDisponible { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    DateFinDisponible
     * DESCRIPTION:  Date jusqu'à laquelle le plan est disponible
     *********************************************************************/
    public DateTime? DateFinDisponible { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    Statut
     * DESCRIPTION:  État du plan (1 = actif, 2 = inactif)
     *********************************************************************/
    public int Statut { get; set; }

    /*********************************************************************
     * PROPRIÉTÉ:    IsEditing
     * DESCRIPTION:  Indique si le plan est en cours d'édition dans l'interface
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