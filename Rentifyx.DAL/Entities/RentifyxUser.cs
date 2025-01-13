using Rentifyx.DAL.Core;

namespace Rentifyx.DAL.Entities
{
    public class RentifyxUser : BaseUserEntity 
    {
        public string Name { get; set; } = string.Empty;    
    }
}
