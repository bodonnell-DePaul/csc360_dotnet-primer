namespace dotnet_primer;

public class RecipeIngredients
{
    public int Id {get; set;}
    public int RecipeId {get;set;}
    public string? RecipeTitle {get;set;}
    public string? Name {get;set;}
    public bool Prepared {get;set;}
}
