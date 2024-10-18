using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vista.DB.Schema;

namespace AsvtTPL.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ResponseCache(NoStore = true)]
public class ImageGateController : ControllerBase
{
  [HttpGet("{id}")]
  public IActionResult Get(string id)
  {
    if (id == "abc")
      return PhysicalFile(@"C:\WFAHRS_Attach\Image\火拳.png", "image/png");

    if (id == "foo")
    {
      FileInfo imgFile = new FileInfo(@"C:\Users\RelyK\OneDrive\圖片\Saved Pictures\妮可羅賓.jpg");
      using var fs = imgFile.OpenRead();
      byte[] imgBlob = new byte[fs.Length];
      fs.Read(imgBlob, 0, imgBlob.Length);
      return File(imgBlob, "image/jpeg");
    }

    return BadRequest();
  }
}
