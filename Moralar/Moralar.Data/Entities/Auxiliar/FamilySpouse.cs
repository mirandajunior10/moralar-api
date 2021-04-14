using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Data.Entities.Auxiliar
{
    public class FamilySpouse
    {
        public string Name { get; set; }
        public long? Birthday { get; set; }
        public TypeGenre? Genre { get; set; }
        public TypeScholarity? SpouseScholarity { get; set; }
    }
}
