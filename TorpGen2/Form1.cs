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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using GenericTree;

namespace TorpGen2
{
    public enum PersonPageMode
    {
        None, Add, Edit
    }

    public partial class Form1 : Form
    {
        /// <summary>
        /// The database of all persons etc.
        /// </summary>
        TorpgenDB mDB = new TorpgenDB();
        /// <summary>
        /// The current person being handled
        /// </summary>
        Person mCurrentPerson = null;
        /// <summary>
        /// State variable for the PersonPage
        /// </summary>
        PersonPageMode mPageMode = PersonPageMode.None;
        /// <summary>
        /// Variable used for storing all men temporarily
        /// </summary>
        List<Person> mMen = new List<Person>();
        /// <summary>
        /// Variable used for storing all women temporarily
        /// </summary>
        List<Person> mWomen = new List<Person>();
        /// <summary>
        /// The database file name
        /// </summary>
        String mFileName; // Default: @"C:\temp\torpgen.xml"
        /// <summary>
        /// Graphics instance
        /// </summary>
        Graphics mGraphics = null;
        /// <summary>
        /// An instance of the persons tree
        /// </summary>
        GenericTree.GenericTree mTree = null;
        /// <summary>
        /// Instance used for printing
        /// </summary>
        private PersonPrint mPrint = PersonPrint.Instance;

        // Temporary variables used during adding/editing of a person
        List<Marriage> mTempMarriages = new List<Marriage>();
        Guid mTempFather = Guid.Empty;
        Guid mTempMother = Guid.Empty;


        /// <summary>
        /// Constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            mCurrentPerson = null;
            mPageMode = PersonPageMode.None;
            mFileName = "";

            LoadPersonsFromFile();
            toolStripStatusLabel1.Text = "";
        }

        /// <summary>
        /// Handles addition of a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbAdd_Click(object sender, EventArgs e)
        {
            HandleAddPerson();
        }


        private void HandleAddPerson()
        {
            this.lbPanelTopLabel.Text = "Add Person";
            this.lbEdit.ForeColor = SystemColors.InactiveCaption;
            this.lbRemove.ForeColor = SystemColors.InactiveCaption;

            this.mCurrentPerson = new Person(mDB);
            this.mCurrentPerson.id = System.Guid.NewGuid();

            lbPersons.Enabled = false;

            ShowPersonPage(PersonPageMode.Add, mCurrentPerson);
        }


        /// <summary>
        /// Handles editing of a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbEdit_Click(object sender, EventArgs e)
        {
            HandleEditPerson(false);
        }

        /// <summary>
        /// Handles removal of a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbRemove_Click(object sender, EventArgs e)
        {
            HandleRemovePerson(false);
        }

        /// <summary>
        /// User selects to exit the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbExit_Click(object sender, EventArgs e)
        {
            HandleExitApplication();

        }

