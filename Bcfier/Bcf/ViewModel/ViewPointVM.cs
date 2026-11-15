using System;
using System.ComponentModel;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="BcfViewpoint"/>. Holds the UI-facing state
  /// such as <see cref="SnapshotPath"/> and raises change notifications.
  /// </summary>
  public class ViewPointVM : INotifyPropertyChanged
  {
    public BcfViewpoint Model { get; private set; }

    public ViewPointVM(BcfViewpoint model)
    {
      Model = model;
    }

    public static ViewPointVM FromModel(BcfViewpoint model)
    {
      return new ViewPointVM(model);
    }

    public Guid Guid
    {
      get { return Model.Guid; }
      set { Model.Guid = value; NotifyPropertyChanged("Guid"); }
    }

    public string ViewpointFileName
    {
      get { return Model.ViewpointFileName; }
      set { Model.ViewpointFileName = value; NotifyPropertyChanged("ViewpointFileName"); }
    }

    public string SnapshotFileName
    {
      get { return Model.SnapshotFileName; }
      set { Model.SnapshotFileName = value; NotifyPropertyChanged("SnapshotFileName"); }
    }

    public BcfViewpointData Data
    {
      get { return Model.Data; }
      set { Model.Data = value; NotifyPropertyChanged("Data"); }
    }

    //used for an easier binding in the UI
    public string SnapshotPath
    {
      get { return Model.SnapshotPath; }
      set
      {
        Model.SnapshotPath = value;
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