using Rentifyx.BLL.Models;
using Rentifyx.DAL.Entities;

namespace Rentifyx.BLL.Extensions
{
    public static class RentifyxUserExtension
    {
        public static RentifyxUserModel GetRentifyxUserModelFromRentifyxUser(this RentifyxUser rentifyxUser) 
        {
            return new RentifyxUserModel()
            {
                Name = rentifyxUser.Name,
                Email = rentifyxUser.Email,
                PhoneNumber = rentifyxUser.PhoneNumber
            };
        }
    }
}
