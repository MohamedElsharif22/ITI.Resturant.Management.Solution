using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Application.ExternalServices.Contracts
{
    public interface IFileUploadService
    {
        const string MenuImgsFolder = "menu";
        Task<string> UploadImageAsync(IFormFile file, string folder);
        bool DeleteImage(string filePath);
        bool IsValidImageFile(IFormFile file);
    }
}