        private void HandleExitApplication()
        {
            if (MessageBox.Show("Do you want to exit?", "Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close();
            }
        }

        /// <summary>
        /// User wants to save the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSave_Click(object sender, EventArgs e)
        {
            SavePersonsToFile();
        }

        /// <summary>
        /// User wants to open an existing database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbOpen_Click(object sender, EventArgs e)
        {
            HandleLoadDatabase();
        }

        private void HandleLoadDatabase()
        {
            OpenFileDialog d = new OpenFileDialog();

            d.Filter = "TorpGen files (*.xml)|*.xml| All files (*.*)|*.*";
            d.InitialDirectory = "C:";
            d.Title = "Open TorpGen configuration file";

            if (d.ShowDialog() == DialogResult.OK)
            {
                mFileName = d.FileName;
            }
            else
            {
                return;
            }

            LoadPersonsFromFile();
        }



        /// <summary>
        /// User wants to create a new database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbNew_Click(object sender, EventArgs e)
        {
            HandleNewDatabase();
        }

        private void HandleNewDatabase()
        {
            // Open a file chooser to let the user select a new file to work with

            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "TorpGen files (*.xml)|*.xml| All files (*.*)|*.*";
            d.InitialDirectory = "C:";
            d.Title = "Create new TorpGen configuration file";

            if (d.ShowDialog() == DialogResult.OK)
            {
                mFileName = d.FileName;
            }
            else
            {
                return;
            }

            mDB = new TorpgenDB();
        }

        /// <summary>
        /// Mouse enters a "menu label"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuLabel_MouseEnter(object sender, EventArgs e)
        {
            var lb = (Label)sender;

            if (lb.ForeColor != SystemColors.InactiveCaption)
            {

                lb.ForeColor = SystemColors.HighlightText;
                lb.BackColor = SystemColors.Highlight;
            }

            if (sender == lbAdd)
            {
                toolStripStatusLabel1.Text = "Add a new person";
            }
            else if (sender == lbEdit)
            {
                toolStripStatusLabel1.Text = "Edit selected person";
            }
            else if (sender == lbRemove)
            {
                toolStripStatusLabel1.Text = "Remove selected person";
            }
            else if (sender == lbNew)
            {
                toolStripStatusLabel1.Text = "Create new Torpgen database";
            }
            else if (sender == lbOpen)
            {
                toolStripStatusLabel1.Text = "Open existing Torpgen database";
            }
            else if (sender == lbSave)
            {
                toolStripStatusLabel1.Text = "Save Torpgen database";
            }
            else if (sender == lbExit)
            {
                toolStripStatusLabel1.Text = "Exit application";
            }
            else if (sender == lbSpouseAdd)
            {
                toolStripStatusLabel1.Text = "Add spouse to person";
            }
            else if (sender == lbSpouseRemove)
            {
                toolStripStatusLabel1.Text = "Remove spouse from person";
            }
            else if (sender == lbSpouseUpdate)
            {
                toolStripStatusLabel1.Text = "Update spouse information";
            }
            else if (sender == lbPersonCancel)
            {
                toolStripStatusLabel1.Text = "Cancel updates";
            }
            else if (sender == lbPersonSave)
            {
                toolStripStatusLabel1.Text = "Save updates";
            }



        }

        /// <summary>
        /// Mouse leaves a "menu label"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuLabel_MouseLeave(object sender, EventArgs e)
        {
            var lb = (Label)sender;

            if (lb.ForeColor != SystemColors.InactiveCaption)
            {
                lb.ForeColor = Color.Black;
                lb.BackColor = Color.WhiteSmoke;
            }

            toolStripStatusLabel1.Text = "";
        }

        /// <summary>
        /// User aborts editing a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbPersonCancel_Click(object sender, EventArgs e)
        {
            HidePersonPage();
            lbPersons.Enabled = true;
        }

        /// <summary>
        /// User saves a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbPersonSave_Click(object sender, EventArgs e)
        {
            SavePerson();
            this.panelPerson.Visible = false;
            PopulatePersonListBox();

            this.lbAdd.ForeColor = SystemColors.ControlText;
            this.lbEdit.ForeColor = SystemColors.ControlText;
            this.lbRemove.ForeColor = SystemColors.ControlText;

            lbPersons.Enabled = true;
        }

        /// <summary>
        /// Saves a person after adding/editing
        /// </summary>
        private void SavePerson()
        {

            mCurrentPerson.firstName = tbFirstName.Text.Trim();
            mCurrentPerson.lastName = tbLastName.Text.Trim();
            mCurrentPerson.notes = tbNotes.Text.Trim();

            if (rbMale.Checked)
            {
                mCurrentPerson.gender = Gender.Male;
            }
            else
            {
                mCurrentPerson.gender = Gender.Female;
            }

            mCurrentPerson.birth = new LifeEvent(LifeEventTypes.Birth, tbBirthDate.Text.Trim(), tbBirthLocation.Text.Trim(), tbBirthNote.Text.Trim());
            mCurrentPerson.death = new LifeEvent(LifeEventTypes.Death, tbDeathDate.Text.Trim(), tbDeathLocation.Text.Trim(), tbDeathNotes.Text.Trim());

            mTempFather = (cmbFather.SelectedItem != null && cmbFather.Text != "") ? ((Person)cmbFather.SelectedItem).id : Guid.Empty;
            mTempMother = (cmbMother.SelectedItem != null && cmbMother.Text != "") ? ((Person)cmbMother.SelectedItem).id : Guid.Empty;

            // If adding, just add to database
            if (mPageMode == PersonPageMode.Add)
            {
                this.mDB.AddPerson(mCurrentPerson);
            }

            // Check if parents have changed
            if (mTempFather != mCurrentPerson.father)
            {
                mCurrentPerson.father = HandleParentChanges(mCurrentPerson.father, mTempFather, mCurrentPerson.id);
            }

            if (mTempMother != mCurrentPerson.mother)
            {
                mCurrentPerson.mother = HandleParentChanges(mCurrentPerson.mother, mTempMother, mCurrentPerson.id);
            }

            // Handle changes in spouses
            HandleSpouseChanges();

            mCurrentPerson = null;
        }

        /// <summary>
        /// Handles any changes in a parent during editing of a person
        /// </summary>
        /// <param name="origParent"></param>
        /// <param name="newParent"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        private Guid HandleParentChanges(Guid origParent, Guid newParent, Guid personId)
        {
            // Father removed?
            if (newParent == Guid.Empty)
            {
                // Make sure the previous father has the current person removed as a child.
                mDB.RemoveChildFromPerson(origParent, personId);
            }
            else
            {
                // Father changed - Remove from the original, add to new
                mDB.RemoveChildFromPerson(origParent, personId);
                mDB.AddChildToPerson(newParent, personId);
            }

            return newParent;
        }

        /// <summary>
        /// Handles any changes in spouses during editing of a person
        /// </summary>
        private void HandleSpouseChanges()
        {
            // See if any spouses have been removed?

            if (mTempMarriages.Count == 0) 
            {
                // No marriages specified.
                if (mCurrentPerson.marriages != null && mCurrentPerson.marriages.Count != 0) 
                {
                    // All marriages have been removed
                    for (int i = 0; i < mCurrentPerson.marriages.Count; i++) 
                    {
                        mDB.RemoveMarriage(mCurrentPerson.marriages[i]);
                    }

                    mCurrentPerson.marriages.Clear();
                }

                return;
            }

            // Any additions?
            for (int i = 0; i < mTempMarriages.Count; i++)
            {
                if (mCurrentPerson.marriages == null || !mCurrentPerson.marriages.Contains(mTempMarriages[i].id))
                {
                    // This marriage has been added.
                    mDB.AddMarriage(mTempMarriages[i]);
                }
            }

            // Any removals?
            if (mCurrentPerson.marriages != null)
            {
                for (int i = 0; i < mCurrentPerson.marriages.Count; i++)
                {
                    Marriage m = mDB.GetMarriage(mCurrentPerson.marriages[i]);

                    if (!mTempMarriages.Contains(m))
                    {
                        // This marriage has been removed.
                        mDB.RemoveMarriage(m);
                    }
                }
            }
        }

        /// <summary>
        /// Handles editing of a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="menuUsed">Flag if context menu is used to get to this function</param>
        private void HandleEditPerson(bool menuUsed)
        {
            if (this.lbEdit.ForeColor == SystemColors.InactiveCaption ||
                this.lbPersons.SelectedItem == null)
            {
                return;
            }

            lbPersons.Enabled = false;

            lbAdd.ForeColor = SystemColors.InactiveCaption;
            lbRemove.ForeColor = SystemColors.InactiveCaption;


            if (menuUsed)
            {
                // Get person under the mouse
                mCurrentPerson = (Person)mTree.GetSelectedNode().GetUserData();
            }
            else
            {
                // Get selected person from the listbox
                mCurrentPerson = (Person)lbPersons.SelectedItem;
            }

            ShowPersonPage(PersonPageMode.Edit, this.mCurrentPerson);
        }

        /// <summary>
        /// Handles removal of a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="menuUsed">Flag if context menu is used to get to this function</param>
        private void HandleRemovePerson(bool menuUsed)
        {
            if (this.lbRemove.ForeColor == SystemColors.InactiveCaption)
            {
                return;
            }

            if (menuUsed)
            {
                // Get person under the mouse
                mCurrentPerson = (Person)mTree.GetSelectedNode().GetUserData();
            }
            else
            {
                // Get selected person from the listbox
                mCurrentPerson = (Person)lbPersons.SelectedItem;
            }

            if (mCurrentPerson == null)
            {
                return;
            }

            String str = "Do you want to remove \"" + mCurrentPerson.listBoxString + "\"?";

            if (MessageBox.Show(str, "Remove person?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Remove person.
            }

        }

        /// <summary>
        /// Saves the database
        /// </summary>
        private void SavePersonsToFile()
        {
            XmlSerializer ser = new XmlSerializer(mDB.GetType());
            TextWriter writer = new StreamWriter(@"C:\temp\torpgen.xml");
            ser.Serialize(writer, mDB);
            writer.Close();
        }

        /// <summary>
        /// Loads the database
        /// </summary>
        private void LoadPersonsFromFile()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(mDB.GetType());
                StreamReader reader = new StreamReader(@"C:\temp\torpgen.xml");
                mDB = (TorpgenDB)ser.Deserialize(reader);
                reader.Close();

                mDB.updateDict();

                PopulatePersonListBox();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in XML file: " + e.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // See if we can create a file if it did not exist.
                //savePersonsToFile();
            }

        }

