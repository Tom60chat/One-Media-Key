using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace One_Media_Key
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string AppBaseDirectory = @"./";

        public MainWindow()
        {
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("You already launched this application !\n\nCheck the notification area and double-click the application icon.", "OneMediaKey: Error");
                Process.GetCurrentProcess().Kill();
            }

            InitializeComponent();
            // App Path
            AppBaseDirectory = (AppDomain.CurrentDomain.BaseDirectory + @"Resources\");

            // Load saved settings
            try
            {
                KeyConverter k = new KeyConverter();
                AssignedKey = (Key)k.ConvertFromString(OneMediaKey.Properties.Settings.Default.AssignedKey);
                SecondeAssignedKey = (Key)k.ConvertFromString(OneMediaKey.Properties.Settings.Default.SecondeAssignedKey);
                ThirdAssignedKey = (Key)k.ConvertFromString(OneMediaKey.Properties.Settings.Default.ThirdAssignedKey);
                Console.WriteLine("Load keys conf: " + "AssignedKey: " + AssignedKey + " | SecondeAssignedKey: " + SecondeAssignedKey + " | ThirdAssignedKey: " + ThirdAssignedKey);
                if (ThirdAssignedKey != Key.None)
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString() + " + " + ThirdAssignedKey.ToString();
                else if (SecondeAssignedKey != Key.None)
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString();
                else
                    KeyButton.Content = AssignedKey.ToString();

                CheckBox_OpenMedia.IsChecked = OneMediaKey.Properties.Settings.Default.OpenMPlayerOnNotOp;
            } catch
            {
                MessageBox.Show("One Media Key.exe.config are not detected\nPlease put it in the app directory or else the save are not available", "OneMediaKey: Error");
            }

            // Check if the apps is autostarted
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (key.GetValue("OneMediaKey") == null)
            {
                CheckBox_AutoStart.IsChecked = false;
            } else
            {
                CheckBox_AutoStart.IsChecked = true;
            }

            // Check if the list exist
            if (File.Exists(AppBaseDirectory + "MusicPlayerName.txt"))
            {
                CheckBox_OpenMedia.IsEnabled = true;
            } else
            {
                CheckBox_OpenMedia.IsEnabled = false;
            }
            DetectKey();
        }


        Key AssignedKey;
        Key SecondeAssignedKey;
        Key ThirdAssignedKey;

        bool Assignating = false;

        private void KeyButton_OnMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            KeyButton.Content = "...";
            AssignedKey = Key.None;
            SecondeAssignedKey = Key.None;
            ThirdAssignedKey = Key.None;
            Assignating = true;
        }

        

        // =============
        // Assigne Keys
        private void KeyButton_OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (Assignating)
            {
                if (AssignedKey == Key.None)
                {
                    Console.WriteLine("AssignedKey: " + e.Key);
                    AssignedKey = e.Key;
                    KeyButton.Content = AssignedKey.ToString() + "...";
                    return;
                }
                if (SecondeAssignedKey == Key.None)
                {
                    Console.WriteLine("SecondeAssignedKey: " + e.Key);
                    SecondeAssignedKey = e.Key;
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString() + "...";
                    return;
                }
                if (ThirdAssignedKey == Key.None)
                {
                    Console.WriteLine("ThirdAssignedKey: " + e.Key);
                    ThirdAssignedKey = e.Key;
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString() + " + " + ThirdAssignedKey.ToString();
                    return;
                }

            }
        }

        private void KeyButton_OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (Assignating)
            {
                try
                {
                  OneMediaKey.Properties.Settings.Default.AssignedKey = AssignedKey.ToString();
                  OneMediaKey.Properties.Settings.Default.SecondeAssignedKey = SecondeAssignedKey.ToString();
                  OneMediaKey.Properties.Settings.Default.ThirdAssignedKey = ThirdAssignedKey.ToString();
                  OneMediaKey.Properties.Settings.Default.Save();
                }
                catch
                {
                    //MessageBox.Show("One Media Key.exe.config are not detected\nPlease put it in the app directory or else the save are not available");
                    Console.WriteLine("One Media Key.exe.config are not detected\nPlease put it in the app directory or else the save are not available");
                }

                Console.WriteLine("Save keys conf: " + "AssignedKey: " + AssignedKey + " SecondeAssignedKey: " + SecondeAssignedKey + " ThirdAssignedKey: " + ThirdAssignedKey);

                if (ThirdAssignedKey != Key.None)
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString() + " + " + ThirdAssignedKey.ToString();
                else if (SecondeAssignedKey != Key.None)
                    KeyButton.Content = AssignedKey.ToString() + " + " + SecondeAssignedKey.ToString();
                else
                    KeyButton.Content = AssignedKey.ToString();
                Assignating = false;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        int KeyPressedCount = 0;

        async void DetectKey()
        {
            while (true) // looping loopa loop
            {
                await Task.Delay(50);
                //IsAnyKeyDown();
                if (!Assignating)
                {
                    if (ThirdAssignedKey != Key.None)
                    {
                        if (Keyboard.IsKeyDown(ThirdAssignedKey) && Keyboard.IsKeyDown(SecondeAssignedKey) && Keyboard.IsKeyDown(AssignedKey))
                        {
                            int TimePressed = 0;
                            while (Keyboard.IsKeyDown(ThirdAssignedKey) && Keyboard.IsKeyDown(SecondeAssignedKey) && Keyboard.IsKeyDown(AssignedKey))
                            {
                                await Task.Delay(50);
                                TimePressed += 1;
                                if (TimePressed == 20)
                                    Process.Start("ms-cortana://Reactive/?StartMode=Reactive&ListeningMode=True");
                            }
                            if (TimePressed <= 20)
                            {
                                KeyPressedCount += 1;
                                PressKey(KeyPressedCount);
                            }
                        }
                    }
                    else if (SecondeAssignedKey != Key.None)
                    {
                        if (Keyboard.IsKeyDown(AssignedKey) && Keyboard.IsKeyDown(SecondeAssignedKey))
                        {
                            int TimePressed = 0;
                            while (Keyboard.IsKeyDown(AssignedKey) && Keyboard.IsKeyDown(SecondeAssignedKey))
                            {
                                await Task.Delay(50);
                                TimePressed += 1;
                                if (TimePressed == 20)
                                    Process.Start("ms-cortana://Reactive/?StartMode=Reactive&ListeningMode=True");
                            }
                            if (TimePressed <= 20)
                            {
                                KeyPressedCount += 1;
                                PressKey(KeyPressedCount);
                            }
                        }
                    }
                    else if (AssignedKey != Key.None)
                    {
                        if (Keyboard.IsKeyDown(AssignedKey))
                        {
                            int TimePressed = 0;
                            while (Keyboard.IsKeyDown(AssignedKey))
                            {
                                await Task.Delay(50);
                                TimePressed += 1;

                                // If pressed 2s lunch cortana search
                                if (TimePressed == 20)
                                {
                                    /*var KEYEVENTF_KEYUP = 0x0002;
                                    var LWin = (byte)System.Windows.Forms.Keys.LWin;
                                    var C = (byte)System.Windows.Forms.Keys.C;
                                    keybd_event(LWin, LWin, 0, 0);
                                    keybd_event(C, C, 0, 0);
                                    keybd_event(C, C, KEYEVENTF_KEYUP, 0);
                                    keybd_event(LWin, LWin, KEYEVENTF_KEYUP, 0);*/
                                    Process.Start("ms-cortana://Reactive/?StartMode=Reactive&ListeningMode=True");
                                }
                            }
                            if (TimePressed <= 20)
                            {
                                KeyPressedCount += 1;
                                PressKey(KeyPressedCount);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsAnyKeyDown()
        {
            var values = Enum.GetValues(typeof(Key));

            foreach (var v in values)
            {
                if (((Key)v) != Key.None)
                {
                    if (Keyboard.IsKeyDown((Key)v))
                    {
                        if (v.ToString() != "LineFeed" && v.ToString() != "System" && v.ToString() != "DeadCharProcessed")
                        {
                            Console.WriteLine("key: " + v.ToString());
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        async void PressKey(int touchCount)
        {
            //Console.WriteLine("touchCount: " + touchCount);
            await Task.Delay(300);
            if (touchCount == KeyPressedCount)
            {
                var KEYEVENTF_KEYUP = 0x0002;
                KeyPressedCount = 0;
                if (touchCount == 1)
                {
                    // ========================================
                    // Detect if a media player is already open
                    // ========================================
                    bool MediaPlayerOpen = false;

                    if (File.Exists(AppBaseDirectory + "MusicPlayerName.txt"))
                    {
                        CheckBox_OpenMedia.IsEnabled = true;
                        string text = File.ReadAllText(AppBaseDirectory + "MusicPlayerName.txt");
                        string[] lines = text.Split('\n');
                        string output = lines[0].Substring(lines[0].Length - 1, 1);
                        //Console.WriteLine("last char: " + output);
                        Process[] processes = Process.GetProcesses();
                        foreach (Process p in processes)
                            if (!String.IsNullOrEmpty(p.MainWindowTitle))
                            {
                                //Console.WriteLine(p.MainWindowTitle + " / " + p.ProcessName);
                                
                                for (int i = 0; i < lines.Length; i++)
                                    if (p.MainWindowTitle == lines[i].Replace(output, "") || p.ProcessName == lines[i].Replace(output, ""))
                                        MediaPlayerOpen = true;
                            }
                    } else
                    {
                        CheckBox_OpenMedia.IsEnabled = false;
                    }

                    // ========================================
                    // if a media player is not already open open
                    // ========================================

                    //Console.WriteLine("MediaPlayerOpen: " + MediaPlayerOpen);
                    if (MediaPlayerOpen == false && CheckBox_OpenMedia.IsChecked == true)
                    {
                        /*var selectMedia = (byte)System.Windows.Forms.Keys.SelectMedia;
                        keybd_event(selectMedia, 0, 0, 0);
                        keybd_event(selectMedia, 0, KEYEVENTF_KEYUP, 0);
                        Console.WriteLine("openup");*/
                        try
                        {
                            Process.Start(AppBaseDirectory + "OpenDefaultMusicPlayer.lnk");
                            Console.WriteLine("Media Player: Default MPlayer launched");
                        } catch (Exception e) {
                            Process.Start("mswindowsmusic:");
                            Console.WriteLine("Media Player: mswindowsmusic launched because: " + e.Message);
                            string DebugMsgDefaultLocation = ("Media Player: Default MPlayer location [" + AppBaseDirectory + "OpenDefaultMusicPlayer.lnk" + "]");
                            Console.WriteLine(DebugMsgDefaultLocation);

                            #region External Debugger Log

                            string text = "";

                            if (File.Exists(AppBaseDirectory + "DebugLog.txt"))
                            {
                                text = File.ReadAllText(AppBaseDirectory + "DebugLog.txt");
                            }

                            string Newtext = text + e + "\r\n" + DebugMsgDefaultLocation + "\r\n";
                            File.WriteAllText(AppBaseDirectory + "DebugLog.txt", Newtext);

                            #endregion
                        }
                    }

                    // ========================================
                    // if a media player is already open play
                    // ========================================
                    var mediaPlayPause = (byte)System.Windows.Forms.Keys.MediaPlayPause;
                    keybd_event(mediaPlayPause, mediaPlayPause, 0, 0);
                    keybd_event(mediaPlayPause, mediaPlayPause, KEYEVENTF_KEYUP, 0);
                    Console.WriteLine("Media Player: mediaPlayPause");
                    return;
                }
                if (touchCount == 2)
                {
                    var mediaNextTrack = (byte)System.Windows.Forms.Keys.MediaNextTrack;
                    keybd_event(mediaNextTrack, mediaNextTrack, 0, 0);
                    keybd_event(mediaNextTrack, mediaNextTrack, KEYEVENTF_KEYUP, 0);
                    Console.WriteLine("Media Player: mediaNextTrack");
                    return;
                }
                if (touchCount == 3)
                {
                    var mediaPreviousTrack = (byte)System.Windows.Forms.Keys.MediaPreviousTrack;
                    keybd_event(mediaPreviousTrack, mediaPreviousTrack, 0, 0);
                    keybd_event(mediaPreviousTrack, mediaPreviousTrack, KEYEVENTF_KEYUP, 0);
                    Console.WriteLine("Media Player: mediaPreviousTrack");
                    return;
                }
            }
        }

        public static IEnumerable<Key> KeysDown()
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (Keyboard.IsKeyDown(key))
                    yield return key;
            }
        }



        // ==============================================
        //              CopyRight part
        // ==============================================

        private void CopyRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("This Software was created by OLIVIER Tom (Tom60chat)\n \nClick Yes to see my Youtube Channel\nor\nClick No to make a donation to me :)\n \nClick cancel for return\nYes this buttons method is ridiculous", "," + "OneMediaKey: CopyRight", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
                Process.Start("https://www.youtube.com/channel/UCFmyqR2GRUErhFiuiFFtnKQ");
            if (result == MessageBoxResult.No)
                Process.Start("https://www.paypal.me/Tom60chat");
        }

        private void CopyRight_MouseEnter(object sender, MouseEventArgs e)
        {
            CopyRight.Foreground = Brushes.Blue;
        }

        private void CopyRight_MouseLeave(object sender, MouseEventArgs e)
        {
            CopyRight.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF707070"));
        }

        private void CheckBox_OpenMedia_Ckecked(object sender, RoutedEventArgs e)
        {
            try
            {
              OneMediaKey.Properties.Settings.Default.OpenMPlayerOnNotOp = true;
              OneMediaKey.Properties.Settings.Default.Save();
            }
            catch
            {
                Console.WriteLine("One Media Key.exe.config are not detected\nPlease put it in the app directory or else the save are not available");
            }
        }

        private void CheckBox_OpenMedia_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
              OneMediaKey.Properties.Settings.Default.OpenMPlayerOnNotOp = false;
              OneMediaKey.Properties.Settings.Default.Save();
            }
            catch
            {
                Console.WriteLine("One Media Key.exe.config are not detected\nPlease put it in the app directory or else the save are not available");
            }
        }

        private void CheckBox_AutoStart_Ckecked(object sender, RoutedEventArgs e)
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("OneMediaKey", CurrentProcess.MainModule.FileName);
        }

        private void CheckBox_AutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.DeleteValue("OneMediaKey", false);
        }
    }
}
