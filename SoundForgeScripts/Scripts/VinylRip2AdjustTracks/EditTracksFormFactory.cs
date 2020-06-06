using System;
using SoundForgeScriptsLib.Utils;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksFormFactory
    {
        public EditTracksForm Create(EditTracksViewModel viewModel, EditTracksController controller,
            OutputHelper output)
        {
            EditTracksForm form = new EditTracksForm();
            BindViewModel(viewModel, form);
            form.Text = viewModel.FormTitle;
            form.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                KeyboardBindings(form, viewModel, controller, e);
            };

            form.Closing += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                output.ToStatusField1(string.Empty);
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
            form.BtnAddTrackBefore.DataBindings.Add("Enabled", viewModel, "CanAddTrackBefore", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnAddTrackAfter.DataBindings.Add("Enabled", viewModel, "CanAddTrackAfter", false,
                DataSourceUpdateMode.OnPropertyChanged);

            form.BtnMoveStartPlus.DataBindings.Add("Enabled", viewModel, "CanMoveStartPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveStartPlusPlus.DataBindings.Add("Enabled", viewModel, "CanMoveStartPlusPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveStartMinus.DataBindings.Add("Enabled", viewModel, "CanMoveStartMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveStartMinusMinus.DataBindings.Add("Enabled", viewModel, "CanMoveStartMinusMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);

            form.BtnMoveFadeInPlus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeInPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeInPlusPlus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeInPlusPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeInMinus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeInMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeInMinusMinus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeInMinusMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);

            form.BtnMoveEndPlus.DataBindings.Add("Enabled", viewModel, "CanMoveEndPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveEndPlusPlus.DataBindings.Add("Enabled", viewModel, "CanMoveEndPlusPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveEndMinus.DataBindings.Add("Enabled", viewModel, "CanMoveEndMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveEndMinusMinus.DataBindings.Add("Enabled", viewModel, "CanMoveEndMinusMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);

            form.BtnMoveFadeOutPlus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeOutPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeOutPlusPlus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeOutPlusPlus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeOutMinus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeOutMinus", false,
                DataSourceUpdateMode.OnPropertyChanged);
            form.BtnMoveFadeOutMinusMinus.DataBindings.Add("Enabled", viewModel, "CanMoveFadeOutMinusMinus", false,
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

            if (e.KeyCode == Keys.J)
            {
                (e.Shift ? form.BtnMoveStartMinus : form.BtnMoveStartMinusMinus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.K)
            {
                (e.Shift ? form.BtnMoveStartPlus : form.BtnMoveStartPlusPlus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.H)
            {
                (e.Shift ? form.BtnMoveEndMinus : form.BtnMoveEndMinusMinus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.L)
            {
                (e.Shift ? form.BtnMoveEndPlus : form.BtnMoveEndPlusPlus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.U)
            {
                (e.Shift ? form.BtnMoveFadeInMinus : form.BtnMoveFadeInMinusMinus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.I)
            {
                (e.Shift ? form.BtnMoveFadeInPlus : form.BtnMoveFadeInPlusPlus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Y)
            {
                (e.Shift ? form.BtnMoveFadeOutMinus : form.BtnMoveFadeOutMinusMinus).PerformClick();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.O)
            {
                (e.Shift ? form.BtnMoveFadeOutPlus : form.BtnMoveFadeOutPlusPlus).PerformClick();
                e.Handled = true;
            }
        }
    }
}
