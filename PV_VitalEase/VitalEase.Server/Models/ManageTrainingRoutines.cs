namespace VitalEase.Server.Models
{
    public class ManageTrainingRoutines
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Certifique-se de que é string ou enum
        public bool IsCustom { get; set; } // Boolean para saber se é personalizada
        public string Needs { get; set; } // Pode ser uma string para necessidades específicas
        public ICollection<Exercise> Exercises { get; set; }
    }
}
