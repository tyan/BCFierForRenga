using System;
using System.ComponentModel;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="BcfComment"/>.
  /// </summary>
  public class CommentVM : INotifyPropertyChanged
  {
    public BcfComment Model { get; private set; }

    public CommentVM(BcfComment model)
    {
      Model = model;
    }

    public static CommentVM FromModel(BcfComment model)
    {
      return new CommentVM(model);
    }

    public Guid Guid
    {
      get { return Model.Guid; }
      set { Model.Guid = value; NotifyPropertyChanged("Guid"); }
    }

    public DateTime Date
    {
      get { return Model.Date; }
      set { Model.Date = value; NotifyPropertyChanged("Date"); }
    }

    public string Author
    {
      get { return Model.Author; }
      set { Model.Author = value; NotifyPropertyChanged("Author"); }
    }

    public string Comment
    {
      get { return Model.Comment; }
      set { Model.Comment = value; NotifyPropertyChanged("Comment"); }
    }

    public Guid? ViewpointGuid
    {
      get { return Model.ViewpointGuid; }
      set { Model.ViewpointGuid = value; NotifyPropertyChanged("ViewpointGuid"); }
    }

    public DateTime ModifiedDate
    {
      get { return Model.ModifiedDate; }
      set { Model.ModifiedDate = value; NotifyPropertyChanged("ModifiedDate"); }
    }

    public string ModifiedAuthor
    {
      get { return Model.ModifiedAuthor; }
      set { Model.ModifiedAuthor = value; NotifyPropertyChanged("ModifiedAuthor"); }
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