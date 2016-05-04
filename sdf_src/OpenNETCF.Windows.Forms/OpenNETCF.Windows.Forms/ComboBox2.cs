using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.Win32;
using OpenNETCF.Runtime.InteropServices;
using System.Runtime.InteropServices;

namespace OpenNETCF.Windows.Forms
{
    public partial class ComboBox2 : ComboBox, IWin32Window
    {
        public ComboBox2(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private int m_ddwidth = -1;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ComboBox2()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentChanged(EventArgs e)
        {
            if (this.Parent != null)
            {
                //update dropdown width
                this.DropDownWidth = m_ddwidth;
            }
            base.OnParentChanged(e);
        }



        /// <summary>
        /// Finds the first item in the combo box that starts with the specified string.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/> to search for.</param>
        /// <returns>The zero-based index of the first item found; returns -1 if no match is found.</returns>
        public int FindString(string s)
        {
            return FindString(s, -1);
        }

        /// <summary>
        /// Finds the first item in the combo box that starts with the specified string.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/> to search for.</param>
        /// <param name="startIndex">The zero-based index of the item before the first item to be searched. Set to -1 to search from the beginning of the control.</param>
        /// <returns>The zero-based index of the first item found; returns -1 if no match is found.</returns>
        public int FindString(string s, int startIndex)
        {
            if (StaticMethods.IsDesignMode(this))
                return -1;

            int index = -1;
            IntPtr pString = Marshal2.StringToHGlobalUni(s);
            index = (int)Win32Window.SendMessage(this.Handle, (int)CB.FINDSTRING, startIndex, pString);
            Marshal.FreeHGlobal(pString);
            return index;
        }

        /// <summary>
        /// Finds the first item in the combo box that matches the specified string.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/> to search for.</param>
        /// <returns>The zero-based index of the first item found; returns -1 if no match is found.</returns>
        public int FindStringExact(string s)
        {
            return FindStringExact(s, -1);
        }

        /// <summary>
        /// Finds the first item after the specified index that matches the specified string.
        /// </summary>
        /// <param name="s">The <see cref="System.String"/> to search for.</param>
        /// <param name="startIndex">The zero-based index of the item before the first item to be searched. Set to -1 to search from the beginning of the control.</param>
        /// <returns>The zero-based index of the first item found; returns -1 if no match is found.</returns>
        public int FindStringExact(string s, int startIndex)
        {
            if (StaticMethods.IsDesignMode(this))
                return -1;
            int index = -1;
            IntPtr pString = Marshal2.StringToHGlobalUni(s);
            index = (int)Win32Window.SendMessage(this.Handle, (int)CB.FINDSTRINGEXACT, startIndex, pString);
            Marshal.FreeHGlobal(pString);
            return index;
        }


        /// <summary>
        /// Gets or sets the width of the of the drop-down portion of a combo box.
        /// </summary>
        public int DropDownWidth
        {
            get
            {
                if (StaticMethods.IsDesignMode(this))
                {
                    return this.m_ddwidth;
                }

                m_ddwidth = (int)Win32Window.SendMessage(this.Handle, (int)CB.GETDROPPEDWIDTH, 0, 0);
                if (m_ddwidth == -1)
                {
                    return this.Width;
                }
                return m_ddwidth;
            }
            set
            {
                m_ddwidth = value;
                if (StaticMethods.IsDesignMode(this))
                    return;
                Win32Window.SendMessage(this.Handle, (int)CB.SETDROPPEDWIDTH, m_ddwidth, 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the combo box is displaying its drop-down portion.
        /// </summary>
        public bool DroppedDown
        {
            get
            {
                if (StaticMethods.IsDesignMode(this))
                    return false;
                return Win32Window.SendMessage(this.Handle, (int)CB.GETDROPPEDSTATE, 0, 0) == IntPtr.Zero ? false : true;
                
            }
        }

        /// <summary>
        /// Show or hides the list of the combobox that has the DropDown or DropDownList
        /// </summary>
        public bool DropDown
        {
            set
            {
                if (StaticMethods.IsDesignMode(this))
                    return;
                Win32Window.SendMessage(this.Handle, (int)CB.SHOWDROPDOWN, value ? 1 : 0, 0);
            }
        }
    }
}
