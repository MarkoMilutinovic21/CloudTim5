using SmartApiary.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApiary.Domain.Models
{
    public class Crop : AggregateRoot
    {
        public string Name { get; private set; }
        public DateTime SowingDate { get; private set; }
        public Guid ParcelId { get; private set; }

        private Crop() { }

        public static Crop Create(
            string name,
            DateTime sowingDate,
            Guid parcelId)
        {
            return new Crop
            {
                Name = name,
                SowingDate = sowingDate,
                ParcelId = parcelId
            };
        }

        public static Crop Rehydrate(
            Guid id,
            string name,
            DateTime sowingDate,
            Guid parcelId)
        {
            return new Crop
            {
                Id = id,
                Name = name,
                SowingDate = sowingDate,
                ParcelId = parcelId
            };
        }
    }
}
