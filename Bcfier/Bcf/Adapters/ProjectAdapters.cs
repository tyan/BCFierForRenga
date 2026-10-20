using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Placeholder adapter around the current legacy BCF 2.1 persistence (BcfSerializer + Bcf2.*).
  /// Implemented in step 2a (mapper Bcf2 -> domain) and 2b (mapper domain -> Bcf2).
  /// Lives in Bcfier.Bcf.Adapters (not in the domain model).
  /// </summary>
  public class Bcf21FileAdapter : IProjectAdapter
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
  /// Placeholder for the BCF 3.0 file adapter (step 4).
  /// </summary>
  public class Bcf30FileAdapter : IProjectAdapter
  {
    public ProjectFormat Format { get { return ProjectFormat.Bcf30; } }

    public BcfProject Load(string path)
    {
      throw new System.NotImplementedException("Bcf30FileAdapter is implemented in step 4.");
    }

    public bool Save(BcfProject project, string path)
    {
      throw new System.NotImplementedException("Bcf30FileAdapter is implemented in step 4.");
    }

    public ProjectFormat Detect(string path)
    {
      return ProjectFormat.Bcf30;
    }
  }

  /// <summary>
  /// Placeholder for the BCF-API adapter (step 5).
  /// </summary>
  public class BcfApiAdapter : IProjectAdapter
  {
    public ProjectFormat Format { get { return ProjectFormat.BcfApi; } }

    public BcfProject Load(string path)
    {
      throw new System.NotImplementedException("BcfApiAdapter is implemented in step 5.");
    }

    public bool Save(BcfProject project, string path)
    {
      throw new System.NotImplementedException("BcfApiAdapter is implemented in step 5.");
    }

    public ProjectFormat Detect(string path)
    {
      return ProjectFormat.BcfApi;
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
        case ProjectFormat.Bcf21:
          return new Bcf21FileAdapter();
        case ProjectFormat.Bcf30:
          return new Bcf30FileAdapter();
        case ProjectFormat.BcfApi:
          return new BcfApiAdapter();
        default:
          throw new System.NotSupportedException("Unsupported project format: " + format);
      }
    }
  }
}