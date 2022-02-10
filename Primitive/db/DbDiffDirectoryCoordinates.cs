using System.Collections.Generic;
using System.Data;
using PrimitiveCodebaseElements.Primitive.db.util;

namespace PrimitiveCodebaseElements.Primitive.db
{
    public class DbDiffDirectoryCoordinates
    {
        public readonly int BranchId;
        public readonly int OriginalDirectoryId;
        public readonly int AddedDirectoryId;
        public readonly int IsDeleted;
        public readonly int PositionX;
        public readonly int PositionY;

        public DbDiffDirectoryCoordinates(int branchId, int originalDirectoryId, int addedDirectoryId, int isDeleted,
            int positionX, int positionY)
        {
            BranchId = branchId;
            OriginalDirectoryId = originalDirectoryId;
            AddedDirectoryId = addedDirectoryId;
            IsDeleted = isDeleted;
            PositionX = positionX;
            PositionY = positionY;
        }

        public static readonly string CreateTable =
            @"CREATE TABLE diff_directories_coordinates (
                         branch_id INTEGER NOT NULL,
                         original_directory_id INTEGER NULL,
                         added_directory_id INTEGER NULL,
                         is_deleted byte NOT NULL DEFAULT 0,
                         position_x REAL NOT NULL,
                         position_y REAL NOT NULL,
                         FOREIGN KEY(branch_id) REFERENCES diff_branches(id) ON UPDATE CASCADE,
                         FOREIGN KEY(added_directory_id) REFERENCES diff_directories_added (id) ON UPDATE CASCADE,
                         FOREIGN KEY(original_directory_id) REFERENCES directories(id) ON UPDATE CASCADE);
              CREATE INDEX diff_directories_coordinates_branch_id ON diff_directories_coordinates (branch_id);";
        
        
        public static void SaveAll(IEnumerable<DbDiffDirectoryCoordinates> directories, IDbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            IDbTransaction transaction = conn.BeginTransaction();
            cmd.CommandText =
                @"INSERT INTO diff_directories_coordinates (
                        branch_id,
                        original_directory_id,
                        added_directory_id,
                        is_deleted,
                        position_x,
                        position_y
                      ) VALUES (
                        @BranchId,
                        @OriginalDirectoryId,
                        @AddedDirectoryId,
                        @IsDeleted,
                        @PositionX,
                        @PositionY
                      )";

            foreach (DbDiffDirectoryCoordinates dir in directories)
            {
                cmd.AddParameter(System.Data.DbType.Int32, "@BranchId", dir.BranchId);
                cmd.AddParameter(System.Data.DbType.Int32, "@OriginalDirectoryId", dir.OriginalDirectoryId);
                cmd.AddParameter(System.Data.DbType.Int32, "@AddedDirectoryId", dir.AddedDirectoryId);
                cmd.AddParameter(System.Data.DbType.Int32, "@IsDeleted", dir.IsDeleted);
                cmd.AddParameter(System.Data.DbType.Int32, "@PositionX", dir.PositionX);
                cmd.AddParameter(System.Data.DbType.Int32, "@PositionY", dir.PositionY);
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
            transaction.Dispose();
            cmd.Dispose();
        }

        public static List<DbDiffDirectoryCoordinates> ReadAll(IDbConnection conn)
        {
            string query = @"
                    SELECT
                        branch_id,
                        original_directory_id,
                        added_directory_id,
                        is_deleted,
                        position_x,
                        position_y
                    FROM diff_directories_coordinates
            ";

            return conn.Execute(query).TransformRows(row => new DbDiffDirectoryCoordinates(
                branchId: row.GetInt32("branch_id"),
                originalDirectoryId: row.GetInt32("original_directory_id"),
                addedDirectoryId: row.GetInt32("added_directory_id"),
                isDeleted: row.GetInt32("is_deleted"),
                positionX: row.GetInt32("position_x"),
                positionY: row.GetInt32("position_y")
            ));
        }
    }
}