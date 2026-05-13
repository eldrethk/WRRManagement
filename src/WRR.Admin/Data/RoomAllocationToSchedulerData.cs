using WRRManagement.Core.RoomTypes;
using Smart.Blazor;

namespace WRR.Admin.Data
{
    public class RoomAllocationToSchedulerData
    {
        private readonly IRoomAllocation _roomAllocationRep;

        public RoomAllocationToSchedulerData(IRoomAllocation roomAllocationRep)
        {
            _roomAllocationRep = roomAllocationRep;
        }

        public List<SchedulerDataSource> GetSchedulerDataSources(int roomid)
        {
            List<SchedulerDataSource> schedulerDataSources = new List<SchedulerDataSource>();
            List<RoomAllocation> roomAllocations = _roomAllocationRep.GetAllForRoom(roomid);
            return schedulerDataSources;
        }
    }
}
