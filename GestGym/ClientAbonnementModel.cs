namespace GestGym.Models;

public class ClientAbonnementModel
{
    public int ClientAbonnement_id { get; set; }
    public int REF_Client_ID { get; set; }
    public int REF_PlanDeBase_ID { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int Statut { get; set; }
    public bool IsNew { get; set; }

    // Additional fields for plan information
    public string PlanNom { get; set; } = "";
    public int PlanDuree { get; set; }
    public float PlanPrix { get; set; }
}