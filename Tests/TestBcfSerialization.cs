using Bcfier.Bcf;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
  public class TestDeserializeBcf
  {
    private readonly string _noSnapshotBcfPath = Path.Combine(AppContext.BaseDirectory, "data", "no_snapshot.bcf");
    private readonly string _missingBcfvPath = Path.Combine(AppContext.BaseDirectory, "data", "missing_bcfv.bcf");

    [TearDown]
    public void CleanUp()
    {
      var tempDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      if (Directory.Exists(tempDir))
        Directory.Delete(tempDir, true);
    }

    [Test]
    public void deserialize_bcf_without_snapshot_does_not_throw()
    {
      // given
      var container = new BcfContainer();

      // when / then
      Assert.That(() => container.OpenFile(_noSnapshotBcfPath), Throws.Nothing);
    }

    [Test]
    public void deserialize_bcf_without_snapshot_leaves_snapshot_path_null()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_noSnapshotBcfPath);

      // when
      var viewpoint = container.BcfFiles.Single().Issues.Single().Viewpoints.Single();

      // then
      Assert.That(viewpoint.SnapshotPath, Is.Null);
    }

    [Test]
    public void save_bcf_without_snapshot_does_not_throw()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      bcf.Fullname = Path.Combine(outputDir, "no_snapshot_out.bcf");

      // when / then
      Assert.That(() => container.SaveFile(bcf), Throws.Nothing);
    }

    [Test]
    public void save_and_reload_bcf_without_snapshot_leaves_snapshot_path_null()
    {
      // given
      var container = new BcfContainer();
      container.OpenFile(_noSnapshotBcfPath);

      var bcf = container.BcfFiles.Single();
      var outputDir = Path.Combine(AppContext.BaseDirectory, "test_temp");
      Directory.CreateDirectory(outputDir);
      var outPath = Path.Combine(outputDir, "no_snapshot_out.bcf");
      bcf.Fullname = outPath;

      // when
      container.SaveFile(bcf);

      // reload the saved file
      var reloaded = new BcfContainer();
      reloaded.OpenFile(outPath);

      // then
      var viewpoint = reloaded.BcfFiles.Single().Issues.Single().Viewpoints.Single();
      Assert.That(viewpoint.SnapshotPath, Is.Null);
    }

    [Test]
    public void load_bcf_with_missing_viewpoint_file_throws_invalid_data_exception()
    {
      // when / then
      Assert.That(() => BcfSerializer.load(_missingBcfvPath), Throws.InstanceOf<System.IO.InvalidDataException>());
    }
  }
}
