using Safebox.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public sealed partial class Passwords : Page
    {
     

        public Passwords()
        {
            this.InitializeComponent();
            if (MainPage.loggedIn)
            {
                MainPage.populatePasswords();
            }
            MainPage.passwordsFiltered = new ObservableCollection<PasswordEntity>(MainPage._passwordEntities);            
            list.ItemsSource = MainPage.passwordsFiltered;
            
        }

     

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<PasswordEntity> tmpFiltered;
            tmpFiltered = MainPage._passwordEntities.Where( entity =>  entity.Name.Contains(search.Text, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x=>x.Name).ToList();
            
            for (int i = MainPage.passwordsFiltered.Count -1; i>=0; i--)
            {
                var item = MainPage.passwordsFiltered[i];
                //if (!tmpFiltered.Contains(item))
                //{
                MainPage.passwordsFiltered.Remove(item);
                //}
            }

            foreach (var item in tmpFiltered)
            {
                if (!MainPage.passwordsFiltered.Contains(item))
                {
                    MainPage.passwordsFiltered.Add(item);
                }
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

        private void newPass(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewPassword));            
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            PasswordEntity passwordEntity = (PasswordEntity) e.ClickedItem;
            if (passwordEntity != null)
            {
                PasswordSheet.selectedEntity = passwordEntity;

                this.Frame.Navigate(typeof(PasswordSheet));
            }
        }
    }
}
