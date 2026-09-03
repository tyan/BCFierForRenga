using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="Topic"/>. Exposes the scalar data plus
  /// the UI-facing dropdown option collections (not part of the BCF model).
  /// </summary>
  public class TopicVM : INotifyPropertyChanged
  {
    public Topic Model { get; private set; }

    public TopicVM(Topic model)
    {
      Model = model;

      TopicStatusesCollection = new ObservableCollection<string>();
      TopicTypesCollection = new ObservableCollection<string>();
      foreach (var status in Globals.AvailStatuses)
        TopicStatusesCollection.Add(status);
      foreach (var type in Globals.AvailTypes)
        TopicTypesCollection.Add(type);
    }

    public static TopicVM FromModel(Topic model)
    {
      return new TopicVM(model);
    }

    public string Guid
    {
      get { return Model.Guid; }
      set { Model.Guid = value; NotifyPropertyChanged("Guid"); }
    }

    public string Title
    {
      get { return Model.Title; }
      set { Model.Title = value; NotifyPropertyChanged("Title"); }
    }

    public string Description
    {
      get { return Model.Description; }
      set { Model.Description = value; NotifyPropertyChanged("Description"); }
    }

    public string Priority
    {
      get { return Model.Priority; }
      set { Model.Priority = value; NotifyPropertyChanged("Priority"); }
    }

    public int Index
    {
      get { return Model.Index; }
      set { Model.Index = value; NotifyPropertyChanged("Index"); }
    }

    public string[] Labels
    {
      get { return Model.Labels; }
      set { Model.Labels = value; NotifyPropertyChanged("Labels"); }
    }

    public DateTime CreationDate
    {
      get { return Model.CreationDate; }
      set { Model.CreationDate = value; NotifyPropertyChanged("CreationDate"); }
    }

    public string CreationAuthor
    {
      get { return Model.CreationAuthor; }
      set { Model.CreationAuthor = value; NotifyPropertyChanged("CreationAuthor"); }
    }

    public DateTime ModifiedDate
    {
      get { return Model.ModifiedDate; }
      set { Model.ModifiedDate = value; NotifyPropertyChanged("ModifiedDate"); }
    }

    public DateTime DueDate
    {
      get { return Model.DueDate; }
      set { Model.DueDate = value; NotifyPropertyChanged("DueDate"); }
    }

    public string AssignedTo
    {
      get { return Model.AssignedTo; }
      set { Model.AssignedTo = value; NotifyPropertyChanged("AssignedTo"); }
    }

    public string Stage
    {
      get { return Model.Stage; }
      set { Model.Stage = value; NotifyPropertyChanged("Stage"); }
    }

    public string TopicType
    {
      get { return Model.TopicType; }
      set { Model.TopicType = value; NotifyPropertyChanged("TopicType"); }
    }

    public string TopicStatus
    {
      get { return Model.TopicStatus; }
      set { Model.TopicStatus = value; NotifyPropertyChanged("TopicStatus"); }
    }

    public ObservableCollection<string> TopicStatusesCollection { get; private set; }
    public ObservableCollection<string> TopicTypesCollection { get; private set; }

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