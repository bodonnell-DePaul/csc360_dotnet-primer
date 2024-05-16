namespace dotnet_primer;

public class RecipeTitle
{
    public int Id {get; set;}
    public string Title { get; set;}
    public string? Description { get; set;}
    public RecipeReview? Review { get; set;}   
    public string? ImageUrl {get; set;}

    public RecipeTitle(int id = -1, string title = "My Recipe", string? description = null, string? imageUrl = null){
        this.Title = title;
        this.Description = description;
        this.Review = null;
        this.ImageUrl = imageUrl;

        if(id < 0){
            this.Id = Random.Shared.Next(1000, 9999);
        }


        
    }
}


