using System.Text.Json.Nodes;
using MockTest.Models.dto;
using MockTest.Models.models;

namespace MockTest.Application;

public interface IExchangeService
{
    public bool AddExchange(JsonNode? json);
    
    public CountryDTO SearchExchangeByCountry(JsonNode? json);
    
    public Country[] SearchExchangeByCurrency(JsonNode? json);
}