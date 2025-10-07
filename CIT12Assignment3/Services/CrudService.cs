using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


public class CrudService
{
   private readonly RequestValidator _requestValidator;
   private readonly UrlParser _urlParser;
   private static readonly CategoryService _categoryService = new();
   private readonly ResponseService _responseService;


   public CrudService()
   {
       _requestValidator = new RequestValidator();
       _urlParser = new UrlParser();
       // static service shared across requests
       _responseService = new ResponseService();
   }


   public Response HandleRequest(Request request)
   {
       // Step 1: General request validation first so we return detailed reasons
       var validationResponse = _requestValidator.ValidateRequest(request);
       if (validationResponse.Status != "1 Ok")
       {
           return validationResponse;
       }

      var method = request.Method!.ToLower();

      // Echo ignores path completely — handle before any URL parsing
      if (method == "echo")
       {
           string bodyStr = request.Body switch
           {
               string s => s,
               System.Text.Json.JsonElement je when je.ValueKind == System.Text.Json.JsonValueKind.String => je.GetString() ?? "",
               _ => request.Body?.ToString() ?? ""
           };
           return _responseService.CreateSuccessResponse(bodyStr);
       }

      // Step 2: Parse URL and apply path/id constraints for categories (non-echo)
      if (!_urlParser.ParseUrl(request.Path!))
      {
          return _responseService.CreateErrorResponse();
      }

       if (!request.Path!.StartsWith("/api/categories"))
           return _responseService.CreateNotFoundResponse();

       // invalid id string like /api/categories/xxx
       if (request.Path.StartsWith("/api/categories/") && !_urlParser.HasId)
           return _responseService.CreateErrorResponse();

       if (method == "create" && _urlParser.HasId)
           return _responseService.CreateErrorResponse();

       if ((method == "update" || method == "delete") && !_urlParser.HasId)
           return _responseService.CreateErrorResponse();

       // Step 3: Execute CRUD
       return ExecuteCrudOperation(request);
   }


   private Response ExecuteCrudOperation(Request request)
   {
       var method = request.Method!.ToLower();
       var path = _urlParser.Path;


       if (method == "echo")
       {
           return _responseService.CreateSuccessResponse(request.Body?.ToString() ?? "");
       }


       // Guard: malformed id under /api/categories/<non-int>
       if (request.Path != null && request.Path.StartsWith("/api/categories/") && !_urlParser.HasId)
       {
           return _responseService.CreateErrorResponse();
       }

       // Handle categories CRUD operations with Path == "/api/categories"
       if (path == "/api/categories")
       {
           return HandleCategoriesCrud(request);
       }


       // Unknown path
       return _responseService.CreateNotFoundResponse();
   }


   private Response HandleCategoriesCrud(Request request)
   {
       var method = request.Method!.ToLower();


       switch (method)
       {
           case "read":
               return HandleReadOperation();
          
           case "create":
               return HandleCreateOperation(request);
          
           case "update":
               return HandleUpdateOperation(request);
          
           case "delete":
               return HandleDeleteOperation();
          
           default:
               return _responseService.CreateErrorResponse("Invalid method for categories endpoint");
       }
   }


   private Response HandleReadOperation()
   {
      if (_urlParser.HasId)
       {
          // READ specific category by ID
          if (!int.TryParse(_urlParser.Id, out var id))
          {
              return _responseService.CreateErrorResponse("Invalid id in path");
          }
          var category = _categoryService.GetCategory(id);
           if (category == null)
           {
               return _responseService.CreateNotFoundResponse();
           }


          var categoryData = new { cid = category.Id, name = category.Name };
           return _responseService.CreateSuccessResponse(categoryData);
       }
       else
       {
           // READ all categories
           var categories = _categoryService.GetCategories();
          var categoriesData = categories.Select(c => new { cid = c.Id, name = c.Name });
           return _responseService.CreateSuccessResponse(categoriesData);
       }
   }


   private Response HandleCreateOperation(Request request)
   {
       // CREATE operation cannot have ID in URL
       if (_urlParser.HasId)
       {
           return _responseService.CreateErrorResponse("CREATE operation cannot have ID in URL");
       }


      try
      {
          var body = TryParseBodyToDictionary(request.Body);
           if (body == null || !body.ContainsKey("name"))
           {
               return _responseService.CreateErrorResponse("Missing 'name' field in request body");
           }


          var name = body["name"].ToString();
           if (string.IsNullOrEmpty(name))
           {
               return _responseService.CreateErrorResponse("Name cannot be empty");
           }


           // Find next available ID
          var nextId = _categoryService.GetCategories().Any()
              ? _categoryService.GetCategories().Max(c => c.Id) + 1
               : 1;
          
           var success = _categoryService.CreateCategory(nextId, name);
          
           if (success)
           {
               var newCategory = _categoryService.GetCategory(nextId);
              var categoryData = new { cid = newCategory!.Id, name = newCategory.Name };
               return _responseService.CreateSuccessResponse(categoryData);
           }
          
           return _responseService.CreateErrorResponse("Failed to create category");
       }
       catch
       {
           return _responseService.CreateErrorResponse("Invalid JSON in request body");
       }
   }


   // Handle UPDATE operation with Path == "/api/categories/{id}"
   private Response HandleUpdateOperation(Request request)
   {
       // UPDATE operation requires ID in URL
       if (!_urlParser.HasId)
       {
           return _responseService.CreateErrorResponse("UPDATE operation requires ID in URL");
       }


      try
      {
          var body = TryParseBodyToDictionary(request.Body);
           if (body == null || !body.ContainsKey("name"))
           {
               return _responseService.CreateErrorResponse("Missing 'name' field in request body");
           }


           var name = body["name"].ToString();
           if (string.IsNullOrEmpty(name))
           {
               return _responseService.CreateErrorResponse("Name cannot be empty");
           }


          if (!int.TryParse(_urlParser.Id, out var id))
          {
              return _responseService.CreateErrorResponse("Invalid id in path");
          }
          var success = _categoryService.UpdateCategory(id, name);
          return success ? _responseService.CreateUpdatedResponse() : _responseService.CreateNotFoundResponse();
       }
       catch
       {
           return _responseService.CreateErrorResponse("Invalid JSON in request body");
       }
   }


   // Handle DELETE operation with Path == "/api/categories/{id}"
   private Response HandleDeleteOperation()
   {
       // DELETE skal have ID i URL
       if (!_urlParser.HasId)
       {
           return _responseService.CreateErrorResponse("DELETE operation requires ID in URL");
       }


      if (!int.TryParse(_urlParser.Id, out var id))
      {
          return _responseService.CreateErrorResponse("Invalid id in path");
      }
      var success = _categoryService.DeleteCategory(id);
      return success ? _responseService.CreateResponse("1 Ok") : _responseService.CreateNotFoundResponse();
   }

   // Helpers
   private static Dictionary<string, object>? TryParseBodyToDictionary(object? bodyObj)
   {
       if (bodyObj == null) return null;
       try
       {
           if (bodyObj is string s)
           {
               return JsonSerializer.Deserialize<Dictionary<string, object>>(s);
           }
           if (bodyObj is System.Text.Json.JsonElement je)
           {
               if (je.ValueKind == System.Text.Json.JsonValueKind.String)
               {
                   var str = je.GetString();
                   return str == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(str);
               }
               if (je.ValueKind == System.Text.Json.JsonValueKind.Object)
               {
                   var json = je.GetRawText();
                   return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
               }
           }
       }
        catch { }
       return null;
   }
}
