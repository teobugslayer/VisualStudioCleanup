namespace VisualStudioCleanup
{
    class Uninstallable
    {
        public Uninstallable(string name, string command)
        {
            this.Name = name;
            this.Command = command;
        }

        public string Name { get; }
        public string Command { get; }
    }
}
