using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Forms;
using Xamarin.Forms;

namespace Urho3d.Rube
{
	public partial class MainPage : ContentPage
	{
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string assetsFolder;
            switch (Device.RuntimePlatform)
            {
                case Device.UWP:
                    assetsFolder = "Assets/Data";
                    break;
                default:
                    assetsFolder = "Data";
                    break;
            }

            await MainUrhoSurface.Show<Samples.UrhoApp>(new Urho.ApplicationOptions(assetsFolder));
        }
    }
}
