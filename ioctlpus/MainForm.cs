using Be.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static ioctlpus.Utilities.IOCTL;
using static ioctlpus.Utilities.NativeMethods;

namespace ioctlpus
{
    public partial class MainForm : Form
    {
        private List<Request> requests = new List<Request>();
        private Utilities.CRC16 CRC16 = new Utilities.CRC16();

        public MainForm()
        {
            InitializeComponent();

            // Add placeholder text to filters textbox
            SendMessage(tbFilters.Handle, EM_SETCUEBANNER, 0, "Filters (e.g. 9C412000 br=64 !ec=C000000D)");

            // Setup HexBoxes
            InitializeHexBox(hbInput, (int)nudInputSize.Value);
            InitializeHexBox(hbOutput, (int)nudOutputSize.Value);

            // Setup TreeListView
            InitializeTreeListView();

            // Setup initial parameters
            tbDevicePath.Text = @"\\.\PhysicalDrive0";
            tbIOCTL.Text = "70000";
            tbAccessMask.Text = "20000000";
            cmbACL.SelectedItem = "ANY_ACCESS";
        }

        // Initialize the given buffer view
        private void InitializeHexBox(HexBox hexBox, int size)
        {
            hexBox.Visible = true;
            hexBox.InfoForeColor = System.Drawing.Color.Black;
            hexBox.UseFixedBytesPerLine = true;
            hexBox.BytesPerLine = 8;
            hexBox.ColumnInfoVisible = true;
            hexBox.LineInfoVisible = true;
            hexBox.StringViewVisible = true;
            hexBox.VScrollBarVisible = true;
            hexBox.GroupSeparatorVisible = true;
            hexBox.GroupSize = 4;
            hexBox.ShadowSelectionVisible = true;
            hexBox.ByteProvider = new DynamicByteProvider(new byte[size]);
        }

        // Initialize the Request History views
        private void InitializeTreeListView()
        {
            // Add colours to request rows
            tlvRequestHistory.FormatRow += (sender, eventArgs) =>
            {
                Request transmission = (Request)eventArgs.Model;
                if (transmission.IsFavourite)
                    eventArgs.Item.BackColor = Color.LightYellow;
                else if (transmission.ReturnValue > 0)
                    eventArgs.Item.BackColor = Color.MistyRose;
            };

            // Rename requests when double-clicked or F2 is pressed
            tlvRequestHistory.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;

            // How to identify if a row has children
            tlvRequestHistory.CanExpandGetter = delegate (Object tx)
            {
                return ((Request)tx).Children.Count > 0;
            };

            // Where row children are located
            tlvRequestHistory.ChildrenGetter = delegate (Object tx)
            {
                return ((Request)tx).Children;
            };

            // Populate HexBoxes when the selection changes
            tlvRequestHistory.SelectionChanged += (sender, eventArgs) =>
            {
                if (tlvRequestHistory.SelectedIndex == -1) return;

                Request tx = (Request)tlvRequestHistory.SelectedObject;

                tbDevicePath.Text = tx.DevicePath;
                tbIOCTL.Text = tx.IOCTL.ToString("X");
                nudInputSize.Value = tx.PreCallInput.Length;
                nudOutputSize.Value = tx.PostCallOutput.Length;

                DynamicByteProvider dbpDataInput = new DynamicByteProvider(tx.PreCallInput);
                hbInput.ByteProvider = dbpDataInput;

                DynamicByteProvider dbpDataOutput = new DynamicByteProvider(tx.PostCallOutput);
                hbOutput.ByteProvider = dbpDataOutput;
            };
        }

        // Validate provided DeviceName
        private void tbDevicePath_TextChanged(object sender, EventArgs e)
        {
            Guid guid;
            if (Guid.TryParse(tbDevicePath.Text, out guid))
            {
                Point toolTipCoords = tbDevicePath.Location;
                //toolTipCoords.X += 20;
                //toolTipCoords.Y -= 4;

                try
                {
                    string devicePath = ResolveDeviceInstanceGUID(guid);
                    tbDevicePath.Text = devicePath;
                    toolTip.Show("Resolved device instance GUID to symbolic link.", tbDevicePath, toolTipCoords, 3000);
                }
                catch (ArgumentException exception)
                {
                    toolTip.Show(exception.Message, tbDevicePath, toolTipCoords, 5000);
                }
            }

            if (IsValidDevicePath(tbDevicePath.Text))
                tbDevicePath.BackColor = Color.Honeydew;
            else
                tbDevicePath.BackColor = Color.MistyRose;
        }

