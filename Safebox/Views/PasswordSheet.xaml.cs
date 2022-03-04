using Safebox.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Safebox.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PasswordSheet : Page
    {

        public static PasswordEntity selectedEntity;

        public PasswordSheet()
        {
            this.InitializeComponent();

            if (selectedEntity != null)
            {
                Name.Text = selectedEntity.Name;                
                Password.Password = selectedEntity.Password;
                Uri.Text = selectedEntity.Uri;
                Note.Text = selectedEntity.Note;
                PrivateKey.Text = selectedEntity.PrivateKey;
                UserName.Text = selectedEntity.Username;
            }

        }


        /// <summary>
        /// MainPage referencia elérhető az alábbi módon
        /// </summary>
        MainPage MainPage
        {
            get
            {
                return (Window.Current.Content as Frame)?.Content as MainPage;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainPage.PageTo("passwords");
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            string u = AesHandler.memDec(App.user);
            string p = AesHandler.memDec(App.pswd);
            string value = Password.Password;
            if (Password.PasswordRevealMode == PasswordRevealMode.Hidden)
            {
                Password.Password = AesHandler.Decrypt(p, value, u);
                Password.PasswordRevealMode = PasswordRevealMode.Visible;

            } else
            {
                Password.Password = AesHandler.Encrypt(p, value, u);
                Password.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        private void Password_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);

            if (ctrl && e.Key == Windows.System.VirtualKey.C)
            {
                var dataPackage = new DataPackage();
                Password.SelectAll();
                dataPackage.SetText(Password.Password);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Password_Paste(object sender, TextControlPasteEventArgs e)
        {
            e.Handled= true;
        }
    }
}
