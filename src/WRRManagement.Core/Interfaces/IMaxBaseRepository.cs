using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IMaxBaseRepository
    {
        Task<int> AddAsync(MaxBase maxBase);
        Task<MaxBase> GetByRoomID(int roomId);
    }
}
