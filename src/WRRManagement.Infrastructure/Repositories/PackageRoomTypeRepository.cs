using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageRoomTypeRepository : DapperRepository, IPackageRoomTypeRepository
    {
        public PackageRoomTypeRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<RoomType>> GetRoomTypesForPackageAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<RoomType>("dbo.genSelPackageRooms", parameters);
        }

        public async Task SetRoomAssociationsAsync(int packageId, IEnumerable<int> roomTypeIds)
        {
            await ExecuteWithTransactionAsync(async (connection, transaction) =>
            {
                await connection.ExecuteAsync("dbo.genDropRoomsFromPackage",
                    new { PackageID = packageId },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                foreach (var roomTypeId in roomTypeIds)
                {
                    await connection.ExecuteAsync("dbo.genInsPackageRoom",
                        new { PackageID = packageId, RoomTypeID = roomTypeId },
                        transaction,
                        commandType: CommandType.StoredProcedure);
                }
            });
        }
    }
}
