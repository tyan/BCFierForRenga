using System;
using System.Collections.ObjectModel;

namespace Bcfier.Bcf.ViewModel
{
  /// <summary>
  /// ObservableCollection of ViewModel items that mirrors every mutation into the
  /// underlying model collection, so that a plain <c>Add</c>/<c>Remove</c>/<c>Clear</c>
  /// keeps the model (the serialization source of truth) in sync without manual dual writes.
  /// </summary>
  public class ObservableCollectionVM<TVm, TModel> : ObservableCollection<TVm>
  {
    private readonly ObservableCollection<TModel> _model;
    private readonly Func<TVm, TModel> _toModel;

    public ObservableCollectionVM(ObservableCollection<TModel> model, Func<TModel, TVm> fromModel, Func<TVm, TModel> toModel)
    {
      _model = model;
      _toModel = toModel;
      //wrap the items that already exist in the model without mirroring them back
      foreach (var modelItem in model)
        base.InsertItem(Count, fromModel(modelItem));
    }

    protected override void InsertItem(int index, TVm item)
    {
      base.InsertItem(index, item);
      _model.Add(_toModel(item));
    }

    protected override void RemoveItem(int index)
    {
      var item = Items[index];
      base.RemoveItem(index);
      _model.Remove(_toModel(item));
    }

    protected override void ClearItems()
    {
      base.ClearItems();
      _model.Clear();
    }

    protected override void SetItem(int index, TVm item)
    {
      var oldItem = Items[index];
      base.SetItem(index, item);
      var modelIndex = _model.IndexOf(_toModel(oldItem));
      if (modelIndex >= 0)
        _model[modelIndex] = _toModel(item);
    }
  }
}