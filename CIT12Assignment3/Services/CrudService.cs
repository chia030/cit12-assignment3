using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


public class CrudService
{
   private readonly RequestValidator _requestValidator;
   private readonly UrlParser _urlParser;
   private readonly CategoryService _categoryService;
   private readonly ResponseService _responseService;


   public CrudService()
   {
       _requestValidator = new RequestValidator();
       _urlParser = new UrlParser();
       _categoryService = new CategoryService();
       _responseService = new ResponseService();
   }


   public Response HandleRequest(Request request)
   {
       // Step 1: Valider request with RequestValidator
       var validationResponse = _requestValidator.ValidateRequest(request);
       if (validationResponse.Status != "1 Ok")
       {
           return validationResponse;
       }


       // Step 2: Parse URL with UrlParser
       if (!_urlParser.ParseUrl(request.Path!))
       {
           return _responseService.CreateErrorResponse("Invalid URL format");
       }


       // Step 3: Execute CRUD operation based on method and path
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
           var category = _categoryService.GetCategory(_urlParser.Id);
           if (category == null)
           {
               return _responseService.CreateNotFoundResponse();
           }


           var categoryData = new { cid = category.Cid, name = category.Name };
           return _responseService.CreateSuccessResponse(categoryData);
       }
       else
       {
           // READ all categories
           var categories = _categoryService.GetCategories();
           var categoriesData = categories.Select(c => new { cid = c.Cid, name = c.Name });
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
           var body = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Body!.ToString()!);
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
               ? _categoryService.GetCategories().Max(c => c.Cid) + 1
               : 1;
          
           var success = _categoryService.CreateCategory(nextId, name);
          
           if (success)
           {
               var newCategory = _categoryService.GetCategory(nextId);
               var categoryData = new { cid = newCategory!.Cid, name = newCategory.Name };
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
           var body = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Body!.ToString()!);
           if (body == null || !body.ContainsKey("name"))
           {
               return _responseService.CreateErrorResponse("Missing 'name' field in request body");
           }


           var name = body["name"].ToString();
           if (string.IsNullOrEmpty(name))
           {
               return _responseService.CreateErrorResponse("Name cannot be empty");
           }


           var success = _categoryService.UpdateCategory(_urlParser.Id, name);
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


       var success = _categoryService.DeleteCategory(_urlParser.Id);
       return success ? _responseService.CreateSuccessResponse("") : _responseService.CreateNotFoundResponse();
   }
}
