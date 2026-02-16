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
    }
}
