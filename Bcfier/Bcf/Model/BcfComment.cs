using System;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of a comment on a BCF topic.
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfComment
  {
    public Guid Guid { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; }
    public string Comment { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ModifiedAuthor { get; set; }
    public Guid? ViewpointGuid { get; set; }

    public BcfComment()
    {
      Guid = System.Guid.NewGuid();
    }
  }
}