        /// <summary>
        /// Fills a listbox with the persons in the database
        /// </summary>
        private void PopulatePersonListBox()
        {
            lbPersons.Items.Clear();

            List<Person> list = mDB.GetPersonList();

            if (list == null)
            {
                return;
            }

            lbPersons.Items.AddRange(list.ToArray());
            lbPersons.DisplayMember = "listBoxString";
            lbPersons.ValueMember = "id";
        }

        /// <summary>
        /// Shows the person page
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="person"></param>
        private void ShowPersonPage(PersonPageMode mode, Person person)
        {
            mPageMode = mode;

            if (mMen != null)
            {
                mMen.Clear();
            }

            if (mWomen != null)
            {
                mWomen.Clear();
            }

            mMen = mDB.GetPersonsWithGender(Gender.Male);
            mWomen = mDB.GetPersonsWithGender(Gender.Female);

            // Make sure all fields are empty.
            tbFirstName.Text = "";
            tbLastName.Text = "";
            rbMale.Checked = true;
            rbFemale.Checked = false;
            tbBirthDate.Text = "";
            tbBirthLocation.Text = "";
            tbBirthNote.Text = "";
            tbDeathDate.Text = "";
            tbDeathLocation.Text = "";
            tbDeathNotes.Text = "";

            tbMarriageDate.Text = "";
            tbMarriageLocation.Text = "";
            tbMarriageNotes.Text = "";

            tbNotes.Text = "";
            lbSpouses.Items.Clear();


            if (mMen != null)
            {
                cmbFather.Items.Clear();
                cmbFather.Items.Add(String.Empty);
                cmbFather.Items.AddRange(mMen.ToArray());
            }

            if (mWomen != null)
            {
                cmbMother.Items.Clear();
                cmbMother.Items.Add(String.Empty);
                cmbMother.Items.AddRange(mWomen.ToArray());
            }

            cmbFather.DisplayMember = "listBoxString";
            cmbFather.ValueMember = "id";
            cmbMother.DisplayMember = "listBoxString";
            cmbMother.ValueMember = "id";


            if (mode == PersonPageMode.Add)
            {
                lbPanelTopLabel.Text = "Add Person";
                mTempFather = Guid.Empty;
                mTempFather = Guid.Empty;
                mTempMarriages = new List<Marriage>();

                person = mCurrentPerson;
            }
            else if (mode == PersonPageMode.Edit)
            {
                lbPanelTopLabel.Text = "Edit Person";

                tbFirstName.Text = person.firstName;
                tbLastName.Text = person.lastName;

                if (person.birth != null)
                {
                    tbBirthDate.Text = person.birth.date;
                    tbBirthLocation.Text = person.birth.place;
                    tbBirthNote.Text = person.birth.note;
                }

                if (person.death != null)
                {
                    tbDeathDate.Text = person.death.date;
                    tbDeathLocation.Text = person.death.place;
                    tbDeathNotes.Text = person.death.note;
                }

                mTempFather = person.father;
                mTempFather = person.mother;

                if (person.father != Guid.Empty)
                {
                    cmbFather.SelectedItem = mDB.GetPerson(person.father);
                }

                if (person.mother != Guid.Empty)
                {
                    cmbMother.SelectedItem = mDB.GetPerson(person.mother);
                }


                // Create a temporary list of marriages to be able to handle changes upon saving the person
                mTempMarriages = new List<Marriage>();
                
                if (person.marriages.Count > 0)
                {
                    for (int i = 0; i < person.marriages.Count; i++)
                    {
                        mTempMarriages.Add(mDB.GetMarriage(person.marriages[i]));
                    }
                }
            }

            if (person.gender == Gender.Male)
            {
                rbMale.Checked = true;
            }
            else
            {
                rbFemale.Checked = true;
            }


            UpdateMarriageSpouseCombo();
            UpdateMarriageControls();

            // Make sure the first tag is open
            tabPerson.SelectedIndex = 0;

            panelPerson.Visible = true;
        }

