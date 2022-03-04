using Safebox.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class CreateUser : Page
    {
        public CreateUser()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string p1 = password.Password;
            string p2 = confirm.Password;
            if (username.Text == null)
            {
                message.Text = "Please add username.";
            } else if (username.Text.Length<4)
            {
                message.Text = "The length of username is minimum four characters.";
            } else if (p1==null || p1.Length==0)
            {
                message.Text = "Please enter your password.";
            } else if (p1 == p2)
            {
                
                message.Text = "";
                string validationMessage =  AesHandler.Encrypt(p1, Settings.VALIDATION_MESSAGE, username.Text);
                string back = AesHandler.Decrypt(p1, validationMessage, username.Text);
                Settings.insertValue("VALIDATION_MESSAGE", validationMessage);
                Settings.insertValue("USER_CREATED", "True");
                this.Frame.Navigate(typeof(MainPage));       

            } else
            {
                message.Text = "Passwords do not match.";
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
    }
}
