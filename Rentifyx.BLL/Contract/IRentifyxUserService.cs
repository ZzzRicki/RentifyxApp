using Rentifyx.BLL.Core;
using Rentifyx.BLL.Models;

namespace Rentifyx.BLL.Contract
{
    public interface IRentifyxUserService
    {
        Task<ServiceResult<List<RentifyxUserModel>>> GetAll();
    }
}
