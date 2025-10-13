namespace Mo_DataAccess.Services;

public class TextMessageServices:GenericRepository<TextMessage>,ITextMessageServices
{
    public TextMessageServices(SwpGroup6Context context) : base(context)
    {
    }
}
