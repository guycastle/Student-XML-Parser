using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Domain
{
    public class DB<T> : IEnumerable<T> where T: EntityBase, IAuditable, IValidatable
    {
        private List<T> items { get; set; } 

        public DB()
        {
            items = new List<T>();
        }

        public void Add(T item)
        {
            items.Add(item);
        }

        public void Delete(T item)
        {
            items.Remove(item);
        }

        public int Count()
        {
            return items.Count();
        }

        public void Upsert(T item)
        {   
            this.items.RemoveAll(x => x.ID == item.ID);
            this.items.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this.items).GetEnumerator();
        }

        public bool ValidateAll()
        {
            foreach (T item in this.items)
            {
                if (!item.Validate())
                {
                    return false;
                }
            }
            return true;
        }
    }
}