        /// <summary>
        /// Hides the person page.
        /// </summary>
        private void HidePersonPage()
        {
            this.panelPerson.Visible = false;
            this.lbAdd.ForeColor = SystemColors.ControlText;
            this.lbEdit.ForeColor = SystemColors.ControlText;
            this.lbRemove.ForeColor = SystemColors.ControlText;
            this.lbSpouseAdd.ForeColor = SystemColors.ControlText;
            this.mCurrentPerson = null;
            this.mPageMode = PersonPageMode.None;
        }

        /// <summary>
        /// User adds a spouse to a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSpouseAdd_Click(object sender, EventArgs e)
        {
            if (lbSpouseAdd.ForeColor == SystemColors.InactiveCaption)
            {
                // Button inactivated
                return;
            }

            Person spouse = (Person)cmbMarriageSpouse.SelectedItem;

            if (spouse == null)
            {
                return;
            }

            Person man = (mCurrentPerson.gender == Gender.Male) ? mCurrentPerson : spouse;
            Person woman = (mCurrentPerson.gender == Gender.Male) ? spouse : mCurrentPerson;

            // Create a marriage instance, but store it in a temporary variable in case the user changes his mind.
            Marriage m = new Marriage(man.id, woman.id, tbMarriageDate.Text, tbMarriageLocation.Text, tbMarriageNotes.Text);

            mTempMarriages.Add(m);

            lbSpouses.Items.Add(spouse);
            lbSpouses.DisplayMember = "listBoxString";
            lbSpouses.ValueMember = "id";
        }

