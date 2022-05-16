using System;
using System.Xml;
using System.Xml.Schema;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VeryBigShoeCompany.Models;
using System.Xml.Serialization;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace VeryBigShoeCompany.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
      //  private const List<float> validSizes = new List<float> { (float)11.5 };

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            UploadFileModel model = new UploadFileModel();
            model.Orders = new BigShoeDataImport();
            model.Orders.Order = new BigShoeDataImportOrder[0];
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult Upload(UploadFileModel model)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("ValidationMessage");
            ModelState.Remove("Message");

            if (ModelState.IsValid)
            {
                model.IsResponse = true;


                string path = Path.Combine(Directory.GetCurrentDirectory(), "OrderFiles");

                //get file extension
                FileInfo fileInfo = new FileInfo(model.File.FileName);
                string fileName = model.FileName + fileInfo.Extension;

                string fileNameWithPath = Path.Combine(path, fileName);
                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    model.File.CopyTo(stream);
                   
                }
                XmlSerializer serializer = new XmlSerializer(typeof(BigShoeDataImport));
                using (Stream reader = new FileStream(fileNameWithPath, FileMode.Open))
                {
                    var unvalidatedOrders = (BigShoeDataImport)serializer.Deserialize(reader);
                    model.Orders = new BigShoeDataImport();
                    model.Orders.Order=(from order in unvalidatedOrders.Order
                                       where IsOrderValid(order)
                                       select order).ToArray();
                }

                model.IsSuccess = true;
                model.Message = "File upload successfully";
                model.ValidationMessage = "Order Validated succesfully";
            }
            return View("Index", model);
        }
        private bool IsOrderValid(BigShoeDataImportOrder order)
        {
            if (string.IsNullOrEmpty(order.CustomerName))
                return false;
            if (string.IsNullOrEmpty(order.CustomerEmail) || (!new EmailAddressAttribute().IsValid(order.CustomerEmail)))
                return false;
            if (GetEarliestAcceptableDate(DateTime.Now.Date)<= order.DateRequired)
                return false;
            if (order.Size * 2 - (int)(order.Size * 2) != 0 | (order.Size < 11.5 || order.Size > 15))
                return false;
            if (order.Quantity%1000!=0)
                return false;
            return true;
        }
        private DateTime GetEarliestAcceptableDate(DateTime startingDate)
        {
            if (startingDate.DayOfWeek == DayOfWeek.Sunday)
                return startingDate.AddDays(12);
            if (startingDate.DayOfWeek == DayOfWeek.Saturday)
                return startingDate.AddDays(13);
            return startingDate.AddDays(14);
        }

    }
}