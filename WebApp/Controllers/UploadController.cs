using System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Data;
using WebApp.Models;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace WebApp.Controllers
{
    public class UploadController : Controller
    {
        List<GetData> receivedData = new List<GetData>();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection con = new SqlConnection();
        private readonly FilesDbContext _context;
        private readonly ILogger<UploadController> _logger;

        public UploadController(FilesDbContext context, ILogger<UploadController> logger)
        {
            _context = context;
            _logger = logger;
            con.ConnectionString = "Server=LOSTBOY\\SQLEXPRESS02;Database=AuthenticationDatabase;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DownloadFile(string filepath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),"UploadedFiles", filepath);
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);   
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var filename = Path.GetFileName(path);
            return File(memory, contentType, filename);
        }

        private void fetchData()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(receivedData.Count > 0)
            {
                receivedData.Clear();
            }
            try
            {
                con.Open();
                com.Connection = con;
                com.CommandText = "SELECT * FROM [AuthenticationDatabase].[dbo].[File] WHERE UserId='"+userId+"'";
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    receivedData.Add(new GetData()
                    {
                        Id = (int)dr["Id"],
                        Name = dr["Name"].ToString(),
                        Extension = dr["Extension"].ToString(),
                        Size = (int) dr["Size"],
                        UserId = dr["UserId"].ToString(),
                        createdon = (DateTime) dr["createdon"]


                    }) ;
                }
                
                con.Close();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet]
        public ActionResult UploadFile()
        {
            fetchData();
            return View(receivedData);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UploadFile(FormModel model)
        {

            List<IFormFile> files = model.files;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var filePaths = new List<string>();
            foreach (IFormFile formFile in files)
            {
                if (formFile.Length > 0)
                {
                    string filePath = Path.Combine(Path.GetFullPath("UploadedFiles"), userId + Path.GetFileName(formFile.FileName));
                    filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    _logger.LogTrace(message: $"Logging User Identity: {User.Identity?.ToString()}");
                    var f = new WebApp.Models.File()
                    {
                        Name = formFile.FileName,
                        Extension = formFile.ContentType,
                        Size = (int)formFile.Length,
                        createdon = DateTime.Now,
                        UserId = userId,
                    };
                    //_context.Find();
                    _context.Add(f);
                    _context.SaveChanges();
                }
            }
            return Redirect("/");
        }
    }
}
