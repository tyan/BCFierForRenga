using Bcfier.Localization;
using Bcfier.RengaPlugin.Entry;
using Renga;
using System;

using BcfPerspectiveCamera = Bcfier.Bcf.Bcf2.PerspectiveCamera;
using BcfUtils = Bcfier.Data.Utils.Utils;
using Component = Bcfier.Bcf.Bcf2.Component;
using Components = Bcfier.Bcf.Bcf2.Components;
using ComponentVisibility = Bcfier.Bcf.Bcf2.ComponentVisibility;
using Vector3D = System.Windows.Media.Media3D.Vector3D;
using VisualizationInfo = Bcfier.Bcf.Bcf2.VisualizationInfo;

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

    private static Component CreateComponent(Guid uniqueId, Guid owningEntityId)
    {
      var rengaEntityPath = new RengaEntityPath(uniqueId, owningEntityId);
      return new Component
      {
        IfcGuid = Bcfier.Data.Utils.IfcGuid.ToIfcGuid(uniqueId),
        AuthoringToolId = rengaEntityPath.ToString()
      };
    }

    private static ComponentVisibility BuildVisibility(IModel model, IModelView modelView, Guid owningEntityId)
    {
      var hiddenIds = modelView.GetHiddenObjects();
      if (hiddenIds == null)
        throw new System.Exception("IModelView.GetHiddenObjects returned null.");

      if (hiddenIds.Length == 0)
      {
        return new ComponentVisibility
        {
          DefaultVisibilitySpecified = true,
          DefaultVisibility = true,
          Exceptions = Array.Empty<Component>()
        };
      }

      var hiddenComponents = new Component[hiddenIds.Length];
      for (int i = 0; i < hiddenIds.Length; i++)
      {
        var hiddenLocalId = (int)hiddenIds.GetValue(i);
        var hiddenUniqueId = model.GetUniqueIdFromId(hiddenLocalId);
        hiddenComponents[i] = CreateComponent(hiddenUniqueId, owningEntityId);
      }

      // Default visibility stays true because most scenes show the majority of objects.
      return new ComponentVisibility
      {
        DefaultVisibilitySpecified = true,
        DefaultVisibility = true,
        Exceptions = hiddenComponents
      };
    }

    private static Component[] BuildSelectionComponents(IModel model, Array selectedLocalIds, Guid owningEntityId)
    {
      if (selectedLocalIds == null || selectedLocalIds.Length == 0)
        return Array.Empty<Component>();

      var selection = new Component[selectedLocalIds.Length];
      for (int i = 0; i < selectedLocalIds.Length; i++)
      {
        var selectedLocalId = (int)selectedLocalIds.GetValue(i);
        var selectedGlobalId = model.GetUniqueIdFromId(selectedLocalId);
        selection[i] = CreateComponent(selectedGlobalId, owningEntityId);
      }

      return selection;
    }

    private static BcfPerspectiveCamera CreateCameraParams(Renga.IView3DParams view3DParams)
    {
      var rengaCamera = view3DParams.Camera;
      var bcfViewCamera = new BcfPerspectiveCamera();

      // Position
      bcfViewCamera.CameraViewPoint.X = rengaCamera.Position.X / 1000;
      bcfViewCamera.CameraViewPoint.Y = rengaCamera.Position.Y / 1000;
      bcfViewCamera.CameraViewPoint.Z = rengaCamera.Position.Z / 1000;

      // Direction
      var cameraVector = new Vector3D(
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
        if (app == null)
          return null;

        var project = app.Project;
        if (project == null)
          return null;

        if (!(app.ActiveView is Renga.IModelView modelView))
          return null;

        var entityId = modelView.RepresentedEntityId;
        var model = GetModel(project, entityId);
        if (model == null)
          return null;

        var owningEntityId = GetOwningEntityId(project, entityId);
        var visualization = new VisualizationInfo();

        if (modelView is Renga.IView3DParams view3DParams)
          visualization.PerspectiveCamera = CreateCameraParams(view3DParams);

        visualization.Components = new Components
        {
          Visibility = BuildVisibility(model, modelView, owningEntityId),
          Selection = BuildSelectionComponents(model, app.Selection?.GetSelectedObjects(), owningEntityId)
        };

        return visualization;
      }
      catch (System.Exception ex)
      {
        BcfUtils.ShowErrorMessageBox(LocValueGetter.Get("UnknownError"), ex);
      }

      return null;
    }
  }
}
