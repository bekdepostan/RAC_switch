using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenNETCF.Win32;

namespace OpenNETCF.Windows.Forms
{
    /// <summary>
    /// Extends the <see cref="DateTimePicker"/> control.
    /// </summary>
    public class DateTimePicker2 : DateTimePicker, IWin32Window
    {
        private DateTimePickerNativeWindow nativeWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePicker2"/> class.
        /// </summary>
        public DateTimePicker2()
        {
            if (!StaticMethods.IsDesignMode(this))
            {
                nativeWindow = new DateTimePickerNativeWindow(this);
            }
        }

        /// <summary>
        /// Occurs when the drop-down calendar is dismissed and disappears.
        /// </summary>
        public event EventHandler CloseUp;

        /// <summary>
        /// Raises the <see cref="CloseUp"/> event.
        /// </summary>
        /// <param name="eventargs">An <see cref="EventArgs"/> that contains the event data.</param>
        protected internal void OnCloseUp(EventArgs eventargs)
        {
            if (CloseUp != null)
            {
                this.CloseUp(this, eventargs);
            }
        }

        /// <summary>
        /// Occurs when the drop-down calendar is shown.
        /// </summary>
        public event EventHandler DropDown;

        /// <summary>
        /// Raises the <see cref="DropDown"/> event.
        /// </summary>
        /// <param name="eventargs">An <see cref="EventArgs"/> that contains the event data.</param>
        protected internal void OnDropDown(EventArgs eventargs)
        {
            if (DropDown != null)
            {
                this.DropDown(this, eventargs);
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (!StaticMethods.IsDesignMode(this))
            {
                if (this.Parent != null)
                {
                    nativeWindow.AssignHandle(this.Parent.Handle);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!StaticMethods.IsDesignMode(this))
            {
                nativeWindow.ReleaseHandle();
            }

            base.Dispose(disposing);
        }
    }

    internal class DateTimePickerNativeWindow : NativeWindow
    {
        private DateTimePicker2 parent;

        internal DateTimePickerNativeWindow(DateTimePicker2 parent)
        {
            this.parent = parent;
        }
        protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message m)
        {
            if (m.Msg == (int)WM.NOTIFY)
            {
                //marshal notification data into NMHDR struct
                NMHDR hdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));

                if (hdr.hwndFrom == parent.Handle)
                {
                    switch (hdr.code)
                    {
                        //definite selection
                        case (int)DTN.DROPDOWN:
                            //raise dropdown event
                            parent.OnDropDown(EventArgs.Empty);
                            break;
                        case (int)DTN.CLOSEUP:
                            //raise closeup event
                            parent.OnCloseUp(EventArgs.Empty);
                            break;
                    }
                }
            }

            base.WndProc(ref m);
        }
    }
}
