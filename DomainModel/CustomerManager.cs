namespace DomainModel
{
    public class CustomerManager
    {
        public int ResourceId { get; set; }
        public int CustomerId { get; set; }
        public Resource Resource { get; set; }
        public Customer Customer { get; set; }
    }
}
