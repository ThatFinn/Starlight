using System;
using Starlight.Storage;

namespace Starlight.Commands;

internal class MenuVisibilityCommands
{
    internal class OpenCommand : StarlightCommand
    {
        internal OpenCommand(MenuIdentifier identifier, StarlightMenu menu, bool inGameOnly)
        {
            this.identifier = identifier;
            this.menu = menu;
            this.inGameOnly = inGameOnly;
        }
        MenuIdentifier identifier = new MenuIdentifier();
        StarlightMenu menu;
        private bool inGameOnly;
        public override string ID => "open"+identifier.saveKey.ToLower();
        public override string Usage => "open"+identifier.saveKey.ToLower();
        public override Type[] execWhileMenuOpen => new[] { menu.GetType() };
        public override CommandType type => CommandType.DontLoad | CommandType.Menu;
        public override string Description => Tr($"cmd.openmenu.description");
        public override string ExtendedDescription
        {
            get
            {
                string key = $"cmd.openmenu.extendeddescription";
                string translation = Tr(key);
                return key == translation ? Description : translation;
            }
        }
        public override bool Execute(string[] args)
        {
            if (!args.IsBetween(0,0)) return SendNoArguments();
            if (inGameOnly) if(!inGame) return SendLoadASaveFirst();
            if (menu.isOpen) return false;
            MenuEUtil.CloseOpenMenu();
            menu.Open();
            return true;
        }
    }
    internal class CloseCommand : StarlightCommand
    {
        internal CloseCommand(MenuIdentifier identifier, StarlightMenu menu, bool inGameOnly)
        {
            this.identifier = identifier;
            this.menu = menu;
            this.inGameOnly = inGameOnly;
        }
        MenuIdentifier identifier = new MenuIdentifier();
        StarlightMenu menu;
        private bool inGameOnly;
        public override string ID => "close"+identifier.saveKey.ToLower();
        public override string Usage => "close"+identifier.saveKey.ToLower();
        public override Type[] execWhileMenuOpen => new[] { menu.GetType() };
        public override CommandType type => CommandType.DontLoad | CommandType.Menu;
        public override string Description => Tr($"cmd.closemenu.description");
        public override string ExtendedDescription
        {
            get
            {
                string key = $"cmd.closemenu.extendeddescription";
                string translation = Tr(key);
                return key == translation ? Description : translation;
            }
        }
        public override bool Execute(string[] args)
        {
            if (!args.IsBetween(0,0)) return SendNoArguments();
            if (inGameOnly) if(!inGame) return SendLoadASaveFirst();
            if (!menu.isOpen) return false;
            menu.Close();
            return true;
        }
    }
    internal class ToggleCommand : StarlightCommand
    {
        internal ToggleCommand(MenuIdentifier identifier, StarlightMenu menu, bool inGameOnly)
        {
            this.identifier = identifier;
            this.menu = menu;
            this.inGameOnly = inGameOnly;
        }
        MenuIdentifier identifier = new MenuIdentifier();
        StarlightMenu menu;
        private bool inGameOnly;
        public override string ID => "toggle"+identifier.saveKey.ToLower();
        public override string Usage => "toggle"+identifier.saveKey.ToLower();
        public override Type[] execWhileMenuOpen => new[] { menu.GetType() };
        public override CommandType type => CommandType.DontLoad | CommandType.Menu;
        public override string Description => Tr($"cmd.togglemenu.description");
        public override string ExtendedDescription
        {
            get
            {
                string key = $"cmd.togglemenu.extendeddescription";
                string translation = Tr(key);
                return key == translation ? Description : translation;
            }
        }
        public override bool Execute(string[] args)
        {
            if (!args.IsBetween(0,0)) return SendNoArguments();
            if (inGameOnly) if(!inGame) return SendLoadASaveFirst();
            if (!menu.isOpen) MenuEUtil.CloseOpenMenu();
            menu.Toggle();
            return true;
        }
    }
}