
using Wanas.Domain.Entities;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class ReportPhotoRepository : GenericRepository<ReportPhoto> ,IReportPhotoRepository
    {
        public ReportPhotoRepository(AppDBContext context) : base(context)
        {
        }
    }
}