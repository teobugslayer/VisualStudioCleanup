namespace VisualStudioCleanup
{
    class Uninstallable
    {
        public Uninstallable(string name, string command)
        {
            this.Name = name;
            this.Command = command;
        }

        public string Name { get; private set; }
        public string Command { get; private set; }
    }
}
