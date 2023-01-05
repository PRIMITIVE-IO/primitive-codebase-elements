using System;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    /// <summary>
    /// <para>A general set of access flags as defined throughout the Java class file format specification. Some flags
    /// apply to only certain types of structures, such as <c>ACC_STRICT</c> only applying to methods. However, whenever
    /// two flags apply to multiple structures, the underlying value is the same in each case.</para>
    ///
    /// <para>Note that only the flags that are used have corresponding properties, so this is not comprehensive by any
    /// means. For other languages, the native access flags have been mapped to match those defined for Java.</para>
    /// </summary>
    [Flags]
    [PublicAPI]
    public enum AccessFlags : uint
    {
        None = 0x0000,
        AccPublic = 0x0001,
        AccPrivate = 0x0002,
        AccProtected = 0x0004,
        AccStatic = 0x0008,
        AccFinal = 0x0010,
        AccSynchronized = 0x0020,
        AccVolatile = 0x0040,
        AccVarargs = 0x0080,
        AccInterface = 0x0200,
        AccAbstract = 0x0400,
        AccStrict = 0x0800,
        AccEnum = 0x4000
    }

    [PublicAPI]
    public static class AccessFlagsExtensions
    {
        /// <summary>
        /// Return the given access flags, with the specified ones unset,
        /// regardless of their original values.
        /// </summary>
        /// <param name="original">The original flags.</param>
        /// <param name="toRemove">The flags to unset in the original.</param>
        /// <returns></returns>
        public static AccessFlags Without(
            this AccessFlags original,
            AccessFlags toRemove)
        {
            return original & ~toRemove;
        }
    }
}