using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Room_Mate_Prototype
{
    public class Feed_Items
    {
        //public UInt16 GroceryId {get; set;}
        public String ItemName { get; set; }

        public Feed_Items()
        {
            ItemName = String.Empty;
        }

        public Feed_Items(string itemName)
        {
            ItemName = itemName;
        }
    }
}
