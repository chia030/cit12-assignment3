using System;
using System.Text.Json;


// ResponseService handles serializing Response objects to JSON and sending them back to the client
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


   // Serialize Response object to JSON string
   public string SerializeResponse(Response response)
   {
       try
       {
           return JsonSerializer.Serialize(response, _jsonOptions);
       }
       catch (JsonException ex)
       {
           // If serialization fails, return an error response
           var errorResponse = new Response
           {
               Status = "4 Bad Request",
               Body = "JSON serialization error"
           };
           return JsonSerializer.Serialize(errorResponse, _jsonOptions);
       }
   }


   // Create Response object with status and body
   public Response CreateResponse(string status, string body = "")
   {
       return new Response
       {
           Status = status,
           Body = body
       };
   }


   // Create success Response with JSON body
   public Response CreateSuccessResponse(object data)
   {
       try
       {
           var jsonBody = JsonSerializer.Serialize(data, _jsonOptions);
           return new Response
           {
               Status = "1 Ok",
               Body = jsonBody
           };
       }
       catch (JsonException)
       {
           return CreateErrorResponse("JSON serialization error");
       }
   }


   // Create success Response with string body
   public Response CreateSuccessResponse(string body)
   {
       return new Response
       {
           Status = "1 Ok",
           Body = body
       };
   }


   // Create error Response with error message
   public Response CreateErrorResponse(string errorMessage)
   {
       return new Response
       {
           Status = "4 Bad Request",
           Body = errorMessage
       };
   }


   // Create not found Response
   public Response CreateNotFoundResponse()
   {
       return new Response
       {
           Status = "5 Not found",
           Body = ""
       };
   }


   // Create updated Response
   public Response CreateUpdatedResponse()
   {
       return new Response
       {
           Status = "3 Updated",
           Body = ""
       };
   }




   public bool ValidateResponse(Response response)
   {
       if (response == null) return false;
       if (string.IsNullOrEmpty(response.Status)) return false;


       // Validate that status follows the expected format
       var validStatuses = new[] { "1 Ok", "3 Updated", "4 Bad Request", "5 Not found" };
       return Array.Exists(validStatuses, status => response.Status.StartsWith(status));
   }
}
