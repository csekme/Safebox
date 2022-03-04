using Safebox.Entities;
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
    public sealed partial class NewPassword : Page
    {
        public NewPassword()
        {
            this.InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainPage.NavView_Navigate("passwords", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            PasswordEntity entity = new PasswordEntity();
            entity.Name = Name.Text;
            entity.Password = Password.Password;
            entity.Username = UserName.Text;
            entity.Uri = Uri.Text;
            entity.Note = Note.Text;
            entity.PrivateKey = PrivateKey.Text;
            PasswordsRepo.insert(entity);
          
            MainPage.PageTo("passwords");
        }
    }
}
