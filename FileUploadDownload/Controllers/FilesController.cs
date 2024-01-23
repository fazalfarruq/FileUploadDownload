using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using nClam;

namespace FileUploadDownload.Controllers
{
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IConfiguration _configuration;

        public FilesController(ILogger<FilesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("UploadFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            var result = await Scan(file);
            if(result)
                return BadRequest("Virus Found");
            return Ok("Clean File");
        }

        private async Task<string> WriteFile(IFormFile file)
        {
            // Get file name
            string fileName = "";

            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = DateTime.Now.Ticks + extension; //Create a new Name for the file due to security reasons.

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\Files", fileName);

                await using var stream = new FileStream(exactpath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception e)
            {
                //TODO: handle exception
            }

            return fileName;
        }


        private async Task<bool> Scan(IFormFile file)
        {
            var result = false;
            if (file == null || file.Length == 0)
                Debug.WriteLine("Error");
            ;

            var ms = new MemoryStream();
            file.OpenReadStream().CopyTo(ms);
            byte[] fileBytes = ms.ToArray();

            try
            {
                this._logger.LogInformation("ClamAV scan begin for file {0}", file.FileName);
                var clam = new ClamClient(this._configuration["ClamAVServer:URL"],
                    Convert.ToInt32(this._configuration["ClamAVServer:Port"]));
                var scanResult = await clam.SendAndScanFileAsync(fileBytes);
                switch (scanResult.Result)
                {
                    case ClamScanResults.Clean:
                        this._logger.LogInformation("The file is clean! ScanResult:{1}", scanResult.RawResult);
                       break;
                    case ClamScanResults.VirusDetected:
                        this._logger.LogError("Virus Found! Virus name: {1}", scanResult.InfectedFiles.FirstOrDefault().VirusName);
                        result = true;
                        break;
                    case ClamScanResults.Error:
                        this._logger.LogError("An error occured while scaning the file! ScanResult: {1}", scanResult.RawResult);
                        result = true;
                        break;
                    case ClamScanResults.Unknown:
                        result = true;
                        this._logger.LogError("Unknown scan result while scaning the file! ScanResult: {0}", scanResult.RawResult);
                        break;
                }
            }
            catch (Exception ex)
            {

                this._logger.LogError("ClamAV Scan Exception: {0}", ex.ToString());
                return true;
            }
            this._logger.LogInformation("ClamAV scan completed for file {0}", file.FileName);
            return result;
        }

    }
}
