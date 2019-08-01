using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mousefriend
{
    public partial class MainForm : Form
    {
        private Clicker clicker;
        private Keys hotkey;
        private Win32.fsModifiers hotkeyModifiers;

        public MainForm()
        {
            InitializeComponent();
        }

        private void SaveSettings()
        {

        }

        private void LoadSettings()
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            clicker = new Clicker();
            LoadSettings();
            ClickTypeHandler(null, null);
            LocationHandler(null, null);
            DelayHandler(null, null);
            CountHandler(null, null);

            clicker.Finished += HandleFinished;
        }

        private void HandleFinished(object sender, EventArgs e)
        {
            EnableControls();
        }

        private void ClickTypeHandler(object sender, EventArgs e)
        {
            Clicker.ButtonType buttonType;
            bool doubleClick = false;

            if (rdbClickSingleLeft.Checked || rdbClickDoubleLeft.Checked)
            {
                buttonType = Clicker.ButtonType.Left;
            }
            else if (rdbClickSingleMiddle.Checked || rdbClickDoubleMiddle.Checked)
            {
                buttonType = Clicker.ButtonType.Middle;
            }
            else
            {
                buttonType = Clicker.ButtonType.Right;
            }

            if (rdbClickDoubleLeft.Checked || rdbClickDoubleMiddle.Checked || rdbClickDoubleRight.Checked)
            {
                doubleClick = true;
            }

            clicker.UpdateButton(buttonType, doubleClick);
        }

        private void LocationHandler(object sender, EventArgs e)
        {
            Clicker.LocationType locationType;
            int x = -1;
            int y = -1;
            int width = -1;
            int height = -1;

            if (rdbLocationFixed.Checked)
            {
                locationType = Clicker.LocationType.Fixed;
                x = (int)numFixedX.Value;
                y = (int)numFixedY.Value;
            }
            else if (rdbLocationMouse.Checked)
            {
                locationType = Clicker.LocationType.Cursor;
            }
            else if (rdbLocationRandom.Checked)
            {
                locationType = Clicker.LocationType.Random;
            }
            else
            {
                locationType = Clicker.LocationType.RandomRange;
                x = (int)numRandomX.Value;
                y = (int)numRandomY.Value;
                width = (int)numRandomWidth.Value;
                height = (int)numRandomHeight.Value;
            }

            // Toggle visibility of controls.
            if (locationType == Clicker.LocationType.Fixed)
            {
                numFixedX.Enabled = true;
                numFixedY.Enabled = true;
            }
            else
            {
                numFixedX.Enabled = false;
                numFixedY.Enabled = false;
            }

            if (locationType == Clicker.LocationType.RandomRange)
            {
                numRandomX.Enabled = true;
                numRandomY.Enabled = true;
                numRandomWidth.Enabled = true;
                numRandomHeight.Enabled = true;
                btnSelect.Enabled = true;
            }
            else
            {
                numRandomX.Enabled = false;
                numRandomY.Enabled = false;
                numRandomWidth.Enabled = false;
                numRandomHeight.Enabled = false;
                btnSelect.Enabled = false;
            }

            clicker.UpdateLocation(locationType, x, y, width, height);
        }

        private void DelayHandler(object sender, EventArgs e)
        {
            Clicker.DelayType delayType;
            int delay = -1;
            int delayRange = -1;

            if (rdbDelayFixed.Checked)
            {
                delayType = Clicker.DelayType.Fixed;
                delay = (int)numDelayFixed.Value;
            }
            else
            {
                delayType = Clicker.DelayType.Range;
                delay = (int)numDelayRangeMin.Value;
                delayRange = (int)numDelayRangeMax.Value;
            }

            // Toggle visibility of controls.
            if (delayType == Clicker.DelayType.Fixed)
            {
                numDelayFixed.Enabled = true;
                numDelayRangeMax.Enabled = false;
                numDelayRangeMin.Enabled = false;
            }
            else
            {
                numDelayFixed.Enabled = false;
                numDelayRangeMax.Enabled = true;
                numDelayRangeMin.Enabled = true;
            }

            clicker.UpdateDelay(delayType, delay, delayRange);
        }

        private void CountHandler(object sender, EventArgs e)
        {
            Clicker.CountType countType;
            int count = -1;

            if (rdbCount.Checked)
            {
                countType = Clicker.CountType.Fixed;
                count = (int)numCount.Value;
            }
            else
            {
                countType = Clicker.CountType.UntilStopped;
            }

            // Toggle visibility of controls.
            if (countType == Clicker.CountType.Fixed)
            {
                numCount.Enabled = true;
            }
            else
            {
                numCount.Enabled = false;
            }

            clicker.UpdateCount(countType, count);
        }

        delegate void SetEnabledCallback(Control Control, bool Enabled);
        private void SetEnabled(Control Control, bool Enabled)
        {
            if (Control.InvokeRequired)
            {
                var d = new SetEnabledCallback(SetEnabled);
                this.Invoke(d, Control, Enabled);
            }
            else
            {
                Control.Enabled = Enabled;
            }
        }

        private void btnHotkeyRemove_Click(object sender, EventArgs e)
        {
            UnsetHotkey();
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (!clicker.IsAlive)
            {
                clicker.Start();
                DisableControls();
            }
            else
            {
                clicker.Stop();
                EnableControls();
            }
        }

        delegate void SetButtonTextCallback(Button Control, string Text);
        private void SetButtonText(Button Control, string Text)
        {
            if (Control.InvokeRequired)
            {
                var d = new SetButtonTextCallback(SetButtonText);
                this.Invoke(d, Control, Text);
            }
            else
            {
                Control.Text = Text;
            }
        }

        private void EnableControls()
        {
            tslStatus.Text = "Ready...";
            SetEnabled(grpClickType, true);
            SetEnabled(grpLocation, true);
            SetEnabled(grpDelay, true);
            SetEnabled(grpCount, true);
            SetButtonText(btnToggle, "Start");
        }

        private void DisableControls()
        {
            tslStatus.Text = "Clicking...";
            SetEnabled(grpClickType, false);
            SetEnabled(grpLocation, false);
            SetEnabled(grpDelay, false);
            SetEnabled(grpCount, false);
            SetButtonText(btnToggle, "Stop");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == Win32.WM_HOTKEY)
            {
                // Ignore the hotkey if the user is editing it.
                if (txtHotkey.Focused)
                {
                    return;
                }

                Win32.fsModifiers modifiers = (Win32.fsModifiers)((int)m.LParam & 0xFFFF);
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                if (key == (hotkey & Keys.KeyCode) && modifiers == hotkeyModifiers)
                {
                    btnToggle_Click(null, null);
                }
            }
        }

        private void txtHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            // Don't want to do anything if only a modifier key is pressed.
            //     Modifiers                                 Asian keys (kana, hanja, kanji etc)       IME related keys (convert etc)           Korean alt (process)  Windows keys
            if (!((e.KeyValue >= 16 && e.KeyValue <= 18) || (e.KeyValue >= 21 && e.KeyValue <= 25) || (e.KeyValue >= 28 && e.KeyValue <= 31) || e.KeyValue == 229 || (e.KeyValue >= 91 && e.KeyValue <= 92)))
            {
                Win32.UnregisterHotKey(this.Handle, (int)hotkey);
                hotkey = e.KeyData;
                // Extract modifiers
                hotkeyModifiers = 0;
                if ((e.Modifiers & Keys.Shift) != 0)
                {
                    hotkeyModifiers |= Win32.fsModifiers.Shift;
                }
                if ((e.Modifiers & Keys.Control) != 0)
                {
                    hotkeyModifiers |= Win32.fsModifiers.Control;
                }
                if ((e.Modifiers & Keys.Alt) != 0)
                {
                    hotkeyModifiers |= Win32.fsModifiers.Alt;
                }

                SetHotkey();
            }
        }

        private void SetHotkey()
        {
            txtHotkey.Text = KeysConverter.Convert(hotkey);
            Win32.RegisterHotKey(this.Handle, (int)hotkey, (uint)hotkeyModifiers, (uint)(hotkey & Keys.KeyCode));
            btnHotkeyRemove.Enabled = true;
        }

        private void UnsetHotkey()
        {
            Win32.UnregisterHotKey(this.Handle, (int)hotkey);
            btnHotkeyRemove.Enabled = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        public void SendRectangle(int X, int Y, int Width, int Height)
        {
            numRandomX.Value = X;
            numRandomY.Value = Y;
            numRandomWidth.Value = Width;
            numRandomHeight.Value = Height;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var form = new SelectionForm(this);
            form.Show();
        }
    }
}
