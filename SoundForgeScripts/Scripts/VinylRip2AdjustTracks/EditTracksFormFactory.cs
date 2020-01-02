using System;
using SoundForgeScriptsLib.Utils;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksFormFactory
    {
        public EditTracksForm Create(EditTracksViewModel viewModel, EditTracksController controller)
        {
            EditTracksForm form = new EditTracksForm();
            BindViewModel(viewModel, form);
            form.Text = viewModel.FormTitle;
            form.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                KeyboardBindings(form, viewModel, controller, e);
            };
            return form;
        }

        private void BindViewModel(EditTracksViewModel viewModel, EditTracksForm form)
        {
            form.BtnPreviewAll.DataBindings.Add("Enabled", viewModel, "HasTracks", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnPreviewStart.DataBindings.Add("Enabled", viewModel, "HasTracks", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.LblTrack.DataBindings.Add("Text", viewModel, "TrackName", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnPreviewEnd.DataBindings.Add("Enabled", viewModel, "HasTracks", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnPrevious.DataBindings.Add("Enabled", viewModel, "CanNavigatePrevious", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnDelete.DataBindings.Add("Enabled", viewModel, "HasTracks", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnNext.DataBindings.Add("Enabled", viewModel, "CanNavigateNext", false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        private void KeyboardBindings(EditTracksForm form, EditTracksViewModel vm, EditTracksController controller, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Escape)
            {
                form.Close();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Space)
            {
                form.BtnStopPreview.PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Q)
            {
                // "pass-through" behaviour to SF
                controller.ToggleLoopedPlayback();
                e.Handled = true;
            }

            if (vm.HasTracks && e.KeyCode == Keys.Home)
            {
                form.BtnPreviewStart.PerformClick();
                e.Handled = true;
            }

            if (vm.HasTracks && e.KeyCode == Keys.End)
            {
                form.BtnPreviewEnd.PerformClick();
                e.Handled = true;
            }

            if (vm.HasTracks && e.KeyCode == Keys.Delete)
            {
                form.BtnDelete.PerformClick();
                e.Handled = true;
            }

            if (vm.CanNavigatePrevious && e.KeyCode == Keys.Left)
            {
                form.BtnPrevious.PerformClick();
                e.Handled = true;
            }

            if (vm.CanNavigateNext && e.KeyCode == Keys.Right)
            {
                form.BtnNext.PerformClick();
                e.Handled = true;
            }
        }

    }
}
