using System;
using System.Collections.Generic;

public class Request
{
    public string? Method { get; set; }
    public string? Path { get; set; }
    public long? Date { get; set; }
    public object? Body { get; set; }
}

public class Response
{
    public string Status { get; set; } = "1 Ok";
}

public class RequestValidator
{
    private static readonly HashSet<string> ValidMethods = new()
    {
        "create", "read", "update", "delete", "echo"
    };

    public Response ValidateRequest(Request request)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(request.Method)) errors.Add("missing method");
        if (string.IsNullOrEmpty(request.Path)) errors.Add("missing path");
        if (!request.Date.HasValue) errors.Add("missing date");

        if (request.Method != null && !ValidMethods.Contains(request.Method.ToLower()))
            errors.Add("illegal method");

        if (request.Method is "create" or "update" && request.Body is not Dictionary<string, object>)
            errors.Add("invalid body for create/update");

        if (request.Method == "echo" && request.Body is not string)
            errors.Add("invalid body for echo");

        return new Response
        {
            Status = errors.Count == 0 ? "1 Ok" : $"4 Bad Request: {string.Join(", ", errors)}"
        };
    }
}