        /// <summary>
        /// User removes a spouse from a person
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSpouseRemove_Click(object sender, EventArgs e)
        {
            Person spouse = (Person)lbSpouses.SelectedItem;

            Person man = (mCurrentPerson.gender == Gender.Male) ? mCurrentPerson : spouse;
            Person woman = (mCurrentPerson.gender == Gender.Male) ? spouse : mCurrentPerson;

            for (int i = 0; i < mTempMarriages.Count; i++)
            {
                if (mTempMarriages[i].husband == man.id && mTempMarriages[i].wife == woman.id)
                {
                    // Remove this marriage
                    mTempMarriages.Remove(mTempMarriages[i]);

                    tbMarriageDate.Text = "";
                    tbMarriageLocation.Text = "";
                    tbMarriageNotes.Text = "";

                    lbSpouses.Items.Remove(spouse);
                    return;
                }
            }
        }

        private void rbFemale_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMarriageSpouseCombo();
        }

        private void rbMale_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMarriageSpouseCombo();
        }

        /// <summary>
        /// Updates the combobox with with spouses to the current person
        /// </summary>
        private void UpdateMarriageSpouseCombo()
        {
            // Fill the combo with the correct gender.
            cmbMarriageSpouse.Items.Clear();

            if (rbMale.Checked && mWomen != null)
            {
                mCurrentPerson.gender = Gender.Male;
                cmbMarriageSpouse.Items.AddRange(mWomen.ToArray());
            }

            if (rbFemale.Checked && mMen != null)
            {
                mCurrentPerson.gender = Gender.Female;
                cmbMarriageSpouse.Items.AddRange(mMen.ToArray());
            }


            cmbMarriageSpouse.DisplayMember = "listBoxString";
            cmbMarriageSpouse.ValueMember = "id";
        }

        /// <summary>
        /// All controls related to a marriage is updated.
        /// </summary>
        private void UpdateMarriageControls()
        {
            Person spouse;

            lbSpouses.Items.Clear();
            lbSpouses.DisplayMember = "listBoxString";
            lbSpouses.ValueMember = "id";
          
            for (int i = 0; i < mTempMarriages.Count; i++)
            {
                if (mCurrentPerson.gender == Gender.Male)
                {
                    spouse = mDB.GetPerson(mTempMarriages[i].wife);
                }
                else
                {
                    spouse = mDB.GetPerson(mTempMarriages[i].husband);
                }

                lbSpouses.Items.Add(spouse);
            }
        }

        /// <summary>
        /// The spouse of a person has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSpouses_Click(object sender, EventArgs e)
        {
            if (lbSpouses.SelectedItem == null)
            {
                return;
            }

            Person p = (Person)lbSpouses.SelectedItem;
            Person man = (mCurrentPerson.gender == Gender.Male) ? mCurrentPerson : p;
            Person woman = (mCurrentPerson.gender == Gender.Male) ? p : mCurrentPerson;

            for (int i = 0; i < mTempMarriages.Count; i++)
            {
                if (mTempMarriages[i].husband == man.id && mTempMarriages[i].wife == woman.id)
                {
                    tbMarriageDate.Text = mTempMarriages[i].info.date;
                    tbMarriageLocation.Text = mTempMarriages[i].info.place;
                    tbMarriageNotes.Text = mTempMarriages[i].info.note;
                    return;
                }
            }
        }

        /// <summary>
        /// Spouse information has been updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSpouseUpdate_Click(object sender, EventArgs e)
        {
            if (lbSpouses.SelectedItem == null)
            {
                return;
            }

            Person p = (Person)lbSpouses.SelectedItem;
            Person man = (mCurrentPerson.gender == Gender.Male) ? mCurrentPerson : p;
            Person woman = (mCurrentPerson.gender == Gender.Male) ? p : mCurrentPerson;

            for (int i = 0; i < mTempMarriages.Count; i++)
            {
                if (mTempMarriages[i].husband == man.id && mTempMarriages[i].wife == woman.id)
                {
                    mTempMarriages[i].info.date = tbMarriageDate.Text.Trim();
                    mTempMarriages[i].info.place = tbMarriageLocation.Text.Trim();
                    mTempMarriages[i].info.note = tbMarriageNotes.Text.Trim();
                    break;
                }
            }

            lbSpouseAdd.ForeColor = SystemColors.ControlText;
        }

        /// <summary>
        /// The selected spouse has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSpouses_SelectedIndexChanged(object sender, EventArgs e)
        {
            Person p = (Person)lbSpouses.SelectedItem;

            if (p == null)
            {
                return;
            }

            Guid manId = (mCurrentPerson.gender == Gender.Male) ? mCurrentPerson.id : p.id;
            Guid wifeId = (mCurrentPerson.gender == Gender.Male) ? p.id : mCurrentPerson.id;

            Marriage m = mDB.GetMarriage(manId, wifeId);

            if (m == null)
            {
                return;
            }

            cmbMarriageSpouse.SelectedItem = p;

            if (m.info == null)
            {
                return;
            }

            tbMarriageDate.Text = m.info.date;
            tbMarriageLocation.Text = m.info.place;
            tbMarriageNotes.Text = m.info.note;

            lbSpouseAdd.ForeColor = SystemColors.InactiveCaption;
        }

        /// <summary>
        /// User has selected another spouse in the combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSpouse_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// User has pressed a key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle special keys
            if (e.KeyCode == Keys.Escape && this.panelPerson.Visible == true)
            {
                HidePersonPage();
                lbPersons.Enabled = true;
                return;
            }

            // Handle key shortcuts
            if (Control.ModifierKeys == Keys.Alt)
            {
                switch (e.KeyCode) {
                    case Keys.A:
                        HandleAddPerson();
                        break;
                    case Keys.E:
                        HandleEditPerson(false);
                        break;
                    case Keys.R:
                        HandleRemovePerson(false);
                        break;
                    case Keys.N:
                        HandleNewDatabase();
                        break;
                    case Keys.O:
                        HandleLoadDatabase();
                        break;
                    case Keys.S:
                        SavePersonsToFile();
                        break;
                    case Keys.X:
                        HandleExitApplication();
                        break;
                }
            }
        }

        /// <summary>
        /// Selected person has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbPersons_SelectedIndexChanged(object sender, EventArgs e)
        {
            Person person = (Person)lbPersons.SelectedItem;

            if (person == null)
            {
                return;
            }

            DrawTree(person);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // If we are displaying graphics, find the rectangle under the mouse
            if (mGraphics == null)
            {
                return;
            }

            // Right mouse button is allowed
            if (e.Button != MouseButtons.Right) return;

            // Find the node under the mouse.
            var node = mTree.SelectNode(e.Location);
                
            // If there is a node under the mouse,
            // display the context menu.
            if (node != null)
            {
                // Display the context menu.
                ctxMenu.Show(this, e.Location);
            }

        }

        /// <summary>
        /// Draws the person's tree
        /// </summary>
        /// <param name="person"></param>
        private void DrawTree(Person person)
        {
            if (mGraphics != null)
            {
                mGraphics.Clear(System.Drawing.Color.White);
            }
            else
            {
                mGraphics = this.CreateGraphics();
                mGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                mGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            }

            int xmin; int ymin;

            xmin = lbPersons.Location.X + (this.Width - lbPersons.Location.X) / 2;
            ymin = lbPersons.Location.Y;

            // Create a new tree (the panel containing person information for, and align to to listbox containing persons.
            mTree = new GenericTree.GenericTree(mGraphics, panelPerson.Location.X, lbPersons.Location.Y, panelPerson.Width, panelPerson.Height, this.BackColor);

            mTree.SetFont(new FontFamily("Segoe UI"), FontStyle.Regular, 13, 11);

            bool hasParents = false;
            bool hasChildren = false;

            //
            // Add person's parents
            //
            if (person.father != Guid.Empty)
            {
                // Got either mother or father or both.
                hasParents = true;

                Person p = mDB.GetPerson(person.father);

                if (p != null)
                {
                    mTree.AddNode(0, AssembleNodeString(p), p, false, true);
                }
            }

            if (person.mother != Guid.Empty)
            {
                hasParents = true;

                Person p = mDB.GetPerson(person.mother);

                if (p != null)
                {
                    mTree.AddNode(0, AssembleNodeString(p), p, false, true);
                }
            }

            //// Add main person
            hasChildren = true;
            
            if (person.children == null || person.children.Count == 0)
            {
                hasChildren = false;
            }

            mTree.AddNode(1, AssembleNodeString(person), person, hasParents, hasChildren);

            // Add spouse
            if (person.marriages != null && person.marriages.Count > 0)
            {
                Person spouse = null;

                Marriage m = mDB.GetMarriage(person.marriages[0]);

                if (person.gender == Gender.Male)
                {
                    spouse = mDB.GetPerson(m.wife);
                }
                else
                {
                    spouse = mDB.GetPerson(m.husband);
                }

                hasChildren = true;
                if (spouse.children == null || spouse.children.Count == 0)
                {
                    hasChildren = false;
                }


                mTree.AddNode(1, AssembleNodeString(spouse), spouse, false, hasChildren);
            }

            // Add  person's children
            if (person.children != null)
            {
                var sorted = mDB.GetPersonListSortedByBirth(person.children);

                foreach (var c in sorted)
                {
                    mTree.AddNode(2, AssembleNodeString(c), c, true, false);
                }              
            }


            mTree.Draw();
        }


        private String[] AssembleNodeString(Person p)
        {
            String[] txt = new String[3] { "", "", "" };

            if (p.firstName != "")
            {
                txt[0] += p.firstName;
            }
            if (p.lastName != "")
            {
                txt[0] += " " + p.lastName;
            }

            txt[1] += "b. ";
            if (p.birth.date != "")
            {
                txt[1] += p.birth.date;
            }
            if (p.birth.place != "")
            {
                txt[1] += " " + p.birth.place;
            }

            txt[2] += "d. ";
            if (p.death.date != "")
            {
                txt[2] += p.death.date;
            }
            if (p.death.place != "")
            {
                txt[2] += " " + p.death.place;
            }

            return txt;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region Context Menu
        //-----------------------------------------------------------------------
        // CONTEXT MENU
        //-----------------------------------------------------------------------
        private void ctxMenuSelect_Click(object sender, EventArgs e)
        {
            Person p = (Person)mTree.GetSelectedNode().GetUserData();

            DrawTree(p);
        }

        private void ctxMenuEdit_Click(object sender, EventArgs e)
        {
            HandleEditPerson(true);
        }

        private void ctxMenuRemove_Click(object sender, EventArgs e)
        {
            HandleRemovePerson(true);
        }

        private void ctxMenuPrint_Click(object sender, EventArgs e)
        {
            // Get person under the mouse
            mCurrentPerson = (Person)mTree.GetSelectedNode().GetUserData();
            mPrint.PrintPerson(mCurrentPerson, mDB);
        }
        #endregion // Context Menu

        private void lbPersons_MouseDown(object sender, MouseEventArgs e)
        {
            // Right mouse button is allowed
            if (e.Button != MouseButtons.Right) return;

            var lb = (ListBox)sender;

            if (lb.SelectedIndex == -1)
            {
                // Nothing selected
                return;
            }

            var person = (Person)lb.Items[lb.SelectedIndex];

            if (mTree.SelectNode(person) != null)
            {
                // Display the context menu.
                ctxMenu.Show(this, e.Location);
            }

        }

        private void ctxMenu_NewSon(object sender, EventArgs e)
        {

        }

        private void ctxMenu_NewDaughter(object sender, EventArgs e)
        {

        }

        private void ctxMenu_NewBrother(object sender, EventArgs e)
        {

        }

        private void ctxMenu_NewSister(object sender, EventArgs e)
        {

        }

    }

}
