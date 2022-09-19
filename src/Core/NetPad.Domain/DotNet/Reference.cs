using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NetPad.Common;

namespace NetPad.DotNet;

// This attribute is only used for NSwag polymorphism. Haven't figured out how to use our own STJ-based JsonInheritanceConverter
// and have NSwag use that when generating its schema.
[Newtonsoft.Json.JsonConverter(typeof(NJsonSchema.Converters.JsonInheritanceConverter), "discriminator")]

[JsonConverter(typeof(JsonInheritanceConverter<Reference>))]
[KnownType(typeof(AssemblyReference))]
[KnownType(typeof(PackageReference))]
public abstract class Reference
{
    protected Reference(string title)
    {
        Title = title;
    }

    public string Title { get; }
    public abstract void EnsureValid();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        // Same instances must be considered as equal
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        // Must be same type
        var typeOfThis = GetType();
        var typeOfOther = obj.GetType();
        if (typeOfThis != typeOfOther)
        {
            return false;
        }

        if (this is PackageReference thisPkg && obj is PackageReference otherPkg)
        {
            return thisPkg.PackageId == otherPkg.PackageId && thisPkg.Version == otherPkg.Version;
        }
        else if (this is AssemblyReference thisAsmRef && obj is AssemblyReference otherAsmRef)
        {
            return thisAsmRef.AssemblyPath == otherAsmRef.AssemblyPath;
        }

        throw new Exception("Unhandled Reference type");
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (this is PackageReference pkg)
        {
            return $"{pkg.PackageId}{pkg.Version}".GetHashCode();
        }
        else if (this is AssemblyReference asmRef)
        {
            return asmRef.AssemblyPath.GetHashCode();
        }

        throw new Exception("Unhandled Reference type");
    }

    /// <inheritdoc/>
    public static bool operator ==(Reference? left, Reference? right)
    {
        if (Equals(left, null))
        {
            return Equals(right, null);
        }

        if (Equals(right, null))
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(Reference? left, Reference? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this is PackageReference pkg)
        {
            return $"{Title}: {pkg.PackageId} v{pkg.Version}";
        }
        else if (this is AssemblyReference asmRef)
        {
            return $"{Title}: {asmRef.AssemblyPath}";
        }

        return $"{Title}: [{GetType().Name}]";
    }
}