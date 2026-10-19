using System;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of a viewpoint (viewpoint.bcfv + snapshot).
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfViewpoint
  {
    public Guid Guid { get; set; }
    public string ViewpointFileName { get; set; }
    public string SnapshotFileName { get; set; }
    public string SnapshotPath { get; set; }
    public BcfViewpointData Data { get; set; }

    public BcfViewpoint()
    {
      Guid = System.Guid.NewGuid();
    }

    public BcfViewpoint(bool isFirst)
    {
      Guid = System.Guid.NewGuid();
      if (isFirst)
      {
        ViewpointFileName = "viewpoint.bcfv";
        SnapshotFileName = "snapshot.png";
      }
      else
      {
        ViewpointFileName = Guid + ".bcfv";
        SnapshotFileName = Guid + ".png";
      }
    }
  }
}
