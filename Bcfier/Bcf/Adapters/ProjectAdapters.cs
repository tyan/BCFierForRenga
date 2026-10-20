using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Placeholder adapter around the current legacy BCF 2.1 persistence (BcfSerializer + Bcf2.*).
  /// </summary>
  public class Bcf21LegacyFileAdapter : IProjectAdapter
  {
    public ProjectFormat Format { get { return ProjectFormat.Bcf21; } }

    public BcfProject Load(string path)
    {
      throw new System.NotImplementedException("Bcf21FileAdapter.Load is implemented in step 2a.");
    }

    public bool Save(BcfProject project, string path)
    {
      throw new System.NotImplementedException("Bcf21FileAdapter.Save is implemented in step 2b.");
    }

    public ProjectFormat Detect(string path)
    {
      return ProjectFormat.Bcf21;
    }
  }

  
  /// <summary>
  /// Creates the adapter matching a given BCF format.
  /// </summary>
  public static class ProjectAdapterFactory
  {
    public static IProjectAdapter Create(ProjectFormat format)
    {
      switch (format)
      {
        case ProjectFormat.Bcf21Legacy:
          return new Bcf21LegacyFileAdapter();
        case ProjectFormat.Bcf21:
        default:
          throw new System.NotSupportedException("Unsupported project format: " + format);
      }
    }
  }
}
