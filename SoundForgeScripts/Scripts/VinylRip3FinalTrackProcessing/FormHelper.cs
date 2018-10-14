using System.Drawing;
using System.Windows.Forms;

namespace SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing
{
    public class FormHelper
    {
        public static DialogResult ShowScriptPauseDialog(IWin32Window hOwner)
        {
            return ShowScriptPauseDialog(hOwner, "Pause Script");
        }

        //TODO: refactor into a class with properties that have sensible defaults,  and assignable delegate handlers
        public static DialogResult ShowScriptPauseDialog(IWin32Window hOwner, string dialogTitle)
        {
            Form dlg = new Form();
            Size sForm = new Size(520, 150);

            dlg.Text = dialogTitle;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ClientSize = sForm;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 5);
            const int lblHeight = 16;

            // we position the buttons relative to the bottom and left of the form.
            pt = (Point)dlg.ClientSize;
            pt -= sOff;

            Button btn = new Button();
            btn.TabIndex = 5;
            pt -= btn.Size;
            btn.Text = "Cancel";
            btn.Location = pt;
            btn.Click += FormHelper.OnCancel_Click;
            dlg.Controls.Add(btn);
            dlg.CancelButton = btn;
            pt.X -= (btn.Width + 10);

            btn = new Button();
            btn.TabIndex = 4;
            btn.Text = "OK";
            btn.Location = pt;
            btn.Click += FormHelper.OnOK_Click;
            dlg.Controls.Add(btn);
            dlg.AcceptButton = btn;
            pt.X -= (btn.Width + 10);

            // position prompt beside buttons
            pt = new Point(sOff.Width, dlg.ClientSize.Height - sSpacer.Height - btn.Size.Height);

            Label lblPrompt = new Label();
            lblPrompt.Text = "Press OK to continue...";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            dlg.Controls.Add(lblPrompt);
            pt.Y -= lblPrompt.Height;

            DialogResult result = dlg.ShowDialog(hOwner);
            return result;
        }

        // generic Ok button click (sets dialog result and dismisses the form)
        public static void OnOK_Click(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            Form form = (Form)btn.Parent;
            form.DialogResult = DialogResult.OK;
            form.Close();
        }

        // generic Cancel button click (sets dialog result and dismisses the form)
        public static void OnCancel_Click(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            Form form = (Form)btn.Parent;
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }
    }
}