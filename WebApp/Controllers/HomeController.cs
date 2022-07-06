using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApp.Data;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        List<GetData> receivedData = new List<GetData>();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection con = new SqlConnection();


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            con.ConnectionString = "Server=LOSTBOY\\SQLEXPRESS02;Database=AuthenticationDatabase;Trusted_Connection=True;MultipleActiveResultSets=true";
    }

        public IActionResult Index()
        {
            fetchData();
            return View(receivedData);
        }
        public async Task<IActionResult> DownloadFile(string filepath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", filepath);
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
            if (receivedData.Count > 0)
            {
                receivedData.Clear();
            }
            try
            {
                con.Open();
                com.Connection = con;
                com.CommandText = "SELECT * FROM [AuthenticationDatabase].[dbo].[File] WHERE UserId='" + userId + "'";
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    receivedData.Add(new GetData()
                    {
                        Id = (int)dr["Id"],
                        Name = dr["Name"].ToString(),
                        Extension = dr["Extension"].ToString(),
                        Size = (int)dr["Size"],
                        UserId = dr["UserId"].ToString(),
                        createdon = (DateTime)dr["createdon"]


                    });
                }

                con.Close();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //public IActionResult Privacy()
        //{
          //  return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}