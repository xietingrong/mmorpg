using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BagItem
    {
        public ushort ItemId;
        public ushort Count;
        public static BagItem zero = new BagItem { ItemId = 0, Count = 0 };
        public BagItem(int itemId, int count)
        {
            this.ItemId = (ushort)itemId;
            this.Count = (ushort)count;

        }
        public static bool operator == (BagItem lhs,BagItem rhs)
        {
            return lhs.ItemId == rhs.ItemId && lhs.Count == rhs.Count;
        }
        public static bool operator !=(BagItem lhs, BagItem rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object other)
        {
            if(other is BagItem)
            {
                return Equals((BagItem)other);
            }
            return false;
        }
        public  bool Equals(BagItem other)
        {
            
            return this == other;
        }
        public override int GetHashCode()
        {
            return ItemId.GetHashCode()^(Count.GetHashCode()<<2);
        }
    }
}
