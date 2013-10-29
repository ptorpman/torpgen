//=============================================================================================================================================
// Copyright (c) 2013-  Peter R. Torpman (peter at torpman dot se)
//
// This file is part of TorpGen (http://torpgen.sourceforge.net)
//
// TorpGen is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// TorpGen is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
//=============================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorpGen2
{
    /// <summary>
    /// This class implements the database of TorpGen.
    /// </summary>
    [Serializable()]
    public class TorpgenDB
    {
        /// <summary>
        /// Database version
        /// </summary>
        public String mDbVersion { get; set; }     
        /// <summary>
        /// List of all persons
        /// </summary>
        public List<Person> mPersons { get; set; }
        /// <summary>
        /// List of all marriages
        /// </summary>
        public List<Marriage> mMarriages { get; set; }

        /// <summary>
        /// Dictionary for easy access of persons
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Dictionary<Guid, Person> mDictPersons = new Dictionary<Guid, Person>();
        
        
        public TorpgenDB()
        {
            mDbVersion = "0.1.0";
            mPersons = new List<Person>();
            mMarriages = new List<Marriage>();
        }

        /// <summary>
        /// Updates the dictionary to contain all persons
        /// </summary>
        public void updateDict()
        {
            for (int i = 0; i < mPersons.Count; i++)
            {
                mDictPersons.Add(mPersons[i].id, mPersons[i]);
            }
        }

        /// <summary>
        /// Returns a list of persons
        /// </summary>
        /// <returns></returns>
        public List<Person> GetPersonList()
        {
            return mPersons;
        }

        /// <summary>
        /// Adds a person
        /// </summary>
        /// <param name="p">Instance of person.</param>
        public void AddPerson(Person p)
        {
            mPersons.Add(p);

            mDictPersons.Add(p.id, p);

            // If person's parents have been specified, make sure that there their marriage contains the current person as a child
            AddChildToPerson(p.father, p);
            AddChildToPerson(p.mother, p);
        }

        /// <summary>
        /// Returns a list of persons with a specific gender.
        /// </summary>
        /// <param name="gender">Gender wanted</param>
        /// <returns>List of persons, or null</returns>
        public List<Person> GetPersonsWithGender(Gender gender)
        {
            if (mPersons == null || mPersons.Count == 0)
            {
                return null;
            }

            List<Person> l = new List<Person>();

            for (int i = 0; i < mPersons.Count; i++)
            {
                if (mPersons[i].gender == gender)
                {
                    l.Add(mPersons[i]);
                }
            }

            return l;
        }

        /// <summary>
        /// Returns a person with a specific GUID
        /// </summary>
        /// <param name="id">GUID of person</param>
        /// <returns>Instance of person or null</returns>
        public Person GetPerson(Guid id)
        {
            if (mDictPersons.ContainsKey(id))
            {
                return mDictPersons[id];
            }

            return null;
        }

        /// <summary>
        /// Add a marriage
        /// </summary>
        /// <param name="man"></param>
        /// <param name="woman"></param>
        /// <param name="text"></param>
        /// <param name="place"></param>
        /// <param name="notes"></param>
        /// <returns></returns>
        public Marriage AddMarriage(Person man, Person woman, String text, String place, String notes)
        {
            // See if there already is a marriage containing these twe people

            for (int i = 0; i < mMarriages.Count; i++)
            {
                if (mMarriages[i].husband == man.id && mMarriages[i].wife == woman.id)
                {
                    // Already there...
                    return mMarriages[i];
                }
            }
            
            // Not there ...add a new one
            Marriage m = new Marriage(man.id, woman.id, text, place, notes);

            man.AddMarriage(m.id);
            woman.AddMarriage(m.id);

            mMarriages.Add(m);
            return m;
        }

        /// <summary>
        /// Adds a marriage
        /// </summary>
        /// <param name="m"></param>
        public void AddMarriage(Marriage m)
        {
            if (!mMarriages.Contains(m))
            {
                mMarriages.Add(m);
            }

            // Make sure both husband and wife is updated
            mDictPersons[m.husband].AddMarriage(m.id);
            mDictPersons[m.wife].AddMarriage(m.id);
        }


        /// <summary>
        /// Removes a marriage
        /// </summary>
        /// <param name="marriageId"></param>
        public void RemoveMarriage(Guid marriageId)
        {
            Marriage m = GetMarriage(marriageId);

            if (m.husband != Guid.Empty)
            {
                mDictPersons[m.husband].RemoveMarriage(marriageId);
            }

            if (m.wife != Guid.Empty)
            {
                mDictPersons[m.wife].RemoveMarriage(marriageId);
            }

            mMarriages.Remove(m);
        }

        /// <summary>
        /// Removes a marriage
        /// </summary>
        /// <param name="m">Marriage instance</param>
        public void RemoveMarriage(Marriage m)
        {
            mMarriages.Remove(m);
        }

        /// <summary>
        /// Removes a person from a marriage.
        /// </summary>
        /// <param name="marriageId"></param>
        /// <param name="personId"></param>
        public void RemovePersonFromMarriage(Guid marriageId, Guid personId)
        {
            Marriage m = GetMarriage(marriageId);

            if (m == null) 
            {
                return;
            }

            if (m.husband == personId)
            {
                m.husband = Guid.Empty;
            }
            else if (m.wife == personId)
            {
                m.wife = Guid.Empty;
            }

            // Remove marriage if both husband and wife is empty
            if (m.husband == Guid.Empty && m.wife == Guid.Empty)
            {
                RemoveMarriage(m);
            }
        }

        public Marriage GetMarriage(Guid id)
        {
            Marriage tmp = mMarriages.Find(delegate(Marriage m) { return m.id == id; });

            return tmp;
        }

        public Marriage GetMarriage(Guid manId, Guid wifeId)
        {
            Marriage tmp = mMarriages.Find(delegate(Marriage m) { return m.husband == manId && m.wife == wifeId; });

            return tmp;
        }

        /// <summary>
        /// Returns a list of Marriage instances for a supplied list of GUIDs.
        /// </summary>
        /// <param name="guidList">List of marriage GUIDs.</param>
        /// <returns></returns>
        public List<Marriage> GetMarriageList(List<Guid> guidList)
        {
            if (guidList == null || guidList.Count == 0)
            {
                return null;
            }

            List<Marriage> list = new List<Marriage>();

            for (int i = 0; i < guidList.Count; i++)
            {
                list.Add(GetMarriage(guidList[i]));
            }

            return list;
        }

        /// <summary>
        /// Returns if a person has children
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool PersonHasChildren(Person p)
        {
            return (p.children.Count > 0);
        }

        /// <summary>
        /// Adds a child to a person
        /// </summary>
        /// <param name="id">Parent's ID</param>
        /// <param name="child">Childs person data</param>
        public void AddChildToPerson(Guid id, Person child)
        {
            if (mDictPersons.ContainsKey(id)) 
            {
                mDictPersons[id].AddChild(child.id);
            }
        }
        
        /// <summary>
        /// Adds a child to a person
        /// </summary>
        /// <param name="parent">Parent's ID</param>
        /// <param name="child">Child's ID</param>
        public void AddChildToPerson(Guid parent, Guid child)
        {
            if (mDictPersons.ContainsKey(parent))
            {
                if (mDictPersons[parent].children == null || mDictPersons[parent].children.Contains(child) == false)
                {
                    mDictPersons[parent].AddChild(child);
                }
            }
        }

        public void RemoveChildFromPerson(Guid parent, Guid child)
        {
            if (!mDictPersons.ContainsKey(parent))
            {
                return;
            }

            mDictPersons[parent].RemoveChild(child);
        }


        public List<Person> GetChildren(Guid husband, Guid wife)
        {
            List<Person> kids = new List<Person>();
            
            for (int i = 0; i < mPersons.Count; i++)
            {
                if (mPersons[i].father == husband && mPersons[i].mother == wife) {
                    kids.Add(mPersons[i]);
                }
            }

            return kids;           
        }

        /// <summary>
        /// Returns a list of GUIDs sorted by the person's birth date.
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public List<Guid> GetListSortedByBirth(List<Guid> children)
        {
            // Make list sorted by birth 
            var personList = new List<Person>();

            foreach (var c in children)
            {
                personList.Add(GetPerson(c));
            }

            var sortedPersonList = personList.OrderBy(v => v.birth).ToList();
            var sortedList = new List<Guid>();

            foreach (var c in sortedPersonList)
            {
                sortedList.Add(c.id);
            }

            return sortedList;
        }

        /// <summary>
        /// Returns a list of persons sorted by person's birth date
        /// </summary>
        /// <param name="iids"></param>
        /// <returns></returns>
        public List<Person> GetPersonListSortedByBirth(List<Guid> iids)
        {
            // Make list sorted by birth 
            var personList = new List<Person>();

            foreach (var c in iids)
            {
                personList.Add(GetPerson(c));
            }

            return personList.OrderBy(v => v.birth.date).ToList();
        }

    }

}
