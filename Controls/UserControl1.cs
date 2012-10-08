using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public partial class myControl : UserControl
    {
        private string url;

        public string Url
        {
            get { return this.url; }
            set
            {
                this.url = value;
                this.axShockwaveFlash1.Play();
            }
        }


        public myControl()
        {
            InitializeComponent();
        }
    }
}
