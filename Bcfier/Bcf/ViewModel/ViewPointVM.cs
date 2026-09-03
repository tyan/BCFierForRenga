using System;
using System.ComponentModel;
using Bcfier.Bcf.Bcf2;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="ViewPoint"/>. Holds the UI-facing state
  /// such as <see cref="SnapshotPath"/> and raises change notifications.
  /// </summary>
  public class ViewPointVM : INotifyPropertyChanged
  {
    public ViewPoint Model { get; private set; }

    public ViewPointVM(ViewPoint model)
    {
      Model = model;
      SnapshotPath = model.SnapshotPath;
    }

    public static ViewPointVM FromModel(ViewPoint model)
    {
      return new ViewPointVM(model);
    }

    public string Guid
    {
      get { return Model.Guid; }
      set { Model.Guid = value; NotifyPropertyChanged("Guid"); }
    }

    public string Viewpoint
    {
      get { return Model.Viewpoint; }
      set { Model.Viewpoint = value; NotifyPropertyChanged("Viewpoint"); }
    }

    public string Snapshot
    {
      get { return Model.Snapshot; }
      set { Model.Snapshot = value; NotifyPropertyChanged("Snapshot"); }
    }

    public int Index
    {
      get { return Model.Index; }
      set { Model.Index = value; NotifyPropertyChanged("Index"); }
    }

    public VisualizationInfo VisInfo
    {
      get { return Model.VisInfo; }
      set { Model.VisInfo = value; NotifyPropertyChanged("VisInfo"); }
    }

    private string _snapshotPath;

    //used for an easier binding in the UI
    public string SnapshotPath
    {
      get { return _snapshotPath; }
      set
      {
        _snapshotPath = value;
        NotifyPropertyChanged("SnapshotPath");
      }
    }

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
}