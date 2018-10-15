using Microsoft.Win32;

namespace GMA500Helper.System {
    public static class AvalonGraphicsManager {
        public const string DisableHWAcceleration = "DisableHWAcceleration";
        
        public const string BreakOnUnexpectedErrors = "BreakOnUnexpectedErrors";
        public const string EnableDebugControl = "EnableDebugControl";
        public const string MaxMultisampleType = "MaxMultisampleType";
        public const string RecordAvalonFile = "RecordAvalonFile";
        public const string RPCAvalon = "RPCAvalon";
        public const string RequiredVideoDriverDate = "RequiredVideoDriverDate";
        public const string SkipDriverDateCheck = "SkipDriverDateCheck";
        public const string SkipDriverCheck = "SkipDriverCheck";
        public const string UseDX9LText = "UseDX9LText";
        public const string UseReferenceRasterizer = "UseReferenceRasterizer";

        #region REGs
        //HKEY_LOCAL_MACHINE
        //HKEY_USERS\S-1-5-18 == LocalSystem
        //HKEY_USERS\S-1-5-19 == LocalService
        //HKEY_USERS\S-1-5-20 == NetworkService
        //HKEY_CURRENT_USER
        #endregion

        public static void Set(string key, int value) {
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_USERS\.DEFAULT\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.DWord);
        }

        public static void Set(string key, string value) {
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_USERS\.DEFAULT\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.String);
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Avalon.Graphics", key, value, RegistryValueKind.String);
        }
        
        public static void Reset() {
            Set(DisableHWAcceleration, 1);

            Set(BreakOnUnexpectedErrors, 0);
            Set(EnableDebugControl, 0);
            Set(MaxMultisampleType, 0);
            Set(RecordAvalonFile, 0);
            Set(RPCAvalon, 1);
            Set(RequiredVideoDriverDate, "1980/01/01");
            Set(SkipDriverDateCheck, 1);
            Set(SkipDriverCheck, 1);
            Set(UseDX9LText, 0);
            Set(UseReferenceRasterizer, 0);
        }
    }
}
