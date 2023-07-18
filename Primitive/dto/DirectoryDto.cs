using System.Collections.Generic;


namespace PrimitiveCodebaseElements.Primitive.dto
{
    
    public class DirectoryDto
    {
        public readonly string Path;
        public readonly double PositionX;
        public readonly double PositionY;
        public readonly List<FileDto> Files;

        public DirectoryDto(string path, double positionX, double positionY, List<FileDto> files)
        {
            Path = path;
            PositionX = positionX;
            PositionY = positionY;
            Files = files;
        }
    }
}