using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using PrimitiveCodebaseElements.Primitive;
using PrimitiveCodebaseElements.Primitive.db;
using PrimitiveCodebaseElements.Primitive.db.converter;
using PrimitiveCodebaseElements.Primitive.dto;


ReadCodeStructure(args[0]);
return;

void ReadCodeStructure(string analysis)
{
    string connectionString = "URI=file:" + analysis;

    TableSet tableSet;
    using (IDbConnection conn = new SQLiteConnection(connectionString))
    {
        conn.Open();
        tableSet = TableSet.ReadAll(
            conn,
            ProgressTracker.Dummy
        );
    }

    List<DirectoryDto> directoryDtos = TableSetToDtoConverter.ToDirectoryDto(tableSet);

    var x = 1;
}