using SmartApiary.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models
{
    public class SprayingRecord : AggregateRoot
    {
        public DateTime StartTime { get; private set; }
        public double DurationHours { get; private set; }
        public string ChemicalName { get; private set; }
        public Guid ParcelId { get; private set; }

        private SprayingRecord() { }

        public static SprayingRecord Create(
            DateTime startTime,
            double durationHours,
            string chemicalName,
            Guid parcelId)
        {
            return new SprayingRecord
            {
                StartTime = startTime,
                DurationHours = durationHours,
                ChemicalName = chemicalName,
                ParcelId = parcelId
            };
        }

        public static SprayingRecord Rehydrate(
            Guid id,
            DateTime startTime,
            double durationHours,
            string chemicalName,
            Guid parcelId)
        {
            return new SprayingRecord
            {
                Id = id,
                StartTime = startTime,
                DurationHours = durationHours,
                ChemicalName = chemicalName,
                ParcelId = parcelId
            };
        }
    }
}
