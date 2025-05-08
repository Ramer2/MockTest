namespace MockTest.Models.models;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Country()
    {
    }

    public Country(int id, string name)
    {
        Id = id;
        Name = name;
    }

    protected bool Equals(Country other)
    {
        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Country)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}