using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {
        private readonly IFileUploader _fileUploader;
        private readonly IAdvertApiClient _advertApiClient;
        private readonly IMapper _mapper;
        public AdvertManagementController(IFileUploader fileUploader,IAdvertApiClient advertApiClient,IMapper mapper)
        {

            _advertApiClient = advertApiClient;
            _mapper = mapper;
        }

        public IActionResult Create(CreateAdvertViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model,IFormFile imageFile,IMapper mapper)
        {
            if (ModelState.IsValid)
            {
                var createAdvertModel = _mapper.Map<CreateAdvertModel>(model);
                var apiCallResponse = await _advertApiClient.Create(createAdvertModel);
                var id = apiCallResponse.Id;
                var fileName = "";
                if (imageFile !=null)
                {
                    fileName = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : id;
                    var filePath = $"{id}/{fileName}";
                    try
                    {
                        using (var readStream = imageFile.OpenReadStream())
                        {
                            var result = await _fileUploader.UploadFileAsync(filePath, readStream);
                            if (!result)
                                throw new Exception("Could not upload the image to file respository. Please see the logs");
                        }
                        var confirmModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            FilePath=filePath,
                            Status=AdvertApi.Models.AdvertStatus.Active
                        };
                        var isConfirmed= await _advertApiClient.Confirm(confirmModel);
                        if (!isConfirmed)
                        {
                            throw new Exception($"Cannot confirm advert of id={id}");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    catch (Exception e)
                    {
                        var confirmModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertApi.Models.AdvertStatus.Pending
                        };
                        await _advertApiClient.Confirm(confirmModel);
                        Console.WriteLine(e);
                    }
                                               
                }
            }
            return View(model);
        }
    }
}