using System;
using System.Collections.Generic;
using System.Linq;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Bcfier.Localization;
using Renga;

namespace Bcfier.RengaPlugin.Entry
{ 
  public class ExtEvntOpenView
  {
    static private string GetAnyAuthoringToolId(Components components)
    {
      // Here we use AuthoringToolId provided by BCF standard to contain full path to the Renga entity.
      // Consider all Components belong to the same Renga entity

      return components.Selection?.FirstOrDefault()?.AuthoringToolId 
        ?? components.Visibility?.Exceptions?.FirstOrDefault()?.AuthoringToolId 
        ?? components.Coloring?.FirstOrDefault(c => c.Component?.Length > 0)?.Component?.FirstOrDefault()?.AuthoringToolId 
        ?? string.Empty;
    }

    static private Tuple<Renga.IModel, int> GetBuildingModelAndView(Renga.IProject project)
    {
      return new Tuple<Renga.IModel, int>(project.Model, project.BuildingInfo.Id);
    }

    static private Tuple<Renga.IModel, int> GetAssemblyModelAndView(Renga.IProject project, Guid id)
    {
      var assembly = project.Assemblies.GetByUniqueId(id);
      return new Tuple<Renga.IModel, int>(assembly as Renga.IModel, assembly.Id);
    }
    static private Tuple<Renga.IModel, int> GetDrawingModelAndView(Renga.IProject project, Guid id)
    {
      var drawing = project.Drawings2.GetByUniqueId(id);
      return new Tuple<Renga.IModel, int>(drawing as Renga.IModel, drawing.Id);
    }

    static private Tuple<Renga.IModel, int> GetModelAndView(Renga.IProject project, Components components)
    {
      // Building model is the default case
      var anyAuthoringToolIdString = GetAnyAuthoringToolId(components);
      if (anyAuthoringToolIdString == string.Empty)
        return GetBuildingModelAndView(project);

      var rengaEntityPath = new RengaEntityPath();
      if (!RengaEntityPath.TryParse(anyAuthoringToolIdString, out rengaEntityPath))
        return GetBuildingModelAndView(project);

      if (rengaEntityPath.OwningEntityId == Guid.Empty)
        return GetBuildingModelAndView(project);

      if (project.Assemblies.ContainsUniqueId(rengaEntityPath.OwningEntityId))
        return GetAssemblyModelAndView(project, rengaEntityPath.OwningEntityId);
      else if (project.Drawings2.ContainsUniqueId(rengaEntityPath.OwningEntityId))
        return GetDrawingModelAndView(project, rengaEntityPath.OwningEntityId);
      else 
        return GetBuildingModelAndView(project);
    }
    private Renga.IView OpenView(Guid entityId)
    {
      return null;
    }

    static private void SetCameraPosition(Renga.IModelView view, PerspectiveCamera bcfCamera)
    {
      var rengaCamera = (view as Renga.IView3DParams)?.Camera;
      if (rengaCamera == null)
        return;

      var rengaCameraPos = new Renga.FloatPoint3D();
      var bcfCameraPos = bcfCamera.CameraViewPoint;
      rengaCameraPos.X = (float)bcfCameraPos.X * 1000;
      rengaCameraPos.Y = (float)bcfCameraPos.Y * 1000;
      rengaCameraPos.Z = (float)bcfCameraPos.Z * 1000;

      var rengaFocusPoint = new Renga.FloatPoint3D();
      var bcfCameraDirection = bcfCamera.CameraDirection;
      rengaFocusPoint.X = rengaCameraPos.X + (float)bcfCameraDirection.X * 1000;
      rengaFocusPoint.Y = rengaCameraPos.Y + (float)bcfCameraDirection.Y * 1000;
      rengaFocusPoint.Z = rengaCameraPos.Z + (float)bcfCameraDirection.Z * 1000;

      var rengaUpVector = new Renga.FloatVector3D();
      var bcfUpVector = bcfCamera.CameraUpVector;
      rengaUpVector.X = (float)bcfUpVector.X;
      rengaUpVector.Y = (float)bcfUpVector.Y;
      rengaUpVector.Z = (float)bcfUpVector.Z;
      rengaCamera.LookAt(rengaFocusPoint, rengaCameraPos, rengaUpVector);
    }

    static public void Execute(Renga.IApplication app, VisualizationInfo viewInfo)
    {
      try
      {
        if (app.Project == null)
          return;

        var owningModelAndView = GetModelAndView(app.Project, viewInfo.Components);
        
        var project = app.Project;
        var model = owningModelAndView.Item1;

        var view = app.OpenViewByEntity(owningModelAndView.Item2);
        var modelView = view as Renga.IModelView;
        
        if (modelView == null) 
          return;

        if (viewInfo.PerspectiveCamera != null)
          SetCameraPosition(modelView, viewInfo.PerspectiveCamera);

        // Visibility
        var allObjects = model.GetObjects();
        var allObjectIds = model.GetObjects().GetIds();
        var exceptionIds = new List<int>();
        var otherIds = new List<int>();
        var defaultVisibility = false;

        if (viewInfo.Components.Visibility == null)
        {
          otherIds = allObjectIds.OfType<int>().ToList();
        }
        else
        {
          if (viewInfo.Components.Visibility.DefaultVisibilitySpecified)
            defaultVisibility = viewInfo.Components.Visibility.DefaultVisibility;

          Func<string, bool> isExceptionObject = (string ifcGuid) =>
          {
            return Array.Find(viewInfo.Components.Visibility.Exceptions, exception => exception.IfcGuid == ifcGuid) != null;
          };

          foreach (int id in allObjectIds)
          {
            var rengaUuid = allObjects.GetById(id).UniqueId;
            var ifcGuid = IfcGuid.ToIfcGuid(rengaUuid);
            if (isExceptionObject(ifcGuid))
              exceptionIds.Add(id);
            else
              otherIds.Add(id);
          }
        }

        modelView.SetObjectsVisibility(otherIds.ToArray(), defaultVisibility);
        modelView.SetObjectsVisibility(exceptionIds.ToArray(), !defaultVisibility);

        // Selection
        var selectedObjectsLocalIds = new List<int>();

        foreach (var selectedComponent in viewInfo.Components.Selection)
        {
          var selectedObjectGlobalId = IfcGuid.FromIfcGUID(selectedComponent.IfcGuid);
          var selectedObjectLocalId = model.GetIdFromUniqueId(selectedObjectGlobalId);
          selectedObjectsLocalIds.Add(selectedObjectLocalId);
        }
        app.Selection.SetSelectedObjects(selectedObjectsLocalIds.ToArray());
      }
      catch (Exception ex)
      {
        Bcfier.Data.Utils.Utils.ShowErrorMessageBox(LocValueGetter.Get("UnknownError"), ex);
      }
    }
  }
}