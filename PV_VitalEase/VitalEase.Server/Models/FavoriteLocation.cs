namespace VitalEase.Server.Models
{
    public class FavoriteLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }

        public void DeleteLocation()
        {
            // Implementation here
        }

        public (double, double) GetCoordinates()
        {
            return (Latitude, Longitude);
        }

        public double CalculateDistance(double toLatitude, double toLongitude)
        {
            // Implementation here
            return 0.0;
        }
    }
}
