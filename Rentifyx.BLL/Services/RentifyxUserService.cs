using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Core;
using Rentifyx.BLL.Extensions;
using Rentifyx.BLL.Models;
using Rentifyx.DAL.Core;
using Rentifyx.DAL.Entities;

namespace Rentifyx.BLL.Services
{
    public class RentifyxUserService: IRentifyxUserService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly UserManager<RentifyxUser> _userManager;
        public readonly ILogger _logger;

        public RentifyxUserService(IUnitOfWork unitOfWork, UserManager<RentifyxUser> userManager, ILogger<RentifyxUserService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ServiceResult<List<RentifyxUserModel>>> GetAll()
        {
            ServiceResult<List<RentifyxUserModel>> result = new();

            try
            {
                _logger.LogInformation("Getting all users");

                var users = await _unitOfWork.RentifyxUserRepository.GetEntitiesAsync();

                result.Message = "Sucess";
                result.Data = users.Select(x => x.GetRentifyxUserModelFromRentifyxUser()).ToList();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error getting user";
                _logger.LogError($"{result.Message}", ex.ToString());
            }

            return result;
        }

    }
}
