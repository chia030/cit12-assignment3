using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Integreret service der kombinerer RequestValidator, UrlParser og CategoryService
/// til at håndtere CRUD logik på kategorier, samt echo.
/// </summary>
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
        var validation = _requestValidator.ValidateRequest(request);
        if (validation.Status != "1 Ok") return validation;

        if (!_urlParser.ParseUrl(request.Path!))
            return _responseService.CreateErrorResponse("Invalid URL format");

        return ExecuteCrud(request);
    }

    private Response ExecuteCrud(Request request)
    {
        var method = request.Method!.ToLower();
        var path = _urlParser.Path;

        if (method == "echo")
            return _responseService.CreateSuccessResponse(request.Body?.ToString() ?? "");

        if (path == "/api/categories")
            return HandleCategories(request);

        return _responseService.CreateNotFoundResponse();
    }

    private Response HandleCategories(Request request)
    {
        switch (request.Method!.ToLower())
        {
            case "read":   return HandleRead();
            case "create": return HandleCreate(request);
            case "update": return HandleUpdate(request);
            case "delete": return HandleDelete();
            default:        return _responseService.CreateErrorResponse("Invalid method for categories endpoint");
        }
    }

    private Response HandleRead()
    {
        if (_urlParser.HasId)
        {
            var category = _categoryService.GetCategory(_urlParser.Id);
            if (category == null) return _responseService.CreateNotFoundResponse();
            var data = new { cid = category.Cid, name = category.Name };
            return _responseService.CreateSuccessResponse(data);
        }

        var categories = _categoryService.GetCategories();
        var list = categories.Select(c => new { cid = c.Cid, name = c.Name });
        return _responseService.CreateSuccessResponse(list);
    }

    private Response HandleCreate(Request request)
    {
        if (_urlParser.HasId)
            return _responseService.CreateErrorResponse("CREATE operation cannot have ID in URL");

        try
        {
            var body = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Body!.ToString()!);
            if (body == null || !body.ContainsKey("name"))
                return _responseService.CreateErrorResponse("Missing 'name' field in request body");

            var name = body["name"].ToString();
            if (string.IsNullOrEmpty(name))
                return _responseService.CreateErrorResponse("Name cannot be empty");

            var nextId = _categoryService.GetCategories().Any() ? _categoryService.GetCategories().Max(c => c.Cid) + 1 : 1;
            var ok = _categoryService.CreateCategory(nextId, name);
            if (!ok) return _responseService.CreateErrorResponse("Failed to create category");

            var created = _categoryService.GetCategory(nextId)!;
            return _responseService.CreateSuccessResponse(new { cid = created.Cid, name = created.Name });
        }
        catch
        {
            return _responseService.CreateErrorResponse("Invalid JSON in request body");
        }
    }

    private Response HandleUpdate(Request request)
    {
        if (!_urlParser.HasId)
            return _responseService.CreateErrorResponse("UPDATE operation requires ID in URL");

        try
        {
            var body = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Body!.ToString()!);
            if (body == null || !body.ContainsKey("name"))
                return _responseService.CreateErrorResponse("Missing 'name' field in request body");

            var name = body["name"].ToString();
            if (string.IsNullOrEmpty(name))
                return _responseService.CreateErrorResponse("Name cannot be empty");

            var ok = _categoryService.UpdateCategory(_urlParser.Id, name);
            return ok ? _responseService.CreateUpdatedResponse() : _responseService.CreateNotFoundResponse();
        }
        catch
        {
            return _responseService.CreateErrorResponse("Invalid JSON in request body");
        }
    }

    private Response HandleDelete()
    {
        if (!_urlParser.HasId)
            return _responseService.CreateErrorResponse("DELETE operation requires ID in URL");

        var ok = _categoryService.DeleteCategory(_urlParser.Id);
        return ok ? _responseService.CreateSuccessResponse("") : _responseService.CreateNotFoundResponse();
    }
}


