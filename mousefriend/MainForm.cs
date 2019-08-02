﻿using System;
using System.IO;
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
            using (FileStream fs = File.Open("settings.dat", FileMode.Create))
            {
                using (BinaryWriter w = new BinaryWriter(fs))
                {
                    // Button type.
                    if (rdbClickSingleLeft.Checked)
                    {
                        w.Write((byte)1);
                    }
                    else if (rdbClickSingleMiddle.Checked)
                    {
                        w.Write((byte)2);
                    }
                    else if (rdbClickSingleRight.Checked)
                    {
                        w.Write((byte)3);
                    }
                    else if (rdbClickDoubleLeft.Checked)
                    {
                        w.Write((byte)4);
                    }
                    else if (rdbClickDoubleMiddle.Checked)
                    {
                        w.Write((byte)5);
                    }
                    else if (rdbClickDoubleRight.Checked)
                    {
                        w.Write((byte)6);
                    }

                    // Location info
                    if (rdbLocationFixed.Checked)
                    {
                        w.Write((byte)1);
                    }
                    else if (rdbLocationMouse.Checked)
                    {
                        w.Write((byte)2);
                    }
                    else if (rdbLocationRandom.Checked)
                    {
                        w.Write((byte)3);
                    }
                    else if (rdbLocationRandomArea.Checked)
                    {
                        w.Write((byte)4);
                    }

                    w.Write((int)numFixedX.Value);
                    w.Write((int)numFixedY.Value);
                    w.Write((int)numRandomX.Value);
                    w.Write((int)numRandomY.Value);
                    w.Write((int)numRandomWidth.Value);
                    w.Write((int)numRandomHeight.Value);

                    // Delay info
                    if (rdbDelayFixed.Checked)
                    {
                        w.Write((byte)1);
                    }
                    else if (rdbDelayRange.Checked)
                    {
                        w.Write((byte)2);
                    }

                    w.Write((int)numDelayFixed.Value);
                    w.Write((int)numDelayRangeMin.Value);
                    w.Write((int)numDelayRangeMax.Value);

                    // Count info
                    if (rdbCount.Checked)
                    {
                        w.Write((byte)1);
                    }
                    else if (rdbUntilStopped.Checked)
                    {
                        w.Write((byte)2);
                    }

                    w.Write((int)numCount.Value);

                    // Hotkey info
                    w.Write((int)hotkey);
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.dat"))
            {
                using (FileStream fs = File.Open("settings.dat", FileMode.Open))
                {
                    using (BinaryReader r = new BinaryReader(fs))
                    {
                        byte buttonType = r.ReadByte();

                        byte locationType = r.ReadByte();
                        int fixedX = r.ReadInt32();
                        int fixedY = r.ReadInt32();
                        int randomX = r.ReadInt32();
                        int randomY = r.ReadInt32();
                        int randomWidth = r.ReadInt32();
                        int randomHeight = r.ReadInt32();

                        byte delayType = r.ReadByte();
                        int fixedDelay = r.ReadInt32();
                        int rangeDelayMin = r.ReadInt32();
                        int rangeDelayMax = r.ReadInt32();

                        byte countType = r.ReadByte();
                        int count = r.ReadInt32();

                        hotkey = (Keys)r.ReadInt32();

                        switch (buttonType)
                        {
                            case 1:
                                rdbClickSingleLeft.Checked = true;
                                break;
                            case 2:
                                rdbClickSingleMiddle.Checked = true;
                                break;
                            case 3:
                                rdbClickSingleRight.Checked = true;
                                break;
                            case 4:
                                rdbClickDoubleLeft.Checked = true;
                                break;
                            case 5:
                                rdbClickDoubleMiddle.Checked = true;
                                break;
                            case 6:
                                rdbClickDoubleRight.Checked = true;
                                break;
                        }

                        switch (locationType)
                        {
                            case 1:
                                rdbLocationFixed.Checked = true;
                                break;
                            case 2:
                                rdbLocationMouse.Checked = true;
                                break;
                            case 3:
                                rdbLocationRandom.Checked = true;
                                break;
                            case 4:
                                rdbLocationRandomArea.Checked = true;
                                break;
                        }

                        numFixedX.Value = fixedX;
                        numFixedY.Value = fixedY;
                        numRandomX.Value = randomX;
                        numRandomY.Value = randomY;
                        numRandomWidth.Value = randomWidth;
                        numRandomHeight.Value = randomHeight;

                        switch (delayType)
                        {
                            case 1:
                                rdbDelayFixed.Checked = true;
                                break;
                            case 2:
                                rdbDelayRange.Checked = true;
                                break;
                        }

                        numDelayFixed.Value = fixedDelay;
                        numDelayRangeMin.Value = rangeDelayMin;
                        numDelayRangeMax.Value = rangeDelayMax;

                        switch (countType)
                        {
                            case 1:
                                rdbCount.Checked = true;
                                break;
                            case 2:
                                rdbUntilStopped.Checked = true;
                                break;
                        }

                        numCount.Value = count;

                        if (hotkey != Keys.None)
                        {
                            var hotkeyMods = hotkey & Keys.Modifiers;
                            this.hotkeyModifiers = 0;
                            if ((hotkeyMods & Keys.Shift) != 0)
                            {
                                this.hotkeyModifiers |= Win32.fsModifiers.Shift;
                            }
                            if ((hotkeyMods & Keys.Control) != 0)
                            {
                                this.hotkeyModifiers |= Win32.fsModifiers.Control;
                            }
                            if ((hotkeyMods & Keys.Alt) != 0)
                            {
                                this.hotkeyModifiers |= Win32.fsModifiers.Alt;
                            }

                            SetHotkey();
                        }
                    }
                }
            }
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
            btnToggle.Focus();
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
