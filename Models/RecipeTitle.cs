namespace dotnet_primer;

public class RecipeTitle
{
    public int Id {get; set;}
    public string Title { get; set;}
    public string? Description { get; set;}
    public RecipeReview? Review { get; set;}   
    public string? ImageUrl {get; set;}

    public RecipeTitle(){
        this.Title = "My Recipe";
        
    }
}


