using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debugger
{
    public partial class BreakPointSetter : Form
    {
        private readonly DebuggerWindow _debuggerWindow;

        private struct BreakPoint
        {
            public string Source;
            public int Line;

            public string View => $"Source: {Source}, Line: {Line}";
        }

        public BreakPointSetter(DebuggerWindow debuggerWindow)
        {
            _debuggerWindow = debuggerWindow;

            foreach (var item in _debuggerWindow.BreakPointMap)
            {
                foreach (var line in item.Value)
                {
                    listBox1.Items.Add(new BreakPoint
                    {
                        Source = item.Key,
                        Line = line
                    });
                }
            }

            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = listBox1.SelectedIndex;
            if (index != -1)
            {
                var breakPoint = (BreakPoint) listBox1.Items[index];
                txtSrc.Text = breakPoint.Source;
                txtLine.Text = breakPoint.Line.ToString();
            }
            else
            {
                txtSrc.Clear();
                txtLine.Clear();
            }
        }

        private void btnAddSave_Click(object sender, EventArgs e)
        {
            if (txtSrc.Text == string.Empty || txtLine.Text == string.Empty)
            {
                return;
            }

            var breakPoint = new BreakPoint
            {
                Source = txtSrc.Text,
                Line = int.Parse(txtLine.Text)
            };

            HashSet<int> breakPoints;
            if (!_debuggerWindow.BreakPointMap.TryGetValue(breakPoint.Source, out breakPoints) || breakPoints == null)
            {
                breakPoints = new HashSet<int>();
                _debuggerWindow.BreakPointMap[breakPoint.Source] = breakPoints;
            }

            breakPoints.Add(breakPoint.Line);
            listBox1.Items.Add(breakPoint);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem;
            if (item == null)
            {
                return;
            }
            
            listBox1.Items.Remove(item);
            var breakPoint = (BreakPoint)item;

            HashSet<int> breakPoints;
            if (!_debuggerWindow.BreakPointMap.TryGetValue(breakPoint.Source, out breakPoints))
            {
                return;
            }

            if (breakPoints == null)
            {
                _debuggerWindow.BreakPointMap.Remove(breakPoint.Source);
            }
            else
            {
                breakPoints.Remove(breakPoint.Line);
            }
        }

        private void BreakPointSetter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
