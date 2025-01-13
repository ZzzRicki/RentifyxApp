namespace Rentifyx.DAL.Entities.Vehicle
{
    public class VehicleCharacteristic
    {
        public int Id { get; set; }
        public int VehicleId { get; set; } 
        public string? Name { get; set; } 

        public Vehicle? Vehicle { get; set; }
    }
}
