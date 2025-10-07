using System;
using System.Collections.Generic;
using System.Linq;
public class UrlParser
{
   public bool HasId { get; set; }
   public string Id { get; set; } = "";
   public string Path { get; set; } = "";


   public bool ParseUrl(string url)
   {
       if (string.IsNullOrWhiteSpace(url))
           return false;


       var segments = url.Trim('/').Split('/');


       if (segments.Length == 0)
           return false;


       if (int.TryParse(segments.Last(), out int parsedId))
       {
           HasId = true;
           Id = parsedId.ToString();
           Path = "/" + string.Join("/", segments.Take(segments.Length - 1));
       }
       else
       {
           HasId = false;
           Id = "";
           Path = "/" + string.Join("/", segments);
       }


       return true;
   }
}
