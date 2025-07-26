using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CSharpConsoleAddon
{
    public partial class CSharpConsoleClient : Client
    {
        private CSharpConsole consolePanel;
        private bool isVisible = false;

        public CSharpConsoleClient()
        {
            StyleSheet.Load("/CSharpConsole_sbox/ui/scss/CSharpConsole.scss");
            // Bind key H to toggle the console
            Input.RegisterButton( "ToggleCSharpConsole", InputButton.Down );

            // Optionally add console command to toggle
            ConsoleSystem.Register("csharp_console", () =>
            {
                ToggleConsole();
            });
        }

        [Event.Tick.Client]
        public void OnTick()
        {
            if (Input.Pressed(InputButton.Down) && Input.Down("ToggleCSharpConsole"))
            {
                ToggleConsole();
            }
        }

        private void ToggleConsole()
        {
            if (isVisible)
            {
                consolePanel?.Delete();
                consolePanel = null;
                isVisible = false;
            }
            else
            {
                consolePanel = new CSharpConsole();
                RootPanel.AddChild(consolePanel);
                isVisible = true;
            }
        }
    }
}
