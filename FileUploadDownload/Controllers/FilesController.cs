using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace PdfJavascriptDetector.Controllers
{
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpPost]
        [Route("UploadFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile? uploadedFile, CancellationToken cancellationToken)
        {
            bool isValid = false;
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await uploadedFile.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                isValid = await Validate(memoryStream);
            }

            if(isValid)
                return Ok("File is clean!");
            return BadRequest("Javascript detected!");
        }

        


        private async Task<bool> Validate(MemoryStream stream)
        {

            var isValid = true;
            try
            {
                using PdfReader reader = new PdfReader(stream);
                using PdfDocument pdfDoc = new PdfDocument(reader);
                // Check for document-level JavaScript.
                PdfDictionary catalog = pdfDoc.GetCatalog().GetPdfObject(); // get the root object (the whole catalog) of the document
                PdfDictionary names = catalog.GetAsDictionary(PdfName.Names); // get the names dictionary at the root of the document

                if (names != null)
                {
                    PdfDictionary js = names.GetAsDictionary(PdfName.JavaScript); // get the javascript dictionary from the names dictionary
                    if (js != null)
                    {
                        isValid = false;
                    }
                }

                PdfDictionary java = catalog.GetAsDictionary(PdfName.JavaScript);
                if (java != null)
                {

                    isValid = false;

                }

                //check for forms
                PdfDictionary acroForm = catalog.GetAsDictionary(PdfName.AcroForm);
                if (acroForm != null)
                {
                    isValid = false;
                }

                // Check for Aditional Actions like buttons.
                PdfDictionary aditionalActions = catalog.GetAsDictionary(PdfName.AA);

                if (aditionalActions != null)
                {
                    isValid = false;
                }

                // Check for Actions like buttons.
                PdfDictionary action = catalog.GetAsDictionary(PdfName.A);
                if (action != null)
                {
                    isValid = false;

                }

            }
            catch (Exception e)
            {
                //TODO: handle exception
                return false;
            }

            return await Task.FromResult(isValid);
        }

    }
}
