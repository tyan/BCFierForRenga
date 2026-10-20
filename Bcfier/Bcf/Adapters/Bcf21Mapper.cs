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
    public static BcfHeaderFile ToDomain(HeaderFile headerFile)
    {
      if (headerFile == null)
        return null;

      return new BcfHeaderFile
      {
        Filename = headerFile.Filename,
        Date = headerFile.Date,
        Reference = headerFile.Reference,
        IfcProject = headerFile.IfcProject,
        IfcSpatialStructureElement = headerFile.IfcSpatialStructureElement,
        IsExternal = headerFile.isExternal
      };
    }

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
      if (markup.Header != null)
        issue.HeaderFiles.AddRange(markup.Header.Select(ToDomain));
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

    public static BcfFile ToBcf2(BcfProject project)
    {
      if (project == null)
        return null;

      var bcf = new BcfFile
      {
        Id = project.Id,
        ProjectName = project.ProjectName,
        ProjectId = project.ProjectId,
        Filename = project.Filename,
        Fullname = project.Fullname,
        TempPath = project.TempPath
      };

      bcf.Issues.AddRange(project.Issues.Select(ToBcf2));
      return bcf;
    }

    public static Markup ToBcf2(BcfIssue issue)
    {
      var markup = new Markup
      {
        Topic = ToBcf2(issue.Topic),
        Comment = issue.Comments.Select(ToBcf2).ToList(),
        Viewpoints = issue.Viewpoints.Select(ToBcf2).ToList()
      };
      if (issue.HeaderFiles != null && issue.HeaderFiles.Count > 0)
        markup.Header = issue.HeaderFiles.Select(ToBcf2).ToList();
      return markup;
    }

    public static Topic ToBcf2(BcfTopic topic)
    {
      if (topic == null)
        return null;

      var result = new Topic
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
        BimSnippet = ToBcf2(topic.BimSnippet)
      };

      if (topic.DocumentReferences != null)
        result.DocumentReferences = topic.DocumentReferences.Select(ToBcf2).ToArray();

      if (topic.RelatedTopics != null)
        result.RelatedTopics = topic.RelatedTopics.Select(ToBcf2).ToArray();

      return result;
    }

    public static Comment ToBcf2(BcfComment comment)
    {
      if (comment == null)
        return null;

      return new Comment
      {
        Guid = comment.Guid.ToString(),
        Date = comment.Date,
        Author = comment.Author,
        Comment1 = comment.Comment,
        ModifiedDate = comment.ModifiedDate,
        ModifiedAuthor = comment.ModifiedAuthor,
        Viewpoint = comment.ViewpointGuid.HasValue
          ? new CommentViewpoint { Guid = comment.ViewpointGuid.Value.ToString() }
          : null
      };
    }

    public static ViewPoint ToBcf2(BcfViewpoint viewpoint)
    {
      if (viewpoint == null)
        return null;

      return new ViewPoint
      {
        Guid = viewpoint.Guid.ToString(),
        Viewpoint = viewpoint.ViewpointFileName,
        Snapshot = viewpoint.SnapshotFileName,
        VisInfo = ToBcf2(viewpoint.Data)
      };
    }

    public static VisualizationInfo ToBcf2(BcfViewpointData data)
    {
      if (data == null)
        return null;

      var result = new VisualizationInfo
      {
        Guid = data.Guid,
        Components = ToBcf2(data.Components),
        OrthogonalCamera = ToBcf2(data.OrthogonalCamera),
        PerspectiveCamera = ToBcf2(data.PerspectiveCamera),
        SheetCamera = ToBcf2(data.SheetCamera)
      };

      if (data.Lines != null && data.Lines.Count > 0)
        result.Lines = data.Lines.Select(ToBcf2).ToArray();

      if (data.ClippingPlanes != null && data.ClippingPlanes.Count > 0)
        result.ClippingPlanes = data.ClippingPlanes.Select(ToBcf2).ToArray();

      if (data.Bitmaps != null && data.Bitmaps.Count > 0)
        result.Bitmap = data.Bitmaps.Select(ToBcf2).ToArray();

      return result;
    }

    public static Components ToBcf2(BcfComponentsData components)
    {
      if (components == null)
        return null;

      var result = new Components
      {
        ViewSetupHints = ToBcf2(components.ViewSetupHints),
        Visibility = ToBcf2(components.Visibility)
      };

      if (components.Selection != null && components.Selection.Count > 0)
        result.Selection = components.Selection.Select(ToBcf2).ToArray();

      if (components.Coloring != null && components.Coloring.Count > 0)
        result.Coloring = components.Coloring.Select(ToBcf2).ToArray();

      return result;
    }

    public static ViewSetupHints ToBcf2(BcfViewSetupHintsData hints)
    {
      if (hints == null)
        return null;

      return new ViewSetupHints
      {
        SpacesVisible = hints.SpacesVisible,
        SpaceBoundariesVisible = hints.SpaceBoundariesVisible,
        OpeningsVisible = hints.OpeningsVisible
      };
    }

    public static Component ToBcf2(BcfComponentData component)
    {
      if (component == null)
        return null;

      return new Component
      {
        IfcGuid = component.IfcGuid,
        OriginatingSystem = component.OriginatingSystem,
        AuthoringToolId = component.AuthoringToolId
      };
    }

    public static ComponentVisibility ToBcf2(BcfComponentVisibilityData visibility)
    {
      if (visibility == null)
        return null;

      var result = new ComponentVisibility
      {
        DefaultVisibility = visibility.DefaultVisibility
      };

      if (visibility.Exceptions != null && visibility.Exceptions.Count > 0)
        result.Exceptions = visibility.Exceptions.Select(ToBcf2).ToArray();

      return result;
    }

    public static ComponentColoringColor ToBcf2(BcfComponentColoringData coloring)
    {
      if (coloring == null)
        return null;

      var result = new ComponentColoringColor
      {
        Color = coloring.Color
      };

      if (coloring.Components != null && coloring.Components.Count > 0)
        result.Component = coloring.Components.Select(ToBcf2).ToArray();

      return result;
    }

    public static Line ToBcf2(BcfLineData line)
    {
      if (line == null)
        return null;

      return new Line
      {
        StartPoint = ToBcf2(line.StartPoint),
        EndPoint = ToBcf2(line.EndPoint)
      };
    }

    public static ClippingPlane ToBcf2(BcfClippingPlaneData clippingPlane)
    {
      if (clippingPlane == null)
        return null;

      return new ClippingPlane
      {
        Location = ToBcf2(clippingPlane.Location),
        Direction = ToBcf2(clippingPlane.Direction)
      };
    }

    public static VisualizationInfoBitmap ToBcf2(BcfBitmapData bitmap)
    {
      if (bitmap == null)
        return null;

      return new VisualizationInfoBitmap
      {
        Bitmap = ParseBitmapFormat(bitmap.BitmapFormat),
        Reference = bitmap.Reference,
        Location = ToBcf2(bitmap.Location),
        Normal = ToBcf2(bitmap.Normal),
        Up = ToBcf2(bitmap.Up),
        Height = bitmap.Height
      };
    }

    public static OrthogonalCamera ToBcf2(BcfOrthogonalCameraData camera)
    {
      if (camera == null)
        return null;

      return new OrthogonalCamera
      {
        CameraViewPoint = ToBcf2(camera.CameraViewPoint),
        CameraDirection = ToBcf2(camera.CameraDirection),
        CameraUpVector = ToBcf2(camera.CameraUpVector),
        ViewToWorldScale = camera.ViewToWorldScale
      };
    }

    public static PerspectiveCamera ToBcf2(BcfPerspectiveCameraData camera)
    {
      if (camera == null)
        return null;

      return new PerspectiveCamera
      {
        CameraViewPoint = ToBcf2(camera.CameraViewPoint),
        CameraDirection = ToBcf2(camera.CameraDirection),
        CameraUpVector = ToBcf2(camera.CameraUpVector),
        FieldOfView = camera.FieldOfView
      };
    }

    public static SheetCamera ToBcf2(BcfSheetCameraData camera)
    {
      if (camera == null)
        return null;

      return new SheetCamera
      {
        TopLeft = ToBcf2(camera.TopLeft),
        BottomRight = ToBcf2(camera.BottomRight),
        SheetID = camera.SheetID,
        SheetName = camera.SheetName
      };
    }

    public static Point ToBcf2(BcfPointData point)
    {
      if (point == null)
        return null;

      return new Point
      {
        X = point.X,
        Y = point.Y,
        Z = point.Z
      };
    }

    public static Direction ToBcf2(BcfDirectionData direction)
    {
      if (direction == null)
        return null;

      return new Direction
      {
        X = direction.X,
        Y = direction.Y,
        Z = direction.Z
      };
    }

    public static TopicDocumentReferences ToBcf2(BcfDocumentReference documentReference)
    {
      if (documentReference == null)
        return null;

      return new TopicDocumentReferences
      {
        Guid = documentReference.Guid,
        isExternal = documentReference.IsExternal,
        ReferencedDocument = documentReference.ReferencedDocument,
        Description = documentReference.Description
      };
    }

    public static TopicRelatedTopics ToBcf2(BcfRelatedTopic relatedTopic)
    {
      if (relatedTopic == null)
        return null;

      return new TopicRelatedTopics
      {
        Guid = relatedTopic.Guid
      };
    }

    public static BimSnippet ToBcf2(BcfBimSnippet bimSnippet)
    {
      if (bimSnippet == null)
        return null;

      return new BimSnippet
      {
        SnippetType = bimSnippet.SnippetType,
        isExternal = bimSnippet.IsExternal,
        Reference = bimSnippet.Reference,
        ReferenceSchema = bimSnippet.ReferenceSchema
      };
    }

    public static HeaderFile ToBcf2(BcfHeaderFile headerFile)
    {
      if (headerFile == null)
        return null;

      return new HeaderFile
      {
        Filename = headerFile.Filename,
        Date = headerFile.Date,
        DateSpecified = headerFile.Date != default(DateTime),
        Reference = headerFile.Reference,
        IfcProject = headerFile.IfcProject,
        IfcSpatialStructureElement = headerFile.IfcSpatialStructureElement,
        isExternal = headerFile.IsExternal
      };
    }

    private static BitmapFormat ParseBitmapFormat(string format)
    {
      if (Enum.TryParse(format, true, out BitmapFormat parsed))
        return parsed;
      return BitmapFormat.PNG;
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