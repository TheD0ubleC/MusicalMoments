using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class UiEffectsService
    {
        public static void Delay(int time)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < time)
            {
                Application.DoEvents();
            }
        }

        public static void ButtonStabilization(int time, Button button)
        {
            string originalText = button.Text;
            button.Enabled = false;
            for (int i = time; i > 0; i--)
            {
                button.Text = i.ToString();
                Delay(1000);
            }

            button.Enabled = true;
            button.Text = originalText;
        }

        public static async Task FadeIn(int durationMilliseconds, Form form)
        {
            form.Visible = false;
            form.Opacity = 0;
            form.Show();

            for (double opacity = 0; opacity <= 1; opacity += 0.05)
            {
                form.Opacity = opacity;
                await Task.Delay(durationMilliseconds / 20);
            }

            form.Opacity = 1;
        }

        public static async Task FadeOut(int durationMilliseconds, Form form)
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.05)
            {
                form.Opacity = opacity;
                await Task.Delay(durationMilliseconds / 20);
            }

            form.Visible = false;
            form.Dispose();
        }

        public static async Task FadeHideWithoutDispose(int durationMilliseconds, Form form)
        {
            if (form == null || form.IsDisposed || !form.Visible)
            {
                return;
            }

            int steps = Math.Max(1, durationMilliseconds / 14);
            int stepDelay = Math.Max(1, durationMilliseconds / steps);
            double startOpacity = Math.Clamp(form.Opacity, 0.05, 1.0);

            for (int index = 0; index < steps; index++)
            {
                double progress = (index + 1d) / steps;
                form.Opacity = Math.Max(0.0, startOpacity * (1.0 - progress));
                await Task.Delay(stepDelay);
            }

            form.Hide();
            form.Opacity = 1.0;
        }

        public static async Task FadeShowWithoutFlash(int durationMilliseconds, Form form)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            int steps = Math.Max(1, durationMilliseconds / 14);
            int stepDelay = Math.Max(1, durationMilliseconds / steps);

            form.Opacity = 0.0;
            if (!form.Visible)
            {
                form.Show();
            }

            for (int index = 0; index < steps; index++)
            {
                double progress = (index + 1d) / steps;
                form.Opacity = Math.Clamp(progress, 0.0, 1.0);
                await Task.Delay(stepDelay);
            }

            form.Opacity = 1.0;
        }
    }
}
