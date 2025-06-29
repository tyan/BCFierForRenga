using System;
using System.Text.RegularExpressions;


namespace Bcfier.RengaPlugin.Entry
{
  public class RengaEntityPath
  {
    public RengaEntityPath() { }

    public RengaEntityPath(Guid entityId, Guid owningEntityId = new Guid())
    {
      OwningEntityId = owningEntityId;
      EntityId = entityId;
    }

    static private string c_emptyInputErrorMsg = "Empty Renga entity path";
    static private string c_invalidFormatErrorMsg = "Invalid Renga entity path format";
    public static RengaEntityPath Parse(string pathString)
    {
      if (pathString == null || pathString.Length == 0)
        throw new ArgumentNullException(c_emptyInputErrorMsg);

      var matchFormatWithoutOwningEntity = Regex.Match(pathString, @"Renga\|([^|]+)$");
      if (matchFormatWithoutOwningEntity.Success)
        return new RengaEntityPath(Guid.Parse(matchFormatWithoutOwningEntity.Groups[1].Value));

      var matchFormatWithOwningEntity = Regex.Match(pathString, @"Renga\|([^|]+)\|([^|]+)$");
      if (matchFormatWithOwningEntity.Success)
        return new RengaEntityPath(Guid.Parse(matchFormatWithOwningEntity.Groups[2].Value), Guid.Parse(matchFormatWithOwningEntity.Groups[1].Value));

      throw new FormatException(c_invalidFormatErrorMsg);
    }

    static public bool TryParse(string pathString, out RengaEntityPath entityPath)
    {
      try
      {
        entityPath = Parse(pathString);
        return true;
      }
      catch(ArgumentNullException) {entityPath = null; return false;}
      catch (FormatException) { entityPath = null; return false; }
    }

    public override string ToString()
    {
      if (EntityId == Guid.Empty)
        return string.Empty;
      else if (OwningEntityId == Guid.Empty)
        return $"Renga|{EntityId.ToString("D")}";
      else
        return $"Renga|{OwningEntityId.ToString("D")}|{EntityId.ToString("D")}";
    }

    public Guid OwningEntityId { get; set; }
    public Guid EntityId { get; set; }
  }
}
