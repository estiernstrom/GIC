using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Net.Maui;


namespace GIC.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BarcodeScannerPage : ContentPage
    {
        private TaskCompletionSource<string> _scanResultCompletionSource;
        private bool _isBarcodeDetected;

        public BarcodeScannerPage(TaskCompletionSource<string> scanResultCompletionSource)
        {
            InitializeComponent();
            _scanResultCompletionSource = scanResultCompletionSource;
            _isBarcodeDetected = false;

            barcodeReader.Options = new BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.Ean13,
                AutoRotate = true,
                //Multiple = true
            };
        }

        private void barcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();

            if (first is null)
                return;
            _isBarcodeDetected = true;
            Dispatcher.Dispatch(async () =>
            {
                //await DisplayAlert("Barcode Detected", first.Value, "OK");
                _scanResultCompletionSource.TrySetResult(first.Value);
                await Navigation.PopAsync(); // Navigate back to MainPage
            });
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
      
            await Navigation.PopAsync();
        }

    }
}