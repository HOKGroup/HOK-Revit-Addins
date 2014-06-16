using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using HOK.RoomsToMass.ToMass;
using System.IO;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass
{
    public partial class Form_Command : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Autodesk.Revit.DB.Document m_doc;
        private string userSelect = "";

        private bool roomSelected = false;
        private bool areaSelected = false;
        private bool floorSelected = false;

        private bool roomExist = false;
        private bool areaExist = false;
        private bool floorExist = false;

        public string UserSelect { get { return userSelect; } set { userSelect = value; } }

        public bool RoomSelected { get { return roomSelected; } set { roomSelected = value; } }
        public bool AreaSelected { get { return areaSelected; } set { areaSelected = value; } }
        public bool FloorSelected { get { return floorSelected; } set { floorSelected = value; } }

        public bool RoomExist { get { return roomExist; } set { roomExist = value; } }
        public bool AreaExist { get { return areaExist; } set { areaExist = value; } }
        public bool FloorExist { get { return floorExist; } set { floorExist = value; } }

        public Form_Command(UIApplication application)
        {
            m_app = application;
            m_doc = application.ActiveUIDocument.Document;
            InitializeComponent();
        }

        private void Form_Command_Load(object sender, EventArgs e)
        {
            bttnRoom.Enabled = roomExist;
            bttnArea.Enabled = areaExist;
            bttnFloor.Enabled = floorExist;

            checkBoxRooms.Enabled = roomSelected;
            checkBoxAreas.Enabled = areaSelected;
            checkBoxFloors.Enabled = floorSelected;
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnRoom_Click(object sender, EventArgs e)
        {
            if (FindMasterPath())
            {
                if (checkBoxRooms.Checked)
                {
                    userSelect = "SelectedRooms";
                }
                else
                {
                    userSelect = "Rooms";
                }
                
                this.DialogResult = DialogResult.OK;
            }
        }

        private void bttnArea_Click(object sender, EventArgs e)
        {
            if (FindMasterPath())
            {
                if (checkBoxAreas.Checked)
                {
                    userSelect = "SelectedAreas";
                }
                else
                {
                    userSelect = "Areas";
                }
                
                this.DialogResult = DialogResult.OK;
            }
        }

        private void bttnFloor_Click(object sender, EventArgs e)
        {
            if (FindMasterPath())
            {
                if (checkBoxFloors.Checked)
                {
                    userSelect = "SelectedFloors";
                }
                else
                {
                    userSelect = "Floors";
                }
                
                this.DialogResult = DialogResult.OK;
            }
        }

        private bool FindMasterPath()
        {
            bool found = false;
            string masterFilePath = "";
            if (m_doc.IsWorkshared)
            {
                ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
                masterFilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                if (string.IsNullOrEmpty(masterFilePath))
                {
                    masterFilePath = m_doc.PathName;
                }
            }
            else
            {
                masterFilePath = m_doc.PathName;
            }

            if (!string.IsNullOrEmpty(masterFilePath))
            {
                found = true;
            }
            else
            {
                MessageBox.Show("Please save the current Revit project before running the tool.", "File Not Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                found = false;
            }
            return found;
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string htmPath = @"V:\RVT-Data\HOK Program\Documentation\Mass Tool_Instruction.pdf";
            System.Diagnostics.Process.Start(htmPath);
        }

        private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        
    }
}
