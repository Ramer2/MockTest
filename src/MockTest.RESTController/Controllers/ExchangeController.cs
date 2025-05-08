using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using MockTest.Application;

namespace MockTest.RESTController.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ExchangeController : ControllerBase
{
    private IExchangeService _exchangeService;

    public ExchangeController(IExchangeService exchangeService)
    {
        _exchangeService = exchangeService;
    }

    [HttpPost]
    [Route("/api/currency")]
    [Consumes("application/json")]
    public async Task<IResult> CreateExchange()
    {
        using var reader = new StreamReader(Request.Body);
        var rawJson = await reader.ReadToEndAsync();
        
        var json = JsonNode.Parse(rawJson);
        if (json == null)
            return Results.BadRequest("Invalid JSON format.");

        try
        {
            _exchangeService.AddExchange(json);
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
        return Results.Created();
    }

    [HttpGet]
    [Route("/api/search")]
    [Consumes("application/json")]
    public async Task<IResult> SearchExchange()
    {
        using var reader = new StreamReader(Request.Body);
        var rawJson = await reader.ReadToEndAsync();
        
        var json = JsonNode.Parse(rawJson);
        if (json == null)
            return Results.BadRequest("Invalid JSON format.");
        
        try
        {
            var type = json["type"].ToString();
            switch (type)
            {
                case "country":
                {
                    if (_exchangeService.SearchExchangeByCountry(json).Currencies.Length == 0)
                        return Results.NotFound();
                    else
                        return Results.Ok(_exchangeService.SearchExchangeByCountry(json));
                }
                case "currency":
                {
                    if (_exchangeService.SearchExchangeByCurrency(json).Length == 0)
                        return Results.NotFound();
                    else 
                        return Results.Ok(_exchangeService.SearchExchangeByCurrency(json));
                }
                default:
                {
                    return Results.BadRequest("Invalid type.");
                }
            }
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}