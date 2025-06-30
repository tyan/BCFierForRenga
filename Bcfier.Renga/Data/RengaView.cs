using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Bcfier.RengaPlugin.Entry;
using Renga;

namespace Bcfier.RengaPlugin.Data
{
  //Methods for working with views
  public static class RengaView
  {
    private static Renga.IModel GetModel(Renga.IProject project, int entityId)
    {
      if (project.BuildingInfo.Id == entityId)
        return project.Model;
      if (project.Assemblies.Contains(entityId))
        return project.Assemblies.GetById(entityId) as IModel;
      if (project.Drawings2.Contains(entityId))
        return project.Drawings2.GetById(entityId) as IModel;
      return null;
    }

    private static Guid GetOwningEntityId(Renga.IProject project, int entityId)
    {
      if (project.BuildingInfo.Id == entityId)
        return Guid.Empty;
      if (project.Assemblies.Contains(entityId))
        return project.Assemblies.GetById(entityId).UniqueId;
      if (project.Drawings2.Contains(entityId))
        return project.Drawings2.GetById(entityId).UniqueId;
      return Guid.Empty;
    }

    private static Bcfier.Bcf.Bcf2.PerspectiveCamera CreateCameraParams(Renga.IView3DParams view3DParams)
    {
      var rengaCamera = view3DParams.Camera;
      var bcfViewCamera = new Bcfier.Bcf.Bcf2.PerspectiveCamera();
      
      // Position
      bcfViewCamera.CameraViewPoint.X = rengaCamera.Position.X / 1000;
      bcfViewCamera.CameraViewPoint.Y = rengaCamera.Position.Y / 1000;
      bcfViewCamera.CameraViewPoint.Z = rengaCamera.Position.Z / 1000;

      // Direction
      var cameraVector = new System.Windows.Media.Media3D.Vector3D(
        rengaCamera.FocusPoint.X - rengaCamera.Position.X,
        rengaCamera.FocusPoint.Y - rengaCamera.Position.Y,
        rengaCamera.FocusPoint.Z - rengaCamera.Position.Z);
      cameraVector.Normalize();

      bcfViewCamera.CameraDirection.X = cameraVector.X;
      bcfViewCamera.CameraDirection.Y = cameraVector.Y;
      bcfViewCamera.CameraDirection.Z = cameraVector.Z;

      // Up
      bcfViewCamera.CameraUpVector.X = rengaCamera.UpVector.X;
      bcfViewCamera.CameraUpVector.Y = rengaCamera.UpVector.Y;
      bcfViewCamera.CameraUpVector.Z = rengaCamera.UpVector.Z;

      bcfViewCamera.FieldOfView = rengaCamera.FovHorizontal * (180 / Math.PI);

      return bcfViewCamera;
    }

    //<summary>
    //Generate a VisualizationInfo of the current view
    //</summary>
    //<returns></returns>
    public static VisualizationInfo GenerateViewpoint(Renga.IApplication app)
    {
      try
      {
        var project = app.Project;
        if (app.Project == null)
          return null;

        var modelView = app.ActiveView as Renga.IModelView;
        if (modelView == null)
          return null;

        var entityId = modelView.RepresentedEntityId;
        var model = GetModel(project, entityId);
        if (model == null)
          return null;

        var owningEntityId = GetOwningEntityId(project, entityId);

        var objects = model.GetObjects();
        var hiddenComponents = new List<Component>();
        foreach (int id in objects.GetIds())
        {
          if (modelView.IsObjectVisible(id))
            continue;

          var objectUniqueId = objects.GetById(id).UniqueId;
          var ifcGuid = IfcGuid.ToIfcGuid(objectUniqueId);
          var hiddenComponent = new Component();
          hiddenComponent.IfcGuid = ifcGuid;
          var rengaEntityPath = new RengaEntityPath(objectUniqueId, owningEntityId);
          hiddenComponent.AuthoringToolId = rengaEntityPath.ToString();
          hiddenComponents.Add(hiddenComponent);
        }

        var result = new VisualizationInfo();
        var view3DParams = modelView as Renga.IView3DParams;
        if (view3DParams != null)
          result.PerspectiveCamera = CreateCameraParams(view3DParams);

        result.Components = new Components();
        result.Components.Visibility = new ComponentVisibility();
        // Here we believe that the default visibility should be "true"
        // since this is widely the most common scenario.
        // Although in some cases this may not be optimal.
        // TODO: assess the number of visible and invisible objects
        result.Components.Visibility.DefaultVisibilitySpecified = true;
        result.Components.Visibility.DefaultVisibility = true;
        result.Components.Visibility.Exceptions = hiddenComponents.ToArray();

        // Selection
        var selectedLocalIds = app.Selection.GetSelectedObjects();
        result.Components.Selection = new Component[selectedLocalIds.Length];

        for (int i = 0; i < selectedLocalIds.Length; i++)
        {
          var selectedLocalId = (int)selectedLocalIds.GetValue(i);
          var selectedGlobalId = model.GetUniqueIdFromId(selectedLocalId);
          var selectedComponent = new Component();
          selectedComponent.IfcGuid = IfcGuid.ToIfcGuid(selectedGlobalId);
          var rengaEntityPath = new RengaEntityPath(selectedGlobalId, owningEntityId);
          selectedComponent.AuthoringToolId = rengaEntityPath.ToString();
          result.Components.Selection[i] = selectedComponent;
        }

        return result;
      }
      catch (System.Exception ex)
      {
        Bcfier.Data.Utils.Utils.ShowErrorMessageBox("Create view point error.", ex);
      }

      return null;
    }
  }
}
