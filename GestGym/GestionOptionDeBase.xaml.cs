using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Controls;

namespace GestGym;

public class GestionOptionDeBase : UserControl
{
    public GestionOptionDeBase()
    {
        InitializeComponent();
        ChargerOptions();
        OptionsDataGrid.ItemsSource = Options;
    }

    private ObservableCollection<OptionDeBase> Options { get; } = new();

    private DataGrid OptionsDataGrid { get; set; } // Add this property to define OptionsDataGrid.

    private void InitializeComponent()
    {
        // Ensure that OptionsDataGrid is properly initialized in the XAML file or here in code-behind.
        OptionsDataGrid = new DataGrid(); // Add this line to initialize OptionsDataGrid if it is not defined in XAML.
    }

    private void ChargerOptions()
    {
        // À remplacer par un appel à votre Controller si besoin
        var dt = ChargerOptionsDepuisLaBase(); // Méthode à implémenter dans Controller

        Options.Clear();
        foreach (DataRow row in dt.Rows)
            if (Convert.ToInt32(row["Statut"]) > 0)
                Options.Add(new OptionDeBase
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nom = row["Nom"]?.ToString() ?? "",
                    Description = row["Description"]?.ToString() ?? "",
                    Prix = Convert.ToSingle(row["Prix"]),
                    Horaire = row["Horaire"]?.ToString() ?? "",
                    NbreSeance = Convert.ToInt32(row["NbreSeance"]),
                    DateDebutDisponible = row["DateDebutDisponible"] as DateTime?,
                    DateFinDisponible = row["DateFinDisponible"] as DateTime?,
                    Statut = Convert.ToInt32(row["Statut"])
                });
    }

    // Sauvegarde automatique lors de l'édition ou de l'ajout
    private void OptionsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            var option = e.Row.Item as OptionDeBase;
            if (option != null)
            {
                if (option.Id == 0)

                    // Nouvelle option
                    AjouterOptionDansLaBase(option);
                else

                    // Modification
                    ModifierOptionDansLaBase(option);

                // Recharge la liste pour afficher les modifications
                ChargerOptions();
            }
        }
    }

    // À implémenter dans Controller
    private DataTable ChargerOptionsDepuisLaBase()
    {
        return new DataTable();
    }

    private void AjouterOptionDansLaBase(OptionDeBase option)
    {
    }

    private void ModifierOptionDansLaBase(OptionDeBase option)
    {
    }
}

// Modèle de données
public class OptionDeBase
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public string Description { get; set; } = "";
    public float Prix { get; set; }
    public string Horaire { get; set; } = "";
    public int NbreSeance { get; set; }
    public DateTime? DateDebutDisponible { get; set; }
    public DateTime? DateFinDisponible { get; set; }
    public int Statut { get; set; }
}