        // Validate that provided IOCTL is legitimate
        private void tbIOCTL_TextChanged(object sender, EventArgs e)
        {
            Point toolTipCoords = tbIOCTL.Location;
            toolTipCoords.X -= 400;
            toolTipCoords.Y += 5;

            uint ioctl;
            if (!UInt32.TryParse(tbIOCTL.Text, System.Globalization.NumberStyles.HexNumber, null, out ioctl))
            {
                tbIOCTL.BackColor = Color.MistyRose;
                btnSend.Enabled = false;
                toolTip.Show("IOCTL codes must be in hexadecimal format.", tbIOCTL, toolTipCoords, 3000);
            }
            else
            {
                tbIOCTL.BackColor = Color.Honeydew;
                btnSend.Enabled = true;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            uint fa_mask = Convert.ToUInt32("20000000", 16);
            // if Access mask is enabled use that value
            if (tbAccessMask.Enabled == true)
            {
                fa_mask = Convert.ToUInt32(tbAccessMask.Text, 16);
            }
            else
            {
                // otherwise gather the human readable ACL
                switch (cmbACL.SelectedItem)
                {
                    case "ANY_ACCESS":
                        fa_mask = 0;
                        break;
                    case "READ_WRITE_DATA":
                        fa_mask = Convert.ToUInt32(FileAccess.ReadWrite);
                        break;
                    case "READ_DATA":
                        fa_mask = Convert.ToUInt32(FileAccess.Read);
                        break;
                    case "WRITE_DATA":
                        fa_mask = Convert.ToUInt32(FileAccess.Write);
                        break;
                    default:
                        fa_mask = 0;
                        break;

                }
            }
            Debug.WriteLine("\nAccess Mask: " + fa_mask);

            SafeFileHandle sfh = CreateFile(
            tbDevicePath.Text.Trim(),
            dwDesiredAccess: (FileAccess)fa_mask,
            dwShareMode: FileShare.ReadWrite,
            lpSecurityAttributes: IntPtr.Zero,
            dwCreationDisposition: FileMode.Open,
            dwFlagsAndAttributes: FileAttributes.Normal,
            hTemplateFile: IntPtr.Zero);

            int errorCode = 0;

            uint ioctl = Convert.ToUInt32(tbIOCTL.Text.Trim(), 16);
            uint returnedBytes = 0;
            uint inputSize = (uint)nudInputSize.Value;
            uint outputSize = (uint)nudOutputSize.Value;
            byte[] outputBuffer = new byte[outputSize];
            byte[] inputBuffer = new byte[inputSize];


            if (sfh.IsInvalid)
            {
                // invalid DeviceName
                errorCode = Marshal.GetLastWin32Error();
            }
            else
            {
                // create buffer with specified content in memory
                long hbInputLength = ((DynamicByteProvider)hbInput.ByteProvider).Length;
                MemSet(Marshal.UnsafeAddrOfPinnedArrayElement(inputBuffer, 0), 0, (int)hbInputLength);

                for (int i = 0; i < inputSize; i++)
                    if (i < hbInputLength)
                        inputBuffer[i] = ((DynamicByteProvider)hbInput.ByteProvider).ReadByte(i);
                    else
                        inputBuffer[i] = 0;

                long hbOutputLength = ((DynamicByteProvider)hbOutput.ByteProvider).Length;
                MemSet(Marshal.UnsafeAddrOfPinnedArrayElement(outputBuffer, 0), 0, (int)hbOutputLength);
                // execute DeviceIoControl request
                DeviceIoControl(sfh, ioctl, inputBuffer, inputSize, outputBuffer, outputSize, ref returnedBytes, IntPtr.Zero);
                // gather error code
                errorCode = Marshal.GetLastWin32Error();
                sfh.Close();
                // update Input/Output views
                DynamicByteProvider requestData = new DynamicByteProvider(inputBuffer);
                hbInput.ByteProvider = requestData;

                DynamicByteProvider responseData = new DynamicByteProvider(outputBuffer);
                hbOutput.ByteProvider = responseData;
            }
            // update request view
            Request newTx = new Request();
            newTx.RequestName = String.Format(
                "0x{0:X} ({1:X4}-{2:D5})",
                ioctl,
                CRC16.ComputeChecksum(inputBuffer),
                (int)(DateTime.Now.Ticks % 1e11 / 1e6));
            newTx.DevicePath = tbDevicePath.Text;
            newTx.IOCTL = ioctl;
            newTx.PreCallInput = inputBuffer;
            newTx.PostCallOutput = outputBuffer;
            newTx.ReturnValue = errorCode;
            newTx.BytesReturned = returnedBytes;

            //if (tlvRequestHistory.SelectedObject == null)
            //{
            //    newTx.Parent = null;
            //    requests.Add(newTx);
            //}
            //else
            //{
            //    newTx.Parent = (Request)tlvRequestHistory.SelectedObject;

            //    if ((newTx.PreCallInput.SequenceEqual(newTx.Parent.PreCallInput)) && (newTx.Parent.Parent != null))
            //        newTx.Parent.Children.Add(newTx);
            //    else
            //        newTx.Children.Add(newTx);
            //}

            // Avoiding tree structure for now.
            newTx.Parent = null;
            requests.Add(newTx);

            tlvRequestHistory.HideSelection = false;
            tlvRequestHistory.SetObjects(requests);
            tlvRequestHistory.Expand(newTx.Parent);
            tlvRequestHistory.Sort(tlvRequestHistory.GetColumn(3), SortOrder.Descending);
            return;
        }

        // Mark the selected request as a favourite
        private void btnStarRequest_Click(object sender, EventArgs e)
        {
            if (tlvRequestHistory.SelectedIndex == -1) return;
            ((Request)tlvRequestHistory.SelectedObject).IsFavourite ^= true;
            tlvRequestHistory.SetObjects(requests);
        }

        private void btnOpenDB_Click(object sender, EventArgs e)
        {
            // ToDo
        }

        // Filters results in the Request History view (TODO)
        private void tbFilters_TextChanged(object sender, EventArgs e)
        {
            tlvRequestHistory.ModelFilter = null;
            tlvRequestHistory.ModelFilter = new ModelFilter(delegate (Object tx)
            {
                return ((Request)tx).RequestName.Contains(tbFilters.Text);
            });
        }

        // Shows the About window
        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["AboutForm"] as AboutForm == null)
            {
                AboutForm aboutForm = new AboutForm();
                aboutForm.Show();
            }
            else
            {
                AboutForm aboutForm = Application.OpenForms["AboutForm"] as AboutForm;
                aboutForm.Focus();
            }
        }

