namespace Jaap
{
    public interface INestedWriteable
    {
        uint NestedObjectOffset { get; set; }

        void WriteNested(NestedBinaryWriter nestedBinaryWriter);
    }
}
