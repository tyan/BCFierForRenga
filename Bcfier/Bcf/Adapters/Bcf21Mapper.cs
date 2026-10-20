using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bcfier.Bcf.Bcf2;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Maps the legacy <c>Bcf2.*</c> model (as loaded by <see cref="BcfSerializer"/>) to the
  /// version-neutral domain model (<c>Bcfier.Bcf.Model</c>). Step 2a of the domain adapter plan.
  /// </summary>
  internal static class Bcf21Mapper
  {
    public static BcfProject ToDomain(BcfFile bcf)
    {
      if (bcf == null)
        return null;

      var project = new BcfProject
      {
        Id = bcf.Id,
        ProjectName = bcf.ProjectName,
        ProjectId = bcf.ProjectId,
        Filename = bcf.Filename,
        Fullname = bcf.Fullname,
        TempPath = bcf.TempPath
      };

      project.Issues.AddRange(bcf.Issues.Select(ToDomain));
      return project;
    }

    public static BcfIssue ToDomain(Markup markup)
    {
      var issue = new BcfIssue
      {
        Topic = ToDomain(markup.Topic),
        Index = markup.Topic.Index,
        HasBeenSaved = false
      };
      issue.Comments.AddRange(markup.Comment.Select(ToDomain));
      issue.Viewpoints.AddRange(markup.Viewpoints.Select(ToDomain));
      return issue;
    }

    public static BcfTopic ToDomain(Topic topic)
    {
      if (topic == null)
        return null;

      var result = new BcfTopic
      {
        Guid = topic.Guid,
        Title = topic.Title,
        Priority = topic.Priority,
        TopicType = topic.TopicType,
        TopicStatus = topic.TopicStatus,
        Index = topic.Index,
        Labels = topic.Labels,
        CreationDate = topic.CreationDate,
        CreationAuthor = topic.CreationAuthor,
        ModifiedDate = topic.ModifiedDate,
        ModifiedAuthor = topic.ModifiedAuthor,
        DueDate = topic.DueDate,
        AssignedTo = topic.AssignedTo,
        Stage = topic.Stage,
        Description = topic.Description,
        BimSnippet = ToDomain(topic.BimSnippet)
      };

      if (topic.DocumentReferences != null)
        result.DocumentReferences.AddRange(topic.DocumentReferences.Select(ToDomain));

      if (topic.RelatedTopics != null)
        result.RelatedTopics.AddRange(topic.RelatedTopics.Select(ToDomain));

      return result;
    }

    public static BcfComment ToDomain(Comment comment)
    {
      if (comment == null)
        return null;

      return new BcfComment
      {
        Guid = ParseGuidOrEmpty(comment.Guid),
        Date = comment.Date,
        Author = comment.Author,
        Comment = comment.Comment1,
        ModifiedDate = comment.ModifiedDate,
        ModifiedAuthor = comment.ModifiedAuthor,
        ViewpointGuid = comment.Viewpoint != null ? ParseGuidOrNull(comment.Viewpoint.Guid) : null
      };
    }

    public static BcfViewpoint ToDomain(ViewPoint viewpoint)
    {
      if (viewpoint == null)
        return null;

      return new BcfViewpoint
      {
        Guid = ParseGuidOrEmpty(viewpoint.Guid),
        ViewpointFileName = viewpoint.Viewpoint,
        SnapshotFileName = viewpoint.Snapshot,
        Data = ToDomain(viewpoint.VisInfo)
      };
    }

    public static BcfViewpointData ToDomain(VisualizationInfo visInfo)
    {
      if (visInfo == null)
        return null;

      var result = new BcfViewpointData
      {
        Guid = visInfo.Guid,
        Components = ToDomain(visInfo.Components),
        OrthogonalCamera = ToDomain(visInfo.OrthogonalCamera),
        PerspectiveCamera = ToDomain(visInfo.PerspectiveCamera),
        SheetCamera = ToDomain(visInfo.SheetCamera)
      };

      if (visInfo.Lines != null)
        result.Lines.AddRange(visInfo.Lines.Select(ToDomain));

      if (visInfo.ClippingPlanes != null)
        result.ClippingPlanes.AddRange(visInfo.ClippingPlanes.Select(ToDomain));

      if (visInfo.Bitmap != null)
        result.Bitmaps.AddRange(visInfo.Bitmap.Select(ToDomain));

      return result;
    }

    public static BcfComponentsData ToDomain(Components components)
    {
      if (components == null)
        return null;

      var result = new BcfComponentsData
      {
        ViewSetupHints = ToDomain(components.ViewSetupHints),
        Visibility = ToDomain(components.Visibility)
      };

      if (components.Selection != null)
        result.Selection.AddRange(components.Selection.Select(ToDomain));

      if (components.Coloring != null)
        result.Coloring.AddRange(components.Coloring.Select(ToDomain));

      return result;
    }

    public static BcfViewSetupHintsData ToDomain(ViewSetupHints hints)
    {
      if (hints == null)
        return null;

      return new BcfViewSetupHintsData
      {
        SpacesVisible = hints.SpacesVisible,
        SpaceBoundariesVisible = hints.SpaceBoundariesVisible,
        OpeningsVisible = hints.OpeningsVisible
      };
    }

    public static BcfComponentData ToDomain(Component component)
    {
      if (component == null)
        return null;

      return new BcfComponentData
      {
        IfcGuid = component.IfcGuid,
        OriginatingSystem = component.OriginatingSystem,
        AuthoringToolId = component.AuthoringToolId
      };
    }

    public static BcfComponentVisibilityData ToDomain(ComponentVisibility visibility)
    {
      if (visibility == null)
        return null;

      var result = new BcfComponentVisibilityData
      {
        DefaultVisibility = visibility.DefaultVisibility
      };

      if (visibility.Exceptions != null)
        result.Exceptions.AddRange(visibility.Exceptions.Select(ToDomain));

      return result;
    }

    public static BcfComponentColoringData ToDomain(ComponentColoringColor coloring)
    {
      if (coloring == null)
        return null;

      var result = new BcfComponentColoringData
      {
        Color = coloring.Color
      };

      if (coloring.Component != null)
        result.Components.AddRange(coloring.Component.Select(ToDomain));

      return result;
    }

    public static BcfLineData ToDomain(Line line)
    {
      if (line == null)
        return null;

      return new BcfLineData
      {
        StartPoint = ToDomain(line.StartPoint),
        EndPoint = ToDomain(line.EndPoint)
      };
    }

    public static BcfClippingPlaneData ToDomain(ClippingPlane clippingPlane)
    {
      if (clippingPlane == null)
        return null;

      return new BcfClippingPlaneData
      {
        Location = ToDomain(clippingPlane.Location),
        Direction = ToDomain(clippingPlane.Direction)
      };
    }

    public static BcfBitmapData ToDomain(VisualizationInfoBitmap bitmap)
    {
      if (bitmap == null)
        return null;

      return new BcfBitmapData
      {
        BitmapFormat = bitmap.Bitmap.ToString(),
        Reference = bitmap.Reference,
        Location = ToDomain(bitmap.Location),
        Normal = ToDomain(bitmap.Normal),
        Up = ToDomain(bitmap.Up),
        Height = bitmap.Height
      };
    }

    public static BcfOrthogonalCameraData ToDomain(OrthogonalCamera camera)
    {
      if (camera == null)
        return null;

      return new BcfOrthogonalCameraData
      {
        CameraViewPoint = ToDomain(camera.CameraViewPoint),
        CameraDirection = ToDomain(camera.CameraDirection),
        CameraUpVector = ToDomain(camera.CameraUpVector),
        ViewToWorldScale = camera.ViewToWorldScale
      };
    }

    public static BcfPerspectiveCameraData ToDomain(PerspectiveCamera camera)
    {
      if (camera == null)
        return null;

      return new BcfPerspectiveCameraData
      {
        CameraViewPoint = ToDomain(camera.CameraViewPoint),
        CameraDirection = ToDomain(camera.CameraDirection),
        CameraUpVector = ToDomain(camera.CameraUpVector),
        FieldOfView = camera.FieldOfView
      };
    }

    public static BcfSheetCameraData ToDomain(SheetCamera camera)
    {
      if (camera == null)
        return null;

      return new BcfSheetCameraData
      {
        TopLeft = ToDomain(camera.TopLeft),
        BottomRight = ToDomain(camera.BottomRight),
        SheetID = camera.SheetID,
        SheetName = camera.SheetName
      };
    }

    public static BcfPointData ToDomain(Point point)
    {
      if (point == null)
        return null;

      return new BcfPointData
      {
        X = point.X,
        Y = point.Y,
        Z = point.Z
      };
    }

    public static BcfDirectionData ToDomain(Direction direction)
    {
      if (direction == null)
        return null;

      return new BcfDirectionData
      {
        X = direction.X,
        Y = direction.Y,
        Z = direction.Z
      };
    }

    public static BcfDocumentReference ToDomain(TopicDocumentReferences documentReference)
    {
      if (documentReference == null)
        return null;

      return new BcfDocumentReference
      {
        Guid = documentReference.Guid,
        IsExternal = documentReference.isExternal,
        ReferencedDocument = documentReference.ReferencedDocument,
        Description = documentReference.Description
      };
    }

    public static BcfRelatedTopic ToDomain(TopicRelatedTopics relatedTopic)
    {
      if (relatedTopic == null)
        return null;

      return new BcfRelatedTopic
      {
        Guid = relatedTopic.Guid
      };
    }

    public static BcfBimSnippet ToDomain(BimSnippet bimSnippet)
    {
      if (bimSnippet == null)
        return null;

      return new BcfBimSnippet
      {
        SnippetType = bimSnippet.SnippetType,
        IsExternal = bimSnippet.isExternal,
        Reference = bimSnippet.Reference,
        ReferenceSchema = bimSnippet.ReferenceSchema
      };
    }

    private static Guid ParseGuidOrEmpty(string value)
    {
      return Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    }

    private static Guid? ParseGuidOrNull(string value)
    {
      return Guid.TryParse(value, out var parsed) ? parsed : (Guid?)null;
    }
  }
}