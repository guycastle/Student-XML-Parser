using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class Helper
    {
        public static void SetAuditCreationInfo(this IAuditable auditable, string username)
        {
            auditable.CreationDate = DateTime.Now;
            auditable.CreatedBy = username;
        }

        public static string Print(this IEnumerable<Student> items)
        {
            return string.Join("\r\n", items);
        }

        public static int getLeeftijd(this Student student)
        {
            int leeftijd = DateTime.Now.Year - student.GeboorteDatum.Year;
            if (DateTime.Now.Month < student.GeboorteDatum.Month || (DateTime.Now.Month == student.GeboorteDatum.Month && DateTime.Now.Day < student.GeboorteDatum.Day))
            {
                leeftijd--;
            }
            return leeftijd;
        }
    }
}
