using Bcfier.Localization;
using Bcfier.RengaPlugin.Entry;
using Renga;
using System;
using System.Linq;
using Bcfier.Bcf.Model;

using BcfUtils = Bcfier.Data.Utils.Utils;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

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

    private static BcfComponentData CreateComponent(Guid uniqueId, Guid owningEntityId)
    {
      var rengaEntityPath = new RengaEntityPath(uniqueId, owningEntityId);
      return new BcfComponentData
      {
        IfcGuid = Bcfier.Data.Utils.IfcGuid.ToIfcGuid(uniqueId),
        AuthoringToolId = rengaEntityPath.ToString()
      };
    }

    private static BcfComponentVisibilityData BuildVisibility(IModel model, IModelView modelView, Guid owningEntityId)
    {
      var hiddenIds = modelView.GetHiddenObjects();
      if (hiddenIds == null)
        throw new System.Exception("IModelView.GetHiddenObjects returned null.");

      var result = new BcfComponentVisibilityData
      {
        DefaultVisibility = true
      };

      if (hiddenIds.Length == 0)
        return result;

      for (int i = 0; i < hiddenIds.Length; i++)
      {
        var hiddenLocalId = (int)hiddenIds.GetValue(i);
        var hiddenUniqueId = model.GetUniqueIdFromId(hiddenLocalId);
        result.Exceptions.Add(CreateComponent(hiddenUniqueId, owningEntityId));
      }

      // Default visibility stays true because most scenes show the majority of objects.
      return result;
    }

    private static BcfComponentData[] BuildSelectionComponents(IModel model, Array selectedLocalIds, Guid owningEntityId)
    {
      if (selectedLocalIds == null || selectedLocalIds.Length == 0)
        return Array.Empty<BcfComponentData>();

      var selection = new BcfComponentData[selectedLocalIds.Length];
      for (int i = 0; i < selectedLocalIds.Length; i++)
      {
        var selectedLocalId = (int)selectedLocalIds.GetValue(i);
        var selectedGlobalId = model.GetUniqueIdFromId(selectedLocalId);
        selection[i] = CreateComponent(selectedGlobalId, owningEntityId);
      }

      return selection;
    }

    private static BcfPerspectiveCameraData CreateCameraParams(Renga.IView3DParams view3DParams)
    {
      var rengaCamera = view3DParams.Camera;
      var bcfViewCamera = new BcfPerspectiveCameraData
      {
        CameraViewPoint = new BcfPointData(),
        CameraDirection = new BcfDirectionData(),
        CameraUpVector = new BcfDirectionData()
      };

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
    //Generate a BcfViewpointData of the current view
    //</summary>
    //<returns></returns>
    public static BcfViewpointData GenerateViewpoint(Renga.IApplication app)
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
        var visualization = new BcfViewpointData();

        if (modelView is Renga.IView3DParams view3DParams)
          visualization.PerspectiveCamera = CreateCameraParams(view3DParams);

        var components = new BcfComponentsData
        {
          Visibility = BuildVisibility(model, modelView, owningEntityId),
          Selection = BuildSelectionComponents(model, app.Selection?.GetSelectedObjects(), owningEntityId).ToList()
        };

        visualization.Components = components;

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