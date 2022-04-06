using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PrimitiveCodebaseElements.Primitive.db;
using PrimitiveCodebaseElements.Primitive.db.merger;
using Xunit;

namespace PrimitiveCodebaseElements.Tests.db.merger;

public class TableSetMergerTest
{
    [Fact]
    public void MergeTest()
    {
        TableSet a = new(
            directories: new List<DbDirectory> { new DbDirectory(1, "dir", 0, 0) },
            files: new List<DbFile>
            {
                new DbFile(1, 1, "x", "dir/x", "", 1),
                new DbFile(2, 1, "common", "dir/common", "", 1),
            },
            classes: new List<DbClass>
            {
                new DbClass(1, 4, 1, "classInX", 1, 1, 1),
                new DbClass(2, 4, 2, "classInCommon", 1, 1, 1),
            }
        );

        TableSet b = new(
            directories: new List<DbDirectory> { new DbDirectory(1, "dir", 0, 0) },
            files: new List<DbFile>
            {
                new DbFile(1, 1, "y", "dir/y", "", 1),
                new DbFile(2, 1, "common", "dir/common", "", 1),
            },
            classes: new List<DbClass>
            {
                new DbClass(1, 4, 1, "classInY", 1, 1, 1),
                new DbClass(2, 4, 2, "classInCommon2", 1, 1, 1),
            }
        );

        //WHEN
        var c = TableSetMerger.Merge(a, b);
        
        Dictionary<int, DbFile> dbFiles = c.Files.ToDictionary(f => f.Id);

        //THEN
        c.Directories.Count.Should().Be(1);
        c.Files.Count.Should().Be(3);
        c.Classes.Count.Should().Be(4);

        dbFiles[c.Classes.Find(c => c.Fqn == "classInX").ParentId].Path.Should().Be("dir/x");
        dbFiles[c.Classes.Find(c => c.Fqn == "classInCommon").ParentId].Path.Should().Be("dir/common");
        dbFiles[c.Classes.Find(c => c.Fqn == "classInY").ParentId].Path.Should().Be("dir/y");
        dbFiles[c.Classes.Find(c => c.Fqn == "classInCommon2").ParentId].Path.Should().Be("dir/common");
    }
}