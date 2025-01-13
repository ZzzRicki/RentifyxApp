using Rentifyx.DAL.Interfaces;

namespace Rentifyx.DAL.Core
{
    public interface IUnitOfWork
    {
        IRentifyxUserRepository RentifyxUserRepository { get; }
        IVehicleRepository VehicleRepository { get; }

        Task Commit();
    }
}
