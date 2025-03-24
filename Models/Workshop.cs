public class Workshop
{
    public int Id { get; set; }  
    public string Name { get; set; } = string.Empty; 
    public string Responsible { get; set; } = string.Empty; 


    public ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}