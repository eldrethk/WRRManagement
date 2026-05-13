using Microsoft.AspNetCore.Mvc;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{

    public class RoomImagesController : Controller
    {
        private readonly IRoomImageRepository roomImageRep;
        private readonly IRoomTypeRepository roomTypeRep;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public RoomImagesController(IRoomImageRepository roomImageRep, IRoomTypeRepository roomTypeRep, IWebHostEnvironment webHostEnvironment)
        {
            this.roomImageRep = roomImageRep;
            this.roomTypeRep = roomTypeRep;
            _hostingEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int id)
        {
            var images = (await roomImageRep.GetAllImageForRoomAsync(id)).ToList();
            if (images == null)
                return NotFound();
            var roomType = await roomTypeRep.GetByIdAsync(id);
            ViewData["RoomTypeName"] = roomType.Name;
            ViewData["RoomTypeID"] = roomType.RoomTypeID;

            if (roomType == null)
                return NotFound();

            return View(images);

        }

        public async Task<IActionResult> SetMainImage(int id, int roomid)
        {
            await roomImageRep.SetAsMainImageAsync(id, roomid);
            return RedirectToAction("Index", new { id = roomid });
        }

        [HttpGet]
        public IActionResult Create(int roomid)
        {
            UploadImageViewModel model = new UploadImageViewModel { Id = roomid };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UploadImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Image != null)
                {
                    var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                    if (!supportedTypes.Contains(model.Image.ContentType))
                    {
                        ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                        return View(model);
                    }

                    int roomId = model.Id;
                    var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/room-images");
                    var fileName = Guid.NewGuid().ToString() + "_" + roomId.ToString() + "_" + model.Image.FileName;
                    var path = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }
                    var roomImage = new RoomImage()
                    {
                        FileName = fileName,
                        RoomTypeID = roomId,
                        ContentLength = model.Image.Length,
                        ContentType = model.Image.ContentType,
                        IsVisible = true

                    };
                    await roomImageRep.AddAsync(roomImage);

                    return RedirectToAction("Index", new { id = roomId });
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id, int roomid)
        {
            await roomImageRep.SetInvisibleAsync(id);
            return RedirectToAction("Index", new { id = roomid });
        }
    }
}
