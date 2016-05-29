using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Domain
{
    public class DB<T> : IEnumerable<T> where T: EntityBase, IAuditable, IValidatable
    {
        private ConcurrentDictionary<T, String> items;

        private ICollection<T> Items
        {
            get
            {
                return this.items.Keys;
            }
            set
            {
                this.items = new ConcurrentDictionary<T, string>();
                foreach (T entry in value)
                {
                    items.GetOrAdd(entry, "");
                }
            }
        }
        public DB()
        {
            items = new ConcurrentDictionary<T, string>();
        }

        public void Add(T item)
        {
            items.Keys.Add(item);
        }

        public void Delete(T item)
        {
            items.Keys.Remove(item);
        }

        public int Count()
        {
            return items.Count();
        }

        public void Upsert(T item)
        {
            if (this.items.ContainsKey(item))
            {
                this.items.AddOrUpdate(item, "", (key, oldValue) => "");
            }
            this.items.GetOrAdd(item, "");
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.items.Keys).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this.items.Keys).GetEnumerator();
        }

        public bool ValidateAll()
        {
            foreach (T item in this.items.Keys)
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