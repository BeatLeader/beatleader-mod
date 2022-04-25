using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatLeader.Replays.Forms
{
    public partial class NonVRReplayUI : Form
    {
        public NonVRReplayUI()
        {
            InitializeComponent();
        }
        private void Close(object sender, EventArgs e)
        {
            base.Close();
        }
        private void HandlePauseButtonClicked(object sender, EventArgs e)
        {

        }
    }
}
