namespace MyAppApi.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>(); 
    }
}
