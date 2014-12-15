using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Reptile
{
    public partial class MainForm : Form
    {
        private ReptileCore reptile;
        public MainForm()
        {
            InitializeComponent();
        }

        private void btBegin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUrl.Text)) return;
            reptile = new ReptileCore(tbUrl.Text);
            reptile.Register(this);
            reptile.BeginCatch();
        }
    }
}
