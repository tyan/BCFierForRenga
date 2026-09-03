using System;
using System.ComponentModel;
using Bcfier.Bcf.Bcf2;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ViewModel around the model <see cref="Comment"/>.
  /// </summary>
  public class CommentVM : INotifyPropertyChanged
  {
    public Comment Model { get; private set; }

    public CommentVM(Comment model)
    {
      Model = model;
    }

    public static CommentVM FromModel(Comment model)
    {
      return new CommentVM(model);
    }

    public string Guid
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

    public string Comment1
    {
      get { return Model.Comment1; }
      set { Model.Comment1 = value; NotifyPropertyChanged("Comment1"); }
    }

    public CommentViewpoint Viewpoint
    {
      get { return Model.Viewpoint; }
      set { Model.Viewpoint = value; NotifyPropertyChanged("Viewpoint"); }
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