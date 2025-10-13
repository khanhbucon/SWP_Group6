namespace Mo_DataAccess.Services;

public class ImageMessageServices:GenericRepository<ImageMessage>,IImageMessageServices
{
    public ImageMessageServices(SwpGroup6Context context) : base(context)
    {
    }
}