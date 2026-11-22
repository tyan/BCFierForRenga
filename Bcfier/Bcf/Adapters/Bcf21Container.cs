using System;
using System.Collections.Generic;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// In-memory carrier of a loaded schema-valid BCF 2.1 file (counterpart of the legacy
  /// <c>BcfFile</c>): project metadata plus deserialized issue markups. Lives in the
  /// adapters layer; the domain model is <see cref="Bcfier.Bcf.Model.BcfProject"/>.
  /// </summary>
  internal class Bcf21Container
  {
    public Guid Id { get; set; }
    public string ProjectName { get; set; }
    public Guid ProjectId { get; set; }
    public string Filename { get; set; }
    public string Fullname { get; set; }
    public string TempPath { get; set; }

    public List<Bcfier.Bcf.Bcf21.Markup> Issues { get; set; } = new List<Bcfier.Bcf.Bcf21.Markup>();

    public Bcf21Container()
    {
      Id = Guid.NewGuid();
      TempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BCFier", Id.ToString());
    }
  }
}
