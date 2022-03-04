using Safebox.Entities;
using Safebox.Repository;
using Safebox.Views;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Safebox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public IList<PasswordEntity> _passwordEntities = new List<PasswordEntity>();
        public ObservableCollection<PasswordEntity> passwordsFiltered;
        public static bool loggedIn = false;


        public MainPage()
        {
            this.InitializeComponent();
            
        }

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("passwords", typeof(Passwords)),
            ("serials", typeof(ApplicationSerials)),
        };

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigated += ContentFrame_Navigated;
            NavView.SelectedItem = NavView.MenuItems[0];
            
            Debug.WriteLine("Check did the application configured.");
            string usr = Settings.getValue("USER_CREATED");
            if (usr == null)
            {
                this.Frame.Navigate(typeof(CreateUser));
            } else if (!loggedIn)
            {
                this.Frame.Navigate(typeof(Login));                
            } else
            {
                
                PageTo("passwords");
            }
        }

        public void populatePasswords()
        {
            _passwordEntities = PasswordsRepo.findAll();
        }

        public void PageTo(string tag)
        {
      
            NavView_Navigate(tag, new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "Settings";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                if (item.Tag != null)
                {
                    NavView.SelectedItem = NavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .First(n => n.Tag.Equals(item.Tag));

                    NavView.Header =
                        ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
                }
            }
        }

        public void NavView_Navigate( string navItemTag, Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }
    }
}
