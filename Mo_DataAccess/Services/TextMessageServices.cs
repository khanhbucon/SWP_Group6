namespace Mo_DataAccess.Services;

public class TextMessageServices:GenericRepository<TextMessage>,ITextMessageServices
{
    public TextMessageServices(AppDbContext context) : base(context)
    {
    }
}