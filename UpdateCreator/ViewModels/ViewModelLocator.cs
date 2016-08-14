namespace UpdateCreator.ViewModels
{
    public class ViewModelLocator
    {
        private MainViewModel _mainViewModel;
        public MainViewModel MainViewModel
        {
            get { return this._mainViewModel ?? (this._mainViewModel = new MainViewModel()); }
        }
    }
}
