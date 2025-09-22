using Microsoft.AspNetCore.Mvc;
using Upload_Files_APP.Context;
using Upload_Files_APP.Models;
using Upload_Files_APP.ViewModels;

namespace Upload_Files_APP.Controllers
{
    public class FilesController : Controller
    {
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public FilesController(AppDbContext _context,IWebHostEnvironment _webHostEnvironment)
        {
            context = _context;
            webHostEnvironment = _webHostEnvironment;
        }
        public IActionResult Index()
        {
           var files=context.uploadFiles.ToList();
            return View(files);
        }
        public IActionResult Upload()
        {
            return View("Upload");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadFiles(UploadFileFeomViewModel model)
        {
            //&&x.ContentType=="pdf"
            //model.Files= model.Files.Where(x => x.Length > 0 && x.Length < 10000).ToList();
            if (ModelState.IsValid&&model.Files.Count>0)
            {
                List<UploadFile> uploadFiles = new List<UploadFile>();
                foreach (var item in model.Files)
                {
                    var fakFileName=Path.GetRandomFileName();
                    UploadFile uploadFile=new UploadFile()
                    { 
                        ContentType = item.ContentType, 
                        FileName = item.FileName,
                        StoredFileName = fakFileName
                    };
                    var path=Path.Combine(webHostEnvironment.WebRootPath, "Files",fakFileName);

                    FileStream fileStream=new(path, FileMode.Create);
                    item.CopyTo(fileStream);
                    uploadFiles.Add(uploadFile);
                }
                context.uploadFiles.AddRange(uploadFiles);
                context.SaveChanges();
                return RedirectToAction("Index");
            } 
            return View("Upload",model);
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var uploadfile = context.uploadFiles.SingleOrDefault(x => x.StoredFileName == fileName);
            if (uploadfile == null)
                return NotFound();

            var path = Path.Combine(webHostEnvironment.WebRootPath, "Files", fileName);

            MemoryStream memory = new MemoryStream();
            FileStream fileStream = new(path, FileMode.Open);
            fileStream.CopyTo(memory);
            memory.Position = 0;
            return File(memory,uploadfile.ContentType,uploadfile.FileName);  
        }

        [HttpGet]
        public IActionResult DeleteFile(string fileName)
        {
            // 1. نبحث في الداتا بيز عن الملف
            var uploadFile = context.uploadFiles.SingleOrDefault(x => x.StoredFileName == fileName);
            if (uploadFile == null)
                return NotFound();

            // 2. نحدد المسار على السيرفر
            var path = Path.Combine(webHostEnvironment.WebRootPath, "Files", fileName);

            // 3. لو الملف موجود على السيرفر نمسحه
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // 4. نمسح الـ record من الداتا بيز
            context.uploadFiles.Remove(uploadFile);
            context.SaveChanges();

            // 5. بعد المسح نرجع لصفحة الملفات أو Index
            return RedirectToAction("Index");
        }


    }
}
