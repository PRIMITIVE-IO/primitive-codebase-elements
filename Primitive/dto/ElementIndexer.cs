namespace PrimitiveCodebaseElements.Primitive.dto
{
    public static class ElementIndexer
    {
        // index each element, so that the source can be associated
        static int classIndexer = 0;
        static int methodIndexer = 0;
        static int fieldIndexer = 0;
        static int argumentIndexer = 0;

        public static int GetClassndex()
        {
            classIndexer++;
            return classIndexer;
        }
        
        public static int GetMethodIndex()
        {
            methodIndexer++;
            return methodIndexer;
        }
        
        public static int GetFieldIndex()
        {
            fieldIndexer++;
            return fieldIndexer;
        }

        public static int GetArgumentIndex()
        {
            argumentIndexer++;
            return argumentIndexer;
        }
    }
}