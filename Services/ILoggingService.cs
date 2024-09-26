using System.Threading.Tasks;
using LUTE_Server.Models;

namespace LUTE_Server.Services
{
    public interface ILoggingService
    {
        Task LogAsync(UserLog log);  // Single log
        Task LogBulkAsync(UserLog[] logs);  //Bulk logging
    }
}
