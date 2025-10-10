using Mo_DataAccess.Services.Interface;

namespace Mo_DataAccess.Services;

public class MessageServices:GenericRepository<Message>,IMessageServices
{
    public MessageServices(AppDbContext context) : base(context)
    {
    }
}