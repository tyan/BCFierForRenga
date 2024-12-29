using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Bcfier.Bcf.Bcf2;
using Bcfier.Data.Utils;
using Bcfier.Localization;

namespace Bcfier.RengaPlugin.Entry
{
  public class ExtEvntOpenView
  {
    public VisualizationInfo v;

    public void Execute(Renga.IApplication app)
    {
      try
      {
        var modelView = app.ActiveView as Renga.IModelView;
        if (modelView == null) 
          return;

        var camera = (modelView as Renga.IView3DParams)?.Camera;
        if (camera == null)
          return;

        if (v.PerspectiveCamera == null)
          return;

        var rengaCameraPos = new Renga.FloatPoint3D();
        var bcfCameraPos = v.PerspectiveCamera.CameraViewPoint;
        rengaCameraPos.X = (float)bcfCameraPos.X * 1000;
        rengaCameraPos.Y = (float)bcfCameraPos.Y * 1000;
        rengaCameraPos.Z = (float)bcfCameraPos.Z * 1000;

        var rengaFocusPoint = new Renga.FloatPoint3D();
        var bcfCameraDirection = v.PerspectiveCamera.CameraDirection;
        rengaFocusPoint.X = rengaCameraPos.X + (float)bcfCameraDirection.X * 1000;
        rengaFocusPoint.Y = rengaCameraPos.Y + (float)bcfCameraDirection.Y * 1000;
        rengaFocusPoint.Z = rengaCameraPos.Z + (float)bcfCameraDirection.Z * 1000;

        var rengaUpVector = new Renga.FloatVector3D();
        var bcfUpVector = v.PerspectiveCamera.CameraUpVector;
        rengaUpVector.X = (float)bcfUpVector.X;
        rengaUpVector.Y = (float)bcfUpVector.Y;
        rengaUpVector.Z = (float)bcfUpVector.Z;
        camera.LookAt(rengaFocusPoint, rengaCameraPos, rengaUpVector);

        var buildingModel = app.Project?.Model;
        if (buildingModel == null)
          return;

        // Visibility
        var allObjects = buildingModel.GetObjects();
        var allObjectIds = buildingModel.GetObjects().GetIds();
        var exceptionIds = new List<int>();
        var otherIds = new List<int>();
        var defaultVisibility = false;

        if (v.Components.Visibility == null)
        {
          otherIds = allObjectIds.OfType<int>().ToList();
        }
        else
        {
          if (v.Components.Visibility.DefaultVisibilitySpecified)
            defaultVisibility = v.Components.Visibility.DefaultVisibility;

          Func<string, bool> isExceptionObject = (string ifcGuid) =>
          {
            return Array.Find(v.Components.Visibility.Exceptions, exception => exception.IfcGuid == ifcGuid) != null;
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

        foreach (var selectedComponent in v.Components.Selection)
        {
          var selectedObjectGlobalId = IfcGuid.FromIfcGUID(selectedComponent.IfcGuid);
          var selectedObjectLocalId = buildingModel.GetIdFromUniqueId(selectedObjectGlobalId);
          selectedObjectsLocalIds.Add(selectedObjectLocalId);
        }
        app.Selection.SetSelectedObjects(selectedObjectsLocalIds.ToArray());
      }
      catch (Exception ex)
      {
        Utils.ShowErrorMessageBox(LocValueGetter.Get("UnknownError"), ex);
      }
    }
  }
}