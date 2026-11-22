using System;
using System.Linq;
using Bcfier.Bcf.Bcf21;
using Bcfier.Bcf.Model;

namespace Bcfier.Bcf.Adapters
{
  /// <summary>
  /// Maps the schema-valid BCF 2.1 DTOs (<c>Bcfier.Bcf.Bcf21</c>, generated from the official
  /// release_2_1 XSDs) to the version-neutral domain model (<c>Bcfier.Bcf.Model</c>) and back.
  /// Step 3.2 of the domain adapter plan.
  /// </summary>
  internal static class Bcf21ValidMapper
  {
    public static BcfProject ToDomain(Bcf21Container container)
    {
      if (container == null)
        return null;

      var project = new BcfProject
      {
        Id = container.Id,
        ProjectName = container.ProjectName,
        ProjectId = container.ProjectId,
        Filename = container.Filename,
        Fullname = container.Fullname,
        TempPath = container.TempPath,
        SourceFormat = ProjectFormat.Bcf21
      };

      project.Issues.AddRange(container.Issues.Select(ToDomain));
      return project;
    }

    public static BcfIssue ToDomain(Markup markup)
    {
      if (markup == null)
        return null;

      // Topic is required by the BCF 2.1 schema (minOccurs="1"); the loader rejects
      // markup.bcf without it, so here it is a domain invariant.
      var issue = new BcfIssue(ToDomain(markup.Topic))
      {
        Index = markup.Topic.Index,
        HasBeenSaved = true
      };
      if (markup.Comment != null)
        issue.Comments.AddRange(markup.Comment.Select(ToDomain));
      if (markup.Viewpoints != null)
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

      if (topic.DocumentReference != null)
        result.DocumentReferences.AddRange(topic.DocumentReference.Select(ToDomain));

      if (topic.RelatedTopic != null)
        result.RelatedTopics.AddRange(topic.RelatedTopic.Select(ToDomain));

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

    public static BcfDocumentReference ToDomain(TopicDocumentReference documentReference)
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

    public static BcfRelatedTopic ToDomain(TopicRelatedTopic relatedTopic)
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

    public static Bcf21Container ToContainer(BcfProject project)
    {
      if (project == null)
        return null;

      var container = new Bcf21Container
      {
        Id = project.Id,
        ProjectName = project.ProjectName,
        ProjectId = project.ProjectId,
        Filename = project.Filename,
        Fullname = project.Fullname,
        TempPath = project.TempPath
      };

      container.Issues.AddRange(project.Issues.Select(ToValid21));
      return container;
    }

    public static Markup ToValid21(BcfIssue issue)
    {
      var markup = new Markup
      {
        Topic = ToValid21(issue.Topic),
        Comment = issue.Comments.Select(ToValid21).ToArray(),
        Viewpoints = issue.Viewpoints.Select(ToValid21).ToArray()
      };
      if (issue.HeaderFiles != null && issue.HeaderFiles.Count > 0)
        markup.Header = issue.HeaderFiles.Select(ToValid21).ToArray();
      return markup;
    }

    public static Topic ToValid21(BcfTopic topic)
    {
      if (topic == null)
        return null;

      var result = new Topic
      {
        Guid = topic.Guid,
        // Title is required (minOccurs="1" in markup.xsd); keep empty topics schema-valid.
        Title = FirstNonEmpty(topic.Title, "(no title)"),
        Priority = topic.Priority,
        TopicType = topic.TopicType,
        TopicStatus = topic.TopicStatus,
        Index = topic.Index,
        IndexSpecified = true,
        Labels = topic.Labels,
        CreationDate = topic.CreationDate,
        // CreationAuthor is required (minOccurs="1" in markup.xsd); fall back to the
        // modifying author, then to a placeholder so the file stays schema-valid.
        CreationAuthor = FirstNonEmpty(topic.CreationAuthor, topic.ModifiedAuthor, "unknown"),
        ModifiedDate = topic.ModifiedDate,
        ModifiedDateSpecified = topic.ModifiedDate != default(DateTime),
        ModifiedAuthor = topic.ModifiedAuthor,
        DueDate = topic.DueDate,
        DueDateSpecified = topic.DueDate != default(DateTime),
        AssignedTo = topic.AssignedTo,
        Stage = topic.Stage,
        Description = topic.Description,
        BimSnippet = ToValid21(topic.BimSnippet)
      };

      if (topic.DocumentReferences != null && topic.DocumentReferences.Count > 0)
        result.DocumentReference = topic.DocumentReferences.Select(ToValid21).ToArray();

      if (topic.RelatedTopics != null && topic.RelatedTopics.Count > 0)
        result.RelatedTopic = topic.RelatedTopics.Select(ToValid21).ToArray();

      return result;
    }

    public static Comment ToValid21(BcfComment comment)
    {
      if (comment == null)
        return null;

      return new Comment
      {
        Guid = comment.Guid.ToString(),
        Date = comment.Date,
        // Date/Author/Comment are required (minOccurs="1" in markup.xsd); keep
        // degenerate comments schema-valid with placeholders.
        Author = FirstNonEmpty(comment.Author, "unknown"),
        // empty string is a valid placeholder for the required Comment element
        Comment1 = comment.Comment ?? "",
        ModifiedDate = comment.ModifiedDate,
        ModifiedDateSpecified = comment.ModifiedDate != default(DateTime),
        ModifiedAuthor = comment.ModifiedAuthor,
        Viewpoint = comment.ViewpointGuid.HasValue
          ? new CommentViewpoint { Guid = comment.ViewpointGuid.Value.ToString() }
          : null
      };
    }

    public static ViewPoint ToValid21(BcfViewpoint viewpoint)
    {
      if (viewpoint == null)
        return null;

      return new ViewPoint
      {
        Guid = viewpoint.Guid.ToString(),
        Viewpoint = viewpoint.ViewpointFileName,
        Snapshot = viewpoint.SnapshotFileName,
        VisInfo = ToValid21(viewpoint.Data)
      };
    }

    public static VisualizationInfo ToValid21(BcfViewpointData data)
    {
      if (data == null)
        return null;

      var result = new VisualizationInfo
      {
        // Guid is required on VisualizationInfo (visinfo.xsd); legacy files may carry
        // viewpoint payloads without it, so fall back to a fresh guid.
        Guid = FirstNonEmpty(data.Guid) ?? Guid.NewGuid().ToString(),
        Components = ToValid21(data.Components),
        OrthogonalCamera = ToValid21(data.OrthogonalCamera),
        PerspectiveCamera = ToValid21(data.PerspectiveCamera),
        // BCFier extension (declared via ExtensionSchema in project.bcfp).
        SheetCamera = ToValid21(data.SheetCamera)
      };

      if (data.Lines != null && data.Lines.Count > 0)
        result.Lines = data.Lines.Select(ToValid21).ToArray();

      if (data.ClippingPlanes != null && data.ClippingPlanes.Count > 0)
        result.ClippingPlanes = data.ClippingPlanes.Select(ToValid21).ToArray();

      if (data.Bitmaps != null && data.Bitmaps.Count > 0)
        result.Bitmap = data.Bitmaps.Select(ToValid21).ToArray();

      return result;
    }

    public static Components ToValid21(BcfComponentsData components)
    {
      if (components == null)
        return null;

      var result = new Components
      {
        ViewSetupHints = ToValid21(components.ViewSetupHints),
        Visibility = ToValid21(components.Visibility)
      };

      if (components.Selection != null && components.Selection.Count > 0)
        result.Selection = components.Selection.Select(ToValid21).ToArray();

      if (components.Coloring != null && components.Coloring.Count > 0)
        result.Coloring = components.Coloring.Select(ToValid21).ToArray();

      return result;
    }

    public static ViewSetupHints ToValid21(BcfViewSetupHintsData hints)
    {
      if (hints == null)
        return null;

      var result = new ViewSetupHints
      {
        SpacesVisible = hints.SpacesVisible,
        SpacesVisibleSpecified = true,
        SpaceBoundariesVisible = hints.SpaceBoundariesVisible,
        SpaceBoundariesVisibleSpecified = true,
        OpeningsVisible = hints.OpeningsVisible,
        OpeningsVisibleSpecified = true
      };
      return result;
    }

    public static Component ToValid21(BcfComponentData component)
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

    public static ComponentVisibility ToValid21(BcfComponentVisibilityData visibility)
    {
      if (visibility == null)
        return null;

      var result = new ComponentVisibility
      {
        DefaultVisibility = visibility.DefaultVisibility,
        DefaultVisibilitySpecified = true
      };

      if (visibility.Exceptions != null && visibility.Exceptions.Count > 0)
        result.Exceptions = visibility.Exceptions.Select(ToValid21).ToArray();

      return result;
    }

    public static ComponentColoringColor ToValid21(BcfComponentColoringData coloring)
    {
      if (coloring == null)
        return null;

      var result = new ComponentColoringColor
      {
        Color = coloring.Color
      };

      if (coloring.Components != null && coloring.Components.Count > 0)
        result.Component = coloring.Components.Select(ToValid21).ToArray();

      return result;
    }

    public static Line ToValid21(BcfLineData line)
    {
      if (line == null)
        return null;

      return new Line
      {
        StartPoint = ToValid21(line.StartPoint),
        EndPoint = ToValid21(line.EndPoint)
      };
    }

    public static ClippingPlane ToValid21(BcfClippingPlaneData clippingPlane)
    {
      if (clippingPlane == null)
        return null;

      return new ClippingPlane
      {
        Location = ToValid21(clippingPlane.Location),
        Direction = ToValid21(clippingPlane.Direction)
      };
    }

    public static VisualizationInfoBitmap ToValid21(BcfBitmapData bitmap)
    {
      if (bitmap == null)
        return null;

      return new VisualizationInfoBitmap
      {
        Bitmap = ParseBitmapFormat(bitmap.BitmapFormat),
        Reference = bitmap.Reference,
        Location = ToValid21(bitmap.Location),
        Normal = ToValid21(bitmap.Normal),
        Up = ToValid21(bitmap.Up),
        Height = bitmap.Height
      };
    }

    public static OrthogonalCamera ToValid21(BcfOrthogonalCameraData camera)
    {
      if (camera == null)
        return null;

      return new OrthogonalCamera
      {
        CameraViewPoint = ToValid21(camera.CameraViewPoint),
        CameraDirection = ToValid21(camera.CameraDirection),
        CameraUpVector = ToValid21(camera.CameraUpVector),
        ViewToWorldScale = camera.ViewToWorldScale
      };
    }

    public static PerspectiveCamera ToValid21(BcfPerspectiveCameraData camera)
    {
      if (camera == null)
        return null;

      return new PerspectiveCamera
      {
        CameraViewPoint = ToValid21(camera.CameraViewPoint),
        CameraDirection = ToValid21(camera.CameraDirection),
        CameraUpVector = ToValid21(camera.CameraUpVector),
        FieldOfView = camera.FieldOfView
      };
    }

    public static SheetCamera ToValid21(BcfSheetCameraData camera)
    {
      if (camera == null)
        return null;

      return new SheetCamera
      {
        TopLeft = ToValid21(camera.TopLeft),
        BottomRight = ToValid21(camera.BottomRight),
        SheetID = camera.SheetID,
        SheetName = camera.SheetName
      };
    }

    public static Point ToValid21(BcfPointData point)
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

    public static Direction ToValid21(BcfDirectionData direction)
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

    public static TopicDocumentReference ToValid21(BcfDocumentReference documentReference)
    {
      if (documentReference == null)
        return null;

      return new TopicDocumentReference
      {
        Guid = documentReference.Guid,
        isExternal = documentReference.IsExternal,
        ReferencedDocument = documentReference.ReferencedDocument,
        Description = documentReference.Description
      };
    }

    public static TopicRelatedTopic ToValid21(BcfRelatedTopic relatedTopic)
    {
      if (relatedTopic == null)
        return null;

      return new TopicRelatedTopic
      {
        Guid = relatedTopic.Guid
      };
    }

    public static BimSnippet ToValid21(BcfBimSnippet bimSnippet)
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

    public static HeaderFile ToValid21(BcfHeaderFile headerFile)
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

    private static string FirstNonEmpty(params string[] values)
    {
      return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
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
