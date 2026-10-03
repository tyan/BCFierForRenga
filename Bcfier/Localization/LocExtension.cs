using System;
using System.Windows.Markup;

namespace Bcfier.Localization
{
  public class LocExtension : MarkupExtension
  {
    public string Key { get; set; }

    public LocExtension()
    {
    }

    public LocExtension(string key)
    {
      Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return LocValueGetter.Get(Key);
    }
  }
}