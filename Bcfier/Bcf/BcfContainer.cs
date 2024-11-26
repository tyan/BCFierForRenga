using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Bcfier.Data;
using System.Xml.Linq;

namespace Bcfier.Bcf
{
  /// <summary>
  /// Model View, binds to the tab control, contains the BCf files
  /// and the main methods as save, open...
  /// </summary>
  public class BcfContainer : INotifyPropertyChanged
  {
    private ObservableCollection<BcfFile> _bcfFiles { get; set; }
    private int _selectedReport { get; set; }

    public BcfContainer()
    {
      BcfFiles = new ObservableCollection<BcfFile>();
    }

    public ObservableCollection<BcfFile> BcfFiles
    {
      get
      {
        return _bcfFiles;
      }

      set
      {
        _bcfFiles = value;
        NotifyPropertyChanged("BcfFiles");
      }
    }

    public int SelectedReportIndex
    {
      get
      {
        return _selectedReport;
      }

      set
      {
        _selectedReport = value;
        NotifyPropertyChanged("SelectedReportIndex");
      }
    }

    // Remove this
    public void NewFile()
    {
      AddBcf(new BcfFile());
    }
    public void SaveFile(BcfFile bcf)
    {
      SaveBcfFile(bcf);
    }
    public void MergeFiles(BcfFile bcf)
    {
      var bcffiles = OpenBcfDialog();
      if (bcffiles == null)
        return;
      bcf.MergeBcfFile(bcffiles);
    }

    public void OpenFile(string path)
    {
      var bcf = OpenBcfFile(path);
      AddBcf(bcf);
    }
    public void OpenFile()
    {
      var bcfs = OpenBcfDialog();
      if (bcfs == null)
        return;
      foreach (var bcf in bcfs)
      {
        if (bcf == null)
          continue;
        AddBcf(bcf);
      }
    }

    private void AddBcf(BcfFile newbcf)
    {
      if (newbcf == null)
        return;

      BcfFiles.Add(newbcf);
      SelectedReportIndex = BcfFiles.Count - 1;
      if (newbcf.Issues.Any())
        newbcf.SelectedIssue = newbcf.Issues.First();

      foreach (var issue in newbcf.Issues)
      {
        if (!Globals.OpenStatuses.Contains(issue.Topic.TopicStatus))
          Globals.OpenStatuses.Add(issue.Topic.TopicStatus);

        if (!Globals.OpenTypes.Contains(issue.Topic.TopicType))
          Globals.OpenTypes.Add(issue.Topic.TopicType);
      }
    }

    public void CloseFile(BcfFile bcf)
    {
      _bcfFiles.Remove(bcf);
      Utils.DeleteDirectory(bcf.TempPath);
    }

    public void CloseAllFiles()
    {
      foreach (var bcf in _bcfFiles)
        Utils.DeleteDirectory(bcf.TempPath);
      _bcfFiles.Clear();
    }

    /// <summary>
    /// Removes all old statuses and types from the collection and adds the new ones, except for the selected one
    /// This avoids having blank fields in case an existing value is removed
    /// could probably be optimized
    /// </summary>
    public void UpdateDropdowns()
    {
      Globals.SetStatuses(UserSettings.Get("Statuses"));
      Globals.SetTypes(UserSettings.Get("Types"));

      foreach (var bcf in BcfFiles)
      {
        foreach (var issue in bcf.Issues)
        {
          var oldStatus = issue.Topic.TopicStatus;
          var oldType = issue.Topic.TopicType;

          //status
          for (int i = issue.Topic.TopicStatusesCollection.Count - 1; i >= 0; i--)
          {
            if (issue.Topic.TopicStatusesCollection[i] != oldStatus)
              issue.Topic.TopicStatusesCollection.RemoveAt(i);
          }
          foreach (var status in Globals.AvailStatuses)
          {
            if (status != oldStatus || !issue.Topic.TopicStatusesCollection.Contains(status))
              issue.Topic.TopicStatusesCollection.Add(status);
          }
          //type
          for (int i = issue.Topic.TopicTypesCollection.Count - 1; i >= 0; i--)
          {
            if (issue.Topic.TopicTypesCollection[i] != oldType)
              issue.Topic.TopicTypesCollection.RemoveAt(i);
          }
          foreach (var type in Globals.AvailTypes)
          {
            if (type != oldType || !issue.Topic.TopicTypesCollection.Contains(type))
              issue.Topic.TopicTypesCollection.Add(type);
          }
        }
      }
    }

    public static string FileFilter
    {
      get { return String.Format("BIM Collaboration Format (*{0})|*{0}", BcfSerializer.FileExtension); }
    }

    #region private methods
    /// <summary>
    /// Prompts a dialog to select one or more BCF files to open
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<BcfFile> OpenBcfDialog()
    {
      try
      {
        var openFileDialog1 = new Microsoft.Win32.OpenFileDialog
        {
          Title = String.Format("Open BCF file ({0})", BcfSerializer.FileExtension),
          Filter = FileFilter,
          DefaultExt = BcfSerializer.FileExtension,
          Multiselect = true,
          RestoreDirectory = true,
          CheckFileExists = true,
          CheckPathExists = true
        };
        var result = openFileDialog1.ShowDialog(); // Show the dialog.

        if (result == true) // Test result.
          return openFileDialog1.FileNames.Select(OpenBcfFile).ToList();
      }
      catch (Exception ex)
      {
        Utils.ShowErrorMessageBox("Failed to open one or more BCF files", ex);
      }

      return null;
    }

    /// <summary>
    /// Logic that extracts files from a bcf file and deserializes them
    /// </summary>
    /// <param name="bcffile">Path to the .bcf file</param>
    /// <returns></returns>
    private static BcfFile OpenBcfFile(string filePath)
    {
      return BcfSerializer.load(filePath);
    }

    /// <summary>
    /// Serializes to a bcf and saves it to disk
    /// </summary>
    /// <param name="bcffile"></param>
    /// <returns></returns>
    private static bool SaveBcfFile(BcfFile bcf)
    {
      // Show save file dialog box
      var name = !string.IsNullOrEmpty(bcf.Filename)
          ? bcf.Filename
          : "New BCF Report";
      var filename = SaveBcfDialog(name);
      // Process save file dialog box results
      if (string.IsNullOrWhiteSpace(filename))
        return false;
      return BcfSerializer.save(bcf, filename);
    }

    /// <summary>
    /// Prompts a the user to select where to save the bcf
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private static string SaveBcfDialog(string filename)
    {
      var saveFileDialog = new Microsoft.Win32.SaveFileDialog
      {
        Title = String.Format("Save as BCF file ({0})", BcfSerializer.FileExtension),
        FileName = filename,
        DefaultExt = BcfSerializer.FileExtension,
        Filter = FileFilter
      };

      //if it goes fine I return the filename, otherwise empty
      var result = saveFileDialog.ShowDialog();
      return result == true ? saveFileDialog.FileName : "";
    }

    #endregion

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
  }

  class ZipEncoder : UTF8Encoding
  {
    public ZipEncoder()
    {

    }
    public override byte[] GetBytes(string s)
    {
      s = s.Replace("\\", "/");
      return base.GetBytes(s);
    }
  }
}
