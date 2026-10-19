using System;
using System.Collections.Generic;

namespace Bcfier.Bcf.Model
{
  /// <summary>
  /// Version-neutral domain model of a BCF "DocumentReference" (2.1) / "DocumentReference" (3.0) / "document_reference" (API).
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfDocumentReference
  {
    public string Guid { get; set; }
    public bool IsExternal { get; set; }
    public string ReferencedDocument { get; set; }
    public string Description { get; set; }
  }

  /// <summary>
  /// Version-neutral domain model of a BCF "RelatedTopic" (2.1) / "RelatedTopic" (3.0) / "related_topic" (API).
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfRelatedTopic
  {
    public string Guid { get; set; }
  }

  /// <summary>
  /// Version-neutral domain model of the topic payload (Topic element in 2.1/3.0 markup).
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfTopic
  {
    public string Guid { get; set; }
    public string Title { get; set; }
    public string Priority { get; set; }
    public string TopicType { get; set; }
    public string TopicStatus { get; set; }
    public int Index { get; set; }
    public string[] Labels { get; set; }
    public DateTime CreationDate { get; set; }
    public string CreationAuthor { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ModifiedAuthor { get; set; }
    public DateTime DueDate { get; set; }
    public string AssignedTo { get; set; }
    public string Stage { get; set; }
    public string Description { get; set; }
    public List<BcfDocumentReference> DocumentReferences { get; set; } = new List<BcfDocumentReference>();
    public List<BcfRelatedTopic> RelatedTopics { get; set; } = new List<BcfRelatedTopic>();
    public BcfBimSnippet BimSnippet { get; set; }

    public BcfTopic()
    {
      Guid = System.Guid.NewGuid().ToString();
    }

    public BcfTopic(DateTime creationDate)
      : this()
    {
      CreationDate = creationDate;
      ModifiedDate = creationDate;
    }
  }

  /// <summary>
  /// Version-neutral domain model of a BimSnippet.
  /// Pure POCO — no serialization attributes.
  /// </summary>
  public class BcfBimSnippet
  {
    public string SnippetType { get; set; }
    public bool IsExternal { get; set; }
    public string Reference { get; set; }
    public string ReferenceSchema { get; set; }
  }
}
