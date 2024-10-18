namespace Vista.Component.Models;

public record ImageCropInfo
{
  public string ImageFileName { get; set; } = string.Empty;
  public string ImageType { get; set; } = string.Empty;
  public long BlobSize { get; set; } = -1L;
  public int Width { get; set; }
  public int Height { get; set; }
}
