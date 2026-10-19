using System;
using System.Collections.Generic;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of viewpoint data (payload of a .bcfv / VisualizationInfo in 2.1/3.0).
  /// Pure POCO — no serialization attributes. Mirrors Bcf2.VisualizationInfo without loss.
  /// </summary>
  public class BcfViewpointData
  {
    public BcfComponentsData Components { get; set; }
    public BcfOrthogonalCameraData OrthogonalCamera { get; set; }
    public BcfPerspectiveCameraData PerspectiveCamera { get; set; }
    public BcfSheetCameraData SheetCamera { get; set; }
    public List<BcfLineData> Lines { get; set; } = new List<BcfLineData>();
    public List<BcfClippingPlaneData> ClippingPlanes { get; set; } = new List<BcfClippingPlaneData>();
    public List<BcfBitmapData> Bitmaps { get; set; } = new List<BcfBitmapData>();
    public string Guid { get; set; }

    public BcfViewpointData()
    {
      Guid = System.Guid.NewGuid().ToString();
    }
  }

  public class BcfComponentsData
  {
    public BcfViewSetupHintsData ViewSetupHints { get; set; }
    public List<BcfComponentData> Selection { get; set; } = new List<BcfComponentData>();
    public BcfComponentVisibilityData Visibility { get; set; }
    public List<BcfComponentColoringData> Coloring { get; set; } = new List<BcfComponentColoringData>();
  }

  public class BcfViewSetupHintsData
  {
    public bool SpacesVisible { get; set; }
    public bool SpaceBoundariesVisible { get; set; }
    public bool OpeningsVisible { get; set; }
  }

  public class BcfComponentData
  {
    public string IfcGuid { get; set; }
    public string OriginatingSystem { get; set; }
    public string AuthoringToolId { get; set; }
  }

  public class BcfComponentVisibilityData
  {
    public bool DefaultVisibility { get; set; }
    public List<BcfComponentData> Exceptions { get; set; } = new List<BcfComponentData>();
  }

  public class BcfComponentColoringData
  {
    public string Color { get; set; }
    public List<BcfComponentData> Components { get; set; } = new List<BcfComponentData>();
  }

  public class BcfLineData
  {
    public BcfPointData StartPoint { get; set; }
    public BcfPointData EndPoint { get; set; }
  }

  public class BcfClippingPlaneData
  {
    public BcfPointData Location { get; set; }
    public BcfDirectionData Direction { get; set; }
  }

  public class BcfBitmapData
  {
    public string BitmapFormat { get; set; }
    public string Reference { get; set; }
    public BcfPointData Location { get; set; }
    public BcfDirectionData Normal { get; set; }
    public BcfDirectionData Up { get; set; }
    public double Height { get; set; }
  }

  public class BcfOrthogonalCameraData
  {
    public BcfPointData CameraViewPoint { get; set; }
    public BcfDirectionData CameraDirection { get; set; }
    public BcfDirectionData CameraUpVector { get; set; }
    public double ViewToWorldScale { get; set; }
  }

  public class BcfPerspectiveCameraData
  {
    public BcfPointData CameraViewPoint { get; set; }
    public BcfDirectionData CameraDirection { get; set; }
    public BcfDirectionData CameraUpVector { get; set; }
    public double FieldOfView { get; set; }
  }

  public class BcfSheetCameraData
  {
    public BcfPointData TopLeft { get; set; }
    public BcfPointData BottomRight { get; set; }
    public int SheetID { get; set; }
    public string SheetName { get; set; }
  }

  public class BcfPointData
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }

  public class BcfDirectionData
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }
}
