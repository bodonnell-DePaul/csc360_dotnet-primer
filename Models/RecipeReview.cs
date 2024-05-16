namespace dotnet_primer;
public class RecipeReview
{
    public int Id {get;set;}
    public int RecipeId {get;set;}
    public float Rating { get; set;}
    public int Reviews { get; set;}

    public RecipeReview(int recipeId)
    {  
        this.RecipeId = recipeId;

    }
}