using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

namespace Beadscount
{
    public class MainForm : Form
    {
        String imageToProcessPath = string.Empty;
        Bitmap imageToProcess;
        String countingTextOutput;

        public MainForm()
        {
            Title = "Beadscount";
            ClientSize = new Size(400, 350);

            Content = new Panel();

            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About" };
            aboutCommand.Executed += (sender, e) => MessageBox.Show(this, "Beadscount 0.1\nDeveloped by Andrea Giorgio \"Muu?\" Cerioli");

            var loadCommand = new Command { MenuText = "Load", ToolBarText = "Load", Shortcut = Application.Instance.CommonModifier | Keys.L };
            loadCommand.Executed += (sender, e) => LoadForm();

            var countCommand = new Command { MenuText = "Count", ToolBarText = "Count", };
            countCommand.Executed += (sender, e) => CountAction();

            // create menu
            Menu = new MenuBar
            {
                ApplicationItems =
				{
					loadCommand
				},
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            // create toolbar			
            ToolBar = new ToolBar { Items = { loadCommand, countCommand } };
        }

        public void LoadForm()
        {
            // Show a file dialog for loading images
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = string.Empty;
            fileDialog.MultiSelect = false;
            string[] tmpExt = { ".bmp", ".png" };
            FileDialogFilter filter = new FileDialogFilter("Bitmap or PNG file", tmpExt);
            fileDialog.Filters = new FilterCollection<FileDialogFilter> { filter };
            fileDialog.ShowDialog(this);

            // User cancels or closes the form
            if (string.IsNullOrEmpty(fileDialog.FileName)) { return; }

            // Load image
            imageToProcessPath = fileDialog.FileName;
            imageToProcess = new Bitmap(imageToProcessPath);

            // If the user had selected an image, re-update screen to show it
            if (string.IsNullOrEmpty(imageToProcessPath)) { return; }
            
            ImageView imgView = new ImageView();
            imgView.Image = imageToProcess;
            Content = imgView;
        }


        public void CountAction()
        {
            // Exit if no image was loaded
            if (string.IsNullOrEmpty(imageToProcessPath))
            {
                MessageBox.Show(this, "Load an image first");
                return;
            }

            // Draw a now smaller window to notify the user about the counting
            Form progress = new Form();
            progress.Title = "Counting";
            progress.Size = new Size(250, 150);
            progress.Content = new Label { Text = "Counting beads, please be patient\n(the application might not respond during this operation)" };
            progress.Show();

            // Count beads
            countingTextOutput = CountBead(progress);

            // Display the results
            Content = new TextArea { Text = "You can copy/paste those results\n" + countingTextOutput };
        }

        // ┌─┐
        // ┴─┴
        // ಠ_ರೃ
        public string CountBead(Form progressForm)
        {
            // Get image basic infos
            int size = imageToProcess.Size.Width * imageToProcess.Size.Height;
            int width = imageToProcess.Size.Width;
            int height = imageToProcess.Size.Height;

            // NOTE Had to use Sys.Draw.Bitmap here because Eto.Draw.Bitmap misteriously hangs when checking 8bit images.
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(imageToProcessPath);
            Dictionary<Color, int> colorList = new Dictionary<Color, int>();
            Color tmpColor;
            System.Drawing.Color tmpSysColor;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tmpSysColor = img.GetPixel(i, j);
                    tmpColor = Color.FromArgb(tmpSysColor.R, tmpSysColor.G, tmpSysColor.B, tmpSysColor.A);  // Convert Sys.Draw.Color to Eto.Draw.Color for easier reading

                    if (colorList.ContainsKey(tmpColor))
                    { colorList[tmpColor]++; }      // Increment a color count
                    else
                    { colorList.Add(tmpColor, 1); } // When a color is not found, create it
                }
            }

            progressForm.Close();
            
            // Format a string to show colors and quantity
            string colors = string.Empty;
            foreach (KeyValuePair<Color, int> kvp in colorList)
            {
                colors += kvp.Key.ToString() + " : " + kvp.Value.ToString() + "\n";
            }

            return colors;
        }
    }   // Class End
}   // Namespace End