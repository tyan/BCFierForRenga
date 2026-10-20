namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Contract between the UI/VM layer and a concrete BCF persistence backend
  /// (2.1 file, 3.0 file, BCF-API server). Version-neutral.
  /// The domain layer only knows this interface; adapters live in Bcfier.Bcf.Adapters.
  /// </summary>
  public interface IProjectAdapter
  {
    ProjectFormat Format { get; }

    BcfProject Load(string path);
    bool Save(BcfProject project, string path);
    ProjectFormat Detect(string path);
  }
}