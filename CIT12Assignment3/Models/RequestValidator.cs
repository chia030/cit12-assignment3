using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;

// Chiara: moved Request and Response models to their own files
public class RequestValidator
{
    private static readonly HashSet<string> ValidMethods = new()
    {
        "create", "read", "update", "delete", "echo"
    };

    // private enum MethodType // to use instead of the HashSet
    // {

    // }

    /*
    JSON request body should be (with "create" or "update" method):

    {
    "body": { 
        "id": 1,
        "name": "xxx"
    }

    if the method is "echo", it is:
    "body": "Hello!"

    */

    private (bool isValid, string errorType) ValidateBody(string? body)
    {
        // null
        if (string.IsNullOrWhiteSpace(body))
            return (false, "missing body");

        // use CreateOrUpdateBody model to check the body
        try
        {
            var createOrUpdateBody = JsonSerializer.Deserialize<CreateOrUpdateBody>(body);
            if (createOrUpdateBody == null)
                return (false, "illegal body");

            // semantic checks
            else if (createOrUpdateBody.id <= 0)
                return (false, "illegal body");

            else if (string.IsNullOrWhiteSpace(createOrUpdateBody.name))
                return (false, "illegal body");

            return (true, "none");
        }
        catch
        {
            // parsing failed
            return (false, "parsing failed");
        }

    }

    public Response ValidateRequest(Request request)
    {
        var errors = new List<string>();
        // method check
        if (request.Method == "none")
            errors.Add("missing method");

        if (!ValidMethods.Contains(request.Method.ToLower()))
            errors.Add("illegal method");

        // path check
        if (request.Path == "none")
            errors.Add("missing path");

        // date check
        if (request.Date == 0)
            errors.Add("missing date");

        // body check
        if (request.Method is "create" or "update" or "echo" & !string.IsNullOrWhiteSpace(request.Body))
        {
            // case: "CREATE" or "UPDATE"
            if (request.Method is "create" or "update")
            {
                (bool isValid, string errorType) isValidBody = ValidateBody(request.Body);
                if (!isValidBody.isValid) errors.Add(isValidBody.errorType);
            }
            // case: "ECHO"
            // nothing else needed
        }
        else errors.Add("missing body");

        return new Response
        {
            // split status and body || TODO: change back to one Status
            Status = !errors.Any() ? "1 Ok" : "4 Bad Request",
            Body = $"{string.Join(", ", errors)}"

        };
    }
}
