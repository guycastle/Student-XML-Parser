using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Student : EntityBase, IAuditable, IValidatable
    {
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public DateTime GeboorteDatum { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Voornaam))
                return false;

            if (string.IsNullOrWhiteSpace(Achternaam))
                return false;
            int leeftijd = DateTime.Now.Year - GeboorteDatum.Year;
            if (DateTime.Now.Month < GeboorteDatum.Month || (DateTime.Now.Month == GeboorteDatum.Month && DateTime.Now.Day < GeboorteDatum.Day))
            {
                leeftijd--;
            }
            if (leeftijd < 15 || leeftijd > 100)
            {
                return false;
            }
           

            return true;  
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}, {2}", ID, Voornaam, Achternaam);
        }
    }
}
