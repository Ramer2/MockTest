namespace MockTest.Models.models;

public class Currency
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Rate { get; set; }

    public Currency()
    {
    }

    public Currency(int id, string name, float rate)
    {
        Id = id;
        Name = name;
        Rate = rate;
    }
}