namespace MyAppApi.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
    
        
        public ICollection<Car> Cars { get; set; }
    }
}