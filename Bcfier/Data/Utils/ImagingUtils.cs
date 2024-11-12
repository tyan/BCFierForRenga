using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bcfier.Data.Utils
{
  public static class ImagingUtils
  {
    /// <summary>
    /// Classes for working with images
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <returns></returns>
    public static ImageSource ImageSourceFromPath(string sourcePath)
    {
      var image = BitmapFromPath(sourcePath);
      int width = image.PixelWidth;
      int height = image.PixelHeight;
      byte[] imageBytes = LoadImageData(sourcePath);
      return CreateImage(imageBytes, width, 0);
    }

    public static void SaveImageSource(ImageSource image, string destPath)
    {
      var imageBytes = GetEncodedImageData(image, ".jpg");
      SaveImageData(imageBytes, destPath);
    }

    public static BitmapImage BitmapFromPath(string path)
    {
      var image = new BitmapImage();
      image.BeginInit();
      image.UriSource = new Uri(path);
      image.CacheOption = BitmapCacheOption.OnLoad;
      image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
      image.EndInit();
      return image;
    }
    public static BitmapSource ConvertBitmapTo96Dpi(BitmapImage bitmapImage)
    {
      double dpi = 96;
      int width = bitmapImage.PixelWidth;
      int height = bitmapImage.PixelHeight;
      int stride = width * 4; // 4 bytes per pixel
      byte[] pixelData = new byte[stride * height];
      bitmapImage.CopyPixels(pixelData, stride, 0);

      return BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
    }

    private static byte[] LoadImageData(string filePath)
    {
      FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      BinaryReader br = new BinaryReader(fs);
      byte[] imageBytes = br.ReadBytes((int)fs.Length);
      br.Close();
      fs.Close();
      return imageBytes;
    }

    private static ImageSource CreateImage(byte[] imageData, int decodePixelWidth, int decodePixelHeight)
    {
      if (imageData == null) return null;
      BitmapImage result = new BitmapImage();
      result.BeginInit();
      if (decodePixelWidth > 0)
      {
        result.DecodePixelWidth = decodePixelWidth;
      }
      if (decodePixelHeight > 0)
      {
        result.DecodePixelHeight = decodePixelHeight;
      }
      result.StreamSource = new MemoryStream(imageData);
      result.CreateOptions = BitmapCreateOptions.None;
      result.CacheOption = BitmapCacheOption.Default;
      result.EndInit();
      return result;
    }

    private static void SaveImageData(byte[] imageData, string filePath)
    {
      FileStream fs = new FileStream(filePath, FileMode.Create,
      FileAccess.Write);
      BinaryWriter bw = new BinaryWriter(fs);
      bw.Write(imageData);
      bw.Close();
      fs.Close();
    }

    private static byte[] GetEncodedImageData(ImageSource image, string preferredFormat)
    {
      byte[] result = null;
      BitmapEncoder encoder = null;
      switch (preferredFormat.ToLower())
      {
        case ".jpg":
        case ".jpeg":
          encoder = new JpegBitmapEncoder();
          break;
        case ".bmp":
          encoder = new BmpBitmapEncoder();
          break;
        case ".png":
          encoder = new PngBitmapEncoder();
          break;
        case ".tif":
        case ".tiff":
          encoder = new TiffBitmapEncoder();
          break;
        case ".gif":
          encoder = new GifBitmapEncoder();
          break;
        case ".wmp":
          encoder = new WmpBitmapEncoder();
          break;
      }
      if (image is BitmapSource)
      {
        MemoryStream stream = new MemoryStream();
        encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
        encoder.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        result = new byte[stream.Length];
        BinaryReader br = new BinaryReader(stream);
        br.Read(result, 0, (int)stream.Length);
        br.Close();
        stream.Close();
      }
      return result;
    }
  }
}
