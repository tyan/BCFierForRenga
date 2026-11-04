using System;
using System.Collections.Generic;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of an opened BCF project.
  /// </summary>
  public class BcfProject
  {
    public Guid Id { get; set; }
    public string ProjectName { get; set; }
    public Guid ProjectId { get; set; }
    public string Filename { get; set; }
    public string Fullname { get; set; }
    public string TempPath { get; set; }

    /// <summary>
    /// Concrete BCF persistence backend this project was loaded from; for new files it is
    /// the default <see cref="ProjectFormat.Bcf21"/> and decides the save codec.
    /// </summary>
    public ProjectFormat SourceFormat { get; set; } = ProjectFormat.Bcf21;

    public List<BcfIssue> Issues { get; set; } = new List<BcfIssue>();

    public BcfProject()
    {
      Id = Guid.NewGuid();
    }
  }
}
