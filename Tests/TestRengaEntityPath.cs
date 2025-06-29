using NUnit.Framework;
using Bcfier.RengaPlugin.Entry;
using System;


// Generated tests
namespace Bcfier.RengaPlugin.Tests
{
  [TestFixture]
  public class RengaEntityPathTests
  {
    [Test]
    public void parse_valid_path_without_owning_entity_returns_correct_instance()
    {
      // given
      var valid_path = "Renga|123e4567-e89b-12d3-a456-426614174000";
      var expected_entity_id = new Guid("123e4567-e89b-12d3-a456-426614174000");

      // when
      var result = RengaEntityPath.Parse(valid_path);

      // then
      Assert.That(result.EntityId, Is.EqualTo(expected_entity_id));
      Assert.That(result.OwningEntityId, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void parse_valid_path_with_owning_entity_returns_correct_instance()
    {
      // given
      var valid_path = "Renga|123e4567-e89b-12d3-a456-426614174000|abcdef12-3456-7890-abcd-ef1234567890";
      var expected_owning_entity_id = new Guid("123e4567-e89b-12d3-a456-426614174000");
      var expected_entity_id = new Guid("abcdef12-3456-7890-abcd-ef1234567890");

      // when
      var result = RengaEntityPath.Parse(valid_path);

      // then
      Assert.That(result.OwningEntityId, Is.EqualTo(expected_owning_entity_id));
      Assert.That(result.EntityId, Is.EqualTo(expected_entity_id));
    }

    [Test]
    public void parse_empty_string_throws_argument_null_exception()
    {
      // given
      var empty_path = "";

      // then
      Assert.That(() => RengaEntityPath.Parse(empty_path),
          Throws.Exception.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void parse_null_string_throws_argument_null_exception()
    {
      // given
      string null_path = null;

      // then
      Assert.That(() => RengaEntityPath.Parse(null_path),
          Throws.Exception.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void parse_invalid_format_throws_format_exception()
    {
      // given
      var invalid_path = "Renga|123|456";

      // then
      Assert.That(() => RengaEntityPath.Parse(invalid_path),
          Throws.Exception.TypeOf<FormatException>());
    }

    [Test]
    public void try_parse_valid_path_returns_true_and_correct_instance()
    {
      // given
      var valid_path = "Renga|123e4567-e89b-12d3-a456-426614174000|abcdef12-3456-7890-abcd-ef1234567890";
      var result = default(RengaEntityPath);
      var expected_owning_id = new Guid("123e4567-e89b-12d3-a456-426614174000");
      var expected_entity_id = new Guid("abcdef12-3456-7890-abcd-ef1234567890");

      // when
      var success = RengaEntityPath.TryParse(valid_path, out result);

      // then
      Assert.That(success, Is.True);
      Assert.That(result, Is.Not.Null);
      Assert.That(result.OwningEntityId, Is.EqualTo(expected_owning_id));
      Assert.That(result.EntityId, Is.EqualTo(expected_entity_id));
    }

    [Test]
    public void to_string_empty_entity_returns_empty_string()
    {
      // given
      var entityPath = new RengaEntityPath(Guid.Empty);

      // when
      var result = entityPath.ToString();

      // then
      Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void to_string_without_owning_entity_returns_correct_format()
    {
      // given
      var entityId = Guid.NewGuid();
      var expectedString = $"Renga|{entityId.ToString("D")}";
      var entityPath = new RengaEntityPath(entityId);

      // when
      var result = entityPath.ToString();

      // then
      Assert.That(result, Is.EqualTo(expectedString));
    }

    [Test]
    public void to_string_with_owning_entity_returns_correct_format()
    {
      // given
      var owningEntityId = Guid.NewGuid();
      var entityId = Guid.NewGuid();
      var expectedString = $"Renga|{owningEntityId.ToString("D")}|{entityId.ToString("D")}";
      var entityPath = new RengaEntityPath(entityId, owningEntityId);

      // when
      var result = entityPath.ToString();

      // then
      Assert.That(result, Is.EqualTo(expectedString));
    }

    [Test]
    public void to_string_and_parse_roundtrip_without_owning_entity_returns_equal_instance()
    {
      // given
      var originalEntity = new RengaEntityPath(
          entityId: Guid.Parse("a3dce496-21c5-4a6b-9e5f-19c27b4d3456")
      );

      // when
      var serializedString = originalEntity.ToString();
      var parsedEntity = RengaEntityPath.Parse(serializedString);

      // then
      Assert.That(parsedEntity.EntityId, Is.EqualTo(originalEntity.EntityId));
      Assert.That(parsedEntity.OwningEntityId, Is.EqualTo(originalEntity.OwningEntityId));
    }

    [Test]
    public void to_string_and_parse_roundtrip_with_owning_entity_returns_equal_instance()
    {
      // given
      var originalEntity = new RengaEntityPath(
          entityId: Guid.Parse("d9f7e2a1-5c8b-4d98-bc6a-03f89d1a2345"),
          owningEntityId: Guid.Parse("b6c3a9d2-1e4f-4a7c-8d3b-0f5c98234567")
      );

      // when
      var serializedString = originalEntity.ToString();
      var parsedEntity = RengaEntityPath.Parse(serializedString);

      // then
      Assert.That(parsedEntity.EntityId, Is.EqualTo(originalEntity.EntityId));
      Assert.That(parsedEntity.OwningEntityId, Is.EqualTo(originalEntity.OwningEntityId));
    }

    [Test]
    public void to_string_and_try_parse_roundtrip_with_owning_entity_returns_equal_instance()
    {
      // given
      var originalEntity = new RengaEntityPath(
          entityId: Guid.NewGuid(),
          owningEntityId: Guid.NewGuid()
      );

      // when
      var serializedString = originalEntity.ToString();
      var parseResult = RengaEntityPath.TryParse(serializedString, out var parsedEntity);

      // then
      Assert.That(parseResult, Is.True);
      Assert.That(parsedEntity.EntityId, Is.EqualTo(originalEntity.EntityId));
      Assert.That(parsedEntity.OwningEntityId, Is.EqualTo(originalEntity.OwningEntityId));
    }
  }
}