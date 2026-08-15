using System;

namespace Api.CategoriesModule
{
    public class Category : IEquatable<Category>
    {
        public int    CategoryId   { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Category() { }

        public Category(int categoryId, string categoryName)
        {
            CategoryId   = categoryId;
            CategoryName = categoryName;
        }

        public bool Equals(Category? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (CategoryId > 0 || other.CategoryId > 0) return CategoryId == other.CategoryId;
            return string.Equals(CategoryName, other.CategoryName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Category);

        public override int GetHashCode()
        {
            if (CategoryId > 0) return CategoryId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(CategoryName);
        }

        public override string ToString() =>
            $"Category[CategoryId={CategoryId}, CategoryName={CategoryName}]";
    }
}
