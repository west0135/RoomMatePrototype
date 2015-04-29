using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Room_Mate_Prototype
{
    public class Grocery_Items
    {
        //public UInt16 GroceryId {get; set;}
        public Boolean CheckedOff {get; set;}
        public String ItemName {get; set;}
        public Boolean Urgent { get; set; }

        public Grocery_Items() 
        {
            CheckedOff = false;
            ItemName = String.Empty;
            Urgent = false;
        }

        public Grocery_Items(string itemName, bool checkedOff, bool urgent) 
        {
            CheckedOff = checkedOff;
            ItemName = itemName;
            Urgent = urgent;
        }
    }
}
