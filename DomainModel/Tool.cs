namespace DomainModel
{
    public abstract class Tool
    {
        public int ToolId { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
