using System;
using System.Reflection;
using System.Resources;
using WPFLocalizeExtension.Engine;

namespace Bcfier.Localization
{
  public class LocValueGetter
  {
    static Lazy<ResourceManager> _resourceManager = new Lazy<ResourceManager>(() =>
    {
      var assembly = Assembly.GetExecutingAssembly();
      return new ResourceManager("BCFier.Localization.Strings", assembly);
    });

    static public string Get(string key)
    {
      return _resourceManager.Value.GetString(key, LocalizeDictionary.Instance.Culture);
    }
  }
}
