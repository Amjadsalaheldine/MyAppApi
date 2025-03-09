namespace MyAppApi.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Model> Models { get; set; } = new List<Model>();
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>(); // ✅ تمت إضافتها
    }
}
