using System;
using System.Collections.Generic;
using System.IO;

namespace Jaap
{
    public class NestedBinaryWriter : BinaryWriter
    {
        private List<INestedWriteable> _nestedWriteables = new List<INestedWriteable>();
        private List<ObjectReference> _objectReferences = new List<ObjectReference>();
        private Dictionary<INestedWriteable, int> _objectReferenceCount = new Dictionary<INestedWriteable, int>();

        public NestedBinaryWriter(Stream output) : base(output)
        {
        }

        public void EnqueueObject(INestedWriteable nestible)
        {
            if (nestible == null)
                return;

            if (!_nestedWriteables.Contains(nestible))
            {
                _nestedWriteables.Add(nestible);
            }
        }

        public void WriteReference(INestedWriteable nestible)
        {
            Int32 tempPos = 0;
            Write(tempPos);

            if (nestible == null)
                return;

            _objectReferences.Add(new ObjectReference
            {
                NestedWriteable = nestible,
                WhereToWriteOffset = (int)BaseStream.Position
            });

            if (_objectReferenceCount.ContainsKey(nestible))
            {
                _objectReferenceCount[nestible] = _objectReferenceCount[nestible] + 1;
            }
            else
            {
                _objectReferenceCount.Add(nestible, 1);
            }



        }

        public void WriteData()
        {
            // First write everything that is in the queue
            ProcessQueue();

            // Then write the correct offset to the objects
            ProcessReferences();
        }

        protected void ProcessQueue()
        {
            //while (_nestedWriteables.Any())
            for(int i=0; i< _nestedWriteables.Count; i++)
            {
                var nestedWriteable = _nestedWriteables[i];

                UInt32 baseOffset = (UInt32)BaseStream.Position;

                var offsetToByteAlignment = (baseOffset % 16);
                if (offsetToByteAlignment > 0)
                {
                    offsetToByteAlignment = 16 - offsetToByteAlignment;
                    var padding = new byte[offsetToByteAlignment];
                    Write(padding);
                }

                baseOffset = (UInt32)BaseStream.Position;


                //_nestedWriteableOffsets[nestedWriteable] = baseOffset;
                nestedWriteable.NestedObjectOffset = baseOffset;
                nestedWriteable.WriteNested(this);
            }

            _nestedWriteables.Clear();
        }

        protected void ProcessReferences()
        {
            var endPosition = BaseStream.Position;

            foreach (var objectReference in _objectReferences)
            {
                UInt32 offset = 0;
                uint baseValue = 0;
                if (objectReference.NestedWriteable != null)
                {
                    offset = objectReference.NestedWriteable.NestedObjectOffset;
                    baseValue = 5;
                }

                Seek(objectReference.WhereToWriteOffset, SeekOrigin.Begin);

                WriteOffset(offset, baseValue);
            }

            BaseStream.Position = endPosition;
        }

        public void WriteOffset(uint offset, uint typeMask)
        {
            var baseOffset = (uint)offset;

            if (baseOffset != 0)
            {
                baseOffset = baseOffset | (typeMask << 28);
            }
            Write(baseOffset);
        }

        public void ClearHierarchyData()
        {
            _nestedWriteables.Clear();
            _objectReferences.Clear();
            _objectReferenceCount.Clear();
        }
    }
}
