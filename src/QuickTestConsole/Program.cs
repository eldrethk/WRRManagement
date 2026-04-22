// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;
using WRRManagement.Infrastructure.Repositories;

namespace QuickTestConsole
{
    class Program
    {
        
        private const int HotelID = 2; //Test hotel to use always
        static async Task Main(string[] args)
        {
            //build Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("WRRDBConnection");

            //set up DI
            var serviceProvider = new ServiceCollection()
                .AddScoped<IDbConnectionFactory>(sp =>
                new SqlConnectionFactory(connectionString!))
                .AddScoped<IHotelRepository, HotelRepository>()
                .AddScoped<IHotelSystemRepository, HotelSystemRepository>()
                .AddScoped<IRoomTypeRepository, RoomTypeRepository>()
                .BuildServiceProvider();

            //get the repository for the DI container
            var hotelRepository = serviceProvider.GetRequiredService<IHotelRepository>();
            var systemRepository = serviceProvider.GetRequiredService<IHotelSystemRepository>();
            var roomRepository = serviceProvider.GetRequiredService<IRoomTypeRepository>();

            //run Test
           // await TestHotel(hotelRepository);
            await TestHotelSystem(systemRepository);
            await TestRoomType(roomRepository);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static async Task TestRoomType(IRoomTypeRepository roomTypeRepository)
        {
            var AllRooms = await roomTypeRepository.GetAllForHotelAsync(HotelID);
            Console.WriteLine(" -----  All Room Types ----");
            foreach(var room in AllRooms)
            {
                Console.WriteLine(room.Name);
            }
        }
        static async Task TestHotelSystem(IHotelSystemRepository hotelSystem)
        {
            var system = await hotelSystem.GetSystemAsync(HotelID);
            Console.WriteLine($"Does hotel tax on the weekend : {system.AddTaxToWeekendFee} | if so, how much? {system.TaxRate} %");
            int days = await hotelSystem.GetPriorDayBookingAsync(HotelID);
            Console.WriteLine($"Does the hotel have a prior day notice to book? {(days > 0 ? $"Yes - {days} days" : "no")}"); 
        }

        static async Task TestHotel(IHotelRepository hotelRepository)
        {
            Console.WriteLine("-----Hotel By ID-----");
            var hotel = await hotelRepository.GetByIdAsync(HotelID);
            Console.WriteLine($"created hotel with id : {hotel!.HotelID.ToString()} and name is {hotel.Name}");

            hotel.UpdateHotel("123 Test dr", "Myrtle Beach", "SC", "28595", "843-444-4444", "1-800-222-2222", "3:00pm", "11:00am");
            await hotelRepository.UpdateAsync(hotel);
            Console.WriteLine("hotel is update");
        }
 
    }
}



