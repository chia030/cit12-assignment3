using System;
using System.Collections.Generic;


public class Request
{
   public string? Method { get; set; }
   public string? Path { get; set; }
   public string? Date { get; set; }
   public object? Body { get; set; }
}


public class Response
{
   public string Status { get; set; } = "1 Ok";
   public string Body { get; set; } = "";
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
        // Path is required for CRUD methods but not for echo
        if (request.Method is null || request.Method.ToLower() is not "echo")
        {
            if (string.IsNullOrEmpty(request.Path)) errors.Add("missing path");
        }
       if (string.IsNullOrEmpty(request.Date)) errors.Add("missing date");


       if (request.Method != null && !ValidMethods.Contains(request.Method.ToLower()))
           errors.Add("illegal method");


        // Validate body based on method
        if (request.Method is "create" or "update")
        {
            if (request.Body == null)
            {
                errors.Add("missing body");
            }
            else if (request.Body is string s)
            {
                // Accept JSON string bodies for create/update
                try
                {
                    _ = System.Text.Json.JsonDocument.Parse(s);
                }
                catch
                {
                    errors.Add("illegal body");
                }
            }
            else if (request.Body is System.Text.Json.JsonElement je)
            {
                try
                {
                    if (je.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var str = je.GetString();
                        _ = System.Text.Json.JsonDocument.Parse(str ?? "");
                    }
                    else if (je.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        // object is fine
                    }
                    else
                    {
                        errors.Add("illegal body");
                    }
                }
                catch
                {
                    errors.Add("illegal body");
                }
            }
            else if (request.Body is not Dictionary<string, object>)
            {
                errors.Add("illegal body");
            }
        }


        if (request.Method == "echo")
       {
           if (request.Body == null)
               errors.Add("missing body");
            else if (request.Body is string)
            {
                // ok
            }
            else if (request.Body is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                // ok
            }
            else
            {
                errors.Add("illegal body");
            }
       }


       if (!string.IsNullOrEmpty(request.Date))
       {
           if (!long.TryParse(request.Date, out var unixSeconds))
           {
               errors.Add("illegal date");
           }
           else
           {
               try
               {
                   _ = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
               }
               catch (ArgumentOutOfRangeException)
               {
                   errors.Add("illegal date");
               }
           }
       }


       return new Response
       {
           Status = errors.Count == 0 ? "1 Ok" : $"4 Bad Request: {string.Join(", ", errors)}"
       };
   }
}
