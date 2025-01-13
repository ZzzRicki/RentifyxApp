using Rentifyx.DAL.Context;
using Rentifyx.DAL.Interfaces;
using Rentifyx.DAL.Repositories;

namespace Rentifyx.DAL.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RentifyxContext _context;
        public IRentifyxUserRepository RentifyxUserRepository { get; set; }
        public IVehicleRepository VehicleRepository { get; set; }

        public UnitOfWork(RentifyxContext applicationDbContext)
        {
            this._context = applicationDbContext;
            this.RentifyxUserRepository = new RentifyxUserRepository(_context);
            this.VehicleRepository = new VehicleRepository(_context);
        }

        public async Task Commit()
        {
            await this._context.SaveChangesAsync();
        }
    }
}
