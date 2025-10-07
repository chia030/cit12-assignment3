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
                return (false, "invalid body");

            // semantic checks
            else if (createOrUpdateBody.id <= 0)
                return (false, "invalid body");

            else if (string.IsNullOrWhiteSpace(createOrUpdateBody.name))
                return (false, "invalid body");

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
        if (request.Method is "create" or "update" or "echo" & request.Body is not null)
        {
            // case: "CREATE" or "UPDATE"
            if (request.Method is "create" or "update")
            {
                (bool isValid, string errorType) isValidBody = ValidateBody(request.Body);
                if (!isValidBody.isValid) errors.Add(isValidBody.errorType);
            }
            // case: "ECHO"
            if (request.Method is "echo")
            {
                // if
            }
        }
        else errors.Add("invalid body");
        // incomplete [...]





    }

    // public Response ValidateRequest1(Request request)
    // {
    //     var errors = new List<string>();

    //     if (string.IsNullOrEmpty(request.Method)) errors.Add("missing method");
    //     if (string.IsNullOrEmpty(request.Path)) errors.Add("missing path");
    //     if (!request.Date.HasValue) errors.Add("missing date");

    //     if (request.Method != null && !ValidMethods.Contains(request.Method.ToLower()))
    //         errors.Add("illegal method");

    //     if (request.Method is "create" or "update" && request.Body is not Dictionary<string, object>)
    //         errors.Add("invalid body for create/update");

    //     if (request.Method == "echo" && request.Body is not string)
    //         errors.Add("invalid body for echo");

    //     // should return a Response obj?
    //     return new Response
    //     {
    //         // split status and body
    //         Status = errors.Count == 0 ? "1 Ok" : $"4 Bad Request: {string.Join(", ", errors)}"

    //     };
    // }
}
