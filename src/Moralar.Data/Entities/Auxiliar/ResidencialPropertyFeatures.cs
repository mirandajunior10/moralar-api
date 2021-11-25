using MongoDB.Bson.Serialization.Attributes;
using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moralar.Data.Entities.Auxiliar
{
    public class ResidencialPropertyFeatures
    {
        public double PropertyValue { get; set; }
        public TypeProperty TypeProperty { get; set; }
        public double SquareFootage { get; set; }
        public double CondominiumValue { get; set; }
        public double IptuValue { get; set; }
        public string Neighborhood { get; set; }
        public int NumberFloors { get; set; }
        public int FloorLocation { get; set; }
        public bool HasElavator { get; set; }
        public int NumberOfBedrooms { get; set; }
        public int NumberOfBathrooms { get; set; }
        public bool HasServiceArea { get; set; }
        public bool HasGarage { get; set; }
        public bool HasYard { get; set; }
        public bool HasCistern { get; set; }
        public bool HasWall { get; set; }
        public bool HasAccessLadder { get; set; }
        public bool HasAccessRamp { get; set; }
        public bool HasAdaptedToPcd { get; set; }
        public TypePropertyRegularization PropertyRegularization { get; set; }
        public TypePropertyGasInstallation TypeGasInstallation { get; set; }

    }
}