        // parse and validate Access Mask
        private void tbAccessMask_TextChanged(object sender, EventArgs e)
        {
            Point toolTipCoords = tbAccessMask.Location;
            toolTipCoords.X -= 400;
            toolTipCoords.Y -= 20;

            uint fa_mask;
            if (!UInt32.TryParse(tbAccessMask.Text, System.Globalization.NumberStyles.HexNumber, null, out fa_mask))
            {
                tbAccessMask.BackColor = Color.MistyRose;
                btnSend.Enabled = false;
                toolTip.Show("Access Mask code must be in hexadecimal format.", tbAccessMask, toolTipCoords, 3000);
            }
            else
            {
                tbAccessMask.BackColor = Color.Honeydew;
                btnSend.Enabled = true;
            }

        }

        // update input panel when the InputSize is changed
        private void nudInputSize_ValueChanged(object sender, EventArgs e)
        {
            DynamicByteProvider dbpData = new DynamicByteProvider(new byte[(int)nudInputSize.Value]);
            hbInput.ByteProvider = dbpData;
        }

        // update output panel when the OutputSize is changed
        private void nudOutputSize_ValueChanged(object sender, EventArgs e)
        {
            DynamicByteProvider dbpData = new DynamicByteProvider(new byte[(int)nudOutputSize.Value]);
            hbOutput.ByteProvider = dbpData;
        }

        private void hbInput_ByteProviderChanged(object sender, EventArgs e)
        {
            //DynamicByteProvider dbpData = new DynamicByteProvider(new byte[(int)hbInput.ByteProvider.Length]);
            //nudInputSize.Value = (int)dbpData;
        }

        // show the settings form
        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["SettingsForm"] as SettingsForm == null)
            {
                SettingsForm settingsForm = new SettingsForm(this);
                settingsForm.Show();
            }
            else
            {
                SettingsForm settingsForm = Application.OpenForms["SettingsForm"] as SettingsForm;
                settingsForm.Focus();
            }
        }

        // enable/disable access mask
        private void chkEnableAccessMask_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableAccessMask.Checked == true)
            {
                tbAccessMask.Enabled = true;
                cmbACL.Enabled = false;
            }
            else
            {
                tbAccessMask.Enabled = false;
                cmbACL.Enabled = true;
            }

        }
    }
}
