using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IAdultBaseRepository
    {
        Task<int> AddAsync(AdultBase adult);
        Task<AdultBase> GetByRoomIDAsync(int roomID);

    }
}
