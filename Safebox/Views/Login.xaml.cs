using Safebox.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string uname = username.Text; //TODO clean from memory
            string pass = password.Password;
            if (uname != null && pass != null && uname.Length > 3)
            {
                try
                {
                    string validation = Settings.getValue("VALIDATION_MESSAGE");
                    if (AesHandler.Decrypt(pass, validation, uname) == Settings.VALIDATION_MESSAGE)
                    {
                        message.Text = "";
                        
                        
                        App.user = AesHandler.memEnc(uname);
                        App.pswd = AesHandler.memEnc(pass);

                        CryptoCore.cleanString(ref uname);
                        CryptoCore.cleanString(ref pass);

                        MainPage.loggedIn = true;
                        this.Frame.Navigate(typeof(MainPage));
                    }
                    else
                    {
                        message.Text = "Wrong username or password";
                    }
                } catch (Exception ex)
                {
                    message.Text = "Wrong username or password";
                }
            }
            else
            {
                message.Text = "Wrong username or password";
            }

        }
    }
}
