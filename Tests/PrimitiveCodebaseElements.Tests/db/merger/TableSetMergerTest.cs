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
            directories: new List<DbDirectory> { new(1, "dir", 0, 0) },
            files: new List<DbFile>
            {
                new(1, 1, "x", "dir/x", string.Empty, 1),
                new(2, 1, "common", "dir/common", string.Empty, 1)
            },
            classes: new List<DbClass>
            {
                new(1, 4, 1, "classInX", 1,  1),
                new(2, 4, 2, "classInCommon", 1, 1)
            }
        );

        TableSet b = new(
            directories: new List<DbDirectory> { new(1, "dir", 0, 0) },
            files: new List<DbFile>
            {
                new(1, 1, "y", "dir/y", string.Empty, 1),
                new(2, 1, "common", "dir/common", string.Empty, 1)
            },
            classes: new List<DbClass>
            {
                new(1, 4, 1, "classInY", 1, 1),
                new(2, 4, 2, "classInCommon2", 1, 1)
            }
        );

        //WHEN
        TableSet c = TableSetMerger.Merge(a, b);
        
        Dictionary<int, DbFile> dbFiles = c.Files.ToDictionary(f => f.Id);

        //THEN
        c.Directories.Count.Should().Be(1);
        c.Files.Count.Should().Be(3);
        c.Classes.Count.Should().Be(4);

        /*
        dbFiles[c.Classes.Find(dbClass => dbClass.Fqn == "classInX").ParentClassId!.Value].Path.Should().Be("dir/x");
        dbFiles[c.Classes.Find(dbClass => dbClass.Fqn == "classInCommon").ParentClassId!.Value].Path.Should().Be("dir/common");
        dbFiles[c.Classes.Find(dbClass => dbClass.Fqn == "classInY").ParentClassId!.Value].Path.Should().Be("dir/y");
        dbFiles[c.Classes.Find(dbClass => dbClass.Fqn == "classInCommon2").ParentClassId!.Value].Path.Should().Be("dir/common");
        */
    }
}