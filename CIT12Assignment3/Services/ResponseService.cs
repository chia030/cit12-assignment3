using System;
using System.Text.Json;

/// <summary>
/// Service der håndterer serialisering af Response objekter til JSON
/// og opretter konsistente responses
/// </summary>
public class ResponseService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ResponseService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public string SerializeResponse(Response response)
    {
        try
        {
            return JsonSerializer.Serialize(response, _jsonOptions);
        }
        catch (JsonException)
        {
            var errorResponse = new Response { Status = "4 Bad Request", Body = "JSON serialization error" };
            return JsonSerializer.Serialize(errorResponse, _jsonOptions);
        }
    }

    public Response CreateResponse(string status, string body = "")
    {
        return new Response { Status = status, Body = body };
    }

    public Response CreateSuccessResponse(object data)
    {
        try
        {
            var jsonBody = JsonSerializer.Serialize(data, _jsonOptions);
            return new Response { Status = "1 Ok", Body = jsonBody };
        }
        catch (JsonException)
        {
            return CreateErrorResponse("JSON serialization error");
        }
    }

    public Response CreateSuccessResponse(string body)
    {
        return new Response { Status = "1 Ok", Body = body };
    }

    public Response CreateErrorResponse(string errorMessage)
    {
        return new Response { Status = "4 Bad Request", Body = errorMessage };
    }

    public Response CreateNotFoundResponse()
    {
        return new Response { Status = "5 Not found", Body = "" };
    }

    public Response CreateUpdatedResponse()
    {
        return new Response { Status = "3 Updated", Body = "" };
    }
}


