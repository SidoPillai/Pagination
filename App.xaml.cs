namespace Pagination
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = new Window(new AppShell());
            //window.Width = 460;  // Set the width
            //window.Height = 640; // Set the height
            return window;
        }
    }
}