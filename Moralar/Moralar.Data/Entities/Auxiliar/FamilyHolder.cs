using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Data.Entities.Auxiliar
{
    public class FamilyHolder
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Cpf { get; set; }
        public long Birthday { get; set; }
        public TypeGenre Genre { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public TypeScholarity Scholarity { get; set; }
    }
}
