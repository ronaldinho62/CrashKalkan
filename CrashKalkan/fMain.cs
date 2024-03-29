﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CrashKalkan
{
    public partial class fMain : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        public fMain()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, 0, 0, Keys.Pause.GetHashCode());
            RegisterHotKey(this.Handle, 1, 0, Keys.Scroll.GetHashCode());
            RegisterHotKey(this.Handle, 2, 0, Keys.PrintScreen.GetHashCode());
            RegisterHotKey(this.Handle, 3, 0, Keys.End.GetHashCode());
            DataStorage.FirewallControl.UnlockLobby();
            FormClosing += fMain_Closing;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0312)
            {
                if (isGTAVRunning())
                {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    int id = m.WParam.ToInt32();

                    if (key == Keys.Pause)
                    {
                        SweepSession();
                    }
                    else if (key == Keys.Scroll)
                    {
                        ToggleLockSession();
                    }
                    else if (key == Keys.PrintScreen)
                    {
                        ToggleNetwork();
                    }
                    else if (key == Keys.End)
                    {
                        ToggleAntiIdling();
                    }
                }
                else
                {
                    Activate();
                    MessageBox.Show("Açık bir GTA V bulunamadı!", "CrashKalkan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool isGTAVRunning()
        {
            bool isRunning = false;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Equals("GTA5"))
                {
                    isRunning = true;
                    DataStorage.pGTAV = process;
                    break;
                }
            }
            return isRunning;
        }

        private void ToggleNetwork()
        {
            if (isGTAVRunning())
            {
                if (!tmrNetwork.Enabled)
                {
                    DataStorage.pPending.Play();
                    NetworkConnectionControl.Disconnect();
                    tmrNetwork.Start();
                }
            }
            else
            {
                Activate();
                MessageBox.Show("Açık bir GTA V bulunamadı!", "CrashKalkan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleAntiIdling()
        {
            DataStorage.AntiIdlingActive = !DataStorage.AntiIdlingActive;

            if (isGTAVRunning())
            {
                if (DataStorage.AntiIdlingActive)
                {
                    DataStorage.MouseEventControl.Start();
                    lblAfkStatus.BackColor = Color.Green;
                    lblAfkStatus.Text = "Otomaik fare haraketi açık!";
                }
                else
                {
                    DataStorage.MouseEventControl.Stop();
                    lblAfkStatus.BackColor = Color.Red;
                    lblAfkStatus.Text = "Otomaik fare haraketi kapalı!";
                }
            }
            else
            {
                Activate();
                MessageBox.Show("Açık bir GTA V bulunamadı!", "CrashKalkan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SweepSession()
        {
            if (isGTAVRunning())
            {
                if (!tmrResume.Enabled)
                {
                    DataStorage.pPending.Play();
                    Toolkit.SuspendProcess(DataStorage.pGTAV.Id);
                    tmrResume.Start();
                }
            }
            else
            {
                Activate();
                MessageBox.Show("Açık bir GTA V bulunamadı!", "CrashKalkan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleLockSession()
        {
            if (!DataStorage.HasAdministrativeRight) { return; }
            if (isGTAVRunning())
            {
                if (DataStorage.LobbyLocked)
                {
                    DataStorage.FirewallControl.UnlockLobby(true);
                    lblSessionStatus.BackColor = Color.Green;
                    lblSessionStatus.Text = "Sunucu kilitli değil!";
                }
                else
                {
                    DataStorage.FirewallControl.LockLobby(true);
                    lblSessionStatus.BackColor = Color.Red;
                    lblSessionStatus.Text = "Sunucu kilitli!";
                }
                DataStorage.LobbyLocked = !DataStorage.LobbyLocked;
            }
            else
            {
                Activate();
                MessageBox.Show("Açık bir GTA V bulunamadı!", "CrashKalkan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tmrNetwork_Tick(object sender, EventArgs e)
        {
            NetworkConnectionControl.Connect();
            DataStorage.pSweeped.Play();
            tmrNetwork.Stop();
        }

        private void tmrResume_Tick(object sender, EventArgs e)
        {
            Toolkit.ResumeProcess(DataStorage.pGTAV.Id);
            DataStorage.pSweeped.Play();
            tmrResume.Stop();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            if (!DataStorage.HasAdministrativeRight)
            {
                lblSessionStatus.Text = "Programı yönetici olarak başlatın!";
                lblSessionStatus.BackColor = Color.Red;
                lblLockSessionInfo.Font = new Font(lblLockSessionInfo.Font, FontStyle.Strikeout);
                btnScrollLock.Enabled = false;
            }

            lblAfkStatus.Text = "Otomaik fare haraketi kapalı!";
            lblAfkStatus.BackColor = Color.Red;
        }

        private void fMain_Closing(object sender, CancelEventArgs e)
        {
            DataStorage.FirewallControl.UnlockLobby();
            UnregisterHotKey(this.Handle, 0);
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 3);
        }

        private void btnPauseBreak_Click(object sender, EventArgs e)
        {
            SweepSession();
        }

        private void btnScrollLock_Click(object sender, EventArgs e)
        {
            ToggleLockSession();
        }

        private void btnPrintScreen_Click(object sender, EventArgs e)
        {
            ToggleNetwork();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            ToggleAntiIdling();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://gtaotr.xyz");
        }
    }
}

// by ens for GTA Online Türkiye https://gtaotr.xyz