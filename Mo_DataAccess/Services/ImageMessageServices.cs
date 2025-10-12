namespace Mo_DataAccess.Services;

public class ImageMessageServices:GenericRepository<ImageMessage>,IImageMessageServices
{
    public ImageMessageServices(AppDbContext context) : base(context)
    {
    }
}