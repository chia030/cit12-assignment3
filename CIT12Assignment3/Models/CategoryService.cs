using System;
using System.Collections.Generic;
using System.Linq;


    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }


   public class CategoryService
   {
       private readonly Dictionary<int, Category> _categories = new()
   {
        { 1, new Category { Id = 1, Name = "Beverages" } },
        { 2, new Category { Id = 2, Name = "Condiments" } },
        { 3, new Category { Id = 3, Name = "Confections" } }
   };


        public List<Category> GetCategories() => _categories.Values.ToList();


        public Category? GetCategory(int cid) =>
            _categories.TryGetValue(cid, out var category) ? category : null;


       public bool UpdateCategory(int id, string newName)
       {
           if (!_categories.ContainsKey(id)) return false;
            _categories[id].Name = newName;
           return true;
       }


       public bool DeleteCategory(int id) => _categories.Remove(id);


        public bool CreateCategory(int id, string name)
       {
           if (_categories.ContainsKey(id)) return false;
            _categories[id] = new Category { Id = id, Name = name };
           return true;
       }
   }
