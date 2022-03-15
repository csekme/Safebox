using Safebox.Entities;
using Safebox.Repository;
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
        private int mode = 0;

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
            if (mode == 0)
            {
                Name.IsReadOnly = false;
                Uri.IsReadOnly = false;
                Note.IsReadOnly = false;
                PrivateKey.IsReadOnly = false;
                UserName.IsReadOnly = false;
                mode = 1;
                Edit.Content = "Save";
            } else if (mode == 1)
            {
                Name.IsReadOnly = true;
                Uri.IsReadOnly = true;
                Note.IsReadOnly = true;
                PrivateKey.IsReadOnly = true;
                UserName.IsReadOnly = true;
                mode = 0;
                Edit.Content = "Edit";
                //save here
                selectedEntity.Name = Name.Text;

                string u = AesHandler.memDec(App.user);
                string p = AesHandler.memDec(App.pswd);
                string value = Password.Password;
                if (Password.PasswordRevealMode == PasswordRevealMode.Hidden)
                {
                    value = AesHandler.Decrypt(p, value, u);                    
                }                
               
                selectedEntity.Password = value;
                selectedEntity.Note = Note.Text;
                selectedEntity.PrivateKey = PrivateKey.Text;
                selectedEntity.Uri = Uri.Text;
                selectedEntity.Username = UserName.Text;

                PasswordsRepo.update(selectedEntity);
                
                CryptoCore.cleanString(ref u);
                CryptoCore.cleanString(ref p);
                CryptoCore.cleanString(ref value);
            }

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
            CryptoCore.cleanString(ref u);
            CryptoCore.cleanString(ref p);
            CryptoCore.cleanString(ref value);

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
                if (mode == 0) //on show
                {
                    e.Handled = true;
                }
            }
        }

        private void Password_Paste(object sender, TextControlPasteEventArgs e)
        {
            if (mode == 0) //on show
            {
                e.Handled = true;
            }
        }
    }
}
