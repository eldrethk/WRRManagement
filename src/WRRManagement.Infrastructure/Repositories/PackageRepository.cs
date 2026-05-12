using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageRepository : DapperRepository, IPackageRepository
    {
        public PackageRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<Package?> GetByIdAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryFirstOrDefaultAsync<Package>("dbo.genSelPackageByID", parameters);
        }

        public async Task<IEnumerable<Package>> GetAllForHotelAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<Package>("dbo.genSelPackagesByHotelID", parameters);
        }

        public async Task<IEnumerable<Package>> GetByRoomAsync(int roomTypeId, DateTime start, DateTime end)
        {
            var parameters = new { RoomTypeID = roomTypeId, Start = start, End = end };
            return await QueryAsync<Package>("dbo.genSelPackagesAssociatedWithRoom", parameters);
        }

        public async Task<IEnumerable<Package>> GetForSpecialPageAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<Package>("dbo.genSelPackageForSpecialPage", parameters);
        }

        public async Task<IEnumerable<Package>> GetWithRackRatesAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<Package>("dbo.genSelPackagesWithRates", parameters);
        }

        public async Task<IEnumerable<RoomType>> GetRoomTypesAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<RoomType>("dbo.genSelPackageRooms", parameters);
        }

        public async Task<int> AddAsync(Package package)
        {
            var parameters = new
            {
                package.HotelID,
                package.Name,
                package.Description,
                package.ShortDescription,
                package.Amenity,
                package.ArrMon,
                package.ArrTues,
                package.ArrWed,
                package.ArrThurs,
                package.ArrFri,
                package.ArrSat,
                package.ArrSun,
                package.MinDays,
                package.MaxDays,
                package.WeekendSurcharge,
                package.ResortFees,
                package.ValidFrom,
                package.ValidTo,
                package.EndDisplayDate,
                package.Visible,
                package.NightsFree,
                package.NumberOfNights,
                package.PercentOff,
                package.PercentageOff,
                package.PricePoint,
                package.Deposit,
                package.ExtraPersonFee,
                package.PackageAllocation,
                package.DeletedPackage,
                package.SmImage,
                package.Order,
                package.SpecialPage
            };
            return await ExecuteScalarIntAsync("dbo.genInsPackage", parameters);
        }

        public async Task UpdateAsync(Package package)
        {
            var parameters = new
            {
                package.PackageID,
                package.Name,
                package.Description,
                package.ShortDescription,
                package.Amenity,
                package.ArrMon,
                package.ArrTues,
                package.ArrWed,
                package.ArrThurs,
                package.ArrFri,
                package.ArrSat,
                package.ArrSun,
                package.MinDays,
                package.MaxDays,
                package.WeekendSurcharge,
                package.ResortFees,
                package.ValidFrom,
                package.ValidTo,
                package.EndDisplayDate,
                package.Visible,
                package.NightsFree,
                package.NumberOfNights,
                package.PercentOff,
                package.PercentageOff,
                package.PricePoint,
                package.Deposit,
                package.ExtraPersonFee,
                package.PackageAllocation,
                package.DeletedPackage,
                package.Order,
                package.SpecialPage
            };
            await ExecuteAsync("dbo.genUpdPackage", parameters);
        }

        public async Task UpdateImageAsync(int packageId, string imagePath)
        {
            var parameters = new { PackageID = packageId, SmImage = imagePath };
            await ExecuteAsync("dbo.genUpdPackageImage", parameters);
        }

        public async Task SetInvisibleAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            await ExecuteAsync("dbo.genInvisiblePackage", parameters);
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
