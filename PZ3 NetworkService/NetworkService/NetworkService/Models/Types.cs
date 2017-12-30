using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService
{
    public static class Types
    {
        public static readonly List<Type> ListOfTypes = new List<Type>
        {
            new Type() { NAME = "IA", IMG_URL = "Images/highway.png"},
            new Type() { NAME = "IB", IMG_URL = "Images/motorway.png"}
        };
    }
}
