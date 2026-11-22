namespace Bcfier.Bcf.Bcf21
{
  // Stores the deserialized VisualizationInfo inside the markup ViewPoint so there is a
  // 1:1 correspondence between the Markup/Viewpoints entries and the actual .bcfv payloads
  // (same pattern as the legacy Bcf2.ViewPoint partial).
  public partial class ViewPoint
  {
    private VisualizationInfo _visInfoField;

    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public VisualizationInfo VisInfo
    {
      get { return this._visInfoField; }
      set { this._visInfoField = value; }
    }
  }
}
