using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
#if ANDROID
using Android.Content;
using Android.App.AppSearch;
#endif

namespace GIC.Utilities
{
    public class PhoneCall
    {
        public async Task CallEmergencyNumber(ContentPage page)
        {
            // Check and request permission
            var status = await Permissions.CheckStatusAsync<Permissions.Phone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Phone>();
            }

            if (status == PermissionStatus.Granted)
            {
                bool isConfirmed = await page.DisplayAlert("Nödnummer", "Vill du verkligen ringa nödnummret 112?", "Ja", "Nej");
                if (isConfirmed)
                {
#if ANDROID
            MakeEmergencyCall("112");
#endif
                }
            }
            else
            {
                await page.DisplayAlert("Permission Denied", "Phone call permission is not granted", "OK");
            }
        }

#if ANDROID
private void MakeEmergencyCall(string number)
{
    var intent = new Intent(Intent.ActionCall);
    intent.SetData(Android.Net.Uri.Parse($"tel:{number}"));
    intent.AddFlags(ActivityFlags.NewTask);
    Android.App.Application.Context.StartActivity(intent);
}
#endif


        // Method to simulate calling the non-emergency number
        public async Task CallNonEmergencyNumber(ContentPage page)
        {
            // Check and request permission
            var status = await Permissions.CheckStatusAsync<Permissions.Phone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Phone>();
            }

            if (status == PermissionStatus.Granted)
            {
                // Directly make the call without asking for confirmation
#if ANDROID
        MakeNonEmergencyCall("010-456 6700");
#endif
            }
            else
            {
                await page.DisplayAlert("Permission Denied", "Phone call permission is not granted", "OK");
            }
        }


#if ANDROID
private void MakeNonEmergencyCall(string number)
{
    var intent = new Intent(Intent.ActionCall);
    intent.SetData(Android.Net.Uri.Parse($"tel:{number}"));
    intent.AddFlags(ActivityFlags.NewTask);
    Android.App.Application.Context.StartActivity(intent);
}
#endif

    }
}
