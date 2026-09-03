using System;
using System.Collections.Generic;
using Bcfier.Bcf.Bcf2;

namespace Bcfier.Bcf
{
  /// <summary>
  /// Pure data model of a deserialized BCF. No UI state and no change notifications;
  /// those live in the <c>Bcfier.Bcf.ViewModel</c> layer.
  /// </summary>
  public class BcfFile
  {
    private Guid id;

    public string TempPath { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Filename { get; set; }
    public string Fullname { get; set; }

    public List<Markup> Issues { get; set; }

    public Guid Id
    {
      get { return id; }
      set { id = value; }
    }

    public BcfFile()
    {
      Id = Guid.NewGuid();
      TempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BCFier", Id.ToString());
      Issues = new List<Markup>();
    }
  }
}